
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class PIDSettingPanel : IDroneSettingPanel
    {
        [SerializeField]
        public bool isAnglePID;

        [SerializeField]
        InputField m_pInputField;
        [SerializeField]
        InputField m_iInputField;
        [SerializeField]
        InputField m_dInputField;

        float m_p = 1.0f;
        float m_i = 0.7f;
        float m_d = 0.0f;

        public void OnChangePIDValue()
        {
            Debug.Log("[DroneSetting] OnChangePIDValue");

            UpdateInputValue();
        }

        void UpdateInputValue()
        {
            var newP = ParseOverwrite(m_pInputField.text, m_p);
            var newI = ParseOverwrite(m_iInputField.text, m_i);
            var newD = ParseOverwrite(m_dInputField.text, m_d);

            if(!Mathf.Approximately( newP , m_p))
            {
                Debug.Log($"[DroneSetting] Update p");
                m_p = newP;
                m_pInputField.text = $"{m_p}";
            }
            if (!Mathf.Approximately(newI, m_i))
            {
                Debug.Log($"[DroneSetting] Update i");
                m_i = newI;
                m_iInputField.text = $"{m_i}";
            }
            if (!Mathf.Approximately(newD, m_d))
            {
                Debug.Log($"[DroneSetting] Update d");
                m_d = newD;
                m_dInputField.text = $"{m_d}";
            }
        }

        public void ApplyPIDSetting()
        {
            foreach (var drone in udrones)
            {
                if (isAnglePID)
                {
                    Debug.Log($"[DroneSetting] Apply Angle PID [{m_p}, {m_i}, {m_d}]");
                    drone.ApplyAnglePID(m_p, m_i, m_d);
                }
                else
                {
                    Debug.Log($"[DroneSetting] Apply Acro PID [{m_p}, {m_i}, {m_d}]");
                    drone.ApplyAngluerVPID(m_p, m_i, m_d);
                }
            }
        }

        public override void ApplyDroneSetting()
        {
            UpdateInputValue();
            ApplyPIDSetting();
        }

        /// <summary>
        /// inputがfloatにパースできた場合はその値を、できない場合はdefaultValueの値を返す。
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        float ParseOverwrite(string input, float defaultValue)
        {
            float output = 0;
            if (float.TryParse(input, out output))
            {
                return output;
            }
            else
            {
                return defaultValue;
            }

        }
    }
}