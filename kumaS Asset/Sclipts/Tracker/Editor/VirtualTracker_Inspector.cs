
#if uOSC_EXIST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using kumaS.PoseNet;
using kumaS.FaceTrack;
using System.Linq;

namespace kumaS.Tracker
{
    [CustomEditor(typeof(VirtualTracker))]
    public class VirtualTracker_Inspector : Editor
    {
        private Dictionary<string, SerializedProperty> serialized = new Dictionary<string, SerializedProperty>();
        private VirtualTracker tar = default;
        private readonly SendData[][] allowData = new SendData[][] { new SendData[] { SendData.Body }, new SendData[] { SendData.Body, SendData.Face, SendData.Both }, new SendData[] { SendData.Body, SendData.Face, SendData.Both } };

        private void OnEnable()
        {
            serialized.Add("to", serializedObject.FindProperty("to"));
            serialized.Add("data", serializedObject.FindProperty("data"));
            serialized.Add("body", serializedObject.FindProperty("body"));
            serialized.Add("face", serializedObject.FindProperty("face"));
            serialized.Add("model", serializedObject.FindProperty("model"));
            serialized.Add("offset", serializedObject.FindProperty("offset"));
            serialized.Add("offset_right_leg", serializedObject.FindProperty("offset_right_leg"));
            serialized.Add("offset_left_leg", serializedObject.FindProperty("offset_left_leg"));
            serialized.Add("offset_abdomen", serializedObject.FindProperty("offset_abdomen"));
            serialized.Add("offset_right_elbow", serializedObject.FindProperty("offset_right_elbow"));
            serialized.Add("offset_left_elbow", serializedObject.FindProperty("offset_left_elbow"));
            serialized.Add("offset_right_knee", serializedObject.FindProperty("offset_right_knee"));
            serialized.Add("offset_left_knee", serializedObject.FindProperty("offset_left_knee"));
            serialized.Add("offset_head", serializedObject.FindProperty("offset_head"));
            serialized.Add("offset_right_wrist", serializedObject.FindProperty("offset_right_wrist"));
            serialized.Add("offset_left_wrist", serializedObject.FindProperty("offset_left_wrist"));
            tar = target as VirtualTracker;
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

            EditorGUI.BeginChangeCheck();
            serialized["to"].enumValueIndex = EditorGUILayout.Popup("Where to send", serialized["to"].enumValueIndex, serialized["to"].enumDisplayNames);
            if (EditorGUI.EndChangeCheck())
            {
                BridgeVirtualTrackerAnduOCSClient.Apply(((VirtualTracker)target).gameObject, (SendTo)serialized["to"].enumValueIndex);
            }

            serialized["data"].enumValueIndex = EditorGUILayout.Popup("What data send", serialized["data"].enumValueIndex, serialized["data"].enumDisplayNames);

            if (!allowData[serialized["to"].enumValueIndex].Contains((SendData)serialized["data"].enumValueIndex))
            {
                EditorGUILayout.HelpBox("そのデータは送れません。\nYou can't send that data.", MessageType.Error);
            }

            if (serialized["to"].enumValueIndex == (int)SendTo.VMCProtocolToMarionette)
            {
                serialized["model"].objectReferenceValue = EditorGUILayout.ObjectField("Target model", serialized["model"].objectReferenceValue, typeof(GameObject), true);
                if (serialized["model"].objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Target model が null です。\nTarget model is null!", MessageType.Error);
                }
            }
            else
            {

                if (serialized["data"].enumValueIndex != (int)SendData.Face)
                {
                    serialized["body"].objectReferenceValue = EditorGUILayout.ObjectField("Body Tracking", serialized["body"].objectReferenceValue, typeof(BodyTracking), true);
                    if (serialized["body"].objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("Body Tracking が null です。\nBody Tracking is null!", MessageType.Error);
                    }
                }

                if (serialized["data"].enumValueIndex != (int)SendData.Body)
                {
                    serialized["face"].objectReferenceValue = EditorGUILayout.ObjectField("Face Tracking", serialized["face"].objectReferenceValue, typeof(FaceTracking), true);
                    if (serialized["face"].objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("Face Tracking が null です。\nFace Tracking is null!", MessageType.Error);
                    }
                }
            }

            if (serialized["to"].enumValueIndex != (int)SendTo.VMCProtocolToMarionette)
            {

                EditorGUILayout.Space();
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Offset settings", leftbold);
                EditorGUI.indentLevel++;
                serialized["offset"].vector3Value = EditorGUILayout.Vector3Field("Root offset", serialized["offset"].vector3Value);

                if (serialized["data"].enumValueIndex != (int)SendData.Face)
                {
                    serialized["offset_right_leg"].vector3Value = EditorGUILayout.Vector3Field("Right leg offset", serialized["offset_right_leg"].vector3Value);
                    serialized["offset_left_leg"].vector3Value = EditorGUILayout.Vector3Field("Left leg offset", serialized["offset_left_leg"].vector3Value);
                    serialized["offset_abdomen"].vector3Value = EditorGUILayout.Vector3Field("Abdomen offset", serialized["offset_abdomen"].vector3Value);
                    serialized["offset_right_elbow"].vector3Value = EditorGUILayout.Vector3Field("Right elbow offset", serialized["offset_right_elbow"].vector3Value);
                    serialized["offset_left_elbow"].vector3Value = EditorGUILayout.Vector3Field("Left elbow offset", serialized["offset_left_elbow"].vector3Value);
                    serialized["offset_right_knee"].vector3Value = EditorGUILayout.Vector3Field("Right knee offset", serialized["offset_right_knee"].vector3Value);
                    serialized["offset_left_knee"].vector3Value = EditorGUILayout.Vector3Field("Left knee offset", serialized["offset_left_knee"].vector3Value);
                    serialized["offset_head"].vector3Value = EditorGUILayout.Vector3Field("Head offset", serialized["offset_head"].vector3Value);
                    serialized["offset_right_wrist"].vector3Value = EditorGUILayout.Vector3Field("Right wrist offset", serialized["offset_right_wrist"].vector3Value);
                    serialized["offset_left_wrist"].vector3Value = EditorGUILayout.Vector3Field("Left wrist offset", serialized["offset_left_wrist"].vector3Value);
                }
                tar.SetOffsets();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif