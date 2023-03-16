
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// スピードメーター表示
/// </summary>
public class SpeedMeter : UdonSharpBehaviour
{
    [SerializeField]
    Rigidbody target;

    [SerializeField]
    [Tooltip("水平速度の表示用Text")]
    TextMeshProUGUI[] textHorizontal;
    [SerializeField]
    [Tooltip("垂直速度の表示用Text")]
    TextMeshProUGUI[] textVertical;

    private void Update()
    {
        var horizontalVelocity = new Vector3(target.velocity.x, 0.0f, target.velocity.z).magnitude;
        var verticalVelocity = target.velocity.y;

        horizontalVelocity = horizontalVelocity * 60.0f * 60.0f / 1000.0f;
        verticalVelocity = verticalVelocity * 60.0f * 60.0f / 1000.0f;

        for(int i = 0; i < textHorizontal.Length; ++i)
        {
            textHorizontal[i].text = string.Format("{0:000.0} km/h", horizontalVelocity);
        }
        for (int i = 0; i < textVertical.Length; ++i)
        {
            textVertical[i].text = string.Format("{0:000.0} km/h", verticalVelocity);
        }
        
        
    }
}
