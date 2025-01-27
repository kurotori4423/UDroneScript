
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kurotori.UDrone
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DroneNamePlate : UdonSharpBehaviour
    {
        [SerializeField]
        public float NamePlateScale = 1.0f;
        [SerializeField]
        public float PlateHeight = 0.5f;

        [SerializeField]
        private Transform nameplateBase;

        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private UdonDroneCore droneCore;
        [SerializeField]
        private UdonDroneManualSyncVariables syncVariables;

        bool IsShow = true;

        private void Start()
        {
            syncVariables = droneCore.m_ManualSyncVariables;
            syncVariables.AddOnChangeIsArmCallback(GetComponent<UdonBehaviour>());
            nameplateBase.gameObject.SetActive(false);
        }

        public void OnChangeIsArm()
        {
            if (Utilities.IsValid(syncVariables))
            {
                nameplateBase.gameObject.SetActive(syncVariables.IsArm && IsShow);

                if (syncVariables.IsArm)
                {
                    nameText.text = Networking.GetOwner(droneCore.gameObject).displayName;
                }
            }
        }

        public void ShowDroneNamePlate(bool visible)
        {
            IsShow = visible;
            OnChangeIsArm();
        }

        private void LateUpdate()
        {
            if (syncVariables.IsArm)
            {
                var platePosition = droneCore.transform.position + Vector3.up * PlateHeight;
                nameplateBase.position = platePosition;

                var headPos = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                nameplateBase.LookAt(headPos);

                var distance = Vector3.Distance(nameplateBase.position, headPos);
                nameplateBase.localScale = distance * NamePlateScale * Vector3.one;
            }
        }
    }
}