using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamingPriorityTool 
{
    [ExecuteAlways]
    public class Settings
    {
        public enum SortingAlgorithm
        {
            ClosestFirst,
            ClosestFirstInView,
            SphereTracing,
            SphereTracingSizePriority,
            OccluderWithMotionPredictor,
            RayTracing
        }


        public static SortingAlgorithm SelectedAlgorithm
        {
            get
            {
                string algstr = System.IO.File.ReadAllText(@"C:\Users\Cowo\detective\Assets\Editor\UI\StreamingPriorityTool\lastalg.txt");
                if (Enum.TryParse<Settings.SortingAlgorithm>(algstr, out Settings.SortingAlgorithm algo)) return algo;
                else return SortingAlgorithm.ClosestFirst;
            }
            set
            {
                System.IO.File.WriteAllText(@"C:\Users\Cowo\detective\Assets\Editor\UI\StreamingPriorityTool\lastalg.txt", value.ToString());
            }
        }
    }
}