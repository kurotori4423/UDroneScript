
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace Kurotori.UDrone
{
    /// <summary>
    /// ドローンの設定パネルのインタフェース
    /// </summary>
    public class IDroneSettingPanel : UdonSharpBehaviour
    {
        protected UdonDroneCore[] udrones;
        virtual public void SetDroneCores(UdonDroneCore[] _udrones)
        {
            udrones = _udrones;
        }

        /// <summary>
        /// 現在の設定をドローンに適用します
        /// </summary>
        virtual public void ApplyDroneSetting()
        {

        }

    }
}