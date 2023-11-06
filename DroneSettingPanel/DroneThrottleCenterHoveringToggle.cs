
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneThrottleCenterHoveringToggle : IDroneSettingPanel
    {
        [SerializeField]
        Toggle toggle;

        public void OnClick()
        {
            UpdateMode();
        }

        void UpdateMode()
        {
            Debug.Log($"[DroneSetting] DroneThrottleCenterHoveringToggle :{toggle.isOn}");

            foreach (var drone in udrones)
            {
                drone.SetThrottleCenterHoveringMode(toggle.isOn);
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