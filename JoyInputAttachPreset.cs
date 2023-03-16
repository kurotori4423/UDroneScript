
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class JoyInputAttachPreset : UdonSharpBehaviour
    {
        [SerializeField]
        int leftHorizonID = 0;
        [SerializeField]
        int leftVerticalID = 0;
        [SerializeField]
        int rightHorizonID = 0;
        [SerializeField]
        int rightVerticalID = 0;

        [SerializeField]
        bool leftHorizontalInvert = false;
        [SerializeField]
        bool leftVerticalInvert = false;
        [SerializeField]
        bool rightHorizontalInvert = false;
        [SerializeField]
        bool rightVerticalInvert = false;

        [SerializeField]
        Dropdown LHDropDown;
        [SerializeField]
        Dropdown LVDropDown;
        [SerializeField]
        Dropdown RHDropDown;
        [SerializeField]
        Dropdown RVDropDown;

        [SerializeField]
        Toggle LHToggle;
        [SerializeField]
        Toggle LVToggle;
        [SerializeField]
        Toggle RHToggle;
        [SerializeField]
        Toggle RVToggle;

        public void ApplyPreset()
        {
            LHDropDown.value = leftHorizonID;
            LVDropDown.value = leftVerticalID;
            RHDropDown.value = rightHorizonID;
            RVDropDown.value = rightVerticalID;

            LHToggle.isOn = leftHorizontalInvert;
            LVToggle.isOn = leftVerticalInvert;
            RHToggle.isOn = rightHorizontalInvert;
            RVToggle.isOn = rightVerticalInvert;
        }
    }
}