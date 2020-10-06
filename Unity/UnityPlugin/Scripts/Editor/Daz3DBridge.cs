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
        private Vector2 scrollPos;
        System.Tuple<UnityEngine.Object, Texture> thumbnail = null;

        private static Daz3DBridge _instance;
        [MenuItem("Daz3D/Open Daz3DBridge window")]
        public static void ShowWindow()
        {
            _instance = (Daz3DBridge)GetWindow(typeof(Daz3DBridge));
            _instance.titleContent = new GUIContent("Daz3D Bridge");
        }

        public static float Progress = 0;
        private static readonly Color _themedColor = new Color(.7f, 1f, .8f);

        private bool _needsRepaint = false;
        void Update()
        {
            if(_needsRepaint)
            {
                _needsRepaint = false;
                Repaint();
            }
        }

        void OnSelectionChange()
        {
            _needsRepaint = true;
        }

        Texture headColor = null;
        Texture logoWhite = null;


        void OnGUI()
        {
            var temp = GUI.backgroundColor;

            GUI.backgroundColor = _themedColor;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();




            if (Daz3DDTUImporter.ValidateDTUSelected())
            {
                var dtu = Selection.activeObject;
                var dtuPath = AssetDatabase.GetAssetPath(dtu);
                var fbxPath = dtuPath.Replace(".dtu", ".fbx");

                if (thumbnail == null || dtu != thumbnail.Item1)
                {
                    var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                    thumbnail = new System.Tuple<UnityEngine.Object, Texture>(dtu, AssetPreview.GetAssetPreview(fbx));
                }

                GUIStyle bigStyle = new GUIStyle(GUI.skin.label);
                bigStyle.fontSize = 24;
                bigStyle.normal.textColor = _themedColor;

                //big conspicuous prompt to click
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Click to create prefab for " + dtu.name + " ->", bigStyle);
                GUILayout.EndHorizontal();

                //caption with full path
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("(" + dtuPath + ")");
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                //the button
                if (GUILayout.Button(thumbnail.Item2, GUILayout.Height(100), GUILayout.Width(100)))
                {
                    Daz3DDTUImporter.Import(dtuPath, fbxPath);
                }


            }
            else
            {
                thumbnail = null;
                if (headColor == null)
                    headColor = Resources.Load<Texture>("Daz_Head_Color_5K");
                else if (logoWhite == null)
                    logoWhite = Resources.Load<Texture>("Daz_Logo_White_4K");
                else
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(headColor, GUILayout.Height(100), GUILayout.Width(190));
                    GUILayout.Label(logoWhite, GUILayout.Height(100));
                    GUILayout.FlexibleSpace();

                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUIStyle myStyle = new GUIStyle(GUI.skin.label);
            myStyle.margin = new RectOffset(0, 0, 0, 0);

            var labels = new string[] { "History", "Read Me", "Options" };
            toolbarMode = GUILayout.Toolbar(toolbarMode, labels);

            switch (toolbarMode)
            {
                case (0):
                    DrawHistory(myStyle);
                    break;
                case 1:
                    DrawHelpReadMe();
                    break;
                case 2:
                    DrawOptions();
                    break;

            }

            GUI.backgroundColor = temp;

        }

        private void DrawOptions()
        {
            GUILayout.Label("Coming Soon...");
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

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var record in Daz3DDTUImporter.EventQueue)
            {

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Import Event: " + record.Timestamp);
                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                GUILayout.Space(16);

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
                        GUILayout.Space(16);
                    }

                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.EndVertical();

                GUILayout.Space(8);

            }




            EditorGUILayout.EndScrollView();
        }

        int toolbarMode = 0;

        Vector2 readMePos = Vector2.zero;
        private void DrawHelpReadMe()
        {
            var readMe = Resources.Load<TextAsset>("ReadMe");
            readMePos = GUILayout.BeginScrollView(readMePos);
            GUILayout.TextArea(readMe.text);
            GUILayout.EndScrollView();
        }
    }

}
