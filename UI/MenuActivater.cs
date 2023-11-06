
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    /// <summary>
    /// メニューを有効にする
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class MenuActivater : UdonSharpBehaviour
    {

        [SerializeField]
        public GameObject menuPanel;

        [HideInInspector]
        public int id;

        [HideInInspector]
        public SettingPanelManager manager;

        public void MenuActivate()
        {
            manager.SetActiveMenu(id);
            menuPanel.SetActive(true);
        }

        public void SetMenuDisable()
        {
            menuPanel.SetActive(false);
        }

        public void SetMenuActive()
        {
            menuPanel.SetActive(true);
        }
    }
}