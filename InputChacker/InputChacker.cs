
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace Kurotori.UDrone
{
    public class InputChacker : UdonSharpBehaviour
    {
        string[] stickInputs;

        [SerializeField]
        GameObject prefab;

        [SerializeField]
        Transform parent;

        GameObject[] sticks;
        Slider[] stickValues;

        void Start()
        {
            stickInputs = new string[]
                {
            "Joy1 Axis 1",
            "Joy1 Axis 2",
            "Joy1 Axis 3",
            "Joy1 Axis 4",
            "Joy1 Axis 5",
            "Joy1 Axis 6",
            "Joy1 Axis 7",
            "Joy1 Axis 8",
            "Joy1 Axis 9",
            "Joy1 Axis 10",
            "Joy2 Axis 1",
            "Joy2 Axis 2",
            "Joy2 Axis 3",
            "Joy2 Axis 4",
            "Joy2 Axis 5",
            "Joy2 Axis 6",
            "Joy2 Axis 7",
            "Joy2 Axis 8",
            "Joy2 Axis 9",
            "Joy2 Axis 10",
            "Horizontal",
            "Vertical",
            "Oculus_GearVR_LThumbstickX",
            "Oculus_GearVR_LThumbstickY",
            "Oculus_GearVR_RThumbstickX",
            "Oculus_GearVR_RThumbstickY",
            "Oculus_GearVR_DpadX",
            "Oculus_GearVR_DpadY",
            "Oculus_GearVR_LIndexTrigger",
            "Oculus_GearVR_RIndexTrigger",
            "Oculus_CrossPlatform_PrimaryIndexTrigger",
            "Oculus_CrossPlatform_SecondaryIndexTrigger",
            "Oculus_CrossPlatform_PrimaryHandTrigger",
            "Oculus_CrossPlatform_SecondaryHandTrigger",
            "Oculus_CrossPlatform_PrimaryThumbstickHorizontal",
            "Oculus_CrossPlatform_PrimaryThumbstickVertical",
            "Oculus_CrossPlatform_SecondaryThumbstickHorizontal",
            "Oculus_CrossPlatform_SecondaryThumbstickVertical"
            };


            sticks = new GameObject[stickInputs.Length];
            stickValues = new Slider[stickInputs.Length];

            for (int i = 0; i < stickInputs.Length; ++i)
            {
                sticks[i] = VRCInstantiate(prefab);
                sticks[i].transform.SetParent(parent);
                sticks[i].transform.localScale = Vector3.one;
                sticks[i].transform.localPosition = Vector3.zero;
                sticks[i].transform.localRotation = Quaternion.identity;

                var label = sticks[i].GetComponentInChildren<TextMeshProUGUI>();
                label.text = stickInputs[i];


                stickValues[i] = sticks[i].GetComponentInChildren<Slider>();
            }
        }

        private void Update()
        {
            for (int i = 0; i < stickInputs.Length; ++i)
            {
                var value = Input.GetAxisRaw(stickInputs[i]);
                stickValues[i].value = value;
            }
        }
    }
}