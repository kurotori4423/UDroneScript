
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using TMPro;

namespace Kurotori.UDrone
{
    /// <summary>
    /// ドローンのコントローラー
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class UdonDroneController : IDroneController
    {
        float tmpRunSpeed;
        float tmpStrafeSpeed;
        float tmpWalkSpeed;

        bool controlling;

        // RenderTextureカメラは負荷が高いので、機体1つごとに1RenderTextureではなく、1台のカメラを共有します。

        [Header("カメラ")]
        [SerializeField]
        [Tooltip("機体側のカメラ位置")]
        public Transform droneCamRig;

        [SerializeField, HideInInspector]
        public DroneShareCamera droneCam;

        [SerializeField]
        UdonDroneManualSyncVariables syncVariables;

        [SerializeField]
        bool mode1 = true;

        [SerializeField]
        GameObject droneCamDisplay;

        [Header("構成要素")]
        [SerializeField]
        [Tooltip("VRCStation")]
        VRCStation station;

        Transform stationTransform;

        [SerializeField]
        GameObject ControllerVisual;

        [Header("SettingPanel")]
        [SerializeField] Transform m_settingPanelPivot;

        [HideInInspector]
        public SettingPanelManager m_settingPanel;

        [HideInInspector]
        public TimeAttackManager m_timeAttackManager;

        VRC_Pickup pickup;
        
        Collider pickupCollider;


        // リセット用初期姿勢
        Vector3 initPosition;
        Quaternion initRotation;

        // 飛行モード
        int flymode = 0;
        const int MODE_ANGLE = 0;
        const int MODE_ACRO = 1;

        //bool detailPanelOn = false;

        // 入力系統
        int m_InputTypeIndex = 0;
        IControllerInput[] m_controllerInputs;

        void Start()
        {

            controlling = false;
            droneCamDisplay.SetActive(false);

            initPosition = droneCamDisplay.transform.position;
            initRotation = droneCamDisplay.transform.rotation;

            pickup = (VRC_Pickup)gameObject.GetComponent(typeof(VRC_Pickup));
            pickupCollider = gameObject.GetComponent<Collider>();
            
            
            // Stationの設定
            if (station)
            {
                stationTransform = station.gameObject.transform;
                station.disableStationExit = true;

                station.gameObject.SetActive(false);
            }

        }

        #region Pickup
        public override void OnPickup()
        {

        }

        public override void OnDrop()
        {
        }

        public override void OnPickupUseDown()
        {


            // ピックアップをオフに
            pickup.pickupable = false;
            //pickupRenderer.enabled = false;
            ControllerVisual.SetActive(false);
            pickupCollider.enabled = false;
            pickup.Drop();

            // ステーションの位置をプレイヤーの足元に移動させ、向いている方向に設置
            var playerPos = Networking.LocalPlayer.GetPosition();
            var playerRotation = Networking.LocalPlayer.GetRotation();
            var playerRotationDir = Vector3.ProjectOnPlane(playerRotation * Vector3.forward, Vector3.up);

            stationTransform.position = playerPos;
            stationTransform.LookAt(playerPos + playerRotationDir, Vector3.up);

            // プレイヤーをStationに座らせる
            station.gameObject.SetActive(true);
            station.UseStation(Networking.LocalPlayer);

            // オーナー権を設定
            if (droneCore && !Networking.LocalPlayer.IsOwner(droneCore.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, droneCore.gameObject);
            }

            DisplayOpen();

            // フラグ設定
            controlling = true;

            // 他プレイヤーへ非表示指示
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HidePickup));

            if (droneCore)
            {
                droneCore.SetIsArmLocal(true);
            }

            syncVariables.IsArm = true;

            m_settingPanel.AttachController(m_settingPanelPivot);
        }

        public void HidePickup()
        {
            if (!controlling)
            {
                pickup.pickupable = false;
                ControllerVisual.SetActive(false);
                pickupCollider.enabled = false;
            }
        }

        public void ShowPickup()
        {
            if (!controlling)
            {
                pickup.pickupable = true;
                ControllerVisual.SetActive(true);
                pickupCollider.enabled = true;
            }
        }

        public override void OnPickupUseUp()
        {
        }

        #endregion

        /// <summary>
        /// コントロールから離れる処理
        /// </summary>
        public void ExitControl()
        {
            // プレイヤーのStationからの離脱
            station.ExitStation(Networking.LocalPlayer);
            ResetController();
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowPickup));
        }

        public void ResetController()
        {
            station.gameObject.SetActive(false);

            // ディスプレイ類をオフにする
            DisplayOff();

            // パネルを元の位置に戻します。
            m_settingPanel.SetDefaultPosition();

            pickup.pickupable = true;
            ControllerVisual.SetActive(true);
            pickupCollider.enabled = true;
            controlling = false;

            if (droneCore)
            {
                droneCore.SetIsArmLocal(false);
            }

            syncVariables.IsArm = false;
        }

        /// <summary>
        /// コントローラー入力系統をセットする。
        /// </summary>
        /// <param name="inputs"></param>
        public void SetControllerInputs(IControllerInput[] inputs)
        {
            m_controllerInputs = inputs;
        }

        /// <summary>
        /// コントロール用の表示を展開する。
        /// </summary>
        void DisplayOpen()
        {
            // FPVディスプレイを表示
            DisplayFPVDrone();

            // セッティングパネルを表示
        }

        void DisplayOff()
        {
            droneCamDisplay.SetActive(false);
        }

        void DisplayFPVDrone()
        {
            droneCamDisplay.SetActive(true);

            //CameraAngleChange();

            // ドローンカメラを設定する
            if (droneCam != null && droneCamRig != null)
            {
                droneCam.transform.SetParent(droneCamRig);
                droneCam.transform.localPosition = Vector3.zero;
                droneCam.transform.localRotation = Quaternion.identity;
                droneCam.AttachDrone(droneCore.GetRigidbody());
            }
            else
            {
                Debug.LogError("DroneCame not found");
            }

        }

        public void ResetOwner()
        {
            if (!Networking.LocalPlayer.IsOwner(droneCore.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, droneCore.gameObject);
        }

        public override void SetDrone(UdonDroneCore droneCore)
        {
            this.droneCore = droneCore;
            SetCameraAngleGlobal();

            Debug.Log("Set Drone");
        }

        public void ChangeMode()
        {
            mode1 = !mode1;
        }

        public void SetMode(bool isMode1)
        {
            mode1 = isMode1;
        }

        public void ChangeFlyMode()
        {
            if (droneCore)
            {
                switch (flymode)
                {
                    case MODE_ANGLE:
                        flymode = MODE_ACRO;
                        droneCore.ChangeMode_Acro();
                        break;
                    case MODE_ACRO:
                        flymode = MODE_ANGLE;
                        droneCore.ChangeMode_Angle();
                        break;
                }
            }
        }

        public void SettingPanelToggle()
        {
            m_settingPanel.ToggleMainPanelDisplay();
        }

        public void SetCameraAngleGlobal()
        {
            Debug.Log("Set Global Camera Angle :" + syncVariables.CameraAngles.ToString());
            droneCore.SetCameraAngle(syncVariables.CameraAngles);
        }

        public void ResetDrone()
        {
            if (droneCore)
            {
                droneCore.ResetAll_All();
            }
        }

        public void FlipOverDrone()
        {
            if(droneCore)
            {
                droneCore.FlipOver();
            }
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (player.isLocal && controlling)
            {
                ExitControl();
            }
        }

        public void OnPushTimerResetButton()
        {
            if(m_timeAttackManager != null)
                m_timeAttackManager.ResetRaceAll();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            // コントロール状態なら全体へ通知する
            if(controlling)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HidePickup));
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            // オーナーが不正な場合、使っていたプレイヤーがLeftした可能性が高い
            var owner = Networking.GetOwner(gameObject);
            if (Utilities.IsValid(owner))
            {
                if(owner.isLocal && !controlling)
                {
                    Debug.Log("UDRONE: OnPlayerLeft ShowPickup");
                    ResetController();
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowPickup));
                }
            }

        }

        private void Update()
        {
            if (Networking.LocalPlayer == null) return;

            if (controlling)
            {
                Control();

                if (m_controllerInputs[m_InputTypeIndex].GetResetButtonInput())
                {
                    ResetDrone();
                }

                if (m_controllerInputs[m_InputTypeIndex].GetFlipOverButtonInput())
                {
                    FlipOverDrone();
                }

                if (Input.GetKeyDown(m_settingPanel.ResetDroneKey))
                {
                    ResetDrone();
                }

                if(Input.GetKeyDown(m_settingPanel.FlipOverKey))
                {
                    FlipOverDrone();
                }
            }
        }

        public void SetControllerInput(int index)
        {
            m_InputTypeIndex = index;
        }


        void Control()
        {
            if(mode1)
            {
                droneCore.SetRudder(m_controllerInputs[m_InputTypeIndex].GetLHorizontalAxis());
                droneCore.SetElevator(m_controllerInputs[m_InputTypeIndex].GetLVerticalAxis());
                droneCore.SetAileron(m_controllerInputs[m_InputTypeIndex].GetRHorizontalAxis());
                droneCore.SetThrottle(m_controllerInputs[m_InputTypeIndex].GetRVerticalAxis());
            }
            else
            {
                droneCore.SetRudder(m_controllerInputs[m_InputTypeIndex].GetLHorizontalAxis());
                droneCore.SetThrottle(m_controllerInputs[m_InputTypeIndex].GetLVerticalAxis());
                droneCore.SetAileron(m_controllerInputs[m_InputTypeIndex].GetRHorizontalAxis());
                droneCore.SetElevator(m_controllerInputs[m_InputTypeIndex].GetRVerticalAxis());
            }
        }

        void DesktopAngleControl()
        {
            if (mode1)
            {
                droneCore.SetRudder(KeyboardAxis(KeyCode.A, KeyCode.D, true));
                droneCore.SetElevator(KeyboardAxis(KeyCode.S, KeyCode.W, true));
                droneCore.SetAileron(KeyboardAxis(KeyCode.LeftArrow, KeyCode.RightArrow, true));
                droneCore.SetThrottle(KeyboardAxis(KeyCode.DownArrow, KeyCode.UpArrow, true));
            }
            else
            {
                droneCore.SetRudder(KeyboardAxis(KeyCode.A, KeyCode.D, true));
                droneCore.SetThrottle(KeyboardAxis(KeyCode.S, KeyCode.W, true));
                droneCore.SetAileron(KeyboardAxis(KeyCode.LeftArrow, KeyCode.RightArrow, true));
                droneCore.SetElevator(KeyboardAxis(KeyCode.DownArrow, KeyCode.UpArrow, true));
            }
        }

        void DesktopAcroControl()
        {
            if (mode1)
            {
                droneCore.SetRudder(KeyboardAxis(KeyCode.A, KeyCode.D, true));
                droneCore.SetElevator(KeyboardAxis(KeyCode.S, KeyCode.W, true));
                droneCore.SetAileron(KeyboardAxis(KeyCode.LeftArrow, KeyCode.RightArrow, true));
                droneCore.SetThrottle(KeyboardAxis(KeyCode.DownArrow, KeyCode.UpArrow, false));
            }
            else
            {
                droneCore.SetRudder(KeyboardAxis(KeyCode.A, KeyCode.D, true));
                droneCore.SetThrottle(KeyboardAxis(KeyCode.S, KeyCode.W, false));
                droneCore.SetAileron(KeyboardAxis(KeyCode.LeftArrow, KeyCode.RightArrow, true));
                droneCore.SetElevator(KeyboardAxis(KeyCode.DownArrow, KeyCode.UpArrow, true));
            }
        }

        void DesktopControl()
        {
            // キー入力操作
            switch (droneCore.GetFlyingMode())
            {
                case MODE_ANGLE:
                    {
                        DesktopAngleControl();
                    }
                    break;
                case MODE_ACRO:
                    {
                        DesktopAcroControl();
                    }
                    break;
            }
        }

        /// <summary>
        /// キーボード入力を-1から1にマップする
        /// </summary>
        /// <param name="key_down"></param>
        /// <param name="key_up"></param>
        /// <param name="releaseCenter"></param>
        /// <returns></returns>
        float KeyboardAxis(KeyCode key_down, KeyCode key_up, bool releaseCenter)
        {
            float value = 0.0f;

            if (releaseCenter)
            {
                if (Input.GetKey(key_down) && Input.GetKeyUp(key_up))
                {
                    value = 0.0f;
                }
                else if (Input.GetKey(key_down))
                {
                    value = -1.0f;
                }
                else if (Input.GetKey(key_up))
                {
                    value = 1.0f;
                }
                else
                {
                    value = 0.0f;
                }
            }
            else
            {
                if (Input.GetKey(key_down) && Input.GetKeyUp(key_up))
                {
                    value = -1.0f;
                }
                else if (Input.GetKey(key_down))
                {
                    value = 0.0f;
                }
                else if (Input.GetKey(key_up))
                {
                    value = 1.0f;
                }
                else
                {
                    value = -1.0f;
                }
            }

            return value;
        }
    }
}