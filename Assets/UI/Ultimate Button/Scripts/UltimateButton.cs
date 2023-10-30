/* Written by Kaz Crowe */
/* UltimateButton.cs */
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
#endif

[ExecuteInEditMode]
#if ENABLE_INPUT_SYSTEM
public class UltimateButton : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
#else
public class UltimateButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
#endif
{
	// INTERNAL CALCULATIONS //
	RectTransform baseTrans;
	Rect buttonRect;
	CanvasGroup canvasGroup;
	bool getButtonDown = false;
	bool getButton = false;
	bool getButtonUp = false;
	bool getTapCount = false;
	public Canvas ParentCanvas
	{
		get;
		private set;
	}
	RectTransform canvasRectTrans;
	// Store the relative transform information.
	Vector2 relativeTransformSize = Vector2.zero;
	Vector3 relativeTransformPos = Vector3.zero;
	#if ENABLE_INPUT_SYSTEM
	[InputControl( layout = "Button" )]
	[SerializeField]
	private string _controlPath;
	protected override string controlPathInternal
	{
		get => _controlPath;
		set => _controlPath = value;
	}
	#endif

	// BUTTON POSITIONING //
	public Image buttonBase;
	public enum Positioning
	{
		Disabled,
		ScreenSpace,
		RelativeToTransform
	}
	public Positioning positioning = Positioning.ScreenSpace;
	public float buttonSize = 1.75f;
	public bool useActivationSize = false;
	public float positionHorizontal = 5.0f, positionVertical = 20.0f;
	public float activationRange = 1.0f;
	public enum ScalingAxis
	{
		Width,
		Height
	}
	public ScalingAxis scalingAxis = ScalingAxis.Height;
	public enum Anchor
	{
		Left,
		Right
	}
	public Anchor anchor = Anchor.Right;
	public RectTransform relativeTransform;
	public float relativeSpaceMod = 1.5f;
	public bool orbitTransform = false;
	public float orbitDistance = 1.0f;
	public float centerAngle = 0.0f;

	// INPUT SETTINGS //
	public enum Boundary
	{
		Circular,
		Square
	}
	public Boundary boundary = Boundary.Circular;
	public bool trackInput = false;
	public bool transmitInput = false;
	public GameObject receiver;
	IPointerDownHandler downHandler;
	IDragHandler dragHandler;
	IPointerUpHandler upHandler;
	public enum TapCountOption
	{
		NoCount,
		Accumulate,
		TouchRelease
	}
	public TapCountOption tapCountOption = TapCountOption.NoCount;
	public float tapCountDuration = 0.5f;
	public int targetTapCount = 2;
	float currentTapTime = 0.0f;
	int tapCount = 0;

	// VISUAL OPTIONS //
	public bool inputTransition = false;
	public float transitionUntouchedDuration = 0.1f, transitionTouchedDuration = 0.1f;
	float transitionUntouchedSpeed, transitionTouchedSpeed;
	public bool useTension = false;
	public Color tensionColorNone = Color.white, tensionColorFull = Color.white;
	public Image tensionAccent;
	public bool useFade = false;
	public float fadeUntouched = 1.0f, fadeTouched = 0.5f;
	public bool useScale = false;
	public float scaleTouched = 0.9f;
	public bool useHighlight = false;
	public Image buttonHighlight;
	public Color highlightColor = Color.white;
	public bool useCooldown = false;
	public Image buttonCooldown;
	public bool useCooldownText = false;
	public bool displayDecimalCooldown = false;
	public AnimationCurve cooldownTextScaleCurve = new AnimationCurve( new Keyframe[ 2 ] { new Keyframe( 0.0f, 1.0f ), new Keyframe( 1.0f, 1.0f ) } );
	public Text cooldownText;
	public bool InCooldown
	{
		get;
		private set;
	}
	public bool useIcon = false;
	public Image buttonIcon;

	// SCRIPT REFERENCE //
	public string buttonName;
	static Dictionary<string, UltimateButton> UltimateButtons = new Dictionary<string, UltimateButton>();

	// BUTTON EVENTS //
	public UnityEvent onButtonDown, onButtonUp;
	public UnityEvent tapCountEvent;

	// PUBLIC CALLBACKS //
	public event Action OnGetButtonDown, OnGetButton, OnGetButtonUp;
	public event Action OnGetTapCount;


	void Awake ()
	{
		// If the application is being run, then send this button name and states to the static dictionary for reference.
		if( Application.isPlaying && buttonName != string.Empty )
		{
			// If the dictionary already contains a Ultimate Button with this name, then remove the button.
			if( UltimateButtons.ContainsKey( buttonName ) )
				UltimateButtons.Remove( buttonName );
			
			// Add the button name and this Ultimate Button into the dictionary.
			UltimateButtons.Add( buttonName, GetComponent<UltimateButton>() );
		}
	}

	void Start ()
	{
		// If the application is not running then return.
		if( !Application.isPlaying )
			return;
		
		// Try to get the parent canvas component.
		UpdateParentCanvas();

		// If it is still null, then log a error and return.
		if( ParentCanvas == null )
		{
			Debug.LogError( "Ultimate Button - This component is not within a Canvas object. Disabling this component to avoid any errors." );
			enabled = false;
			return;
		}
		// Else, the parent canvas has been found...
		else
		{
			// Check if the parent canvas has the screen size updater, and if not then add it.
			if( !ParentCanvas.GetComponent<UltimateButtonScreenSizeUpdater>() )
				ParentCanvas.gameObject.AddComponent<UltimateButtonScreenSizeUpdater>();
		}

		// Store the base transform.
		baseTrans = GetComponent<RectTransform>();

		// If the user wants the button positioned on the screen, then update the size and positioning of the button.
		if( positioning == Positioning.ScreenSpace )
			UpdatePositioning();

		// If the user wants to transmit input to another UI object, and that object is assigned...
		if( transmitInput && receiver != null )
		{
			// Store the input of the receiver.
			downHandler = receiver.GetComponent<IPointerDownHandler>();
			dragHandler = receiver.GetComponent<IDragHandler>();
			upHandler = receiver.GetComponent<IPointerUpHandler>();
		}

		// If the user wants to transition on different input...
		if( inputTransition )
		{
			// Try to store the canvas group.
			canvasGroup = GetComponent<CanvasGroup>();

			// If the canvas group is still null, then add a canvas group component.
			if( canvasGroup == null )
				canvasGroup = gameObject.AddComponent<CanvasGroup>();

			// Configure the transition speeds.
			transitionUntouchedSpeed = 1.0f / transitionUntouchedDuration;
			transitionTouchedSpeed = 1.0f / transitionTouchedDuration;
		}

		// If the user is wanting to show the highlight color of the button, update the highlight image.
		if( useHighlight && buttonHighlight != null )
			buttonHighlight.color = highlightColor;

		// If the user is wanting to display cooldown on this button...
		if( useCooldown )
		{
			// If the cooldown image is assigned, then set the fill amount to zero.
			if( buttonCooldown != null )
				buttonCooldown.fillAmount = 0.0f;

			// If the user wants text and the text is assigned, then reset the text string.
			if( useCooldownText && cooldownText != null )
				cooldownText.text = "";
		}
	}
	
	public void OnPointerDown ( PointerEventData touchInfo )
	{
		// If the user wants to transmit the input and the event is assigned, then call the function.
		if( transmitInput && downHandler != null )
			downHandler.OnPointerDown( touchInfo );

		// If the button is already in use, then return.
		if( getButton )
			return;

		// If the input is not in range, then return.
		if( !IsInRange( touchInfo.position ) )
			return;

		// If the user wants to display a cooldown, and this button is currently in cooldown, then return.
		if( useCooldown && InCooldown )
			return;
		
		// Set the down state to true since the button was pressed this frame.
		getButtonDown = true;

		// Set the buttons state to true since it is being interacted with.
		getButton = true;

		// If the down event is assigned, then call the event.
		if( onButtonDown != null )
			onButtonDown.Invoke();

		// If the OnGetButtonDown action has any subscribers, then notify them.
		if( OnGetButtonDown != null )
			OnGetButtonDown.Invoke();

		// If the user is wanting to count taps on this button...
		if( tapCountOption != TapCountOption.NoCount )
		{
			// If the user is wanting to accumulate taps...
			if( tapCountOption == TapCountOption.Accumulate )
			{
				// If the timer is not currently counting down...
				if( currentTapTime <= 0 )
				{
					// Then start the count down timer, and set the current tapCount to 1.
					StartCoroutine( "TapCountdown" );
					tapCount = 1;
				}
				// Else the timer is already running, so increase tapCount by 1.
				else
					++tapCount;

				// If the timer is still going, and the target tap count has been reached...
				if( currentTapTime > 0 && tapCount >= targetTapCount )
				{
					// Stop the timer by setting the tap time to zero, start the one frame delay for the static reference of tap count, and call the tapCountEvent.
					currentTapTime = 0;

					// Set the tap count to true since the tap count was achieved.
					getTapCount = true;

					// If there are any subscribers to the tap count event, notify them.
					if( tapCountEvent != null )
						tapCountEvent.Invoke();

					if( OnGetTapCount != null )
						OnGetTapCount.Invoke();
				}
			}
			// Else the user is wanting to send tap counts by way of a quick touch and release...
			else
			{
				// If the timer is not currently counting down, then start the coroutine.
				if( currentTapTime <= 0 )
					StartCoroutine( "TapCountdown" );
				// Else reset the timer.
				else
					currentTapTime = tapCountDuration;
			}
		}

		// If the user wants to show the input transitions...
		if( inputTransition )
		{
			// If either of the transition durations are set to something other than 0, then start the coroutine to transition over time.
			if( transitionUntouchedDuration > 0 || transitionTouchedDuration > 0 )
				StartCoroutine( "InputTransition" );
			// Else the user does not want to transition over time.
			else
			{
				// If the user wants tension to be displayed, then set the color to full tension.
				if( useTension && tensionAccent != null )
					tensionAccent.color = tensionColorFull;

				// If the user wants to fade the button alpha, then set the alpha value.
				if( useFade && canvasGroup != null )
					canvasGroup.alpha = fadeTouched;

				// If the user wants to scale the button, then set the scale.
				if( useScale )
					buttonBase.rectTransform.localScale = Vector3.one * scaleTouched;
			}
		}

		#if ENABLE_INPUT_SYSTEM
		SendValueToControl( 1.0f );
		#endif
	}

	public void OnDrag ( PointerEventData touchInfo )
	{
		// If the user is transmitting input, and the Drag event is assigned, then call the function.
		if( transmitInput && dragHandler != null )
			dragHandler.OnDrag( touchInfo );

		// If the pointer event that is calling this function is not the same as the one that initiated the button, then return.
		if( !getButton )
			return;

		// If the user wants to track the input, and the input is not within range of the button, then release the input.
		if( trackInput && !IsInRange( touchInfo.position ) )
			OnPointerUp( touchInfo );
	}

	public void OnPointerUp ( PointerEventData touchInfo )
	{
		// If the user wants to transmit the input and the OnPointerUp variable is assigned, then call the function.
		if( transmitInput && upHandler != null )
			upHandler.OnPointerUp( touchInfo );

		// If the pointer event that is calling this function is not the same as the one that initiated the button, then return.
		if( !getButton )
			return;

		// Set the buttons state to false.
		getButton = false;

		// Set getButtonUp to true since the input have been released.
		getButtonUp = true;
		
		// If the up event is assigned, then call the event.
		if( onButtonUp != null )
			onButtonUp.Invoke();

		// If the OnGetButtonUp action has any subscribers, then notify them.
		if( OnGetButtonUp != null )
			OnGetButtonUp.Invoke();

		// If the user is wanting to count the amount of taps by Touch and Release...
		if( tapCountOption == TapCountOption.TouchRelease )
		{
			// If the current tap time is greater than zero, then the player achieved the tap count, so...
			if( currentTapTime > 0 )
			{
				// Set getTapCount to true for reference.
				getTapCount = true;

				// Invoke the tap count event if there is one assigned.
				if( tapCountEvent != null )
					tapCountEvent.Invoke();

				// Notify any subscribers that the tap count has been achieved.
				if( OnGetTapCount != null )
					OnGetTapCount.Invoke();
			}

			// Set the tap time to 0 to reset the timer.
			currentTapTime = 0;
		}

		// If the user wants an input transition, but the durations of both touched and untouched states are zero...
		if( inputTransition && ( transitionTouchedDuration <= 0 && transitionUntouchedDuration <= 0 ) )
		{
			// If the users wants tension to be displayed, then reset the color.
			if( useTension && tensionAccent != null )
				tensionAccent.color = tensionColorNone;

			// Then just apply the alpha.
			if( useFade && canvasGroup != null )
				canvasGroup.alpha = fadeUntouched;

			// And reset the scale back to one.
			if( useScale )
				buttonBase.rectTransform.localScale = Vector3.one;
		}

		#if ENABLE_INPUT_SYSTEM
		SendValueToControl( 0.0f );
		#endif
	}

	/// <summary>
	/// Returns the current state of the input being in range of the button or not.
	/// </summary>
	/// <param name="inputPosition">The current position of the input for calculations.</param>
	bool IsInRange ( Vector2 inputPosition )
	{
		// If the user has a circular button image...
		if( boundary == Boundary.Circular )
		{
			// distance = distance between the world position of the joystickBase cast to a local position of the ParentCanvas (* by scale factor) - half of the actual canvas size, and the input position.
			float distance = Vector2.Distance( ( Vector2 )( ParentCanvas.transform.InverseTransformPoint( buttonBase.rectTransform.position ) * ParentCanvas.scaleFactor ) + ( ( canvasRectTrans.sizeDelta * ParentCanvas.scaleFactor ) / 2 ), inputPosition );

			// If the distance is out of range, then just return.
			if( distance / ( baseTrans.sizeDelta.x * ParentCanvas.scaleFactor ) > 0.5f )
				return false;
		}
		// Else the user has a square button...
		else
		{
			// If the rect of the button does not contain the input position, then return false.
			if( !buttonRect.Contains( inputPosition ) )
				return false;
		}

		return true;
	}

	/// <summary>
	/// This function is used for counting down for the TapCount options.
	/// </summary>
	IEnumerator TapCountdown ()
	{
		// Set currentTapTime to equal the max time.
		currentTapTime = tapCountDuration;

		// While the current tap time is greater than zero, decrease the time window by time.
		while( currentTapTime > 0 )
		{
			currentTapTime -= Time.deltaTime;
			yield return null;
		}
	}
	
	/// <summary>
	/// This coroutine will handle the input transitions over time according to the users options.
	/// </summary>
	IEnumerator InputTransition ()
	{
		// Store the current values for the tension, alpha and scale of the button.
		Color currentTension = tensionAccent == null ? Color.white : tensionAccent.color;
		float currentAlpha = canvasGroup == null ? 1.0f : canvasGroup.alpha;
		float currentScale = buttonBase.rectTransform.localScale.x;

		// If the scaleInSpeed is NaN....
		if( float.IsInfinity( transitionTouchedSpeed ) )
		{
			// Set the color of the tension image to the full color.
			if( useTension && tensionAccent != null )
				tensionAccent.color = tensionColorFull;

			// Set the alpha to the touched value.
			if( useFade && canvasGroup != null )
				canvasGroup.alpha = fadeTouched;

			// Set the scale to the touched value.
			if( useScale )
				buttonBase.rectTransform.localScale = Vector3.one * scaleTouched;
		}
		// Else run the loop to transition to the desired values over time.
		else
		{
			// This for loop will continue for the transition duration.
			for( float transition = 0.0f; transition < 1.0f && getButton; transition += Time.deltaTime * transitionTouchedSpeed )
			{
				// Lerp the color between the current color to the full color by the fadeIn value above.
				if( useTension && tensionAccent != null )
					tensionAccent.color = Color.Lerp( currentTension, tensionColorFull, transition );

				// Lerp the alpha of the canvas group.
				if( useFade && canvasGroup != null )
					canvasGroup.alpha = Mathf.Lerp( currentAlpha, fadeTouched, transition );

				// Lerp the scale of the button.
				if( useScale )
					buttonBase.rectTransform.localScale = Vector3.one * Mathf.Lerp( currentScale, scaleTouched, transition );

				yield return null;
			}

			// If the button is still being interacted with, then finalize the values since the loop above has ended.
			if( getButton )
			{
				if( useTension && tensionAccent != null )
					tensionAccent.color = tensionColorFull;

				if( useFade && canvasGroup != null )
					canvasGroup.alpha = fadeTouched;

				if( useScale )
					buttonBase.rectTransform.localScale = Vector3.one * scaleTouched;
			}
		}

		// While loop for while joystickState is true
		while( getButton )
			yield return null;

		// Set the current values.
		currentAlpha = canvasGroup == null ? 1.0f : canvasGroup.alpha;
		currentScale = buttonBase.rectTransform.localScale.x;
		currentTension = tensionAccent == null ? Color.white : tensionAccent.color;

		// If the scaleOutSpeed value is NaN, then apply the desired values.
		if( float.IsInfinity( transitionUntouchedSpeed ) )
		{
			if( useTension && tensionAccent != null )
				tensionAccent.color = tensionColorNone;

			if( useFade && canvasGroup != null )
				canvasGroup.alpha = fadeUntouched;

			if( useScale )
				buttonBase.rectTransform.localScale = Vector3.one;
		}
		// Else run the loop to transition to the desired values over time.
		else
		{
			for( float transition = 0.0f; transition < 1.0f && !getButton; transition += Time.deltaTime * transitionUntouchedSpeed )
			{
				// Lerp the color between the current color to the full color by the fadeIn value above.
				if( useTension && tensionAccent != null )
					tensionAccent.color = Color.Lerp( currentTension, tensionColorNone, transition );

				// Lerp the fade value.
				if( useFade && canvasGroup != null )
					canvasGroup.alpha = Mathf.Lerp( currentAlpha, fadeUntouched, transition );

				// Lerp the scale.
				if( useScale )
					buttonBase.rectTransform.localScale = Vector3.one * Mathf.Lerp( currentScale, 1.0f, transition );
				
				yield return null;
			}

			// If the button is still not being interacted with, then finalize the alpha and scale since the loop above finished.
			if( !getButton )
			{
				if( useTension && tensionAccent != null )
					tensionAccent.color = tensionColorNone;

				if( useFade && canvasGroup != null )
					canvasGroup.alpha = fadeUntouched;

				if( useScale )
					buttonBase.rectTransform.localScale = Vector3.one;
			}
		}
	}

	/// <summary>
	/// This function is called by Unity when the parent of this transform changes.
	/// </summary>
	void OnTransformParentChanged ()
	{
		UpdateParentCanvas();
	}

	/// <summary>
	/// Updates the parent canvas if it has changed.
	/// </summary>
	public void UpdateParentCanvas ()
	{
		// Store the parent of this object.
		Transform parent = transform.parent;

		// If the parent is null, then just return.
		if( parent == null )
			return;

		// While the parent is assigned...
		while( parent != null )
		{
			// If the parent object has a Canvas component, then assign the ParentCanvas and transform.
			if( parent.transform.GetComponent<Canvas>() )
			{
				ParentCanvas = parent.transform.GetComponent<Canvas>();
				canvasRectTrans = ParentCanvas.GetComponent<RectTransform>();
				return;
			}

			// If the parent does not have a canvas, then store it's parent to loop again.
			parent = parent.transform.parent;
		}
	}

	/// <summary>
	/// Resets the button input information and stops any coroutines that might have been running.
	/// </summary>
	void ResetButton ()
	{
		// Set the buttons state to false.
		getButton = false;
		getButtonDown = false;
		getButtonUp = false;
		
		// If the user wants to transition on input...
		if( inputTransition )
		{
			// Stop the input transition coroutine.
			StopCoroutine( "InputTransition" );

			// If the user has tension enabled and the image is assigned, then reset it.
			if( useTension && tensionAccent != null )
				tensionAccent.color = tensionColorNone;

			// If the user is using alpha fade, then reset the alpha.
			if( useFade && canvasGroup != null )
				canvasGroup.alpha = fadeUntouched;

			// If the user has scale selected, reset it.
			if( useScale )
				buttonBase.rectTransform.localScale = Vector3.one;
		}
	}

	#if UNITY_EDITOR
	void Update ()
	{
		// The button will be updated constantly when the game is not being run.
		if( !Application.isPlaying )
			UpdatePositioning();
	}
	#endif

	void LateUpdate ()
	{
		// If there are any subscribers to the OnGetButton callback, then notify them.
		if( getButton && OnGetButton != null )
			OnGetButton.Invoke();

		// Reset all the input variables since this is the end of the frame.
		getButtonDown = false;
		getButtonUp = false;
		getTapCount = false;

		if( positioning == Positioning.RelativeToTransform && relativeTransform != null )
		{
			// If the transform has changed at all...
			if( relativeTransformSize != relativeTransform.sizeDelta || relativeTransformPos != relativeTransform.position )
			{
				// Store the new transform information.
				relativeTransformSize = relativeTransform.sizeDelta;
				relativeTransformPos = relativeTransform.position;

				// Update the positioning.
				UpdatePositioning();
			}
		}
	}

	// --------------------------------------------- *** PUBLIC FUNCTIONS FOR THE USER *** --------------------------------------------- //
	/// <summary>
	/// Updates the size and placement of the Ultimate Button. Useful for when applying any options changed at runtime.
	/// </summary>
	public void UpdatePositioning ()
	{
		// If the game is running, then reset the button.
		if( Application.isPlaying )
			ResetButton();
		
		// If the positioning option is disabled, then return.
		if( positioning == Positioning.Disabled )
			return;

		// If the parent canvas is null, then try to get the parent canvas component.
		if( ParentCanvas == null )
			UpdateParentCanvas();

		// If it is still null, then log a error and return.
		if( ParentCanvas == null )
		{
			Debug.LogError( "Ultimate Button - There is no parent canvas object. Please make sure that the Ultimate Button is placed within a canvas." );
			return;
		}

		// If the buttonBase is left unassigned, then inform the user and return.
		if( buttonBase == null )
		{
			if( Application.isPlaying )
				Debug.LogError( "Ultimate Button - The buttonBase variable has not been assigned. Please make sure that this variable is assigned in the Button Positioning section." );

			return;
		}

		// Set the current reference size for scaling.
		float referenceSize = scalingAxis == ScalingAxis.Height ? canvasRectTrans.sizeDelta.y : canvasRectTrans.sizeDelta.x;

		// Configure a size for the image based on the Canvas's size and scale.
		float textureSize = referenceSize * ( buttonSize / 10 );

		// Store the texture size for calculations, so that this variable can be modified depending on the users options.
		float textureSizeForCalc = textureSize;

		// If the user wants to use the activation size for positioning the button, then calculate the texture size.
		if( useActivationSize )
			textureSizeForCalc = textureSize * activationRange;

		// If baseTrans is null, store this object's RectTrans so that it can be positioned.
		if( baseTrans == null )
			baseTrans = GetComponent<RectTransform>();

		// Force the anchors and pivot so the button will function correctly. This is also needed here for older versions of the Ultimate Button that didn't use these rect transform settings.
		baseTrans.anchorMin = Vector2.zero;
		baseTrans.anchorMax = Vector2.zero;
		baseTrans.pivot = new Vector2( 0.5f, 0.5f );
		baseTrans.localScale = Vector3.one;
		baseTrans.localRotation = Quaternion.identity;

		// Set the anchors of the button base. It is important to have the anchors centered for calculations.
		buttonBase.rectTransform.anchorMin = new Vector2( 0.5f, 0.5f );
		buttonBase.rectTransform.anchorMax = new Vector2( 0.5f, 0.5f );
		buttonBase.rectTransform.pivot = new Vector2( 0.5f, 0.5f );
		buttonBase.rectTransform.localRotation = Quaternion.identity;

		// Configure the position that the user wants the button to be located.
		Vector2 buttonPosition = new Vector2( canvasRectTrans.sizeDelta.x * ( positionHorizontal / 100 ) - ( textureSizeForCalc * ( positionHorizontal / 100 ) ) + ( textureSizeForCalc / 2 ), canvasRectTrans.sizeDelta.y * ( positionVertical / 100 ) - ( textureSizeForCalc * ( positionVertical / 100 ) ) + ( textureSizeForCalc / 2 ) ) - ( canvasRectTrans.sizeDelta / 2 );

		// If the anchor is to the right, then make the horizontal button position negative.
		if( anchor == Anchor.Right )
			buttonPosition.x = -buttonPosition.x;

		// If the user wants the positioning to be relative to another transform...
		if( positioning == Positioning.RelativeToTransform )
		{
			// If the relative transform has been assigned...
			if( relativeTransform != null )
			{
				// Calculate the center position of the relative transform.
				Vector3 relativeTransformPosition = ParentCanvas.transform.InverseTransformPoint( relativeTransform.position ) - ( Vector3 )( relativeTransform.sizeDelta * ( relativeTransform.pivot - new Vector2( 0.5f, 0.5f ) ) );

				if( orbitTransform )
				{
					// Reconfigure the texture size and reference size based on the relative transform.
					textureSize = relativeTransform.sizeDelta.y * ( buttonSize / 5 );
					
					// Configure a new position for the button.
					buttonPosition = relativeTransformPosition;

					// Calculate the position of the button according to the user options.
					buttonPosition.x += ( Mathf.Cos( ( -centerAngle * Mathf.Deg2Rad ) + ( 90 * Mathf.Deg2Rad ) ) * ( relativeTransform.sizeDelta.x * orbitDistance ) );
					buttonPosition.y += ( Mathf.Sin( ( -centerAngle * Mathf.Deg2Rad ) + ( 90 * Mathf.Deg2Rad ) ) * ( relativeTransform.sizeDelta.x * orbitDistance ) );
				}
				else
				{
					// Reconfigure the texture size and reference size based on the relative transform.
					textureSize = relativeTransform.sizeDelta.y * ( buttonSize / 5 );

					// Store the texture size for calculations, so that this variable can be modified depending on the users options.
					textureSizeForCalc = textureSize;

					// If the user wants to use the activation size for positioning the button, then calculate the texture size.
					if( useActivationSize )
						textureSizeForCalc = textureSize * activationRange;

					// Fix the position data to be between -0.5 and 0.5 for easy calculations.
					Vector2 positionData = new Vector2( positionHorizontal - 50, positionVertical - 50 ) / 100;

					// Configure the new button position according to the relative transform.
					buttonPosition = relativeTransformPosition + ( Vector3 )( ( relativeTransform.sizeDelta * relativeSpaceMod ) * positionData ) - ( Vector3 )( ( Vector2.one * textureSizeForCalc ) * positionData );
				}

				// Store the relative transform information.
				relativeTransformSize = relativeTransform.sizeDelta;
				relativeTransformPos = relativeTransform.position;

				// EDIT: I should do a check right here to see if the relative transform is unassigned, then inform the user and set the positioning variable to Disabled to avoid extra checks.
			}
		}

		// Apply the button size multiplied by the activation range.
		baseTrans.sizeDelta = new Vector2( textureSize, textureSize ) * activationRange;

		// Apply the calculated position.
		baseTrans.localPosition = buttonPosition;

		// Apply the size and position to the buttonBase.
		buttonBase.rectTransform.sizeDelta = new Vector2( textureSize, textureSize );
		buttonBase.rectTransform.localPosition = Vector3.zero;
		
		// Configure the actual size delta and position of the base trans regardless of the canvas scaler setting.
		Vector2 baseSizeDelta = baseTrans.sizeDelta * ParentCanvas.scaleFactor;
		Vector2 baseLocalPosition = baseTrans.localPosition * ParentCanvas.scaleFactor;

		// Calculate the rect of the base trans.
		buttonRect = new Rect( new Vector2( baseLocalPosition.x - ( baseSizeDelta.x / 2 ), baseLocalPosition.y - ( baseSizeDelta.y / 2 ) ) + ( ( canvasRectTrans.sizeDelta * ParentCanvas.scaleFactor ) / 2 ), baseSizeDelta );
		
		// Ensure that all additional images are stretched correctly.
		if( inputTransition && useTension && tensionAccent != null && tensionAccent.rectTransform.anchoredPosition != Vector2.zero )
		{
			tensionAccent.rectTransform.offsetMax = Vector2.zero;
			tensionAccent.rectTransform.offsetMin = Vector2.zero;
		}

		if( useHighlight && buttonHighlight != null && buttonHighlight.rectTransform.anchoredPosition != Vector2.zero )
		{
			buttonHighlight.rectTransform.offsetMax = Vector2.zero;
			buttonHighlight.rectTransform.offsetMin = Vector2.zero;
		}

		if( useCooldown && buttonCooldown != null && buttonCooldown.rectTransform.anchoredPosition != Vector2.zero )
		{
			buttonCooldown.rectTransform.offsetMax = Vector2.zero;
			buttonCooldown.rectTransform.offsetMin = Vector2.zero;
		}

		if( useIcon && buttonIcon != null && buttonIcon.rectTransform.anchoredPosition != Vector2.zero )
		{
			buttonIcon.rectTransform.offsetMax = Vector2.zero;
			buttonIcon.rectTransform.offsetMin = Vector2.zero;
		}
	}

	/// <summary>
	/// Returns true on the frame that the Ultimate Button is pressed down.
	/// </summary>
	public bool GetButtonDown ()
	{
		return getButtonDown;
	}

	/// <summary>
	/// Returns true on the frames that the Ultimate Button is being interacted with.
	/// </summary>
	public bool GetButton ()
	{
		return getButton;
	}

	/// <summary>
	/// Returns true on the frame that the Ultimate Button is released.
	/// </summary>
	public bool GetButtonUp ()
	{
		return getButtonUp;
	}

	/// <summary>
	/// Returns true when the Tap Count option has been achieved.
	/// </summary>
	public bool GetTapCount ()
	{
		return getTapCount;
	}

	/// <summary>
	/// Disables the Ultimate Button.
	/// </summary>
	public void DisableButton ()
	{
		// If this game object is already disabled, then return.
		if( !gameObject.activeInHierarchy )
			return;

		// Set the buttons state to false.
		getButton = false;
		getButtonDown = false;
		getButtonUp = false;
		
		// If the user wants to show a transition on the different input states...
		if( inputTransition )
		{
			// Stop the input transition coroutine.
			StopCoroutine( "InputTransition" );

			// If the user wants to display tension, then reset the color.
			if( useTension && tensionAccent != null )
				tensionAccent.color = tensionColorNone;

			// If the user is displaying a fade, then reset to the untouched state.
			if( useFade && canvasGroup != null )
				canvasGroup.alpha = fadeUntouched;

			// If the user is scaling the button, then reset the scale.
			if( useScale )
				buttonBase.rectTransform.transform.localScale = Vector3.one;
		}

		// If the user wants to display a cooldown, then reset the cooldown.
		if( useCooldown )
			ResetCooldown();
		
		// Disable the gameObject.
		gameObject.SetActive( false );
	}

	/// <summary>
	/// Enables the Ultimate Button.
	/// </summary>
	public void EnableButton ()
	{
		// If the game object is already active, then return.
		if( gameObject.activeInHierarchy )
			return;

		// Enable the gameObject.
		gameObject.SetActive( true );
	}
	
	/// <summary>
	/// Assigns a new sprite to the button's icon image.
	/// </summary>
	/// <param name="newIcon">The new sprite to assign as the icon for the button.</param>
	public void UpdateIcon ( Sprite newIcon )
	{
		// If the button icon is unassigned, then notify the user and return.
		if( buttonIcon == null )
		{
			Debug.LogError( "Ultimate Button - The Button Icon image is not assigned. Please make sure to assign this image variable before trying to modify the button icon sprite." );
			return;
		}

		// Apply the new icon to the button icon.
		buttonIcon.sprite = newIcon;
	}

	/// <summary>
	/// Updates the needed variables to display the current cooldown time of the button.
	/// </summary>
	/// <param name="currentTime">The current time of the cooldown.</param>
	/// <param name="maxTime">The max time of the cooldown.</param>
	public void UpdateCooldown ( float currentTime, float maxTime )
	{
		// If the button is currently interactable, then set InCooldown to false so the player cannot use this button while it is in cooldown.
		if( !InCooldown )
			InCooldown = true;

		// Configure the overall percentage of the cooldown with the values provided.
		float overallCooldownPercentage = currentTime / maxTime;

		// If the cooldown image is assigned, then set the fill amount.
		if( buttonCooldown != null )
			buttonCooldown.fillAmount = overallCooldownPercentage;

		// If the cooldown text is assigned...
		if( cooldownText != null )
		{
			// Round the current time value to the nearest int.
			int i = Mathf.RoundToInt( currentTime );

			// If the i value is greater than the current time provided by the user, then reduce i by one. This is because rounding will cause the time to be incorrect.
			if( i > currentTime )
				i -= 1;

			// Configure the string to display on the text by formatting it to a whole number.
			string textToDisplay = ( i + 1 ).ToString( "F0" );

			// If the user wants to show the decimal point however, then format the text for that.
			if( displayDecimalCooldown )
				textToDisplay = currentTime.ToString( "F1" );

			// Apply the configured string.
			cooldownText.text = textToDisplay;

			// Configure a time value that is between 0 and 1 so that the text can be scaled by the animation curve.
			float t = currentTime - i;

			// Evaluate the animation curve.
			cooldownText.rectTransform.localScale = Vector3.Lerp( Vector3.zero, Vector3.one, cooldownTextScaleCurve.Evaluate( -t + 1 ) );
		}

		// If the overall percentage of the cooldown is zero or less, then reset the cooldown since it is done.
		if( overallCooldownPercentage <= 0.0f )
			ResetCooldown();
	}

	/// <summary>
	/// Resets the cooldown variables.
	/// </summary>
	public void ResetCooldown ()
	{
		// Reset the boolean controller so the player can use this button again.
		InCooldown = false;

		// If the cooldown image is assigned, then reset the fill amount.
		if( buttonCooldown != null )
			buttonCooldown.fillAmount = 0.0f;

		// If the cooldown text is assigned, then reset it.
		if( cooldownText != null )
			cooldownText.text = "";
	}
	// ------------------------------------------- *** END PUBLIC FUNCTIONS FOR THE USER *** ------------------------------------------- //

	// --------------------------------------------- *** STATIC FUNCTIONS FOR THE USER *** --------------------------------------------- //
	/// <summary>
	/// Returns the targeted Ultimate Button if it exists within the scene.
	/// </summary>
	/// <param name="buttonName">The name of the targeted Ultimate Button.</param>
	public static UltimateButton GetUltimateButton ( string buttonName )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return null;

		return UltimateButtons[ buttonName ];
	}

	/// <summary>
	/// Returns true on the frame that the targeted Ultimate Button is pressed down.
	/// </summary>
	/// <param name="buttonName">The name of the targeted Ultimate Button.</param>
	public static bool GetButtonDown ( string buttonName )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return false;
		
		return UltimateButtons[ buttonName ].getButtonDown;
	}

	/// <summary>
	/// Returns true on the frames that the targeted Ultimate Button is being interacted with.
	/// </summary>
	/// <param name="buttonName">The name of the targeted Ultimate Button.</param>
	public static bool GetButton ( string buttonName )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return false;

		return UltimateButtons[ buttonName ].getButton;
	}

	/// <summary>
	/// Returns true on the frame that the targeted Ultimate Button is released.
	/// </summary>
	/// <param name="buttonName">The name of the targeted Ultimate Button.</param>
	/// <returns></returns>
	public static bool GetButtonUp ( string buttonName )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return false;

		return UltimateButtons[ buttonName ].getButtonUp;
	}

	/// <summary>
	/// Returns true when the Tap Count option has been achieved.
	/// </summary>
	/// <param name="buttonName">The name of the targeted Ultimate Button.</param>
	public static bool GetTapCount ( string buttonName )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return false;

		return UltimateButtons[ buttonName ].getTapCount;
	}

	/// <summary>
	/// Disables the targeted Ultimate Button.
	/// </summary>
	/// <param name="buttonName">The name of the desired Ultimate Button.</param>
	public static void DisableButton ( string buttonName )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return;

		UltimateButtons[ buttonName ].DisableButton();
	}

	/// <summary>
	/// Enables the targeted Ultimate Button.
	/// </summary>
	/// <param name="buttonName">The name of the desired Ultimate Button.</param>
	public static void EnableButton ( string buttonName )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return;

		UltimateButtons[ buttonName ].EnableButton();
	}

	/// <summary>
	/// Assigns a new sprite icon to the targeted Ultimate Button.
	/// </summary>
	/// <param name="buttonName">The name of the desired Ultimate Button.</param>
	/// <param name="newIcon">The new sprite icon to apply to the targeted Ultimate Button.</param>
	public static void UpdateIcon ( string buttonName, Sprite newIcon )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return;

		UltimateButtons[ buttonName ].UpdateIcon( newIcon );
	}

	/// <summary>
	/// Updates the cooldown of the targeted Ultimate Button.
	/// </summary>
	/// <param name="buttonName">The name of the desired Ultimate Button.</param>
	/// <param name="currentTime">The current cooldown time.</param>
	/// <param name="maxTime">The max cooldown time.</param>
	public static void UpdateCooldown ( string buttonName, float currentTime, float maxTime )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return;

		UltimateButtons[ buttonName ].UpdateCooldown( currentTime, maxTime );
	}

	/// <summary>
	/// Resets the cooldown of the targeted Ultimate Button.
	/// </summary>
	/// <param name="buttonName">The name of the desired Ultimate Button.</param>
	public static void ResetCooldown ( string buttonName )
	{
		// If this button name is not connected with an Ultimate Button component, then return.
		if( !ButtonConfirmed( buttonName ) )
			return;

		UltimateButtons[ buttonName ].EnableButton();
	}

	static bool ButtonConfirmed ( string buttonName )
	{
		// If the dictionary does not have a button registered with the provided name, then notify the user and return.
		if( !UltimateButtons.ContainsKey( buttonName ) )
		{
			Debug.LogWarning( "Ultimate Button - No Ultimate Button has been registered with the name: " + buttonName + "." );
			return false;
		}
		return true;
	}
	// ------------------------------------------- *** END STATIC FUNCTIONS FOR THE USER *** ------------------------------------------- //
}