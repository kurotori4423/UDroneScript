
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace Kurotori.UDrone
{
    public class InputChacker : UdonSharpBehaviour
    {
        string[] stickInputs;

        [SerializeField]
        GameObject prefab;

        [SerializeField]
        Transform parent;

        [SerializeField]
        TextMeshProUGUI inputMethodText;

        GameObject[] sticks;
        Slider[] stickValues;
        TextMeshProUGUI[] valueLabels;

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
            valueLabels = new TextMeshProUGUI[stickInputs.Length];

            for (int i = 0; i < stickInputs.Length; ++i)
            {
                sticks[i] = Instantiate(prefab);
                sticks[i].transform.SetParent(parent);
                sticks[i].transform.localScale = Vector3.one;
                sticks[i].transform.localPosition = Vector3.zero;
                sticks[i].transform.localRotation = Quaternion.identity;

                var labels = sticks[i].GetComponentsInChildren<TextMeshProUGUI>();


                foreach(var label in labels)
                {
                    if(label.gameObject.name.Equals("InputLabel"))
                    {
                        label.text = stickInputs[i];
                    }
                    else if(label.gameObject.name.Equals("ValueLabel"))
                    {
                        valueLabels[i] = label;
                    }
                }



                stickValues[i] = sticks[i].GetComponentInChildren<Slider>();
            }
        }

        private void Update()
        {
            for (int i = 0; i < stickInputs.Length; ++i)
            {
                var value = Input.GetAxisRaw(stickInputs[i]);
                stickValues[i].value = value;
                valueLabels[i].text = value.ToString();
            }
            VRCInputMethod inputMethod = InputManager.GetLastUsedInputMethod();

            switch(inputMethod)
            {
                case VRCInputMethod.Keyboard:
                    inputMethodText.text = "Keyboad";
                    break;
                case VRCInputMethod.Mouse:
                    inputMethodText.text = "Mouse";
                    break;
                case VRCInputMethod.Controller:
                    inputMethodText.text = "Controller";
                    break;
                case VRCInputMethod.Gaze:
                    inputMethodText.text = "Gaze";
                    break;
                case VRCInputMethod.Vive:
                    inputMethodText.text = "Vive";
                    break;
                case VRCInputMethod.Oculus:
                    inputMethodText.text = "Oculus";
                    break;
                case VRCInputMethod.ViveXr:
                    inputMethodText.text = "ViveXr";
                    break;
                case VRCInputMethod.Index:
                    inputMethodText.text = "Index";
                    break;
                case VRCInputMethod.HPMotionController:
                    inputMethodText.text = "HPMotionController";
                    break;
                case VRCInputMethod.Osc:
                    inputMethodText.text = "Osc";
                    break;
                case VRCInputMethod.QuestHands:
                    inputMethodText.text = "QuestHands";
                    break;
                case VRCInputMethod.Generic:
                    inputMethodText.text = "Generic";
                    break;
                case VRCInputMethod.Touch:
                    inputMethodText.text = "Touch";
                    break;
                case VRCInputMethod.OpenXRGeneric:
                    inputMethodText.text = "OpenXRGeneric";
                    break;
                case VRCInputMethod.Pico:
                    inputMethodText.text = "Pico";
                    break;
                default:
                    inputMethodText.text = $"Unkown : {inputMethod}";
                    break;
            }

        }
    }
}