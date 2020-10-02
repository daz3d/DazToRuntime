using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Daz3D
{
    /// <summary>
    /// A compact coroutine manager for executing coroutines in the unity editor
    /// without the need for the optional editor coroutine package to be installed
    /// </summary>
    public static class DazCoroutine
    {
        private static List<IEnumerator> s_activeCoroutines = new List<IEnumerator>();
        private static int s_activeCoroutineIndex = 0;

        static DazCoroutine()
        {
            if (!IsEditorCoroutineActive())
                EditorApplication.update += ExecuteCoroutines;
        }

        public static IEnumerator StartCoroutine(IEnumerator coroutine)
        {
            s_activeCoroutines.Add(coroutine);
            return coroutine;
        }

        public static void StopCoroutine(IEnumerator coroutine)
        {
            if (s_activeCoroutines.Contains(coroutine))
                s_activeCoroutines.Remove(coroutine);
        }

        private static void ExecuteCoroutines()
        {
            if (s_activeCoroutines.Count > 0)
            {
                s_activeCoroutineIndex++;
                s_activeCoroutineIndex %= s_activeCoroutines.Count;

                if (!s_activeCoroutines[s_activeCoroutineIndex].MoveNext())
                    s_activeCoroutines.RemoveAt(s_activeCoroutineIndex);
            }
        }

        private static bool IsEditorCoroutineActive()
        {
            var list = EditorApplication.update.GetInvocationList();
            return Array.Exists(list, element => element.Method.Name == "ExecuteCoroutines");
        }
    }

}