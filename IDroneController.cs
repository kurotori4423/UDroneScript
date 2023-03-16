
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone {
    /// <summary>
    /// ドローン操作のインタフェース
    /// </summary>
    public class IDroneController : UdonSharpBehaviour
    {
        protected UdonDroneCore droneCore;
        public virtual void SetDrone(UdonDroneCore droneCore)
        {
            this.droneCore = droneCore;
        }
    }
}
