﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UdonSharpEditor;

namespace Kurotori.UDrone
{
    [CustomEditor(typeof(SettingPanelManager))]
    public class SettingPanelManagerInspector : Editor
    {
        SerializedProperty _udrones;
        SerializedProperty _activaters;
        SerializedProperty _defaultPosition;
        SerializedProperty _settingPanels;
        SerializedProperty _mainPanel;

        private void OnEnable()
        {
            _udrones = serializedObject.FindProperty("udrones");
            _activaters = serializedObject.FindProperty("activaters");
            _defaultPosition = serializedObject.FindProperty("defaultPosition");
            _settingPanels = serializedObject.FindProperty("settingPanels");
            _mainPanel = serializedObject.FindProperty("mainPanel");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
                return;

            GUIStyle boldCenterStyle = new GUIStyle(GUI.skin.label);
            boldCenterStyle.alignment = TextAnchor.MiddleCenter; // 中心揃え
            boldCenterStyle.fontStyle = FontStyle.Bold; // 太字

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("--- UDroneを追加したらCollectUDroneを押してください ---", boldCenterStyle);
            EditorGUILayout.Space();

            if (GUILayout.Button("Collect UDrone"))
            {
                CollectUDrone();
            }

            EditorGUILayout.LabelField("UDrone : " + _udrones.arraySize);
            using (new EditorGUI.DisabledScope(true))
            {
                for (int i = 0; i < _udrones.arraySize; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(i.ToString() + ":", GUILayout.Width(20));
                        EditorGUILayout.ObjectField(_udrones.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                    }
                }
            }

            EditorGUILayout.Space(2);

            EditorGUILayout.LabelField("メニューコンポーネント", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_defaultPosition);
            EditorGUILayout.PropertyField(_mainPanel);
            EditorGUILayout.PropertyField(_activaters);
            EditorGUILayout.PropertyField(_settingPanels);

            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }

        void CollectUDrone()
        {
            List<UdonDroneCore> droneCores = new List<UdonDroneCore>();

            foreach(GameObject obj in FindObjectsOfType<GameObject>())
            {
                if(obj.activeInHierarchy)
                {
                    var droneCore = obj.GetComponent<UdonDroneCore>();

                    if ( droneCore != null)
                    {
                        droneCores.Add(droneCore);
                    }
                }
            }

            _udrones.arraySize = 0;
            _udrones.arraySize = droneCores.Count;

            for( int i = 0; i < _udrones.arraySize; i++ )
            {
                using(var element = _udrones.GetArrayElementAtIndex(i))
                {
                    element.objectReferenceValue = droneCores[i];
                }
            }
        }
    }
}