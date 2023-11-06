
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputViewer : UdonSharpBehaviour
    {
        [SerializeField]
        JoyInputSetting inputSetting;
        [SerializeField]
        float scale = 100.0f;

        [SerializeField]
        Transform leftPoint;
        [SerializeField]
        Transform rightPoint;

        private void Update()
        {
            var lhAxis = inputSetting.GetLHAxisName();
            var lvAxis = inputSetting.GetLVAxisName();
            var rhAxis = inputSetting.GetRHAxisName();
            var rvAxis = inputSetting.GetRVAxisName();

            float lx = Input.GetAxis(lhAxis) * (inputSetting.LHAxisInvert ? -1 : 1);
            float ly = Input.GetAxis(lvAxis) * (inputSetting.LVAxisInvert ? -1 : 1);
            float rx = Input.GetAxis(rhAxis) * (inputSetting.RHAxisInvert ? -1 : 1);
            float ry = Input.GetAxis(rvAxis) * (inputSetting.RVAxisInvert ? -1 : 1);

            leftPoint.localPosition = new Vector3(lx, ly) * scale;
            rightPoint.localPosition = new Vector3(rx, ry) * scale;
        }
    }
}
