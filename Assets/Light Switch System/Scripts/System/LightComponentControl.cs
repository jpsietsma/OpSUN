using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightSwitchSystem
{
    // all the things the light needs to be controlled
    public class LightComponentControl : MonoBehaviour
    {
        public int asociatedSwitches;
        public int onRequests;
        public Light lightComp;
        public void setLight()
        {
            lightComp = GetComponent<Light>();
        }
    }
}
