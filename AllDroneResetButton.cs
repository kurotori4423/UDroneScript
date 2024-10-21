
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{

    /// <summary>
    /// 全てのドローンをリセットするボタン
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AllDroneResetButton : UdonSharpBehaviour
    {
        [HideInInspector]
        public UdonDroneCore[] droneCores;

        public void OnPushReset()
        {
            foreach(var drone in droneCores)
            {
                drone.ResetAll_All();
            }
        }
    }
}
