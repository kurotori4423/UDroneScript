
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
        public UdonDroneCore[] droneCores;
        [Tooltip("ドローンカメラ本体")]
        public Camera droneCam;

        [Tooltip("カメラ解像度X")]
        public int ResolutionX = 640;
        [Tooltip("カメラ解像度Y")]
        public int ResolutionY = 480;

        [SerializeField, Tooltip("RenderTextureをセットしたいオブジェクト")]
        private DroneCamRenderTextureAssigner[] m_renderTextureAssigners;

        public GameObject[] turnOffObjects;
        public GameObject[] turnOnObjects;

        public TextMeshProUGUI text;

        private RenderTexture renderTexture;

        public bool isVirtualCameraMode;

        public Transform targetObject;

        int currentCam = 0;

        UdonBehaviour m_udonBehaviour;

        void Start()
        {
#if UNITY_ANDROID
            // Android(Quest)ではPostProcessが使えないのでDefaultのまま
            renderTexture = new RenderTexture(ResolutionX, ResolutionY, 16, RenderTextureFormat.Default);
#else
            // Postprocessに対応するためRenderTextureFormatをDefaultHDRへ
            renderTexture = new RenderTexture(ResolutionX, ResolutionY, 16, RenderTextureFormat.DefaultHDR);
#endif

            droneCam.targetTexture = renderTexture;

            foreach(var assigner in m_renderTextureAssigners)
            {
                assigner.SetRenderTexture(renderTexture);
            }

            m_udonBehaviour = GetComponent<UdonBehaviour>();

            var syncVariables = droneCores[currentCam].m_ManualSyncVariables;
            syncVariables.AddOnChangeIsArmCallback(m_udonBehaviour);

            SetLabel();
            TurnOff();

        }

        void SetCamera()
        {
            if (isVirtualCameraMode)
            {
                targetObject.SetParent(droneCores[currentCam].CameraRig);
                targetObject.localPosition = Vector3.zero;
                targetObject.localRotation = Quaternion.identity;
            }
            else
            {
                droneCam.transform.SetParent(droneCores[currentCam].CameraRig);
                droneCam.transform.localPosition = Vector3.zero;
                droneCam.transform.localRotation = Quaternion.identity;
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

            droneCam.enabled = true;

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

            droneCam.enabled = false;
        }

        void SetLabel()
        {
            string ownerName = Networking.GetOwner(droneCores[currentCam].gameObject).displayName;

            var syncVariables = droneCores[currentCam].m_ManualSyncVariables;

            if (!syncVariables.IsArm)
            {
                ownerName = "NO Player";
            }

            text.text = string.Format("[{0}]{1}", currentCam, ownerName);
        }

        /// <summary>
        /// ドローンの操作状態が変化したときのコールバック
        /// </summary>
        public void OnChangeIsArm()
        {
            SetLabel();
        }

        public void Next()
        {
            var prevSyncVariables = droneCores[currentCam].m_ManualSyncVariables;

            currentCam = currentCam + 1 > droneCores.Length - 1 ? 0 : currentCam + 1;

            var nextSyncVariables = droneCores[currentCam].m_ManualSyncVariables;

            prevSyncVariables.RemoveOnChangeIsArmCallback(m_udonBehaviour);
            nextSyncVariables.AddOnChangeIsArmCallback(m_udonBehaviour);

            SetLabel();
            SetCamera();
        }

        public void Prev()
        {
            var prevSyncVariables = droneCores[currentCam].m_ManualSyncVariables;

            currentCam = currentCam - 1 < 0 ? droneCores.Length - 1 : currentCam - 1;

            var nextSyncVariables = droneCores[currentCam].m_ManualSyncVariables;

            prevSyncVariables.RemoveOnChangeIsArmCallback(m_udonBehaviour);
            nextSyncVariables.AddOnChangeIsArmCallback(m_udonBehaviour);
            
            SetLabel();
            SetCamera();
        }

    }
}