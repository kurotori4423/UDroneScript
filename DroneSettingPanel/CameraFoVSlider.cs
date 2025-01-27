
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CameraFoVSlider : IDroneSettingPanel
    {
        [SerializeField]
        Slider cameraFoVSlider;
        [SerializeField]
        TMP_Text label;

        [SerializeField]
        float minFoV = 90.0f;
        [SerializeField]
        float maxFoV = 150.0f;

        public override void SetDroneCores(UdonDroneCore[] _udrones)
        {
            base.SetDroneCores(_udrones);
        }

        public void OnChangeCameraFoV()
        {
            foreach(var drone in udrones)
            {
                var fov = Mathf.Lerp(minFoV, maxFoV, cameraFoVSlider.value);
                label.text = $"{fov:0.0}";
                drone.m_ManualSyncVariables.SetCameraFov(fov);
            }
        }

        public override void ApplyDroneSetting()
        {
            OnChangeCameraFoV();
        }
    }
}