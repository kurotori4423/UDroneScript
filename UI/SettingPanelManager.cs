
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace Kurotori.UDrone
{
    /// <summary>
    /// ドローンの設定を行うUdon
    /// ドローンの挙動の設定、コントローラーの設定、画面の設定などを行うパネルのマネージャ
    /// 
    /// </summary>
    public class SettingPanelManager : UdonSharpBehaviour
    {
        [SerializeField]
        MenuActivater[] activaters;
        [SerializeField]
        Transform defaultPosition;

        [SerializeField, HideInInspector]
        public UdonDroneCore[] udrones;

        [SerializeField]
        IDroneSettingPanel[] settingPanels;

        [SerializeField]
        private DroneShareCamera shareCamera;

        [SerializeField]
        GameObject mainPanel;

        [SerializeField, HideInInspector]
        public TimeAttackManager timeAttackManager;

        bool displayMainPanel = false; // コントローラーにアタッチ時のメインパネルの表示状態

        [SerializeField, Tooltip("コントローラー入力スクリプト")]
        private IControllerInput[] controllerInputs;

        [SerializeField]
        public KeyCode ResetDroneKey = KeyCode.P;
        [SerializeField]
        public KeyCode FlipOverKey = KeyCode.O;
        [SerializeField]
        public KeyCode ResetTimeAttackKey = KeyCode.P;

        void Start()
        {
            mainPanel.SetActive(true);
            
            Setup();

            SetupPanelTab();
            SetupDroneSettings();
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        void Setup()
        {
            for (int i = 0; i < udrones.Length; i++)
            {
                var controller =udrones[i].GetController();
                controller.m_settingPanel = this;
                controller.droneCam = shareCamera;

                controller.SetControllerInputs(controllerInputs);

                if(timeAttackManager)
                {
                    // タイムアタックオブジェクトが存在する場合
                    controller.m_timeAttackManager = timeAttackManager;
                }
            }
        }

        /// <summary>
        /// 設定パネルのタブの表示状態を初期化します。
        /// </summary>
        void SetupPanelTab()
        {
            for (int i = 0; i < activaters.Length; ++i)
            {
                activaters[i].id = i;
                activaters[i].manager = this;

                if (i == 0)
                {
                    activaters[i].SetMenuActive();
                }
                else
                {
                    activaters[i].SetMenuDisable();
                }
            }
        }

        /// <summary>
        /// ドローン設定機能のセットアップを行います
        /// </summary>
        void SetupDroneSettings()
        {
            foreach(var setting in settingPanels)
            {
                setting.SetDroneCores(udrones);
                setting.ApplyDroneSetting();
            }
        }

        public void SetActiveMenu(int id)
        {
            for (int i = 0; i < activaters.Length; ++i)
            {
                if (i != id)
                {
                    activaters[i].SetMenuDisable();
                }
            }
        }

        /// <summary>
        /// メニューをアタッチします。
        /// </summary>
        /// <param name="pivot"></param>
        public void AttachController(Transform pivot)
        {
            this.transform.position = pivot.position;
            this.transform.rotation = pivot.rotation;

            mainPanel.SetActive(displayMainPanel);
        }

        public void ToggleMainPanelDisplay()
        {
            displayMainPanel = !displayMainPanel;

            mainPanel.SetActive(displayMainPanel);
        }

        /// <summary>
        /// メニューをデフォルト位置に戻す
        /// </summary>
        public void SetDefaultPosition()
        {
            this.transform.position = defaultPosition.position;
            this.transform.rotation = defaultPosition.rotation;

            mainPanel.SetActive(true);
        }
    }
}