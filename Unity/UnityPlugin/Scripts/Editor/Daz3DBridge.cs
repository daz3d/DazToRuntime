using UnityEngine;
using UnityEditor;
using System;




namespace Daz3D
{

    using record = Daz3DDTUImporter.ImportEventRecord;

    /// <summary>
    /// An editor window where unity user can monitor the progress and history and details of DTU import activity 
    /// </summary>
    public class Daz3DBridge : EditorWindow
    {
        private Vector2 _scrollPos;
        //Tuple<UnityEngine.Object, Texture> thumbnail = null;
        public static readonly Color ThemedColor = new Color(.7f, 1f, .8f);

        Texture masthead = null;
        public enum ToolbarMode
        {
            History,
            ReadMe,
            Options,
            Commands
        }
        public static ToolbarMode CurrentToolbarMode = ToolbarMode.ReadMe;
        private bool _needsRepaint = false;

        Vector2 readMePos = Vector2.zero;
        private static float _progress;
        public static float Progress
        {
            get { return _progress; }
            set { _progress = value; _instance?.Repaint(); }
        }

        private static Daz3DBridge _instance;
        [MenuItem("Daz3D/Open Daz3DBridge window")]
        public static void ShowWindow()
        {
            ObtainInstance();
        }
        private static void ObtainInstance() 
        {  
            _instance = (Daz3DBridge)GetWindow(typeof(Daz3DBridge));
            _instance.titleContent = new GUIContent("Daz to Unity Bridge");
        }

        void Update()
        {
            if (_needsRepaint)
            {
                _needsRepaint = false;

                Repaint();
            }

            if (!_instance == null)
                ObtainInstance();
        }



        private void DrawProgressBar()
        {
            if (Progress > 0)
            {
                //float progress = Mathf.Abs(DateTime.Now.Millisecond * .001f);

                GUI.backgroundColor = new Color(1, .2f, .1f);
                GUILayout.Button("", GUILayout.Width(position.width * Progress), GUILayout.Height(12));
            }
        }


        void OnGUI()
        {
            DrawProgressBar();

            var temp = GUI.backgroundColor;

            GUI.backgroundColor = ThemedColor;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();

            if (masthead == null)
                masthead = Resources.Load<Texture>("Daz_Combined_Small");

            GUILayout.FlexibleSpace();
            GUILayout.Label(masthead, GUILayout.Height(100));
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUIStyle myStyle = new GUIStyle(GUI.skin.label);
            myStyle.margin = new RectOffset(0, 0, 0, 0);

            var labels = new string[] { "History", "Read Me", "Options", "Commands" };
            CurrentToolbarMode = (ToolbarMode)GUILayout.Toolbar((int)CurrentToolbarMode, labels);

            switch (CurrentToolbarMode)
            {
                case ToolbarMode.History:
                    DrawHistory(myStyle);
                    break;
                case ToolbarMode.ReadMe:
                    DrawHelpReadMe();
                    break;
                case ToolbarMode.Options:
                    DrawOptions();
                    break;
                case ToolbarMode.Commands:
                    DrawCommands();
                    break;

            }

            GUI.backgroundColor = temp;

        }


        void OnSelectionChange()
        {
            if (Daz3DDTUImporter.ValidateDTUSelected())
                CurrentToolbarMode = ToolbarMode.Commands;
            _needsRepaint = true;
        }

        private void DrawCommands()
        {
            GUIStyle bigStyle = new GUIStyle(GUI.skin.button);
            bigStyle.fontSize = 14;
            bigStyle.normal.textColor = ThemedColor;
            bigStyle.padding = new RectOffset(24, 24, 12, 12);
            bigStyle.margin = new RectOffset(12, 12, 12, 12);

            if (CommandButton("Clear History", bigStyle))
                Daz3DDTUImporter.EmptyEventQueue();

            if (Daz3DDTUImporter.ValidateDTUSelected())
            {
                var dtu = Selection.activeObject;

                //the button
                if (CommandButton("Create Unity Prefab from '" + dtu.name + ".dtu'", bigStyle))
                {
                    var dtuPath = AssetDatabase.GetAssetPath(dtu);
                    var fbxPath = dtuPath.Replace(".dtu", ".fbx");
                    Daz3DDTUImporter.Import(dtuPath, fbxPath);
                }

                if (CommandButton("Extract Materials from '" + dtu.name + ".dtu'", bigStyle))
                    DTUConverter.MenuItemConvert();
            }
            else
            {
                GUILayout.Space(12);
                GUILayout.Label("Select a DTU file in project window for more commands...");
            }
        }


        private bool CommandButton(string label, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var result = GUILayout.Button(label, style);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return result;
        }

        private void DrawOptions()
        {
            GUIStyle bigStyle = new GUIStyle(GUI.skin.toggle);
            bigStyle.fontSize = 14;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.Space(12);

            Daz3DDTUImporter.AutoImportDTUChanges = GUILayout.Toggle(Daz3DDTUImporter.AutoImportDTUChanges, "Automatically import when DTU changes are detected", bigStyle);
            Daz3DDTUImporter.GenerateUnityPrefab = GUILayout.Toggle(Daz3DDTUImporter.GenerateUnityPrefab, "Generate a Unity Prefab based on FBX and DTU", bigStyle);
            Daz3DDTUImporter.ReplaceSceneInstances = GUILayout.Toggle(Daz3DDTUImporter.ReplaceSceneInstances, "Replace instances of Unity Prefab in active scene(s)", bigStyle);
            Daz3DDTUImporter.AutomateMecanimAvatarMappings = GUILayout.Toggle(Daz3DDTUImporter.AutomateMecanimAvatarMappings, "Automatically setup the Mecanim Avatar", bigStyle);
            Daz3DDTUImporter.ReplaceMaterials = GUILayout.Toggle(Daz3DDTUImporter.ReplaceMaterials, "Replace FBX materials with high quality Daz-shader materials", bigStyle);

            GUILayout.Space(12);
            if (GUILayout.Button("Reset All", GUILayout.Width(100)))
                Daz3DDTUImporter.ResetOptions();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawHistory(GUIStyle myStyle)
        {
            if (Daz3DDTUImporter.EventQueue.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("*Click the ", myStyle);
                GUI.contentColor = Color.cyan;
                GUILayout.Label("links", myStyle);
                GUI.contentColor = Color.white;
                GUILayout.Label(" below to select those assets in Project window.", myStyle);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var record in Daz3DDTUImporter.EventQueue)
            {

                GUILayout.BeginVertical(GUI.skin.box);
                record.Unfold = EditorGUILayout.Foldout(record.Unfold, "Import Event: " + record.Timestamp);

                //GUILayout.Label("Import Event: " + record.Timestamp);

                if (record.Unfold)
                {
                    GUILayout.Space(4);//lead
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(16);//indent

                    //foreach (var word in words)
                    foreach (var token in record.Tokens)
                    {
                        if (token.Selectable)
                        {
                            GUI.contentColor = Color.cyan;

                            if (GUILayout.Button(token.Text, myStyle))
                                Selection.activeObject = token.Selectable;

                            GUI.contentColor = Color.white;

                        }
                        else
                        {
                            GUILayout.Label(token.Text, myStyle);
                        }

                        if (token.EndLine)
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(16);//indent
                        }

                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();

                //GUILayout.Space(8);

            }




            EditorGUILayout.EndScrollView();
        }

        private void DrawHelpReadMe()
        {
            var readMe = Resources.Load<TextAsset>("ReadMe");
            readMePos = GUILayout.BeginScrollView(readMePos);
            GUILayout.TextArea(readMe.text);
            GUILayout.EndScrollView();
        }
    }


    [InitializeOnLoad]
    public static class OrnamenfftDTUFileInProjectWindow
    {

        static OrnamenfftDTUFileInProjectWindow()
        {
            EditorApplication.projectWindowItemOnGUI += DrawAssetDetails;

        }
        private static void DrawAssetDetails(string guid, Rect rect)
        {
            if (Application.isPlaying || Event.current.type != EventType.Repaint || !IsMainListAsset(rect))
                return;

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.ToLower().EndsWith(".dtu"))
                return;

            rect.x += 5;
            //rect.width = rect.height;


            GUI.color = new Color(.5f, 1, .5f);
            GUI.Label(rect, "■");

            rect.x -= 3;
            rect.y += 4;

            GUI.color = Color.yellow;
            GUI.Label(rect, "◢");

            rect.x -= 6;
            rect.y -= 4;

            GUI.color = new Color(1f, .3f, .2f);
            GUI.Label(rect, "◀");

            //rect.x --;
            //rect.y -= 2;
            GUI.color = Color.cyan;
            GUI.Label(rect, "◤");

            GUI.color = Daz3DBridge.ThemedColor * .7f;
            rect.x -= 30;
            GUI.Label(rect, "DTU");

            GUI.color = Color.white;
        }

        private static bool IsMainListAsset(Rect rect)
        {
            return rect.height <= 20;
        }
    }

}
