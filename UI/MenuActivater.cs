
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    /// <summary>
    /// メニューパネルを有効化するUdon
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class MenuActivater : UdonSharpBehaviour
    {

        /// <summary>
        /// メニューパネルのゲームオブジェクト
        /// </summary>
        [SerializeField, Tooltip("メニューパネルのゲームオブジェクト")]
        public GameObject menuPanel;

        [NonSerialized]
        public int id;

        [NonSerialized]
        public SettingPanelManager manager;

        /// <summary>
        /// メニューを有効化する
        /// </summary>
        public void MenuActivate()
        {
            manager.SetActiveMenu(id);
            menuPanel.SetActive(true);
        }

        /// <summary>
        /// メニューを無効化する
        /// </summary>
        public void SetMenuDisable()
        {
            menuPanel.SetActive(false);
        }

        /// <summary>
        /// メニューの有効化（SettingPanelManagerから呼ばれるやつ）
        /// </summary>
        public void SetMenuActive()
        {
            menuPanel.SetActive(true);
        }
    }
}