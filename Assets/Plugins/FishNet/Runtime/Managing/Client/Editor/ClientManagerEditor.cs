#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FishNet.Managing.Client.Editing
{
    [CustomEditor(typeof(ClientManager), true)]
    [CanEditMultipleObjects]
    public class ClientManagerEditor : Editor
    {
        private SerializedProperty _remoteServerTimeout;
        private SerializedProperty _remoteServerTimeoutDuration;
        private SerializedProperty _changeFrameRate;
        private SerializedProperty _frameRate;

        protected virtual void OnEnable()
        {
            if (target == null) return;

            _remoteServerTimeout = serializedObject.FindProperty(nameof(_remoteServerTimeout));
            _remoteServerTimeoutDuration = serializedObject.FindProperty(nameof(_remoteServerTimeoutDuration));
            _changeFrameRate = serializedObject.FindProperty(nameof(_changeFrameRate));
            _frameRate = serializedObject.FindProperty(nameof(_frameRate));
        }

        public override void OnInspectorGUI()
        {
            if (target == null) return;

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((ClientManager)target), typeof(ClientManager), false);
            GUI.enabled = true;

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(_remoteServerTimeout);
            if ((RemoteTimeoutType)_remoteServerTimeout.intValue != RemoteTimeoutType.Disabled)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_remoteServerTimeoutDuration, new GUIContent("Timeout"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_changeFrameRate);
            if (_changeFrameRate.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_frameRate);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif