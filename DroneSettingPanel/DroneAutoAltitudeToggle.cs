
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneAutoAltitudeToggle : IDroneSettingPanel
    {
        [SerializeField]
        Toggle toggle;

        public void OnClick()
        {
            UpdateMode();
        }

        void UpdateMode()
        {
            Debug.Log($"[DroneSetting] DroneAutoAltitudeToggle :{toggle.isOn}");

            foreach(var drone in udrones)
            {
                drone.SetHightAdjustMode(toggle.isOn);
            }
        }

        public override void ApplyDroneSetting()
        {
            UpdateMode();
        }

        public void SetMode(bool value)
        {
            toggle.isOn = value;
        }
    }
}