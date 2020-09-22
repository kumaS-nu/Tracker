
#if LIVE2D_EXIST

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace kumaS.FaceTrack
{

    [CustomEditor(typeof(ApplyToLive2D))]
    public class ApplyToLive2D_Inspector_C : Editor
    {
        Dictionary<string, SerializedProperty> serialized = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            serialized.Add("model", serializedObject.FindProperty("model"));
            serialized.Add("ParamAngleX", serializedObject.FindProperty("ParamAngleX"));
            serialized.Add("ParamAngleY", serializedObject.FindProperty("ParamAngleY"));
            serialized.Add("ParamAngleZ", serializedObject.FindProperty("ParamAngleZ"));
            serialized.Add("ParamEyeLOpen", serializedObject.FindProperty("ParamEyeLOpen"));
            serialized.Add("ParamEyeROpen", serializedObject.FindProperty("ParamEyeROpen"));
            serialized.Add("ParamEyeBallX", serializedObject.FindProperty("ParamEyeBallX"));
            serialized.Add("ParamEyeBallY", serializedObject.FindProperty("ParamEyeBallY"));
            serialized.Add("ParamBodyAngleX", serializedObject.FindProperty("ParamBodyAngleX"));
            serialized.Add("ParamBodyAngleY", serializedObject.FindProperty("ParamBodyAngleY"));
            serialized.Add("ParamBodyAngleZ", serializedObject.FindProperty("ParamBodyAngleZ"));
        }

        public override void OnInspectorGUI()
        {
            GUIStyle leftbold = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
            };
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField("Target Live2D Model", leftbold);
            EditorGUILayout.ObjectField(serialized["model"]);
            EditorGUILayout.LabelField("Parameter Names", leftbold);
            NameFiled(serialized["ParamAngleX"], "ParamAngleX");
            NameFiled(serialized["ParamAngleY"], "ParamAngleY");
            NameFiled(serialized["ParamAngleZ"], "ParamAngelZ");
            NameFiled(serialized["ParamEyeLOpen"], "ParamEyeLOpen");
            NameFiled(serialized["ParamEyeROpen"], "ParamEyeROpen");
            NameFiled(serialized["ParamEyeBallX"], "ParamEyeBallX");
            NameFiled(serialized["ParamEyeBallY"], "ParamEyeBallY");
            NameFiled(serialized["ParamBodyAngleX"], "ParamBodyAngleX");
            NameFiled(serialized["ParamBodyAngleY"], "ParamBodyAngleY");
            NameFiled(serialized["ParamBodyAngleZ"], "ParamBodyAngleZ");
            EditorGUILayout.HelpBox("各パラメーターの名前を入力、またはD&Dしてください。ここに書いてあるものは標準のIDに準拠しています。\n" +
                "Enter or D & D the name of each parameter. Here is written in standard ID.", MessageType.None);
            serializedObject.ApplyModifiedProperties();
        }

        private void NameFiled(SerializedProperty p, string title)
        {
            p.stringValue = EditorGUILayout.TextField(title, p.stringValue);
            DAndD(p);
        }

        private void DAndD(SerializedProperty p)
        {
            var evt = Event.current;

            var dropArea = new Rect(GUILayoutUtility.GetLastRect().x, GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height * 0.1f, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height * 0.8f);
            int id = GUIUtility.GetControlID(FocusType.Passive);
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                    {
                        break;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.activeControlID = id;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            p.stringValue = obj.name;
                        }
                        DragAndDrop.activeControlID = 0;
                    }
                    Event.current.Use();
                    break;
            }
        }

    }
}

#endif