
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using TMPro;

namespace Kurotori.UDrone
{

    public class UdonDroneController : UdonSharpBehaviour
    {
        UdonDroneCore drone;

        float tmpRunSpeed;
        float tmpStrafeSpeed;
        float tmpWalkSpeed;

        bool controlling;

        // RenderTextureカメラは負荷が高いので、機体1つごとに1RenderTextureではなく、1台のカメラを共有します。

        [Header("カメラ")]
        [SerializeField]
        [Tooltip("機体側のカメラ位置")]
        Transform droneCamRig;

        [SerializeField]
        [Tooltip("機体カメラの回転軸")]
        Transform droneCamRotateRig;

        [SerializeField]
        [Tooltip("カメラ本体")]
        Transform droneCam;

        [SerializeField]
        bool mode1 = true;

        [SerializeField]
        GameObject droneCamDisplay;

        //[SerializeField]
        //GameObject exitButton;
        //[SerializeField]
        //GameObject enterButton;

        [SerializeField]
        GameObject mode1Obj;
        [SerializeField]
        GameObject mode2Obj;

        [Header("構成要素")]
        [SerializeField]
        [Tooltip("VRCStation")]
        VRCStation station;

        Transform stationTransform;

        [SerializeField]
        [Tooltip("VRプレイヤー用FPV画面")]
        GameObject droneFPVDisplayForVR;

        [Header("SettingPanel")]

        [SerializeField]
        [Tooltip("詳細設定パネル")]
        GameObject detailPanel;

        [SerializeField]
        [Tooltip("飛行モード表示")]
        TextMeshProUGUI flymodeLabel;

        [SerializeField]
        [Tooltip("操作モード表示")]
        TextMeshProUGUI controlModeLabel;

        [SerializeField]
        [Tooltip("カスタム入力を有効にするかのトグルスイッチ")]
        Toggle useCustomInputToggle;

        [SerializeField]
        [Tooltip("カメラ回転角のスライダー")]
        Slider cameraRotateSlider;
        [SerializeField]
        [Tooltip("カメラ角度ラベル")]
        TextMeshProUGUI cameraAngleLabel;

        [Header("入力設定")]

        [SerializeField]
        bool useCustomInput = false;

        [Header("VRコントローラー入力")]
        [SerializeField]
        string vrLHorizontal = "Oculus_CrossPlatform_PrimaryThumbstickHorizontal";
        [SerializeField]
        string vrLVertical = "Oculus_CrossPlatform_PrimaryThumbstickVertical";
        [SerializeField]
        string vrRHorizontal = "Oculus_CrossPlatform_SecondaryThumbstickHorizontal";
        [SerializeField]
        string vrRVertical = "Oculus_CrossPlatform_SecondaryThumbstickVertical";

        [Header("カスタム入力(プロポなど)")]
        [SerializeField]
        public string customLHorizontal = "Joy1 Axis 4";
        [SerializeField]
        public string customLVertical = "Joy1 Axis 3";
        [SerializeField]
        public string customRHorizontal = "Joy1 Axis 1";
        [SerializeField]
        public string customRVertical = "Joy1 Axis 2";

        [SerializeField]
        bool invertLH = false;
        [SerializeField]
        bool invertLV = false;
        [SerializeField]
        bool invertRH = false;
        [SerializeField]
        bool invertRV = false;

        VRC_Pickup pickup;
        Renderer pickupRenderer;
        Collider pickupCollider;


        // リセット用初期姿勢
        Vector3 initPosition;
        Quaternion initRotation;

        // 飛行モード
        int flymode = 0;
        const int MODE_ANGLE = 0;
        const int MODE_ACRO = 1;

        bool detailPanelOn = false;

        void Start()
        {
            controlling = false;
            droneCamDisplay.SetActive(false);

            initPosition = droneCamDisplay.transform.position;
            initRotation = droneCamDisplay.transform.rotation;

            if (mode1Obj)
                mode1Obj.SetActive(mode1);

            if (mode2Obj)
                mode2Obj.SetActive(!mode1);

            pickup = (VRC_Pickup)gameObject.GetComponent(typeof(VRC_Pickup));
            pickupRenderer = gameObject.GetComponent<Renderer>();
            pickupCollider = gameObject.GetComponent<Collider>();

            // Stationの設定
            if (station)
            {
                stationTransform = station.gameObject.transform;
                station.disableStationExit = true;

                station.gameObject.SetActive(false);
            }

            if (droneFPVDisplayForVR)
            {
                droneFPVDisplayForVR.SetActive(false);
            }

            // Settingパネル
            if (flymodeLabel)
            {
                switch (flymode)
                {
                    case MODE_ANGLE:
                        flymodeLabel.text = "ANGLE";
                        break;
                    case MODE_ACRO:
                        flymodeLabel.text = "ACRO";
                        break;
                }
            }

            if (controlModeLabel)
            {
                if (mode1)
                {
                    controlModeLabel.text = "MODE 1";
                }
                else
                {
                    controlModeLabel.text = "MODE 2";
                }
            }

            if (detailPanel)
            {
                detailPanel.SetActive(detailPanelOn);
            }


            if (useCustomInputToggle)
            {
                useCustomInputToggle.isOn = useCustomInput;

                if (useCustomInputToggle.isOn)
                {
                    UseCustomInputON();
                }
                else
                {
                    UseCustomInputOFF();
                }
            }

            CameraAngleChange();

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
            pickupRenderer.enabled = false;
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
            if (drone && !Networking.LocalPlayer.IsOwner(drone.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, drone.gameObject);
            }

            DisplayOpen();

            // フラグ設定
            controlling = true;

            //    controlling = false;
            //    Networking.LocalPlayer.Immobilize(false);
            //    droneCamDisplay.SetActive(false);

            // 他プレイヤーへ非表示指示
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HidePickup));

            if (drone)
            {
                drone.isArm = true;
            }
        }

        public void HidePickup()
        {
            if (!controlling)
            {
                pickup.pickupable = false;
                pickupRenderer.enabled = false;
                pickupCollider.enabled = false;
            }
        }

        public void ShowPickup()
        {
            if (!controlling)
            {
                pickup.pickupable = true;
                pickupRenderer.enabled = true;
                pickupCollider.enabled = true;
            }
        }

        public override void OnPickupUseUp()
        {
            //controlling = true;
            //Networking.LocalPlayer.Immobilize(true);
            //droneCamDisplay.SetActive(true);
        }

        #endregion

        /// <summary>
        /// コントロールから離れる処理
        /// </summary>
        public void ExitControl()
        {
            // プレイヤーのStationからの離脱
            station.ExitStation(Networking.LocalPlayer);
            station.gameObject.SetActive(false);

            // ディスプレイ類をオフにする
            DisplayOff();

            pickup.pickupable = true;
            pickupRenderer.enabled = true;
            pickupCollider.enabled = true;
            controlling = false;

            if (drone)
            {
                drone.isArm = false;
            }

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShowPickup));
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

            if (droneFPVDisplayForVR)
            {
                droneFPVDisplayForVR.SetActive(false);
            }
        }

        void DisplayFPVDrone()
        {
            droneCamDisplay.SetActive(true);

            if (droneFPVDisplayForVR)
            {
                droneFPVDisplayForVR.SetActive(true);
            }

            // ドローンカメラを設定する
            if (droneCam != null && droneCamRig != null)
            {
                droneCam.SetParent(droneCamRig);
                droneCam.localPosition = Vector3.zero;
                droneCam.localRotation = Quaternion.identity;
            }
            else
            {
                Debug.LogError("DroneCame not found");
            }

        }

        public void UseCustomInputToggle()
        {
            if (useCustomInputToggle == null) return;

            if (useCustomInputToggle.isOn)
            {
                UseCustomInputON();
            }
            else
            {
                UseCustomInputOFF();
            }
        }

        public void UseCustomInputON()
        {
            useCustomInput = true;
            if (drone)
                drone.useVRRate = !useCustomInput;
        }

        public void UseCustomInputOFF()
        {
            useCustomInput = false;
            if (drone)
                drone.useVRRate = !useCustomInput;
        }

        public void ToggleUseCustomInput()
        {
            useCustomInput = !useCustomInput;
            if (drone)
                drone.useVRRate = !useCustomInput;
        }

        public void ResetOwner()
        {
            if (!Networking.LocalPlayer.IsOwner(drone.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, drone.gameObject);
        }

        public void SetDrone(UdonDroneCore droneCore)
        {
            drone = droneCore;

            drone.useVRRate = !useCustomInput;
            Debug.Log("Set Drone");
        }

        public void ChangeMode()
        {
            mode1 = !mode1;

            if (mode1Obj)
                mode1Obj.SetActive(mode1);
            if (mode2Obj)
                mode2Obj.SetActive(!mode1);

            if (controlModeLabel)
            {
                if (mode1)
                {
                    controlModeLabel.text = "MODE 1";
                }
                else
                {
                    controlModeLabel.text = "MODE 2";
                }
            }
        }

        public void ChangeFlyMode()
        {
            if (drone)
            {
                switch (flymode)
                {
                    case MODE_ANGLE:
                        flymode = MODE_ACRO;
                        drone.ChangeMode_Acro();
                        flymodeLabel.text = "ACRO";
                        break;
                    case MODE_ACRO:
                        flymode = MODE_ANGLE;
                        drone.ChangeMode_Angle();
                        flymodeLabel.text = "ANGLE";
                        break;
                }
            }
        }

        public void DetailPanelToggle()
        {
            if (detailPanel)
            {
                detailPanelOn = !detailPanelOn;
                detailPanel.SetActive(detailPanelOn);
            }
        }

        public void CameraAngleChange()
        {
            if (cameraRotateSlider)
            {
                var angle = Mathf.Lerp(0, 90, cameraRotateSlider.value);
                cameraAngleLabel.text = string.Format("{0:0}°", angle);
                droneCamRotateRig.localRotation = Quaternion.AngleAxis(angle, Vector3.right);
            }
        }

        public void ResetDrone()
        {
            if (drone)
            {
                drone.ResetAll_All();
            }
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (player.isLocal && controlling)
            {
                ExitControl();
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if(player.isLocal)
            {
                if(station.seated)
                {
                    HidePickup();
                }
                else
                {
                    ShowPickup();
                }
            }
        }

        private void Update()
        {
            if (controlling)
            {
                if (Networking.LocalPlayer.IsUserInVR())
                {
                    if (useCustomInput)
                    {
                        CustomControl();
                    }
                    else
                    {
                        VRControl();
                    }
                }
                else
                {
                    if (useCustomInput)
                    {
                        CustomControl();
                    }
                    else
                    {
                        DesktopControl();
                    }
                }

                if (Input.GetKeyDown(KeyCode.P))
                {
                    ResetDrone();
                }
            }
        }

        void CustomControl()
        {
            if (mode1)
            {
                drone.rudder = Input.GetAxis(customLHorizontal) * (invertLH ? -1 : 1);
                drone.elevator = Input.GetAxis(customLVertical) * (invertLV ? -1 : 1);
                drone.aileron = Input.GetAxis(customRHorizontal) * (invertRH ? -1 : 1);
                drone.throttle = Input.GetAxis(customRVertical) * (invertRV ? -1 : 1);
            }
            else
            {
                drone.rudder = Input.GetAxis(customLHorizontal) * (invertLH ? -1 : 1);
                drone.throttle = Input.GetAxis(customLVertical) * (invertLV ? -1 : 1);
                drone.aileron = Input.GetAxis(customRHorizontal) * (invertRH ? -1 : 1);
                drone.elevator = Input.GetAxis(customRVertical) * (invertRV ? -1 : 1);
            }
        }

        void VRControl()
        {
            if (mode1)
            {
                drone.rudder = Input.GetAxis(vrLHorizontal);
                drone.elevator = Input.GetAxis(vrLVertical);
                drone.aileron = Input.GetAxis(vrRHorizontal);
                drone.throttle = Input.GetAxis(vrRVertical);
            }
            else
            {
                drone.rudder = Input.GetAxis(vrLHorizontal);
                drone.throttle = Input.GetAxis(vrLVertical);
                drone.aileron = Input.GetAxis(vrRHorizontal);
                drone.elevator = Input.GetAxis(vrRVertical);
            }
        }

        void DesktopAngleControl()
        {
            if (mode1)
            {
                drone.rudder = KeyboardAxis(KeyCode.A, KeyCode.D, true);
                drone.elevator = KeyboardAxis(KeyCode.S, KeyCode.W, true);
                drone.aileron = KeyboardAxis(KeyCode.LeftArrow, KeyCode.RightArrow, true);
                drone.throttle = KeyboardAxis(KeyCode.DownArrow, KeyCode.UpArrow, true);
            }
            else
            {
                drone.rudder = KeyboardAxis(KeyCode.A, KeyCode.D, true);
                drone.throttle = KeyboardAxis(KeyCode.S, KeyCode.W, true);
                drone.aileron = KeyboardAxis(KeyCode.LeftArrow, KeyCode.RightArrow, true);
                drone.elevator = KeyboardAxis(KeyCode.DownArrow, KeyCode.UpArrow, true);
            }
        }

        void DesktopAcroControl()
        {
            if (mode1)
            {
                drone.rudder = KeyboardAxis(KeyCode.A, KeyCode.D, true);
                drone.elevator = KeyboardAxis(KeyCode.S, KeyCode.W, true);
                drone.aileron = KeyboardAxis(KeyCode.LeftArrow, KeyCode.RightArrow, true);
                drone.throttle = KeyboardAxis(KeyCode.DownArrow, KeyCode.UpArrow, false);
            }
            else
            {
                drone.rudder = KeyboardAxis(KeyCode.A, KeyCode.D, true);
                drone.throttle = KeyboardAxis(KeyCode.S, KeyCode.W, false);
                drone.aileron = KeyboardAxis(KeyCode.LeftArrow, KeyCode.RightArrow, true);
                drone.elevator = KeyboardAxis(KeyCode.DownArrow, KeyCode.UpArrow, true);
            }
        }

        void DesktopControl()
        {
            // キー入力操作
            switch (drone.mode)
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


        public void SetInvertLHON()
        {
            invertLH = true;
        }
        public void SetInvertLHOFF()
        {
            invertLH = false;
        }
        public void SetInvertLVON()
        {
            invertLV = true;
        }
        public void SetInvertLVOFF()
        {
            invertLV = false;
        }
        public void SetInvertRHON()
        {
            invertRH = true;
        }
        public void SetInvertRHOFF()
        {
            invertRH = false;
        }
        public void SetInvertRVON()
        {
            invertRV = true;
        }
        public void SetInvertRVOFF()
        {
            invertRV = false;
        }

        #region SetCustomInput

        // Joy1 Axis 1

        public void SetLH_Joy1Axis1()
        {
            customLHorizontal = "Joy1 Axis 1";
        }
        public void SetLV_Joy1Axis1()
        {
            customLVertical = "Joy1 Axis 1";
        }
        public void SetRH_Joy1Axis1()
        {
            customRHorizontal = "Joy1 Axis 1";
        }
        public void SetRV_Joy1Axis1()
        {
            customRVertical = "Joy1 Axis 1";
        }

        // Joy1 Axis 2

        public void SetLH_Joy1Axis2()
        {
            customLHorizontal = "Joy1 Axis 2";
        }
        public void SetLV_Joy1Axis2()
        {
            customLVertical = "Joy1 Axis 2";
        }
        public void SetRH_Joy1Axis2()
        {
            customRHorizontal = "Joy1 Axis 2";
        }
        public void SetRV_Joy1Axis2()
        {
            customRVertical = "Joy1 Axis 2";
        }

        // Joy1 Axis 3

        public void SetLH_Joy1Axis3()
        {
            customLHorizontal = "Joy1 Axis 3";
        }
        public void SetLV_Joy1Axis3()
        {
            customLVertical = "Joy1 Axis 3";
        }
        public void SetRH_Joy1Axis3()
        {
            customRHorizontal = "Joy1 Axis 3";
        }
        public void SetRV_Joy1Axis3()
        {
            customRVertical = "Joy1 Axis 3";
        }

        // Joy1 Axis 4

        public void SetLH_Joy1Axis4()
        {
            customLHorizontal = "Joy1 Axis 4";
        }
        public void SetLV_Joy1Axis4()
        {
            customLVertical = "Joy1 Axis 4";
        }
        public void SetRH_Joy1Axis4()
        {
            customRHorizontal = "Joy1 Axis 4";
        }
        public void SetRV_Joy1Axis4()
        {
            customRVertical = "Joy1 Axis 4";
        }

        // Joy1 Axis 5

        public void SetLH_Joy1Axis5()
        {
            customLHorizontal = "Joy1 Axis 5";
        }
        public void SetLV_Joy1Axis5()
        {
            customLVertical = "Joy1 Axis 5";
        }
        public void SetRH_Joy1Axis5()
        {
            customRHorizontal = "Joy1 Axis 5";
        }
        public void SetRV_Joy1Axis5()
        {
            customRVertical = "Joy1 Axis 5";
        }

        // Joy1 Axis 6

        public void SetLH_Joy1Axis6()
        {
            customLHorizontal = "Joy1 Axis 6";
        }
        public void SetLV_Joy1Axis6()
        {
            customLVertical = "Joy1 Axis 6";
        }
        public void SetRH_Joy1Axis6()
        {
            customRHorizontal = "Joy1 Axis 6";
        }
        public void SetRV_Joy1Axis6()
        {
            customRVertical = "Joy1 Axis 6";
        }

        // Joy1 Axis 7

        public void SetLH_Joy1Axis7()
        {
            customLHorizontal = "Joy1 Axis 7";
        }
        public void SetLV_Joy1Axis7()
        {
            customLVertical = "Joy1 Axis 7";
        }
        public void SetRH_Joy1Axis7()
        {
            customRHorizontal = "Joy1 Axis 7";
        }
        public void SetRV_Joy1Axis7()
        {
            customRVertical = "Joy1 Axis 7";
        }

        // Joy1 Axis 8

        public void SetLH_Joy1Axis8()
        {
            customLHorizontal = "Joy1 Axis 8";
        }
        public void SetLV_Joy1Axis8()
        {
            customLVertical = "Joy1 Axis 8";
        }
        public void SetRH_Joy1Axis8()
        {
            customRHorizontal = "Joy1 Axis 8";
        }
        public void SetRV_Joy1Axis8()
        {
            customRVertical = "Joy1 Axis 8";
        }

        // Joy1 Axis 9

        public void SetLH_Joy1Axis9()
        {
            customLHorizontal = "Joy1 Axis 9";
        }
        public void SetLV_Joy1Axis9()
        {
            customLVertical = "Joy1 Axis 9";
        }
        public void SetRH_Joy1Axis9()
        {
            customRHorizontal = "Joy1 Axis 9";
        }
        public void SetRV_Joy1Axis9()
        {
            customRVertical = "Joy1 Axis 9";
        }

        // Joy1 Axis 10

        public void SetLH_Joy1Axis10()
        {
            customLHorizontal = "Joy1 Axis 10";
        }
        public void SetLV_Joy1Axis10()
        {
            customLVertical = "Joy1 Axis 10";
        }
        public void SetRH_Joy1Axis10()
        {
            customRHorizontal = "Joy1 Axis 10";
        }
        public void SetRV_Joy1Axis10()
        {
            customRVertical = "Joy1 Axis 10";
        }

        // Joy2 Axis 1

        public void SetLH_Joy2Axis1()
        {
            customLHorizontal = "Joy2 Axis 1";
        }
        public void SetLV_Joy2Axis1()
        {
            customLVertical = "Joy2 Axis 1";
        }
        public void SetRH_Joy2Axis1()
        {
            customRHorizontal = "Joy2 Axis 1";
        }
        public void SetRV_Joy2Axis1()
        {
            customRVertical = "Joy2 Axis 1";
        }

        // Joy2 Axis 2

        public void SetLH_Joy2Axis2()
        {
            customLHorizontal = "Joy2 Axis 2";
        }
        public void SetLV_Joy2Axis2()
        {
            customLVertical = "Joy2 Axis 2";
        }
        public void SetRH_Joy2Axis2()
        {
            customRHorizontal = "Joy2 Axis 2";
        }
        public void SetRV_Joy2Axis2()
        {
            customRVertical = "Joy2 Axis 2";
        }

        // Joy2 Axis 3

        public void SetLH_Joy2Axis3()
        {
            customLHorizontal = "Joy2 Axis 3";
        }
        public void SetLV_Joy2Axis3()
        {
            customLVertical = "Joy2 Axis 3";
        }
        public void SetRH_Joy2Axis3()
        {
            customRHorizontal = "Joy2 Axis 3";
        }
        public void SetRV_Joy2Axis3()
        {
            customRVertical = "Joy2 Axis 3";
        }

        // Joy2 Axis 4

        public void SetLH_Joy2Axis4()
        {
            customLHorizontal = "Joy2 Axis 4";
        }
        public void SetLV_Joy2Axis4()
        {
            customLVertical = "Joy2 Axis 4";
        }
        public void SetRH_Joy2Axis4()
        {
            customRHorizontal = "Joy2 Axis 4";
        }
        public void SetRV_Joy2Axis4()
        {
            customRVertical = "Joy2 Axis 4";
        }

        // Joy2 Axis 5

        public void SetLH_Joy2Axis5()
        {
            customLHorizontal = "Joy2 Axis 5";
        }
        public void SetLV_Joy2Axis5()
        {
            customLVertical = "Joy2 Axis 5";
        }
        public void SetRH_Joy2Axis5()
        {
            customRHorizontal = "Joy2 Axis 5";
        }
        public void SetRV_Joy2Axis5()
        {
            customRVertical = "Joy2 Axis 5";
        }

        // Joy2 Axis 6

        public void SetLH_Joy2Axis6()
        {
            customLHorizontal = "Joy2 Axis 6";
        }
        public void SetLV_Joy2Axis6()
        {
            customLVertical = "Joy2 Axis 6";
        }
        public void SetRH_Joy2Axis6()
        {
            customRHorizontal = "Joy2 Axis 6";
        }
        public void SetRV_Joy2Axis6()
        {
            customRVertical = "Joy2 Axis 6";
        }

        // Joy2 Axis 7

        public void SetLH_Joy2Axis7()
        {
            customLHorizontal = "Joy2 Axis 7";
        }
        public void SetLV_Joy2Axis7()
        {
            customLVertical = "Joy2 Axis 7";
        }
        public void SetRH_Joy2Axis7()
        {
            customRHorizontal = "Joy2 Axis 7";
        }
        public void SetRV_Joy2Axis7()
        {
            customRVertical = "Joy2 Axis 7";
        }

        // Joy2 Axis 8

        public void SetLH_Joy2Axis8()
        {
            customLHorizontal = "Joy2 Axis 8";
        }
        public void SetLV_Joy2Axis8()
        {
            customLVertical = "Joy2 Axis 8";
        }
        public void SetRH_Joy2Axis8()
        {
            customRHorizontal = "Joy2 Axis 8";
        }
        public void SetRV_Joy2Axis8()
        {
            customRVertical = "Joy2 Axis 8";
        }

        // Joy2 Axis 9

        public void SetLH_Joy2Axis9()
        {
            customLHorizontal = "Joy2 Axis 9";
        }
        public void SetLV_Joy2Axis9()
        {
            customLVertical = "Joy2 Axis 9";
        }
        public void SetRH_Joy2Axis9()
        {
            customRHorizontal = "Joy2 Axis 9";
        }
        public void SetRV_Joy2Axis9()
        {
            customRVertical = "Joy2 Axis 9";
        }

        // Joy2 Axis 10

        public void SetLH_Joy2Axis10()
        {
            customLHorizontal = "Joy2 Axis 10";
        }
        public void SetLV_Joy2Axis10()
        {
            customLVertical = "Joy2 Axis 10";
        }
        public void SetRH_Joy2Axis10()
        {
            customRHorizontal = "Joy2 Axis 10";
        }
        public void SetRV_Joy2Axis10()
        {
            customRVertical = "Joy2 Axis 10";
        }

        // Oculus_GearVR_Thumbstickにはデッドゾーンがない

        // Oculus_GearVR_LThumbstickX
        public void SetLH_Oculus_GearVR_LThumbstickX()
        {
            customLHorizontal = "Oculus_GearVR_LThumbstickX";
        }
        public void SetLV_Oculus_GearVR_LThumbstickX()
        {
            customLVertical = "Oculus_GearVR_LThumbstickX";
        }
        public void SetRH_Oculus_GearVR_LThumbstickX()
        {
            customRHorizontal = "Oculus_GearVR_LThumbstickX";
        }
        public void SetRV_Oculus_GearVR_LThumbstickX()
        {
            customRVertical = "Oculus_GearVR_LThumbstickX";
        }

        // Oculus_GearVR_LThumbstickY
        public void SetLH_Oculus_GearVR_LThumbstickY()
        {
            customLHorizontal = "Oculus_GearVR_LThumbstickY";
        }
        public void SetLV_Oculus_GearVR_LThumbstickY()
        {
            customLVertical = "Oculus_GearVR_LThumbstickY";
        }
        public void SetRH_Oculus_GearVR_LThumbstickY()
        {
            customRHorizontal = "Oculus_GearVR_LThumbstickY";
        }
        public void SetRV_Oculus_GearVR_LThumbstickY()
        {
            customRVertical = "Oculus_GearVR_LThumbstickY";
        }


        // Oculus_GearVR_RThumbstickX
        public void SetLH_Oculus_GearVR_RThumbstickX()
        {
            customLHorizontal = "Oculus_GearVR_RThumbstickX";
        }
        public void SetLV_Oculus_GearVR_RThumbstickX()
        {
            customLVertical = "Oculus_GearVR_RThumbstickX";
        }
        public void SetRH_Oculus_GearVR_RThumbstickX()
        {
            customRHorizontal = "Oculus_GearVR_RThumbstickX";
        }
        public void SetRV_Oculus_GearVR_RThumbstickX()
        {
            customRVertical = "Oculus_GearVR_RThumbstickX";
        }


        // Oculus_GearVR_RThumbstickY
        public void SetLH_Oculus_GearVR_RThumbstickY()
        {
            customLHorizontal = "Oculus_GearVR_RThumbstickY";
        }
        public void SetLV_Oculus_GearVR_RThumbstickY()
        {
            customLVertical = "Oculus_GearVR_RThumbstickY";
        }
        public void SetRH_Oculus_GearVR_RThumbstickY()
        {
            customRHorizontal = "Oculus_GearVR_RThumbstickY";
        }
        public void SetRV_Oculus_GearVR_RThumbstickY()
        {
            customRVertical = "Oculus_GearVR_RThumbstickY";
        }


        #endregion

    }
}