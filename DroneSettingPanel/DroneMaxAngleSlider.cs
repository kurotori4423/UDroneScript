
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneMaxAngleSlider : IDroneSettingPanel
    {
        [SerializeField]
        Slider maxAngleSlider;
        [SerializeField]
        TextMeshProUGUI label;

        public void OnChangeMaxAngle()
        {
            var maxAngle = maxAngleSlider.value * 90.0f;

            foreach (var udrone in udrones)
            {
                udrone.SetMaxAngle(maxAngle);
                label.text = $"{maxAngle:0.0}";
            }
        }

        public override void ApplyDroneSetting()
        {
            OnChangeMaxAngle();
        }
    }
}