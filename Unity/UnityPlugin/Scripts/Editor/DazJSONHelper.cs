using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Daz3D
{
    //    [Serializable]
    //    public class DTUData
    //    {
    //        //public string AssetName = "|||||||||||||||||"; //are these cruft from unreal dtu?
    //        //public string AssetType = "|||||||||||||||||";
    //        //public string FBXFile = "|||||||||||||||||";
    //        //public string ImportFolder = "|||||||||||||||||";

    //        public List<Material> Materials = new List<Material>();
    //        public List<Morph> Morphs = new List<Morph>();

    //        [Serializable]
    //        public class Material
    //        {
    //            public int Version = -999;
    //            public string AssetName = "~~~~~~~~";
    //            public string MaterialName = "~~~~~~~~";
    //            public string MaterialType = "~~~~~~~~";
    //            public string Value = "~~~~~~~~";
    //            public List<Property> Properties = new List<Property>();


    //            [Serializable]
    //            public class Property
    //            {
    //                public string Name = "@@@@@";
    //                public string Value = "@@@@@";
    //                public string DataType = "@@@@@";
    //                public string Texture = "@@@@@";
    //            }

    //        }

    //        [Serializable]
    //        public class Morph
    //        {
    //            public string CTRLBodyBuilder;
    //        }
    //    }



    //}
    public static class DazJSONHelper
    {
        //    public static string FixKeySpaces(string input)
        //    {
        //        var output = input.Replace("Asset Name", "AssetName");
        //        output = output.Replace("Asset Type", "AssetType");
        //        output = output.Replace("Import Folder", "ImportFolder");
        //        output = output.Replace("FBX File", "FBXFile");

        //        output = output.Replace("Material Name", "MaterialName");
        //        output = output.Replace("Material Type", "MaterialType");
        //        output = output.Replace("Data Type", "DataType");

        //        return output;

        //    }

        public static string BuildUnityPath(string path, string subDir)
        {
            var result = Path.Combine(path, subDir).Replace("\\", "/"); //unity likes forward slashes in asset database paths;

            return result;
        }

        //    //    public static T[] FromJson<T>(string json)
        //    //    {
        //    //        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        //    //        return wrapper.Items;
        //    //    }

        //    //    public static string ToJson<T>(T[] array)
        //    //    {
        //    //        Wrapper<T> wrapper = new Wrapper<T>();
        //    //        wrapper.Items = array;
        //    //        return JsonUtility.ToJson(wrapper);
        //    //    }

        //    //    public static string ToJson<T>(T[] array, bool prettyPrint)
        //    //    {
        //    //        Wrapper<T> wrapper = new Wrapper<T>();
        //    //        wrapper.Items = array;
        //    //        return JsonUtility.ToJson(wrapper, prettyPrint);
        //    //    }

        //    //    [Serializable]
        //    //    private class Wrapper<T>
        //    //    {
        //    //        public T[] Items;
        //    //    }
    }
}


