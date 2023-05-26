using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreamingPriorityTool
{
    /**
     * # This is component is only for demonstration purposes #
     * 
     * Disables every gameobject at start to then enable them gradually in a "smart" way
     */
    public class WaveEnabler : MonoBehaviour
    {
        [SerializeField] private int enablesPerFrame = 3;
        [SerializeField] private int targetFrameRate = 60;

        [SerializeField] private bool isScreenshotEnabled = false;
        [SerializeField] private string path;
        
        private int current = 0;
        private List<GameObject> assets;
        private GOValue[] sortedGov;
        public int r = 0, c = 0;

        bool done = false;
        private string enablingTimes;
        bool wrote = true;
        float preTime = 0f;

        int index;
        string scene;
        string timestr;

        // time complexity: O(N*log(N))     [N + N*log(N)]
        // space complexity: O(N)           [4N]
        void Start()
        {
            
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
            string realtiveTargetPath = $"Editor/UI/StreamingPriorityTool/SortingSerializations";

            // retrieve evaluated objects
            float t1 = Time.realtimeSinceStartup;
            var container = JsonHandler.GetContainer<SerializableDictionary<string, float>>($"{realtiveTargetPath}/{SceneManager.GetActiveScene().name}/{gameObject.name}");
            Debug.Log($"TIME TO RETRIEVE: {Time.realtimeSinceStartup - t1}");
            float t2 = Time.realtimeSinceStartup;

            // retrieve gameobjects in scene (i should time this)
            assets = AssetRetriever.GetAssets(SceneManager.GetActiveScene());
            
            // O(N)
            // populate evaluated array <gameobject, value>
            sortedGov = new GOValue[assets.Count];
            for(int i = 0; i < assets.Count; i++) // TODO: check if they key is contained, it might happen that some go are spawned very early on
                sortedGov[i] = new GOValue(assets[i], container[assets[i].name]); // objects with the same name have the same priority (sowwy)


            // O(N*log(N))
            // sort 
            sortedGov = HeapSort.Sort(sortedGov);
            Debug.Log($"TO SORT: {Time.realtimeSinceStartup - t1} , NET SORTING: {Time.realtimeSinceStartup - t2}");
            float t3 = Time.realtimeSinceStartup;

            Debug.Log($"length: {sortedGov.Length}");

            foreach (var asset in sortedGov)
                asset.obj.SetActive(false);
         
            gameObject.SetActive(true);
            
            Debug.Log($"TO DISABLE: {Time.realtimeSinceStartup - t1}, NET DISABLING: {Time.realtimeSinceStartup - t3}");

            enablingTimes = "";

            //StartCoroutine(Sampler());
            index = 0;
            scene = SceneManager.GetActiveScene().name;
            timestr = DateTime.Now.ToString("dd-MM-yy hh-mm-ss");
            Directory.CreateDirectory($"{path}/{scene}");
            Directory.CreateDirectory($"{path}/{scene}/{timestr} {Settings.SelectedAlgorithm}");

        }

        void Update()
        {
            if (current <= sortedGov.Length - enablesPerFrame - 1)
            {
                for (int i = current; i < current + enablesPerFrame; i++)
                    sortedGov[i].obj.SetActive(true);
                
                current += enablesPerFrame;
                if(isScreenshotEnabled)
                    ScreenCapture.CaptureScreenshot($"{path}/{scene}/{timestr} {Settings.SelectedAlgorithm}/{index++}.png");
            } else if (!done && isScreenshotEnabled)
            {
                done = true;
                Debug.Log("Frame capture completed");
            }
        }


        public List<GameObject> GOValuesToList(GOValue[] govs)
        {
            List<GameObject> list = new List<GameObject>();
            foreach (var gov in govs) list.Add(gov.obj);
            return list;
        }
    }
}
