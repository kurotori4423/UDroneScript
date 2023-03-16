using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UdonSharpEditor;

namespace Kurotori.UDrone
{

    [CustomEditor(typeof(JoyInputAttacher))]
    public class JoyInputAttacherInspector : Editor
    {
        private bool _isOpen = false;

        SerializedProperty _controllersObj;

        SerializedProperty _currentLHText;
        SerializedProperty _currentLVText;
        SerializedProperty _currentRHText;
        SerializedProperty _currentRVText;

        SerializedProperty _InvLHToggle;
        SerializedProperty _InvLVToggle;
        SerializedProperty _InvRHToggle;
        SerializedProperty _InvRVToggle;

        SerializedProperty _LHDropDown;
        SerializedProperty _LVDropDown;
        SerializedProperty _RHDropDown;
        SerializedProperty _RVDropDown;

        SerializedProperty _stateDisplay;
        SerializedProperty _RightStickInputCheck;
        SerializedProperty _LeftStickInputCheck;

        SerializedProperty _moveSpeed;

        private void OnEnable()
        {
            _controllersObj = serializedObject.FindProperty("controllersObj");

            _currentLHText = serializedObject.FindProperty("currentLHText");
            _currentLVText = serializedObject.FindProperty("currentLVText");
            _currentRHText = serializedObject.FindProperty("currentRHText");
            _currentRVText = serializedObject.FindProperty("currentRVText");

            _InvLHToggle = serializedObject.FindProperty("InvLHToggle");
            _InvLVToggle = serializedObject.FindProperty("InvLVToggle");
            _InvRHToggle = serializedObject.FindProperty("InvRHToggle");
            _InvRVToggle = serializedObject.FindProperty("InvRVToggle");

            _LHDropDown = serializedObject.FindProperty("LHDropDown");
            _LVDropDown = serializedObject.FindProperty("LVDropDown");
            _RHDropDown = serializedObject.FindProperty("RHDropDown");
            _RVDropDown = serializedObject.FindProperty("RVDropDown");

            _stateDisplay = serializedObject.FindProperty("stateDisplay");
            _RightStickInputCheck = serializedObject.FindProperty("RightStickInputCheck");
            _LeftStickInputCheck = serializedObject.FindProperty("LeftStickInputCheck");
            _moveSpeed = serializedObject.FindProperty("moveSpeed");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
                return;

            var joyInputAttacher = target as JoyInputAttacher;

            EditorGUILayout.LabelField("When you have finished installing the drone, press the \"Collect UDrone\" button.");
            EditorGUILayout.LabelField("ドローンを配置し終わったら、下のボタンを押してください。");

            if (GUILayout.Button("Collect UDrone"))
            {
                CollectUDroneController();
            }

            EditorGUILayout.LabelField("UDrone Controller List : " + _controllersObj.arraySize);
            using(new EditorGUI.DisabledScope(true))
            {
                for (var i = 0; i < _controllersObj.arraySize; ++i)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(i.ToString() + ":", GUILayout.Width(20));
                        EditorGUILayout.ObjectField(_controllersObj.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                    }
                }
            }

            _isOpen = EditorGUILayout.Foldout(_isOpen, "UI Elements");

            if (_isOpen)
            {
                EditorGUILayout.PropertyField(_currentLHText);
                EditorGUILayout.PropertyField(_currentLVText);
                EditorGUILayout.PropertyField(_currentRVText);
                EditorGUILayout.PropertyField(_currentRHText);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_InvLHToggle);
                EditorGUILayout.PropertyField(_InvLVToggle);
                EditorGUILayout.PropertyField(_InvRHToggle);
                EditorGUILayout.PropertyField(_InvRVToggle);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_LHDropDown);
                EditorGUILayout.PropertyField(_LVDropDown);
                EditorGUILayout.PropertyField(_RHDropDown);
                EditorGUILayout.PropertyField(_RVDropDown);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_stateDisplay);
                EditorGUILayout.PropertyField(_RightStickInputCheck);
                EditorGUILayout.PropertyField(_LeftStickInputCheck);
                EditorGUILayout.PropertyField(_moveSpeed);
            }

            if (serializedObject.hasModifiedProperties)
                serializedObject.ApplyModifiedProperties();
        }

        void CollectUDroneController()
        {
            List<GameObject> droneController = new List<GameObject>();

            foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
            {
                if (obj.activeInHierarchy)
                {
                    if (obj.GetComponent<UdonDroneController>() != null)
                    {
                        droneController.Add(obj);
                    }
                }
            }

            _controllersObj.arraySize = 0;
            _controllersObj.arraySize = droneController.Count;

            for (int i = 0; i < _controllersObj.arraySize; ++i)
            {
                using (var element = _controllersObj.GetArrayElementAtIndex(i))
                {
                    element.objectReferenceValue = droneController[i];
                }
            }

        }
    }
}