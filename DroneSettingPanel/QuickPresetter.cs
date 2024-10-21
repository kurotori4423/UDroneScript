
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    public enum StickInputLabel
    {
        Joy1_Axis_1,
        Joy1_Axis_2,
        Joy1_Axis_3,
        Joy1_Axis_4,
        Joy1_Axis_5,
        Joy1_Axis_6,
        Joy1_Axis_7,
        Joy1_Axis_8,
        Joy1_Axis_9,
        Joy1_Axis_10,
        Joy2_Axis_1,
        Joy2_Axis_2,
        Joy2_Axis_3,
        Joy2_Axis_4,
        Joy2_Axis_5,
        Joy2_Axis_6,
        Joy2_Axis_7,
        Joy2_Axis_8,
        Joy2_Axis_9,
        Joy2_Axis_10,
        Oculus_GearVR_LThumbstickX,
        Oculus_GearVR_LThumbstickY,
        Oculus_GearVR_RThumbstickX,
        Oculus_GearVR_RThumbstickY,
        Oculus_GearVR_DpadX,
        Oculus_GearVR_DpadY,
        Oculus_GearVR_LIndexTrigger,
        Oculus_GearVR_RIndexTrigger,
        Oculus_CrossPlatform_PrimaryIndexTrigger,
        Oculus_CrossPlatform_SecondaryIndexTrigger,
        Oculus_CrossPlatform_PrimaryHandTrigger,
        Oculus_CrossPlatform_SecondaryHandTrigger,
        Oculus_CrossPlatform_PrimaryThumbstickHorizontal,
        Oculus_CrossPlatform_PrimaryThumbstickVertical,
        Oculus_CrossPlatform_SecondaryThumbstickHorizontal,
        Oculus_CrossPlatform_SecondaryThumbstickVertical,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class QuickPresetter : UdonSharpBehaviour
    {
        [SerializeField] StickInputLabel lhAxisID = 0;
        [SerializeField] StickInputLabel lvAxisID = 0;
        [SerializeField] StickInputLabel rhAxisID = 0;
        [SerializeField] StickInputLabel rvAxisID = 0;
        [SerializeField] bool lhAxis_Invert = false;
        [SerializeField] bool lvAxis_Invert = false;
        [SerializeField] bool rhAxis_Invert = false;
        [SerializeField] bool rvAxis_Invert = false;
        [SerializeField] bool autoAltitude = false;
        [SerializeField] bool throttleCenterHovering = false;
        [SerializeField] CONTROL_TYPE controlType = 0;
        [SerializeField] CONTROL_MODE controlMode = 0;
        [SerializeField] FLIGHTMODE flightMode = 0;
        [SerializeField,Range(0,1)] float throttlePower = 1;

        [Header("Rate Setting")]
        [SerializeField] float rcRate = 1.0f;
        [SerializeField] float spRate = 0.7f;
        [SerializeField] float expo = 0.0f; 

        [Space]
        [Header("Component")]

        [SerializeField]
        JoyInputSetting inputSetting;
        [SerializeField]
        DroneAutoAltitudeToggle autoAltitudeToggle;
        [SerializeField]
        DroneThrottleCenterHoveringToggle droneThrottleCenterHovering;
        [SerializeField]
        DroneControlTypeToggle controlTypeToggle;
        [SerializeField]
        DroneControlModeToggle controlModeToggle;
        [SerializeField]
        DroneFlightModeToggle flightModeToggle;
        [SerializeField]
        DroneThrottlePowerSlider powerSlider;
        [SerializeField]
        RateSettingPanel rateSettingPanel;

        public void OnClick()
        {
            Debug.Log($"[DroneSetting] Apply QuickPreset : {name}");
            inputSetting.SetAxisSetting((int)lhAxisID, (int)lvAxisID, (int)rhAxisID, (int)rvAxisID, lhAxis_Invert, lvAxis_Invert, rhAxis_Invert, rvAxis_Invert);
            autoAltitudeToggle.SetMode(autoAltitude);
            droneThrottleCenterHovering.SetMode(throttleCenterHovering);
            controlTypeToggle.SetControlType(controlType);
            controlModeToggle.SetControlMode(controlMode);
            flightModeToggle.SetFlightMode(flightMode);
            powerSlider.SetMaxThrottle(throttlePower);
            rateSettingPanel.SetRateSetting(rcRate, spRate, expo);
        }
    }
}