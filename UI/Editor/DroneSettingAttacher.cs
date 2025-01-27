using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;
using HarmonyLib;
using System.Linq;

namespace Kurotori.UDrone
{
    /// <summary>
    /// シーン内のドローンオブジェクトなどをチェックして自動的に設定を行うポストプロセススクリプト
    /// </summary>
    public class DroneSettingAttacher : IProcessSceneWithReport
    {
        public int callbackOrder => -1;

        /// <summary>
        /// シーンのビルド時に呼ばれる処理
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="report"></param>
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var sceneRootObjects = scene.GetRootGameObjects();

            List<SettingPanelManager> sceneDroneSettings = new List<SettingPanelManager>();
            List<UdonDroneCore> droneCores = new List<UdonDroneCore>();
            List<DroneCamViewer> sceneDroneCamViewers = new List<DroneCamViewer>();
            List<SyncDroneCamView> sceneSyncDroneCamViews = new List<SyncDroneCamView>();

            List<TimeAttackManager> sceneTimeAttackManagers = new List<TimeAttackManager>();
            List<TimeAttackTrack> sceneTimeAttackTracks = new List<TimeAttackTrack>();

            List<TimeAttackLog> sceneTimeAttackLog = new List<TimeAttackLog>();
            List<AllDroneResetButton> sceneDroneResetButton = new List<AllDroneResetButton>();

            foreach (var sceneRootObject in sceneRootObjects)
            {
                var droneSettings = sceneRootObject.GetComponentsInChildren<SettingPanelManager>(true);
                sceneDroneSettings.AddRange(droneSettings);

                var drones = sceneRootObject.GetComponentsInChildren<UdonDroneCore>(true);
                droneCores.AddRange(drones);

                var droneCamViewers = sceneRootObject.GetComponentsInChildren<DroneCamViewer>(true);
                sceneDroneCamViewers.AddRange(droneCamViewers);

                var timeAttackManagers = sceneRootObject.GetComponentsInChildren<TimeAttackManager>(true);
                sceneTimeAttackManagers.AddRange(timeAttackManagers);

                var timeAttackTracks = sceneRootObject.GetComponentsInChildren<TimeAttackTrack>(true);
                sceneTimeAttackTracks.AddRange(timeAttackTracks);

                var timeAttackLog = sceneRootObject.GetComponentsInChildren<TimeAttackLog>(true);
                sceneTimeAttackLog.AddRange(timeAttackLog);

                var syncCameras = sceneRootObject.GetComponentsInChildren<SyncDroneCamView>(true);
                sceneSyncDroneCamViews.AddRange(syncCameras);

                var droneResetButtons = sceneRootObject.GetComponentsInChildren<AllDroneResetButton>(true);
                sceneDroneResetButton.AddRange(droneResetButtons);
            }
            Debug.Log($"Scene Drone Num:{droneCores.Count}");

            if (sceneDroneSettings.Count == 1)
            {
                var droneSetting = sceneDroneSettings[0];

                // ドローン設定にシーン内のドローンをすべて追加
                droneSetting.udrones = droneCores.ToArray();

                // タイムアタックオブジェクトが存在したら設定する
                if (sceneTimeAttackManagers.Count == 1)
                {
                    droneSetting.timeAttackManager = sceneTimeAttackManagers[0];

                    // タイムアタックログオブジェクトがあったら設定する。
                    if (sceneTimeAttackLog.Count > 0)
                    {
                        sceneTimeAttackManagers[0].TimeAttackLog = sceneTimeAttackLog[0];
                    }

                    // シーンのトラックをマネージャに設定する
                    sceneTimeAttackManagers[0].tracks = sceneTimeAttackTracks.ToArray();

                }
                else if(sceneTimeAttackManagers.Count > 1)
                {
                    Debug.LogError("シーンに TimeAttackManager が２つ以上存在します。シーンには一つだけ置いてください。");
                }

            }
            else
            {
                if (droneCores.Count > 0)
                {
                    if (sceneDroneSettings.Count == 0)
                    {
                        Debug.LogError("シーンにDroneSettingが存在しません。");
                    }
                    else
                    {
                        Debug.LogError("シーンにDroneSettingが２つ以上存在します。シーンには一つだけ置いてください。");
                    }
                }
            }

            foreach (var droneCamViewer in sceneDroneCamViewers)
            {
                droneCamViewer.droneCores = droneCores.ToArray();
            }

            foreach(var syncDroneCamView in sceneSyncDroneCamViews)
            {
                syncDroneCamView.m_droneCores = droneCores.ToArray();
            }

            foreach(var resetDroneButton in sceneDroneResetButton)
            {
                resetDroneButton.droneCores = droneCores.ToArray();
            }
        }
    }
}