using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFunctions : MonoBehaviour
{
    public List<Light> lights;
    public Light directional;

    public void AllLightsOnOff()
    {
        if(lights != null)
        {
            foreach(Light light in lights)
            {
                if(light.gameObject.activeSelf)
                {
                    light.gameObject.SetActive(false);
                }
                else
                {
                    light.gameObject.SetActive(true);
                }
            }
        }
    }

    public void DirectionalLightOnOff()
    {
        if(directional.gameObject.activeSelf)
        {
            directional.gameObject.SetActive(false);
        }
        else
        {
            directional.gameObject.SetActive(true);
        }
    }
}
