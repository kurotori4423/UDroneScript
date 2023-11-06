
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneVolumeSettingSlider : IDroneSettingPanel
    {
        [SerializeField]
        Slider volumeSlider;
        [SerializeField]
        TextMeshProUGUI label;

        public override void SetDroneCores(UdonDroneCore[] _udrones)
        {
            base.SetDroneCores(_udrones);

            Debug.Log("[DroneSetting] DroneVolumeSettingSlider SetDroneCores");
        }

        public void OnChangeDroneVolume()
        {
            Debug.Log("[DroneSetting] DroneVolumeSettingSlider OnChangeDroneVolume");
            foreach (var udrone in udrones)
            {
                udrone.SetDroneSoundVolume(volumeSlider.value);
                label.text = $"{volumeSlider.value * 100:F0}";
            }
        }

        public override void ApplyDroneSetting()
        {
            OnChangeDroneVolume();
        }
    }
}