using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StreamingPriorityTool
{
    public class RayTracing : PriorityAlgorithm
    {
        private const float MAX_DIST = 200f;

        private GameObject entryPoint;
        private Renderer[] rends;
        private float offset = 0;
        private Light light;
        private float shadowCasterFactor;
        public void Init(Light light, float shadowCasterFactor)
        {
            this.light = light;
            this.shadowCasterFactor = shadowCasterFactor;
        }
        public override List<GameObject> Sort(List<GameObject> assets, GameObject entryPoint, List<GameObject> except)
        {
            this.entryPoint = entryPoint;

            GOValue[] govalues = new GOValue[assets.Count];
            List<GameObject> res = new List<GameObject>();

            // handle excepted assets
            if (except != null && except.Count > 0)
            {
                assets.RemoveAll(item => except.Contains(item));
                res.AddRange(except);
            }
            for (int i = 0; i < except.Count; i++)
                govalues[i] = new GOValue(except[i], float.MinValue+i); // distances are all non-negative hence -1 is always smaller
            Utilities.Paint(except, Color.magenta, Color.black);

            Camera cam = SpawnCamera(false, 100f, 0.1f, 40f, 1.777778f);
            var onCamera = RetrieveOnCameraRenderers(assets, cam);

            if (rends == null || rends.Length == 0)
            {
                Debug.Log("No rends");
                return null; // throw exception here
            }

            //Dictionary<string, GameObject> onSightContainer = RayTrace(cam);
            Dictionary<string, GOValue> onSightSizeContainer = RayTraceSize(cam);

            // destroy camera
            if (Application.isEditor)
                GameObject.DestroyImmediate(cam);
            else
                GameObject.Destroy(cam);

            // first the ones on sight
            GOValue[] onSight = onSightSizeContainer.Values.ToArray(); 
                Utilities.Paint(onSight, Color.green, Color.red); 
            offset = Utilities.MaxGOValue(onSight);
            var onSightList = Utilities.GOValuesToList(onSight);
            res.AddRange(onSightList);

            // secondly the occluded ones on camera
            GOValue[] occludedOnCamera = Evaluate(onCamera.Except(onSightList)); 
                Utilities.Paint(occludedOnCamera, Color.cyan, Color.blue); 
            offset = Utilities.MaxGOValue(occludedOnCamera);
            res.AddRange(Utilities.GOValuesToList(occludedOnCamera));


            // at last the ones outside the camera frustum
            GOValue[] notOnCamera = Evaluate(assets.Except(onCamera).Except(onSightList)); 
                Utilities.Paint(notOnCamera, Color.yellow, Color.magenta);
            res.AddRange(Utilities.GOValuesToList(notOnCamera));

            // SERIALIZE SORTING
            onSight.CopyTo(govalues, except.Count());
            occludedOnCamera.CopyTo(govalues, except.Count() + onSight.Length);
            notOnCamera.CopyTo(govalues, except.Count + onSight.Length + occludedOnCamera.Length);

            Utilities.LogSorting(govalues, entryPoint, this.GetType());
            _ = Utilities.SaveSorting(govalues, entryPoint);
            return res;

        }

        protected override GOValue[] Evaluate(IEnumerable<GameObject> assets)
        {
            GOValue[] arr = new GOValue[assets.Count()];
            int index = 0;
            foreach (var obj in assets)
                arr[index++] = new GOValue(obj, Distance(obj) + offset);

            return arr;
        }
        private Camera SpawnCamera(bool isOrthographic, float farClipPlane, float nearClipPlane, float fov, float aspectRatio)
        {
            Camera cam;
            if (entryPoint.TryGetComponent<Camera>(out cam)) { }
            else
                cam = entryPoint.AddComponent<Camera>();

            cam.orthographic = isOrthographic;
            cam.farClipPlane = farClipPlane;
            cam.nearClipPlane = nearClipPlane;
            cam.fieldOfView = fov;
            cam.aspect = aspectRatio;

            return cam;
        }

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

        private Dictionary<string, GameObject> RayTrace(Camera refCam)
        {
            Vector3 pos = entryPoint.transform.position;
            Dictionary<string, GameObject> onSightContainer = new Dictionary<string, GameObject>(); // i use a dictionary for it's efficiency on the "insert or update" function
            int width = 1920;
            int height = 1080;
            Vector3 origin = refCam.transform.position;

            for (int i = 0; i <= width; i++)
            {
                for (int j = 0; j <= height; j++)
                {
                    Vector3 target = refCam.ScreenToWorldPoint(new Vector3(i, j, 1));
                    RaycastHit hit;
                    if (Physics.Raycast(refCam.transform.position, target - origin, out hit, MAX_DIST))
                    {
                        var obj = hit.collider.gameObject;
                        obj.GetComponent<Renderer>().material.color = Color.green;
                        onSightContainer[obj.name] = obj; // if we'd use Add() it would just... add, instead this notation also updates an already contained key's value
                    }
                }
            }
            return onSightContainer;
        }
        private Dictionary<string, GOValue> RayTraceSize(Camera refCam)
        {
            Dictionary<string, GOValue> onSightContainer = new Dictionary<string, GOValue>(); // i use a dictionary for it's efficiency on the "insert or update" function

            int width = 1920;
            int height = 1080;
            Vector3 origin = refCam.transform.position;

            for (int i = 0; i <= width; i++)
            {
                for (int j = 0; j <= height; j++)
                {
                    Vector3 target = refCam.ScreenToWorldPoint(new Vector3(i, j, 1));
                    RaycastHit hit;
                    if (Physics.Raycast(refCam.transform.position, target - origin, out hit, MAX_DIST))
                    {
                        var obj = hit.collider.gameObject;
                        if (onSightContainer.ContainsKey(obj.name)) onSightContainer[obj.name].value--;
                        else onSightContainer[obj.name] = new GOValue(obj, -1); // sugary notation

                        // ray bounce to shadow caster
                        RaycastHit hitShadow;
                        if (Physics.Raycast(hit.point, -light.transform.forward, out hitShadow, MAX_DIST))
                        {
                            var objsh = hitShadow.collider.gameObject;
                            if (onSightContainer.ContainsKey(objsh.name)) onSightContainer[objsh.name].value -= shadowCasterFactor;
                            else
                            {
                                onSightContainer[objsh.name] = new GOValue(objsh, -shadowCasterFactor);
                                Debug.Log($"{obj.name} covered by {objsh.name}");
                            }
                        }
                    }
                }
            }
            return onSightContainer;
        }

    }
}
