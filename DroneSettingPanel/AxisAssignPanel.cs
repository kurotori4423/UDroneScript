
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace Kurotori.UDrone
{
    /// <summary>
    /// スティックアサイン設定パネル
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AxisAssignPanel : UdonSharpBehaviour
    {
        [SerializeField]
        JoyInputSetting joyInputSetting;

        [SerializeField]
        public TextMeshProUGUI label;
        [SerializeField]
        Toggle invertToggle;
        [SerializeField]
        Dropdown dropdown;


        public int id;

        public void OnPushCalibrationButton()
        {
            joyInputSetting.OnPushCalibrationButton(id);
        }

        public void OnInvertToggle()
        {
            joyInputSetting.OnInvertToggle(id, invertToggle.isOn);
        }

        public void OnChangeAxis()
        {
            joyInputSetting.OnChangeAxis(id, dropdown.value);
        }

        public bool GetInvertToggle()
        {
            return invertToggle.isOn;
        }

        public int GetAxisID()
        {
            return dropdown.value;
        }

        public void SetAxisSetting(int index, bool invert)
        {
            dropdown.SetValueWithoutNotify(index);
            invertToggle.SetIsOnWithoutNotify(invert);
        }
    }
}
