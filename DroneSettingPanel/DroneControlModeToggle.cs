
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    public enum CONTROL_MODE
    {
        MODE1,
        MODE2,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneControlModeToggle : IDroneSettingPanel
    {
        [SerializeField]
        TextMeshProUGUI label;

        CONTROL_MODE currentMode = 0;
        int maxModeNum = 2;

        string[] modeName = { "MODE 1", "MODE 2" };

        public void OnPushButton()
        {
            currentMode = (CONTROL_MODE)(((int)currentMode + 1) % maxModeNum);

            UpdateMode();
        }

        void UpdateMode()
        {
            Debug.Log($"[DroneSetting] DroneControlModeToggle MODE:{modeName[(int)currentMode]}");

            label.text = modeName[(int)currentMode];

            switch (currentMode)
            {
                case CONTROL_MODE.MODE1: // Mode1
                    foreach (var drone in udrones)
                    {
                        var controller = drone.GetController();
                        controller.SetMode(true);

                    }
                    break;
                case CONTROL_MODE.MODE2: // Mode2
                    foreach (var drone in udrones)
                    {
                        var controller = drone.GetController();
                        controller.SetMode(false);

                    }
                    break;
            }
        }

        public override void ApplyDroneSetting()
        {
            UpdateMode();
        }

        public void SetControlMode(CONTROL_MODE mode)
        {
            currentMode = mode;
            UpdateMode();
        }
    }
}