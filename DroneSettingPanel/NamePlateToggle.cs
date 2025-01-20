
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NamePlateToggle : IDroneSettingPanel
    {
        [SerializeField]
        Toggle toggle;

        public void OnClick()
        {
            UpdateMode();
        }

        void UpdateMode()
        {
            foreach(var drone in udrones)
            {
                drone.SetShowDroneNamePlate(toggle.isOn);
            }
        }

        public override void ApplyDroneSetting()
        {
            UpdateMode();
        }
    }
}