
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneCustomInputToggle : IDroneSettingPanel
    {
        [SerializeField]
        Toggle toggle;

        public void OnClick()
        {
            UpdateMode();
        }

        void UpdateMode()
        {
            Debug.Log($"[DroneSetting] DroneCustomInputToggle :{toggle.isOn}");

            foreach (var drone in udrones)
            {
                if (toggle.isOn)
                {
                    drone.GetController().UseCustomInputON();
                }
                else
                {
                    drone.GetController().UseCustomInputOFF();
                }
                
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