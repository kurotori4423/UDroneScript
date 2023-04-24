using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
/// <summary>
/// SyncTextのタグ丈夫だけ保持するコンポーネント
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class SyncTextInfo : MonoBehaviour
{
    public string targetTag;
}
#endif
