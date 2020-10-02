using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Daz3D
{

    [ScriptedImporter(1, "dtu", 0x7FFFFFFF)]
    public class Daz3DDTUImporter : ScriptedImporter
    {
        [Serializable]
        public class ImportEventRecord
        {
            public DateTime Timestamp = DateTime.Now;
            public struct Token
            {
                public string Text;
                public UnityEngine.Object Selectable;
                public bool EndLine;
            }

            public List<Token> Tokens = new List<Token>();

            internal void AddToken(string str, UnityEngine.Object obj = null, bool endline = false)
            {
                Tokens.Add(new Token() { Text = str, Selectable = obj, EndLine = endline });
            }
        }

        public static Queue<ImportEventRecord> EventQueue = new Queue<ImportEventRecord>();
        private static Dictionary<string, Material> s_StandardMaterialCollection = new Dictionary<string, Material>();

        public override void OnImportAsset(AssetImportContext ctx)
        {
            //ImportDTU(ctx.assetPath);



            var dtuPath = ctx.assetPath;
            var fbxPath = dtuPath.Replace(".dtu", ".fbx");

            Import(dtuPath, fbxPath);

            var icon = Texture2D.blackTexture;  
            ctx.AddObjectToAsset("booboo", ctx.mainObject, icon);  
            ctx.SetMainObject(ctx.mainObject);  
        }

        [MenuItem("Daz3D/Create Unity Prefab from selected DTU")]
        public static void MenuItemConvert()
        {
            var activeObject = Selection.activeObject;
            var dtuPath = AssetDatabase.GetAssetPath(activeObject);
            var fbxPath = dtuPath.Replace(".dtu", ".fbx");

            Import(dtuPath, fbxPath); 

        }

        [MenuItem("Daz3D/Extract materials from selected DTU", true)]
        [MenuItem("Assets/Daz3D/Create Unity Prefab", true)]
        [MenuItem("Daz3D/Create Unity Prefab from selected DTU", true)]
        [MenuItem("Assets/Daz3D/Extract materials", true)]
        public static bool ValidateDTUSelected()
        {
            var obj = Selection.activeObject;

            // Return false if no transform is selected.
            if (obj == null)
                return false;

            return (AssetDatabase.GetAssetPath(obj).ToLower().EndsWith(".dtu"));
        }

        public class MaterialMap
        {
            public MaterialMap(string path)
            {
                Path = path;
            }

            public void AddMaterial(UnityEngine.Material material)
            {
                if (material && !Map.ContainsKey(material.name))
                    Map.Add(material.name, material);
            }
            public string Path { get; set; }
            public Dictionary<string, UnityEngine.Material> Map = new Dictionary<string, UnityEngine.Material>();
        }



        public static void Import(string dtuPath, string fbxPath)
        {
            DazCoroutine.StartCoroutine(ImportRoutine(dtuPath, fbxPath));
        }


        private static MaterialMap _map = null; 

        private static IEnumerator ImportRoutine(string dtuPath, string fbxPath)
        {
            Daz3DBridge.Progress = .1f;

            while (!IrayShadersReady())
            {
                yield return new WaitForSeconds(.1f);
            }

            _map = new MaterialMap(dtuPath);
            
            ImportDTU(dtuPath);

            var platform = DazFigurePlatform.Genesis8; //todo discover the platform while parsing the dtu, above

            GeneratePrefabFromFBX(fbxPath, platform);

            _map = null;

            yield break;
        }

        private static bool IrayShadersReady()
        {
            if (Shader.Find(DTU_Constants.shaderNameIrayOpaque) == null ||
                Shader.Find(DTU_Constants.shaderNameIrayTransparent) == null ||
                Shader.Find(DTU_Constants.shaderNameIraySkin) == null ||
                Shader.Find(DTU_Constants.shaderNameHair) == null)
                return false;

            return true;
        }

        private const bool ENDLINE = true;

        public static void ImportDTU(string path)
        {
            Debug.LogWarning("ImportDTU for " + path + "=======================================");

            ImportEventRecord record = new ImportEventRecord();
            EventQueue.Enqueue(record);

            var dtu = DTUConverter.ParseDTUFile(path);

            var dtuObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

            record.AddToken("Imported DTU file: " + path);
            record.AddToken(dtuObject.name, dtuObject, ENDLINE);

            //UnityEngine.Debug.Log("DTU: " + dtu.AssetName + " contains: " + dtu.Materials.Count + " materials");

            record.AddToken("Generated materials: ");
            foreach (var dtuMat in dtu.Materials)
            {
                var material = dtu.ConvertToUnity(dtuMat);
                _map.AddMaterial(material);

                record.AddToken(material.name, material);
            }
            record.AddToken(" based on DTU file.", null, ENDLINE);


            Daz3DBridge bridge = EditorWindow.GetWindow(typeof(Daz3DBridge)) as Daz3DBridge;
            if (bridge == null)
            {
                var consoleType = Type.GetType("ConsoleWindow,UnityEditor.dll");
                bridge = EditorWindow.CreateWindow<Daz3DBridge>(new[] { consoleType });
            }

            bridge?.Focus();

            //just a safeguard to keep the history data at a managable size (100 records)
            while (EventQueue.Count > 100)
            {
                EventQueue.Dequeue();
            }

        }


        
        IEnumerator boo()
        {
            int count = 0;

            while (count < 1000)
            {
                Debug.Log(count++);
                yield return new WaitForSeconds(1);
            }
            yield break;
        }


        enum MaterialID //these positions map to the bitflags in the compiled HDRP lit shader
        {
            SSS = 0,
            Standard = 1,
            Anisotropy = 2,
            Iridescence = 3,
            SpecularColor = 4,
            Translucent = 5
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

        public static void GeneratePrefabFromFBX(string fbxPath, DazFigurePlatform platform)
        {
            var fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

            if (fbxPrefab == null)
            {
                Debug.LogWarning("null prefab ");
                return;
            }

            if (PrefabUtility.GetPrefabAssetType(fbxPrefab) != PrefabAssetType.Model)
            {
                Debug.LogWarning(fbxPath + " is not a model prefab ");
                return;
            }

            // TODO update progress graph


            var record = new ImportEventRecord();

            ModelImporter importer = GetAtPath(fbxPath) as ModelImporter;
            if (importer)
            {
                var description = importer.humanDescription;
                DescribeHumanJointsForFigure(ref description, platform);

                importer.humanDescription = description;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

                AssetDatabase.WriteImportSettingsIfDirty(fbxPath);
                AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);

                record.AddToken("Automated Mecanim avatar setup for " + fbxPrefab.name + ": ");

                //a little dance to get the avatar just reimported
                var allAvatars = Resources.FindObjectsOfTypeAll(typeof(Avatar));
                var avatar = Array.Find(allAvatars, element => element.name.StartsWith(fbxPrefab.name));
                if (avatar)
                    record.AddToken(avatar.name, avatar, ENDLINE);

            }
            else
            {
                Debug.LogWarning("Could not acquire importer for " + fbxPath + " ...could not automatically configure humanoid avatar.");
                record.AddToken("Could not acquire importer for " + fbxPath + " ...could not automatically configure humanoid avatar.", null, ENDLINE);

            }

            //TODO update progress graph
            EventQueue.Enqueue(record);


            var workingInstance = Instantiate(fbxPrefab);

            if (workingInstance == null)
            {
                Debug.LogWarning("failed to instatiate model prefab into scene");
                return;
            }

            workingInstance.name = "Daz3d_" + fbxPrefab.name;
            //Debug.Log("about to do surgery on " + instance.name);

            var renderers = workingInstance.GetComponentsInChildren<Renderer>();
            if (renderers?.Length == 0)
            {
                Debug.LogWarning("no renderers found for material remapping");
                return;
            }

            var modelPath = AssetDatabase.GetAssetPath(fbxPrefab);


            foreach (var renderer in renderers)
            {
                var dict = new Dictionary<Material, Material>();

                if (renderer.name.ToLower().Contains("eyelashes"))
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                foreach (var keyMat in renderer.sharedMaterials)
                {
                    var key = keyMat.name;

                    key = Daz3D.Utilities.ScrubKey(key);

                    Material nuMat = null;

                    if (_map != null && _map.Map.ContainsKey(key))
                    {
                        nuMat = _map.Map[key];// the preferred uber/iRay based material generated by the DTUConverter
                    }
                    else if (s_StandardMaterialCollection.ContainsKey(key))
                    {
                        nuMat = new Material(s_StandardMaterialCollection[key]);
                        //FixupStandardBasedMaterial(ref nuMat, fbxPrefab, keyMat.name, data);
                    }
                    else
                    {
                        var shader = Shader.Find("HDRP/Lit");

                        if (shader == null)
                        {
                            Debug.LogWarning("couldn't find HDRP/Lit shader");
                            continue;
                        }

                        nuMat = new Material(shader);
                        nuMat.CopyPropertiesFromMaterial(keyMat);

                        // just copy the textures, colors and scalars that are appropriate given the base material type
                        //DazMaterialPropertiesInfo info = new DazMaterialPropertiesInfo();
                        //CustomizeMaterial(ref nuMat, info);

                        var matPath = Path.GetDirectoryName(modelPath);
                        matPath = Path.Combine(matPath, fbxPrefab.name + "Daz3D_Materials");
                        matPath = AssetDatabase.GenerateUniqueAssetPath(matPath);

                        if (!Directory.Exists(matPath))
                            Directory.CreateDirectory(matPath);

                        //Debug.Log("obj path " + path);
                        AssetDatabase.CreateAsset(nuMat, matPath + "/Daz3D_" + keyMat.name + ".mat");

                    }

                    dict.Add(keyMat, nuMat);

                }

                //remap the meshes in the fbx prefab to the value materials in dict
                var count = renderer.sharedMaterials.Length;
                var copy = new Material[count]; //makes a copy
                for (int i = 0; i < count; i++)
                {
                    var key = renderer.sharedMaterials[i];
                    //Debug.Log("remapping: " + renderer.sharedMaterials[i].name + " to " + dict[key].name);
                    copy[i] = dict[key];//fill copy
                }

                renderer.sharedMaterials = copy;//overwrite sharedMaterials, because set indexer assigns to a copy

            }

            //write the prefab to the asset database
            // Make sure the file name is unique, in case an existing Prefab has the same name.
            var nuPrefabPathPath = Path.GetDirectoryName(modelPath);
            nuPrefabPathPath = Path.Combine(nuPrefabPathPath, fbxPrefab.name + "_Prefab");
            nuPrefabPathPath = AssetDatabase.GenerateUniqueAssetPath(nuPrefabPathPath);
            if (!Directory.Exists(nuPrefabPathPath))
                Directory.CreateDirectory(nuPrefabPathPath);

            nuPrefabPathPath += "/Daz3D_" + fbxPrefab.name + ".prefab";

            // For future refreshment
            var component = workingInstance.AddComponent<Daz3DInstance>();
            component.SourceFBX = fbxPrefab;

            // Create the new Prefab.
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(workingInstance, nuPrefabPathPath, InteractionMode.AutomatedAction);
            Selection.activeGameObject = prefab;

            //now, seek other instance(s) in the scene having been sourced from this fbx asset
            var otherInstances = FindObjectsOfType<Daz3DInstance>();
            int foundCount = 0;

            var resultingInstance = workingInstance;

            foreach( var otherInstance in otherInstances)
            {
                if (otherInstance == component)//ignore this working instance
                    continue;
                if (otherInstance.SourceFBX != fbxPrefab)//ignore instances of other assets
                    continue;

                foundCount++;

                //for any found that flag ReplaceOnImport, delete that instance and replace with a copy of 
                //this one, at their respective transforms
                if (otherInstance.ReplaceOnImport)
                {
                    var xform = otherInstance.transform;
                    var replacementInstance = Instantiate(workingInstance, xform.position, xform.rotation, xform.parent);
                    resultingInstance = replacementInstance;
                }
            }

            //if no prior instances found, then don't destroy this instance
            //since it appears to be the first one to arrive
            if (foundCount > 0)
                DestroyImmediate(workingInstance);


            ImportEventRecord pfbRecord = new ImportEventRecord();
            pfbRecord.AddToken("Created Unity Prefab: ");
            pfbRecord.AddToken(prefab.name, prefab);
            pfbRecord.AddToken(" and an instance in the scene: ");
            pfbRecord.AddToken(resultingInstance.name, resultingInstance, ENDLINE);
            EventQueue.Enqueue(pfbRecord);


        }



        public enum DazFigurePlatform
        {
            Genesis8,
            Genesis3,
            Genesis2,
            Victoria,
            Genesis,
            Michael,
            TheFreak,
            Victoria4,
            Victoria4Elite,
            Michael4,
            Michael4Elite,
            Stephanie4,
            Aiko4
        }
        private static void DescribeHumanJointsForFigure(ref HumanDescription description, DazFigurePlatform figure)
        {
            var map = GetJointMapForFigure(figure);

            HumanBone[] humanBones = new HumanBone[HumanTrait.BoneName.Length];
            int mapIdx = 0;

            foreach (var humanBoneName in HumanTrait.BoneName)
            {
                if (map.ContainsKey(humanBoneName))
                {
                    HumanBone humanBone = new HumanBone();
                    humanBone.humanName = humanBoneName;
                    humanBone.boneName = map[humanBoneName];
                    humanBone.limit.useDefaultValues = true; //todo get limits from dtu?
                    humanBones[mapIdx++] = humanBone;
                }
            }

            description.human = humanBones;
        }

        private static Dictionary<string, string> GetJointMapForFigure(DazFigurePlatform figure)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            switch (figure)
            {
                case DazFigurePlatform.Genesis8:
                case DazFigurePlatform.Genesis3:

                    //note: Genesis 3 finger bones have "Carpal#" parents

                    //Body/Body (Gen8)
                    map["Hips"] = "hip";
                    map["Spine"] = "abdomenUpper";
                    map["Chest"] = "chestLower";
                    map["UpperChest"] = "chestUpper";

                    //Body/Left Arm (Gen8)
                    map["LeftShoulder"] = "lCollar";
                    map["LeftUpperArm"] = "lShldrBend";
                    map["LeftLowerArm"] = "lForearmBend";
                    map["LeftHand"] = "lHand";

                    //Body/Right Arm (Gen8)
                    map["RightShoulder"] = "rCollar";
                    map["RightUpperArm"] = "rShldrBend";
                    map["RightLowerArm"] = "rForearmBend";
                    map["RightHand"] = "rHand";

                    //Body/Left Leg (Gen8)
                    map["LeftUpperLeg"] = "lThighBend";
                    map["LeftLowerLeg"] = "lShin";
                    map["LeftFoot"] = "lFoot";
                    map["LeftToes"] = "lToe";

                    //Body/Right Leg (Gen8)
                    map["RightUpperLeg"] = "rThighBend";
                    map["RightLowerLeg"] = "rShin";
                    map["RightFoot"] = "rFoot";
                    map["RightToes"] = "rToe";

                    //Head (Gen8)
                    map["Neck"] = "neckLower";
                    map["Head"] = "head";
                    map["LeftEye"] = "lEye";
                    map["RightEye"] = "rEye";
                    map["Jaw"] = "lowerJaw";

                    //Left Hand (Gen8)
                    map["Left Thumb Proximal"] = "lThumb1";
                    map["Left Thumb Intermediate"] = "lThumb2";
                    map["Left Thumb Distal"] = "lThumb3";
                    map["Left Index Proximal"] = "lIndex1";
                    map["Left Index Intermediate"] = "lIndex2";
                    map["Left Index Distal"] = "lIndex3";
                    map["Left Middle Proximal"] = "lMid1";
                    map["Left Middle Intermediate"] = "lMid2";
                    map["Left Middle Distal"] = "lMid3";
                    map["Left Ring Proximal"] = "lRing1";
                    map["Left Ring Intermediate"] = "lRing2";
                    map["Left Ring Distal"] = "lRing3";
                    map["Left Little Proximal"] = "lPinky1";
                    map["Left Little Intermediate"] = "lPinky2";
                    map["Left Little Distal"] = "lPinky3";

                    //Right Hand (Gen8)
                    map["Right Thumb Proximal"] = "rThumb1";
                    map["Right Thumb Intermediate"] = "rThumb2";
                    map["Right Thumb Distal"] = "rThumb3";
                    map["Right Index Proximal"] = "rIndex1";
                    map["Right Index Intermediate"] = "rIndex2";
                    map["Right Index Distal"] = "rIndex3";
                    map["Right Middle Proximal"] = "rMid1";
                    map["Right Middle Intermediate"] = "rMid2";
                    map["Right Middle Distal"] = "rMid3";
                    map["Right Ring Proximal"] = "rRing1";
                    map["Right Ring Intermediate"] = "rRing2";
                    map["Right Ring Distal"] = "rRing3";
                    map["Right Little Proximal"] = "rPinky1";
                    map["Right Little Intermediate"] = "rPinky2";
                    map["Right Little Distal"] = "rPinky3";

                    break;

                
                     

                default:
                    map["Chest"] = "___"; //bogus
                    map["Head"] = "___"; //bogus
                    map["Hips"] = "___"; //bogus
                    map["LeftFoot"] = "___"; //bogus
                    map["LeftHand"] = "___"; //bogus
                    map["LeftLowerArm"] = "___"; //bogus
                    map["LeftLowerLeg"] = "___"; //bogus
                    map["LeftShoulder"] = "___"; //bogus
                    map["LeftUpperArm"] = "___"; //bogus
                    map["LeftUpperLeg"] = "___"; //bogus
                    map["RightFoot"] = "___"; //bogus
                    map["RightHand"] = "___"; //bogus
                    map["RightLowerArm"] = "___"; //bogus
                    map["RightLowerLeg"] = "___"; //bogus
                    map["RightShoulder"] = "___"; //bogus
                    map["RightUpperArm"] = "___"; //bogus
                    map["RightUpperLeg"] = "___";//bogus
                    map["Spine"] = "___"; //bogus

                    break;

            }

            return map; 
        }

        private void FixupStandardBasedMaterial(ref Material nuMat, GameObject fbxPrefab, string key/*, DTUData data*/)
        {
            ////todo need fixup missing textures from the json
            //Debug.LogWarning("dtuData has " + data.Materials.Count + " materials ");

            //var modelPath = AssetDatabase.GetAssetPath(fbxPrefab);
            //var nuTexturePath = Path.GetDirectoryName(modelPath);
            //nuTexturePath = BuildUnityPath(nuTexturePath, fbxPrefab.name + "Textures___");
            //nuTexturePath = AssetDatabase.GenerateUniqueAssetPath(nuTexturePath);

            ////walk data until find a material named with key
            //foreach (var material in data.Materials)
            //{
            //    if (material.MaterialName == key && false) //TODO hack to bypass unfinished fn
            //    {
            //        //walk properties and work on any with a texture path
            //        foreach (var property in material.Properties)
            //        {
            //            if (!string.IsNullOrEmpty(property.Texture))
            //            {
            //                //and the daz folder has that texture 
            //                if (File.Exists(property.Texture))
            //                {
            //                    //copy it into the local textures folder
            //                    if (!Directory.Exists(nuTexturePath))
            //                        Directory.CreateDirectory(nuTexturePath);

            //                    var nuTextureName = BuildUnityPath(nuTexturePath, Path.GetFileName(property.Texture));

            //                    //TODO-----------------------------
            //                    //todo some diffuse maps are jpg with no alpha channel, 
            //                    //instead use the FBX's embedded/collected texture which already has alpha channel, 
            //                    //test whether that material already has a valid diffuse color texture
            //                    //if so, reimport that with the importer options below

            //                    //copy the texture file from the daz folder to nuTexturePath
            //                    File.Copy(property.Texture, nuTextureName);

            //                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(nuTextureName);
            //                    if (importer != null)
            //                    {
            //                        //todo twiddle other switches here, before the reimport happens only once
            //                        importer.alphaIsTransparency = KeyToTransparency(key);
            //                        importer.alphaSource = KeyToAlphaSource(key);
            //                        importer.convertToNormalmap = KeyToNormalMap(key);
            //                        importer.heightmapScale = KeyToHeightmapScale(key);
            //                        importer.normalmapFilter = KeyToNormalMapFilter(key);
            //                        importer.wrapMode = KeyToWrapMode(key);

            //                        importer.SaveAndReimport();
            //                    }
            //                    else
            //                    {
            //                        Debug.LogWarning("texture " + nuTextureName + " is not a project asset.");
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
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

        //private void CustomizeMaterial(ref Material material, DazMaterialPropertiesInfo info)
        //{
        //    material.SetColor("_BaseColor", info.BaseColor);
        //    material.SetFloat("_SurfaceType", info.Transparent ? 1 : 0); //0 == opaque, 1 == transparent

        //    Texture mainTexture = material.mainTexture;
        //    CustomizeTexture(ref mainTexture, info.Transparent);

        //    var normalMap = material.GetTexture("_NormalMap");
        //    if (!IsValidNormalMap(normalMap))
        //        material.SetTexture("_NormalMap", null);//nuke the invalid normal map, if its a mistake


        //    material.SetFloat("_Metallic", info.Metallic);
        //    material.SetFloat("_Smoothness", info.Smoothness);
        //    material.SetInt("_MaterialID", (int)info.MaterialType);
        //    material.SetFloat("_DoubleSidedEnable", info.DoubleSided ? 0 : 1);
        //}


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



        // Validated menu item.
        // Add a menu item named "Log Selected Transform Name" to MyMenu in the menu bar.
        // We use a second function to validate the menu item
        // so it will only be enabled if we have a transform selected.
        [MenuItem("Assets/Daz3D/Create Unity Prefab")]
        static void DoStuffToSelectedDTU()
        {
            CreateDTUPrefab(Selection.activeObject);
            Debug.Log("Selected Transform is on " + Selection.activeTransform.gameObject.name + ".");
        }

        //// Validate the menu item defined by the function above.
        //// The menu item will be disabled if this function returns false.
        //[MenuItem("Assets/Daz3D/Create Unity Prefab", true)]
        //static bool ValidateDTUSelected2()
        //{
        //    return ValidateDTUSelected();
        //}

        private static void CreateDTUPrefab(UnityEngine.Object activeObject)
        {
            if (activeObject)
            {
                var dtuPath = AssetDatabase.GetAssetPath(activeObject);
                var fbxPath = dtuPath.Replace(".dtu", ".fbx");

                Import(dtuPath, fbxPath);
            }
        }


    }

}