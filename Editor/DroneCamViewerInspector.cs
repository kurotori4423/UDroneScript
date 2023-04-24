using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UdonSharpEditor;

namespace Kurotori.UDrone
{
    [CustomEditor(typeof(DroneCamViewer))]
    public class DroneCamViewerInspector : Editor
    {
        SerializedProperty _droneCameraRigs;
        SerializedProperty _droneCam;
        SerializedProperty _turnOffObjects;
        SerializedProperty _turnOnObjects;

        SerializedProperty _text;

        SerializedProperty _isVirtualCameraMode;
        SerializedProperty _targetObject;

        private void OnEnable()
        {
            _droneCameraRigs = serializedObject.FindProperty("droneCameraRigs");
            _droneCam = serializedObject.FindProperty("droneCam");
            _turnOffObjects = serializedObject.FindProperty("turnOffObjects");
            _turnOnObjects = serializedObject.FindProperty("turnOnObjects");
            _text = serializedObject.FindProperty("text");
            _isVirtualCameraMode = serializedObject.FindProperty("isVirtualCameraMode");
            _targetObject = serializedObject.FindProperty("targetObject");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
                return;

            var droneCamViewer = target as DroneCamViewer;

            EditorGUILayout.LabelField("1. Add DroneCam below");
            EditorGUILayout.LabelField("1. ドローンカメラを以下に追加");

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                EditorGUILayout.PropertyField(_droneCam);
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("2. When you have finished installing the drone, press the \"Auto Setting\" button.");
            EditorGUILayout.LabelField("2. ドローンを配置し終わったら、下のボタンを押してください。");
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Auto Setting"))
            {
                CollectCameraRigs();
            }

            EditorGUILayout.LabelField("CameraRigs : " + _droneCameraRigs.arraySize);
            using (new EditorGUI.DisabledScope(true))
            {
                for (var i = 0; i < _droneCameraRigs.arraySize; ++i)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(i.ToString() + ":", GUILayout.Width(20));
                        EditorGUILayout.ObjectField(_droneCameraRigs.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                    }
                }
            }

            var style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = GUI.skin.button.normal.textColor;

            

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UI Elements", style);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(_turnOffObjects);
            EditorGUILayout.PropertyField(_turnOnObjects);
            EditorGUILayout.PropertyField(_text);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_isVirtualCameraMode);
            EditorGUILayout.PropertyField(_targetObject);


            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }

        void CollectCameraRigs()
        {
            List<GameObject> droneCamRigs = new List<GameObject>();

            foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
            {
                if (obj.activeInHierarchy)
                {
                    var droneController = obj.GetUdonSharpComponent<UdonDroneController>();
                    if (droneController)
                    {
                        if(droneController.droneCamRig)
                            droneCamRigs.Add(droneController.droneCamRig.gameObject);
                    }
                }
            }

            _droneCameraRigs.arraySize = 0;
            _droneCameraRigs.arraySize = droneCamRigs.Count;

            for (int i = 0; i < _droneCameraRigs.arraySize; ++i)
            {
                using (var element = _droneCameraRigs.GetArrayElementAtIndex(i))
                {
                    element.objectReferenceValue = droneCamRigs[i];
                }
            }

        }
    }
}