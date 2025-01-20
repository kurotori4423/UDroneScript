
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DroneColorApplier : IDroneColorApplier
    {
        [SerializeField]
        MeshRenderer[] Meshes;
        [SerializeField]
        string ParamName;

        public override void SetColor(Color color)
        {
            var materialPropertyBlock = new MaterialPropertyBlock();
            

            foreach(var renderer in Meshes)
            {
                renderer.GetPropertyBlock(materialPropertyBlock);

                materialPropertyBlock.SetColor(ParamName, color);

                renderer.SetPropertyBlock(materialPropertyBlock);
            }
        }
    }
}
