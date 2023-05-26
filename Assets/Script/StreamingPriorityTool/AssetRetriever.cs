using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreamingPriorityTool
{
    public class AssetRetriever
    {
        private static AssetRetriever instance;
        public static AssetRetriever Instance
        {
            get
            {
                instance ??= new AssetRetriever();
                return instance;
            }
        }

        private Dictionary<string, List<GameObject>> assetCache;

        private AssetRetriever()  
        {
            assetCache = new Dictionary<string, List<GameObject>>();
        }

        /**
         * retrieves every asset from the given scene at {scenePath} 
         */
        public static List<GameObject> GetAssets(string scenePath)
        {
            if (scenePath == null)
            {
                Debug.Log("Null scene received while retrieving assets");
                return null;
            }

            Instance.assetCache ??= new Dictionary<string, List<GameObject>>();
            // i should wrap the scene opening in a try-catch to let him gracefully explode if the path is incorrect

            return Instance.assetCache.ContainsKey(scenePath) ? Instance.assetCache[scenePath] : GetAssets(EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)); 
        }

        /**
         * retrieves every asset from the referenced {scene}
         */
        public static List<GameObject> GetAssets(Scene scene)
        {
            if (!scene.Equals(SceneManager.GetActiveScene()) || scene == null) return null;

            Queue<GameObject> gos = new Queue<GameObject>(scene.GetRootGameObjects());
            List<GameObject> assets = new List<GameObject>();
            while (gos.Count > 0)
            {
                GameObject go = gos.Dequeue();
                assets.Add(go);

                for (int i = 0; i < go.transform.childCount; i++)
                {
                    var child = go.transform.GetChild(i);
                    if (child.childCount > 0)
                        gos.Enqueue(child.gameObject);
                    else /*if(child.gameObject.activeInHierarchy) // get only active objects, we do NOT want to enable stuff that we're not supposed to*/
                        assets.Add(child.gameObject);
                }
            }
            Instance.assetCache[scene.path] = assets;

            return assets;
        }

        public static List<Renderer> GetRenderers(Scene scene)
        {
            var assets = GetAssets(scene);
            var renderers = new List<Renderer>();
            foreach(var asset in assets)
            {
                try
                {
                    renderers.Add(asset.GetComponent<Renderer>());
                }
                catch { continue; }
            }
            return renderers;
        }

        /**
         * retrieves every asset from the given scene at {scenePath} if its tag matches {filterTag}
         */
        public static List<GameObject> GetAssets(string scenePath, string filterTag)
        {
            return GetAssets(scenePath).Where((asset) => asset.CompareTag(filterTag)).ToList();
        }

        /**
         * retrieve a list containing every assets from every scene in {scenePath}
         * probably useless, since objects IDs are unique in their scene
         */
        public static List<GameObject> GetFlattenedAssets(List<string> scenesPath)
        {
            List<GameObject> assets = new List<GameObject>();
            foreach (string path in scenesPath)
                assets.AddRange(GetAssets(path));

            return assets;
        }
    }
}
