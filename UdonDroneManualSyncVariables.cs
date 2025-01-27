
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    /// <summary>
    /// 全体で同期が必要なドローンの同期変数(カメラアングルなど)
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class UdonDroneManualSyncVariables : UdonSharpBehaviour
    {
        /// <summary>
        /// そのプレイヤー固有のカメラアングル
        /// </summary>
        public float localCameraAngle;

        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(CameraAngles))]
        private float m_cameraAngles;
        [HideInInspector]
        public float CameraAngles
        {
            get { return m_cameraAngles; } 
            
            set
            {
                m_cameraAngles = value;

                OnChangeCameraAngle();
            }
        }

        public float localCameraFoV;

        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(CameraFoV))]
        private float m_cameraFoV;
        public float CameraFoV
        {
            get => m_cameraFoV;
            set
            {
                m_cameraFoV = value;
                
                OnChangeCameraFoV();
            }
        }


        /// <summary>
        /// 操作状態
        /// </summary>
        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(IsArm))]
        private bool m_isArm;
        [HideInInspector]
        public bool IsArm
        {
            get { return m_isArm; }
            set 
            { 
                m_isArm = value;
                if(Networking.IsOwner(gameObject))
                {
                    RequestSerialization();
                }

                if (OnChangeIsArmCallback != null)
                {

                    foreach (var behavior in OnChangeIsArmCallback)
                    {
                        if (behavior != null)
                        {
                            behavior.SendCustomEvent("OnChangeIsArm");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 使用状態が変化したときに呼び出すコールバックUdon
        /// </summary>
        private UdonBehaviour[] OnChangeIsArmCallback;


        public UdonDroneCore m_droneCore;

        /// <summary>
        /// 操作状態が変化したときのコールバックを追加します
        /// </summary>
        /// <param name="behavior">コールバック</param>
        public void AddOnChangeIsArmCallback(UdonBehaviour behavior)
        {
            if(OnChangeIsArmCallback == null)
            {
                OnChangeIsArmCallback = new UdonBehaviour[5];
            }

            for (int i = 0; i < OnChangeIsArmCallback.Length; i++)
            {
                if (OnChangeIsArmCallback[i] == null)
                {
                    OnChangeIsArmCallback[i] = behavior;
                    return;
                }

                if (OnChangeIsArmCallback[i].Equals(behavior))
                {
                    // すでに追加済の場合はスキップ
                    return;
                }
            }

            // 満杯の場合は要素数を増やして再登録

            var callbacks = new UdonBehaviour[OnChangeIsArmCallback.Length + 5];

            for (int i = 0; i < OnChangeIsArmCallback.Length; i++)
            {
                callbacks[i] = OnChangeIsArmCallback[i];
            }

            callbacks[OnChangeIsArmCallback.Length] = behavior;

            OnChangeIsArmCallback = callbacks;
        }

        /// <summary>
        /// 操作状態が変化したときのコールバックを削除します
        /// </summary>
        /// <param name="behavior">コールバック</param>
        public void RemoveOnChangeIsArmCallback(UdonBehaviour behavior)
        {
            for (int i = 0; i < OnChangeIsArmCallback.Length; i++)
            {
                if (OnChangeIsArmCallback[i] == null) continue;

                if (OnChangeIsArmCallback[i].Equals(behavior))
                {
                    OnChangeIsArmCallback[i] = null;
                    return;
                }
            }
        }


        public void OnChangeCameraAngle()
        {
            if (Utilities.IsValid(m_droneCore))
            {
                m_droneCore.SetCameraAngle(CameraAngles);
            }
        }

        public void OnChangeCameraFoV()
        {
            if(Utilities.IsValid(m_droneCore))
            {
                m_droneCore.SetCameraFoV();
            }
        }

        /// <summary>
        /// オーナーの場合のみカメラアングルを変更します。
        /// オーナーではないものに関してはローカルカメラアングルとして保持します。
        /// </summary>
        /// <param name="angle"></param>
        public void SetCameraAngles(float angle)
        {
            localCameraAngle = angle;

            if (Utilities.IsValid(Networking.LocalPlayer) && Networking.IsOwner(gameObject))
            {
                CameraAngles = localCameraAngle;
                RequestSerialization();
            }
        }

        public void SetCameraFov(float fov)
        {
            localCameraFoV = fov;

            if(Utilities.IsValid(Networking.LocalPlayer) && Networking.IsOwner(gameObject))
            {
                CameraFoV = localCameraFoV;
                RequestSerialization();
            }
        }

        /// <summary>
        /// オーナーシップが変更されたらローカルカメラアングルを同期します。
        /// </summary>
        /// <param name="player"></param>
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if(player.isLocal)
            {
                CameraAngles = localCameraAngle;
                CameraFoV = localCameraFoV;
                RequestSerialization();
            }
        }
    }
}
