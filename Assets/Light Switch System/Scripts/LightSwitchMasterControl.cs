using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
namespace LightSwitchSystem
{
    public class LightSwitchMasterControl : MonoBehaviour
    {
        [SerializeField] List<LightKitWrapper> controlledLightKits;
        [SerializeField] string buttonName = "Fire1";
        [SerializeField] string triggerTag = "Player";
        [SerializeField] bool usingPlayerTag;
        [SerializeField] bool usingMotionSensor;
        [SerializeField] bool showInputStatus;
        [SerializeField] bool lightsRequestState = true;
        [SerializeField] bool ShowStartingState;
        [SerializeField] bool ProMode;
        [SerializeField] Light singleLight;
        [SerializeField] EmissiveMaterialControl singleMat;
        LightComponentControl singleLightControl;
        SwitchToggleControl switchToggle;
        bool PlayerOnSensor;
        bool showHiddenVars = false;

        #region editor
#if UNITY_EDITOR
        [CustomEditor(typeof(LightSwitchMasterControl))]

        public class inspectorEditor : Editor
        {
            private ReorderableList listK;

            private void OnEnable()
            {
                listK = new ReorderableList(serializedObject, serializedObject.FindProperty("controlledLightKits"))
                {
                    displayAdd = false,
                    displayRemove = false,
                    draggable = true,
                };

                listK.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = listK.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);

                };
                listK.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Controlled Lights and Materials Groups");
                };

            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                LightSwitchMasterControl LightsControl = (LightSwitchMasterControl)target;
                bool mustRefresh = false;
                Undo.RecordObject(LightsControl, "Door settings changes");

                if (LightsControl.showHiddenVars)
                {
                    base.OnInspectorGUI();
                    EditorGUILayout.Space(40);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                }
                //header
                else
                {
                    Color headerColor(bool mode)
                    {
                        if (mode) return new Color(.7f, .6f, .1f);
                        else return new Color(.2f, .4f, .6f);
                    }
                    Rect headerArea = new Rect(0, 0, EditorGUIUtility.currentViewWidth, 35);
                    GUILayout.BeginArea(headerArea);
                    EditorGUILayout.Space(5);
                    EditorGUI.DrawRect(headerArea, headerColor(LightsControl.ProMode));
                    GUI.skin.label.fontSize = 15;
                    GUI.skin.label.fontStyle = FontStyle.BoldAndItalic;
                    if (LightsControl.ProMode) GUILayout.Label("LIGHT SWITCH Pro - Master control");
                    else GUILayout.Label("LIGHT SWITCH Lite - Master control");
                    GUILayout.EndArea();
                    EditorGUILayout.Space(40);
                }
                EditorGUILayout.LabelField("The player must have a collider component for the trigger sensor to work", EditorStyles.helpBox);
                EditorGUILayout.Space(2);
                string modeOption(bool mode)
                {
                    if (mode) return new string("Make it Lite");
                    else return new string("Make it pro");
                }
                if (GUILayout.Button(modeOption(LightsControl.ProMode)))
                {
                    LightsControl.ProMode = !LightsControl.ProMode;
                }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                EditorGUILayout.Space(2);
                if (LightsControl.ProMode)
                {

                    #region lists render

                    SerializedProperty kitsListProperty = serializedObject.FindProperty("controlledLightKits");


                    EditorGUILayout.PropertyField(kitsListProperty, true);


                    if (LightsControl.controlledLightKits != null && kitsListProperty.arraySize > LightsControl.controlledLightKits.Count)
                    {
                        mustRefresh = true;
                    }
                    serializedObject.ApplyModifiedProperties();
                    if (mustRefresh)
                    {
                        LightsControl.refreshLastItem();
                        mustRefresh = false;
                    }

                    #endregion

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add Light kit"))
                    {
                        LightsControl.addLight();
                    }
                    if (GUILayout.Button("Remove last"))
                    {
                        LightsControl.removeLight();
                    }
                    EditorGUILayout.EndHorizontal();

                }
                else
                {
                    LightsControl.singleLight = EditorGUILayout.ObjectField("Drop your light here", LightsControl.singleLight, typeof(Light), true) as Light;
                    LightsControl.singleMat = EditorGUILayout.ObjectField("Drop your emmisive object here", LightsControl.singleMat, typeof(EmissiveMaterialControl), true) as EmissiveMaterialControl;
                    EditorGUILayout.Space(2);

                    EditorGUILayout.LabelField("The object must have the lightsMaterialControl component to work", EditorStyles.helpBox);

                }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                LightsControl.usingPlayerTag = EditorGUILayout.Toggle("Use a player tag", LightsControl.usingPlayerTag);
                if (LightsControl.usingPlayerTag)
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.Space();
                    LightsControl.usingPlayerTag = true;
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField("Write the tag of the player here", EditorStyles.boldLabel);
                    LightsControl.triggerTag = EditorGUILayout.TextField("Tag", LightsControl.triggerTag);
                }



                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.BeginHorizontal();

                LightsControl.usingMotionSensor = EditorGUILayout.Toggle("Use Motion Sensor", LightsControl.usingMotionSensor);
                if (LightsControl.usingMotionSensor)
                {

                    LightsControl.usingMotionSensor = true;
                    EditorGUILayout.EndHorizontal();

                }

                else
                {
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(2);

                    EditorGUILayout.LabelField("Player input Button");


#if ENABLE_LEGACY_INPUT_MANAGER

                    LightsControl.buttonName = EditorGUILayout.TextField(LightsControl.buttonName);
                    EditorGUILayout.EndHorizontal();
#endif
#if ENABLE_INPUT_SYSTEM

                    //If you use the new input system you have to call the method "openRequestInput" of the SensorControl
                    EditorGUILayout.LabelField("For the new input system call the mehtod LightSwitch(bool), with your selected interactions button, ", EditorStyles.helpBox);

#endif
                }
                EditorGUILayout.Space(2);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Should the lights start on?");
                LightsControl.lightsRequestState = EditorGUILayout.ToggleLeft(" Lights start On", LightsControl.lightsRequestState);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                LightsControl.showHiddenVars = EditorGUILayout.Toggle("show all vars", LightsControl.showHiddenVars);

                if (GUI.changed)
                    EditorUtility.SetDirty(LightsControl);
            }
        }
#endif
        #endregion
        #region Light kits management functions
        public void addLight()
        {
            controlledLightKits.Add(new LightKitWrapper());
        }
        public void removeLight()
        {
            if (controlledLightKits.Count > 0) controlledLightKits.RemoveAt(controlledLightKits.Count - 1);
        }
        public void refreshLastItem()
        {
            controlledLightKits[controlledLightKits.Count - 1].controlledLightComponents = new List<Light>();
            controlledLightKits[controlledLightKits.Count - 1].controlledMaterialComponents = new List<EmissiveMaterialControl>();
        }
        #endregion
        private void Start()
        {
            // system start autosetup
            lightsRequestState = !lightsRequestState;
            switchToggle = GetComponentInChildren<SwitchToggleControl>();
            SwitchLightsCheck(true);
        }
        //sensor check methods
        private void OnTriggerEnter(Collider other)
        {
            if (usingPlayerTag && other.CompareTag(triggerTag))
            {
                PlayerOnSensor = true;
                if (usingMotionSensor)
                {
                    SwitchLights();
                }
            }
            else if (!usingPlayerTag)
            {
                PlayerOnSensor = true;
                if (usingMotionSensor)
                {
                    SwitchLights();
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (usingPlayerTag && other.CompareTag(triggerTag))
            {
                PlayerOnSensor = false;
                if (usingMotionSensor)
                {
                    SwitchLights();
                }
            }
            else if (!usingPlayerTag)
            {
                PlayerOnSensor = false;
                if (usingMotionSensor)
                {
                    SwitchLights();
                }
            }
        }
        // player input methods and light switch methods
#if ENABLE_LEGACY_INPUT_MANAGER
        private void Update()
        {
            if (!usingMotionSensor && Input.GetButtonDown(buttonName))
            {
                SwitchLightsCheck(false);
            }
        }
#endif
        public void SwitchLightsCheck(bool skipSensor)
        {
            if (skipSensor || PlayerOnSensor)
            {
                SwitchLights();
            }
        }

        bool lightsConnected;
        private void SwitchLights()
        {

            if (!lightsConnected)
            {
                if (ProMode)
                {
                    int groupsToCheck = 0;
                    foreach (LightKitWrapper kit in controlledLightKits)
                    {
                        connectManyLights(groupsToCheck);
                        groupsToCheck++;
                    }
                }
                else
                {
                    connectLight(singleLight, 0);
                }
            }
            lightsConnected = true;
            startSwitchRequest();
        }
        void startSwitchRequest()
        {
            bool toggle = !lightsRequestState;
            if (switchToggle != null) switchToggle.changeState(toggle);
            if (ProMode)
            {
                int lightGroup = 0;
                foreach (LightKitWrapper kit in controlledLightKits)
                {
                    sendManyLightRequests(lightGroup, toggle);
                    updateManyMaterials(lightGroup, toggle);
                    lightGroup++;
                }
            }
            else
            {
                sendLightRequest(singleLightControl, toggle);
                updateMaterial(singleMat, toggle);
            }
            lightsRequestState = !lightsRequestState;
        }
        // lights on-off state methods
        void sendManyLightRequests(int lightGroup, bool toggle)
        {
            int lightNum = 0;
            foreach (LightComponentControl individualLightControl in controlledLightKits[lightGroup].connectedLights)
            {
                sendLightRequest(individualLightControl, toggle);
                lightNum++;
            }
        }
        void sendLightRequest(LightComponentControl individualLight, bool toggle)
        {
            if (individualLight != null)
            {
                if (toggle) individualLight.onRequests += 1;
                else individualLight.onRequests -= 1;

                if (individualLight.asociatedSwitches == individualLight.onRequests)
                {
                    individualLight.lightComp.enabled = true;
                }
                else
                {
                    individualLight.lightComp.enabled = false;
                }
            }
        }
        // materials on-off state methods
        void updateManyMaterials(int materialGroup, bool toggle)
        {
            int matNum = 0;
            foreach (EmissiveMaterialControl individualMaterial in controlledLightKits[materialGroup].controlledMaterialComponents)
            {
                updateMaterial(individualMaterial, toggle);
                matNum++;
            }
        }
        void updateMaterial(EmissiveMaterialControl individualMaterial, bool toggle)
        {
            if (individualMaterial != null)
            {
                if (toggle) individualMaterial.onRequests += 1;
                else individualMaterial.onRequests -= 1;

                if (individualMaterial.asociatedSwitches == individualMaterial.onRequests)
                {
                    individualMaterial.changeMaterialState(true);
                }
                else
                {
                    individualMaterial.changeMaterialState(false);
                }
            }
        }
        // light links methods
        void connectManyLights(int lightGroup)
        {
            foreach (Light L in controlledLightKits[lightGroup].controlledLightComponents)
            {
                connectLight(L, lightGroup);
            }
            foreach (EmissiveMaterialControl controlledMat in controlledLightKits[lightGroup].controlledMaterialComponents)
            {
                if (controlledMat != null)
                {
                    controlledMat.asociatedSwitches += 1;
                    if (lightsRequestState) controlledMat.onRequests += 1;
                }
            }
        }
        void connectLight(Light L, int lightGroup)
        {
            if (L != null)
            {
                LightComponentControl CL;

                if (L.gameObject.GetComponent<LightComponentControl>() == null) CL = L.gameObject.AddComponent<LightComponentControl>();
                else CL = L.gameObject.GetComponent<LightComponentControl>();

                CL.setLight();
                CL.asociatedSwitches += 1;

                if (ProMode)
                {
                    controlledLightKits[lightGroup].connectedLights.Add(CL);
                }
                else
                {
                    singleLightControl = CL;

                }
                // requests count starting state compensation
                if (lightsRequestState) CL.onRequests += 1;
            }
            if (!ProMode && singleMat != null)
            {
                singleMat.asociatedSwitches += 1;
                if (lightsRequestState) singleMat.onRequests += 1;
            }
        }
    }
}

