
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class RateSettingPanel : IDroneSettingPanel
    {
        [SerializeField]
        TMP_InputField m_rcRateInputField;
        [SerializeField]
        TMP_InputField m_spRateInputField;
        [SerializeField]
        TMP_InputField m_expoInputField;

        [SerializeField]
        TextMeshProUGUI m_maxAngleVelocityValue;

        float m_rcRate = 1.0f;
        float m_spRate = 0.7f;
        float m_expo = 0.0f;

        public void OnChangeRateValue()
        {
            Debug.Log("[DroneSetting] OnChangeRateValue");
            UpdateInputRateValue();
            UpdateMaxVel();
        }

        public void ApplyRateSetting()
        {
            Debug.Log($"[DroneSetting] Apply RateSetting [{m_rcRate}, {m_spRate}, {m_expo}]");
            foreach (var drone in udrones)
            {
                drone.ApplyDroneRate(m_rcRate, m_spRate, m_expo);
            }
        }

        public override void ApplyDroneSetting()
        {
            CheckInputRateValue();
            UpdateMaxVel(); 
            ApplyRateSetting();
        }

        /// <summary>
        /// 外部から直接レートを設定する
        /// </summary>
        /// <param name="rcRate"></param>
        /// <param name="spRate"></param>
        /// <param name="expo"></param>
        public void SetRateSetting(float rcRate, float spRate, float expo)
        {
            m_rcRateInputField.text = (rcRate.ToString());
            m_spRateInputField.text = (spRate.ToString());
            m_expoInputField.text = (expo.ToString());

            ApplyDroneSetting();
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

        void CheckInputRateValue()
        {
            m_rcRate = ParseOverwrite(m_rcRateInputField.text, m_rcRate);
            m_spRate = ParseOverwrite(m_spRateInputField.text, m_spRate);
            m_expo = ParseOverwrite(m_expoInputField.text, m_expo);
        }

        void UpdateInputRateValue()
        {
            
            var newRcRate = ParseOverwrite(m_rcRateInputField.text, m_rcRate);
            var newSpRate = ParseOverwrite(m_spRateInputField.text, m_spRate);
            var newExpo = ParseOverwrite(m_expoInputField.text, m_expo);

            
            if (newRcRate != m_rcRate)
            {
                Debug.Log($"[DroneSetting] Update RcRate");
                m_rcRate = newRcRate;
                m_rcRateInputField.text = $"{m_rcRate:f2}";
                
            }
            if (newSpRate != m_spRate)
            {
                Debug.Log($"[DroneSetting] Update ScRate");
                m_spRate = newSpRate;
                m_spRateInputField.text = $"{m_spRate:f2}";
            }
            if (newExpo != m_expo)
            {
                Debug.Log($"[DroneSetting] Update expo");
                m_expo = newExpo;
                m_expoInputField.text = $"{m_expo:f2}";
            }
        }

        /// <summary>
        /// レート設定から最大角速度を求めて表示を更新する
        /// </summary>
        private void UpdateMaxVel()
        {
            var maxVel = CalcBetaFlightMaxVel(m_rcRate, m_spRate, m_expo);

            m_maxAngleVelocityValue.text = string.Format("{0}", Mathf.Floor(maxVel));
        }

        /// <summary>
        /// BetaFlightの計算式でレート計算する
        /// </summary>
        /// <param name="rcRate"></param>
        /// <param name="sRate"></param>
        /// <param name="expo"></param>
        /// <returns></returns>
        private float CalcBetaFlightMaxVel(float rcRate, float sRate, float expo)
        {
            float adsValue = 1;

            float superFactor = 1.0f / (1.0f - (adsValue * sRate));
            float rcCommandFactor = (Mathf.Pow(adsValue, 4.0f) * expo) + adsValue * (1 - expo);
            float expoFactor = 200 * rcCommandFactor * rcRate;

            float rate = (expoFactor * superFactor);

            return rate;
        }
    }
}