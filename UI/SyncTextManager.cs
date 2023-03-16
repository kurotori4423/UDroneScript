
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SyncTextManager : UdonSharpBehaviour
{
    [SerializeField]
    TextMeshProUGUI[] tmProTexts;

    [SerializeField]
    string shareText;

    void Start()
    {
        foreach(var text in tmProTexts)
        {
            text.text = shareText;
        }
    }

    public void SetText(string newText)
    {
        shareText = newText;

        foreach(var text in tmProTexts)
        {
            text.text = shareText;
        }
    }
}
