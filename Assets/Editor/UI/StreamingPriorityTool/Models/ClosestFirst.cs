using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreamingPriorityTool
{
    public class ClosestFirst : PriorityAlgorithm
    {
		private static GameObject entryPoint;

		// i still don't get why are we working with dictionaries if we need priorities, isn't it faster and easier to use an array where the index represents the priority?
		private SerializableDictionary<string, float> assets; // identifier -> priority
		

		/**
		 * TODO: validity checks: null, empty, valid entry point (?)
		 */
        public override List<GameObject> Sort(List<GameObject> assets, GameObject entryPoint, List<GameObject> except)
        {
			ClosestFirst.entryPoint = entryPoint;
			
			GOValue[] govalues = new GOValue[assets.Count()];
			
			// handle excepted assets
			if (except != null && except.Count > 0)
			{
				assets.RemoveAll(item => except.Contains(item));
				int exceptedCount = except.Count();
				for (int i = 0; i < exceptedCount; i++)
					govalues[i] = new GOValue(except[i], float.MinValue+i); // distances are all non-negative hence -1 is always smaller

				HeapSort.Sort(Evaluate(assets)).CopyTo(govalues, exceptedCount);
			}
			else
				govalues = HeapSort.Sort(Evaluate(assets));

			Utilities.Paint(govalues, Color.green, Color.red);

			Utilities.LogSorting(govalues, entryPoint, this.GetType());
			_ = Utilities.SaveSorting(govalues, entryPoint);
			return Utilities.GOValuesToList(govalues);
        }

        private float Distance(GameObject from, GameObject to)
		{
			if(from == null)
            {
				Debug.Log("GO null");
				return -1;
            }
			if(to == null)
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
				arr[index++] = new GOValue(obj, Distance(obj));

			return arr;
		}
	}
}
