
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [AddComponentMenu("")]
    public class IDroneColorApplier : UdonSharpBehaviour
    {
        public virtual void SetColor(Color color)
        {
        }
    }
}