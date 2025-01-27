using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kurotori.UDrone
{

    public class ShortcutViewSetting : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var sceneRootObjects = scene.GetRootGameObjects();

            List<SettingPanelManager> sceneDroneSettings = new List<SettingPanelManager>();
            List<ShortcutKeyInfo> sceneShortcutKeyInfoList = new List<ShortcutKeyInfo>();

            foreach (var sceneRootObject in sceneRootObjects)
            {
                var droneSettings = sceneRootObject.GetComponentsInChildren<SettingPanelManager>(true);
                sceneDroneSettings.AddRange(droneSettings);

                var shortcutKeyInfoList = sceneRootObject.GetComponentsInChildren<ShortcutKeyInfo>(true);
                sceneShortcutKeyInfoList.AddRange(shortcutKeyInfoList);
            }

            if(sceneDroneSettings.Count == 1)
            {
                var droneSetting = sceneDroneSettings[0];

                foreach(var  sceneShortcutKey in sceneShortcutKeyInfoList)
                {
                    var textMesh = sceneShortcutKey.shortcutInfoText;
                    var playerCameraOverrider = sceneShortcutKey.playerCameraOverrider;
                    textMesh.text = $"{playerCameraOverrider.keyCode} : Full screen (for Desktop)\n" +
                        $"{droneSetting.ResetDroneKey} : Reset drone position\n" +
                        $"{droneSetting.FlipOverKey} : Flip over\n" +
                        $"{droneSetting.ResetTimeAttackKey} : Reset time attack\n";
                }
            }

        }
    }
}