
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TextColorApplier : IDroneColorApplier
    {
        [SerializeField]
        TMP_Text text;

        public override void SetColor(Color color)
        {
            text.color = color;
        }
    }
}