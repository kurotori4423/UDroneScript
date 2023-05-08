
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace Kurotori.UDrone
{

    /// <summary>
    /// ドローンカメラビューワ
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DroneCamViewer : UdonSharpBehaviour
    {
        [Tooltip("ドローンのカメラ固定場所を指定する")]
        public GameObject[] droneCameraRigs;
        [Tooltip("ドローンカメラ本体")]
        public Transform droneCam;

        public GameObject[] turnOffObjects;
        public GameObject[] turnOnObjects;

        public TextMeshProUGUI text;

        private RenderTexture renderTexture;

        public bool isVirtualCameraMode;

        public Transform targetObject;

        int currentCam = 0;

        void Start()
        {
            SetLabel();
            TurnOff();

        }

        void SetCamera()
        {
            if (isVirtualCameraMode)
            {
                targetObject.SetParent(droneCameraRigs[currentCam].transform);
                targetObject.localPosition = Vector3.zero;
                targetObject.localRotation = Quaternion.identity;
            }
            else
            {
                droneCam.SetParent(droneCameraRigs[currentCam].transform);
                droneCam.localPosition = Vector3.zero;
                droneCam.localRotation = Quaternion.identity;
            }
        }

        public void TurnOn()
        {
            foreach (var obj in turnOffObjects)
            {
                obj.SetActive(false);
            }
            foreach (var obj in turnOnObjects)
            {
                obj.SetActive(true);
            }
            SetCamera();

        }

        public void TurnOff()
        {
            foreach (var obj in turnOffObjects)
            {
                obj.SetActive(true);
            }
            foreach (var obj in turnOnObjects)
            {
                obj.SetActive(false);
            }
        }

        void SetLabel()
        {
#if !UNITY_EDITOR
            string ownerName = Networking.GetOwner(droneCameraRigs[currentCam]).displayName;
            text.text = string.Format("[{0}]{1}", currentCam, ownerName);
#endif
        }

        public void Next()
        {
            currentCam = currentCam + 1 > droneCameraRigs.Length - 1 ? 0 : currentCam + 1;

            SetLabel();
            SetCamera();
        }

        public void Prev()
        {
            currentCam = currentCam - 1 < 0 ? droneCameraRigs.Length - 1 : currentCam - 1;

            SetLabel();
            SetCamera();
        }
    }
}