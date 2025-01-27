
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MIDIControllerInput : IControllerInput
    {
        private float LHorizontalValue = 0f;
        private float LVerticalValue = 0f;
        private float RHorizontalValue = 0f;
        private float RVerticalValue = 0f;

        private bool resetSignal = false;

        private bool flipOverSignal = false;
        private bool timeAttackResetSignal = false;

        public override float GetLHorizontalAxis()
        {
            return LHorizontalValue;
        }

        public override float GetLVerticalAxis()
        {
            return LVerticalValue;
        }

        public override float GetRHorizontalAxis()
        {
            return RHorizontalValue;
        }

        public override float GetRVerticalAxis()
        {
            return RVerticalValue;
        }

        public override bool GetResetButtonInput()
        {
            return resetSignal;
        }

        public override bool GetFlipOverButtonInput()
        {
            return flipOverSignal;
        }

        public override bool GetTimeAttackResetButtonInput()
        {
            return timeAttackResetSignal;
        }


        public override void MidiControlChange(int channel, int number, int value)
        {
            int input = (number << 7) + value;
            float stickInput = ((float)input / 16384.0f) * 2.0f - 1.0f;

            switch (channel)
            {
                case 0:
                    LHorizontalValue = stickInput;
                    //Debug.Log($"INPUT:LH {LHorizontalValue}");
                    break;
                case 1:
                    LVerticalValue = stickInput;
                    //Debug.Log($"INPUT:LV {LVerticalValue}");

                    break;
                case 2:
                    RHorizontalValue = stickInput;
                    //Debug.Log($"INPUT:RH {LHorizontalValue}");
                    break;
                case 3:
                    RVerticalValue = stickInput;
                    //Debug.Log($"INPUT:RV {LHorizontalValue}");
                    break;
            }
        }

        public override void MidiNoteOn(int channel, int number, int velocity)
        {
            switch(channel)
            {
                case 0:
                    resetSignal = (number == 1);
                    Debug.Log($"ResetSignal:{resetSignal} {number}");
                    break;
                case 1:
                    flipOverSignal = (number == 1);
                    break;
                case 2:
                    timeAttackResetSignal = (number == 1);
                    break;
            }
        }

    }
}