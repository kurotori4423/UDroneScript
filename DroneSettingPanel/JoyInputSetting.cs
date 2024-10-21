
using UdonSharp;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class JoyInputSetting : IDroneSettingPanel
    {
        [SerializeField]
        CustomControllerInput customControllerInput;

        [SerializeField]
        AxisAssignPanel LHAxisPanel;
        [SerializeField]
        AxisAssignPanel LVAxisPanel;
        [SerializeField]
        AxisAssignPanel RHAxisPanel;
        [SerializeField]
        AxisAssignPanel RVAxisPanel;
        [Space]
        [SerializeField]
        InputViewer inputViewer;

        [HideInInspector] public int LHAxisID = 0;
        [HideInInspector] public int LVAxisID = 0;
        [HideInInspector] public int RHAxisID = 0;
        [HideInInspector] public int RVAxisID = 0;

        [HideInInspector] public bool LHAxisInvert = false;
        [HideInInspector] public bool LVAxisInvert = false;
        [HideInInspector] public bool RHAxisInvert = false;
        [HideInInspector] public bool RVAxisInvert = false;


        string[] stickInputsLabel =
        {
            "Joy1 Axis 1", // 0
            "Joy1 Axis 2", // 1
            "Joy1 Axis 3", // 2
            "Joy1 Axis 4", // 3
            "Joy1 Axis 5", // 4
            "Joy1 Axis 6", // 5
            "Joy1 Axis 7", // 6
            "Joy1 Axis 8", // 7
            "Joy1 Axis 9", // 8
            "Joy1 Axis 10", // 9
            "Joy2 Axis 1", // 10
            "Joy2 Axis 2", // 11
            "Joy2 Axis 3", // 12
            "Joy2 Axis 4", // 13
            "Joy2 Axis 5", // 14
            "Joy2 Axis 6", // 15
            "Joy2 Axis 7", // 16
            "Joy2 Axis 8", // 17
            "Joy2 Axis 9", // 18
            "Joy2 Axis 10", // 19
            "Oculus_GearVR_LThumbstickX", //20
            "Oculus_GearVR_LThumbstickY", //21
            "Oculus_GearVR_RThumbstickX", //22
            "Oculus_GearVR_RThumbstickY", //23
            "Oculus_GearVR_DpadX", // 24
            "Oculus_GearVR_DpadY", // 25
            "Oculus_GearVR_LIndexTrigger", // 26
            "Oculus_GearVR_RIndexTrigger", // 27
            "Oculus_CrossPlatform_PrimaryIndexTrigger", // 28
            "Oculus_CrossPlatform_SecondaryIndexTrigger", // 29
            "Oculus_CrossPlatform_PrimaryHandTrigger", // 30
            "Oculus_CrossPlatform_SecondaryHandTrigger", // 31
            "Oculus_CrossPlatform_PrimaryThumbstickHorizontal", // 32
            "Oculus_CrossPlatform_PrimaryThumbstickVertical", // 33
            "Oculus_CrossPlatform_SecondaryThumbstickHorizontal", // 34
            "Oculus_CrossPlatform_SecondaryThumbstickVertical" // 35
        };

        void Start()
        {
            LHAxisPanel.id = 0;
            LVAxisPanel.id = 1;
            RHAxisPanel.id = 2;
            RVAxisPanel.id = 3;
        }

        public string GetLHAxisName()
        {
            return stickInputsLabel[LHAxisID];
        }
        public string GetLVAxisName()
        {
            return stickInputsLabel[LVAxisID];
        }
        public string GetRHAxisName()
        {
            return stickInputsLabel[RHAxisID];
        }
        public string GetRVAxisName()
        {
            return stickInputsLabel[RVAxisID];
        }

        public void SetAxisSetting(int lhAxisID, int lvAxisID, int rhAxisID, int rvAxisID, bool lhInvert, bool lvInvert, bool rhInvert, bool rvInvert)
        {
            LHAxisPanel.SetAxisSetting(lhAxisID, lhInvert);
            LHAxisPanel.label.text = stickInputsLabel[lhAxisID];

            LVAxisPanel.SetAxisSetting(lvAxisID, lvInvert);
            LVAxisPanel.label.text = stickInputsLabel[lvAxisID];

            RHAxisPanel.SetAxisSetting(rhAxisID, rhInvert);
            RHAxisPanel.label.text = stickInputsLabel[rhAxisID];

            RVAxisPanel.SetAxisSetting(rvAxisID, rvInvert);
            RVAxisPanel.label.text = stickInputsLabel[rvAxisID];

            ApplyDroneSetting();
            ApplyAxisSetting();
        }

        public void OnPushCalibrationButton(int id)
        {
            switch(id)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
        }

        public override void ApplyDroneSetting()
        {
            LHAxisInvert = LHAxisPanel.GetInvertToggle();
            LVAxisInvert = LVAxisPanel.GetInvertToggle();
            RHAxisInvert = RHAxisPanel.GetInvertToggle();
            RVAxisInvert = RVAxisPanel.GetInvertToggle();

            LHAxisID = LHAxisPanel.GetAxisID();
            LVAxisID = LVAxisPanel.GetAxisID();
            RHAxisID = RHAxisPanel.GetAxisID();
            RVAxisID = RVAxisPanel.GetAxisID();
        }

        public void OnInvertToggle(int id, bool value)
        {
            switch (id)
            {
                case 0:
                    {
                        Debug.Log($"[DroneSetting] Set Custom LH Invert : {value}");
                        customControllerInput.SetInvertLH(value);

                        LHAxisInvert = value;
                        break;
                    }
                case 1:
                    {
                        Debug.Log($"[DroneSetting] Set Custom LV Invert : {value}");
                        customControllerInput.SetInvertLV(value);
                        LVAxisInvert = value;
                        break;
                    }
                case 2:
                    {
                        Debug.Log($"[DroneSetting] Set Custom RH Invert : {value}");
                        customControllerInput.SetInvertRH(value);
                        RHAxisInvert = value;
                        break;
                    }
                case 3:
                    {
                        Debug.Log($"[DroneSetting] Set Custom RV Invert : {value}");
                        customControllerInput.SetInvertRV(value);
                        RVAxisInvert = value;
                        break;
                    }
            }
        }

        public void OnChangeAxis(int id, int axisID)
        {
            switch(id)
            {
                case 0:
                    {
                        Debug.Log($"[DroneSetting] Set Custom LH : {stickInputsLabel[axisID]}");
                        customControllerInput.SetCustomLHInput(stickInputsLabel[axisID]);
                        LHAxisID = axisID;
                        LHAxisPanel.label.text = stickInputsLabel[axisID];
                        break;
                    }
                case 1:
                    {
                        Debug.Log($"[DroneSetting] Set Custom LV : {stickInputsLabel[axisID]}");
                        customControllerInput.SetCustomLVInput(stickInputsLabel[axisID]);

                        LVAxisID = axisID;
                        LVAxisPanel.label.text = stickInputsLabel[axisID];
                        break;
                    }
                case 2:
                    {
                        Debug.Log($"[DroneSetting] Set Custom RH : {stickInputsLabel[axisID]}");
                        customControllerInput.SetCustomRHInput(stickInputsLabel[axisID]);

                        RHAxisID = axisID;
                        RHAxisPanel.label.text = stickInputsLabel[axisID];
                        break;
                    }
                case 3:
                    {
                        Debug.Log($"[DroneSetting] Set Custom RV : {stickInputsLabel[axisID]}");
                        customControllerInput.SetCustomRVInput(stickInputsLabel[axisID]);

                        RVAxisID = axisID;
                        RVAxisPanel.label.text = stickInputsLabel[axisID];
                        break;
                    }
            }
        }

        public void ApplyAxisSetting()
        {
            foreach (var drone in udrones)
            {
                customControllerInput.SetCustomLHInput(stickInputsLabel[LHAxisID]);
                customControllerInput.SetCustomLVInput(stickInputsLabel[LVAxisID]);
                customControllerInput.SetCustomRHInput(stickInputsLabel[RHAxisID]);
                customControllerInput.SetCustomRVInput(stickInputsLabel[RVAxisID]);

                customControllerInput.SetInvertLH(LHAxisInvert);
                customControllerInput.SetInvertLV(LVAxisInvert);
                customControllerInput.SetInvertRH(RHAxisInvert);
                customControllerInput.SetInvertRV(RVAxisInvert);

                Debug.Log($"[DroneSetting] Set Custom LH : {stickInputsLabel[LHAxisID]} : {LHAxisInvert}");
                Debug.Log($"[DroneSetting] Set Custom LV : {stickInputsLabel[LVAxisID]} : {LVAxisInvert}");
                Debug.Log($"[DroneSetting] Set Custom RH : {stickInputsLabel[RHAxisID]} : {RHAxisInvert}");
                Debug.Log($"[DroneSetting] Set Custom RV : {stickInputsLabel[RVAxisID]} : {RVAxisInvert}");
            }
        }
    }
}