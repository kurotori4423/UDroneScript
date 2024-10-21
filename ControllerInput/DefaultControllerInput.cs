
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    /// <summary>
    /// VRコントローラー・キーボード入力のコントローラー入力
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DefaultControllerInput : IControllerInput
    {
        [Header("VRコントローラー入力")]
        [SerializeField]
        string vrLHorizontal = "Oculus_CrossPlatform_PrimaryThumbstickHorizontal";
        [SerializeField]
        string vrLVertical = "Oculus_CrossPlatform_PrimaryThumbstickVertical";
        [SerializeField]
        string vrRHorizontal = "Oculus_CrossPlatform_SecondaryThumbstickHorizontal";
        [SerializeField]
        string vrRVertical = "Oculus_CrossPlatform_SecondaryThumbstickVertical";

        public override float GetLHorizontalAxis()
        {
            if(Networking.LocalPlayer.IsUserInVR())
            {
                return Input.GetAxis(vrLHorizontal);
            }
            else
            {
                return KeyboardAxis(KeyCode.A, KeyCode.D, true);
            }
        }

        public override float GetLVerticalAxis()
        {
            if(Networking.LocalPlayer.IsUserInVR())
            {
                return Input.GetAxis(vrLVertical);
            }
            else
            {
                return KeyboardAxis(KeyCode.S, KeyCode.W, true);
            }
        }

        public override float GetRHorizontalAxis()
        {
            if(Networking.LocalPlayer.IsUserInVR())
            {
                return Input.GetAxis(vrRHorizontal);
            }
            else
            {
                return KeyboardAxis(KeyCode.LeftArrow, KeyCode.RightArrow, true);
            }
        }

        public override float GetRVerticalAxis()
        {
            if (Networking.LocalPlayer.IsUserInVR())
            {
                return Input.GetAxis(vrRVertical);
            }
            else
            {
                return KeyboardAxis(KeyCode.DownArrow, KeyCode.UpArrow, true);
            }
        }

        /// <summary>
        /// キーボード入力を-1から1にマップする
        /// </summary>
        /// <param name="key_down"></param>
        /// <param name="key_up"></param>
        /// <param name="releaseCenter"></param>
        /// <returns></returns>
        float KeyboardAxis(KeyCode key_down, KeyCode key_up, bool releaseCenter)
        {
            float value = 0.0f;

            if (releaseCenter)
            {
                if (Input.GetKey(key_down) && Input.GetKeyUp(key_up))
                {
                    value = 0.0f;
                }
                else if (Input.GetKey(key_down))
                {
                    value = -1.0f;
                }
                else if (Input.GetKey(key_up))
                {
                    value = 1.0f;
                }
                else
                {
                    value = 0.0f;
                }
            }
            else
            {
                if (Input.GetKey(key_down) && Input.GetKeyUp(key_up))
                {
                    value = -1.0f;
                }
                else if (Input.GetKey(key_down))
                {
                    value = 0.0f;
                }
                else if (Input.GetKey(key_up))
                {
                    value = 1.0f;
                }
                else
                {
                    value = -1.0f;
                }
            }

            return value;
        }
    }
}