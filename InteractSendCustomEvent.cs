
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InteractSendCustomEvent : UdonSharpBehaviour
{
    [SerializeField]
    UdonBehaviour behaviour;
    [SerializeField]
    string eventName;

    public override void Interact()
    {
        behaviour.SendCustomEvent(eventName);
    }
}
