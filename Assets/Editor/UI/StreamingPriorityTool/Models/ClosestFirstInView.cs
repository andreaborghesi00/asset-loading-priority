using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StreamingPriorityTool
{
    public class ClosestFirstInView : PriorityAlgorithm
    {
        private static GameObject entryPoint;
        private GameObject camContainer;
        private float offset = 0;

        public override List<GameObject> Sort(List<GameObject> assets, GameObject entryPoint, List<GameObject> except)
        {
            ClosestFirstInView.entryPoint = entryPoint;

            GOValue[] govalues = new GOValue[assets.Count()];
            // handle excepted assets
            if (except != null && except.Count > 0) assets.RemoveAll(item => except.Contains(item));
            for (int i = 0; i < except.Count(); i++)
                govalues[i] = new GOValue(except[i], float.MinValue+i); // distances are all non-negative hence -1 is always smaller
                                                                                    Utilities.Paint(except, Color.magenta, Color.magenta);

            var seen = SeenGameObjects(assets, entryPoint);
            
            var evaluated = Evaluate(seen);
            var sorted = HeapSort.Sort(evaluated);                                  Utilities.Paint(sorted, Color.white, Color.blue);
            offset = sorted[sorted.Length - 1].value;                               Debug.Log($"OFFSET: {sorted[sorted.Length - 1].obj.name} {offset}");
            List<GameObject> sortedList = Utilities.GOValuesToList(sorted);

            var remainingList = new List<GameObject>();
            remainingList.AddRange(assets.Except(sortedList));                                          
            var remaining = HeapSort.Sort(Evaluate(remainingList));                 Utilities.Paint(remaining, Color.green, Color.red);
            
            //GOValue[] govalues = new GOValue[sorted.Length + remaining.Length];
            sorted.CopyTo(govalues, except.Count());
            remaining.CopyTo(govalues, except.Count() + sorted.Length);

            Utilities.LogSorting(govalues, entryPoint, this.GetType());
            _ = Utilities.SaveSorting(govalues, entryPoint);

            return sortedList;
        }

        /**
         * Retrieves the gameobjects inside of the camera frustum
         */
        private List<GameObject> SeenGameObjects(List<GameObject> assets, GameObject epoint)
        {
            List<GameObject> onCamera = new List<GameObject>();
            
            // instantiate and position camera gameobject
            camContainer = new GameObject(); // automatic instantiation
            camContainer.SetActive(false); // i don't need it to be active to use the methods that i need, it assures me that i do not render anything (even tho i'm offline)
            camContainer.name = "CAMERA";
            camContainer.transform.position = entryPoint.transform.position;
            camContainer.transform.rotation = entryPoint.transform.rotation;
            // add camera and set details
            Camera cam = camContainer.AddComponent<Camera>();
            cam.farClipPlane = 100f;
            cam.nearClipPlane = 0.1f;
            cam.fieldOfView = 40f;
            cam.aspect = 1.777778f;
            // check for intersections
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            foreach(var asset in assets)
            {
                try
                {
                    if(asset.TryGetComponent<Renderer>(out Renderer renderer) && GeometryUtility.TestPlanesAABB(planes, renderer.bounds) && renderer.isVisible)
                    {
                        onCamera.Add(asset);
                    }
                } catch(Exception e) { continue; }
            }

            // destroy camera
            if (Application.isEditor)
                GameObject.DestroyImmediate(camContainer);
            else
                GameObject.Destroy(camContainer);

            return onCamera;
        }

        private float Distance(GameObject from, GameObject to)
        {
            if (from == null)
            {
                Debug.Log("GO null");
                return -1;
            }
            if (to == null)
            {
                Debug.Log("entry point null");
                return -2;
            }

            // it ensures that i'm checking world coordinates, if the obj is already in the outer "layer" the matrix is an identity matrix
            return (to.transform.position - from.transform.position).magnitude;
        }

        private float Distance(GameObject from)
        {
            return Distance(from, entryPoint);
        }

        /**
         * We value distance from the entry point, the closer the higher the priority
         * Highest prio: 0
         * Lowest prio: +inf
         */
        protected override GOValue[] Evaluate(IEnumerable<GameObject> assets)
        {
            GOValue[] arr = new GOValue[assets.Count()];
            int index = 0;
            foreach (var obj in assets)
                arr[index++] = new GOValue(obj, Distance(obj) + offset);

            return arr;
        }


    }
}
