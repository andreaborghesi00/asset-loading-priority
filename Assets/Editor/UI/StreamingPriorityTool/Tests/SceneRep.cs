using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamingPriorityTool 
{
    public class SceneRep : ScriptableObject
    {
        public SceneRep() { }

        public string sceneName = "";
        public bool isSelected = false;
    }
}
