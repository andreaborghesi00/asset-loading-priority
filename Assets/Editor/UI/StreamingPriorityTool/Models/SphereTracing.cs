using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/**
 * i should start using exceptions
 */

namespace StreamingPriorityTool
{
    public class SphereTracing : PriorityAlgorithm
    {
        private const ushort MAX_STEPS = 100;
        private const float MAX_DIST = 100f;
        private const float MIN_DIST = .001f;
        private const byte STEP = 3; // 1 => 50 min to 1 hour of computing, with little to no improvement on the testing scene

        private Renderer[] rends;
        private GameObject entryPoint; // used in distance, idk y
        private float offset;

        public override List<GameObject> Sort(List<GameObject> assets, GameObject entryPoint, List<GameObject> except)
        {
            this.entryPoint = entryPoint;

            GOValue[] govalues = new GOValue[assets.Count()];
            // handle excepted assets
            if (except != null && except.Count > 0) assets.RemoveAll(item => except.Contains(item));
            for (int i = 0; i < except.Count(); i++)
                govalues[i] = new GOValue(except[i], float.MinValue+i); // distances are all non-negative hence -1 is always smaller
            Utilities.Paint(except, Color.magenta, Color.black);

            //                                                  16:9
            Camera cam = SpawnCamera(false, 100f, 0.1f, 40f, 1.777778f);
            var onCamera = RetrieveOnCameraRenderers(assets, cam);

            if (rends == null || rends.Length == 0)
            {
                Debug.Log("No rends");
                return null; // throw exception here
            }

            Dictionary<string, GameObject> onSightContainer = GetOnSightAssets(cam);
            var onSightSizePriority = GetOnSightSizePriority(cam);

            // destroy camera
            if (Application.isEditor)
                GameObject.DestroyImmediate(cam);
            else
                GameObject.Destroy(cam);

            // SORTING
            List<GameObject> sorted = new List<GameObject>();

            // first the ones on sight
            GOValue[] onSight = Evaluate(onSightContainer.Values); Utilities.Paint(onSight, Color.green, Color.red); Debug.Log($"OFFSET: {offset}");
            offset = Utilities.MaxGOValue(onSight);
            sorted.AddRange(Utilities.GOValuesToList(onSight));

            // secondly the occluded ones on camera
            GOValue[] occludedOnCamera = Evaluate(onCamera.Except(onSightContainer.Values)); Utilities.Paint(occludedOnCamera, Color.cyan, Color.blue); Debug.Log($"OFFSET: {offset}");
            offset = Utilities.MaxGOValue(occludedOnCamera);
            sorted.AddRange(Utilities.GOValuesToList(occludedOnCamera));


            // at last the ones outside the camera frustum
            GOValue[] notOnCamera = Evaluate(assets.Except(onCamera)); Utilities.Paint(notOnCamera, Color.yellow, Color.magenta);
            sorted.AddRange(Utilities.GOValuesToList(notOnCamera));

            // SERIALIZE SORTING
            //GOValue[] govalues = new GOValue[onSight.Length + occludedOnCamera.Length + notOnCamera.Length];
            onSight.CopyTo(govalues, except.Count());
            occludedOnCamera.CopyTo(govalues, except.Count() + onSight.Length);
            notOnCamera.CopyTo(govalues, except.Count() + onSight.Length + occludedOnCamera.Length);

            Utilities.LogSorting(govalues, entryPoint, this.GetType());
            _ = Utilities.SaveSorting(govalues, entryPoint);
            return sorted;
        }

        private Dictionary<string, GameObject> GetOnSightAssets(Camera refCam)
        {
            Vector3 pos = entryPoint.transform.position;
            Dictionary<string, GameObject> onSightContainer = new Dictionary<string, GameObject>(); // i use a dictionary for it's efficiency on the "insert or update" function
            int width = 1920;
            int height = 1080;
             
            for (int i = 0; i <= width; i += STEP)
            {
                for (int j = 0; j <= height; j += STEP)
                {
                    Vector3 target = refCam.ScreenToWorldPoint(new Vector3(i, j, 1));
                    var hit = RayMarching(pos, target - pos);
                    if (hit != null)
                    {
                        hit.GetComponent<Renderer>().material.color = Color.green;
                        onSightContainer[hit.name] = hit; // if we'd use Add() it would just... add, instead this notation also updates an already contained key's value
                    }
                }
            }

            return onSightContainer;
        }

        private Dictionary<string, GOValue> GetOnSightSizePriority(Camera refCam)
        {
            Vector3 pos = entryPoint.transform.position;
            Dictionary<string, GOValue> onSightContainer = new Dictionary<string, GOValue>(); // i use a dictionary for it's efficiency on the "insert or update" function
            ushort width = 1920;
            ushort height = 1080;
            for (int i = 0; i <= width; i += STEP)
            {
                for (int j = 0; j <= height; j += STEP)
                {
                    Vector3 target = refCam.ScreenToWorldPoint(new Vector3(i, j, 1));
                    var hit = RayMarching(pos, target - pos);
                    if (hit != null)
                    {
                        if (onSightContainer.ContainsKey(hit.name)) onSightContainer[hit.name].value++;
                        else onSightContainer[hit.name] = new GOValue(hit, 1); // if we'd use Add() it would just... add, instead this notation also updates an already contained key's value
                    }
                }
            }

            return onSightContainer;
        }

        // add camera and set details
        // make sure that they're the same used in the gizmo for demonstration/debugging purposes
        private Camera SpawnCamera(bool isOrthographic, float farClipPlane, float nearClipPlane, float fov, float aspectRatio)
        {
            Camera cam;
            if  (entryPoint.TryGetComponent<Camera>(out cam)) { }
            else
                cam = entryPoint.AddComponent<Camera>();
            
            cam.orthographic = isOrthographic;
            cam.farClipPlane = farClipPlane;
            cam.nearClipPlane = nearClipPlane;
            cam.fieldOfView = fov;
            cam.aspect = aspectRatio;

            return cam;
        }

        // the only objects considered are the ones intersecting the camera frustum
        // populates the rends list and returns the gameobjects intersecting the camera frustum
        private List<GameObject> RetrieveOnCameraRenderers(List<GameObject> assets, Camera cam)
        {
            List<Renderer> rends = new List<Renderer>();
            List<GameObject> onCamera = new List<GameObject>();
           // check for intersections
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            foreach (var asset in assets)
            {
                try
                {
                    if (asset.TryGetComponent<Renderer>(out Renderer renderer) && GeometryUtility.TestPlanesAABB(planes, renderer.bounds) && renderer.isVisible)
                    {
                        onCamera.Add(asset);
                        rends.Add(asset.GetComponent<Renderer>());
                    }
                }
                catch (Exception e) { continue; }
            }

            this.rends = rends.ToArray();
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

            return (to.transform.position - from.transform.position).magnitude;
        }

        private float Distance(GameObject from)
        {
            return Distance(from, entryPoint);
        }

        protected override GOValue[] Evaluate(IEnumerable<GameObject> assets)
        {
            GOValue[] arr = new GOValue[assets.Count()];
            int index = 0;
            foreach (var obj in assets)
                arr[index++] = new GOValue(obj, Distance(obj) + offset);

            return arr;
        }

        private GameObject RayMarching(Vector3 from, Vector3 dir)
        {
            dir.Normalize();
            float safeDist = 0f;
            Tuple<Renderer, float> closest = null;
            // MAX_STEPS * O(N) = O(N)
            for (int i = 0; i < MAX_STEPS; i++)
            {
                Vector3 p = from + safeDist * dir;
                closest = ClosestPointToBounds(p); // O(N) - item1: renderer | item2: distance
                safeDist += closest.Item2;
                if (closest.Item2 < MIN_DIST || safeDist > MAX_DIST) break;
            }
            return closest.Item1.gameObject;
        }

        // O(N)
        // BSP tree
        // oct tree
        // bvh
        private Tuple<Renderer, float> ClosestPointToBounds(Vector3 fromPos)
        {
            float min = Mathf.Infinity;
            Renderer closest = null;
            if (rends == null || rends.Length == 0) return Tuple.Create<Renderer, float>(null, -1f); // i have to specify T1 and T2 since null could be of almost any type
            
            foreach (Renderer rend in rends) // for each rendered asset
            {
                float dist = ClosestPointToBounds(fromPos, rend); // what's its closest point to fromPos
                if (dist < min)
                {
                    min = dist;
                    closest = rend;
                }
            }

            return Tuple.Create(closest, (closest.bounds.ClosestPoint(fromPos) - fromPos).magnitude);
        }

        // O(1) since {ClosestPoint(...)} is calculating the intersection on the bounding box, and not a generic much more complex shape, 
        // such intersection is known to be computable in O(1) time
        private float ClosestPointToBounds(Vector3 fromPos, Renderer to)
        {
            return (to.bounds.ClosestPoint(fromPos) - fromPos).magnitude;
        }

    }
}
