using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR

/// <summary>
/// SyncSliderのタグ情報だけ保持するコンポーネント、実行時は使われない
/// </summary>
[RequireComponent(typeof(Slider))]
public class SyncSliderInfo : MonoBehaviour
{
    public string targetTag;
}
#endif