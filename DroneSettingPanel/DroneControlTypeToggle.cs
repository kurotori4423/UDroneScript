
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
        MIDI,

        CONTROL_NUM,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneControlTypeToggle : IDroneSettingPanel
    {
        [SerializeField]
        TextMeshProUGUI label;

        CONTROL_TYPE currentMode = 0;
        int maxModeNum = (int)CONTROL_TYPE.CONTROL_NUM;

        string[] modeName = { "Default","CustomInput","MIDI"};

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
                        var controller = drone.GetController();
                        controller.SetControllerInput(0);
                    }
                    break;
                case CONTROL_TYPE.CUSTOM: // CustomInput
                    foreach(var drone in udrones)
                    {
                        var controller = drone.GetController();
                        controller.SetControllerInput(1);
                    }
                    break;
                case CONTROL_TYPE.MIDI:
                    foreach(var drone in udrones)
                    {
                        var controller = drone.GetController();
                        controller.SetControllerInput(2);
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