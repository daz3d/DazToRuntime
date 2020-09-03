using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

using Debug = UnityEngine.Debug;
using UnityEngine.Rendering;
using System;
using DTUJson;
using DTUData = DTUJson.DTUData;

/// <summary>
/// TODO Daz's commenting convention 
/// </summary>
public class Daz3DBridge : EditorWindow
{

    Vector2 scrollPos;

    enum MaterialID //these positions map to the bitflags in the compiled HDRP lit shader
    {
        SSS = 0,
        Standard = 1,
        Anisotropy = 2,
        Iridescence = 3,
        SpecularColor = 4,
        Translucent = 5
    }

    internal void Process(string relativePath)
    {
        var jsonPath = relativePath;
         
        if (jsonPath.StartsWith("Assets/"))
            jsonPath = jsonPath.Replace("Assets/", "");

        jsonPath = BuildUnityPath(Application.dataPath, jsonPath);

        if (!File.Exists(jsonPath))
        {
            Debug.LogError(jsonPath + " does not exist");
            return;
        }

        var dtuData = ParseJSON_DTU(jsonPath);

        var fbxPath = relativePath.Replace(".dtu", ".fbx");
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbx && PrefabUtility.GetPrefabAssetType(fbx) == PrefabAssetType.Model)
        {
            TestSurgery(fbx, dtuData);
        }

    }

    static Dictionary<string, Material> s_StandardMaterialCollection = new Dictionary<string, Material>();



    private ErrorState s_errorState = ErrorState.Clean;
    public void Fail(ErrorState error)
    {
        s_errorState = error;
        Debug.LogError("Daz3DBridge fails with error code: " + s_errorState);

    }
    public enum ErrorState  
    {
        Clean,
        NoDazRootPath,
        NoDazScenePath
    }


    //TODO this class is the deserialization target for the DTU JSON
    [System.Serializable]
    class DazMaterialPropertiesInfo
    {
        public DazMaterialPropertiesInfo() //experimental ctor
        {
            BaseColor = Color.white;// UnityEngine.Random.ColorHSV();

            //these are suitable for hair surfaces
            Transparent = true;//for hair!!!???
            Metallic = 1;//for hair!!!???
            Smoothness = 0;//for hair!!!???
            MaterialType = MaterialID.Anisotropy;//for hair!!!???
            DoubleSided = true; //for hair!!!???
        }

        public Color BaseColor { get; set; }
        public bool Transparent { get; set; }

        public float Metallic { get; set; }
        public float Smoothness { get; set; }

        public MaterialID MaterialType { get; set; }

        public bool DoubleSided { get; set; }
    }

     


    //TODO demote to dazScene
    private static Dictionary<string, Material> s_materialsMap = new Dictionary<string, Material>();

                string m_String = "Bridge";
                //bool m_groupEnabled;
                //bool m_Bool = true;
                //float m_Float = 1.23f;

     

    static string s_dazRootDir = "NOT_SET_DAZROOTDIR";
    //static Material s_baseMaterial;

    //TODO create materials from json source
    //static void CreateMaterial(DazMaterialPropertiesInfo matInfo, string path)
    //{
    //    bool specular = false;
    //    var shader = Shader.Find(specular ? "Standard(Specular setup)" : "Standard");

    //    //TODO use the HDRP/Lit shader
    //    shader = Shader.Find("HDRP/Lit");

    //    Material material = new Material(shader);  

    //    material.CopyPropertiesFromMaterial(s_baseMaterial);
    //    material.EnableKeyword("_NORMALMAP"); //etc. TODO research which keywords are important for daz-like features

    //    material.name = "NEEDS_A_KEY_FOR_A_NAME"; //TODO name the materials after their daz material counterparts


    //    //TODO resolve home folder for this material ...........this should be computed by caller
    //    //var modelPath = s_dazRootDir;// AssetDatabase.GetAssetPath(model);
    //        //var matPath =  BuildUnityPath(s_dazRootDir, "MaterialsTestFolder");
    //        var matPath = BuildUnityPath(path, "Materials");



    //    if (!Directory.Exists(matPath))
    //    {
    //        AssetDatabase.CreateFolder("Assets/Daz3D", "Materials");
    //    }


    //    //here we need to account for model instances which are assigned an earlier version of this material
    //    //and assign this material to those meshes instead of the material instance from earlier
    //    //writing this asset does not override the assignment of materials copied from a prior instance of this mat asset
    //    //in other words, materials is the scene remain the same but the underlying asset file changes

    //    //TODO perhaps test whether the material asset already exists, overwrite it if it changes?

    //    AssetDatabase.CreateAsset(material, "Assets/Daz3D/Materials/TestMaterial.mat");
    //    Debug.Log(AssetDatabase.GetAssetPath(material));
    //}

    //TODO create scene from json source
    //static void CreateScene(/* parm info */)
    //{
    //    var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
    //}

    //TODO create prefab from FBX source and remap materials
    //static void CreatePrefab(/*parm info*/)
    //{
    //    GameObject[] objectArray = Selection.gameObjects; // TODO use an object instanced here?

    //    foreach (GameObject gameObject in objectArray)
    //    {
    //        string localPath = "Assets/" + gameObject.name + ".prefab"; //TODO use a sensible daz subfolder

    //        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath); //unique path/name

    //        PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, localPath, InteractionMode.UserAction);
    //    }
    //}



    // Add menu item named "My Window" to the Window menu
    [MenuItem("Daz3D/Daz3DBridge")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(Daz3DBridge));
    }


    void CheckRefresh()
    {
        //CreateBaseMaterial();

        var assetsDir = BuildUnityPath(Application.dataPath, "Assets");
        s_dazRootDir = BuildUnityPath(Application.dataPath, "Daz3D");

        if (!Directory.Exists(s_dazRootDir))
        {
            //if the daz root directory is missing then all bets are off -- fail all the open scenes
            //foreach (var scene in m_openScenes)
            //    scene.Fail(ErrorState.NoDazRootPath);

            Fail(ErrorState.NoDazRootPath);

            return;

            //string guid = AssetDatabase.CreateFolder("Assets", "Daz3D");
            //dazRootDir = AssetDatabase.GUIDToAssetPath(guid);
            //Debug.Log("created folder   " + s_dazRootDir);
        }

        //TODO check to see if the daz root folder for the loaded scene(s) are dirty
        //and refresh accordingly

    }

    //static void CreateBaseMaterial()
    //{
    //    if (s_baseMaterial == null)
    //    {
    //        s_baseMaterial = new Material(Shader.Find("Standard"));
    //        //TODO assign properties to the base material
    //        //TODO create two base materials, spec and metallic
    //    }
    //}

    public static string BuildUnityPath(string path, string subDir )
    {
        var result = Path.Combine(path, subDir).Replace("\\", "/"); //unity likes forward slashes in asset database paths;

        return result;
    }

   
    void Update()
    {
        Repaint();
    }

    void OnGUI()
    {
        var temp = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;

        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();

        GameObject fbxPrefab = Selection.activeGameObject;
        if (fbxPrefab && PrefabUtility.GetPrefabAssetType(fbxPrefab) == PrefabAssetType.Model)
        {
            var tex = AssetPreview.GetAssetPreview(fbxPrefab);

            if (GUILayout.Button(tex, GUILayout.Height(100), GUILayout.Width(100)))
            {
                TestSurgery(fbxPrefab, null);
            }

            GUILayout.BeginVertical();
            var path = AssetDatabase.GetAssetPath(fbxPrefab);
            GUILayout.Label(path);
            if (!string.IsNullOrEmpty(path))
            {
                var fileInfo = new FileInfo(path);
                GUILayout.Label("Created: " + fileInfo.CreationTime);
                GUILayout.Label("Last edit: " + fileInfo.LastWriteTime);
                GUILayout.Label("Size: " + fileInfo.Length / 1024L + " kB");
            }

            //var user = File.GetAccessControl(path).GetOwner(typeof(System.Security.Principal.NTAccount));
            //GUILayout.Label("Author: " + user);

            GUILayout.EndVertical();
        }
        else
        {
            var dtu = Selection.activeObject;
            if (dtu)
            {
                var tex = AssetPreview.GetAssetPreview(dtu);
                var dtuPath = AssetDatabase.GetAssetPath(dtu);

                if (dtuPath.ToLower().EndsWith("dtu"))//todo more robust test?
                {
                    if (GUILayout.Button(tex, GUILayout.Height(100), GUILayout.Width(100)))
                    {
                        
                        var data = ParseJSON_DTU(dtuPath);

                        var fbxPath = dtuPath.Replace(".dtu", ".fbx");
                        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                        if (fbx && PrefabUtility.GetPrefabAssetType(fbx) == PrefabAssetType.Model)
                        {
                            TestSurgery(fbx, data);
                        }
                    }
                    GUILayout.Label(dtuPath + " is a dtu file.");
                }
                 
            }
        }

        

        GUILayout.EndHorizontal();

        //GUILayout.ProgressBar( 50f / 100f, "Computing Stuff, chill.");

        GUILayout.Space(10);


        scrollPos =
            EditorGUILayout.BeginScrollView(scrollPos,  GUILayout.Height(100));

        foreach( var record in Daz3DDTUImporter.EventQueue)
        {
            GUILayout.BeginVertical(GUI.skin.button);
            GUILayout.BeginHorizontal();
            GUILayout.TextField(record.Timestamp.ToString());
            GUILayout.TextField(record.Path);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        //GUILayout.Label("he1llo\nheffff2llo\nhe3llo\nh4ello\nhe5llo\nh6llo\nhefff7llo\n8ello\nh9ello\n10hffffffello\nh11ello\nh12ello\nh13ello\nh14ello\nhe15llo\nh16bbbbello\nhe17llo");
        EditorGUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUI.backgroundColor = temp;

        

    }

    //void TestMat()
    //{
    //    DazMaterialPropertiesInfo matInfo = new DazMaterialPropertiesInfo();
    //    CreateMaterial(matInfo, s_dazRootDir);
    //}


    static string[,] uberFieldNames =
    {
        //{"UV Set","" },
        {"Smooth On","_Smoothness" }, //bool MaterialID == 4 means specular
        {"Smooth Angle","_Smoothness" },
        //{"Line Preview Color","" },// interesting color
        //{"Line Start Width","" },
        //{"Line End Width","" },
        //{"Line UV Width","" },
        {"Render Priority","_TransparentSortPriority" },
        //{"Propagate Priority","" },
  //      {"Follow Blend", "" },//bool
		//{"Base Mixing","" },
        {"Metallic Weight","_Metallic" },
        //{"Diffuse Weight","" },
        {"Diffuse Color", "_BaseColor" },//interesting color &| texture
		//{"Diffuse Strength","" },
  //      {"Diffuse Roughness","" },
        //{"Diffuse Overlay Weight","" }, //what is analog to diffuse overlay (tattoo?) "Detail?"
  //      {"Diffuse Overlay Weight Squared","" },
  //      {"Diffuse Overlay Color", "" },// interesting color
		//{"Diffuse Overlay Color Effect","" },
  //      {"Diffuse Overlay Roughness","" },
        //{"Translucency Weight","" },
        {"Base Color Effect","" },
        {"Translucency Color", "" },//interesting color &| texture
		//{"Invert Transmission Normal","" }, //bool
		{"SSS Reflectance Tint", "" },// interesting color
		//{"Dual Lobe Specular Weight","" },
  //      {"Dual Lobe Specular Reflectivity","" },
        //{"Specular Lobe 1 Roughness","" },
        //{"Specular Lobe 2 Roughness","" },
        //{"Specular Lobe 1 Glossiness","" },
        //{"Specular Lobe 2 Glossiness","" },
        //{"Dual Lobe Specular Ratio","" },
        //{"Glossy Weight","" }, 
        //{"Glossy Layered Weight","" },
        //{"Share Glossy Inputs", "" },//bool //bool MaterialID == 4 means specular
		{"Glossy Color", "_SpecularColor" },// interesting color // _SpecularColorMap
		//{"Glossy Color Effect","" },// is this like materialID?
        {"Glossy Reflectivity","_Smoothness" },
        {"Glossy Specular", "" },// interesting color, MaterialID == 4 means specular
		{"Glossy Roughness","_Smoothness" }, //inverted range?????????
        {"Glossiness","_SpecularColorMap" }, //_SpecularColorMap? is this a texture in iray?
        {"Glossy Anisotropy","_Anisotropy" },//_AnisotropyMap
  //      {"Backscattering Weight","" },
  //      {"Backscattering Color","" },// interesting color
		//{"Backscattering Roughness","" },
  //      {"Backscattering Glossiness","" },
  //      {"Backscattering Anisotropy","" },
        {"Refraction Index","_DistortionEnable" },
        {"Refraction Weight","_DistortionScale" },
        {"Refraction Color","_DistortionBlendMode" },// interesting color
		{"Refraction Roughness","_DistortionBlurScale" },
        //{"Refraction Glossiness","" }, ///_Smoothness?
        {"Abbe","_IridescenceThickness" }, //abbe is iridescence
        {"Glossy Anisotropy Rotations","" },
        //{"Base Thin Film","" },
        //{"Base Thin Film IOR","" },
        {"Bump Strength","_NormalScale" },//_HeightMap?
        {"Normal Map", "_NormalMap" },//bool???
		//{"Metallic Flakes Weight","" },
  //      {"Metallic Flakes Color","" },// interesting color 
		//{"Metallic Flakes Color Effect","" },
  //      {"Metallic Flakes Roughness","" },
  //      {"Metallic Flakes Glossiness","" },
  //      {"Metallic Flakes Size","" },
  //      {"Metallic Flakes Strength","" },//texture?//_IridescenceMask
  //      {"Metallic Flakes Density","" },
  //      {"Metallic Flakes Thin Film","" },
  //      {"Metallic Flakes Thin Film IOR","" },
  //      {"Top Coat Weight","" },
  //      {"Top Coat Color","" },// interesting color
		//{"Top Coat Color Effect","" },//bool???
		//{"Top Coat Roughness","" },
  //      {"Top Coat Glossiness","" },
  //      {"Top Coat Layering Mode","" },
        {"Reflectivity","_SpecularColorMap" },//is refletivity an environment map?
  //      {"Top Coat IOR","" },//index of reflection/fraction
  //      {"Top Coat Curve Normal","" },
  //      {"Top Coat Curve Grazing", "" },//bool???
		//{"Top Coat Curve Exponent","" },
  //      {"Top Coat Anisotropy","" },
  //      {"Top Coat Rotations","" },
  //      {"Top Coat Thin Film","" },
  //      {"Top Coat Thin Film IOR","" },
  //      {"Top Coat Bump Mode","" },
  //      {"Top Coat Bump","" },
  //      {"Thin Walled","" },//not compatible with abbe
        {"Transmitted Measurement Distance","_TransmissionEnable" },
        {"Transmitted Color","_TransmittanceColor" },// interesting color _TransmissionEnable
		{"SSS Mode","_ReceivesSSR" }, //_ReceivesSSR
        //{"Scattering Measurement Distance","" },
        {"SSS Amount","_SubsurfaceMask" }, // consider the stencil usage flags in unity
        //{"SSS Color","" },// interesting color
		//{"SSS Direction","" },
        {"Emission Color","_EmissiveColor" },// interesting color //_EmissiveColorMap //LDR light decreasing resistance
		//{"Emission Temperature","" },//is there an equiv in hdrp shaderland?
  //      {"Two Sided Light", "" },//bool
		//{"Luminance","" },
  //      {"Luminance Units","" },
  //      {"Luminous Efficacy","" },
        {"Cutout Opacity","_AlphaCutoff" },
        //{"Displacement Strength","" }, //what is unity analog for displacement?
        //{"Minimum Displacement","" },
        //{"Maximum Displacement","" },
        //{"SubD Displacement Level","" },
  //      {"Horizontal Tiles","" },//bool
		//{"Horizontal Offset","" },
  //      {"Vertical Tiles","" },//bool
		//{"Vertical Offset","" },
  //      {"Round Corners Radius","" },
  //      {"Round Corners Across Materials","" },//bool
		//{"Round Corners Roundness","" },
        //{"Roughness Squared","" },
        //{"Glossiness Squared","" },
        //{"BSDF Type","" },//bidirectional scattering
        //{"OutlineColorID", "" }  // interesting color

    };

    private static Dictionary<BaseMaterialType, Material> s_BaseMaterialCollection = new Dictionary<BaseMaterialType, Material>();
    

    private enum BaseMaterialType
    {
        Fallback,
        Scalp,
        EyeMoisture,
        Alpha,
        Skin,
        Cornea,
        Cutout,
        Pupil,
        Iris,
        Sclera,
        Eyelash,
        Hair,
        NoDraw 
    }


    private enum StandardMaterialType
    {
        Arms,
        Cornea,
        Ears,
        Eyelashes,
        EyeMoisture_1,
        EyeMoisture,
        EyeSocket,
        Face,
        Fingernails,
        Irises,
        Legs,
        Lips,
        Mouth,
        Pupils,
        Sclera,
        Teeth,
        Toenails,
        Torso
    }


    static void InitBaseMaterials()
    {
        foreach (BaseMaterialType i in Enum.GetValues(typeof(BaseMaterialType)))
            InitBaseMaterial(i);

        foreach (StandardMaterialType i in Enum.GetValues(typeof(StandardMaterialType)))
            InitStandardMaterial(i);

    }

    static void InitStandardMaterial(StandardMaterialType type)
    {
        var key = type.ToString();
        if (s_StandardMaterialCollection.ContainsKey(key))
            return;

        var standardName = key + "_Standard.mat";
        var path = BuildUnityPath("Assets/Daz3D/StandardMaterialsGen8", standardName);

        //find the material asset
        var standardMat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (standardMat != null)
            s_StandardMaterialCollection.Add(key, standardMat);

    }

    static void InitBaseMaterial(BaseMaterialType type)
    {
        if (s_BaseMaterialCollection.ContainsKey(type))
            return;

        Material mat = null;
        switch (type)
        {
            case BaseMaterialType.NoDraw:
                mat = InitBaseNoDrawMaterial();
                break;
            case BaseMaterialType.Fallback:
                mat = InitBaseFallbackMaterial();
                break;
            case BaseMaterialType.Cutout:
                mat = InitBaseCutoutMaterial();
                break;
            case BaseMaterialType.Alpha:
                mat = InitBaseAlphaMaterial();
                break;
            default:
                mat = InitAnatomicalBaseMaterial(type);
                break;
        }

        if (mat)
        {
            s_BaseMaterialCollection.Add(type, mat);
            mat.name = "DazBaseMat_" + type;
        }
  
    }

    private static Material InitBaseNoDrawMaterial()
    {
        var mat = new Material(Shader.Find("Daz/DazNoDraw"));
        return mat;
    }

    private static Material InitBaseFallbackMaterial()
    {
        var mat = new Material(Shader.Find("HDRP/Lit"));
        return mat;
    }

    private static Material InitBaseCutoutMaterial()
    {
        var mat = new Material(Shader.Find("HDRP/Lit"));
        //todo adjust for cutout
        return mat;
    }

    private static Material InitBaseAlphaMaterial()
    {
        var mat = new Material(Shader.Find("HDRP/Lit"));
        //todo adjust for alpha
        return mat;
    }

    private static Material InitAnatomicalBaseMaterial( BaseMaterialType type)
    {
        var mat = new Material(Shader.Find("HDRP/Lit"));

        switch(type)
        {
            case BaseMaterialType.Scalp:
                //todo adjust for scalp
                break;
            case BaseMaterialType.Skin:
                //todo adjust for skin
                break;
            case BaseMaterialType.Sclera:
                //todo adjust for sclera
                break;
            case BaseMaterialType.Pupil:
                //todo adjust for scalp
                break;
            case BaseMaterialType.EyeMoisture:
                //todo adjust for eyemoisture
                break;
            case BaseMaterialType.Cornea:
                //todo adjust for cornea
                break;
            case BaseMaterialType.Iris:
                //todo adjust for iris
                break;
            case BaseMaterialType.Eyelash:
                //todo adjust for eyelash
                break;
            case BaseMaterialType.Hair:
                //todo adjust for hair
                break;
        }

        return mat;
    }

    private BaseMaterialType ChooseBestBaseMaterialType(DTUData.Material mat, ref Dictionary<BaseMaterialType, int> scores)
    {
        //TODO some massive simplification of the DTUData.Material properties is called for before this eval
        //becuz they are all raw strings that need to be parsed, and many (perhaps most) of the properties 
        //can/should be omitted because of zero net effect (all fields are written in json, even if null)


        CullInvalidBaseMaterials(mat, ref scores);
        CullUnsuitableBaseMaterials(mat, ref scores);
        ScoreCompatableBaseMaterials(mat, ref scores);

        //highest score wins, or if scores is empty, fallback to 'lit' shader
        var best = new KeyValuePair<BaseMaterialType, int>(BaseMaterialType.Fallback, int.MinValue);

        foreach (var score in scores)
            if (score.Value > best.Value) 
                best = score;

        return best.Key;
    }

    void CullInvalidBaseMaterials(DTUData.Material mat, ref Dictionary<BaseMaterialType, int> scores)
    {
        //1) Cull uber materials that have invalid or missing associated assets, 
        //  1a) e.g. bitmaps that are not valid normal or tangent maps but assigned as such
    }

    void CullUnsuitableBaseMaterials(DTUData.Material mat, ref Dictionary<BaseMaterialType, int> scores)
    {
        //2) Cull uber materials that are deemed unsuitable matches for keywords 
        //  2a) "iris" requires metallic and aniso settings, so cull other shaders
        //  2b) "moisture," "cornea" require spec highlight only (aditive blend), cull opaque and alpha
        //  2c) "lash" requires diffuse blacking, so cull any that can't tint diffuse


         
    }

    void ScoreCompatableBaseMaterials(DTUData.Material mat, ref Dictionary<BaseMaterialType, int> scores)
    {
        //3) Accumulate score points for correlated properties between unity shader and ubershader

        //  3a) Complicated by the fact that a field might be present in the shader source, but not
        //  compiled in because it is toggled off or switched off in a modal enum... just a raw
        //  string compare will not be informative enough.  Need to configure the unity material fields
        //  and toggles accordingly later on, and consider the one of many enumerated or switched
        //  properties that will match, instead of all the ones that can match

        //  3b) Complicated by the fact that all fields are expressed in the iray uber data
        //  and it is tricky to discern whether the value of the field has a net effect versus 
        //  just summary inclusion in the json output/  Need to tell the difference per field, based on
        //  discrete validation across dependent properties, 
        //  for example a thin walled surface invalidates abbe

         
        //var materialName = mat.MaterialName;

        //if (materialName.EndsWith("_NoDraw"))
        //{
        //    scores[BaseMaterialType.NoDraw]++;
        //}
        //else if (assetType == "Follower/Hair")
        //{
        //    if (materialName.EndsWith("_scalp"))
        //        scores[BaseMaterialType.Scalp]++;
        //    else
        //        scores[BaseMaterialType.Hair]++;
        //}
        //else if (assetType == "Follower/Attachment/Head/Face/Eyelashes")
        //{
        //    if (materialName.Contains("_EyeMoisture"))
        //        scores[BaseMaterialType.EyeMoisture]++;
        //    else
        //        scores[BaseMaterialType.Eyelash]++;
        //}
        //else if (assetType == "Follower/Attachment/Lower-Body/Hip/Front" &&
        //    materialName.Contains("_Genitalia"))
        //{
        //    scores[BaseMaterialType.Skin]++;
        //}
        //else if (assetType == "Actor/Character")
        //{
        //    // Check for skin materials
        //    if (materialName.EndsWith("_Face") ||
        //        materialName.EndsWith("_Lips") ||
        //        materialName.EndsWith("_Legs") ||
        //        materialName.EndsWith("_Torso") ||
        //        materialName.EndsWith("_Arms") ||
        //        materialName.EndsWith("_EyeSocket") ||
        //        materialName.EndsWith("_Ears") ||
        //        materialName.EndsWith("_Fingernails") ||
        //        materialName.EndsWith("_Toenails") ||
        //        materialName.EndsWith("_Genitalia"))
        //    {
        //        scores[BaseMaterialType.Skin]++;
        //    }
        //    else if (materialName.Contains("_EyeMoisture"))
        //    {
        //        scores[BaseMaterialType.EyeMoisture]++;
        //    }
        //    else if (materialName.EndsWith("_EyeLashes"))
        //    {
        //        scores[BaseMaterialType.Eyelash]++;
        //    }
        //    else if (materialName.EndsWith("_cornea"))
        //    {
        //        scores[BaseMaterialType.Cornea]++;
        //    }
        //    else if (materialName.EndsWith("_sclera"))
        //    {
        //        scores[BaseMaterialType.Sclera]++;
        //    }
        //    else if (materialName.EndsWith("_irises"))
        //    {
        //        scores[BaseMaterialType.Iris]++;
        //    }
        //    else if (materialName.EndsWith("_pupils"))
        //    {
        //        scores[BaseMaterialType.Pupil]++;
        //    }
        //    else
        //    {
        //        foreach ( var property in mat.Properties)
        //        {
        //            if (property.Name == "Cutout Opacity Texture")//TODO this might fail, if property is written all the time
        //            {
        //                scores[BaseMaterialType.Cutout]++;
        //                return;
        //            }
        //        }

        //        scores[BaseMaterialType.Fallback]++;  

        //    }
        //}
        //else if (materialName.Contains("_EyeMoisture"))
        //{
        //    scores[BaseMaterialType.EyeMoisture]++;
        //}
        //else
        //{
        //    foreach (var property in mat.Properties)
        //    {
        //        if (property.Name == "Cutout Opacity Texture")
        //        {
        //            scores[BaseMaterialType.Cutout]++;
        //            return;
        //        }
        //        else if (property.Name == "Opacity Strength" && property.Value != "1")
        //        {
        //            scores[BaseMaterialType.Alpha]++;
        //            return;
        //        }
        //    }

        //    scores[BaseMaterialType.Fallback]++;  

        //}



    }


    //DTUData TestJson (TextAsset textAsset) //todo deprecate?
    //{
    //    var dtuData = TestJson(textAsset.text);
    //    return dtuData;
    //}



    DTUData ParseJSON_DTU( string dtuPath )
    {
        var json = File.ReadAllText(dtuPath);
        //todo expect json to be a very long string... validate for min length?

        InitBaseMaterials();

        json = DazJSONHelper.FixKeySpaces(json);

        //Debug.Log(json);

        DTUJson.DTUData mo = JsonUtility.FromJson<DTUData>(json);

        DebugDumpDTUData(mo);

        return mo;
    }

    private static void DebugDumpDTUData(DTUData mo)
    {
        foreach(var plim in mo.Materials)
        {
            Debug.Log(" Material.Version " + plim.Version);
            Debug.Log(" Material.Asset Name " + plim.AssetName);//useful
            //Debug.Log(" Material.Asset Type " + plim.AssetType);//useful
            Debug.Log(" Material.Material Name " + plim.MaterialName);//useful
            Debug.Log(" Material.Material Type " + plim.MaterialType);
            //Debug.Log(" Material.Data Type " + plim.DataType);//useful

            for (int i = 0; i < plim.Properties.Count; i++)
            {
                var property = plim.Properties[i];
                Debug.Log(plim.MaterialName + " name " + property.Name + " value " + property.Value + " dataType " + property.DataType + " texture " + property.Texture);//useful
            }
        }
    }

    void TestSurgery(GameObject fbxPrefab, DTUData data)
    {
        InitBaseMaterials();

        if (fbxPrefab == null)
        {
            Debug.LogWarning("null prefab ");
            return;
        }

        if ( PrefabUtility.GetPrefabAssetType(fbxPrefab) != PrefabAssetType.Model)
        {
            Debug.LogWarning("not a model prefab ");
            return;
        }



        // TODO instantiate the prefab into the scene

        var instance = Instantiate(fbxPrefab);
        
        if (instance == null)
        {
            Debug.LogWarning("failed to instatiate model prefab into scene");
            return;
        }

        instance.name = "working__instance__delete__me";
        Debug.Log("about to do surgery on " + instance.name);

        var renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers?.Length == 0)
        {
            Debug.LogWarning("no renderers found for material remapping");
            return;
        }


        //TODO select an alternate shader, given variances from JSON, or user prefs???
        var shader = Shader.Find("HDRP/Lit");

        if (shader == null)
        {
            Debug.LogWarning("couldn't find shader");
            return;
        }

        var modelPath = AssetDatabase.GetAssetPath(fbxPrefab);
        var matPath = Path.GetDirectoryName(modelPath);
        matPath = Path.Combine(matPath, fbxPrefab.name + "Materials___");
        matPath = AssetDatabase.GenerateUniqueAssetPath(matPath);

        if (!Directory.Exists(matPath))
            Directory.CreateDirectory(matPath);

        foreach (var renderer in renderers)
        {
            var dict = new Dictionary<Material, Material>();
            foreach (var keyMat in renderer.sharedMaterials)
            {
                //Debug.Log("found: " + keyMat.name);


                // // // // // // // // // // // // // // // // // // // // // // // // // // // // // 
                // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // 




                Material nuMat = null;

                if (s_StandardMaterialCollection.ContainsKey(keyMat.name))
                {
                    nuMat = new Material(s_StandardMaterialCollection[keyMat.name]);

                    

                    FixupStandardBasedMaterial(ref nuMat, fbxPrefab, keyMat.name, data);

                }
                else
                {  
                        nuMat = new Material(shader);
                        if (nuMat == null)
                        {
                            Debug.LogWarning("couldn't construct material from " + shader.name);
                            continue;
                        }

                        nuMat.CopyPropertiesFromMaterial(keyMat);

                        // just copy the textures, colors and scalars that are appropriate given the base material type
                        DazMaterialPropertiesInfo info = new DazMaterialPropertiesInfo();
                        CustomizeMaterial(ref nuMat, info);

                     
                }

                

                // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // 
                // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // 



                //Debug.Log("obj path " + path);
                AssetDatabase.CreateAsset(nuMat, matPath + "/" + keyMat.name + "_Replaced.mat");

                dict.Add(keyMat, nuMat);

            }

            //foreach (var key in dict.Keys)
            //{
            //    Debug.Log("pair = " + key.name + " == " + dict[key].name);
            //}

            //remap the meshes in the fbx prefab to the value materials in dict
            var count = renderer.sharedMaterials.Length;
            var copy = new Material[count]; //makes a copy
            for (int i = 0; i < count; i++)
            {
                var key = renderer.sharedMaterials[i];
                Debug.Log("remapping: " + renderer.sharedMaterials[i].name +" to " + dict[key].name);
                copy[i] = dict[key];//fill copy
            }

            renderer.sharedMaterials = copy;


        }





        //material.name = "MonkeyMaterial";
        //var path = AssetDatabase.GetAssetPath(obj);
        //Debug.Log("obj path " + path);
        //AssetDatabase.CreateAsset(material, path);
        //AssetDatabase.AddObjectToAsset(material, obj);
        //AssetDatabase.ImportAsset(path);
        //Debug.Log("material path " + AssetDatabase.GetAssetPath(material));

        //write the prefab to the asset database
        // Make sure the file name is unique, in case an existing Prefab has the same name.
        var nuPrefabPathPath = Path.GetDirectoryName(modelPath);
        nuPrefabPathPath = Path.Combine(nuPrefabPathPath, fbxPrefab.name + "Prefabs___");
        nuPrefabPathPath = AssetDatabase.GenerateUniqueAssetPath(nuPrefabPathPath);
        if (!Directory.Exists(nuPrefabPathPath))
            Directory.CreateDirectory(nuPrefabPathPath);

        nuPrefabPathPath += "/" + fbxPrefab.name + "_Replaced.prefab";

        // Create the new Prefab.
        PrefabUtility.SaveAsPrefabAssetAndConnect(instance, nuPrefabPathPath, InteractionMode.AutomatedAction);
        
        //PrefabUtility.CreatePrefab(matPath+"/wackyprefab", instance);

        Debug.Log("prefab path " + nuPrefabPathPath);



        //DestroyImmediate(instance);
    }

    private void FixupStandardBasedMaterial(ref Material nuMat, GameObject fbxPrefab, string key, DTUData data)
    {
        //todo need fixup missing textures from the json
        Debug.LogWarning("dtuData has " + data.Materials.Count + " materials ");


        var modelPath = AssetDatabase.GetAssetPath(fbxPrefab);
        var nuTexturePath = Path.GetDirectoryName(modelPath);
        nuTexturePath = BuildUnityPath(nuTexturePath, fbxPrefab.name + "Textures___");
        nuTexturePath = AssetDatabase.GenerateUniqueAssetPath(nuTexturePath);


        //walk data until find a material named with key
        foreach (var material in data.Materials)
        {
            if (material.MaterialName == key && false) //TODO hack to bypass unfinished fn
            {
                //walk properties and work on any with a texture path
                foreach(var property in material.Properties)
                {
                    if (!string.IsNullOrEmpty(property.Texture))
                    {
                        //and the daz folder has that texture 
                        if (File.Exists(property.Texture))
                        {
                            //copy it into the local textures folder
                            if (!Directory.Exists(nuTexturePath))
                                Directory.CreateDirectory(nuTexturePath);

                            var nuTextureName = BuildUnityPath(nuTexturePath, Path.GetFileName(property.Texture));


                            //TODO-----------------------------
                            //todo some diffuse maps are jpg with no alpha channel, 
                            //instead use the FBX's embedded/collected texture which already has alpha channel, 
                            //test whether that material already has a valid diffuse color texture
                            //if so, reimport that with the importer options below


                            //copy the texture file from the daz folder to nuTexturePath
                            File.Copy(property.Texture, nuTextureName);

                            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(nuTextureName);
                            if (importer != null)
                            {
                                //todo twiddle other switches here, before the reimport happens only once
                                importer.alphaIsTransparency = KeyToTransparency(key);
                                importer.alphaSource = KeyToAlphaSource(key); 
                                importer.convertToNormalmap = KeyToNormalMap(key);
                                importer.heightmapScale = KeyToHeightmapScale(key);
                                importer.normalmapFilter = KeyToNormalMapFilter(key);
                                importer.wrapMode = KeyToWrapMode(key);

                                importer.SaveAndReimport();
                            }
                            else
                                Debug.LogWarning("texture " + nuTextureName + " is not a project asset.");

                            

                        }
                    }
                }

                  
            }
        }

    }

    private TextureImporterAlphaSource KeyToAlphaSource(string key)
    {
        throw new NotImplementedException();
    }

    private TextureWrapMode KeyToWrapMode(string key)
    {
        throw new NotImplementedException();
    }

    private TextureImporterNormalFilter KeyToNormalMapFilter(string key)
    {
        throw new NotImplementedException();
    }

    private float KeyToHeightmapScale(string key)
    {
        throw new NotImplementedException();
    }

    private bool KeyToNormalMap(string key)
    {
        throw new NotImplementedException();
    }

    private bool KeyToTransparency(string key)
    {
        throw new NotImplementedException();
    }

    private void CustomizeMaterial(ref Material material, DazMaterialPropertiesInfo info)
    {
        material.SetColor("_BaseColor", info.BaseColor); 
        material.SetFloat("_SurfaceType", info.Transparent ? 1 : 0); //0 == opaque, 1 == transparent

        Texture mainTexture = material.mainTexture;
        CustomizeTexture(ref mainTexture, info.Transparent);

        var normalMap = material.GetTexture("_NormalMap");
        if (!IsValidNormalMap(normalMap))
            material.SetTexture("_NormalMap", null);//nuke the invalid normal map, if its a mistake
            
 
        material.SetFloat("_Metallic", info.Metallic);
        material.SetFloat("_Smoothness", info.Smoothness);
        material.SetInt("_MaterialID", (int)info.MaterialType);
        material.SetFloat("_DoubleSidedEnable", info.DoubleSided ? 0 : 1);
    }


    void CustomizeTexture(ref Texture texture, bool alphaIsTransparent)
    {
        if (texture != null)
        {
            var texPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
            if (importer != null)
            {
                if (alphaIsTransparent && importer.DoesSourceTextureHaveAlpha())
                {
                    importer.alphaIsTransparency = true;
                }

                //todo twiddle other switches here, before the reimport happens only once

                importer.SaveAndReimport();
            }
            else
                Debug.LogWarning("texture " + texture.name + " is not a project asset.");

        }
        else
            Debug.LogWarning("null texture");
    }


    bool IsValidNormalMap(Texture normalMap)
    {
        if (normalMap == null)
            return false;

        var nmPath = AssetDatabase.GetAssetPath(normalMap);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(nmPath);
        if (importer != null)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            return settings.textureType == TextureImporterType.NormalMap;
        }
        else
            Debug.LogWarning("texture " + normalMap.name + " is not a project asset.");

        return true;
    }

    void OnFocus()
    {
        CheckRefresh();

        m_String = s_dazRootDir;
    }

    void OnEnable()
    {
        CheckRefresh();
    }



}

/*
  
for reference these are the properties of the hdrp lit shader

Shader "HDRenderPipeline/Lit"
{
    Properties
    {
        // Versioning of material to help for upgrading
        [HideInInspector] _HdrpVersion("_HdrpVersion", Float) = 1
 
        // Following set of parameters represent the parameters node inside the MaterialGraph.
        // They are use to fill a SurfaceData. With a MaterialGraph this should not exist.
 
        // Reminder. Color here are in linear but the UI (color picker) do the conversion sRGB to linear
        _BaseColor("BaseColor", Color) = (1,1,1,1)
        _BaseColorMap("BaseColorMap", 2D) = "white" {}
 
        _Metallic("_Metallic", Range(0.0, 1.0)) = 0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 1.0
        _MaskMap("MaskMap", 2D) = "white" {}
        _SmoothnessRemapMin("SmoothnessRemapMin", Float) = 0.0
        _SmoothnessRemapMax("SmoothnessRemapMax", Float) = 1.0
        _AORemapMin("AORemapMin", Float) = 0.0
        _AORemapMax("AORemapMax", Float) = 1.0
 
        _NormalMap("NormalMap", 2D) = "bump" {}     // Tangent space normal map
        _NormalMapOS("NormalMapOS", 2D) = "white" {} // Object space normal map - no good default value
        _NormalScale("_NormalScale", Range(0.0, 2.0)) = 1
 
        _BentNormalMap("_BentNormalMap", 2D) = "bump" {}
        _BentNormalMapOS("_BentNormalMapOS", 2D) = "white" {}
 
        _HeightMap("HeightMap", 2D) = "black" {}
        // Caution: Default value of _HeightAmplitude must be (_HeightMax - _HeightMin) * 0.01
        // Those two properties are computed from the ones exposed in the UI and depends on the displaement mode so they are separate because we don't want to lose information upon displacement mode change.
        [HideInInspector] _HeightAmplitude("Height Amplitude", Float) = 0.02 // In world units. This will be computed in the UI.
        [HideInInspector] _HeightCenter("Height Center", Range(0.0, 1.0)) = 0.5 // In texture space
 
        [Enum(MinMax, 0, Amplitude, 1)] _HeightMapParametrization("Heightmap Parametrization", Int) = 0
        // These parameters are for vertex displacement/Tessellation
        _HeightOffset("Height Offset", Float) = 0
        // MinMax mode
        _HeightMin("Heightmap Min", Float) = -1
        _HeightMax("Heightmap Max", Float) = 1
        // Amplitude mode
        _HeightTessAmplitude("Amplitude", Float) = 2.0 // in Centimeters
        _HeightTessCenter("Height Center", Range(0.0, 1.0)) = 0.5 // In texture space
 
        // These parameters are for pixel displacement
        _HeightPoMAmplitude("Height Amplitude", Float) = 2.0 // In centimeters
 
        _DetailMap("DetailMap", 2D) = "black" {}
        _DetailAlbedoScale("_DetailAlbedoScale", Range(0.0, 2.0)) = 1
        _DetailNormalScale("_DetailNormalScale", Range(0.0, 2.0)) = 1
        _DetailSmoothnessScale("_DetailSmoothnessScale", Range(0.0, 2.0)) = 1
 
        _TangentMap("TangentMap", 2D) = "bump" {}
        _TangentMapOS("TangentMapOS", 2D) = "white" {}
        _Anisotropy("Anisotropy", Range(-1.0, 1.0)) = 0
        _AnisotropyMap("AnisotropyMap", 2D) = "white" {}
 
        _DiffusionProfile("Diffusion Profile", Int) = 0
        _SubsurfaceMask("Subsurface Radius", Range(0.0, 1.0)) = 1.0
        _SubsurfaceMaskMap("Subsurface Radius Map", 2D) = "white" {}
        _Thickness("Thickness", Range(0.0, 1.0)) = 1.0
        _ThicknessMap("Thickness Map", 2D) = "white" {}
        _ThicknessRemap("Thickness Remap", Vector) = (0, 1, 0, 0)
 
        _IridescenceThickness("Iridescence Thickness", Range(0.0, 1.0)) = 1.0
        _IridescenceThicknessMap("Iridescence Thickness Map", 2D) = "white" {}
        _IridescenceThicknessRemap("Iridescence Thickness Remap", Vector) = (0, 1, 0, 0)
        _IridescenceMask("Iridescence Mask", Range(0.0, 1.0)) = 1.0
        _IridescenceMaskMap("Iridescence Mask Map", 2D) = "white" {}
 
        _CoatMask("Coat Mask", Range(0.0, 1.0)) = 0.0
        _CoatMaskMap("CoatMaskMap", 2D) = "white" {}
 
        [ToggleUI] _EnergyConservingSpecularColor("_EnergyConservingSpecularColor", Float) = 1.0
        _SpecularColor("SpecularColor", Color) = (1, 1, 1, 1)
        _SpecularColorMap("SpecularColorMap", 2D) = "white" {}

 */
