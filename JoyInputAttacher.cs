
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace Kurotori.UDrone
{
    public class JoyInputAttacher : UdonSharpBehaviour
    {
        [Header("ドローンコントローラー")]
        [SerializeField]
        public GameObject[] controllersObj;

        [Header("UI表示")]
        [SerializeField]
        TextMeshProUGUI currentLHText;
        [SerializeField]
        TextMeshProUGUI currentLVText;
        [SerializeField]
        TextMeshProUGUI currentRHText;
        [SerializeField]
        TextMeshProUGUI currentRVText;

        [Space]
        [SerializeField]
        Toggle InvLHToggle;
        [SerializeField]
        Toggle InvLVToggle;
        [SerializeField]
        Toggle InvRHToggle;
        [SerializeField]
        Toggle InvRVToggle;
        [Space]
        [SerializeField]
        Dropdown LHDropDown;
        [SerializeField]
        Dropdown LVDropDown;
        [SerializeField]
        Dropdown RHDropDown;
        [SerializeField]
        Dropdown RVDropDown;

        [Space]
        [SerializeField]
        TextMeshProUGUI stateDisplay;

        [Space]
        [SerializeField]
        Transform RightStickInputCheck;
        [SerializeField]
        Transform LeftStickInputCheck;

        [SerializeField]
        float moveSpeed = 3.0f;


        int state = 0;

        const int STATE_WAIT = 0;
        const int STATE_L_HORIZONTAL = 1;
        const int STATE_L_VERTICAL = 2;
        const int STATE_R_HORIZONTAL = 3;
        const int STATE_R_VERTICAL = 4;

        string[] stickInputsLabel;

        float[] stickInputPrevValues;
        float[] stickInputTotalValues;

        string prevInput = "";



        bool totallingOn = false;

        // 一定時間中最も変動量が多かったInputを採用する

        void Start()
        {
            stateDisplay.text = "Select the axis you want to set.";

            stickInputsLabel = new string[]
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
            "Joy2 Axis 10"
                };

            stickInputTotalValues = new float[stickInputsLabel.Length];
            stickInputPrevValues = new float[stickInputsLabel.Length];

            for (int i = 0; i < stickInputsLabel.Length; ++i)
            {
                stickInputPrevValues[i] = 0;
                stickInputTotalValues[i] = 0;
            }

        }

        string GetJoyName(int id)
        {
            int joynum = (id + 1) / 10;
            int axisNum = id - joynum * 10;

            return string.Format("Joy{0}Axis{1}", joynum + 1, axisNum + 1);
        }

        void AssignJoyAxis(string functionName)
        {

            Debug.LogWarning(functionName);

            SendCustomEventAll(functionName);

            stateDisplay.text = "Axis Setting Complete!";

            SendCustomEventDelayedSeconds(nameof(EndPhase), 1.0f);
        }

        void SendCustomEventAll(string functionName)
        {
            for (int i = 0; i < controllersObj.Length; ++i)
            {
                var udonBehavior = (UdonBehaviour)controllersObj[i].GetComponent(typeof(UdonBehaviour));

                udonBehavior.SendCustomEvent(functionName);
            }
        }



        public void StartAttachLHorizontal()
        {
            if (state != STATE_WAIT) return;

            state = STATE_L_HORIZONTAL;

            prevInput = currentLHText.text;
            currentLHText.text = "---";

            PreparTotalling();

        }
        public void AttachLHorizontal(int id)
        {
            string functionName = "SetLH_" + GetJoyName(id);

            currentLHText.text = stickInputsLabel[id];
            LHDropDown.SetValueWithoutNotify(id);

            AssignJoyAxis(functionName);
        }

        public void StartAttachLVertical()
        {
            if (state != STATE_WAIT) return;

            state = STATE_L_VERTICAL;

            prevInput = currentLVText.text;
            currentLVText.text = "---";

            PreparTotalling();

        }
        public void AttachLVertical(int id)
        {
            string functionName = "SetLV_" + GetJoyName(id);

            currentLVText.text = stickInputsLabel[id];
            LVDropDown.SetValueWithoutNotify(id);
            AssignJoyAxis(functionName);
        }

        public void StartAttachRHorizontal()
        {
            if (state != STATE_WAIT) return;

            state = STATE_R_HORIZONTAL;

            prevInput = currentRHText.text;
            currentRHText.text = "---";

            PreparTotalling();

        }
        public void AttachRHorizontal(int id)
        {
            string functionName = "SetRH_" + GetJoyName(id);
            currentRHText.text = stickInputsLabel[id];
            RHDropDown.SetValueWithoutNotify(id);
            AssignJoyAxis(functionName);
        }

        public void StartAttachRVertical()
        {
            if (state != STATE_WAIT) return;

            state = STATE_R_VERTICAL;

            prevInput = currentRVText.text;
            currentRVText.text = "---";

            PreparTotalling();

        }
        public void AttachRVertical(int id)
        {
            string functionName = "SetRV_" + GetJoyName(id);

            currentRVText.text = stickInputsLabel[id];
            RVDropDown.SetValueWithoutNotify(id);

            AssignJoyAxis(functionName);

        }

        public void PreparTotalling()
        {
            stateDisplay.text = "After 3 seconds, input the axis you chose for 3 seconds.";
            SendCustomEventDelayedSeconds(nameof(StartTotalling), 3.0f);
        }

        public void StartTotalling()
        {
            for (int i = 0; i < stickInputsLabel.Length; ++i)
            {
                stickInputPrevValues[i] = Input.GetAxis(stickInputsLabel[i]);
            }

            totallingOn = true;

            stateDisplay.text = "Input the your chose axis.";

            SendCustomEventDelayedSeconds(nameof(EndTotalling), 3.0f);
        }

        public void EndTotalling()
        {
            totallingOn = false;

            int maxIndex = -1;
            float maxValue = 0.0f;
            for (int i = 0; i < stickInputsLabel.Length; ++i)
            {
                if (maxValue < stickInputTotalValues[i])
                {
                    maxIndex = i;
                    maxValue = stickInputTotalValues[i];
                }
            }

            if (maxIndex >= 0)
            {
                switch (state)
                {
                    case STATE_L_HORIZONTAL:
                        AttachLHorizontal(maxIndex);
                        break;
                    case STATE_L_VERTICAL:
                        AttachLVertical(maxIndex);
                        break;
                    case STATE_R_HORIZONTAL:
                        AttachRHorizontal(maxIndex);
                        break;
                    case STATE_R_VERTICAL:
                        AttachRVertical(maxIndex);
                        break;
                }
            }
            else
            {
                EndError();
            }
        }

        public void EndPhase()
        {
            state = STATE_WAIT;
            stateDisplay.text = "Select the axis you want to set.";

            for (int i = 0; i < stickInputsLabel.Length; ++i)
            {
                stickInputTotalValues[i] = 0;
            }

        }

        public void EndError()
        {
            stateDisplay.text = "Attaching failed.";

            switch (state)
            {
                case STATE_L_HORIZONTAL:
                    currentLHText.text = prevInput;
                    break;
                case STATE_L_VERTICAL:
                    currentLVText.text = prevInput;
                    break;
                case STATE_R_HORIZONTAL:
                    currentRHText.text = prevInput;
                    break;
                case STATE_R_VERTICAL:
                    currentRVText.text = prevInput;
                    break;
            }

            SendCustomEventDelayedSeconds(nameof(EndPhase), 1.0f);

        }

        private void Update()
        {
            if (state == STATE_WAIT)
            {
                float lx = Input.GetAxis(currentLHText.text) * (InvLHToggle.isOn ? -1 : 1);
                float ly = Input.GetAxis(currentLVText.text) * (InvLVToggle.isOn ? -1 : 1);
                LeftStickInputCheck.localPosition = new Vector3(lx, ly);

                float rx = Input.GetAxis(currentRHText.text) * (InvRHToggle.isOn ? -1 : 1);
                float ry = Input.GetAxis(currentRVText.text) * (InvRVToggle.isOn ? -1 : 1);
                RightStickInputCheck.localPosition = new Vector3(rx, ry);

            }
            else
            {
                if (totallingOn)
                {
                    for (int i = 0; i < stickInputsLabel.Length; ++i)
                    {
                        var currentValue = Input.GetAxis(stickInputsLabel[i]);

                        stickInputTotalValues[i] += Mathf.Abs(stickInputPrevValues[i] - currentValue);
                        stickInputPrevValues[i] = currentValue;
                    }
                }

                float lx = 0;
                float ly = 0;
                float rx = 0;
                float ry = 0;

                switch (state)
                {
                    case STATE_L_HORIZONTAL:
                        lx = Mathf.Sin(Time.time * moveSpeed);
                        break;
                    case STATE_L_VERTICAL:
                        ly = Mathf.Sin(Time.time * moveSpeed);
                        break;
                    case STATE_R_HORIZONTAL:
                        rx = Mathf.Sin(Time.time * moveSpeed);
                        break;
                    case STATE_R_VERTICAL:
                        ry = Mathf.Sin(Time.time * moveSpeed);
                        break;
                }

                LeftStickInputCheck.localPosition = new Vector3(lx, ly);
                RightStickInputCheck.localPosition = new Vector3(rx, ry);
            }
        }


        public void InvLH()
        {
            if (InvLHToggle.isOn)
            {
                SendCustomEventAll("SetInvertLHON");
            }
            else
            {
                SendCustomEventAll("SetInvertLHOFF");
            }
        }

        public void InvLV()
        {
            if (InvLVToggle.isOn)
            {
                SendCustomEventAll("SetInvertLVON");
            }
            else
            {
                SendCustomEventAll("SetInvertLVOFF");
            }
        }

        public void InvRH()
        {
            if (InvRHToggle.isOn)
            {
                SendCustomEventAll("SetInvertRHON");
            }
            else
            {
                SendCustomEventAll("SetInvertRHOFF");
            }
        }

        public void InvRV()
        {
            if (InvRVToggle.isOn)
            {
                SendCustomEventAll("SetInvertRVON");
            }
            else
            {
                SendCustomEventAll("SetInvertRVOFF");
            }
        }

        public void LHDropDownSet()
        {
            int index = LHDropDown.value;
            currentLHText.text = stickInputsLabel[index];

            string functionName = "SetLH_" + GetJoyName(index);
            SendCustomEventAll(functionName);
        }

        public void LVDropDownSet()
        {
            int index = LVDropDown.value;
            currentLVText.text = stickInputsLabel[index];

            string functionName = "SetLV_" + GetJoyName(index);
            SendCustomEventAll(functionName);
        }

        public void RHDropDownSet()
        {
            int index = RHDropDown.value;
            currentRHText.text = stickInputsLabel[index];

            string functionName = "SetRH_" + GetJoyName(index);
            SendCustomEventAll(functionName);
        }

        public void RVDropDownSet()
        {
            int index = RVDropDown.value;
            currentRVText.text = stickInputsLabel[index];

            string functionName = "SetRV_" + GetJoyName(index);
            SendCustomEventAll(functionName);
        }

    }
}