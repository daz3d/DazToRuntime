using UnityEditor;
using UnityEngine;

namespace Daz3D
{
    /// <summary>
    /// A component to attach to the scene instance of the prefab created by the DTUImporter
    /// allows the instance to be "refreshed" with the newer prefab, between successive import sessions
    /// If the scene containing the instance is open and the fbx is fresher than the scene, then 
    /// a instance of the new prefab will replace the old
    /// TODO add a switch in the DTU JSON to override this behavior
    /// </summary>
    public class Daz3DInstance : MonoBehaviour
    {
        /// <summary>
        /// The FBX modelPrefab is used as the unifying key between the import session
        /// and the instance in the scene.
        /// </summary>
        public GameObject SourceFBX;

        [Tooltip("If true, future bridge imports will replace this instance in the scene, including any prefab overrides.")]
        public bool ReplaceOnImport = true;

        // // // // // // // // 


        //adds rightclick menu to project view
        //[MenuItem("Assets/MUNKEE/DoSomething")]
        //public static void Init()
        //{
        //    var file = Selection.activeObject.name;
        //    Debug.LogWarning(file);
        //}


        //// Add a menu item named "Do Something" to MyMenu in the menu bar.
        //[MenuItem("MyMenu/Do Something M U N K E E _____________")]
        //static void DoSomething()
        //{
        //    Debug.Log("Doing Something...");
        //}

        

        // Add a menu item named "Do Something with a Shortcut Key" to MyMenu in the menu bar
        // and give it a shortcut (ctrl-g on Windows, cmd-g on macOS).
        //[MenuItem("MyMenu/Do Something with a Shortcut Key %g M U N K E E _____________")]
        //static void DoSomethingWithAShortcutKey()
        //{
        //    Debug.Log("Doing something with a Shortcut Key...");
        //}

        // Add a menu item called "Double Mass" to a Rigidbody's context menu.
        //[MenuItem("CONTEXT/Rigidbody/Double Mass M U N K E E _____________")]
        //static void DoubleMass(MenuCommand command)
        //{
        //    Rigidbody body = (Rigidbody)command.context;
        //    body.mass = body.mass * 2;
        //    Debug.Log("Doubled Rigidbody's Mass to " + body.mass + " from Context Menu.");
        //}

        // Add a menu item to create custom GameObjects.
        // Priority 1 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarchy context menus.
        //[MenuItem("GameObject/M U N K E E _____________/Custom Game Object", false, 10)]
        //static void CreateCustomGameObject(MenuCommand menuCommand)
        //{
        //    // Create a custom game object
        //    GameObject go = new GameObject("Custom Game Object");
        //    // Ensure it gets reparented if this was a context click (otherwise does nothing)
        //    GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        //    // Register the creation in the undo system
        //    Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        //    Selection.activeObject = go;
        //}






    }
}