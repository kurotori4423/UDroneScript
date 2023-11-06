
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class QuickPresetter : UdonSharpBehaviour
    {
        [SerializeField] int lhAxisID = 0;
        [SerializeField] int lvAxisID = 0;
        [SerializeField] int rhAxisID = 0;
        [SerializeField] int rvAxisID = 0;
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
      

        public void OnClick()
        {
            Debug.Log($"[DroneSetting] Apply QuickPreset : {name}");
            inputSetting.SetAxisSetting(lhAxisID, lvAxisID, rhAxisID, rvAxisID, lhAxis_Invert, lvAxis_Invert, rhAxis_Invert, rvAxis_Invert);
            autoAltitudeToggle.SetMode(autoAltitude);
            droneThrottleCenterHovering.SetMode(throttleCenterHovering);
            controlTypeToggle.SetControlType(controlType);
            controlModeToggle.SetControlMode(controlMode);
            flightModeToggle.SetFlightMode(flightMode);
            powerSlider.SetMaxThrottle(throttlePower);
        }
    }
}