
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DroneThrottlePowerSlider : IDroneSettingPanel
    {
        [SerializeField]
        Slider throttleSlider;
        [SerializeField]
        TextMeshProUGUI label;

        [SerializeField]
        float maxThrottle = 8.0f;


        public void OnChangeMaxThrottle()
        {
            var throttle = throttleSlider.value * maxThrottle;
            Debug.Log($"[DroneSetting] MaxThrottle : {throttle:0.0}");

            foreach (var udrone in udrones)
            {
                udrone.SetThrottleForce(throttle);
                label.text = $"{throttle:0.0}";
            }
        }

        public override void ApplyDroneSetting()
        {
            OnChangeMaxThrottle();
        }

        public void SetMaxThrottle(float throttle)
        {
            throttleSlider.value = throttle;
        }
    }
}