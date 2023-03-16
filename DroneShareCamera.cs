
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace Kurotori.UDrone
{
    /// <summary>
    /// ドローン間でシェアされるカメラ
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DroneShareCamera : UdonSharpBehaviour
    {
        [HideInInspector]
        public Rigidbody attachedDrone;

        [SerializeField]
        TextMeshProUGUI horizontalSpeed;
        [SerializeField]
        TextMeshProUGUI verticalSpeed;



        private void Update()
        {
            if(attachedDrone)
            {
                var horizontalVelocity = new Vector3(attachedDrone.velocity.x, 0.0f, attachedDrone.velocity.z).magnitude;
                var verticalVelocity = attachedDrone.velocity.y;

                horizontalVelocity = horizontalVelocity * 60.0f * 60.0f / 1000.0f;
                verticalVelocity = verticalVelocity * 60.0f * 60.0f / 1000.0f;

                horizontalSpeed.text = string.Format("{0:000.0} km/h", horizontalVelocity);
                verticalSpeed.text = string.Format("{0:000.0} km/h", verticalVelocity);
            }
        }

        public void AttachDrone(Rigidbody drone)
        {
            attachedDrone = drone;
        }
    }
}
