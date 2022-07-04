
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{

    public class PlayerCameraOverride : UdonSharpBehaviour
    {
        [SerializeField]
        Camera overrideCamera;


        void Start()
        {
            overrideCamera.enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                overrideCamera.enabled = !overrideCamera.enabled;
            }
        }
    }
}