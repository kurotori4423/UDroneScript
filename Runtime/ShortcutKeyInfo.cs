#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Kurotori.UDrone
{

    public class ShortcutKeyInfo : MonoBehaviour
    {
        public PlayerCameraOverride playerCameraOverrider;
        public TMP_Text shortcutInfoText;
    }
}

#endif