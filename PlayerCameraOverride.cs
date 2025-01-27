
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

        public KeyCode keyCode = KeyCode.F10;


        void Start()
        {
            overrideCamera.enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(keyCode))
            {
                overrideCamera.enabled = !overrideCamera.enabled;
            }
        }
    }
}