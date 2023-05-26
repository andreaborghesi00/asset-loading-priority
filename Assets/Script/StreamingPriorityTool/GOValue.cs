using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamingPriorityTool
{
	[Serializable]
    public class GOValue
    {
        public float value;
		private string name;
        [NonSerialized] public GameObject obj;

		public GOValue(GameObject o, float val)
		{
			obj = o;
			value = val;
			name = obj.name /*!= null ? obj.name : "franco"*/;
		}

		public GOValue(string n, float val)
        {
			obj = null;
			value = val;
			name = n;
        }

		public static bool operator >(GOValue a, GOValue b)
		{
			if (a.value > b.value) return true;
			else return false;
		}

		public static bool operator <(GOValue a, GOValue b)
		{
			if (a.value < b.value) return true;
			else return false;
		}

		public static bool operator >=(GOValue a, GOValue b)
		{
			if (a.value >= b.value) return true;
			else return false;
		}

		public static bool operator <=(GOValue a, GOValue b)
		{
			if (a.value <= b.value) return true;
			else return false;
		}

		public override string ToString()
		{
			return $"{obj.name}: {value}";
		}
	}
}
