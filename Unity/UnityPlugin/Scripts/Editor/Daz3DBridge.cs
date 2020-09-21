using UnityEngine;
using UnityEditor;

namespace Daz3D
{
    /// <summary>
    /// An editor window where unity user can monitor the progress and history and details of DTU import activity 
    /// </summary>
    public class Daz3DBridge : EditorWindow
    {
        private Vector2 scrollPos;
        //private ErrorState s_errorState = ErrorState.Clean;




        private static Daz3DBridge _instance;
        [MenuItem("Daz3D/Daz3DBridge")]
        public static void ShowWindow()
        {
            _instance = (Daz3DBridge)GetWindow(typeof(Daz3DBridge));
        }

        public enum ErrorState
        {
            Clean,
            NoDazRootPath,
            NoDazScenePath
        }

        void OnFocus()
        {
            CheckRefresh();
        }

        void OnEnable()
        {
            CheckRefresh();
        }

        void CheckRefresh()
        {
            //CreateBaseMaterial();
            CheckDazDirectories();
        }

        void CheckDazDirectories()
        {
            //var assetsDir = BuildUnityPath(Application.dataPath, "Assets");
            //s_dazRootDir = BuildUnityPath(Application.dataPath, "Daz3D");

            //if (!Directory.Exists(s_dazRootDir))
            //{
            //    //if the daz root directory is missing then all bets are off -- fail all the open scenes
            //    //foreach (var scene in m_openScenes)
            //    //    scene.Fail(ErrorState.NoDazRootPath);

            //    Fail(ErrorState.NoDazRootPath);

            //    return;

            //    //string guid = AssetDatabase.CreateFolder("Assets", "Daz3D");
            //    //dazRootDir = AssetDatabase.GUIDToAssetPath(guid);
            //    //Debug.Log("created folder   " + s_dazRootDir);
            //}

            //TODO check to see if the daz root folder for the loaded scene(s) are dirty
            //and refresh accordingly

        }

       

        void Update()
        {
            Repaint();
        }



        //todo if selection.active is non null, then refresh the thumbnail from the fbx file
        //null the thumbnail when its null or changed


        System.Tuple<Object, Texture> thumbnail = null;

        void OnGUI()
        {
            var temp = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();

            var dtu = Selection.activeObject;
            if (dtu)
            {
                var dtuPath = AssetDatabase.GetAssetPath(dtu);
                if (thumbnail == null || dtu != thumbnail.Item1)
                {
                    var fbxPath = dtuPath.Replace(".dtu", ".fbx");
                    var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                    thumbnail = new System.Tuple<Object, Texture>(dtu, AssetPreview.GetAssetPreview(fbx));
                }


                if (dtuPath.ToLower().EndsWith("dtu"))//todo more robust test?
                {
                    if (GUILayout.Button(thumbnail.Item2, GUILayout.Height(100), GUILayout.Width(100)))
                    {
                        var dtuMaterialMap = Daz3DDTUImporter.ImportDTU(dtuPath);

                        var fbxPath = dtuPath.Replace(".dtu", ".fbx");
                        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                        if (fbx && PrefabUtility.GetPrefabAssetType(fbx) == PrefabAssetType.Model)
                        {
                            Daz3DDTUImporter.GeneratePrefabFromFBX(fbx, dtuMaterialMap);
                        }
                    }
                    GUILayout.Label(dtuPath + " is a dtu file.");
                }

            }

            GUILayout.EndHorizontal();
            //GUILayout.ProgressBar( 50f / 100f, "Computing Stuff, chill.");

            GUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));

            foreach (var record in Daz3DDTUImporter.EventQueue)
            {
                GUILayout.BeginVertical(GUI.skin.button);
                GUILayout.BeginHorizontal();
                GUILayout.TextField(record.Timestamp.ToString());
                GUILayout.TextField(record.Path);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUI.backgroundColor = temp;

        }


        //public void Fail(ErrorState error)
        //{
        //    s_errorState = error;
        //    Debug.LogError("Daz3DBridge fails with error code: " + s_errorState);

        //}




    }

}
