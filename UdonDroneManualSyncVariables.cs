
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

        [UdonSynced(UdonSyncMode.None)]
        public float CameraAngles;

        public UdonDroneCore m_droneCore;

        /// <summary>
        /// オーナーの場合のみカメラアングルを変更します。
        /// オーナーではないものに関してはローカルカメラアングルとして保持します。
        /// </summary>
        /// <param name="angle"></param>
        public void SetCameraAngles(float angle)
        {
            localCameraAngle = angle;
            
            if(Utilities.IsValid(Networking.LocalPlayer) && Networking.LocalPlayer.IsOwner(gameObject))
            {
                CameraAngles = localCameraAngle;
                m_droneCore.SetCameraAngle(CameraAngles);
                RequestSerialization();
            }

        }

        public override void OnDeserialization()
        {
            m_droneCore.SetCameraAngle(CameraAngles);
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
                m_droneCore.SetCameraAngle(CameraAngles);
                RequestSerialization();
            }
        }
    }
}
