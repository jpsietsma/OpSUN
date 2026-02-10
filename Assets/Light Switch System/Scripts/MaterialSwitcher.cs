using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightSwitchSystem
{
    public class MaterialSwitcher : EmissiveMaterialControl
    {
        [SerializeField] Material lightsOnMat;
        [SerializeField] Material lightsOffMat;
        Renderer meshRenderer;
        private void Awake()
        {
            meshRenderer = GetComponent<Renderer>();
        }
        // change material methods
        public override void changeMaterialState(bool toggle)
        {
            if (meshRenderer != null)
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
}
