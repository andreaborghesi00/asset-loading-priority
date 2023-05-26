using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreamingPriorityTool
{
    public class SceneRetriever
    {
        public SceneRetriever() { }

        /**
         * returns every scene in the project
         */
        public static List<string> ListScenes()
        {
            List<string> scenePaths = new List<string>();
            foreach (string file in GetFiles(Application.dataPath /** + "/Scenes" */))
            {
                if (Path.GetExtension(file).Equals(".unity"))
                {
                    string path = Path.GetRelativePath(Application.dataPath, file);
                    scenePaths.Add($"Assets\\{path}");
                }
            }
            return scenePaths;
        }

        /**
         * returns only scenes containing {filter} in its file name
         */
        public static List<string> ListScenes(string filter)
        {
            return ListScenes().Where((scene) => scene.Contains(filter)).ToList();
        }

        public static List<string> ListArtScenes()
        {
            return ListScenes("_art");
        }


        /**
         * retrieves every file recursively from the directory at path {path}
         */
        private static IEnumerable<string> GetFiles(string path) 
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);

            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                        queue.Enqueue(subDir);
                }
                catch (Exception e) { Debug.LogError(e); }

                string[] files = null;

                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception e) { Debug.LogError(e); }

                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                        yield return files[i];
                }
            }
        }
    }
}
