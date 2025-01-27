
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{

    public class IControllerInput : UdonSharpBehaviour
    {
        virtual public float GetLHorizontalAxis()
        {
            return 0;
        }

        virtual public float GetLVerticalAxis()
        {
            return 0;
        }

        virtual public float GetRHorizontalAxis()
        {
            return 0;
        }

        virtual public float GetRVerticalAxis()
        {
            return 0;
        }

        virtual public bool GetResetButtonInput()
        {
            return false;
        }

        virtual public bool GetFlipOverButtonInput()
        {
            return false;
        }

        virtual public bool GetTimeAttackResetButtonInput()
        {
            return false;
        }

    }
}