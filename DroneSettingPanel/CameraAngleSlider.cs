
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    /// <summary>
    /// ドローンのカメラアングルを指定するスライダー
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CameraAngleSlider : IDroneSettingPanel
    {
        [SerializeField]
        Slider cameraAngleSlider;
        [SerializeField]
        TextMeshProUGUI label;

        public override void SetDroneCores(UdonDroneCore[] _udrones)
        {
            base.SetDroneCores(_udrones);

            Debug.Log("[DroneSetting] CameraAngleSlider SetDroneCores");
        }

        public void OnChangeCameraAngle()
        {
            Debug.Log("[DroneSetting] CameraAngleSlider OnChangeCameraAngle");

            foreach(var drone in udrones)
            {
                var angle = Mathf.Lerp(0, 90, cameraAngleSlider.value);
                label.text = $"{angle:0.0}";
                drone.m_ManualSyncVariables.SetCameraAngles(angle);
            }
        }

        public override void ApplyDroneSetting()
        {
            OnChangeCameraAngle();
        }

    }
}