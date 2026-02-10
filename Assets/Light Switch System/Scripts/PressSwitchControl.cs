using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LightSwitchSystem
{
    public class PressSwitchControl : SwitchToggleControl
    {
        [SerializeField] float TranslationAmmount = .1f;
        [SerializeField] axisSelect translationAxis;
        Vector3 selectedTranslation;

        //change position methods
        public override void changeState(bool toggle)
        {
            if (selectedTranslation == Vector3.zero)
            {
                setRotation();
            }

            if (toggle)
            {
                transform.localPosition -= selectedTranslation * TranslationAmmount;
            }
            else
            {
                transform.localPosition += selectedTranslation * TranslationAmmount;
            }
        }
        void setRotation()
        {

            switch (translationAxis)
            {
                case axisSelect.onX:
                    selectedTranslation = Vector3.left;
                    break;
                case axisSelect.onY:
                    selectedTranslation = Vector3.up;
                    break;
                case axisSelect.onZ:
                    selectedTranslation = Vector3.forward;
                    break;
                default:
                    selectedTranslation = Vector3.forward;
                    break;
            }
        }
    }
}
