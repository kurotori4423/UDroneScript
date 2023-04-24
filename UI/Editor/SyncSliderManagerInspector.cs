using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UdonSharpEditor;


[CustomEditor(typeof(SyncSliderManager))]
public class SyncSliderManagerInspector : Editor
{

    SerializedProperty _sliderTag;
    SerializedProperty _sliders;
    SerializedProperty _udons;
    SerializedProperty _customEventName;
    SerializedProperty _shareValue;

    private void OnEnable()
    {
        _sliderTag = serializedObject.FindProperty("sliderTag");
        _sliders = serializedObject.FindProperty("sliders");
        _udons = serializedObject.FindProperty("udons");
        _customEventName = serializedObject.FindProperty("customEventName");
        _shareValue = serializedObject.FindProperty("shareValue");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            return;

        var syncSliderManager = target as SyncSliderManager;

        EditorGUILayout.PropertyField(_sliderTag);

        if (GUILayout.Button("Collect SyncSlider"))
        {
            CollectSlider();
        }

        EditorGUILayout.LabelField("Slider List : " + _sliders.arraySize);
        using (new EditorGUI.DisabledScope(true))
        {
            for (var i = 0; i < _sliders.arraySize; ++i)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(i.ToString() + ":", GUILayout.Width(20));
                    EditorGUILayout.ObjectField(_sliders.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Slider), true);
                }
            }
        }
        EditorGUILayout.PropertyField(_udons);
        EditorGUILayout.PropertyField(_customEventName);
        EditorGUILayout.PropertyField(_shareValue);

        if (serializedObject.hasModifiedProperties)
            serializedObject.ApplyModifiedProperties();
    }

    public void CollectSlider()
    {
        var syncSliderManager = target as SyncSliderManager;

        var udon = UdonSharpEditorUtility.GetBackingUdonBehaviour(syncSliderManager);

        List<Slider> sliders = new List<Slider>();

        foreach (var  sliderInfo in StageUtility.GetCurrentStageHandle().FindComponentsOfType<SyncSliderInfo>())
        {
            {
                if (sliderInfo != null && sliderInfo.targetTag.Equals(syncSliderManager.sliderTag))
                {
                    sliders.Add(sliderInfo.gameObject.GetComponent<Slider>());
                }
            }
        }

        _sliders.arraySize = 0;
        _sliders.arraySize = sliders.Count;

        for(int i = 0; i < _sliders.arraySize; ++i)
        {
            using(var element = _sliders.GetArrayElementAtIndex(i))
            {
                element.objectReferenceValue = sliders[i];

                using (var so = new SerializedObject(sliders[i]))
                {
                    using(var callsProperty = so.FindProperty("m_OnValueChanged.m_PersistentCalls.m_Calls"))
                    {
                        callsProperty.arraySize = 0;
                        callsProperty.arraySize = 1;

                        using (var callelement = callsProperty.GetArrayElementAtIndex(0))
                        {
                            callelement.FindPropertyRelative("m_Target").objectReferenceValue = udon;
                            callelement.FindPropertyRelative("m_MethodName").stringValue = "SendCustomEvent";
                            callelement.FindPropertyRelative("m_Mode").enumValueIndex = (int)PersistentListenerMode.String;
                            callelement.FindPropertyRelative("m_Arguments.m_StringArgument").stringValue = "OnSliderChange";
                            callelement.FindPropertyRelative("m_CallState").enumValueIndex = (int)UnityEventCallState.RuntimeOnly;
                        }

                        so.ApplyModifiedProperties();
                    }
                }
            }
        }

        if (serializedObject.hasModifiedProperties)
            serializedObject.ApplyModifiedProperties();
    }
}
