using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightSwitchSystem
{
    public abstract class EmissiveMaterialControl : MonoBehaviour
    {
        // base class to control materials
        public int asociatedSwitches;
        public int onRequests;
        public abstract void changeMaterialState(bool toggle);
    }
}
