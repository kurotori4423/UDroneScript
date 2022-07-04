using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UdonSharpEditor;

#if false

[CustomEditor(typeof(UdonDroneCore))]
public class UdonDroneCoreEditor : Editor
{
    static Color GetDefaultColor()
    {
        if(EditorGUIUtility.isProSkin)
        {
            return Color.white;
        }
        else
        {
            return Color.black;
        }
    }

    static GUIStyle GetHeaderStyle()
    {
        GUIStyle style = new GUIStyle();

        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = GetDefaultColor();

        return style;
    }

    static void DrawHorizontalLine()
    {
        Color oldColor = GUI.color;
        GUI.color = GetDefaultColor();

        GUILayout.Box("", new GUIStyle(), GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUI.color = oldColor;
    }

    public enum FLYMODE
    {
        ANGLE,
        ACRO
    };

    static void DrawHeader(string label)
    {
        EditorGUILayout.LabelField(new GUIContent(label), GetHeaderStyle());
    }

    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

        UdonDroneCore udonDroneCore = (UdonDroneCore)target;

        udonDroneCore.controller = EditorGUILayout.ObjectField(new GUIContent("Controller", "コントローラー"),udonDroneCore.controller, typeof(UdonDroneController), true) as UdonDroneController;
        udonDroneCore.resetPos = EditorGUILayout.ObjectField(new GUIContent("Reset Position", "リセット位置"),udonDroneCore.resetPos, typeof(Transform), true) as Transform;

        EditorGUILayout.Space();

        UdonSharpGUI.DrawUILine();
        DrawHeader("機体制御設定");

        udonDroneCore.mode = Convert.ToInt32(EditorGUILayout.EnumPopup(new GUIContent("Flymode", "飛行モード"), (FLYMODE)udonDroneCore.mode));


    }
}

#endif