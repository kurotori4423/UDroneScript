
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

#if !COMPILE_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

namespace Kurotori.UDrone
{
    public enum DRONE_MODE
    {
        ANGLE,
        ACRO
    };

    /// <summary>
    /// ドローン本体スクリプト
    /// </summary>
    public class UdonDroneCore : UdonSharpBehaviour
    {
        float COS45 = 0.52532198881f;

        Rigidbody m_Body;

        [SerializeField]
        public UdonDroneManualSyncVariables m_ManualSyncVariables;

        [SerializeField]
        public DroneNamePlate m_DroneNamePlate;

        [Header("Controller")]
        [SerializeField]
        private UdonDroneController m_Controller;

        [SerializeField]
        [Tooltip("リセット位置")]
        public Transform m_ResetPos;

        [Header("機体制御設定 -----------------------------------------------------------------")]
        [SerializeField]
        [Tooltip("制御モード\n 0:アングルモード \n 1:アクロモード")]
        [Range(0, 1)]
        private int m_Mode = 0;

        const int MODE_ANGLE = 0;
        const int MODE_ACRO = 1;

        [SerializeField]
        [Tooltip("高度維持機能[アングルモードのみ]")]
        private bool m_HightAdjustMode = true;

        [SerializeField]
        [Tooltip("スロットルセンターホバリングモード\n[スロットルが中間の時にホバリング出力に補正する]")]
        private bool m_ThrottleCenterHovering = true;

        [SerializeField]
        [Tooltip("プロペラの逆転を許可(許可しない方がリアルですが、許可した方が操作性が上がります)")]
        bool m_AllowPropellerReversal = true;

        [Header("機体形状設定 -----------------------------------------------------------------")]
        [SerializeField]
        [Tooltip("推進力が発生するポイントの重心からの距離です")]
        float m_ForcePointRadius = 1.0f;
        [SerializeField]
        [Tooltip("推進力が発生するポイントの重心からの上下方向のオフセット量です")]
        float m_ForcePointHeight = 0.1f;

        [SerializeField]
        [Tooltip("空気抵抗")]
        float m_Drag = 0.18f;

        [SerializeField]
        [Tooltip("プロペラ推力気流速度比")]
        float m_PropellarAirSpeedRate = 0.1f;

        [SerializeField]
        [Tooltip("機体最高速[m/s]")]
        float m_TopSpeed = 30;

        [Header("機体出力設定 -----------------------------------------------------------------")]

        [SerializeField]
        [Tooltip("最大上昇スロットル[機体質量に比例]")]
        float m_MaxThrottleForce = 2.0f;

        [SerializeField]
        [Tooltip("最大回転出力[回転時に使う最高スロットル出力][機体質量に比例]")]
        float m_MaxRotationThrottleForce = 1.0f;

        [SerializeField]
        [Tooltip("ヨー回転時の最大出力[機体質量に比例]")]
        float m_YawForce = 10.0f;

        float m_Force = 0;

        float m_FrontForce = 0;
        float m_BackForce = 0;
        float m_LeftForce = 0;
        float m_RightForce = 0;
        float m_RollTorque = 0;

        float m_ForceRF = 0;
        float m_ForceRB = 0;
        float m_ForceLF = 0;
        float m_ForceLB = 0;

        [Header("操作感度設定 -----------------------------------------------------------------")]

        // コントローラー入力
        [HideInInspector] private float m_Throttle = -1; // 上昇
        [HideInInspector] private float m_Rudder = 0; // 左右旋回
        [HideInInspector] private float m_Elevator = 0; // 前進・後退
        [HideInInspector] private float m_Aileron = 0; // 左右スライド

        [Header("レート設定")]
        [SerializeField]
        float m_RcRate = 1.0f;
        [SerializeField]
        float m_SuperRate = 0.6f;
        [SerializeField]
        float m_RcExpo = 0.0f;


        [Header("高度維持機能関係 ---------------------------------------------------------------")]
        [SerializeField]
        [Tooltip("高度維持有効時の最大上昇下降速度(自由落下速度を超えると無視されます)")]
        float m_MaxSpeedOnHightAdjust = 3.0f;

        [Header("高度維持PID")]
        [SerializeField]
        float m_Hight_Kp = 0.2f;
        [SerializeField]
        float m_Hight_Ki = 0.2f;
        [SerializeField]
        float m_Hight_Kd = 0.1f;

        float m_TargetVelocity = 0; // 高度維持機能の目標速度

        float m_PrevPrevHightDiff = 0.0f;
        float m_PrevHightDiff = 0.0f;
        float m_IntegralHight = 0.0f;

        [Header("姿勢制御関係 -----------------------------------------------------------------")]
        [Header("アングルモード専用")]
        // 姿勢制御機能
        [SerializeField]
        [Tooltip("最大傾斜角")]
        [Range(0, 90)]
        float m_MaxAngle = 35.0f; // 最大傾斜角



        [Header("角度維持PID（アングルモード）")]

        [SerializeField]
        float m_Angle_Kp = 0.7f;
        [SerializeField]
        float m_Angle_Ki = 0.7f;
        [SerializeField]
        float m_Angle_Kd = 0.1f;

        float m_GoalAngleX = 0;
        float m_GoalAngleZ = 0;

        float m_PrevPrevAngleXDiff = 0.0f;
        float m_PrevAngleXDiff = 0.0f;
        float m_IntegralAngleX = 0.0f;

        float m_PrevPrevAngleZDiff = 0.0f;
        float m_PrevAngleZDiff = 0.0f;
        float m_IntegralAngleZ = 0.0f;

        [Header("ヨー制御PID（アングルモード）")]
        [SerializeField]
        float m_AnglerYaw_Kp = 1;
        [SerializeField]
        float m_AnglerYaw_Ki = 0.5f;
        [SerializeField]
        float m_AnglerYaw_Kd = 0.01f;

        float m_GoalAnglerYaw = 0.0f;

        float m_PrevPrevAnglerYawDiff = 0.0f;
        float m_PrevAnglerYawDiff = 0.0f;
        float m_IntegralAnglerYaw = 0.0f;

        [Header("角速度制御PID（アクロモード）")]

        [SerializeField]
        float m_AnglerV_Kp = 1.0f;
        [SerializeField]
        float m_AnglerV_Ki = 0.5f;
        [SerializeField]
        float m_AnglerV_Kd = 0.01f;

        Vector3 m_GoalAnglerV;

        /// <summary>
        /// 前回の角速度誤差
        /// </summary>
        Vector3 m_PrevPrevAnglerVDiff;
        /// <summary>
        /// 現在の角速度誤差
        /// </summary>
        Vector3 m_PrevAnglerVDiff;
        /// <summary>
        /// 誤差の微分
        /// </summary>
        Vector3 m_IntegralAnglerV;

        [Header("見た目の設定 -----------------------------------------------------------------")]
        [SerializeField]
        Transform m_FrontRightFin;
        [SerializeField]
        Transform m_FrontLeftFin;
        [SerializeField]
        Transform m_BackRightFin;
        [SerializeField]
        Transform m_BackLeftFin;
        [SerializeField]
        float m_LookRotationSpeed = 3;
        [SerializeField]
        [Tooltip("回転軸[X = 0, Y = 1, Z = 2]")]
        [Range(0, 2)]
        int m_LookPropellerAxis = 0;

        [UdonSynced(UdonSyncMode.Smooth)]
        float m_EngineSpeed;

        [SerializeField]
        AudioClip m_AudioClip;
        [SerializeField]
        public AudioSource m_AudioSource;
        [SerializeField]
        public Transform AudioSourcePivot;
        [SerializeField]
        public Transform AudioSourceControllerPivot;

        [SerializeField]
        float m_PitchFactor = 2.0f;
        [SerializeField]
        float m_MaxPitch = 10.0f;
        [SerializeField]
        float m_MaxPitchForce = 8.0f;
        [SerializeField]
        TrailRenderer m_TrailRenderer;

        [Header("カメラの設定")]
        [SerializeField, Tooltip("カメラのアタッチ先")]
        public Transform CameraRig;
        [SerializeField,Tooltip("カメラの回転軸")]
        public Transform m_CameraRotateRig;

        [Header("UI")]

        // リセット用初期姿勢
        Vector3 m_InitPosition;
        Quaternion m_InitRotation;

        [Tooltip("自分が操作状態かどうか")]
        private bool m_IsArmLocal = false;

        VRCObjectSync m_objectSync;

        
        /// <summary>
        /// ドローンの音声をコントローラー位置に固定するかどうか
        /// </summary>
        private bool m_audioFixToController = false;

        void Start()
        {
            m_ManualSyncVariables.m_droneCore = this;

            m_IsArmLocal = false;

            #region RigidBody Setting

            m_Body = gameObject.GetComponent<Rigidbody>();

            // 角速度最大値は無限にしておく（問題があれば修正）
            m_Body.maxAngularVelocity = float.PositiveInfinity;

            // 空気抵抗は自前で計算するため0に設定
            m_Body.drag = 0.0f;

            // 重心を原点に
            m_Body.centerOfMass = Vector3.zero;

            m_objectSync = GetComponent<VRCObjectSync>();


#endregion

            if (m_Controller != null)
                m_Controller.SetDrone(this);

#region Audio Setup
            if (m_AudioSource != null && m_AudioClip != null)
            {
                m_AudioSource.clip = m_AudioClip;
                m_AudioSource.loop = true;
                m_AudioSource.Play();
            }
#endregion

            m_InitPosition = m_Body.position;
            m_InitRotation = m_Body.rotation;

#region Setup UI

            switch (m_Mode)
            {
                case MODE_ACRO:
                    break;
                case MODE_ANGLE:
                    break;
            }

#endregion

        }

        private void FixedUpdate()
        {

        if (Networking.LocalPlayer != null && !Networking.LocalPlayer.IsOwner(gameObject))
        {
            LookUpdate();
            return;
        }
            LookUpdate();

            InputUpdate();

            
            if (m_IsArmLocal)
            {

                // PID制御 上下方向速度を0になるように調整する
                if (m_HightAdjustMode)
                {
                    HightAdjustPID();
                }

                // 姿勢制御 特定の角度を維持する

                switch (m_Mode)
                {
                    case MODE_ANGLE:
                        {
                            CalcAngleModePID();
                        }
                        break;
                    case MODE_ACRO:
                        {
                            CalcAcroModePID();
                        }
                        break;
                }

                m_ForceRF = m_Force + m_FrontForce + m_RightForce;
                m_ForceLF = m_Force + m_FrontForce + m_LeftForce;
                m_ForceRB = m_Force + m_BackForce + m_RightForce;
                m_ForceLB = m_Force + m_BackForce + m_LeftForce;

                ApplyPropellerReversal();

            }
            else
            {
                m_Force = 0;
                m_ForceRF = 0;
                m_ForceLF = 0;
                m_ForceRB = 0;
                m_ForceLB = 0;
            }

            //Debug.Log("m_Force:" + m_Force);

            float forceAll = Mathf.Abs(m_ForceRF + m_ForceLF + m_ForceRB + m_ForceLB);

            m_EngineSpeed = m_Force;
            
            ApplyDroneForce();

            var propDir = m_Body.transform.TransformDirection(Vector3.up);
            CalcPropDrop(propDir);
            {
                // 空気抵抗計算
                m_Body.AddForce(-m_Body.velocity * m_Drag);
            }
        }

        /// <summary>
        /// プロペラの逆転をするかしないか適用
        /// </summary>
        void ApplyPropellerReversal()
        {
            if (!m_AllowPropellerReversal)
            {
                m_ForceRF = Mathf.Max(m_ForceRF, 0);
                m_ForceLF = Mathf.Max(m_ForceLF, 0);
                m_ForceRB = Mathf.Max(m_ForceRB, 0);
                m_ForceLB = Mathf.Max(m_ForceLB, 0);
            }
        }

        void CalcAngleModePID()
        {
            float angleX = Mathf.Repeat(m_Body.transform.localRotation.eulerAngles.x + 180, 360) - 180;
            float angleZ = Mathf.Repeat(m_Body.transform.localRotation.eulerAngles.z + 180, 360) - 180;

            {
                m_PrevPrevAngleXDiff = m_PrevAngleXDiff;

                m_PrevAngleXDiff = angleX - m_GoalAngleX;
                m_IntegralAngleX += (m_PrevPrevAngleXDiff + m_PrevAngleXDiff) * 0.5f * Time.deltaTime;

                float P = m_PrevAngleXDiff * (m_Angle_Kp * m_Body.mass);
                float I = m_IntegralAngleX * (m_Angle_Ki * m_Body.mass);
                float D = (m_Angle_Kd * m_Body.mass) * (m_PrevAngleXDiff - m_PrevPrevAngleXDiff) / Time.deltaTime;

                float adjustAngleXForce = -(P + I + D);

                adjustAngleXForce = Mathf.Sign(adjustAngleXForce) * Mathf.Min(Mathf.Abs(adjustAngleXForce), m_MaxRotationThrottleForce * m_Body.mass);

                m_FrontForce = -adjustAngleXForce;
                m_BackForce = adjustAngleXForce;
            }


            {
                m_PrevPrevAngleZDiff = m_PrevAngleZDiff;
                m_PrevAngleZDiff = angleZ - m_GoalAngleZ;
                m_IntegralAngleZ += (m_PrevPrevAngleZDiff + m_PrevAngleZDiff) * 0.5f * Time.deltaTime;

                float P = m_PrevAngleZDiff * (m_Angle_Kp * m_Body.mass);
                float I = m_IntegralAngleZ * (m_Angle_Ki * m_Body.mass);
                float D = (m_Angle_Kd * m_Body.mass) * (m_PrevAngleZDiff - m_PrevPrevAngleZDiff) / Time.deltaTime;

                float adjustAngleZForce = -(P + I + D);

                adjustAngleZForce = Mathf.Sign(adjustAngleZForce) * Mathf.Min(Mathf.Abs(adjustAngleZForce), m_MaxRotationThrottleForce * m_Body.mass);

                m_LeftForce = -adjustAngleZForce;
                m_RightForce = adjustAngleZForce;
            }

            {

                m_PrevPrevAnglerYawDiff = m_PrevAnglerYawDiff;
                m_PrevAnglerYawDiff = m_Body.angularVelocity.y - m_GoalAnglerYaw;
                m_IntegralAnglerYaw += (m_PrevPrevAnglerYawDiff + m_PrevAnglerYawDiff) * 0.5f * Time.deltaTime;

                float P = m_PrevAnglerYawDiff * (m_AnglerYaw_Kp * m_Body.mass);
                float I = m_IntegralAnglerYaw * (m_AnglerYaw_Ki * m_Body.mass);
                float D = (m_AnglerYaw_Kd * m_Body.mass) * (m_PrevAnglerYawDiff - m_PrevPrevAnglerYawDiff) / Time.deltaTime;

                float adjustAnglerYawForce = -(P + I + D);
                float angleForceY = Mathf.Sign(adjustAnglerYawForce) * Mathf.Min(Mathf.Abs(adjustAnglerYawForce), m_MaxRotationThrottleForce * m_Body.mass);

                //body.AddTorque(Vector3.up * (angleForceY * yawForce));
                m_RollTorque = angleForceY;
            }
        }

        void CalcAcroModePID()
        {
            //Vector3 localAnglerVelocity = m_Body.transform.InverseTransformDirection(m_Body.angularVelocity).normalized * m_Body.angularVelocity.magnitude;
            Vector3 localAnglerVelocity = m_Body.transform.InverseTransformVector(m_Body.angularVelocity);

            m_PrevPrevAnglerVDiff = m_PrevAnglerVDiff;
            m_PrevAnglerVDiff = localAnglerVelocity - m_GoalAnglerV; // 誤差を計算

            m_IntegralAnglerV += (m_PrevPrevAnglerVDiff + m_PrevAnglerVDiff) * 0.5f * Time.deltaTime; // 誤差の積分を近似計算

            Vector3 P = m_PrevAnglerVDiff * (m_AnglerV_Kp);
            Vector3 I = m_IntegralAnglerV * (m_AnglerV_Ki);
            Vector3 D = m_AnglerV_Kd * ((m_PrevAnglerVDiff - m_PrevPrevAnglerVDiff) / Time.deltaTime);

            Vector3 pid = -(P + I + D);

            Debug.Log($"PID:{P.y} {I.y} {D.y}");

            float angleForceX = Mathf.Sign(pid.x) * Mathf.Min(Mathf.Abs(pid.x) * m_MaxRotationThrottleForce * m_Body.mass, m_MaxRotationThrottleForce * m_Body.mass);
            //float angleForceX = Mathf.Sign(pid.x) * Mathf.Abs(pid.x) * m_MaxRotationThrottleForce * m_Body.mass;
            m_FrontForce = -angleForceX;
            m_BackForce = angleForceX;

            float angleForceZ = Mathf.Sign(pid.z) * Mathf.Min(Mathf.Abs(pid.z) * m_MaxRotationThrottleForce * m_Body.mass, m_MaxRotationThrottleForce * m_Body.mass);
            //float angleForceZ = Mathf.Sign(pid.z) * Mathf.Abs(pid.z) * m_MaxRotationThrottleForce * m_Body.mass;
            m_LeftForce = -angleForceZ;
            m_RightForce = angleForceZ;

            float angleForceY = Mathf.Sign(pid.y) * Mathf.Min(Mathf.Abs(pid.y) * m_MaxRotationThrottleForce * m_Body.mass, m_MaxRotationThrottleForce * m_Body.mass);
            //float angleForceY = Mathf.Sign(pid.y) * Mathf.Abs(pid.y) * m_MaxRotationThrottleForce * m_Body.mass;
            //body.AddRelativeTorque(Vector3.up * (angleForceY * yawForce));
            m_RollTorque = angleForceY;

            Debug.Log($"RollTorque:{m_RollTorque}");

            //Debug.Log("UDRONE: TargetVelocity:(" + localAnglerVelocity.x + "," + localAnglerVelocity.y + "," + localAnglerVelocity.z + ")");
        }

        /// <summary>
        /// ドローンの推力を発生させる
        /// </summary>
        void ApplyDroneForce()
        {
            var axleLen = m_ForcePointRadius * COS45;

            var frontRight = transform.TransformPoint(new Vector3(axleLen, m_ForcePointHeight + m_Body.centerOfMass.y, axleLen));
            var frontLeft = transform.TransformPoint(new Vector3(-axleLen, m_ForcePointHeight + m_Body.centerOfMass.y, axleLen));
            var backRight = transform.TransformPoint(new Vector3(axleLen, m_ForcePointHeight + m_Body.centerOfMass.y, -axleLen));
            var backLeft = transform.TransformPoint(new Vector3(-axleLen, m_ForcePointHeight + m_Body.centerOfMass.y, -axleLen));

            var downDir = m_Body.transform.TransformDirection(Vector3.up);


            Vector3 frontRightThrust = m_ForceRF * downDir;
            Vector3 frontLeftThrust = m_ForceLF * downDir;
            Vector3 backRightThrust = m_ForceRB * downDir;
            Vector3 backLeftThrust = m_ForceLB * downDir;

            m_Body.AddForceAtPosition(frontRightThrust, frontRight);
            m_Body.AddForceAtPosition(frontLeftThrust, frontLeft);
            m_Body.AddForceAtPosition(backRightThrust, backRight);
            m_Body.AddForceAtPosition(backLeftThrust, backLeft);

            switch (m_Mode)
            {
                case MODE_ANGLE:
                    {
                        m_Body.AddTorque(Vector3.up * (m_RollTorque * m_YawForce));
                    }
                    break;
                case MODE_ACRO:
                    {
                        m_Body.AddRelativeTorque(Vector3.up * (m_RollTorque * m_YawForce));
                    }
                    break;
            }

        }

        public UdonDroneController GetController()
        {
            return m_Controller;
        }

        public bool GetIsArmLocal()
        {
            return m_IsArmLocal;
        }

        public void SetIsArmLocal(bool flag)
        {
            m_IsArmLocal = flag;

            UpdateAudioFix();
        }

        /// <summary>
        /// オーディオの固定位置を変更
        /// </summary>
        void UpdateAudioFix()
        {
            if (m_IsArmLocal)
            {
                if (m_audioFixToController)
                {
                    m_AudioSource.transform.parent = AudioSourceControllerPivot;
                    m_AudioSource.transform.localPosition = Vector3.zero;
                }
                else
                {
                    m_AudioSource.transform.parent = AudioSourcePivot;
                    m_AudioSource.transform.localPosition = Vector3.zero;
                }
            }
            else
            {
                // 操作していないドローンはドローンの位置で音が鳴る
                m_AudioSource.transform.parent = AudioSourcePivot;
                m_AudioSource.transform.localPosition = Vector3.zero;
            }
        }

        public void ChangeMode_Angle()
        {
            m_Mode = MODE_ANGLE;
        }

        public void ChangeMode_Acro()
        {
            m_Mode = MODE_ACRO;
        }

        public int GetFlyingMode()
        {
            return m_Mode;
        }

        public void SetThrottleCenterHoveringMode(bool flag)
        {
            m_ThrottleCenterHovering = flag;
        }

        public void SetHightAdjustMode(bool flag)
        {
            m_HightAdjustMode = flag;
        }

        public void SetThrottle(float throttle)
        {
            m_Throttle = throttle;
        }

        public void SetRudder(float rudder)
        {
            m_Rudder = Mathf.Floor(rudder * 100.0f) / 100.0f;
        }

        public void SetElevator(float elevator)
        {
            m_Elevator = Mathf.Floor(elevator * 100.0f) / 100.0f;
        }

        public void SetAileron(float aileron)
        {
            m_Aileron = Mathf.Floor(aileron * 100.0f) / 100.0f;
        }

        public void SetMaxAngle(float angle)
        {
            m_MaxAngle = angle;
        }

        public void SetThrottleForce(float throttle)
        {
            m_MaxThrottleForce = throttle;
        }

        public float GetMaxAngle()
        {
            return m_MaxAngle;
        }

        /// <summary>
        /// 機体のカメラアングルを指定します
        /// </summary>
        public void SetCameraAngle(float eularAngle)
        {
            m_CameraRotateRig.localRotation = Quaternion.AngleAxis(eularAngle, Vector3.right);
        }

        public void ChangeMode()
        {
            switch (m_Mode)
            {
                case MODE_ANGLE:
                    m_Mode = MODE_ACRO;
                    break;
                case MODE_ACRO:
                    m_Mode = MODE_ANGLE;
                    break;
            }
        }

        public override void OnPickup()
        {

        }

        public override void OnDrop()
        {
            if (m_Controller != null)
                m_Controller.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "ResetOwner");
        }

        public void ResetAll_All()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetAll");
        }

        public void ResetAll()
        {
            if(Networking.IsOwner(gameObject))
            {
                m_objectSync.FlagDiscontinuity();
                if (m_ResetPos)
                    m_objectSync.TeleportTo(m_ResetPos);
                else
                    m_objectSync.Respawn();
            }

            //m_Body.isKinematic = true;

            m_Body.velocity = Vector3.zero;
            m_Body.angularVelocity = Vector3.zero;

            //if (m_ResetPos)
            //{
            //    m_Body.position = m_ResetPos.position;
            //    m_Body.rotation = m_ResetPos.rotation;
            //}
            //else
            //{
            //    m_Body.rotation = m_InitRotation;
            //    m_Body.position = m_InitPosition;
            //}

            ResetPIDParameter();

            //SendCustomEventDelayedFrames("ResetEnd", 5);
        }

        void ResetPIDParameter()
        {
            m_PrevPrevHightDiff = 0.0f;
            m_PrevHightDiff = 0.0f;
            m_IntegralHight = 0.0f;

            m_PrevPrevAngleXDiff = 0.0f;
            m_PrevAngleXDiff = 0.0f;
            m_IntegralAngleX = 0.0f;

            m_PrevPrevAngleZDiff = 0.0f;
            m_PrevAngleZDiff = 0.0f;
            m_IntegralAngleZ = 0.0f;

            m_Force = 0;

            m_FrontForce = 0;
            m_BackForce = 0;
            m_LeftForce = 0;
            m_RightForce = 0;

            m_PrevPrevAnglerYawDiff = 0.0f;
            m_PrevAnglerYawDiff = 0.0f;
            m_IntegralAnglerYaw = 0.0f;

            m_PrevPrevAnglerVDiff = Vector3.zero;
            m_PrevAnglerVDiff = Vector3.zero;
            m_IntegralAnglerV = Vector3.zero;
        }

        public void ResetEnd()
        {
            m_Body.isKinematic = false;
        }

        private void LookUpdate()
        {
            float engineMax = m_MaxPitchForce;//maxForce / body.mass;
            float pitch = m_EngineSpeed / Mathf.Max(engineMax, 0.0001f);

            if (m_AudioSource)
                m_AudioSource.pitch = Mathf.Min(pitch * m_PitchFactor, m_MaxPitch);

            var RotationDir = Vector3.up;

            switch (m_LookPropellerAxis)
            {
                case 0: // X
                    RotationDir = Vector3.right;
                    break;
                case 1: // Y
                    RotationDir = Vector3.up;
                    break;
                case 2: // Z
                    RotationDir = Vector3.forward;
                    break;
            }

            m_FrontRightFin.Rotate(RotationDir, -m_LookRotationSpeed * m_EngineSpeed);
            m_FrontLeftFin.Rotate(RotationDir, -m_LookRotationSpeed * m_EngineSpeed);
            m_BackRightFin.Rotate(RotationDir, m_LookRotationSpeed * m_EngineSpeed);
            m_BackLeftFin.Rotate(RotationDir, m_LookRotationSpeed * m_EngineSpeed);
        }

        private void InputUpdate()
        {
#if false //UNITY_EDITOR
            if (m_Mode == MODE_ACRO)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    m_Throttle = 1;
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    m_Throttle = -1;
                }
                else
                {
                    m_Throttle = 0;
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    m_Throttle = 1;
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    m_Throttle = -1;
                }
                else
                {
                    m_Throttle = 0;
                }
            }

            if (Input.GetKey(KeyCode.E))
            {
                m_Rudder = 1;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                m_Rudder = -1;
            }
            else
            {
                m_Rudder = 0;
            }

            if (Input.GetKey(KeyCode.W))
            {
                m_Elevator = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                m_Elevator = -1;
            }
            else
            {
                m_Elevator = 0;
            }

            if (Input.GetKey(KeyCode.D))
            {
                m_Aileron = -1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                m_Aileron = 1;
            }
            else
            {
                m_Aileron = 0;
            }
#endif

            switch (m_Mode)
            {
                case MODE_ANGLE:
                    {
                        InputAngleMode();
                    }
                    break;
                case MODE_ACRO:
                    {
                        InputAcroMode();
                    }
                    break;
            }

        }

        private float ThrottleCenterHoveringMapping(float maxForce)
        {
            // ホバリングに必要な出力は重力加速度×質量で決まる
            float hoveringForce = -Physics.gravity.y * m_Body.mass * 0.25f;
            // -1 -> 0 は スロットル0からホバリング出力まで、 0 -> 1 は ホバリング出力から最大上昇出力に線形マッピングされる。
            return m_Throttle > 0 ? Mathf.Lerp(hoveringForce, maxForce, m_Throttle) : Mathf.Lerp(hoveringForce, 0, -m_Throttle);
        }

        private void InputAngleMode()
        {
            m_TargetVelocity = m_Throttle * m_MaxSpeedOnHightAdjust;

            // 角度制限あり
            m_GoalAngleX = m_Elevator * m_MaxAngle;
            m_GoalAngleZ = -m_Aileron * m_MaxAngle;

            // ヨー制御
            m_GoalAnglerYaw = m_Rudder * m_YawForce;

            if (!m_HightAdjustMode)
            {
                // 最大上昇出力
                float maxRiseForceOutput = m_MaxThrottleForce; /*hoveringForce * upForce;*/

                float throttleForce = 0;
                if (m_ThrottleCenterHovering)
                {
                    throttleForce = ThrottleCenterHoveringMapping(maxRiseForceOutput);
                }
                else
                {
                    // 0から最大出力に線形マッピング
                    throttleForce = Mathf.Lerp(0, maxRiseForceOutput, (m_Throttle * 0.5f + 0.5f));
                }
                m_Force = throttleForce;
            }
        }

        private void InputAcroMode()
        {
            // 最大上昇出力
            {
                float maxRiseForceOutput = m_MaxThrottleForce;

                float throttleForce = 0;
                if (m_ThrottleCenterHovering)
                {
                    throttleForce = ThrottleCenterHoveringMapping(maxRiseForceOutput);
                }
                else
                {
                    // 0から最大出力に線形マッピング
                    throttleForce = Mathf.Lerp(0, maxRiseForceOutput, (m_Throttle * 0.5f + 0.5f));
                }
                m_Force = throttleForce;
            }

            // 角度制御：角速度基準
            {
                float goalAnglerVX = CalcBetaFlightRate(m_Elevator, m_RcRate, m_SuperRate, m_RcExpo);//elevator;
                float goalAnglerVZ = CalcBetaFlightRate(-m_Aileron, m_RcRate, m_SuperRate, m_RcExpo);//-aileron * rotateSensitivity;
                float goalAnglerVY = CalcBetaFlightRate(m_Rudder, m_RcRate, m_SuperRate, m_RcExpo);//rudder * yawForce;

                m_GoalAnglerV = new Vector3(goalAnglerVX, goalAnglerVY, goalAnglerVZ);
            }
        }

        /// <summary>
        /// 逆さまになったときにその場で姿勢を戻すコマンド
        /// </summary>
        public void FlipOver()
        {
            Debug.Log("FlipOver");
            // PIDをリセット
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ResetPIDParameter));

            // 機体の状態を変更
            m_Body.isKinematic = true;

            m_Body.velocity = Vector3.zero;
            m_Body.angularVelocity = Vector3.zero;

            m_Body.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(m_Body.transform.forward, Vector3.up));

            SendCustomEventDelayedFrames(nameof(ResetEnd), 5);
        }

        /// <summary>
        /// BetaFlightのレート変換式
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rcRate"></param>
        /// <param name="sRate"></param>
        /// <param name="expo"></param>
        /// <returns>毎秒角速度[ラジアン/s]</returns>
        private float CalcBetaFlightRate(float value, float rcRate, float sRate, float expo)
        {
            float sign = Mathf.Sign(value);
            float adsValue = Mathf.Abs(value);

            float superFactor = 1.0f / (1.0f - (adsValue * sRate));
            float rcCommandFactor = (Mathf.Pow(adsValue, 4.0f) * expo) + adsValue * (1 - expo);
            float expoFactor = 200 * rcCommandFactor * rcRate;

            float rate = (expoFactor * superFactor);

            // 角度からラジアンへ変換
            rate *= Mathf.Deg2Rad;

            return rate * sign;
        }

        /// <summary>
        /// 推力減衰：対気速度による推力減少の計算【未使用】
        /// </summary>
        private Vector3 CalcThrustDamping(Vector3 thrust)
        {
            Vector3 result = Vector3.zero;

            // 推力がゼロなら計算しない
            if (Mathf.Approximately(thrust.sqrMagnitude,0))
            {
                return result;
            }

            result = thrust;
            
            // 推力方向への対気速度を計算
            float airSpeed = Vector3.Dot(m_Body.velocity, thrust) / thrust.sqrMagnitude;

            Vector3 airVelocity = airSpeed * thrust * m_PropellarAirSpeedRate;

            //Debug.Log(airVelocity);

            if (airSpeed > 0)
            {
                // 推力方向と対気速度が同じ向きの場合
                // 推力が減少する
                result = result - airVelocity;
            }
            else
            {
                // 推力方向と対気速度が逆向きの場合
                // 推力が上昇する？
                //result = result - airVelocity;
            }

            return result;

        }

        /// <summary>
        /// プロペラ抵抗の計算
        /// </summary>
        private void CalcPropDrop(Vector3 propDir)
        {
            if(Mathf.Approximately(m_Force, 0)) return;

            float moveSpeed = Vector3.Dot(m_Body.velocity, propDir);

            var propDrag = Mathf.Min(m_TopSpeed - moveSpeed, 0.0f);

            m_Body.AddForce(m_Body.velocity.normalized * propDrag);

        }

        /// <summary>
        /// 高度維持PID処理
        /// </summary>
        private void HightAdjustPID()
        {

            {
                m_PrevPrevHightDiff = m_PrevHightDiff;
                m_PrevHightDiff = m_Body.velocity.y - m_TargetVelocity;
                m_IntegralHight += (m_PrevPrevHightDiff + m_PrevHightDiff) * 0.5f * Time.deltaTime;

                float P = m_PrevHightDiff * (m_Hight_Kp * m_Body.mass);
                float I = m_IntegralHight * (m_Hight_Ki * m_Body.mass);
                float D = (m_Hight_Kd * m_Body.mass) * (m_PrevHightDiff - m_PrevPrevHightDiff) / Time.deltaTime;

                float hightAdjustForce = P + I + D;

                var adjustForce = -hightAdjustForce;

                //adjustForce = Mathf.Abs(adjustForce) > (m_BrakeMax * m_Body.mass) ? Mathf.Sign(adjustForce) * (m_BrakeMax * m_Body.mass) : adjustForce;

                m_Force = Mathf.Max(m_Force + adjustForce, 0.0f);

            }
        }

        /// <summary>
        /// ドローン設定を適用する
        /// </summary>
        //public void ApplyDroneSetting()
        //{

        //    if(m_DroneSetting)
        //    {
        //        m_RcRate = m_DroneSetting.m_rcRate;
        //        m_SuperRate = m_DroneSetting.m_spRate;
        //        m_RcExpo = m_DroneSetting.m_expo;

        //        m_AnglerV_Kp = m_DroneSetting.m_acro_p;
        //        m_AnglerV_Ki = m_DroneSetting.m_acro_i;
        //        m_AnglerV_Kd = m_DroneSetting.m_acro_d;
        //    }
        //}

        public void ApplyDroneRate(float rcRate, float spRate, float expo)
        {
            m_RcRate = rcRate;
            m_SuperRate = spRate;
            m_RcExpo = expo;
        }

        public void ApplyAngluerVPID(float p, float i, float d)
        {
            m_AnglerV_Kp = p;
            m_AnglerV_Ki = i;
            m_AnglerV_Kd = d;
        }

        public void ApplyAnglePID(float p, float i, float d)
        {
            m_Angle_Kp = p;
            m_Angle_Ki = i;
            m_Angle_Kd = d;

            Debug.Log(string.Format("UDRONE: Apply AnglePID ({0},{1},{2})", p, i, d));
        }

        public Rigidbody GetRigidbody()
        {
            return m_Body;
        }


        public void SetDroneSoundVolume(float volume)
        {
            m_AudioSource.volume = volume;
        }

        /// <summary>
        /// ドローンの音声の固定先をコントローラーに固定するかしないか
        /// </summary>
        /// <param name="flag"></param>
        public void SetDroneAudioFixController(bool flag)
        {
            m_audioFixToController = flag;

            UpdateAudioFix();
        }

        /// <summary>
        /// ドローンのネームプレートを表示するかどうか
        /// </summary>
        /// <param name="flag"></param>
        public void SetShowDroneNamePlate(bool flag)
        {
            m_DroneNamePlate.ShowDroneNamePlate(flag);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if(player.isLocal)
            {
                m_Body.isKinematic = false;
                Networking.SetOwner(player, m_ManualSyncVariables.gameObject);
            }
            else
            {
                m_Body.isKinematic = true;
            }
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (m_Body == null)
            {
                m_Body = gameObject.GetComponent<Rigidbody>();
            }
            var center = m_Body.worldCenterOfMass;


            // RootOnly update will only copy the data for this behaviour from Udon to the proxy
            //this.UpdateProxy(ProxySerializationPolicy.RootOnly);
            //UdonSharpEditorUtility.CopyUdonToProxy(this);

            var axleLen = m_ForcePointRadius * COS45;

            var frontRight = transform.TransformPoint(new Vector3(axleLen, m_ForcePointHeight + m_Body.centerOfMass.y, axleLen));
            var frontLeft = transform.TransformPoint(new Vector3(-axleLen, m_ForcePointHeight + m_Body.centerOfMass.y, axleLen));
            var backRight = transform.TransformPoint(new Vector3(axleLen, m_ForcePointHeight + m_Body.centerOfMass.y, -axleLen));
            var backLeft = transform.TransformPoint(new Vector3(-axleLen, m_ForcePointHeight + m_Body.centerOfMass.y, -axleLen));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, frontRight);
            Gizmos.DrawLine(center, frontLeft);
            Gizmos.color = Color.black;
            Gizmos.DrawLine(center, backRight);
            Gizmos.DrawLine(center, backLeft);

            var downDir = m_Body.transform.TransformDirection(Vector3.down);
            var frontDir = m_Body.transform.TransformDirection(Vector3.forward);
            var rightDir = m_Body.transform.TransformDirection(Vector3.right);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(frontRight, frontRight + downDir * m_ForceRF);
            Gizmos.DrawLine(frontLeft, frontLeft + downDir * m_ForceLF);
            Gizmos.DrawLine(backRight, backRight + downDir * m_ForceRB);
            Gizmos.DrawLine(backLeft, backLeft + downDir * m_ForceLB);

            //Handles.color = Color.green;
            //Handles.DrawWireArc(frontRight, -downDir, frontDir, 270, 0.3f);
            //// 矢印の矢じり
            //Vector3 arrowStart = frontRight + frontDir * 0.3f;
            //Vector3 arrowEnd1 = rightDir * 0.1f + frontDir * 0.1f;
            //Vector3 arrowEnd2 = rightDir * 0.1f + frontDir * -0.1f;
            //Handles.DrawLine(arrowStart, arrowStart + arrowEnd1);
            //Handles.DrawLine(arrowStart, arrowStart + arrowEnd2);

            var arrowRadius = (frontRight - center).magnitude * 0.3f;

            Vector3 propellerUpVector = Vector3.zero;
            Vector3 propellerForwardVector = Vector3.zero;
            switch (m_LookPropellerAxis)
            {
                case 0: // X
                    propellerUpVector = Vector3.right;
                    propellerForwardVector = Vector3.up;
                    break;
                case 1: // Y
                    propellerUpVector = Vector3.up;
                    propellerForwardVector = Vector3.forward;
                    break;
                case 2: // Z
                    propellerUpVector = Vector3.forward;
                    propellerForwardVector = Vector3.up;
                    break;
            }

            if (m_FrontRightFin)
            {
                DrawCircleArrow(m_FrontRightFin.position, m_FrontRightFin.TransformDirection(propellerUpVector), m_FrontRightFin.TransformDirection(propellerForwardVector), arrowRadius, true, Color.green);
            }
            if (m_FrontLeftFin)
            {
                DrawCircleArrow(m_FrontLeftFin.position, m_FrontLeftFin.TransformDirection(propellerUpVector), m_FrontLeftFin.TransformDirection(propellerForwardVector), arrowRadius, true, Color.green);
            }
            if (m_BackRightFin)
            {
                DrawCircleArrow(m_BackRightFin.position, m_FrontRightFin.TransformDirection(propellerUpVector), m_BackRightFin.TransformDirection(propellerForwardVector), arrowRadius, false, Color.green);
            }
            if (m_BackLeftFin)
            {
                DrawCircleArrow(m_BackLeftFin.position, m_FrontRightFin.TransformDirection(propellerUpVector), m_BackLeftFin.TransformDirection(propellerForwardVector), arrowRadius, false, Color.green);
            }
        }

        private void DrawCircleArrow(Vector3 center, Vector3 upDir, Vector3 frontDir, float radius, bool rightRoll, Color color)
        {
            Vector3 rightDir = Vector3.Cross(upDir, frontDir).normalized;

            upDir *= rightRoll ? 1.0f : -1.0f;
            rightDir *= rightRoll ? 1.0f : -1.0f;

            Handles.color = color;
            Handles.DrawWireArc(center, upDir, frontDir, 270, radius);

            float arrowLen = radius / 3.0f;

            // 矢印の矢じり
            Vector3 arrowStart = center + frontDir * radius;
            Vector3 arrowEnd1 = rightDir * arrowLen + frontDir * arrowLen;
            Vector3 arrowEnd2 = rightDir * arrowLen + frontDir * -arrowLen;
            Handles.DrawLine(arrowStart, arrowStart + arrowEnd1);
            Handles.DrawLine(arrowStart, arrowStart + arrowEnd2);
        }
#endif
    }
}