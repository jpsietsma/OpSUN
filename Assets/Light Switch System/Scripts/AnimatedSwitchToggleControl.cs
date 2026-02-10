using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightSwitchSystem
{
    public class AnimatedSwitchToggleControl : SwitchToggleControl
    {
        Animator ani;
        private void Awake()
        {
            ani = GetComponent<Animator>();
        }
        // change animation state methods
        public override void changeState(bool toggle)
        {
            if (toggle)
            {
                ani.SetBool("Toggle", true);
            }
            else
            {
                ani.SetBool("Toggle", false);
            }
        }
    }
}
