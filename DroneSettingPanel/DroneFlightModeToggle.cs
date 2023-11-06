
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    public enum FLIGHTMODE
    {
        ANGLE,
        ACRO,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneFlightModeToggle : IDroneSettingPanel
    {
        [SerializeField]
        TextMeshProUGUI label;

        FLIGHTMODE currentMode = 0;
        int maxModeNum = 2;

        string[] modeName = { "ANGLE", "ACRO" };

        public void OnPushButton()
        {
            currentMode = (FLIGHTMODE)(((int)currentMode + 1) % maxModeNum);

            UpdateMode();
        }

        void UpdateMode()
        {
            Debug.Log($"[DroneSetting] DroneFlightModeToggle MODE:{modeName[(int)currentMode]}");

            label.text = modeName[(int)currentMode];

            switch (currentMode)
            {
                case FLIGHTMODE.ANGLE: // ANGLE
                    foreach (var drone in udrones)
                    {
                        drone.ChangeMode_Angle();
                    }
                    break;
                case FLIGHTMODE.ACRO: // ACRO
                    foreach (var drone in udrones)
                    {
                        drone.ChangeMode_Acro();
                    }
                    break;
            }
        }

        public override void ApplyDroneSetting()
        {
            UpdateMode();
        }

        public void SetFlightMode(FLIGHTMODE mode)
        {
            currentMode = mode;
            UpdateMode();
        }
    }
}