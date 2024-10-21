
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace Kurotori.UDrone
{
    /// <summary>
    /// ドローンの観戦カメラのレンダーテクスチャをメッシュに対して割り当てるUdon
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None), RequireComponent(typeof(Renderer))]
    public class DroneCamRenderTextureAssigner : UdonSharpBehaviour
    {
        private Renderer m_renderer = null;

        [SerializeField]
        private string m_TextureName = "";

        /// <summary>
        /// 指定のレンダラーに対してメインテクスチャにレンダーテクスチャを割り当てる。
        /// </summary>
        /// <param name="renderTexture"></param>
        public void SetRenderTexture(RenderTexture renderTexture)
        {
            if(m_renderer == null)
            {
                m_renderer = GetComponent<Renderer>();
            }

            if (m_TextureName.Equals(""))
            {
                m_renderer.material.mainTexture = renderTexture;
            }
            else
            {
                m_renderer.material.SetTexture(m_TextureName, renderTexture);
            }
        }
    }
}