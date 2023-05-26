using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace StreamingPriorityTool
{
    public class Utilities
    {
        public static void Paint(GOValue[] assets, Color c1, Color c2)
        {
            return;
            //if (SceneManager.GetActiveScene().name.Contains("SCN"))
            //{
            //    //Debug.Log($"Ignored: ALL (diorama scene) | Changed: None");
            //    return; // i don't wanna fuck up materials again
            //}

            //int changed = 0, ignored = 0;
            //float t = 0, increment = 1f / ((float)assets.Length);

            //foreach (var asset in assets)
            //{
            //    if (asset.obj.TryGetComponent<Renderer>(out Renderer renderer))
            //    {
            //        try
            //        {
            //            renderer.material.color = Color.Lerp(c1, c2, t);
            //            changed++;
            //            t += increment;
            //        }
            //        catch (Exception e) { ignored++; }
            //    }
            //}
            //Debug.Log($"Ignored: {ignored} | Changed: {changed}");
        }


        public static void Paint(List<GameObject> sorted, Color c1, Color c2)
        {
            return;
            //if (SceneManager.GetActiveScene().name.Contains("SCN")) return; // i don't wanna fuck up materials again

            //int changed = 0, ignored = 0;
            //float t = 0, increment = 1f / ((float)sorted.Count);

            //foreach (var asset in sorted)
            //{
            //    if (asset.TryGetComponent<Renderer>(out Renderer renderer))
            //    {
            //        try
            //        {
            //            renderer.material.color = Color.Lerp(c1, c2, t);
            //            changed++;
            //            t += increment;
            //        }
            //        catch (Exception e) { ignored++; }
            //    }
            //}
            //Debug.Log($"Ignored: {ignored} | Changed: {changed}");
        }


        public static string PrintAssets(GOValue[] assets)
        {
            string res = "";
            foreach (var asset in assets) res += $"{asset.obj.name} : \t\t{asset.value}\n";
            return res;
        }


        public static SerializableDictionary<string, float> SaveSorting(GOValue[] assets, GameObject epoint)
        {
            string realtiveTargetPath = $"Editor/UI/StreamingPriorityTool/SortingSerializations";
            int missed = 0;
            SerializableDictionary<string, float> dassets = new SerializableDictionary<string, float>();

            foreach (var asset in assets)
            {
                if (!dassets.ContainsKey(asset.obj.name))
                    dassets.Add(asset.obj.name, asset.value);
                else missed++;
            }
            Debug.Log($"Missed {missed} out of {assets.Length}");

            Directory.CreateDirectory($"{Application.dataPath}/{realtiveTargetPath}/{SceneManager.GetActiveScene().name}");
            JsonHandler.OverwriteContainer<SerializableDictionary<string, float>>(dassets, $"{realtiveTargetPath}/{SceneManager.GetActiveScene().name}/{epoint.name}");
            return dassets;
        }


        // save the last serialization for each type of sorting
        public static SerializableDictionary<string, float> SaveSorting(GOValue[] assets, GameObject epoint, Type sortingType)
        {
            string realtiveTargetPath = $"Editor/UI/StreamingPriorityTool/SortingSerializations";
            int missed = 0;
            SerializableDictionary<string, float> dassets = new SerializableDictionary<string, float>();

            foreach (var asset in assets)
            {
                if (!dassets.ContainsKey(asset.obj.name))
                    dassets.Add(asset.obj.name, asset.value);
                else missed++; 
            }
            Debug.Log($"Missed {missed} out of {assets.Length}");

            Directory.CreateDirectory($"{Application.dataPath}/{realtiveTargetPath}/{SceneManager.GetActiveScene().name}");
            JsonHandler.OverwriteContainer<SerializableDictionary<string, float>>(dassets, $"{realtiveTargetPath}/{SceneManager.GetActiveScene().name}/{epoint.name}-{sortingType.Name}");
            return dassets;
        }


        public static void LogSorting(GOValue[] values, GameObject epoint, Type sortingType)
        {
            string logPath = $"{Application.dataPath}/Editor/UI/StreamingPriorityTool/Logs";
            string logName = $"{DateTime.Now.ToString("dd-MM-yy hh-mm-ss")} {sortingType.Name}";
            File.WriteAllText($"{logPath}/{logName}.txt", PrintAssets(values));

            Debug.Log($"Logged at {logPath}/{logName}.txt");
        }
        public static List<GameObject> GOValuesToList(IEnumerable<GOValue> govs)
        {
            List<GameObject> list = new List<GameObject>();
            foreach (var gov in govs) list.Add(gov.obj);
            return list;
        }

        public static float MaxGOValue(IEnumerable<GOValue> govs)
        {
            float max = float.MinValue;
            foreach (GOValue gov in govs) max = gov.value > max ? gov.value : max;
            return max;
        }
    }
}
