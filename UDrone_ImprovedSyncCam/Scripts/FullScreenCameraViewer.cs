
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace Kurotori.UDrone
{
    /// <summary>
    /// フルスクリーン表示用のカメラビューア
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None), DefaultExecutionOrder(1)]
    public class FullScreenCameraViewer : UdonSharpBehaviour
    {
        [SerializeField,Tooltip("ビューアのオンオフに使うキー")]
        KeyCode m_turnOnOffCode = KeyCode.F11;
        [SerializeField, Tooltip("ビューアに指定するカメラ")]
        Camera[] cameras;

        [SerializeField, Tooltip("ビューアの表示領域のRawImageのプレハブ")]
        RawImage m_viewerImagePrefab;

        [SerializeField, Tooltip("RawImageをインスタンス化する親オブジェクト")]
        Transform m_parent;


        RawImage[] m_viewerImages;
        
        int m_cameraIndex = 0;

        bool m_isOn = false;

        private void Start()
        {
            m_viewerImages = new RawImage[cameras.Length];

            for(int i = 0; i < cameras.Length; i++)
            {
                var obj = Instantiate(m_viewerImagePrefab.gameObject, m_parent);
                m_viewerImages[i] = obj.GetComponent<RawImage>();
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(m_turnOnOffCode))
            {
                ToggleOnOff();
            }

            if(m_isOn && Input.GetKeyDown(KeyCode.RightArrow))
            {
                NextView();
            }

            if(m_isOn && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PrevView();
            }
        }

        /// <summary>
        /// ビューのオンオフを切り替える
        /// </summary>
        private void ToggleOnOff()
        {
            if(m_isOn)
            {
                // 押したときオンの時
                var rawImage = m_viewerImages[m_cameraIndex];
                rawImage.gameObject.SetActive(false);
                m_isOn = false;
            }
            else
            {
                // 押したときオフの時
                var rawImage = m_viewerImages[m_cameraIndex];
                rawImage.gameObject.SetActive(true);
                rawImage.texture = cameras[m_cameraIndex].targetTexture;

                m_isOn = true;
            }
        }

        /// <summary>
        /// 次のビューを表示する
        /// </summary>
        private void NextView()
        {
            // 現在のビューをオフにする
            var currentView = m_viewerImages[m_cameraIndex];
            currentView.gameObject.SetActive(false);
            
            // 次のカメラを有効に
            var nextIndex = (m_cameraIndex + 1) % cameras.Length;
            var nextView = m_viewerImages[nextIndex];
            nextView.gameObject.SetActive(true);
            nextView.texture = cameras[nextIndex].targetTexture;

            m_cameraIndex = nextIndex;
        }

        /// <summary>
        /// 前のビューを表示する
        /// </summary>
        private void PrevView()
        {
            // 現在のビューをオフにする
            var currentView = m_viewerImages[m_cameraIndex];
            currentView.gameObject.SetActive(false);

            // 前のカメラを有効に
            var prevIndex = (m_cameraIndex - 1) < 0 ? (cameras.Length - 1) : (m_cameraIndex - 1);
            var prevView = m_viewerImages[prevIndex];
            prevView.gameObject.SetActive(true);
            prevView.texture = cameras[prevIndex].targetTexture;

            m_cameraIndex = prevIndex;
        }
    }
}