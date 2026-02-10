using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightSwitchSystem
{
    public enum axisSelect
    {
        onX,
        onY,
        onZ
    }
    public abstract class SwitchToggleControl : MonoBehaviour
    {
        // base class to control switch toggles
        public abstract void changeState(bool toggle);

    }
}
