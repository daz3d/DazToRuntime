using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using System.IO;

namespace Daz3D
{

    [ScriptedImporter(1, "dtu", 0x7FFFFFFF)]
    public class Daz3DDTUImporter : ScriptedImporter
    {
        [Serializable]
        public class ImportEventRecord
        {
            public string Text = string.Empty;
            public DateTime Timestamp = DateTime.Now;
            public string Path = string.Empty;
        }

        public static Queue<ImportEventRecord> EventQueue = new Queue<ImportEventRecord>();
        // // // // // // //
        private static Dictionary<string, Material> s_StandardMaterialCollection = new Dictionary<string, Material>();
        private static string s_dazRootDir = "NOT_SET_DAZROOTDIR";
        // // // // // // //


        public override void OnImportAsset(AssetImportContext ctx)
        {
            ImportDTU(ctx.assetPath);
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

        public static MaterialMap ImportDTU(string path)
        {
            var theMap = new MaterialMap(path);

            ImportEventRecord record = new ImportEventRecord()
            {
                Path = path 
            };

            var dtu = DTUConverter.ParseDTUFile(record.Path);

            UnityEngine.Debug.Log("DTU: " + dtu.AssetName + " contains: " + dtu.Materials.Count + " materials");
            foreach (var dtuMat in dtu.Materials)
            {
                var material = dtu.ConvertToUnity(dtuMat);
                theMap.AddMaterial(material);
            }

            Daz3DBridge bridge = EditorWindow.GetWindow(typeof(Daz3DBridge)) as Daz3DBridge;
            if (bridge == null)
            {
                var consoleType = System.Type.GetType("ConsoleWindow,UnityEditor.dll");
                bridge = EditorWindow.CreateWindow<Daz3DBridge>(new[] { consoleType });
            }
            if (bridge != null)
            {
                bridge.Focus();
            }

            EventQueue.Enqueue(record);

            while (EventQueue.Count > 100)
            {
                EventQueue.Dequeue();
            }

            return theMap;//to quote Time bandits
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





        // // // // // // //
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
        // // // // // // //




        //internal void Process(string relativePath)
        //{
        //    var jsonPath = relativePath;

        //    if (jsonPath.StartsWith("Assets/"))
        //        jsonPath = jsonPath.Replace("Assets/", "");

        //    jsonPath = BuildUnityPath(Application.dataPath, jsonPath);

        //    if (!File.Exists(jsonPath))
        //    {
        //        Debug.LogError(jsonPath + " does not exist");
        //        return;
        //    }

        //    //var dtuData = ParseJSON_DTU(jsonPath);

        //    var fbxPath = relativePath.Replace(".dtu", ".fbx");
        //    var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        //    if (fbx && PrefabUtility.GetPrefabAssetType(fbx) == PrefabAssetType.Model)
        //    {
        //        GeneratePrefabFromFBX(fbx);
        //    }

        //}














        //DTUData ParseJSON_DTU(string dtuPath)
        //{
        //    var json = File.ReadAllText(dtuPath);
        //    //todo expect json to be a very long string... validate for min length?

        //    //InitBaseMaterials();

        //    json = DazJSONHelper.FixKeySpaces(json);

        //    //Debug.Log(json);

        //    DTUData mo = JsonUtility.FromJson<DTUData>(json);

        //    DebugDumpDTUData(mo);

        //    return mo;
        //}

        //private static void DebugDumpDTUData(DTUData mo)
        //{
        //    foreach (var plim in mo.Materials)
        //    {
        //        Debug.Log(" Material.Version " + plim.Version);
        //        Debug.Log(" Material.Asset Name " + plim.AssetName);//useful
        //                                                            //Debug.Log(" Material.Asset Type " + plim.AssetType);//useful
        //        Debug.Log(" Material.Material Name " + plim.MaterialName);//useful
        //        Debug.Log(" Material.Material Type " + plim.MaterialType);
        //        //Debug.Log(" Material.Data Type " + plim.DataType);//useful

        //        for (int i = 0; i < plim.Properties.Count; i++)
        //        {
        //            var property = plim.Properties[i];
        //            Debug.Log(plim.MaterialName + " name " + property.Name + " value " + property.Value + " dataType " + property.DataType + " texture " + property.Texture);//useful
        //        }
        //    }
        //}

        public static void GeneratePrefabFromFBX(GameObject fbxPrefab, Daz3DDTUImporter.MaterialMap uberMap = null)
        {
            //InitBaseMaterials();

            if (fbxPrefab == null)
            {
                Debug.LogWarning("null prefab ");
                return;
            }

            if (PrefabUtility.GetPrefabAssetType(fbxPrefab) != PrefabAssetType.Model)
            {
                Debug.LogWarning("not a model prefab ");
                return;
            }

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

            var modelPath = AssetDatabase.GetAssetPath(fbxPrefab);


            foreach (var renderer in renderers)
            {
                var dict = new Dictionary<Material, Material>();

                foreach (var keyMat in renderer.sharedMaterials)
                {
                    var key = keyMat.name;

                    key = Daz3D.Utilities.ScrubKey(key);

                    Material nuMat = null;

                    if (uberMap != null && uberMap.Map.ContainsKey(key))
                    {
                        nuMat = uberMap.Map[key];// the preferred uber/iRay based material generated by the DTUConverter
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
                        matPath = Path.Combine(matPath, fbxPrefab.name + "Materials___");
                        matPath = AssetDatabase.GenerateUniqueAssetPath(matPath);

                        if (!Directory.Exists(matPath))
                            Directory.CreateDirectory(matPath);

                        //Debug.Log("obj path " + path);
                        AssetDatabase.CreateAsset(nuMat, matPath + "/" + keyMat.name + "_Replaced.mat");

                    }

                    dict.Add(keyMat, nuMat);

                }

                //remap the meshes in the fbx prefab to the value materials in dict
                var count = renderer.sharedMaterials.Length;
                var copy = new Material[count]; //makes a copy
                for (int i = 0; i < count; i++)
                {
                    var key = renderer.sharedMaterials[i];
                    Debug.Log("remapping: " + renderer.sharedMaterials[i].name + " to " + dict[key].name);
                    copy[i] = dict[key];//fill copy
                }

                renderer.sharedMaterials = copy;//overwrite sharedMaterials, because set indexer assigns to a copy

            }

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

            Debug.Log("prefab path " + nuPrefabPathPath);

            //DestroyImmediate(instance);
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


    }
}