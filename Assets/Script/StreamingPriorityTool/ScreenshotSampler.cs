using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreamingPriorityTool
{
    public class ScreenshotSampler : MonoBehaviour
    {
        [SerializeField] string path = "C:/Users/00bor/Desktop/samples";
        [SerializeField] float targetFps = 3f;
        
        void Start()
        {
            StartCoroutine(Sampler());
        }

        private IEnumerator Sampler()
        {
            int index = 0;
            string scene = SceneManager.GetActiveScene().name;
            string timestr = DateTime.Now.ToString("dd-MM-yy hh-mm-ss");
            Directory.CreateDirectory($"{path}/{scene}");
            Directory.CreateDirectory($"{path}/{scene}/{timestr} {Settings.SelectedAlgorithm}");

            float fps = 1f / targetFps; // evitiamo di fare un quintilione di volte la stessa divisione
            for (; ;)
            {
                ScreenCapture.CaptureScreenshot($"{path}/{scene}/{timestr} {Settings.SelectedAlgorithm}/{index++}.png");
                yield return new WaitForSecondsRealtime(fps);
            }
        }

    }
}