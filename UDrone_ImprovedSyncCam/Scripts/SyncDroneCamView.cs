
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace Kurotori.UDrone
{

    /// <summary>
    /// FollowCameraSync用のカメラビューア
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncDroneCamView : UdonSharpBehaviour
    {
        [HideInInspector, Tooltip("UDroneコントローラ（シーン内のすべてのドローンを指定すること）")]
        public UdonDroneCore[] m_droneCores;

        [SerializeField, Tooltip("FollowCameraSyncオブジェクト")]
        FollowCameraSync m_followCamera;

        [SerializeField, Tooltip("現在追跡中のドローンの番号を表示するラベル")]
        TextMeshProUGUI m_label;

        [SerializeField, Tooltip("カメラ")]
        Camera m_camera;

        [Tooltip("カメラ解像度X")]
        public int ResolutionX = 640;
        [Tooltip("カメラ解像度Y")]
        public int ResolutionY = 480;


        [SerializeField, Tooltip("RenderTextureをセットしたいオブジェクト")]
        private DroneCamRenderTextureAssigner[] m_renderTextureAssigners;

        [SerializeField, Tooltip("カメラ有効時に有効になるオブジェクト群")]
        GameObject[] m_enableCameraObjects;

        [SerializeField, Tooltip("カメラ無効時に有効になるオブジェクト群")]
        GameObject[] m_disableCameraObjects;

        private RenderTexture renderTexture;

        /// <summary>
        /// 現在観測中のドローン番号
        /// </summary>
        [UdonSynced]
        private int m_index = 0;

        /// <summary>
        /// カメラの有効無効
        /// </summary>
        private bool m_cameraEnable = false;

        [SerializeField]
        Material m_material;

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

            m_camera.targetTexture = renderTexture;

            foreach(var assigner in m_renderTextureAssigners)
            {
                assigner.SetRenderTexture(renderTexture);
            }

            m_followCamera.SetNewTarget(m_droneCores[m_index].CameraRig);

            UpdateCameraState();

            m_udonBehaviour = GetComponent<UdonBehaviour>();

            var syncVariables = m_droneCores[m_index].m_ManualSyncVariables;
            syncVariables.AddOnChangeIsArmCallback(m_udonBehaviour);

            m_camera.enabled = false;
        }

        /// <summary>
        /// カメラの状態を更新
        /// </summary>
        void UpdateCameraState()
        {
            m_camera.enabled = m_cameraEnable;

            foreach (var obj in m_enableCameraObjects)
            {
                obj.SetActive(m_cameraEnable);
            }

            foreach (var obj in m_disableCameraObjects)
            {
                obj.SetActive(!m_cameraEnable);
            }
        }

        private void SetNewOwner()
        {
            var owner = Networking.GetOwner(m_droneCores[m_index].gameObject);

            if (Utilities.IsValid(owner))
            {
                // 設定したドローンのオーナーにカメラのオーナーを設定する
                Networking.SetOwner(owner, m_followCamera.gameObject);
            }
        }

        /// <summary>
        /// 新しい追跡ターゲットの指定
        /// </summary>
        private void SetNewTarget()
        {
            // 新たなターゲットを指定します（全体）
            m_followCamera.SetNewTarget(m_droneCores[m_index].CameraRig);
        }

        /// <summary>
        /// 次のドローンを表示するボタンを押したとき
        /// </summary>
        public void OnPushNextDrone()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ChangeNextDrone));
        }

        /// <summary>
        /// 次のドローンに追尾対象を変更する（オーナーのみで実行）
        /// </summary>
        public void ChangeNextDrone()
        {
            var prevSyncVariables = m_droneCores[m_index].m_ManualSyncVariables;

            m_index = m_index + 1 > m_droneCores.Length - 1 ? 0 : m_index + 1;

            SetLabel();

            var nextSyncVariables = m_droneCores[m_index].m_ManualSyncVariables;

            prevSyncVariables.RemoveOnChangeIsArmCallback(m_udonBehaviour);
            nextSyncVariables.AddOnChangeIsArmCallback(m_udonBehaviour);

            SetNewOwner();
            SetNewTarget();
            RequestSerialization();
        }

        /// <summary>
        /// 前のドローンを表示するボタンを押したとき
        /// </summary>
        public void OnPushPrevDrone()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ChangePrevDrone));
        }

        /// <summary>
        /// 前のドローンに追尾対象を変更する（オーナーのみで実行）
        /// </summary>
        public void ChangePrevDrone()
        {
            var prevSyncVariables = m_droneCores[m_index].m_ManualSyncVariables;

            m_index = m_index - 1 < 0 ? m_droneCores.Length - 1 : m_index - 1;

            var nextSyncVariables = m_droneCores[m_index].m_ManualSyncVariables;

            prevSyncVariables.RemoveOnChangeIsArmCallback(m_udonBehaviour);
            nextSyncVariables.AddOnChangeIsArmCallback(m_udonBehaviour);

            SetLabel();

            SetNewOwner();
            SetNewTarget();
            RequestSerialization();
        }

        /// <summary>
        /// カメラを有効にする
        /// </summary>
        public void OnEnableCamera()
        {
            m_cameraEnable = true;
            UpdateCameraState();
        }

        /// <summary>
        /// カメラを無効にする
        /// </summary>
        public void OnDisableCamera()
        {
            m_cameraEnable = false;
            UpdateCameraState();
        }

        /// <summary>
        /// カメラの状態をトグルする。
        /// </summary>
        public void OnToggleCamera()
        {
            m_cameraEnable = !m_cameraEnable;
            UpdateCameraState();
        }

        public override void OnDeserialization()
        {
            SetLabel();

            SetNewTarget();
        }

        private void SetLabel()
        {
            string ownerName = Networking.GetOwner(m_droneCores[m_index].gameObject).displayName;

            var syncVariables = m_droneCores[m_index].m_ManualSyncVariables;

            if (!syncVariables.IsArm)
            {
                ownerName = "NO Player";
            }

            m_label.text = string.Format("[{0}]{1}", m_index, ownerName);
        }


        /// <summary>
        /// ドローンの操作状態が変化したときのコールバック
        /// </summary>
        public void OnChangeIsArm()
        {
            SetLabel();
        }
    }
}
