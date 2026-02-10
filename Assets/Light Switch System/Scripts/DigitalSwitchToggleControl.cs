using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightSwitchSystem
{
    public class DigitalSwitchToggleControl : SwitchToggleControl
    {
        [SerializeField] Material lightsOnMat;
        [SerializeField] Material lightsOffMat;
        Renderer meshRenderer;
        private void Awake()
        {
            meshRenderer = GetComponent<Renderer>();
        }
        // change material methods
        public override void changeState(bool toggle)
        {
            if (toggle)
            {
                meshRenderer.material = lightsOnMat;
            }
            else
            {
                meshRenderer.material = lightsOffMat;
            }
        }
    }
}
