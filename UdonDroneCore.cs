
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILE_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

namespace Kurotori.UDrone
{
    /// <summary>
    /// ドローン本体スクリプト
    /// </summary>
    public class UdonDroneCore : UdonSharpBehaviour
    {
        float COS45 = 0.52532198881f;

        Rigidbody body;

        [Header("Controller")]
        [SerializeField]
        public UdonDroneController controller;

        [SerializeField]
        [Tooltip("リセット位置")]
        public Transform resetPos;



        [Header("機体制御設定 -----------------------------------------------------------------")]
        [SerializeField]
        [Tooltip("制御モード\n 0:アングルモード \n 1:アクロモード")]
        [Range(0, 1)]
        public int mode = 0;

        const int MODE_ANGLE = 0;
        const int MODE_ACRO = 1;

        [SerializeField]
        [Tooltip("高度維持機能[アングルモードのみ]")]
        bool HightAdjustMode = true;

        [SerializeField]
        [Tooltip("プロペラの逆転を許可(許可しない方がリアルですが、許可した方が操作性が上がります)")]
        bool allowPropellerReversal = true;

        [Header("機体形状設定 -----------------------------------------------------------------")]
        [SerializeField]
        [Tooltip("推進力が発生するポイントの重心からの距離です")]
        float forcePointRadius = 1.0f;
        [SerializeField]
        [Tooltip("推進力が発生するポイントの重心からの上下方向のオフセット量です")]
        float forcePointHeight = 0.1f;

        [SerializeField]
        [Tooltip("空気抵抗")]
        float drag = 0.18f;

        [Header("機体出力設定 -----------------------------------------------------------------")]
        [SerializeField]
        [Tooltip("最大機体出力: 重力と釣り合う状態を0とします。[機体質量に比例]")]
        float maxForce = 5.0f;

        [SerializeField]
        [Tooltip("最大上昇スロットル[機体質量に比例]")]
        float upForce = 2.0f;

        [SerializeField]
        [Tooltip("最大回転出力[回転時に使う最高スロットル出力][機体質量に比例]")]
        float maxRotationThrottleForce = 1.0f;

        [SerializeField]
        [Tooltip("ヨー回転時の最大出力[機体質量に比例]")]
        float yawForce = 10.0f;

        float force = 0;

        float frontForce = 0;
        float backForce = 0;
        float leftForce = 0;
        float rightForce = 0;

        float forceRF = 0;
        float forceRB = 0;
        float forceLF = 0;
        float forceLB = 0;

        [Header("操作感度設定 -----------------------------------------------------------------")]
        [SerializeField]
        [Tooltip("高度維持機能有効時の最大出力変化量")]
        float brakeMax = 0.3f; // 最大変化量

        // コントローラー入力
        [HideInInspector] public float throttle = -1; // 上昇
        [HideInInspector] public float rudder = 0; // 左右旋回
        [HideInInspector] public float elevator = 0; // 前進・後退
        [HideInInspector] public float aileron = 0; // 左右スライド

        float throttleOutput = 0; // 高度制御なしの時のスロットル出力


        [SerializeField]
        [Tooltip("スロットル感度[高度維持機能オフ時]")]
        float throttleSensitivity = 2.0f; // スロットル感度
        [SerializeField]
        [Tooltip("回転操作感度[アクロモード時]")]
        float rotateSensitivity = 5.0f; // 回転感度

        [Header("レート設定")]
        [SerializeField]
        public bool useVRRate = true;
        [SerializeField]
        float rcRate = 1.0f;
        [SerializeField]
        float superRate = 0.6f;
        [SerializeField]
        float rcExpo = 0.0f;
        [Header("VR用レート")]
        [SerializeField]
        float rcRateVR = 3.0f;
        [SerializeField]
        float superRateVR = 0.0f;
        [SerializeField]
        float rcExpoVR = 0.0f;


        [Header("高度維持機能関係 ---------------------------------------------------------------")]
        [SerializeField]
        [Tooltip("高度維持有効時の最大上昇下降速度(自由落下速度を超えると無視されます)")]
        float maxSpeedOnHightAdjust = 3.0f;

        [Header("高度維持PID")]
        [SerializeField]
        float hight_Kp = 0.2f;
        [SerializeField]
        float hight_Ki = 0.2f;
        [SerializeField]
        float hight_Kd = 0.1f;

        float targetVelocity = 0; // 高度維持機能の目標速度

        float prevPrevHightDiff = 0.0f;
        float prevHightDiff = 0.0f;
        float integralHight = 0.0f;

        [Header("姿勢制御関係 -----------------------------------------------------------------")]
        [Header("アングルモード専用")]
        // 姿勢制御機能
        [SerializeField]
        [Tooltip("最大傾斜角")]
        [Range(0, 90)]
        float maxAngle = 35.0f; // 最大傾斜角



        [Header("角度維持PID（アングルモード）")]

        [SerializeField]
        float angle_Kp = 0.7f;
        [SerializeField]
        float angle_Ki = 0.7f;
        [SerializeField]
        float angle_Kd = 0.1f;

        float goalAngleX = 0;
        float goalAngleZ = 0;

        float prevPrevAngleXDiff = 0.0f;
        float prevAngleXDiff = 0.0f;
        float integralAngleX = 0.0f;

        float prevPrevAngleZDiff = 0.0f;
        float prevAngleZDiff = 0.0f;
        float integralAngleZ = 0.0f;

        [Header("ヨー制御PID（アングルモード）")]
        [SerializeField]
        float anglerYaw_Kp = 1;
        [SerializeField]
        float anglerYaw_Ki = 0.5f;
        [SerializeField]
        float anglerYaw_Kd = 0.01f;

        float goalAnglerYaw = 0.0f;

        float prevPrevAnglerYawDiff = 0.0f;
        float prevAnglerYawDiff = 0.0f;
        float integralAnglerYaw = 0.0f;

        [Header("角速度制御PID（アクロモード）")]

        [SerializeField]
        float anglerV_Kp = 1.0f;
        [SerializeField]
        float anglerV_Ki = 0.5f;
        [SerializeField]
        float anglerV_Kd = 0.01f;

        Vector3 goalAnglerV;

        Vector3 prevPrevAnglerVDiff;
        Vector3 prevAnglerVDiff;
        Vector3 integralAnglerV;

        [Header("見た目の設定 -----------------------------------------------------------------")]
        [SerializeField]
        Transform frontRightFin;
        [SerializeField]
        Transform frontLeftFin;
        [SerializeField]
        Transform backRightFin;
        [SerializeField]
        Transform backLeftFin;
        [SerializeField]
        float lookRotationSpeed = 3;
        [SerializeField]
        [Tooltip("回転軸[X = 0, Y = 1, Z = 2]")]
        [Range(0, 2)]
        int lookPropellerAxis = 0;

        [UdonSynced(UdonSyncMode.Smooth)]
        float engineSpeed;

        [SerializeField]
        AudioClip audioClip;
        [SerializeField]
        AudioSource audioSource;
        [SerializeField]
        float pitchFactor = 2.0f;
        [SerializeField]
        float maxPitch = 10.0f;

        [Header("UI")]
        [SerializeField]
        GameObject Mode_ACRO;
        [SerializeField]
        GameObject Mode_ANGLE;

        // リセット用初期姿勢
        Vector3 initPosition;
        Quaternion initRotation;

        [Tooltip("操作状態")]
        public bool isArm = false;

        void Start()
        {
            isArm = false;

            body = gameObject.GetComponent<Rigidbody>();

            // 空気抵抗は自前で計算するため0に設定
            body.drag = 0.0f;

            if (controller != null)
                controller.SetDrone(this);

            if (audioSource != null && audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.loop = true;
                audioSource.Play();
            }

            initPosition = body.position;
            initRotation = body.rotation;

            switch (mode)
            {
                case MODE_ACRO:
                    if (Mode_ANGLE)
                        Mode_ANGLE.SetActive(false);
                    if (Mode_ACRO)
                        Mode_ACRO.SetActive(true);
                    break;
                case MODE_ANGLE:
                    if (Mode_ANGLE)
                        Mode_ANGLE.SetActive(true);
                    if (Mode_ACRO)
                        Mode_ACRO.SetActive(false);
                    break;
            }

        }

        private void FixedUpdate()
        {
#if !UNITY_EDITOR
        if (!Networking.LocalPlayer.IsOwner(gameObject))
        {
            LookUpdate();
            return;
        }
#endif
            LookUpdate();

            InputUpdate();

            var axleLen = forcePointRadius * COS45;

            var frontRight = transform.TransformPoint(new Vector3(axleLen, forcePointHeight + body.centerOfMass.y, axleLen));
            var frontLeft = transform.TransformPoint(new Vector3(-axleLen, forcePointHeight + body.centerOfMass.y, axleLen));
            var backRight = transform.TransformPoint(new Vector3(axleLen, forcePointHeight + body.centerOfMass.y, -axleLen));
            var backLeft = transform.TransformPoint(new Vector3(-axleLen, forcePointHeight + body.centerOfMass.y, -axleLen));

            var downDir = body.transform.TransformDirection(Vector3.up);

            if (isArm)
            {

                // PID制御 上下方向速度を0になるように調整する
                if (HightAdjustMode)
                {
                    HightAdjustPID();
                }

                // 姿勢制御 特定の角度を維持する

                switch (mode)
                {
                    case MODE_ANGLE:
                        {
                            float angleX = Mathf.Repeat(body.transform.localRotation.eulerAngles.x + 180, 360) - 180;
                            float angleZ = Mathf.Repeat(body.transform.localRotation.eulerAngles.z + 180, 360) - 180;

                            {
                                prevPrevAngleXDiff = prevAngleXDiff;

                                prevAngleXDiff = angleX - goalAngleX;
                                integralAngleX = (prevPrevAngleXDiff + prevAngleXDiff) * 0.5f * Time.deltaTime;

                                float P = prevAngleXDiff * (angle_Kp * body.mass);
                                float I = integralAngleX * (angle_Ki * body.mass);
                                float D = (angle_Kd * body.mass) * (prevAngleXDiff - prevPrevAngleXDiff) / Time.deltaTime;

                                float adjustAngleXForce = -(P + I + D);

                                adjustAngleXForce = Mathf.Sign(adjustAngleXForce) * Mathf.Min(Mathf.Abs(adjustAngleXForce), maxRotationThrottleForce * body.mass);

                                frontForce = -adjustAngleXForce;
                                backForce = adjustAngleXForce;
                            }


                            {
                                prevPrevAngleZDiff = prevAngleZDiff;
                                prevAngleZDiff = angleZ - goalAngleZ;
                                integralAngleZ = (prevPrevAngleZDiff + prevAngleZDiff) * 0.5f * Time.deltaTime;

                                float P = prevAngleZDiff * (anglerYaw_Kp * body.mass);
                                float I = integralAngleZ * (angle_Ki * body.mass);
                                float D = (angle_Kd * body.mass) * (prevAngleZDiff - prevPrevAngleZDiff) / Time.deltaTime;

                                float adjustAngleZForce = -(P + I + D);

                                adjustAngleZForce = Mathf.Sign(adjustAngleZForce) * Mathf.Min(Mathf.Abs(adjustAngleZForce), maxRotationThrottleForce * body.mass);

                                leftForce = -adjustAngleZForce;
                                rightForce = adjustAngleZForce;
                            }

                            {

                                prevPrevAnglerYawDiff = prevAnglerYawDiff;
                                prevAnglerYawDiff = body.angularVelocity.y - goalAnglerYaw;
                                integralAnglerYaw = (prevPrevAnglerYawDiff + prevAnglerYawDiff) * 0.5f * Time.deltaTime;

                                float P = prevAnglerYawDiff * (anglerYaw_Kp * body.mass);
                                float I = integralAnglerYaw * (anglerYaw_Ki * body.mass);
                                float D = (anglerYaw_Kd * body.mass) * (prevAnglerYawDiff - prevPrevAnglerYawDiff) / Time.deltaTime;

                                float adjustAnglerYawForce = -(P + I + D);
                                float angleForceY = Mathf.Sign(adjustAnglerYawForce) * Mathf.Min(Mathf.Abs(adjustAnglerYawForce), maxRotationThrottleForce * body.mass);

                                body.AddTorque(Vector3.up * (angleForceY * yawForce));
                            }
                        }
                        break;
                    case MODE_ACRO:
                        {
                            Vector3 localAnglerVelocity = body.transform.InverseTransformDirection(body.angularVelocity).normalized * body.angularVelocity.magnitude;

                            prevPrevAnglerVDiff = prevAnglerVDiff;

                            prevAnglerVDiff = localAnglerVelocity - goalAnglerV;
                            integralAnglerV = (prevPrevAnglerVDiff - prevAnglerVDiff) * 0.5f * Time.deltaTime;

                            // PID値は機体質量に比例
                            Vector3 P = prevAnglerVDiff * (anglerV_Kp * body.mass);
                            Vector3 I = integralAnglerV * (anglerV_Ki * body.mass);
                            Vector3 D = (anglerV_Kd * body.mass) * (prevAnglerVDiff - prevPrevAnglerVDiff) / Time.deltaTime;

                            Vector3 pid = -(P + I + D);

                            float angleForceX = Mathf.Sign(pid.x) * Mathf.Min(Mathf.Abs(pid.x), maxRotationThrottleForce * body.mass);
                            frontForce = -angleForceX;
                            backForce = angleForceX;

                            float angleForceZ = Mathf.Sign(pid.z) * Mathf.Min(Mathf.Abs(pid.z), maxRotationThrottleForce * body.mass);
                            leftForce = -angleForceZ;
                            rightForce = angleForceZ;

                            float angleForceY = Mathf.Sign(pid.y) * Mathf.Min(Mathf.Abs(pid.y), maxRotationThrottleForce * body.mass);
                            body.AddRelativeTorque(Vector3.up * (angleForceY * yawForce));

                        }
                        break;
                }


                if (allowPropellerReversal)
                {
                    forceRF = Mathf.Min(force + frontForce + rightForce, maxForce);
                    forceLF = Mathf.Min(force + frontForce + leftForce, maxForce);
                    forceRB = Mathf.Min(force + backForce + rightForce, maxForce);
                    forceLB = Mathf.Min(force + backForce + leftForce, maxForce);
                }
                else
                {
                    forceRF = Mathf.Clamp(force + frontForce + rightForce, 0, maxForce);
                    forceLF = Mathf.Clamp(force + frontForce + leftForce, 0, maxForce);
                    forceRB = Mathf.Clamp(force + backForce + rightForce, 0, maxForce);
                    forceLB = Mathf.Clamp(force + backForce + leftForce, 0, maxForce);
                }

            }
            else
            {
                forceRF = 0;
                forceLF = 0;
                forceRB = 0;
                forceLB = 0;
            }
            float forceAll = Mathf.Abs(forceRF + forceLF + forceRB + forceLB);

            engineSpeed = forceAll / body.mass;

            body.AddForceAtPosition(downDir * forceRF, frontRight);
            body.AddForceAtPosition(downDir * forceLF, frontLeft);
            body.AddForceAtPosition(downDir * forceRB, backRight);
            body.AddForceAtPosition(downDir * forceLB, backLeft);

            // 空気抵抗計算
            {
                body.AddForce(-body.velocity * drag);
            }

        }

        public void ChangeMode_Angle()
        {
            mode = MODE_ANGLE;
        }

        public void ChangeMode_Acro()
        {
            mode = MODE_ACRO;
        }

        public void ChangeMode()
        {
            switch (mode)
            {
                case MODE_ANGLE:
                    mode = MODE_ACRO;
                    if (Mode_ANGLE)
                        Mode_ANGLE.SetActive(false);
                    if (Mode_ACRO)
                        Mode_ACRO.SetActive(true);
                    break;
                case MODE_ACRO:
                    mode = MODE_ANGLE;
                    if (Mode_ANGLE)
                        Mode_ANGLE.SetActive(true);
                    if (Mode_ACRO)
                        Mode_ACRO.SetActive(false);
                    break;
            }
        }

        public override void OnPickup()
        {

        }

        public override void OnDrop()
        {
            if (controller != null)
                controller.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "ResetOwner");
        }

        public void ResetAll_All()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetAll");
        }

        public void ResetAll()
        {

            body.isKinematic = true;

            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            if (resetPos)
            {
                body.position = resetPos.position;
                body.rotation = resetPos.rotation;
            }
            else
            {
                body.rotation = initRotation;
                body.position = initPosition;
            }




            prevPrevHightDiff = 0.0f;
            prevHightDiff = 0.0f;
            integralHight = 0.0f;

            prevPrevAngleXDiff = 0.0f;
            prevAngleXDiff = 0.0f;
            integralAngleX = 0.0f;

            prevPrevAngleZDiff = 0.0f;
            prevAngleZDiff = 0.0f;
            integralAngleZ = 0.0f;

            force = 0;

            frontForce = 0;
            backForce = 0;
            leftForce = 0;
            rightForce = 0;

            prevPrevAnglerYawDiff = 0.0f;
            prevAnglerYawDiff = 0.0f;
            integralAnglerYaw = 0.0f;

            prevPrevAnglerVDiff = Vector3.zero;
            prevAnglerVDiff = Vector3.zero;
            integralAnglerV = Vector3.zero;

            SendCustomEventDelayedFrames("ResetEnd", 5);
        }

        public void ResetEnd()
        {
            body.isKinematic = false;
        }

        private void LookUpdate()
        {
            float engineMax = maxForce / body.mass;
            float pitch = engineSpeed / Mathf.Max(engineMax, 0.0001f);

            if (audioSource)
                audioSource.pitch = Mathf.Min(pitch * pitchFactor, maxPitch);

            var RotationDir = Vector3.up;

            switch (lookPropellerAxis)
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

            frontRightFin.Rotate(RotationDir, -lookRotationSpeed * engineSpeed);
            frontLeftFin.Rotate(RotationDir, -lookRotationSpeed * engineSpeed);
            backRightFin.Rotate(RotationDir, lookRotationSpeed * engineSpeed);
            backLeftFin.Rotate(RotationDir, lookRotationSpeed * engineSpeed);
        }

        private void InputUpdate()
        {
#if UNITY_EDITOR
            if (mode == MODE_ACRO)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    throttle = 1;
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    throttle = -1;
                }
                else
                {
                    throttle = 0;
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    throttle = 1;
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    throttle = -1;
                }
                else
                {
                    throttle = 0;
                }
            }

            if (Input.GetKey(KeyCode.E))
            {
                rudder = 1;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                rudder = -1;
            }
            else
            {
                rudder = 0;
            }

            if (Input.GetKey(KeyCode.W))
            {
                elevator = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                elevator = -1;
            }
            else
            {
                elevator = 0;
            }

            if (Input.GetKey(KeyCode.D))
            {
                aileron = -1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                aileron = 1;
            }
            else
            {
                aileron = 0;
            }
#endif

            switch (mode)
            {
                case MODE_ANGLE:
                    {
                        // 高度制御を行う
                        targetVelocity = throttle * maxSpeedOnHightAdjust;

                        // 角度制限あり
                        goalAngleX = elevator * maxAngle;
                        goalAngleZ = -aileron * maxAngle;

                        // ヨー制御
                        goalAnglerYaw = rudder * yawForce;

                    }
                    break;
                case MODE_ACRO:
                    {
                        // 高度制御なし

                        // ホバリングに必要な出力は重力加速度×質量で決まる
                        float hoveringForce = -Physics.gravity.y * body.mass * 0.25f;

                        // 最大上昇出力
                        float maxRiseForceOutput = hoveringForce * upForce;

                        // -1 -> 0 は スロットル0からホバリング出力まで、 0 -> 1 は ホバリング出力から最大上昇出力に線形マッピングされる。
                        throttleOutput = throttle;
                        float throttleForce = throttleOutput > 0 ? Mathf.Lerp(hoveringForce, maxRiseForceOutput * throttleSensitivity, throttleOutput) : Mathf.Lerp(hoveringForce, 0, -throttleOutput);
                        force = throttleForce;

                        // 角度制御：角速度基準

                        float goalAnglerVX = 0;
                        float goalAnglerVZ = 0;
                        float goalAnglerVY = 0;

                        if (useVRRate)
                        {
                            goalAnglerVX = CalcBetaFlightRate(elevator, rcRateVR, superRateVR, rcExpoVR);//elevator;
                            goalAnglerVZ = CalcBetaFlightRate(-aileron, rcRateVR, superRateVR, rcExpoVR);//-aileron * rotateSensitivity;
                            goalAnglerVY = CalcBetaFlightRate(rudder, rcRateVR, superRateVR, rcExpoVR);//rudder * yawForce;
                        }
                        else
                        {
                            goalAnglerVX = CalcBetaFlightRate(elevator, rcRate, superRate, rcExpo);//elevator;
                            goalAnglerVZ = CalcBetaFlightRate(-aileron, rcRate, superRate, rcExpo);//-aileron * rotateSensitivity;
                            goalAnglerVY = CalcBetaFlightRate(rudder, rcRate, superRate, rcExpo);//rudder * yawForce;
                        }
                        goalAnglerV = new Vector3(goalAnglerVX, goalAnglerVY, goalAnglerVZ);
                    }
                    break;
            }


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

        private void HightAdjustPID()
        {

            {
                prevPrevHightDiff = prevHightDiff;
                prevHightDiff = body.velocity.y - targetVelocity;
                integralHight += (prevPrevHightDiff + prevHightDiff) * 0.5f * Time.deltaTime;

                float P = prevHightDiff * (hight_Kp * body.mass);
                float I = integralHight * (hight_Ki * body.mass);
                float D = (hight_Kd * body.mass) * (prevHightDiff - prevPrevHightDiff) / Time.deltaTime;

                float hightAdjustForce = P + I + D;

                var adjustForce = -hightAdjustForce;

                adjustForce = Mathf.Abs(adjustForce) > (brakeMax * body.mass) ? Mathf.Sign(adjustForce) * (brakeMax * body.mass) : adjustForce;

                force = Mathf.Max(force + adjustForce, 0.0f);

            }
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (body == null)
            {
                body = gameObject.GetComponent<Rigidbody>();
            }
            var center = body.worldCenterOfMass;


            // RootOnly update will only copy the data for this behaviour from Udon to the proxy
            this.UpdateProxy(ProxySerializationPolicy.RootOnly);

            var axleLen = forcePointRadius * COS45;

            var frontRight = transform.TransformPoint(new Vector3(axleLen, forcePointHeight + body.centerOfMass.y, axleLen));
            var frontLeft = transform.TransformPoint(new Vector3(-axleLen, forcePointHeight + body.centerOfMass.y, axleLen));
            var backRight = transform.TransformPoint(new Vector3(axleLen, forcePointHeight + body.centerOfMass.y, -axleLen));
            var backLeft = transform.TransformPoint(new Vector3(-axleLen, forcePointHeight + body.centerOfMass.y, -axleLen));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, frontRight);
            Gizmos.DrawLine(center, frontLeft);
            Gizmos.color = Color.black;
            Gizmos.DrawLine(center, backRight);
            Gizmos.DrawLine(center, backLeft);

            var downDir = body.transform.TransformDirection(Vector3.down);
            var frontDir = body.transform.TransformDirection(Vector3.forward);
            var rightDir = body.transform.TransformDirection(Vector3.right);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(frontRight, frontRight + downDir * forceRF);
            Gizmos.DrawLine(frontLeft, frontLeft + downDir * forceLF);
            Gizmos.DrawLine(backRight, backRight + downDir * forceRB);
            Gizmos.DrawLine(backLeft, backLeft + downDir * forceLB);

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
            switch (lookPropellerAxis)
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

            if (frontRightFin)
            {
                DrawCircleArrow(frontRightFin.position, frontRightFin.TransformDirection(propellerUpVector), frontRightFin.TransformDirection(propellerForwardVector), arrowRadius, true, Color.green);
            }
            if (frontLeftFin)
            {
                DrawCircleArrow(frontLeftFin.position, frontLeftFin.TransformDirection(propellerUpVector), frontLeftFin.TransformDirection(propellerForwardVector), arrowRadius, true, Color.green);
            }
            if (backRightFin)
            {
                DrawCircleArrow(backRightFin.position, frontRightFin.TransformDirection(propellerUpVector), backRightFin.TransformDirection(propellerForwardVector), arrowRadius, false, Color.green);
            }
            if (backLeftFin)
            {
                DrawCircleArrow(backLeftFin.position, frontRightFin.TransformDirection(propellerUpVector), backLeftFin.TransformDirection(propellerForwardVector), arrowRadius, false, Color.green);
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