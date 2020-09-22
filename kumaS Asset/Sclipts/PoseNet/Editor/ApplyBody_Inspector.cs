using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace kumaS.PoseNet
{
    [CustomEditor(typeof(ApplyBody))]
    public class ApplyBodyInspector : Editor
    {
        Dictionary<string, SerializedProperty> serialized = new Dictionary<string, SerializedProperty>();

        private bool f1 = false;

        private void OnEnable()
        {
            serialized.Add("Position", serializedObject.FindProperty("Position"));
            serialized.Add("Bip_C_Hips", serializedObject.FindProperty("Bip_C_Hips"));
            serialized.Add("Bip_C_Head", serializedObject.FindProperty("Bip_C_Head"));
            serialized.Add("Bip_R_UpperArm", serializedObject.FindProperty("Bip_R_UpperArm"));
            serialized.Add("Bip_R_LowerArm", serializedObject.FindProperty("Bip_R_LowerArm"));
            serialized.Add("Bip_L_UpperArm", serializedObject.FindProperty("Bip_L_UpperArm"));
            serialized.Add("Bip_L_LowerArm", serializedObject.FindProperty("Bip_L_LowerArm"));
            serialized.Add("Bip_R_UpperLeg", serializedObject.FindProperty("Bip_R_UpperLeg"));
            serialized.Add("Bip_R_LowerLeg", serializedObject.FindProperty("Bip_R_LowerLeg"));
            serialized.Add("Bip_L_UpperLeg", serializedObject.FindProperty("Bip_L_UpperLeg"));
            serialized.Add("Bip_L_LowerLeg", serializedObject.FindProperty("Bip_L_LowerLeg"));
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


            EditorGUILayout.LabelField("Transform reference setting", leftbold);
            EditorGUI.indentLevel++;
            f1 = EditorGUILayout.Foldout(f1, "content");
            if (f1)
            {
                EditorGUI.indentLevel++;
                serialized["Position"].objectReferenceValue = EditorGUILayout.ObjectField("Position",
                    serialized["Position"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_C_Hips"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_C_Hips",
                    serialized["Bip_C_Hips"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_C_Head"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_C_Head",
                    serialized["Bip_C_Head"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_R_UpperArm"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_R_UpperArm",
                    serialized["Bip_R_UpperArm"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_R_LowerArm"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_R_LowerArm",
                    serialized["Bip_R_LowerArm"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_L_UpperArm"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_L_UpperArm",
                    serialized["Bip_L_UpperArm"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_L_LowerArm"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_L_LowerArm",
                    serialized["Bip_L_LowerArm"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_R_UpperLeg"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_R_UpperLeg",
                    serialized["Bip_R_UpperLeg"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_R_LowerLeg"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_R_LowerLeg",
                    serialized["Bip_R_LowerLeg"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_L_UpperLeg"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_L_UpperLeg",
                    serialized["Bip_L_UpperLeg"].objectReferenceValue, typeof(Transform), true);
                serialized["Bip_L_LowerLeg"].objectReferenceValue = EditorGUILayout.ObjectField("Bip_L_LowerLeg",
                    serialized["Bip_L_LowerLeg"].objectReferenceValue, typeof(Transform), true);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
