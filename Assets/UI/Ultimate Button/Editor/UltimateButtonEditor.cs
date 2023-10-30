/* Written by Kaz Crowe */
/* UltimateButtonEditor.cs */
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor( typeof( UltimateButton ) )]
public class UltimateButtonEditor : Editor
{
	// -----< INTERNAL >----- //
	UltimateButton targ;
	bool isPrefabInProjectWindow = false;
	Canvas parentCanvas;
	const int afterIndentSpace = 5;

	// -----< BUTTON POSITIONING >----- //
	SerializedProperty buttonBase, positioning;
	Sprite buttonBaseSprite;
	SerializedProperty relativeTransform, relativeSpaceMod;
	SerializedProperty scalingAxis, anchor;
	SerializedProperty buttonSize, activationRange;
	SerializedProperty positionHorizontal, positionVertical;
	SerializedProperty orbitTransform, orbitDistance, centerAngle;

	// -----< INPUT SETTINGS >----- //
	SerializedProperty boundary;
	SerializedProperty trackInput, transmitInput, receiver;
	SerializedProperty tapCountOption, tapCountDuration, targetTapCount;
	SerializedProperty useTouchInput;

	// -----< BUTTON OPTIONS >----- //
	Color baseColor;
	SerializedProperty inputTransition, useFade, useScale;
	SerializedProperty useTension, tensionColorNone, tensionColorFull;
	SerializedProperty tensionAccent;
	Sprite tensionAccentSprite;
	SerializedProperty transitionUntouchedDuration, transitionTouchedDuration;
	SerializedProperty fadeUntouched, fadeTouched, scaleTouched;
	SerializedProperty useHighlight, highlightColor;
	SerializedProperty buttonHighlight;
	Sprite buttonHighlightSprite;
	SerializedProperty useIcon, buttonIcon;
	float iconScale = 1.0f;
	bool useIconMask = false;
	Image buttonIconMask;
	Sprite iconSprite, iconMaskSprite;
	Color iconColor = Color.white;
	float cooldownTestValue = 1.0f;
	float cooldownTimeMax = 1;
	SerializedProperty useCooldown, buttonCooldown, useCooldownText;
	SerializedProperty displayDecimalCooldown, cooldownTextScaleCurve, cooldownText;
	Sprite cooldownSprite;
	Image.FillMethod fillMethod = Image.FillMethod.Radial360;
	Color cooldownColor = new Color( 0.0f, 0.0f, 0.0f, 0.5f ), textColor = Color.white, textOutlineColor = Color.black;
	float textAnchorMod = 0.5f;
	Font textFont;
	bool textOutline = false;
	// Reorder Child Hierarchy //
	List<RectTransform> childTransforms = new List<RectTransform>();
	ReorderableList childObjects;

	// -----< SCRIPT REFERENCE >----- //
	SerializedProperty buttonName;
	class ExampleCode
	{
		public string optionName = "";
		public string optionDescription = "";
		public string basicCode = "";
	}
	ExampleCode[] exampleCodes = new ExampleCode[]
	{
		new ExampleCode() { optionName = "GetButtonDown ( string buttonName )", optionDescription = "Returns true on the frame that the button was pressed down.", basicCode = "UltimateButton.GetButtonDown( \"{0}\" )" },
		new ExampleCode() { optionName = "GetButtonUp ( string buttonName )", optionDescription = "Returns true on the frame that the button was released.", basicCode = "UltimateButton.GetButtonUp( \"{0}\" )" },
		new ExampleCode() { optionName = "GetButton ( string buttonName )", optionDescription = "Returns the current state of the buttons state. True for being pressed down and false for no input.", basicCode = "UltimateButton.GetButton( \"{0}\" )" },
		new ExampleCode() { optionName = "GetTapCount ( string buttonName )", optionDescription = "Returns true when the user has achieved the tap count.", basicCode = "UltimateButton.GetTapCount( \"{0}\" )" },
		new ExampleCode() { optionName = "GetUltimateButton ( string buttonName )", optionDescription = "Returns the Ultimate Button component that has been registered with the targeted button name.", basicCode = "UltimateButton jumpButton = UltimateButton.GetUltimateButton( \"{0}\" );" },
		new ExampleCode() { optionName = "DisableButton ( string buttonName )", optionDescription = "Disables the Ultimate Button.", basicCode = "UltimateButton.DisableButton( \"{0}\" );" },
		new ExampleCode() { optionName = "EnableButton ( string buttonName )", optionDescription = "Enables the Ultimate Button.", basicCode = "UltimateButton.EnableButton( \"{0}\" );" },
	};
	List<string> exampleCodeOptions = new List<string>();
	int exampleCodeIndex = 0;

	// -----< BUTTON EVENTS >----- //
	SerializedProperty onButtonDown;
	SerializedProperty onButtonUp;
	SerializedProperty tapCountEvent;

	// -----< DEVELOPMENT MODE >----- //
	public bool showDefaultInspector = false;

	// -----< SCENE GUI >----- //
	class DisplaySceneGizmo
	{
		public int frames = maxFrames;
		public bool hover = false;

		public bool HighlightGizmo
		{
			get
			{
				return hover || frames < maxFrames;
			}
		}

		public void PropertyUpdated ()
		{
			frames = 0;
		}
	}
	DisplaySceneGizmo DisplayRelativeSpaceMod = new DisplaySceneGizmo();
	DisplaySceneGizmo DisplayActivationRange = new DisplaySceneGizmo();
	DisplaySceneGizmo DisplayOrbitRadius = new DisplaySceneGizmo();
	DisplaySceneGizmo DisplayCenterAngle = new DisplaySceneGizmo();
	DisplaySceneGizmo DisplayBoundary = new DisplaySceneGizmo();
	DisplaySceneGizmo DisplayCooldownTextAnchor = new DisplaySceneGizmo();
	const int maxFrames = 200;

	// GIZMO COLORS //
	Color colorDefault = Color.black;
	Color colorValueChanged = Color.black;

	// -----< EDITOR STYLES >----- //
	GUIStyle handlesCenteredText = new GUIStyle();
	GUIStyle collapsableSectionStyle = new GUIStyle();

	// Multi Button Options //
	float multiCenterAngle = 315.0f;
	float multiAnglePer = 45.0f;

	

	void OnEnable ()
	{
		StoreReferences();

		Undo.undoRedoPerformed += UndoRedoCallback;

		if( targ != null )
		{
			if( !targ.gameObject.GetComponent<Image>() )
				Undo.AddComponent<Image>( targ.gameObject );

			Undo.RecordObject( targ.gameObject.GetComponent<Image>(), "Null Image Alpha" );
			targ.gameObject.GetComponent<Image>().color = new Color( 1.0f, 1.0f, 1.0f, 0.0f );
		}
		
		if( EditorPrefs.HasKey( "UB_ColorHexSetup" ) )
		{
			ColorUtility.TryParseHtmlString( EditorPrefs.GetString( "UB_ColorDefaultHex" ), out colorDefault );
			ColorUtility.TryParseHtmlString( EditorPrefs.GetString( "UB_ColorValueChangedHex" ), out colorValueChanged );
		}

		parentCanvas = GetParentCanvas();
	}

	void OnDisable ()
	{
		Undo.undoRedoPerformed -= UndoRedoCallback;
	}
	
	void UndoRedoCallback ()
	{
		StoreReferences();
	}

	void StoreReferences ()
	{
		targ = ( UltimateButton )target;

		if( targ == null )
			return;

		isPrefabInProjectWindow = AssetDatabase.Contains( targ.gameObject );

		// -----< BUTTON POSITIONING >----- //
		buttonBase = serializedObject.FindProperty( "buttonBase" );
		if( targ.buttonBase != null && targ.buttonBase.sprite != null )
			buttonBaseSprite = targ.buttonBase.sprite;

		positioning = serializedObject.FindProperty( "positioning" );
		relativeTransform = serializedObject.FindProperty( "relativeTransform" );
		relativeSpaceMod = serializedObject.FindProperty( "relativeSpaceMod" );
		scalingAxis = serializedObject.FindProperty( "scalingAxis" );
		anchor = serializedObject.FindProperty( "anchor" );
		activationRange = serializedObject.FindProperty( "activationRange" );
		buttonSize = serializedObject.FindProperty( "buttonSize" );
		positionHorizontal = serializedObject.FindProperty( "positionHorizontal" );
		positionVertical = serializedObject.FindProperty( "positionVertical" );
		orbitTransform = serializedObject.FindProperty( "orbitTransform" );
		orbitDistance = serializedObject.FindProperty( "orbitDistance" );
		centerAngle = serializedObject.FindProperty( "centerAngle" );
		
		// -----< BUTTON SETTINGS >----- //
		boundary = serializedObject.FindProperty( "boundary" );
		trackInput = serializedObject.FindProperty( "trackInput" );
		transmitInput = serializedObject.FindProperty( "transmitInput" );
		receiver = serializedObject.FindProperty( "receiver" );
		tapCountOption = serializedObject.FindProperty( "tapCountOption" );
		tapCountDuration = serializedObject.FindProperty( "tapCountDuration" );
		targetTapCount = serializedObject.FindProperty( "targetTapCount" );
		useTouchInput = serializedObject.FindProperty( "useTouchInput" );
		
		// -----< VISUAL OPTIONS >----- //
		baseColor = targ.buttonBase == null ? Color.white : targ.buttonBase.color;

		inputTransition = serializedObject.FindProperty( "inputTransition" );
		transitionUntouchedDuration = serializedObject.FindProperty( "transitionUntouchedDuration" );
		transitionTouchedDuration = serializedObject.FindProperty( "transitionTouchedDuration" );
		useTension = serializedObject.FindProperty( "useTension" );
		tensionColorNone = serializedObject.FindProperty( "tensionColorNone" );
		tensionColorFull = serializedObject.FindProperty( "tensionColorFull" );
		tensionAccent = serializedObject.FindProperty( "tensionAccent" );
		if( targ.tensionAccent != null && targ.tensionAccent.sprite != null )
			tensionAccentSprite = targ.tensionAccent.sprite;
		useFade = serializedObject.FindProperty( "useFade" );
		fadeUntouched = serializedObject.FindProperty( "fadeUntouched" );
		fadeTouched = serializedObject.FindProperty( "fadeTouched" );
		useScale = serializedObject.FindProperty( "useScale" );
		scaleTouched = serializedObject.FindProperty( "scaleTouched" );

		useHighlight = serializedObject.FindProperty( "useHighlight" );
		highlightColor = serializedObject.FindProperty( "highlightColor" );
		buttonHighlight = serializedObject.FindProperty( "buttonHighlight" );
		if( targ.buttonHighlight != null && targ.buttonHighlight.sprite != null )
			buttonHighlightSprite = targ.buttonHighlight.sprite;

		useCooldown = serializedObject.FindProperty( "useCooldown" );
		buttonCooldown = serializedObject.FindProperty( "buttonCooldown" );
		useCooldownText = serializedObject.FindProperty( "useCooldownText" );
		displayDecimalCooldown = serializedObject.FindProperty( "displayDecimalCooldown" );
		cooldownTextScaleCurve = serializedObject.FindProperty( "cooldownTextScaleCurve" );
		cooldownText = serializedObject.FindProperty( "cooldownText" );
		if( targ.buttonCooldown != null )
		{
			fillMethod = targ.buttonCooldown.fillMethod;
			cooldownTestValue = targ.buttonCooldown.fillAmount * cooldownTimeMax;

			if( targ.buttonCooldown.sprite != null )
				cooldownSprite = targ.buttonCooldown.sprite;

			cooldownColor = targ.buttonCooldown.color;
		}
		if( targ.useCooldownText && targ.cooldownText != null )
		{
			textAnchorMod = Mathf.Lerp( -1.0f, 1.0f, targ.cooldownText.rectTransform.anchorMax.y );
			textFont = targ.cooldownText.font;
			textColor = targ.cooldownText.color;
			if( targ.cooldownText.GetComponent<Outline>() )
			{
				textOutline = true;
				textOutlineColor = targ.cooldownText.GetComponent<Outline>().effectColor;
			}
		}
		useIcon = serializedObject.FindProperty( "useIcon" );
		buttonIcon = serializedObject.FindProperty( "buttonIcon" );
		if( targ.buttonIcon != null )
		{
			iconSprite = targ.buttonIcon.sprite;
			iconColor = targ.buttonIcon.color;
			iconScale = targ.buttonIcon.rectTransform.localScale.x;
		}

		useIconMask = targ.buttonIcon != null && targ.buttonIcon.transform.parent != targ.buttonBase.transform;
		if( useIconMask )
			buttonIconMask = targ.buttonIcon.transform.parent.GetComponent<Image>();
		if( useIconMask && buttonIconMask != null && buttonIconMask.sprite != null )
			iconMaskSprite = buttonIconMask.sprite;

		// ------< SCRIPT REFERENCE >------ //
		buttonName = serializedObject.FindProperty( "buttonName" );
		exampleCodeOptions = new List<string>();
		for( int i = 0; i < exampleCodes.Length; i++ )
			exampleCodeOptions.Add( exampleCodes[ i ].optionName );

		// ------< BUTTON EVENTS >------ //
		onButtonDown = serializedObject.FindProperty( "onButtonDown" );
		onButtonUp = serializedObject.FindProperty( "onButtonUp" );
		tapCountEvent = serializedObject.FindProperty( "tapCountEvent" );
		
		StoreChildTransforms();

		// MULTI-BUTTON OPTIONS //
		if( targets.Length > 1 )
		{
			List<UltimateButton> buttonsToUse = new List<UltimateButton>();

			for( int i = 0; i < targets.Length; i++ )
				buttonsToUse.Add( ( UltimateButton )targets[ i ] );

			float smallestAngle = 360.0f;
			float largestAngle = 0.0f;

			for( int i = 0; i < buttonsToUse.Count; i++ )
			{
				if( buttonsToUse[ i ].centerAngle < smallestAngle )
					smallestAngle = buttonsToUse[ i ].centerAngle;

				if( buttonsToUse[ i ].centerAngle > largestAngle )
					largestAngle = buttonsToUse[ i ].centerAngle;
			}

			multiCenterAngle = smallestAngle + ( ( largestAngle - smallestAngle ) / 2 );
			multiAnglePer = ( largestAngle - smallestAngle ) / ( buttonsToUse.Count - 1 );
		}
	}

	void StoreChildTransforms ()
	{
		if( targ.transform == null )
			return;

		if( targ.buttonBase == null )
			return;

		childTransforms = new List<RectTransform>();
		RectTransform[] childRectTrans = targ.buttonBase.GetComponentsInChildren<RectTransform>();
		for( int i = 0; i < childRectTrans.Length; i++ )
		{
			if( targ.buttonBase != null && childRectTrans[ i ] == targ.buttonBase.rectTransform )
				continue;

			if( targ.buttonIcon != null && childRectTrans[ i ] == targ.buttonIcon.rectTransform )
			{
				if( useIconMask )
					continue;
			}

			if( targ.useCooldown && targ.useCooldownText && targ.cooldownText != null && childRectTrans[ i ] == targ.cooldownText.rectTransform )
				continue;

			childTransforms.Add( childRectTrans[ i ] );
		}

		childObjects = new ReorderableList( childTransforms, typeof( RectTransform ), true, false, false, false );

		childObjects.drawHeaderCallback = ( Rect rect ) =>
		{
			EditorGUI.LabelField( rect, "Ultimate Button" );
		};

		childObjects.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
		{
			if( index > childTransforms.Count - 1 )
				return;

			var element = childObjects.list;
			rect.y += 2;
			EditorGUI.LabelField( new Rect( rect.x, rect.y, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight ), childTransforms[ index ].name );
		};

		childObjects.onChangedCallback = ( ReorderableList l ) =>
		{
			for( int i = 0; i < childTransforms.Count; i++ )
			{
				childTransforms[ i ].SetSiblingIndex( i );
			}
		};
	}

	Canvas GetParentCanvas ()
	{
		if( Selection.activeGameObject == null )
			return null;

		Transform parent = Selection.activeGameObject.transform.parent;

		while( parent != null )
		{
			if( parent.transform.GetComponent<Canvas>() && parent.transform.GetComponent<Canvas>().enabled )
				return parent.transform.GetComponent<Canvas>();

			parent = parent.transform.parent;
		}

		if( parent == null && !AssetDatabase.Contains( Selection.activeGameObject ) )
			RequestCanvas( Selection.activeGameObject );

		return null;
	}

	void DisplayHeaderDropdown ( string headerName, string editorPref )
	{
		EditorGUILayout.Space();

		GUIStyle toolbarStyle = new GUIStyle( EditorStyles.toolbarButton ) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 11 };
		GUILayout.BeginHorizontal();
		GUILayout.Space( -10 );
		EditorPrefs.SetBool( editorPref, GUILayout.Toggle( EditorPrefs.GetBool( editorPref ), ( EditorPrefs.GetBool( editorPref ) ? "▼ " : "► " ) + headerName, toolbarStyle ) );
		GUILayout.EndHorizontal();

		if( EditorPrefs.GetBool( editorPref ) == true )
			EditorGUILayout.Space();
	}

	bool DisplayCollapsibleBoxSection ( string sectionTitle, string editorPref, SerializedProperty enabledProp, ref bool valueChanged )
	{
		if( EditorPrefs.GetBool( editorPref ) && enabledProp.boolValue )
			collapsableSectionStyle.fontStyle = FontStyle.Bold;

		EditorGUILayout.BeginHorizontal();

		EditorGUI.BeginChangeCheck();
		enabledProp.boolValue = EditorGUILayout.Toggle( enabledProp.boolValue, GUILayout.Width( 25 ) );
		if( EditorGUI.EndChangeCheck() )
		{
			serializedObject.ApplyModifiedProperties();

			if( enabledProp.boolValue )
				EditorPrefs.SetBool( editorPref, true );
			else
				EditorPrefs.SetBool( editorPref, false );

			valueChanged = true;
		}

		GUILayout.Space( -25 );

		EditorGUI.BeginDisabledGroup( !enabledProp.boolValue );
		if( GUILayout.Button( sectionTitle, collapsableSectionStyle ) )
			EditorPrefs.SetBool( editorPref, !EditorPrefs.GetBool( editorPref ) );
		EditorGUI.EndDisabledGroup();

		EditorGUILayout.EndHorizontal();

		if( EditorPrefs.GetBool( editorPref ) )
			collapsableSectionStyle.fontStyle = FontStyle.Normal;

		return EditorPrefs.GetBool( editorPref ) && enabledProp.boolValue;
	}
	
	public override void OnInspectorGUI ()
	{
		serializedObject.Update();

		handlesCenteredText = new GUIStyle( EditorStyles.label ) { normal = new GUIStyleState() { textColor = Color.white } };

		collapsableSectionStyle = new GUIStyle( EditorStyles.label ) { alignment = TextAnchor.MiddleCenter };
		collapsableSectionStyle.active.textColor = collapsableSectionStyle.normal.textColor;

		bool valueChanged = false;

		// PREFAB WARNINGS //
		if( isPrefabInProjectWindow )
			EditorGUILayout.HelpBox( "Objects cannot be generated while selecting a Prefab within the Project window. Please make sure to drag this prefab into the scene before trying to generate objects.", MessageType.Warning );

		// BUTTON POSITIONING //
		DisplayHeaderDropdown( "Button Positioning", "UB_ButtonPositioning" );
		if( EditorPrefs.GetBool( "UB_ButtonPositioning" ) )
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( buttonBase, new GUIContent( "Button Base", "The base image to be used for the Ultimate Button." ) );
			if( EditorGUI.EndChangeCheck() )
			{
				serializedObject.ApplyModifiedProperties();

				if( targ.buttonBase != null )
					buttonBaseSprite = targ.buttonBase.sprite;
			}

			EditorGUI.BeginChangeCheck();
			buttonBaseSprite = ( Sprite )EditorGUILayout.ObjectField( "└ Base Sprite", buttonBaseSprite, typeof( Sprite ), true, GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
			if( EditorGUI.EndChangeCheck() )
			{
				if( targ.buttonBase != null )
				{
					Undo.RecordObject( targ.buttonBase, "Update Button Base Sprite" );
					targ.buttonBase.sprite = buttonBaseSprite;
				}
			}

			if( targ.buttonBase == null )
			{
				EditorGUI.BeginDisabledGroup( buttonBaseSprite == null || isPrefabInProjectWindow );
				if( GUILayout.Button( "Generate Button Base", EditorStyles.miniButton ) )
				{
					GameObject newGameObject = new GameObject();
					newGameObject.AddComponent<RectTransform>();
					newGameObject.AddComponent<CanvasRenderer>();
					newGameObject.AddComponent<Image>();

					newGameObject.GetComponent<Image>().sprite = buttonBaseSprite;
					newGameObject.GetComponent<Image>().color = baseColor;

					newGameObject.transform.SetParent( targ.transform );
					newGameObject.transform.SetAsFirstSibling();

					newGameObject.name = "Button Base";

					RectTransform trans = newGameObject.GetComponent<RectTransform>();

					trans.anchorMin = new Vector2( 0.5f, 0.5f );
					trans.anchorMax = new Vector2( 0.5f, 0.5f );
					trans.pivot = new Vector2( 0.5f, 0.5f );
					trans.anchoredPosition = Vector2.zero;
					trans.localScale = Vector3.one;
					trans.localPosition = Vector3.zero;
					trans.localRotation = Quaternion.identity;

					serializedObject.FindProperty( "buttonBase" ).objectReferenceValue = newGameObject.GetComponent<Image>();
					serializedObject.ApplyModifiedProperties();

					Undo.RegisterCreatedObjectUndo( newGameObject, "Create Button Base Object" );

					StoreChildTransforms();
				}
				EditorGUI.EndDisabledGroup();
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( positioning, new GUIContent( "Positioning", "Determines how the Ultimate Button should be positioned, if at all." ) );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();
			
			if( targ.positioning != UltimateButton.Positioning.Disabled )
			{
				if( targ.positioning == UltimateButton.Positioning.RelativeToTransform )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( relativeTransform, new GUIContent( "Relative Transform", "The RectTransform component to position the button in relative position to." ) );
					if( EditorGUI.EndChangeCheck() )
						serializedObject.ApplyModifiedProperties();

					if( RelativeTransformCanvasError() )
						EditorGUILayout.HelpBox( "The assigned Relative Transform is not within the same canvas. This may cause errors and unwanted behavior. Please make sure the Relative Transform is within the same canvas object.", MessageType.Error );
				}
				else
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( scalingAxis, new GUIContent( "Scaling Axis", "The axis of the screen to scale the size of the Ultimate Button." ) );
					EditorGUILayout.PropertyField( anchor, new GUIContent( "Anchor", "Determines which side of the screen the button should be anchored to." ) );
					if( EditorGUI.EndChangeCheck() )
						serializedObject.ApplyModifiedProperties();
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider( buttonSize, 0.0f, 5.0f, new GUIContent( "Button Size", "The overall size of the button." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider( activationRange, 0.0f, 2.0f, new GUIContent( "Activation Range", "The range that the Ultimate Button will react to when initiating and dragging the input." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();
					PropertyUpdated( DisplayActivationRange );
				}
				CheckPropertyHover( DisplayActivationRange );

				EditorGUILayout.BeginVertical( "Box" );
				collapsableSectionStyle.fontStyle = FontStyle.Bold;
				EditorGUILayout.LabelField( "Button Position", collapsableSectionStyle );
				collapsableSectionStyle.fontStyle = FontStyle.Normal;

				if( targ.positioning == UltimateButton.Positioning.RelativeToTransform )
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUI.BeginChangeCheck();
					GUILayout.Toggle( !orbitTransform.boolValue, "Default", EditorStyles.miniButtonLeft );
					if( EditorGUI.EndChangeCheck() )
					{
						orbitTransform.boolValue = false;
						serializedObject.ApplyModifiedProperties();
					}

					EditorGUI.BeginChangeCheck();
					GUILayout.Toggle( orbitTransform.boolValue, " Orbit ", EditorStyles.miniButtonRight );
					if( EditorGUI.EndChangeCheck() )
					{
						orbitTransform.boolValue = true;
						serializedObject.ApplyModifiedProperties();
					}
					EditorGUILayout.EndHorizontal();
				}

				if( targ.positioning == UltimateButton.Positioning.RelativeToTransform && targ.orbitTransform )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.Slider( orbitDistance, 0.0f, 2.0f, new GUIContent( "Orbit Distance", "The distance for the button to orbit around the relative transform." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						serializedObject.ApplyModifiedProperties();

						PropertyUpdated( DisplayOrbitRadius );
					}
					CheckPropertyHover( DisplayOrbitRadius );

					EditorGUI.BeginDisabledGroup( targets.Length > 1 );
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.Slider( centerAngle, 0.0f, 360.0f, new GUIContent( "Center Angle", "The center angle of the button." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						serializedObject.ApplyModifiedProperties();
						PropertyUpdated( DisplayCenterAngle );
					}
					CheckPropertyHover( DisplayCenterAngle );
					EditorGUI.EndDisabledGroup();

					if( targets.Length > 1 )
					{
						List<UltimateButton> buttonsToUse = new List<UltimateButton>();
						bool allUsingOrbitTransform = true;

						for( int i = 0; i < targets.Length; i++ )
						{
							buttonsToUse.Add( ( UltimateButton )targets[ i ] );

							if( buttonsToUse[ i ].positioning != UltimateButton.Positioning.RelativeToTransform || !targ.orbitTransform )
							{
								allUsingOrbitTransform = false;
								break;
							}
						}

						if( allUsingOrbitTransform )
						{
							collapsableSectionStyle.fontStyle = FontStyle.Bold;
							EditorGUILayout.LabelField( "Multi Button Options", collapsableSectionStyle );
							collapsableSectionStyle.fontStyle = FontStyle.Normal;

							multiCenterAngle = EditorGUILayout.Slider( "Center Angle", multiCenterAngle, 0.0f, 360.0f );
							multiAnglePer = EditorGUILayout.Slider( "Angle Per", multiAnglePer, 0.0f, ( 360.0f / buttonsToUse.Count ) );

							EditorGUI.BeginDisabledGroup( buttonsToUse.Count == 0 );
							if( GUILayout.Button( "Auto Arrange Buttons", EditorStyles.miniButton ) )
							{
								float totalAngle = ( buttonsToUse.Count - 1 ) * multiAnglePer;
								float start = multiCenterAngle - ( totalAngle / 2 );
								for( int i = 0; i < buttonsToUse.Count; i++ )
								{
									Undo.RecordObject( buttonsToUse[ i ], "Auto Arrange Ultimate Buttons" );
									buttonsToUse[ i ].centerAngle = start + ( multiAnglePer * i );

									if( i > 0 )
									{
										Undo.RecordObject( buttonsToUse[ i ].transform, "Auto Arrange Ultimate Buttons" );
										buttonsToUse[ i ].transform.SetSiblingIndex( buttonsToUse[ 0 ].transform.GetSiblingIndex() + i );
									}
								}
							}
							EditorGUI.EndDisabledGroup();
						}
					}
				}
				else
				{
					if( targ.positioning == UltimateButton.Positioning.RelativeToTransform )
					{
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.Slider( relativeSpaceMod, 1.5f, 2.5f, new GUIContent( "Relative Space", "The amount of space around the relative transform where the button can be positioned." ) );
						if( EditorGUI.EndChangeCheck() )
						{
							serializedObject.ApplyModifiedProperties();
							PropertyUpdated( DisplayRelativeSpaceMod );
						}
						CheckPropertyHover( DisplayRelativeSpaceMod );
					}

					EditorGUI.BeginChangeCheck();
					EditorGUILayout.Slider( positionHorizontal, 0.0f, targ.positioning == UltimateButton.Positioning.RelativeToTransform ? 100.0f : 50.0f, new GUIContent( "Horizontal Position", "The horizontal position of the button." ) );
					EditorGUILayout.Slider( positionVertical, 0.0f, 100.0f, new GUIContent( "Vertical Position", "The vertical position of the button." ) );
					if( EditorGUI.EndChangeCheck() )
						serializedObject.ApplyModifiedProperties();

					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( serializedObject.FindProperty( "useActivationSize" ) );
					if( EditorGUI.EndChangeCheck() )
						serializedObject.ApplyModifiedProperties();
				}
				EditorGUILayout.EndVertical();
			}
		}

		// INPUT OPTIONS //
		DisplayHeaderDropdown( "Input Settings", "UB_InputSettings" );
		if( EditorPrefs.GetBool( "UB_InputSettings" ) )
		{
			EditorGUI.BeginDisabledGroup( targ.positioning == UltimateButton.Positioning.Disabled );

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( boundary, new GUIContent( "Boundary", "Determines whether the boundary should be circular or square. This option affects how the Activation Range and Track Input options function." ) );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( trackInput, new GUIContent( "Track Input", "Enabling this option will allow the Ultimate Button to track the users input to ensure that button events and states are only called when the input is within the Activation Range." ) );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();

			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( transmitInput, new GUIContent( "Transmit Input", "Should the Ultimate Button transmit input events to another UI game object?" ) );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();

			if( targ.transmitInput )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( receiver, new GUIContent( "└ Input Receiver", "The UI game object to send the input to." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();

				EditorGUILayout.Space();
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( tapCountOption, new GUIContent( "Tap Count Option", "Determines if the button should calculate the amount of taps on it in a give amount of time." ) );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();

			if( targ.tapCountOption != UltimateButton.TapCountOption.NoCount )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider( tapCountDuration, 0.0f, 1.0f, new GUIContent( "Tap Time Window", "The time in seconds that the player is allowed in order to achieve the tap count." ) );
				EditorGUI.BeginDisabledGroup( targ.tapCountOption != UltimateButton.TapCountOption.Accumulate );
				EditorGUILayout.IntSlider( targetTapCount, 1, 5, new GUIContent( "Target Tap Count", "The target number of taps the player must achieve." ) );
				EditorGUI.EndDisabledGroup();
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if( GUILayout.Button( "Example Code" ) )
				{
					EditorPrefs.SetBool( "UB_ScriptReference", true );
					exampleCodeIndex = 3;
				}
				if( GUILayout.Button( "Button Events" ) )
					EditorPrefs.SetBool( "UB_ButtonEvents", true );

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
		}

		// BUTTON SETTINGS //
		DisplayHeaderDropdown( "Button Options", "UB_ButtonOptions" );
		if( EditorPrefs.GetBool( "UB_ButtonOptions" ) )
		{
			// --------------------------< BASE COLOR >------------------------- //
			EditorGUI.BeginChangeCheck();
			baseColor = EditorGUILayout.ColorField( "Base Color", baseColor );
			if( EditorGUI.EndChangeCheck() && targ.buttonBase != null )
			{
				Undo.RecordObject( targ.buttonBase, "Change Base Color" );
				targ.buttonBase.enabled = false;
				targ.buttonBase.color = baseColor;
				targ.buttonBase.enabled = true;
			}
			// ------------------------< END BASE COLOR >----------------------- //

			// -----------------------< INPUT TRANSITION >---------------------- //
			valueChanged = false;
			EditorGUILayout.BeginVertical( "Box" );
			if( DisplayCollapsibleBoxSection( "Input Transition", "UB_InputTransition", inputTransition, ref valueChanged ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( useTension, new GUIContent( "Use Tension", "Determines if a specific image should be used to display the input state of the button." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					if( targ.tensionAccent != null )
					{
						Undo.RecordObject( targ.tensionAccent.gameObject, ( targ.useTension ? "Enable" : "Disable" ) + " Tension Accent" );
						targ.tensionAccent.gameObject.SetActive( targ.useTension );

						StoreChildTransforms();
					}
				}

				if( targ.useTension )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( tensionAccent, new GUIContent( "Tension Accent", "The image component to be used for the button tension accent." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						serializedObject.ApplyModifiedProperties();

						if( targ.tensionAccent != null )
						{
							Undo.RecordObject( targ.tensionAccent, "Update Tension Color" );
							targ.tensionAccent.color = targ.tensionColorNone;
						}
					}

					EditorGUI.BeginChangeCheck();
					tensionAccentSprite = ( Sprite )EditorGUILayout.ObjectField( "└ Image Sprite", tensionAccentSprite, typeof( Sprite ), true, GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
					if( EditorGUI.EndChangeCheck() )
					{
						if( targ.tensionAccent != null )
						{
							Undo.RecordObject( targ.tensionAccent, "Update Tension Accent Sprite" );
							targ.tensionAccent.sprite = tensionAccentSprite;
						}
					}

					if( targ.tensionAccent == null )
					{
						EditorGUI.BeginDisabledGroup( tensionAccentSprite == null || isPrefabInProjectWindow );
						if( GUILayout.Button( "Generate Tension Image", EditorStyles.miniButton ) )
						{
							GameObject newGameObject = new GameObject();
							newGameObject.AddComponent<RectTransform>();
							newGameObject.AddComponent<CanvasRenderer>();
							newGameObject.AddComponent<Image>();

							newGameObject.GetComponent<Image>().sprite = tensionAccentSprite;
							newGameObject.GetComponent<Image>().color = targ.tensionColorNone;

							newGameObject.transform.SetParent( targ.buttonBase.transform );
							newGameObject.transform.SetAsLastSibling();

							newGameObject.name = "Tension Accent";

							RectTransform trans = newGameObject.GetComponent<RectTransform>();

							trans.anchorMin = new Vector2( 0.0f, 0.0f );
							trans.anchorMax = new Vector2( 1.0f, 1.0f );
							trans.offsetMin = Vector2.zero;
							trans.offsetMax = Vector2.zero;
							trans.pivot = new Vector2( 0.5f, 0.5f );
							trans.anchoredPosition = Vector2.zero;
							trans.localScale = Vector3.one;
							trans.localPosition = Vector3.zero;
							trans.localRotation = Quaternion.identity;

							serializedObject.FindProperty( "tensionAccent" ).objectReferenceValue = newGameObject.GetComponent<Image>();
							serializedObject.ApplyModifiedProperties();

							Undo.RegisterCreatedObjectUndo( newGameObject, "Create Tension Accent Object" );

							StoreChildTransforms();
						}
						EditorGUI.EndDisabledGroup();
					}
					EditorGUILayout.Space();
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( useFade );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					Undo.RecordObject( targ.gameObject.GetComponent<CanvasGroup>(), "Enable Button Fade" );
					targ.gameObject.GetComponent<CanvasGroup>().alpha = targ.useFade ? targ.fadeUntouched : 1.0f;
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( useScale );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();
				
				EditorGUILayout.Space();

				EditorGUILayout.LabelField( "Untouched State", EditorStyles.boldLabel );

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( transitionUntouchedDuration, new GUIContent( "Transition Duration", "The time is seconds for the transition to the untouched state." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					if( transitionUntouchedDuration.floatValue < 0 )
						transitionUntouchedDuration.floatValue = 0.0f;

					serializedObject.ApplyModifiedProperties();
				}

				if( targ.useTension )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( tensionColorNone, new GUIContent( "Tension Color", "The Color of the Tension with no input." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						serializedObject.ApplyModifiedProperties();

						if( targ.tensionAccent != null )
						{
							Undo.RecordObject( targ.tensionAccent, "Update Tension Color" );
							targ.tensionAccent.color = targ.tensionColorNone;
						}
					}
				}

				if( targ.useFade )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.Slider( fadeUntouched, 0.0f, 1.0f, new GUIContent( "Untouched Alpha", "The alpha of the button when it is not receiving input." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						serializedObject.ApplyModifiedProperties();
						Undo.RecordObject( targ.gameObject.GetComponent<CanvasGroup>(), "Edit Button Fade" );
						targ.gameObject.GetComponent<CanvasGroup>().alpha = targ.fadeUntouched;
					}
				}
				
				EditorGUILayout.Space();

				EditorGUILayout.LabelField( "Touched State", EditorStyles.boldLabel );

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( transitionTouchedDuration, new GUIContent( "Transition Duration", "The time is seconds for the transition to the touched state." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					if( transitionTouchedDuration.floatValue < 0 )
						transitionTouchedDuration.floatValue = 0.0f;

					serializedObject.ApplyModifiedProperties();
				}

				EditorGUI.BeginChangeCheck();
				if( targ.useTension )
					EditorGUILayout.PropertyField( tensionColorFull, new GUIContent( "Tension Color", "The Color of the Tension when there is input." ) );
				if( targ.useFade )
					EditorGUILayout.Slider( fadeTouched, 0.0f, 1.0f, new GUIContent( "Touched Alpha", "The alpha of the button when receiving input." ) );
				if( targ.useScale )
					EditorGUILayout.Slider( scaleTouched, 0.0f, 2.0f, new GUIContent( "Touched Scale", "The scale of the button when receiving input." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();
				
				GUILayout.Space( 1 );
			}
			EditorGUILayout.EndVertical();
			if( valueChanged )
			{
				if( !targ.gameObject.GetComponent<CanvasGroup>() )
					targ.gameObject.AddComponent<CanvasGroup>();

				if( targ.inputTransition && targ.useFade )
				{
					Undo.RecordObject( targ.gameObject.GetComponent<CanvasGroup>(), "Enable Input Transition" );
					targ.gameObject.GetComponent<CanvasGroup>().alpha = targ.fadeUntouched;
				}
				else
				{
					Undo.RecordObject( targ.gameObject.GetComponent<CanvasGroup>(), "Disable Input Transition" );
					targ.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
				}

				if( targ.useTension && targ.tensionAccent != null )
				{
					Undo.RecordObject( targ.tensionAccent.gameObject, ( targ.useTension ? "Enable" : "Disable" ) + " Tension Accent" );
					targ.tensionAccent.gameObject.SetActive( targ.inputTransition && targ.useTension );

					StoreChildTransforms();
				}
			}
			// ---------------------< END INPUT TRANSITION >-------------------- //

			// --------------------------< HIGHLIGHT >-------------------------- //
			valueChanged = false;
			EditorGUILayout.BeginVertical( "Box" );
			if( DisplayCollapsibleBoxSection( "Highlight", "UB_Highlight", useHighlight, ref valueChanged ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( highlightColor, new GUIContent( "Highlight Color", "The color to apply to the highlight image." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					if( targ.buttonHighlight != null )
					{
						Undo.RecordObject( targ.buttonHighlight, "Update Highlight Color" );
						targ.buttonHighlight.color = targ.highlightColor;
					}
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( buttonHighlight, new GUIContent( "Button Highlight", "The highlight image to use." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					if( targ.buttonHighlight != null )
					{
						Undo.RecordObject( targ.buttonHighlight, "Assign Button Highlight" );
						targ.buttonHighlight.color = targ.highlightColor;
					}
				}

				EditorGUI.BeginChangeCheck();
				buttonHighlightSprite = ( Sprite )EditorGUILayout.ObjectField( "└ Image Sprite", buttonHighlightSprite, typeof( Sprite ), true, GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
				if( EditorGUI.EndChangeCheck() && targ.buttonHighlight != null )
				{
					Undo.RecordObject( targ.buttonHighlight, "Update Button Highlight Sprite" );
					targ.buttonHighlight.sprite = buttonHighlightSprite;
				}

				if( targ.buttonHighlight == null )
				{
					EditorGUI.BeginDisabledGroup( buttonHighlightSprite == null || isPrefabInProjectWindow );
					if( GUILayout.Button( "Generate Button Highlight", EditorStyles.miniButton ) )
					{
						GameObject newGameObject = new GameObject();
						newGameObject.AddComponent<RectTransform>();
						newGameObject.AddComponent<CanvasRenderer>();
						newGameObject.AddComponent<Image>();

						newGameObject.GetComponent<Image>().sprite = buttonHighlightSprite;
						newGameObject.GetComponent<Image>().color = targ.highlightColor;

						newGameObject.transform.SetParent( targ.buttonBase.transform );
						newGameObject.transform.SetAsFirstSibling();

						newGameObject.name = "Button Highlight";

						RectTransform trans = newGameObject.GetComponent<RectTransform>();

						trans.anchorMin = new Vector2( 0.0f, 0.0f );
						trans.anchorMax = new Vector2( 1.0f, 1.0f );
						trans.offsetMin = Vector2.zero;
						trans.offsetMax = Vector2.zero;
						trans.pivot = new Vector2( 0.5f, 0.5f );
						trans.anchoredPosition = Vector2.zero;
						trans.localScale = Vector3.one;
						trans.localPosition = Vector3.zero;
						trans.localRotation = Quaternion.identity;

						serializedObject.FindProperty( "buttonHighlight" ).objectReferenceValue = newGameObject.GetComponent<Image>();
						serializedObject.ApplyModifiedProperties();

						Undo.RegisterCreatedObjectUndo( newGameObject, "Create Button Highlight Object" );

						StoreChildTransforms();
					}
					EditorGUI.EndDisabledGroup();
				}

				GUILayout.Space( 1 );
			}
			EditorGUILayout.EndVertical();
			if( valueChanged && targ.buttonHighlight != null )
			{
				Undo.RecordObject( targ.buttonHighlight.gameObject, ( targ.useHighlight ? "Enable" : "Disable" ) + " Button Highlight" );
				targ.buttonHighlight.gameObject.SetActive( targ.useHighlight );

				StoreChildTransforms();
			}
			// ------------------------< END HIGHLIGHT >------------------------ //
			
			// ------------------------------ ICON SETTINGS ----------------------------- //
			valueChanged = false;
			EditorGUILayout.BeginVertical( "Box" );
			if( DisplayCollapsibleBoxSection( "Icon", "UMQ_IconSettings", useIcon, ref valueChanged ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( buttonIcon, new GUIContent( "Button Icon", "The image component to be used for the button icon." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();

				EditorGUI.BeginChangeCheck();
				iconSprite = ( Sprite )EditorGUILayout.ObjectField( "└ Image Sprite", iconSprite, typeof( Sprite ), true, GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
				if( EditorGUI.EndChangeCheck() && targ.buttonIcon != null )
				{
					Undo.RecordObject( targ.buttonIcon, "Update Icon Sprite" );
					targ.buttonIcon.sprite = iconSprite;
				}

				if( targ.buttonIcon == null )
				{
					EditorGUI.BeginDisabledGroup( iconSprite == null || isPrefabInProjectWindow );
					if( GUILayout.Button( "Generate Icon Image", EditorStyles.miniButton ) )
					{
						GameObject newGameObject = new GameObject();
						RectTransform trans = newGameObject.AddComponent<RectTransform>();
						newGameObject.AddComponent<CanvasRenderer>();
						Image imageComponent = newGameObject.AddComponent<Image>();

						imageComponent.color = iconColor;
						if( iconSprite != null )
							imageComponent.sprite = iconSprite;

						newGameObject.transform.SetParent( targ.buttonBase.transform );
						newGameObject.transform.SetAsFirstSibling();

						trans.anchorMin = new Vector2( 0.0f, 0.0f );
						trans.anchorMax = new Vector2( 1.0f, 1.0f );
						trans.offsetMin = Vector2.zero;
						trans.offsetMax = Vector2.zero;
						trans.localScale = Vector3.one;
						trans.localPosition = Vector3.zero;
						trans.localRotation = Quaternion.identity;

						newGameObject.name = "Button Icon";

						serializedObject.FindProperty( "buttonIcon" ).objectReferenceValue = imageComponent;
						serializedObject.ApplyModifiedProperties();

						Undo.RegisterCreatedObjectUndo( newGameObject, "Create Icon Image Object" );

						StoreChildTransforms();
					}
					EditorGUI.EndDisabledGroup();
				}
				else
				{
					EditorGUI.BeginChangeCheck();
					iconColor = EditorGUILayout.ColorField( new GUIContent( "Image Color", "The color of the icon image." ), iconColor );
					if( EditorGUI.EndChangeCheck() && targ.buttonIcon != null )
					{
						Undo.RecordObject( targ.buttonIcon, "Update Cooldown Color" );
						targ.buttonIcon.enabled = false;
						targ.buttonIcon.color = iconColor;
						targ.buttonIcon.enabled = true;
					}

					EditorGUI.BeginChangeCheck();
					iconScale = EditorGUILayout.Slider( new GUIContent( "Icon Scale", "The scale of the icon for the button." ), iconScale, 0.0f, 2.0f );
					if( EditorGUI.EndChangeCheck() )
					{
						Undo.RecordObject( targ.buttonIcon.rectTransform, "Change Icon Scale" );
						targ.buttonIcon.rectTransform.localScale = Vector3.one * iconScale;
					}
					
					DisplayDivider();

					EditorGUI.BeginChangeCheck();
					useIconMask = EditorGUILayout.Toggle( new GUIContent( "Use Icon Mask", "Determines if the icon should be placed inside a mask image or not." ), useIconMask );
					if( EditorGUI.EndChangeCheck() )
					{
						if( !useIconMask && buttonIconMask != null )
						{
							Undo.SetTransformParent( targ.buttonIcon.transform, targ.buttonBase.transform, "Disable Icon" );
							Undo.DestroyObjectImmediate( buttonIconMask.gameObject );
							useIconMask = false;
							ConfigureIconParent();
							StoreChildTransforms();
						}
					}
					
					if( useIconMask )
					{
						EditorGUI.BeginChangeCheck();
						buttonIconMask = ( Image )EditorGUILayout.ObjectField( new GUIContent( "Icon Mask", "The icon mask image to be used for the button icon." ), buttonIconMask, typeof( Image ), true );
						if( EditorGUI.EndChangeCheck() )
						{
							serializedObject.ApplyModifiedProperties();

							if( buttonIconMask != null )
								iconMaskSprite = buttonIconMask.sprite;

							ConfigureIconParent();
						}

						EditorGUI.BeginChangeCheck();
						iconMaskSprite = ( Sprite )EditorGUILayout.ObjectField( "└ Image Sprite", iconMaskSprite, typeof( Sprite ), true, GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
						if( EditorGUI.EndChangeCheck() && buttonIconMask != null )
						{
							Undo.RecordObject( buttonIconMask, "Update Icon Mask Sprite" );
							buttonIconMask.sprite = iconMaskSprite;
						}

						if( buttonIconMask == null )
						{
							EditorGUI.BeginDisabledGroup( iconMaskSprite == null || isPrefabInProjectWindow );
							if( GUILayout.Button( "Generate Mask Image", EditorStyles.miniButton ) )
							{
								GameObject newGameObject = new GameObject();
								RectTransform trans = newGameObject.AddComponent<RectTransform>();
								newGameObject.AddComponent<CanvasRenderer>();
								Image imageComponent = newGameObject.AddComponent<Image>();
								imageComponent.sprite = iconMaskSprite;
								Mask maskComponent = newGameObject.AddComponent<Mask>();
								maskComponent.showMaskGraphic = false;

								newGameObject.transform.SetParent( targ.buttonBase.transform );
								newGameObject.transform.SetAsFirstSibling();

								trans.anchorMin = new Vector2( 0.0f, 0.0f );
								trans.anchorMax = new Vector2( 1.0f, 1.0f );
								trans.offsetMin = Vector2.zero;
								trans.offsetMax = Vector2.zero;
								trans.localScale = Vector3.one;
								trans.localPosition = Vector3.zero;
								trans.localRotation = Quaternion.identity;

								newGameObject.name = "Button Icon Mask";

								buttonIconMask = imageComponent;

								Undo.RegisterCreatedObjectUndo( newGameObject, "Create Icon Mask Image Object" );
								ConfigureIconParent();
								StoreChildTransforms();
							}
							EditorGUI.EndDisabledGroup();
						}
					}
				}
				GUILayout.Space( 1 );
			}
			EditorGUILayout.EndVertical();
			if( valueChanged )
			{
				if( !targ.useIcon && targ.buttonIcon != null && useIconMask && buttonIconMask != null )
				{
					Undo.SetTransformParent( targ.buttonIcon.transform, targ.buttonBase.transform, "Disable Icon" );
					Undo.DestroyObjectImmediate( buttonIconMask.gameObject );
					useIconMask = false;
				}

				if( targ.buttonIcon != null )
				{
					Undo.RecordObject( targ.buttonIcon.gameObject, ( targ.useIcon ? "Enable " : "Disable " ) + "Button Icon" );
					targ.buttonIcon.gameObject.SetActive( targ.useIcon );
				}
				
				ConfigureIconParent();
				StoreChildTransforms();
			}
			// ---------------------------- END ICON SETTINGS -------------------------- //

			// ---------------------------- COOLDOWN SETTINGS ---------------------------- //
			valueChanged = false;
			EditorGUILayout.BeginVertical( "Box" );
			if( DisplayCollapsibleBoxSection( "Cooldown", "UB_CooldownSettings", useCooldown, ref valueChanged ) )
			{
				EditorGUI.BeginChangeCheck();
				cooldownTestValue = EditorGUILayout.Slider( "Cooldown Test", cooldownTestValue, 0.0f, cooldownTimeMax );
				if( EditorGUI.EndChangeCheck() )
				{
					Undo.RecordObject( targ.buttonCooldown, "Cooldown Test" );

					if( targ.cooldownText != null )
						Undo.RecordObject( targ.cooldownText, "Cooldown Test" );

					targ.buttonCooldown.enabled = false;
					targ.UpdateCooldown( cooldownTestValue, cooldownTimeMax );
					targ.buttonCooldown.enabled = true;

					if( targ.cooldownText != null && cooldownTestValue == 0.0f )
					{
						targ.cooldownText.rectTransform.localScale = Vector3.one;
						targ.cooldownText.text = "00";
					}
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( buttonCooldown, new GUIContent( "Cooldown Image", "The image component to be used for the button cooldown." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					if( buttonCooldown != null )
					{
						cooldownSprite = targ.buttonCooldown.sprite;
						cooldownColor = targ.buttonCooldown.color;
						fillMethod = targ.buttonCooldown.fillMethod;
					}
				}

				EditorGUI.BeginChangeCheck();
				cooldownSprite = ( Sprite )EditorGUILayout.ObjectField( "└ Cooldown Sprite", cooldownSprite, typeof( Sprite ), true, GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
				if( EditorGUI.EndChangeCheck() && targ.buttonCooldown != null )
				{
					Undo.RecordObject( targ.buttonCooldown, "Update Cooldown Sprite" );
					targ.buttonCooldown.sprite = cooldownSprite;
				}

				if( targ.buttonCooldown == null )
				{
					EditorGUI.BeginDisabledGroup( cooldownSprite == null || isPrefabInProjectWindow );
					if( GUILayout.Button( "Generate Cooldown Image", EditorStyles.miniButton ) )
					{
						GameObject newGameObject = new GameObject();
						RectTransform trans = newGameObject.AddComponent<RectTransform>();
						newGameObject.AddComponent<CanvasRenderer>();
						Image imageComponent = newGameObject.AddComponent<Image>();

						imageComponent.color = cooldownColor;
						if( cooldownSprite != null )
							imageComponent.sprite = cooldownSprite;

						imageComponent.type = Image.Type.Filled;
						imageComponent.fillMethod = fillMethod;
						imageComponent.fillAmount = cooldownTestValue;

						newGameObject.transform.SetParent( targ.buttonBase.transform );
						newGameObject.transform.SetAsLastSibling();

						trans.anchorMin = new Vector2( 0.0f, 0.0f );
						trans.anchorMax = new Vector2( 1.0f, 1.0f );
						trans.offsetMin = Vector2.zero;
						trans.offsetMax = Vector2.zero;
						trans.localScale = Vector3.one;
						trans.localPosition = Vector3.zero;
						trans.localRotation = Quaternion.identity;

						newGameObject.name = "Cooldown Image";

						serializedObject.FindProperty( "buttonCooldown" ).objectReferenceValue = imageComponent;
						serializedObject.ApplyModifiedProperties();

						Undo.RegisterCreatedObjectUndo( newGameObject, "Create Cooldown Image Object" );

						StoreChildTransforms();
					}
					EditorGUI.EndDisabledGroup();
				}
				else
				{
					EditorGUI.BeginChangeCheck();
					cooldownColor = EditorGUILayout.ColorField( new GUIContent( "Image Color", "The color of the cooldown image." ), cooldownColor );
					if( EditorGUI.EndChangeCheck() && targ.buttonCooldown != null )
					{
						Undo.RecordObject( targ.buttonCooldown, "Update Cooldown Color" );
						targ.buttonCooldown.color = cooldownColor;
					}

					EditorGUI.BeginChangeCheck();
					fillMethod = ( Image.FillMethod )EditorGUILayout.EnumPopup( "Fill Method", fillMethod );
					if( EditorGUI.EndChangeCheck() && targ.buttonCooldown != null )
					{
						Undo.RecordObject( targ.buttonCooldown, "Change Fill Method" );
						targ.buttonCooldown.enabled = false;
						targ.buttonCooldown.fillMethod = fillMethod;
						targ.buttonCooldown.enabled = true;
					}

					DisplayDivider();

					// ------------------------------ COOLDOWN TEXT SETTINGS ------------------------------ //
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( useCooldownText, new GUIContent( "Use Cooldown Text", "Determines if the buttons should display cooldown text." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						serializedObject.ApplyModifiedProperties();

						if( targ.cooldownText != null )
						{
							Undo.RecordObject( targ.cooldownText.gameObject, "Use Cooldown Text" );
							targ.cooldownText.gameObject.SetActive( targ.useCooldownText );
						}
					}

					if( targ.useCooldownText )
					{
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField( cooldownText, new GUIContent( "Cooldown Text", "The text component to be used for the button cooldown." ) );
						if( EditorGUI.EndChangeCheck() )
						{
							serializedObject.ApplyModifiedProperties();

							if( targ.cooldownText != null )
							{
								textAnchorMod = targ.cooldownText.rectTransform.anchorMax.y;
								textColor = targ.cooldownText.color;

								if( targ.cooldownText.GetComponent<Outline>() )
									textOutlineColor = targ.cooldownText.GetComponent<Outline>().effectColor;
							}
						}

						if( targ.useCooldownText && targ.cooldownText == null )
						{
							EditorGUI.BeginDisabledGroup( isPrefabInProjectWindow );
							if( GUILayout.Button( "Generate Cooldown Text", EditorStyles.miniButton ) )
							{
								GameObject newTextObject = new GameObject();
								RectTransform trans = newTextObject.AddComponent<RectTransform>();
								newTextObject.AddComponent<CanvasRenderer>();
								Text textComponent = newTextObject.AddComponent<Text>();

								newTextObject.transform.SetParent( targ.buttonCooldown.transform );
								newTextObject.gameObject.name = "Cooldown Text";

								trans.anchorMin = new Vector2( 0.0f, 0.25f );
								trans.anchorMax = new Vector2( 1.0f, 0.75f );
								trans.offsetMin = Vector2.zero;
								trans.offsetMax = Vector2.zero;
								trans.localScale = Vector3.one;
								trans.localPosition = Vector3.zero;
								trans.localRotation = Quaternion.identity;

								textComponent.text = "00";
								textComponent.alignment = TextAnchor.MiddleCenter;
								textComponent.alignByGeometry = true;
								textComponent.resizeTextForBestFit = true;
								textComponent.resizeTextMinSize = 0;
								textComponent.resizeTextMaxSize = 300;
								textComponent.color = textColor;
								textComponent.raycastTarget = false;

								if( textFont != null )
									textComponent.font = textFont;

								serializedObject.FindProperty( "cooldownText" ).objectReferenceValue = textComponent;
								serializedObject.ApplyModifiedProperties();

								Undo.RegisterCreatedObjectUndo( newTextObject, "Create Cooldown Text Object" );

								StoreChildTransforms();
							}
							EditorGUI.EndDisabledGroup();
						}

						EditorGUI.BeginChangeCheck();
						textFont = ( Font )EditorGUILayout.ObjectField( "└ Font", textFont, typeof( Font ), false );
						if( EditorGUI.EndChangeCheck() )
						{
							serializedObject.ApplyModifiedProperties();

							if( textFont != null && targ.cooldownText != null )
							{
								Undo.RecordObject( targ.cooldownText, "Update Text Font" );
								targ.cooldownText.enabled = false;
								targ.cooldownText.font = textFont;
								targ.cooldownText.enabled = true;
							}
						}

						GUILayout.Space( afterIndentSpace );

						EditorGUI.BeginChangeCheck();
						textOutline = EditorGUILayout.Toggle( new GUIContent( "Text Outline", "Determines if the text should have an outline or not." ), textOutline );
						if( EditorGUI.EndChangeCheck() && targ.cooldownText != null )
						{
							if( textOutline && !targ.cooldownText.gameObject.GetComponent<Outline>() )
							{
								Undo.AddComponent( targ.cooldownText.gameObject, typeof( Outline ) );
								targ.cooldownText.gameObject.GetComponent<Outline>().effectColor = textOutlineColor;
							}
							else if( !textOutline && targ.cooldownText.gameObject.GetComponent<Outline>() )
								Undo.DestroyObjectImmediate( targ.cooldownText.gameObject.GetComponent<Outline>() );
						}

						EditorGUI.BeginDisabledGroup( !textOutline );
						EditorGUI.BeginChangeCheck();
						textOutlineColor = EditorGUILayout.ColorField( new GUIContent( "└ Outline Color", "The color to apply to the outline component." ), textOutlineColor );
						if( EditorGUI.EndChangeCheck() && targ.cooldownText != null )
						{
							Undo.RecordObject( targ.cooldownText.gameObject.GetComponent<Outline>(), "Update Outline Color" );
							targ.cooldownText.gameObject.GetComponent<Outline>().effectColor = textOutlineColor;
						}
						EditorGUI.EndDisabledGroup();

						GUILayout.Space( afterIndentSpace );

						EditorGUI.BeginChangeCheck();
						textAnchorMod = EditorGUILayout.Slider( new GUIContent( "Text Size", "The size of the cooldown text." ), textAnchorMod, 0.0f, 1.0f );
						if( EditorGUI.EndChangeCheck() && targ.cooldownText != null )
						{
							Undo.RecordObject( targ.cooldownText.rectTransform, "Update Cooldown Text Anchor" );
							targ.cooldownText.rectTransform.anchorMin = new Vector2( 0.0f, Mathf.Lerp( 0.5f, 0.0f, textAnchorMod ) );
							targ.cooldownText.rectTransform.anchorMax = new Vector2( 1.0f, ( 1.0f - Mathf.Lerp( 0.5f, 0.0f, textAnchorMod ) ) );
							targ.cooldownText.rectTransform.offsetMin = Vector2.zero;
							targ.cooldownText.rectTransform.offsetMax = Vector2.zero;
							PropertyUpdated( DisplayCooldownTextAnchor );
						}
						CheckPropertyHover( DisplayCooldownTextAnchor );

						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField( displayDecimalCooldown, new GUIContent( "Display Decimal", "Determines if the cooldown time should display the decimal point." ) );
						EditorGUILayout.PropertyField( cooldownTextScaleCurve, new GUIContent( "Text Scale Curve", "The scale curve of the text while processing the cooldown." ) );
						if( EditorGUI.EndChangeCheck() )
						{
							serializedObject.ApplyModifiedProperties();

							Undo.RecordObject( targ.buttonCooldown, "Cooldown Test" );

							if( targ.cooldownText != null )
								Undo.RecordObject( targ.cooldownText, "Cooldown Text Test" );

							targ.UpdateCooldown( cooldownTestValue, cooldownTimeMax );

							if( targ.cooldownText != null && cooldownTestValue == 0.0f )
							{
								targ.cooldownText.rectTransform.localScale = Vector3.one;
								targ.cooldownText.text = "00";
							}
						}
					}
				}
				GUILayout.Space( 1 );
				// ------------------------------ END COOLDOWN TEXT SETTINGS ------------------------------ //
			}
			EditorGUILayout.EndVertical();
			if( valueChanged )
			{
				if( targ.buttonCooldown != null )
				{
					Undo.RecordObject( targ.buttonCooldown.gameObject, ( targ.useCooldown ? "Enable " : "Disable " ) + "Button Cooldown" );
					targ.buttonCooldown.gameObject.SetActive( targ.useCooldown );
				}

				if( targ.useCooldownText && targ.cooldownText != null )
				{
					Undo.RecordObject( targ.cooldownText.gameObject, ( targ.useCooldown ? "Enable " : "Disable " ) + "Button Cooldown" );
					targ.cooldownText.gameObject.SetActive( targ.useCooldown );
				}

				StoreChildTransforms();
			}
			// -------------------------- END COOLDOWN SETTINGS -------------------------- //

			// ------------------------------ CHILD HEIRARCHY ------------------------------ //
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( "Reorder Child Hierarchy" );
			EditorPrefs.SetBool( "UB_ChildHierarchy", GUILayout.Toggle( EditorPrefs.GetBool( "UB_ChildHierarchy" ), EditorPrefs.GetBool( "UB_ChildHierarchy" ) ? "-" : "+", EditorStyles.miniButton, GUILayout.Width( 20 ) ) );
			EditorGUILayout.EndHorizontal();
			if( EditorPrefs.GetBool( "UB_ChildHierarchy" ) )
				childObjects.DoLayoutList();
			// ------------------------------ END CHILD HEIRARCHY ------------------------------ //
		}

		// SCRIPT REFERENCE //
		DisplayHeaderDropdown( "Script Reference", "UB_ScriptReference" );
		if( EditorPrefs.GetBool( "UB_ScriptReference" ) )
		{
			#if ENABLE_INPUT_SYSTEM
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( serializedObject.FindProperty( "_controlPath" ) );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();
			#endif

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( buttonName );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();

			if( targ.buttonName == string.Empty )
				EditorGUILayout.HelpBox( "Please assign a Button Name in order to be able to get this button's input data.", MessageType.Warning );
			else
			{
				EditorGUILayout.BeginVertical( "Box" );
				GUILayout.Space( 1 );
				EditorGUILayout.LabelField( "Example Code Generator", EditorStyles.boldLabel );

				exampleCodeIndex = EditorGUILayout.Popup( "Function", exampleCodeIndex, exampleCodeOptions.ToArray() );

				EditorGUILayout.LabelField( "Function Description", EditorStyles.boldLabel );
				GUIStyle wordWrappedLabel = new GUIStyle( GUI.skin.label ) { wordWrap = true };
				EditorGUILayout.LabelField( exampleCodes[ exampleCodeIndex ].optionDescription, wordWrappedLabel );

				if( exampleCodeIndex == 3 && targ.tapCountOption == UltimateButton.TapCountOption.NoCount )
					EditorGUILayout.HelpBox( "Tap Count is not being used. Please set the Tap Count Option in order to use this option.", MessageType.Warning );

				EditorGUILayout.LabelField( "Example Code", EditorStyles.boldLabel );
				GUIStyle wordWrappedTextArea = new GUIStyle( GUI.skin.textArea ) { wordWrap = true };
				EditorGUILayout.TextArea( string.Format( exampleCodes[ exampleCodeIndex ].basicCode, buttonName.stringValue ), wordWrappedTextArea );

				GUILayout.Space( 1 );
				EditorGUILayout.EndVertical();
			}

			if( GUILayout.Button( "Open Documentation" ) )
				UltimateButtonReadmeEditor.OpenReadmeDocumentation();
		}

		// BUTTON EVENTS //
		DisplayHeaderDropdown( "Button Events", "UB_ButtonEvents" );
		if( EditorPrefs.GetBool( "UB_ButtonEvents" ) )
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( onButtonDown );
			EditorGUILayout.PropertyField( onButtonUp );

			if( targ.tapCountOption != UltimateButton.TapCountOption.NoCount )
				EditorGUILayout.PropertyField( tapCountEvent );

			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();
		}

		// DEVELOPMENT MODE //
		if( EditorPrefs.GetBool( "UUI_DevelopmentMode" ) )
		{
			EditorGUILayout.Space();
			GUIStyle toolbarStyle = new GUIStyle( EditorStyles.toolbarButton ) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 11, richText = true };
			GUILayout.BeginHorizontal();
			GUILayout.Space( -10 );
			showDefaultInspector = GUILayout.Toggle( showDefaultInspector, ( showDefaultInspector ? "▼ " : "► " ) + "<color=#ff0000ff>Development Inspector</color>", toolbarStyle );
			GUILayout.EndHorizontal();
			if( showDefaultInspector )
			{
				EditorGUILayout.Space();

				base.OnInspectorGUI();
			}
		}

		EditorGUILayout.Space();

		Repaint();
	}

	void ConfigureIconParent ()
	{
		if( !targ.useIcon || targ.buttonIcon == null )
			return;

		if( useIconMask && buttonIconMask != null )
		{
			Undo.SetTransformParent( targ.buttonIcon.transform, buttonIconMask.transform, "Enable Icon Mask" );
			buttonIconMask.transform.SetAsFirstSibling();
		}
		else
		{
			Undo.SetTransformParent( targ.buttonIcon.transform, targ.buttonBase.transform, "Disable Icon Mask" );
			targ.buttonIcon.transform.SetAsFirstSibling();
		}
	}

	void DisplayDivider ()
	{
		EditorGUI.BeginDisabledGroup( true );
		EditorGUILayout.LabelField( "────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────", collapsableSectionStyle );
		EditorGUI.EndDisabledGroup();
	}

	bool RelativeTransformCanvasError ()
	{
		if( Selection.activeGameObject == null )
			return false;

		if( targ.relativeTransform == null )
			return false;

		Transform parent = targ.relativeTransform.parent;

		while( parent != null )
		{
			if( parent.transform.GetComponent<Canvas>() && parent.transform.GetComponent<Canvas>().enabled )
			{
				if( parent.transform.GetComponent<Canvas>() == parentCanvas )
					return false;

				return true;
			}

			parent = parent.transform.parent;
		}
		
		return true;
	}

	// ---------------------------------< SCENE GIZMOS >--------------------------------- //
	void PropertyUpdated ( DisplaySceneGizmo sceneGizmo )
	{
		sceneGizmo.frames = 0;
	}

	void CheckPropertyHover ( DisplaySceneGizmo sceneGizmo )
	{
		sceneGizmo.hover = false;
		var rect = GUILayoutUtility.GetLastRect();
		if( Event.current.type == EventType.Repaint && rect.Contains( Event.current.mousePosition ) )
			sceneGizmo.hover = true;
	}

	bool DisplayGizmo ( DisplaySceneGizmo sceneGizmo )
	{
		Handles.color = colorDefault;
		if( sceneGizmo.frames < maxFrames )
			Handles.color = colorValueChanged;

		return sceneGizmo.hover || sceneGizmo.frames < maxFrames;
	}
	
	void OnSceneGUI ()
	{
		if( targ == null || Selection.activeGameObject == null || Application.isPlaying || Selection.objects.Length > 1 || parentCanvas == null )
			return;

		if( targ.buttonBase == null )
			return;

		Vector3 canvasScale = parentCanvas.transform.localScale;

		RectTransform trans = targ.transform.GetComponent<RectTransform>();
		Vector3 transCenter = trans.position;
		Vector3 joystickCenter = targ.buttonBase.rectTransform.position;
		float actualJoystickSize = targ.buttonBase.rectTransform.sizeDelta.x * canvasScale.x;
		float halfSize = actualJoystickSize / 2;

		Handles.color = colorDefault;
		DisplayRelativeSpaceMod.frames++;
		DisplayActivationRange.frames++;
		DisplayBoundary.frames++;
		DisplayCooldownTextAnchor.frames++;
		DisplayOrbitRadius.frames++;
		DisplayCenterAngle.frames++;

		if( EditorPrefs.GetBool( "UB_ButtonPositioning" ) )
		{
			if( targ.positioning != UltimateButton.Positioning.Disabled && DisplayGizmo( DisplayActivationRange ) )
			{
				if( targ.boundary == UltimateButton.Boundary.Circular )
					Handles.DrawWireDisc( joystickCenter, targ.transform.forward, halfSize * targ.activationRange );
				else
					DrawWireBox( trans );

				Handles.Label( transCenter + ( -trans.transform.up * ( ( trans.sizeDelta.x / 2 ) * canvasScale.x ) ), "Activation Range: " + targ.activationRange, handlesCenteredText );
			}

			if( targ.positioning == UltimateButton.Positioning.ScreenSpace || ( targ.positioning == UltimateButton.Positioning.RelativeToTransform && !targ.orbitTransform ) )
			{
				DrawWireBox( !targ.useActivationSize ? targ.buttonBase.rectTransform : trans );
			}

			if( targ.positioning == UltimateButton.Positioning.RelativeToTransform && targ.relativeTransform != null )
			{
				if( !targ.orbitTransform )
				{
					Handles.color = Color.white;
					if( DisplayGizmo( DisplayRelativeSpaceMod ) )
						Handles.color = colorValueChanged;

					DrawWireBox( targ.relativeTransform, targ.relativeSpaceMod );
				}
				else
				{
					// Orbit Distance
					Handles.color = colorDefault;
					if( DisplayOrbitRadius.HighlightGizmo )
						Handles.color = colorValueChanged;

					Quaternion rot = Quaternion.AngleAxis( ( ( -targ.centerAngle + ( 360 / 2 ) ) - 90 ) - 360, targ.transform.forward );
					Vector3 lDirection = rot * -targ.transform.right;
					Handles.DrawWireArc( targ.relativeTransform.position, targ.transform.forward, lDirection, 360f, ( targ.relativeTransform.sizeDelta.x * targ.orbitDistance ) * parentCanvas.transform.localScale.x );

					// CENTER ANGLE //
					Handles.color = colorDefault;
					if( DisplayCenterAngle.HighlightGizmo )
						Handles.color = colorValueChanged;

					// Draw the center angle.
					Vector3 lineEnd = targ.relativeTransform.position;
					lineEnd.x -= ( Mathf.Cos( ( -targ.centerAngle - 90 ) * Mathf.Deg2Rad ) * ( targ.relativeTransform.sizeDelta.x * ( targ.orbitDistance ) ) );
					lineEnd.y -= ( Mathf.Sin( ( -targ.centerAngle - 90 ) * Mathf.Deg2Rad ) * ( targ.relativeTransform.sizeDelta.x * ( targ.orbitDistance ) ) );
					Vector3 heading = lineEnd - targ.relativeTransform.position;
					heading = heading / heading.magnitude;
					Handles.DrawLine( targ.relativeTransform.position, targ.relativeTransform.position + ( trans.TransformDirection( heading * targ.relativeTransform.sizeDelta ) * targ.orbitDistance ) * parentCanvas.transform.localScale.x );
				}
			}
		}

		if( EditorPrefs.GetBool( "UB_ButtonOptions" ) )
		{
			if( EditorPrefs.GetBool( "UB_CooldownSettings" ) )
			{
				if( DisplayGizmo( DisplayCooldownTextAnchor ) && targ.useCooldownText && targ.cooldownText != null )
				{
					targ.cooldownText.text = "00";
					targ.cooldownText.rectTransform.localScale = Vector3.one;
					DrawWireBox( targ.cooldownText.rectTransform );
				}
			}
		}
		
		SceneView.RepaintAll();
	}

	void DrawWireBox ( RectTransform trans, float radius = 1.0f )
	{
		Vector2 center = trans.rect.center + ( ( trans.rect.size * radius ) * ( trans.pivot - new Vector2( 0.5f, 0.5f ) ) );
		
		Vector3 topLeft = center + ( new Vector2( trans.rect.xMin, trans.rect.yMax ) * radius );
		Vector3 topRight = center + ( new Vector2( trans.rect.xMax, trans.rect.yMax ) * radius );
		Vector3 bottomLeft = center + ( new Vector2( trans.rect.xMin, trans.rect.yMin ) * radius );
		Vector3 bottomRight = center + ( new Vector2( trans.rect.xMax, trans.rect.yMin ) * radius );

		topLeft = trans.TransformPoint( topLeft );
		topRight = trans.TransformPoint( topRight );
		bottomRight = trans.TransformPoint( bottomRight );
		bottomLeft = trans.TransformPoint( bottomLeft );

		Handles.DrawLine( topLeft, topRight );
		Handles.DrawLine( topRight, bottomRight );
		Handles.DrawLine( bottomRight, bottomLeft );
		Handles.DrawLine( bottomLeft, topLeft );
	}
	// ---------------------------------< SCENE GIZMOS >--------------------------------- //

	// ---------------------------------< CANVAS CREATOR FUNCTIONS >--------------------------------- //
	static void CreateNewCanvas ( GameObject child )
	{
		GameObject canvasObject = new GameObject( "Ultimate UI Canvas" );
		canvasObject.layer = LayerMask.NameToLayer( "UI" );
		Canvas canvas = canvasObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvasObject.AddComponent<GraphicRaycaster>();
		canvasObject.AddComponent<CanvasScaler>();
		Undo.RegisterCreatedObjectUndo( canvasObject, "Create " + canvasObject.name );
		Undo.SetTransformParent( child.transform, canvasObject.transform, "Request Canvas" );
		CreateEventSystem();
	}

	static void CreateEventSystem ()
	{
		Object esys = Object.FindObjectOfType<EventSystem>();
		if( esys == null )
		{
			GameObject eventSystem = new GameObject( "EventSystem" );
			esys = eventSystem.AddComponent<EventSystem>();
			eventSystem.AddComponent<StandaloneInputModule>();
			Undo.RegisterCreatedObjectUndo( eventSystem, "Create " + eventSystem.name );
		}
	}

	public static void RequestCanvas ( GameObject child )
	{
		Canvas[] allCanvas = Object.FindObjectsOfType( typeof( Canvas ) ) as Canvas[];

		for( int i = 0; i < allCanvas.Length; i++ )
		{
			if( allCanvas[ i ].enabled == true && allCanvas[ i ].renderMode != RenderMode.WorldSpace )
			{
				Undo.SetTransformParent( child.transform, allCanvas[ i ].transform, "Request Canvas" );
				CreateEventSystem();
				return;
			}
		}
		CreateNewCanvas( child );
	}
	// -------------------------------< END CANVAS CREATOR FUNCTIONS >------------------------------- //
}