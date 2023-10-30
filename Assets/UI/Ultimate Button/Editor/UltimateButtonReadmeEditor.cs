/* Written by Kaz Crowe */
/* UltimateButtonReadmeEditor.cs */
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
[CustomEditor( typeof( UltimateButtonReadme ) )]
public class UltimateButtonReadmeEditor : Editor
{
	static UltimateButtonReadme readme;

	// LAYOUT STYLES //
	string Indent
	{
		get
		{
			return "    ";
		}
	}
	int sectionSpace = 20;
	int itemHeaderSpace = 10;
	int paragraphSpace = 5;
	GUIStyle titleStyle = new GUIStyle();
	GUIStyle sectionHeaderStyle = new GUIStyle();
	GUIStyle itemHeaderStyle = new GUIStyle();
	GUIStyle paragraphStyle = new GUIStyle();
	GUIStyle versionStyle = new GUIStyle();

	class PageInformation
	{
		public string pageName = "";
		public delegate void TargetMethod ();
		public TargetMethod targetMethod;
	}
	static List<PageInformation> pageHistory = new List<PageInformation>();
	static PageInformation[] AllPages = new PageInformation[]
	{
		// MAIN MENU - 0 //
		new PageInformation()
		{
			pageName = "Product Manual"
		},
		// Getting Started - 1 //
		new PageInformation()
		{
			pageName = "Getting Started"
		},
		// Overview - 2 //
		new PageInformation()
		{
			pageName = "Overview"
		},
		// Documentation - 3 //
		new PageInformation()
		{
			pageName = "Documentation"
		},
		// Version History - 4 //
		new PageInformation()
		{
			pageName = "Version History"
		},
		// Important Change - 5 //
		new PageInformation()
		{
			pageName = "Important Change"
		},
		// Thank You! - 6 //
		new PageInformation()
		{
			pageName = "Thank You!"
		},
		// Settings - 7 //
		new PageInformation()
		{
			pageName = "Settings"
		},
	};

	class EndPageComment
	{
		public string comment = "";
		public string url = "";
	}
	EndPageComment[] endPageComments = new EndPageComment[]
	{
		new EndPageComment()
		{
			comment = "Enjoying the Ultimate Button? Leave us a review on the <b><color=blue>Unity Asset Store</color></b>!",
			url = "https://assetstore.unity.com/packages/slug/28824"
		},
		new EndPageComment()
		{
			comment = "Looking for a radial menu for your game? Check out the <b><color=blue>Ultimate Radial Menu</color></b>!",
			url = "https://www.tankandhealerstudio.com/ultimate-radial-menu.html"
		},
		new EndPageComment()
		{
			comment = "Looking for a health bar for your game? Check out the <b><color=blue>Simple Health Bar FREE</color></b>!",
			url = "https://www.tankandhealerstudio.com/simple-health-bar-free.html"
		},
		new EndPageComment()
		{
			comment = "Check out our <b><color=blue>other products</color></b>!",
			url = "https://www.tankandhealerstudio.com/assets.html"
		},
	};
	int randomComment = 0;

	// OVERVIEW //
	bool showButtonPositioning, showInputSettings, showButtonOptions, showScriptReference, showButtonEvents;

	class DocumentationInfo
	{
		public string functionName = "";
		public bool targetShowMore = false;
		public bool showMore = false;
		public string[] parameter;
		public string returnType = "";
		public string description = "";
		public string codeExample = "";
	}
	DocumentationInfo[] StaticFunctions = new DocumentationInfo[]
	{
		// GetUltimateButton
		new DocumentationInfo
		{
			functionName = "GetUltimateButton()",
			parameter = new string[ 1 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with."
			},
			returnType = "UltimateButton",
			description = "Returns the Ultimate Button registered with the buttonName string. This function can be used to call local functions on the Ultimate Button to apply color changes or position updates at runtime.",
			codeExample = "UltimateButton jumpButton = UltimateButton.GetUltimateButton( \"Jump\" );"
		},
		// GetButtonDown
		new DocumentationInfo
		{
			functionName = "GetButtonDown()",
			parameter = new string[ 1 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with."
			},
			description = "Returns true on the frame that the targeted Ultimate Button is pressed down.",
			codeExample = "if( UltimateButton.GetButtonDown( \"Jump\" ) )\n{\n    Debug.Log( \"The user has touched down on the jump button!\" );\n}"
		},
		// GetButtonUp
		new DocumentationInfo
		{
			functionName = "GetButtonUp()",
			parameter = new string[ 1 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with."
			},
			description = "Returns true on the frame that the targeted Ultimate Button is released.",
			codeExample = "if( UltimateButton.GetButtonUp( \"Jump\" ) )\n{\n    Debug.Log( \"The user has released the touch on the jump button!\" );\n}"
		},
		// GetButton
		new DocumentationInfo
		{
			functionName = "GetButton()",
			parameter = new string[ 1 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with."
			},
			description = "Returns true on the frames that the targeted Ultimate Button is being interacted with.",
			codeExample = "if( UltimateButton.GetButton( \"Jump\" ) )\n{\n    Debug.Log( \"The user is touching the jump button!\" );\n}"
		},
		// GetTapCount
		new DocumentationInfo
		{
			functionName = "GetTapCount()",
			parameter = new string[ 1 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with."
			},
			description = "Returns true on the frame that the targeted Ultimate Button has achieved the tap count.",
			codeExample = "if( UltimateButton.GetTapCount( \"Jump\" ) )\n{\n    Debug.Log( \"The user has double tapped the jump button!\" );\n}"
		},
		// DisableButton
		new DocumentationInfo
		{
			functionName = "DisableButton()",
			parameter = new string[ 1 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with."
			},
			description = "This function will reset the Ultimate Button and disable the gameObject. Use this function when wanting to disable the Ultimate Button from being used.",
			codeExample = "UltimateButton.DisableButton( \"Jump\" );"
		},
		// EnableButton
		new DocumentationInfo
		{
			functionName = "EnableButton()",
			parameter = new string[ 1 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with."
			},
			description = "This function will ensure that the Ultimate Button is completely reset before enabling itself to be used again.",
			codeExample = "UltimateButton.EnableButton( \"Jump\" );"
		},
		// UpdateIcon 
		new DocumentationInfo
		{
			functionName = "UpdateIcon()",
			parameter = new string[ 2 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with.",
				"Sprite newIcon - The new sprite to be applied to the button icon."
			},
			description = "This function will update the icon of the button with the new sprite that is provided.",
			codeExample = "UltimateButton.UpdateIcon(  \"Skill01\", newIconSprite );"
		},
		// UpdateCooldown
		new DocumentationInfo
		{
			functionName = "UpdateCooldown()",
			parameter = new string[ 3 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with.",
				"float currentCooldown - The current value of the cooldown.",
				"float maxCooldown - The max value of the cooldown."
			},
			description = "This function will update the cooldown of the button with the provided information.",
			codeExample = "UltimateButton.UpdateCooldown(  \"Skill01\", currentCooldown, maxCooldown );"
		},
		// ResetCooldown
		new DocumentationInfo
		{
			functionName = "ResetCooldown()",
			parameter = new string[ 1 ]
			{
				"string buttonName - The name that the targeted Ultimate Button has been registered with.",
			},
			description = "This function will reset the cooldown.",
			codeExample = "UltimateButton.ResetCooldown( \"Skill01\" );"
		},
	};
	DocumentationInfo[] PublicFunctions = new DocumentationInfo[]
	{
		// UpdatePositioning
		new DocumentationInfo
		{
			functionName = "UpdatePositioning()",
			description = "Updates the size and positioning of the Ultimate Button. This function can be used to update any options that may have been changed prior to Start().",
			codeExample = "button.buttonSize = 4.0f;\nbutton.UpdatePositioning();"
		},
		// GetButtonDown
		new DocumentationInfo
		{
			functionName = "GetButtonDown()",
			description = "Returns true on the frame that the Ultimate Button is pressed down.",
			codeExample = "if( button.GetButtonDown( \"Jump\" ) )\n{\n    Debug.Log( \"The user has touched down on the jump button!\" );\n}"
		},
		// GetButton
		new DocumentationInfo
		{
			functionName = "GetButton()",
			description = "Returns true on the frames that the Ultimate Button is being interacted with.",
			codeExample = "if( button.GetButton( \"Jump\" ) )\n{\n    Debug.Log( \"The user is touching the jump button!\" );\n}"
		},
		// GetButtonUp
		new DocumentationInfo
		{
			functionName = "GetButtonUp()",
			description = "Returns true on the frame that the targeted Ultimate Button is released.",
			codeExample = "if( button.GetButtonUp( \"Jump\" ) )\n{\n    Debug.Log( \"The user has released the touch on the jump button!\" );\n}"
		},
		// GetTapCount
		new DocumentationInfo
		{
			functionName = "GetTapCount()",
			description = "Returns true when the Tap Count option has been achieved.",
			codeExample = "if( button.GetTapCount( \"Jump\" ) )\n{\n    Debug.Log( \"The user has double tapped the jump button!\" );\n}"
		},
		// DisableButton
		new DocumentationInfo 
		{
			functionName = "DisableButton()",
			description = "This function will reset the Ultimate Button and disable the gameObject. Use this function when wanting to disable the Ultimate Button from being used.",
			codeExample = "button.DisableButton();"
		},
		// EnableButton
		new DocumentationInfo
		{
			functionName = "EnableButton()",
			description = "This function will ensure that the Ultimate Button is completely reset before enabling itself to be used again.",
			codeExample = "button.EnableButton();"
		},
		// UpdateIcon 
		new DocumentationInfo
		{
			functionName = "UpdateIcon()",
			parameter = new string[ 1 ]
			{
				"Sprite newIcon - The new sprite to be applied to the button icon."
			},
			description = "This function will update the icon of the button with the new sprite that is provided.",
			codeExample = "button.UpdateIcon( newIconSprite );"
		},
		// UpdateCooldown
		new DocumentationInfo
		{
			functionName = "UpdateCooldown()",
			parameter = new string[ 2 ]
			{
				"float currentCooldown - The current value of the cooldown.",
				"float maxCooldown - The max value of the cooldown."
			},
			description = "This function will update the cooldown of the button with the provided information.",
			codeExample = "button.UpdateCooldown( currentCooldown, maxCooldown );"
		},
		// ResetCooldown
		new DocumentationInfo
		{
			functionName = "ResetCooldown()",
			description = "This function will reset the cooldown.",
			codeExample = "button.ResetCooldown();"
		},
	};


	static UltimateButtonReadmeEditor ()
	{
		EditorApplication.update += WaitForCompile;
	}

	static void WaitForCompile ()
	{
		if( EditorApplication.isCompiling )
			return;

		EditorApplication.update -= WaitForCompile;
		
		if( !EditorPrefs.HasKey( "UltimateButtonVersion" ) )
		{
			EditorPrefs.SetInt( "UltimateButtonVersion", UltimateButtonReadme.ImportantChange );
			SelectReadmeFile();
			NavigateForward( 6 );
		}
		else if( EditorPrefs.GetInt( "UltimateButtonVersion" ) < UltimateButtonReadme.ImportantChange )
		{
			EditorPrefs.SetInt( "UltimateButtonVersion", UltimateButtonReadme.ImportantChange );
			SelectReadmeFile();
			NavigateForward( 5 );
		}
	}

	void OnEnable ()
	{
		readme = ( UltimateButtonReadme )target;

		if( !EditorPrefs.HasKey( "UB_ColorHexSetup" ) )
		{
			EditorPrefs.SetBool( "UB_ColorHexSetup", true );
			ResetColors();
		}

		ColorUtility.TryParseHtmlString( EditorPrefs.GetString( "UB_ColorDefaultHex" ), out readme.colorDefault );
		ColorUtility.TryParseHtmlString( EditorPrefs.GetString( "UB_ColorValueChangedHex" ), out readme.colorValueChanged );

		AllPages[ 0 ].targetMethod = MainPage;
		AllPages[ 1 ].targetMethod = GettingStarted;
		AllPages[ 2 ].targetMethod = Overview;
		AllPages[ 3 ].targetMethod = Documentation;
		AllPages[ 4 ].targetMethod = VersionHistory;
		AllPages[ 5 ].targetMethod = ImportantChange;
		AllPages[ 6 ].targetMethod = ThankYou;
		AllPages[ 7 ].targetMethod = Settings;

		pageHistory = new List<PageInformation>();
		for( int i = 0; i < readme.pageHistory.Count; i++ )
			pageHistory.Add( AllPages[ readme.pageHistory[ i ] ] );

		if( !pageHistory.Contains( AllPages[ 0 ] ) )
		{
			pageHistory.Insert( 0, AllPages[ 0 ] );
			readme.pageHistory.Insert( 0, 0 );
		}

		randomComment = Random.Range( 0, endPageComments.Length );

		Undo.undoRedoPerformed += UndoRedoCallback;
	}

	void OnDisable ()
	{
		Undo.undoRedoPerformed -= UndoRedoCallback;
	}

	void UndoRedoCallback ()
	{
		if( pageHistory[ pageHistory.Count - 1 ] != AllPages[ 7 ] )
			return;

		EditorPrefs.SetString( "UB_ColorDefaultHex", "#" + ColorUtility.ToHtmlStringRGBA( readme.colorDefault ) );
		EditorPrefs.SetString( "UB_ColorValueChangedHex", "#" + ColorUtility.ToHtmlStringRGBA( readme.colorValueChanged ) );
	}

	protected override void OnHeaderGUI ()
	{
		UltimateButtonReadme readme = ( UltimateButtonReadme )target;
		
		var iconWidth = Mathf.Min( EditorGUIUtility.currentViewWidth, 350f );

		Vector2 ratio = new Vector2( readme.icon.width, readme.icon.height ) / ( readme.icon.width > readme.icon.height ? readme.icon.width : readme.icon.height );

		GUILayout.BeginHorizontal( "In BigTitle" );
		{
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.Label( readme.icon, GUILayout.Width( iconWidth * ratio.x ), GUILayout.Height( iconWidth * ratio.y ) );
			GUILayout.Space( -20 );
			if( GUILayout.Button( readme.versionHistory[ 0 ].versionNumber, versionStyle ) && !pageHistory.Contains( AllPages[ 4 ] ) )
				NavigateForward( 4 );
			var rect = GUILayoutUtility.GetLastRect();
			if( !pageHistory.Contains( AllPages[ 4 ] ) )
				EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndHorizontal();
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update();

		paragraphStyle = new GUIStyle( EditorStyles.label ) { wordWrap = true, richText = true, fontSize = 12 };
		itemHeaderStyle = new GUIStyle( paragraphStyle ) { fontSize = 12, fontStyle = FontStyle.Bold };
		sectionHeaderStyle = new GUIStyle( paragraphStyle ) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
		titleStyle = new GUIStyle( paragraphStyle ) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
		versionStyle = new GUIStyle( paragraphStyle ) { alignment = TextAnchor.MiddleCenter, fontSize = 10 };

		// SETTINGS BUTTON //
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( readme.settings, versionStyle, GUILayout.Width( 24 ), GUILayout.Height( 24 ) ) && !pageHistory.Contains( AllPages[ 7 ] ) )
			NavigateForward( 7 );
		var rect = GUILayoutUtility.GetLastRect();
		if( !pageHistory.Contains( AllPages[ 7 ] ) )
			EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		GUILayout.EndHorizontal();
		GUILayout.Space( -24 );
		GUILayout.EndVertical();

		// BACK BUTTON //
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginDisabledGroup( pageHistory.Count <= 1 );
		if( GUILayout.Button( "◄", titleStyle, GUILayout.Width( 24 ) ) )
			NavigateBack();
		if( pageHistory.Count > 1 )
		{
			rect = GUILayoutUtility.GetLastRect();
			EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		}
		EditorGUI.EndDisabledGroup();
		GUILayout.Space( -24 );

		// PAGE TITLE //
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField( pageHistory[ pageHistory.Count - 1 ].pageName, titleStyle );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		// DISPLAY PAGE //
		if( pageHistory[ pageHistory.Count - 1 ].targetMethod != null )
			pageHistory[ pageHistory.Count - 1 ].targetMethod();

		Repaint();
	}

	void StartPage ()
	{
		readme.scrollValue = EditorGUILayout.BeginScrollView( readme.scrollValue, false, false );
		GUILayout.Space( 15 );
	}

	void EndPage ()
	{
		EditorGUILayout.EndScrollView();
	}

	static void NavigateBack ()
	{
		readme.pageHistory.RemoveAt( readme.pageHistory.Count - 1 );
		pageHistory.RemoveAt( pageHistory.Count - 1 );
		GUI.FocusControl( "" );

		readme.scrollValue = Vector2.zero;
	}

	static void NavigateForward ( int menuIndex )
	{
		pageHistory.Add( AllPages[ menuIndex ] );
		GUI.FocusControl( "" );

		readme.pageHistory.Add( menuIndex );
		readme.scrollValue = Vector2.zero;
	}

	void MainPage ()
	{
		StartPage();

		EditorGUILayout.LabelField( "We hope that you are enjoying using the Ultimate Button in your project!", paragraphStyle );
		EditorGUILayout.Space();
		EditorGUILayout.LabelField( "As with any package, you may be having some trouble understanding how to get the Ultimate Button working in your project. If so, have no fear, Tank & Healer Studio is here! Here is a few things that can help you get started:", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • Read the <b><color=blue>Getting Started</color></b> section of this README!", paragraphStyle );
		var rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			NavigateForward( 1 );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • To learn more about the options on the inspector, read the <b><color=blue>Overview</color></b> section!", paragraphStyle );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			NavigateForward( 2 );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • Check out the <b><color=blue>Documentation</color></b> section!", paragraphStyle );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			NavigateForward( 3 );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • Watch our <b><color=blue>Video Tutorials</color></b> on the Ultimate Button!", paragraphStyle );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
		{
			Debug.Log( "Ultimate Button\nOpening YouTube Tutorials" );
			Application.OpenURL( "https://www.youtube.com/playlist?list=PL7crd9xMJ9Tm14vBil6-DwaL0Ucip_buC" );
		}

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • <b><color=blue>Contact Us</color></b> directly with your issue! We'll try to help you out as much as we can.", paragraphStyle );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
		{
			Debug.Log( "Ultimate Button\nOpening Online Contact Form" );
			Application.OpenURL( "https://www.tankandhealerstudio.com/contact-us.html" );
		}
		
		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "Now you have the tools you need to get the Ultimate Button working in your project. Now get out there and make your awesome game!", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "Happy Game Making,\n" + Indent + "Tank & Healer Studio", paragraphStyle );

		EditorGUILayout.Space();

		GUILayout.FlexibleSpace();

		EditorGUILayout.LabelField( endPageComments[ randomComment ].comment, paragraphStyle );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			Application.OpenURL( endPageComments[ randomComment ].url );

		EndPage();
	}

	void GettingStarted ()
	{
		StartPage();

		EditorGUILayout.LabelField( "How To Create", sectionHeaderStyle );

		EditorGUILayout.LabelField( Indent + "To create an Ultimate Button in your scene, simply find the Ultimate Button prefab that you would like to add and drag it into your scene. What this does is creates that Ultimate Button within the scene and ensures that there is a Canvas and an EventSystem so that it can work correctly. If these are not present in the scene, they will be created for you.", paragraphStyle );

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "How To Reference", sectionHeaderStyle );
		EditorGUILayout.LabelField( Indent + "One of the great things about the Ultimate Button is how easy it is to reference to other scripts. The first thing that you will want to make sure to do is determine how you want to use the Ultimate Button within your scripts. If you are used to using the Events that are used in Unity's default UI buttons, then you may want to use the Unity Events options located within the Button Events section of the Ultimate Button inspector. However, if you are used to using Unity's Input system for getting input, then the Script Reference section would probably suit you better.", paragraphStyle );

		#if ENABLE_INPUT_SYSTEM
		GUILayout.Space( paragraphSpace );
		EditorGUILayout.LabelField( "<b>New Input System:</b> In order to reference the Ultimate Button with the new Input System from Unity, simply go to the Script Reference section of the Ultimate Button in your scene and set the Control Path variable to the desired path.", paragraphStyle );
		#endif

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "For this example, we'll go over how to use the Script Reference section. First thing to do is assign the Button Name within the Script Reference section. After this is complete, you will be able to reference that particular button by it's name from a static function within the Ultimate Button script.", paragraphStyle );

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "Example", sectionHeaderStyle );

		EditorGUILayout.LabelField( Indent + "Let's assume you are going to use the Ultimate Button for making a player jump. You will need to check the button's state to determine when the user has touched the button and is wanting the player to jump. So for this example, let's assign the name \"Jump\" in the Script Reference section of the Ultimate Button.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		Vector2 ratio = new Vector2( readme.scriptReference.width, readme.scriptReference.height ) / ( readme.scriptReference.width > readme.scriptReference.height ? readme.scriptReference.width : readme.scriptReference.height );

		float imageWidth = readme.scriptReference.width > Screen.width - 50 ? Screen.width - 50 : readme.scriptReference.width;

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( readme.scriptReference, GUILayout.Width( imageWidth ), GUILayout.Height( imageWidth * ratio.y ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "There are several functions that allow you to check the different states that the Ultimate Button is in. For more information on all the functions that you have available to you, please see the documentation section of this README.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "For this example we will be using the GetButtonDown function to see if the user has pressed down on the button. It is worth noting that this function is useful when wanting to make the player start the jump action on the exact frame that the user has pressed down on the button, and not after.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Example Code:", itemHeaderStyle );
		EditorGUILayout.TextArea( "if( UltimateButton.GetButtonDown( \"Jump\" ) )\n{\n	// Call player jump function.\n}", UnityEngine.GUI.skin.GetStyle( "TextArea" ) );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Feel free to experiment with the different functions of the Ultimate Button to get it working exactly the way you want to. Additionally, if you are curious about how the Ultimate Button has been implemented into an Official Tank and Healer Studio example, then please see the README_TruckController.txt that is included with the example files for the project.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EndPage();
	}

	void Overview ()
	{
		StartPage();

		EditorGUILayout.LabelField( "Sections", sectionHeaderStyle );
		EditorGUILayout.LabelField( Indent + "The display below is mimicking the Ultimate Button inspector. Expand each section to learn what each one is designed for.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		// BUTTON POSITIONING //
		GUIStyle toolbarStyle = new GUIStyle( EditorStyles.toolbarButton ) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 11 };

		showButtonPositioning = GUILayout.Toggle( showButtonPositioning, ( showButtonPositioning ? "▼" : "►" ) + "Button Positioning", toolbarStyle );
		if( showButtonPositioning )
		{
			GUILayout.Space( paragraphSpace );
			EditorGUILayout.LabelField( "This section handles the positioning of the Ultimate Button on the screen.", paragraphStyle );
		}

		EditorGUILayout.Space();

		// INPUT SETTINGS //
		showInputSettings = GUILayout.Toggle( showInputSettings, ( showInputSettings ? "▼" : "►" ) + "Input Settings", toolbarStyle );
		if( showInputSettings )
		{
			GUILayout.Space( paragraphSpace );
			EditorGUILayout.LabelField( "This section contains various settings that affect input tracking and other important input settings.", paragraphStyle );
		}

		EditorGUILayout.Space();

		// BUTTON OPTIONS //
		showButtonOptions = GUILayout.Toggle( showButtonOptions, ( showButtonOptions ? "▼" : "►" ) + "Button Options", toolbarStyle );
		if( showButtonOptions )
		{
			GUILayout.Space( paragraphSpace );

			EditorGUILayout.LabelField( "The Button Options section allows you to add visual functionality to the Ultimate Button depending on your specific needed use.", paragraphStyle );
		}

		EditorGUILayout.Space();

		// SCRIPT REFERENCE //
		showScriptReference = GUILayout.Toggle( showScriptReference, ( showScriptReference ? "▼" : "►" ) + "Script Reference", toolbarStyle );
		if( showScriptReference )
		{
			GUILayout.Space( paragraphSpace );
			EditorGUILayout.LabelField( "In this section you will be able to setup the reference to this Ultimate Button, and you will be provided with code examples to be able to copy and paste into your own scripts.", paragraphStyle );
		}

		EditorGUILayout.Space();
		
		// BUTTON EVENTS //
		showButtonEvents = GUILayout.Toggle( showButtonEvents, ( showButtonEvents ? "▼" : "►" ) + "Button Events", toolbarStyle );
		if( showButtonEvents )
		{
			GUILayout.Space( paragraphSpace );
			EditorGUILayout.LabelField( "If you just want the Ultimate Button to execute a simple function when interacted with, then this section you will be able to assign these events.", paragraphStyle );
		}

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "Tooltips", sectionHeaderStyle );
		EditorGUILayout.LabelField( Indent + "To learn more about each option in these sections, please select the Ultimate Button in your scene, and hover over each item to read the provided tooltip.", paragraphStyle );

		EndPage();
	}

	void Documentation ()
	{
		StartPage();

		/* //// --------------------------- < STATIC FUNCTIONS > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Static Functions", sectionHeaderStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( Indent + "The following functions can be referenced from your scripts without the need for an assigned local Ultimate Button variable. However, each function must have the targeted Ultimate Button name in order to find the correct Ultimate Button in the scene. Each example code provided uses the name 'Jump' as the button name.", paragraphStyle );

		Vector2 ratio = new Vector2( readme.scriptReference.width, readme.scriptReference.height ) / ( readme.scriptReference.width > readme.scriptReference.height ? readme.scriptReference.width : readme.scriptReference.height );

		float imageWidth = readme.scriptReference.width > Screen.width - 50 ? Screen.width - 50 : readme.scriptReference.width;

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( readme.scriptReference, GUILayout.Width( imageWidth ), GUILayout.Height( imageWidth * ratio.y ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField( "Please click on the function name to learn more.", paragraphStyle );

		for( int i = 0; i < StaticFunctions.Length; i++ )
			ShowDocumentation( StaticFunctions[ i ] );

		GUILayout.Space( sectionSpace );

		/* //// --------------------------- < PUBLIC FUNCTIONS > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Public Functions", sectionHeaderStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( Indent + "All of the following public functions are only available from a reference to the Ultimate Button. Each example provided relies on having a Ultimate Button variable named 'button' stored inside your script. When using any of the example code provided, make sure that you have a public Ultimate Button variable like the one below:", paragraphStyle );

		EditorGUILayout.TextArea( "public UltimateButton button;", UnityEngine.GUI.skin.textArea );

		GUILayout.Space( paragraphSpace );

		for( int i = 0; i < PublicFunctions.Length; i++ )
			ShowDocumentation( PublicFunctions[ i ] );

		GUILayout.Space( itemHeaderSpace );

		EndPage();
	}

	void VersionHistory ()
	{
		StartPage();

		for( int i = 0; i < readme.versionHistory.Length; i++ )
		{
			EditorGUILayout.LabelField( "Version " + readme.versionHistory[ i ].versionNumber, itemHeaderStyle );

			for( int n = 0; n < readme.versionHistory[ i ].changes.Length; n++ )
				EditorGUILayout.LabelField( "  • " + readme.versionHistory[ i ].changes[ n ] + ".", paragraphStyle );

			if( i < ( readme.versionHistory.Length - 1 ) )
				GUILayout.Space( itemHeaderSpace );
		}

		EndPage();
	}

	void ImportantChange ()
	{
		StartPage();

		EditorGUILayout.LabelField( Indent + "Thank you for downloading the most recent version of the Ultimate Button. If you are experiencing any errors, please completely remove the Ultimate Button from your project and re-import it. As always, if you run into any issues with the Ultimate Button, please contact us at:", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		EditorGUILayout.SelectableLabel( "tankandhealerstudio@outlook.com", itemHeaderStyle, GUILayout.Height( 15 ) );
		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "INTERNAL CHANGES", sectionHeaderStyle );
		EditorGUILayout.LabelField( "  There were quite a few internal changes that happened in version 3.0.0, so some of your buttons may behave a little strange at first. All that should be needed to fix them is to select the Ultimate Button game object in your hierarchy. Once you have done this the editor script can fix the old buttons that are using depreciated variables.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Got it!", GUILayout.Width( Screen.width / 2 ) ) )
			NavigateBack();

		var rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EndPage();
	}

	void ThankYou ()
	{
		StartPage();

		EditorGUILayout.LabelField( "The two of us at Tank & Healer Studio would like to thank you for purchasing the Ultimate Button asset package from the Unity Asset Store.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "We hope that the Ultimate Button will be a great help to you in the development of your game. Here is a few things that can help you get started:", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • Read the <b><color=blue>Getting Started</color></b> section of this README!", paragraphStyle );
		var rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			NavigateForward( 1 );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • To learn more about the options on the inspector, read the <b><color=blue>Overview</color></b> section!", paragraphStyle );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			NavigateForward( 2 );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • Check out the <b><color=blue>Documentation</color></b> section to learn more about how to use the Ultimate Button in your scripts!", paragraphStyle );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			NavigateForward( 3 );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "You can access this information at any time by clicking on the <b>README</b> file inside the Ultimate Button folder.", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "Again, thank you for downloading the Ultimate Button. We hope that your project is a success!", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "Happy Game Making,\n" + Indent + "Tank & Healer Studio", paragraphStyle );

		GUILayout.Space( 15 );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Continue", GUILayout.Width( Screen.width / 2 ) ) )
			NavigateBack();

		var rect2 = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect2, MouseCursor.Link );

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EndPage();
	}

	void Settings ()
	{
		StartPage();

		EditorGUILayout.LabelField( "Gizmo Colors", sectionHeaderStyle );
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField( serializedObject.FindProperty( "colorDefault" ), new GUIContent( "Default" ) );
		if( EditorGUI.EndChangeCheck() )
		{
			serializedObject.ApplyModifiedProperties();
			EditorPrefs.SetString( "UB_ColorDefaultHex", "#" + ColorUtility.ToHtmlStringRGBA( readme.colorDefault ) );
		}

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField( serializedObject.FindProperty( "colorValueChanged" ), new GUIContent( "Value Changed" ) );
		if( EditorGUI.EndChangeCheck() )
		{
			serializedObject.ApplyModifiedProperties();
			EditorPrefs.SetString( "UB_ColorValueChangedHex", "#" + ColorUtility.ToHtmlStringRGBA( readme.colorValueChanged ) );
		}

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Reset", GUILayout.Width( EditorGUIUtility.currentViewWidth / 2 ) ) )
		{
			if( EditorUtility.DisplayDialog( "Reset Gizmo Color", "Are you sure that you want to reset the gizmo colors back to default?", "Yes", "No" ) )
				ResetColors();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		if( EditorPrefs.GetBool( "UUI_DevelopmentMode" ) )
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField( "Development Mode", sectionHeaderStyle );
			base.OnInspectorGUI();
			EditorGUILayout.Space();
		}

		GUILayout.FlexibleSpace();

		GUILayout.Space( sectionSpace );

		EditorGUI.BeginChangeCheck();
		GUILayout.Toggle( EditorPrefs.GetBool( "UUI_DevelopmentMode" ), ( EditorPrefs.GetBool( "UUI_DevelopmentMode" ) ? "Disable" : "Enable" ) + " Development Mode", EditorStyles.radioButton );
		if( EditorGUI.EndChangeCheck() )
		{
			if( EditorPrefs.GetBool( "UUI_DevelopmentMode" ) == false )
			{
				if( EditorUtility.DisplayDialog( "Enable Development Mode", "Are you sure you want to enable development mode for the Ultimate Button? This mode will allow you to see the default inspector for the Ultimate Button which is useful when adding variables to this script without having to edit the custom editor script.", "Enable", "Cancel" ) )
				{
					EditorPrefs.SetBool( "UUI_DevelopmentMode", !EditorPrefs.GetBool( "UUI_DevelopmentMode" ) );
				}
			}
			else
				EditorPrefs.SetBool( "UUI_DevelopmentMode", !EditorPrefs.GetBool( "UUI_DevelopmentMode" ) );
		}

		EndPage();
	}

	void ResetColors ()
	{
		serializedObject.FindProperty( "colorDefault" ).colorValue = new Color( 1.0f, 1.0f, 1.0f, 0.5f );
		serializedObject.FindProperty( "colorValueChanged" ).colorValue = new Color( 0.0f, 0.376f, 1.0f, 1.0f );
		serializedObject.ApplyModifiedProperties();


		EditorPrefs.SetString( "UB_ColorDefaultHex", "#" + ColorUtility.ToHtmlStringRGBA( new Color( 1.0f, 1.0f, 1.0f, 0.5f ) ) );
		EditorPrefs.SetString( "UB_ColorValueChangedHex", "#" + ColorUtility.ToHtmlStringRGBA( new Color( 0.0f, 0.376f, 1.0f, 1.0f ) ) );
	}

	void ShowDocumentation ( DocumentationInfo info )
	{
		GUILayout.Space( paragraphSpace );
		
		EditorGUILayout.LabelField( info.functionName, itemHeaderStyle );
		var rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			info.targetShowMore = !info.targetShowMore;

		if( info.targetShowMore != info.showMore && Event.current.type == EventType.Layout )
			info.showMore = info.targetShowMore;
		
		if( info.showMore )
		{
			EditorGUILayout.LabelField( Indent + "<i>Description:</i> " + info.description, paragraphStyle );

			if( info.parameter != null )
			{
				for( int i = 0; i < info.parameter.Length; i++ )
					EditorGUILayout.LabelField( Indent + "<i>Parameter:</i> " + info.parameter[ i ], paragraphStyle );
			}
			if( info.returnType != string.Empty )
				EditorGUILayout.LabelField( Indent + "<i>Return type:</i> " + info.returnType, paragraphStyle );

			if( info.codeExample != string.Empty )
				EditorGUILayout.TextArea( info.codeExample, UnityEngine.GUI.skin.textArea );

			GUILayout.Space( paragraphSpace );
		}
	}

	public static void OpenReadmeDocumentation ()
	{
		SelectReadmeFile();

		if( !pageHistory.Contains( AllPages[ 3 ] ) )
			NavigateForward( 3 );
	}

	[MenuItem( "Window/Tank and Healer Studio/Ultimate Button", false, 2 )]
	public static void SelectReadmeFile ()
	{
		var ids = AssetDatabase.FindAssets( "README t:UltimateButtonReadme" );
		if( ids.Length == 1 )
		{
			var readmeObject = AssetDatabase.LoadMainAssetAtPath( AssetDatabase.GUIDToAssetPath( ids[ 0 ] ) );
			Selection.objects = new Object[] { readmeObject };
			readme = ( UltimateButtonReadme )readmeObject;
		}
		else
			Debug.LogError( "There is no README object in the Ultimate Button folder." );
	}
}