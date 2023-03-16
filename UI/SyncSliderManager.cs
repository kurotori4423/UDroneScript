
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 設定値を複数のスライダーで同期させる。
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SyncSliderManager : UdonSharpBehaviour
{
    [SerializeField]
    Slider[] sliders;

    [SerializeField]
    UdonBehaviour[] udons;
    [SerializeField]
    string customEventName;

    [SerializeField, Range(0,1)]
    private float shareValue;

    void Start()
    {
        foreach (var slider in sliders)
        {
            slider.SetValueWithoutNotify(shareValue);
        }
    }

    /// <summary>
    /// 変化したスライダーを取得する
    /// </summary>
    /// <returns></returns>
    Slider GetChangedSlider()
    {
        foreach(var slider in sliders)
        {
            if (!Mathf.Approximately(slider.value, shareValue))
            {
                return slider;
            }
        }

        return null;
    }

    public void OnSliderChange()
    {
        var changedSlider = GetChangedSlider();

        var changedValue = shareValue;

        if (changedSlider != null)
        {
            changedValue = changedSlider.value;
        }
        

        foreach (var slider in sliders)
        {
            slider.SetValueWithoutNotify(changedValue);
        }

        shareValue = changedValue;

        foreach(var udon in udons)
        {
            udon.SendCustomEvent(customEventName);
        }
    }

    public void SetValue(float changedValue)
    {
        foreach (var slider in sliders)
        {
            slider.SetValueWithoutNotify(changedValue);
        }

        shareValue = changedValue;

        foreach (var udon in udons)
        {
            udon.SendCustomEvent(customEventName);
        }
    }

    public float GetValue()
    {
        return shareValue;
    }
}
