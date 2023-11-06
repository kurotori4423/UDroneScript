
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    public enum CONTROL_TYPE
    {
        DEFAULT,
        CUSTOM,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneControlTypeToggle : IDroneSettingPanel
    {
        [SerializeField]
        TextMeshProUGUI label;

        CONTROL_TYPE currentMode = 0;
        int maxModeNum = 2;

        string[] modeName = { "Default","CustomInput"};

        public void OnPushButton()
        {
            currentMode = (CONTROL_TYPE)(((int)currentMode + 1) % maxModeNum);

            UpdateMode();
        }

        void UpdateMode()
        {
            Debug.Log($"[DroneSetting] DroneControlTypeToggle MODE:{modeName[(int)currentMode]}");

            label.text = modeName[(int)currentMode];
            
            switch(currentMode)
            {
                case CONTROL_TYPE.DEFAULT: // Default
                    foreach(var drone in udrones)
                    {
                        drone.SetUseVRRate(false);
                        var controller = drone.GetController();
                        controller.UseCustomInputOFF();

                    }
                    break;
                case CONTROL_TYPE.CUSTOM: // CustomInput
                    foreach(var drone in udrones)
                    {
                        drone.SetUseVRRate(true);
                        var controller = drone.GetController();
                        controller.UseCustomInputON();

                    }
                    break;
            }
        }

        public override void ApplyDroneSetting()
        {
            UpdateMode();
        }

        public void SetControlType(CONTROL_TYPE type)
        {
            currentMode = type;
            UpdateMode();
        }
    }
}