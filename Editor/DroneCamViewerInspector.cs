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
        SerializedProperty _droneCores;
        SerializedProperty _droneCam;
        SerializedProperty _turnOffObjects;
        SerializedProperty _turnOnObjects;
        SerializedProperty _ResolutionX;
        SerializedProperty _ResolutionY;

        SerializedProperty _text;

        SerializedProperty _isVirtualCameraMode;
        SerializedProperty _targetObject;
        SerializedProperty _renderTextureAssigners;

        private void OnEnable()
        {
            _droneCores = serializedObject.FindProperty("droneCores");
            _droneCam = serializedObject.FindProperty("droneCam");
            _ResolutionX = serializedObject.FindProperty("ResolutionX");
            _ResolutionY = serializedObject.FindProperty("ResolutionY");
            _turnOffObjects = serializedObject.FindProperty("turnOffObjects");
            _turnOnObjects = serializedObject.FindProperty("turnOnObjects");
            _text = serializedObject.FindProperty("text");
            _isVirtualCameraMode = serializedObject.FindProperty("isVirtualCameraMode");
            _targetObject = serializedObject.FindProperty("targetObject");
            _renderTextureAssigners = serializedObject.FindProperty("m_renderTextureAssigners");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
                return;

            var droneCamViewer = target as DroneCamViewer;

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                EditorGUILayout.PropertyField(_droneCam);
            }
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(true))
            {
                for (var i = 0; i < _droneCores.arraySize; ++i)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(i.ToString() + ":", GUILayout.Width(20));
                        EditorGUILayout.ObjectField(_droneCores.GetArrayElementAtIndex(i).objectReferenceValue, typeof(UdonDroneCore), true);
                    }
                }
            }

            

            var style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = GUI.skin.button.normal.textColor;

            EditorGUILayout.LabelField("Display Setting", style);
            EditorGUILayout.PropertyField(_renderTextureAssigners);
            EditorGUILayout.PropertyField(_ResolutionX);
            EditorGUILayout.PropertyField(_ResolutionY);

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

        //void CollectCameraRigs()
        //{
        //    List<Transform> droneCamRigs = new List<Transform>();

        //    foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
        //    {
        //        if (obj.activeInHierarchy)
        //        {
        //            var droneController = obj.GetComponent<UdonDroneController>();
        //            if (droneController)
        //            {
        //                if(droneController.droneCamRig)
        //                    droneCamRigs.Add(droneController.droneCamRig);
        //            }
        //        }
        //    }

        //    _droneCameraRigs.arraySize = 0;
        //    _droneCameraRigs.arraySize = droneCamRigs.Count;

        //    for (int i = 0; i < _droneCameraRigs.arraySize; ++i)
        //    {
        //        using (var element = _droneCameraRigs.GetArrayElementAtIndex(i))
        //        {
        //            element.objectReferenceValue = droneCamRigs[i];
        //        }
        //    }

        //}
    }
}