using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.Collections.Generic;
using System;

[ScriptedImporter(1, "dtu", 0x7FFFFFFF)]
public class Daz3DDTUImporter : ScriptedImporter
{
    [Serializable]
    public class ImportEventRecord
    {
        public string Text = "kjas hdfkjah sdkfjha lksdjfh aksjfh lkasjdfh kajsd fhkajsdfh kjasdh";
        public DateTime Timestamp = DateTime.Now;
        public string Path = "C:\\Monkey\\Baboon\\WikhershamBrothers";
    }

    public static Queue<ImportEventRecord> EventQueue = new Queue<ImportEventRecord>();


    public override void OnImportAsset(AssetImportContext ctx)
    {
        //Debug.LogWarning("OnImportAsset: " + ctx.assetPath);
        Daz3DBridge bridge = EditorWindow.GetWindow(typeof(Daz3DBridge)) as Daz3DBridge;
        if (bridge == null)
        {

            var consoleType = System.Type.GetType("ConsoleWindow,UnityEditor.dll");
            bridge = EditorWindow.CreateWindow<Daz3DBridge>(new[] { consoleType }) as Daz3DBridge;
            ////typeof(ConsoleWindow)
        }

        ImportEventRecord record = new ImportEventRecord()
        {
            Path = ctx.assetPath
        };

        EventQueue.Enqueue(record);

        while (EventQueue.Count > 100)
        {
            EventQueue.Dequeue();
        }

        if (bridge)
        {
               
             
            bridge.Process(record.Path);
        }

        //SurveyAllWindows();
    }

    //void SurveyAllWindows()
    //{
    //    EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
         
    //    for (int i = 0; i < allWindows.Length; i++)
    //    {
    //        Debug.Log(allWindows[i] );
             
    //    }
        

    //}
}