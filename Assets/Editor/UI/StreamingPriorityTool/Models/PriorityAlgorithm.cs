using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamingPriorityTool
{
    public abstract class PriorityAlgorithm 
    {
        public abstract List<GameObject> Sort(List<GameObject> assets, GameObject entryPoint, List<GameObject> except);
        protected abstract GOValue[] Evaluate(IEnumerable<GameObject> assets);
    }
}
