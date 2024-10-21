using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    /// <summary>
    /// マニュアル同期型フォローカメラ
    /// このオブジェクトのオーナーから特定のインターバルで回転を同期し、クライアントはその同期された値を用いて球面補間によって滑らかな回転を再現する。
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FollowCameraSync : UdonSharpBehaviour
    {
        [SerializeField, Tooltip("同期インターバル")] private float m_interval = 0.3f;
        [SerializeField, Tooltip("追跡ターゲット")] private Transform m_target = null;

        /// <summary>
        /// 同期される回転変数
        /// </summary>
        [UdonSynced]
        private Quaternion m_syncRotation = Quaternion.identity;

        /// <summary>
        /// 過去の回転
        /// </summary>
        private Quaternion m_prevRotation = Quaternion.identity;

        /// <summary>
        /// 現在の回転
        /// </summary>
        private Quaternion m_currentRotation = Quaternion.identity;

        /// <summary>
        /// 回転の補間率
        /// </summary>
        private float m_lerpTimer = 0;

        void Start()
        {
            // オーナーであれば同期を開始する。
            if (IsGameObjectOwner())
            {
                UpdateParInterval();
            }
        }

        /// <summary>
        /// 一定間隔で実行する
        /// </summary>
        public void UpdateParInterval()
        {
            // オーナーで無くなったら実行を停止する。
            if (!IsGameObjectOwner()) return;

            m_syncRotation = m_target.rotation;
            RequestSerialization();

            SendCustomEventDelayedSeconds(nameof(UpdateParInterval), m_interval);
        }


        /// <summary>
        /// このオブジェクトのオーナーかどうか
        /// </summary>
        /// <returns>オーナーであればtrue、オーナーではない、もしくはローカルプレイヤーが不正であればfalse</returns>
        bool IsGameObjectOwner()
        {
            if (Utilities.IsValid(Networking.LocalPlayer))
            {
                return Networking.LocalPlayer.IsOwner(gameObject);
            }
            else
            {
                return false;
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            // オーナーが切り替わったら新しいオーナーがUpdateParIntervalを実行する
            if (player.isLocal)
            {
                UpdateParInterval();
            }
        }

        public void Update()
        {
            transform.position = m_target.position;
            // オーナー以外の処理
            if (IsGameObjectOwner())
            {
                transform.rotation = m_target.rotation;
            }
            else
            {
                m_lerpTimer += Time.deltaTime;
                // 回転を球面補間する
                transform.rotation = Quaternion.Slerp(m_prevRotation, m_currentRotation, Mathf.Min(m_lerpTimer / m_interval, 1.0f));
            }

        }

        public override void OnDeserialization()
        {
            // 回転値が同期されたら過去の回転と現在の回転をそれぞれ更新する。
            m_prevRotation = m_currentRotation;
            m_currentRotation = m_syncRotation;
            m_lerpTimer = 0;

        }

        /// <summary>
        /// この関数を実行した人をオーナーに指定する。
        /// </summary>
        public void OwnerChange()
        {
            if (!IsGameObjectOwner())
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
        }

        /// <summary>
        /// 新しい追跡ターゲットを指定する
        /// </summary>
        /// <param name="_target">新しい追跡ターゲット</param>
        public void SetNewTarget(Transform _target)
        {
            m_target = _target;
        }

    }
}