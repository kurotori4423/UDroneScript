
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TrailColorApplier : IDroneColorApplier
    {
        [SerializeField]
        TrailRenderer trailRenderer;

        public override void SetColor(Color color)
        {
            trailRenderer.startColor = color;
            trailRenderer.endColor = color;
            var materialPropertyBlock = new MaterialPropertyBlock();
            trailRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetColor("_EmissionColor", color);
            trailRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}