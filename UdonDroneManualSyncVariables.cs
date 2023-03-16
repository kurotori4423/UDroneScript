
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class UdonDroneManualSyncVariables : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.None)]
        public float CameraAngles;

        [HideInInspector]
        public UdonDroneController droneController;

        public void SetCameraAngles(float angle)
        {
            if (Networking.LocalPlayer != null && !Networking.LocalPlayer.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            CameraAngles = angle;
            RequestSerialization();

        }

        public override void OnDeserialization()
        {
            droneController.SetCameraAngleGlobal();
        }
    }
}
