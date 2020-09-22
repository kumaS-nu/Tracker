using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace kumaS
{
    [CustomEditor(typeof(Video))]
    public class Video_Inspector : Editor
    {
        Dictionary<string, SerializedProperty> serialized = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            serialized.Add("useUnity", serializedObject.FindProperty("useUnity"));
            serialized.Add("isFile", serializedObject.FindProperty("isFile"));
            serialized.Add("sourse", serializedObject.FindProperty("sourse"));
            serialized.Add("filename", serializedObject.FindProperty("filename"));
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();
            GUIStyle leftbold = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
            };
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            EditorGUI.indentLevel++;
            serialized["useUnity"].boolValue =
                EditorGUILayout.Toggle("Use Unity camera", serialized["useUnity"].boolValue);
            serialized["isFile"].boolValue = EditorGUILayout.Toggle("is file", serialized["isFile"].boolValue);
            if (serialized["isFile"].boolValue)
            {
                serialized["filename"].stringValue =
                    EditorGUILayout.TextField("File name", serialized["filename"].stringValue);
            }
            else
            {

                if (serialized["useUnity"].boolValue)
                {
                    serialized["sourse"].intValue = EditorGUILayout.Popup("Camera name",
                        serialized["sourse"].intValue,
                        WebCamTexture.devices.Select(value => value.name).ToArray());
                }
                else
                {
                    serialized["sourse"].intValue =
                        EditorGUILayout.IntField("Source No.", serialized["sourse"].intValue);
                }
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}
