using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightSwitchSystem
{
    public class RotatingSwitchToggleControl : SwitchToggleControl
    {
        
        Quaternion SwRotation;
        [SerializeField] axisSelect rotationAxis;
        [SerializeField] int rotationAngle = 20;
        Vector3 rotationOrientation;

        //change rotation methods
        public override void changeState(bool toggle)
        {
            if (rotationOrientation == Vector3.zero)
            {
                setRotation();
            }
            if (toggle)
            {
                SwRotation.eulerAngles = rotationOrientation * rotationAngle;
                transform.localRotation = SwRotation;
            }
            else
            {
                SwRotation.eulerAngles = rotationOrientation * -rotationAngle;
                transform.localRotation = SwRotation;
            }
        }
        void setRotation()
        {

            switch (rotationAxis)
            {
                case axisSelect.onX:
                    rotationOrientation = Vector3.left;
                    break;
                case axisSelect.onY:
                    rotationOrientation = Vector3.up;
                    break;
                case axisSelect.onZ:
                    rotationOrientation = Vector3.forward;
                    break;
                default:
                    rotationOrientation = Vector3.left;
                    break;
            }
        }
    }
}
