using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UdonSharpEditor;

[CustomEditor(typeof(SyncTextManager))]
public class SyncTextManagerInspector : Editor
{
    SerializedProperty _targetTag;
    SerializedProperty _tmProTexts;
    SerializedProperty _shareText;

    private void OnEnable()
    {
        _targetTag = serializedObject.FindProperty("targetTag");
        _tmProTexts = serializedObject.FindProperty("tmProTexts");
        _shareText = serializedObject.FindProperty("shareText");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            return;

        var syncTextManager = target as SyncTextManager;

        EditorGUILayout.PropertyField(_targetTag);

        if (GUILayout.Button("Collect SyncText"))
        {
            Collect();
        }

        EditorGUILayout.LabelField("SyncText List : " + _tmProTexts.arraySize);
        using (new EditorGUI.DisabledScope(true))
        {
            for (var i = 0; i < _tmProTexts.arraySize; ++i)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(i.ToString() + ":", GUILayout.Width(20));
                    EditorGUILayout.ObjectField(_tmProTexts.GetArrayElementAtIndex(i).objectReferenceValue, typeof(TextMeshProUGUI), true);
                }
            }
        }

        EditorGUILayout.PropertyField(_shareText);

        if (serializedObject.hasModifiedProperties)
            serializedObject.ApplyModifiedProperties();
    }

    public void Collect()
    {
        var syncTextManager = target as SyncTextManager;

        List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();

        foreach (var info in StageUtility.GetCurrentStageHandle().FindComponentsOfType<SyncTextInfo>())
        {
            {
                if (info != null && info.targetTag.Equals(syncTextManager.targetTag))
                {
                    texts.Add(info.gameObject.GetComponent<TextMeshProUGUI>());
                }
            }
        }

        _tmProTexts.arraySize = 0;
        _tmProTexts.arraySize = texts.Count;

        for(int i = 0; i < _tmProTexts.arraySize; ++i)
        {
            using (var element = _tmProTexts.GetArrayElementAtIndex(i))
            {
                element.objectReferenceValue = texts[i];
            }
        }

        if (serializedObject.hasModifiedProperties)
            serializedObject.ApplyModifiedProperties();
    }
}
