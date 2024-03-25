
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
        UdonDroneCore[] udrones;

        [SerializeField]
        IDroneSettingPanel[] settingPanels;

        [SerializeField]
        GameObject mainPanel;

        bool displayMainPanel = false; // コントローラーにアタッチ時のメインパネルの表示状態

        void Start()
        {
            mainPanel.SetActive(true);

            SetupPanelTab();
            SetupDroneSettings();
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