using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightSwitchSystem
{
    [System.Serializable]
    public class LightKitWrapper
    {
        // lights and materials serialized class to be linkied in the light master control

        public List<Light> controlledLightComponents;
        public List<EmissiveMaterialControl> controlledMaterialComponents;
        [HideInInspector] public List<LightComponentControl> connectedLights;

        public void setLights()
        {
            foreach (LightComponentControl controlledLight in connectedLights)
            {
                controlledLight.setLight();
            }
        }
    }
}
