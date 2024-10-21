
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    /// <summary>
    /// カスタム入力
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CustomControllerInput : IControllerInput
    {
        [Header("カスタム入力(プロポなど)")]
        [SerializeField]
        public string customLHorizontal = "Joy1 Axis 4";
        [SerializeField]
        public string customLVertical = "Joy1 Axis 3";
        [SerializeField]
        public string customRHorizontal = "Joy1 Axis 1";
        [SerializeField]
        public string customRVertical = "Joy1 Axis 2";

        [SerializeField]
        bool invertLH = false;
        [SerializeField]
        bool invertLV = false;
        [SerializeField]
        bool invertRH = false;
        [SerializeField]
        bool invertRV = false;

        public override float GetLHorizontalAxis()
        {
            return Input.GetAxis(customLHorizontal) * (invertLH ? -1 : 1);
        }

        public override float GetLVerticalAxis()
        {
            return Input.GetAxis(customLVertical) * (invertLV ? -1 : 1);
        }

        public override float GetRHorizontalAxis()
        {
            return Input.GetAxis(customRHorizontal) * (invertRH ? -1 : 1);
        }

        public override float GetRVerticalAxis()
        {
            return Input.GetAxis(customRVertical) * (invertRV ? -1 : 1);
        }

        public void SetInvertLH(bool value)
        {
            invertLH = value;
        }
        public void SetInvertLV(bool value)
        {
            invertLV = value;
        }
        public void SetInvertRH(bool value)
        {
            invertRH = value;
        }
        public void SetInvertRV(bool value)
        {
            invertRV = value;
        }

        public void SetCustomLHInput(string axis)
        {
            customLHorizontal = axis;
        }
        public void SetCustomLVInput(string axis)
        {
            customLVertical = axis;
        }
        public void SetCustomRHInput(string axis)
        {
            customRHorizontal = axis;
        }
        public void SetCustomRVInput(string axis)
        {
            customRVertical = axis;
        }
    }
}