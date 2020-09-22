using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace kumaS.PoseNet
{

    [CustomEditor(typeof(BodyTracking))]
    public class BodyTrackingEditor : Editor
    {
        Dictionary<string, SerializedProperty> serialized = new Dictionary<string, SerializedProperty>();
        private bool f1 = false;
        private bool f2 = false;
        private bool f3 = false;

        private void OnEnable()
        {
            serialized.Add("capture", serializedObject.FindProperty("capture"));
            serialized.Add("thread", serializedObject.FindProperty("thread"));
            serialized.Add("maxfps", serializedObject.FindProperty("maxfps"));
            serialized.Add("smooth", serializedObject.FindProperty("smooth"));
            serialized.Add("tfile", serializedObject.FindProperty("tflie"));
            serialized.Add("z_offset", serializedObject.FindProperty("z_offset"));
            serialized.Add("camera_angle", serializedObject.FindProperty("camera_angle"));
            serialized.Add("shoulderline2nose", serializedObject.FindProperty("shoulderline2nose"));
            serialized.Add("nose2eye", serializedObject.FindProperty("nose2eye"));
            serialized.Add("nose2ear", serializedObject.FindProperty("nose2ear"));
            serialized.Add("shoulder2hip", serializedObject.FindProperty("shoulder2hip"));
            serialized.Add("hip2knee", serializedObject.FindProperty("hip2knee"));
            serialized.Add("knee2ankle", serializedObject.FindProperty("knee2ankle"));
            serialized.Add("shoulder2elbow", serializedObject.FindProperty("shoulder2elbow"));
            serialized.Add("elbow2wrist", serializedObject.FindProperty("elbow2wrist"));
            serialized.Add("nose", serializedObject.FindProperty("nose"));
            serialized.Add("leftEye", serializedObject.FindProperty("leftEye"));
            serialized.Add("rightEye", serializedObject.FindProperty("rightEye"));
            serialized.Add("leftEar", serializedObject.FindProperty("leftEar"));
            serialized.Add("rightEar", serializedObject.FindProperty("rightEar"));
            serialized.Add("leftShoulder", serializedObject.FindProperty("leftShoulder"));
            serialized.Add("rightShoulder", serializedObject.FindProperty("rightShoulder"));
            serialized.Add("leftElbow", serializedObject.FindProperty("leftElbow"));
            serialized.Add("rightElbow", serializedObject.FindProperty("rightElbow"));
            serialized.Add("leftWrist", serializedObject.FindProperty("leftWrist"));
            serialized.Add("rightWrist", serializedObject.FindProperty("rightWrist"));
            serialized.Add("leftHip", serializedObject.FindProperty("leftHip"));
            serialized.Add("rightHip", serializedObject.FindProperty("rightHip"));
            serialized.Add("leftKnee", serializedObject.FindProperty("leftKnee"));
            serialized.Add("rightKnee", serializedObject.FindProperty("rightKnee"));
            serialized.Add("leftAnkle", serializedObject.FindProperty("leftAnkle"));
            serialized.Add("rightAnkle", serializedObject.FindProperty("rightAnkle"));
            serialized.Add("nose_max_speed", serializedObject.FindProperty("nose_max_speed"));
            serialized.Add("eye_max_speed", serializedObject.FindProperty("eye_max_speed"));
            serialized.Add("ear_max_speed", serializedObject.FindProperty("ear_max_speed"));
            serialized.Add("shoulder_max_speed", serializedObject.FindProperty("shoulder_max_speed"));
            serialized.Add("elbow_max_speed", serializedObject.FindProperty("elbow_max_speed"));
            serialized.Add("wrist_max_speed", serializedObject.FindProperty("wrist_max_speed"));
            serialized.Add("hip_max_speed", serializedObject.FindProperty("hip_max_speed"));
            serialized.Add("knee_max_speed", serializedObject.FindProperty("knee_max_speed"));
            serialized.Add("ankle_max_speed", serializedObject.FindProperty("ankle_max_speed"));
            serialized.Add("debug_2d_pos", serializedObject.FindProperty("debug_2d_pos"));
            serialized.Add("debug_3d_pos", serializedObject.FindProperty("debug_3d_pos"));
            serialized.Add("debug_fps", serializedObject.FindProperty("debug_fps"));
            serialized.Add("debug_logfile", serializedObject.FindProperty("debug_logfile"));
        }

        private void OnValidate()
        {
            serializedObject.Update();
            if (serialized["maxfps"].intValue < 30)
            {
                serialized["maxfps"].intValue = 30;
            }

            if (serialized["thread"].intValue < 2)
            {
                serialized["thread"].intValue = 2;
            }

            if (serialized["z_offset"].floatValue < 0)
            {
                serialized["z_offset"].floatValue = 1;
            }

            if (serialized["camera_angle"].floatValue < 0)
            {
                serialized["camera_angle"].floatValue = 22;
            }

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

            EditorGUILayout.LabelField("Debug setting", leftbold);
            EditorGUI.indentLevel++;
            serialized["debug_logfile"].boolValue = EditorGUILayout.Toggle("Debug log to file", serialized["debug_logfile"].boolValue);
            if (!serialized["debug_logfile"].boolValue)
            {
                serialized["debug_2d_pos"].boolValue =
                    EditorGUILayout.Toggle("Debug 2D position", serialized["debug_2d_pos"].boolValue);
                serialized["debug_3d_pos"].boolValue =
                    EditorGUILayout.Toggle("Debug 3D position", serialized["debug_3d_pos"].boolValue);
                serialized["debug_fps"].boolValue =
                    EditorGUILayout.Toggle("Debug fps", serialized["debug_fps"].boolValue);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Performance setting", leftbold);
            EditorGUI.indentLevel++;
            serialized["maxfps"].intValue = EditorGUILayout.IntSlider("Max FPS", serialized["maxfps"].intValue, 30, 144);
            serialized["thread"].intValue = EditorGUILayout.IntSlider("Thread", serialized["thread"].intValue, 2, 32);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Reference setting", leftbold);
            EditorGUI.indentLevel++;
            serialized["tfile"].stringValue =
                EditorGUILayout.TextField("posenet.tflite", serialized["tfile"].stringValue);
            serialized["capture"].objectReferenceValue =
                EditorGUILayout.ObjectField("Video", serialized["capture"].objectReferenceValue, typeof(Video), true);
            if (serialized["capture"].objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Video is null", MessageType.Error);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Distance setting", leftbold);
            EditorGUI.indentLevel++;
            f1 = EditorGUILayout.Foldout(f1, "content");
            if (f1)
            {
                EditorGUI.indentLevel++;
                serialized["shoulderline2nose"].floatValue = EditorGUILayout.FloatField(
                    new GUIContent("Shoulder line to nose", "両肩の中心から鼻までの距離"),
                    serialized["shoulderline2nose"].floatValue);
                serialized["nose2eye"].floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Nose to eye", "鼻から目までの距離"),
                        serialized["nose2eye"].floatValue);
                serialized["nose2ear"].floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Nose to ear", "鼻から耳までの距離"),
                        serialized["nose2ear"].floatValue);
                serialized["shoulder2elbow"].floatValue = EditorGUILayout.FloatField(
                    new GUIContent("Shoulder to elbow", "肩から肘までの距離"), serialized["shoulder2elbow"].floatValue);
                serialized["elbow2wrist"].floatValue = EditorGUILayout.FloatField(
                    new GUIContent("Elbow to wrist", "肘から手首までの距離"), serialized["elbow2wrist"].floatValue);
                serialized["shoulder2hip"].floatValue = EditorGUILayout.FloatField(
                    new GUIContent("Shoulder to hip", "肩から股関節までの距離"), serialized["shoulder2hip"].floatValue);
                serialized["hip2knee"].floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Hip to knee", "股関節から膝までの距離"),
                        serialized["hip2knee"].floatValue);
                serialized["knee2ankle"].floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Knee to ankle", "膝から足首までの距離"),
                        serialized["knee2ankle"].floatValue);
                EditorGUILayout.HelpBox(
                    "体の各部位間の距離。単位はメートル。\t\tThe distance between parts of the body. Units are meters.",
                    MessageType.None);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();


            EditorGUILayout.LabelField("Initial position", leftbold);
            EditorGUI.indentLevel++;
            f2 = EditorGUILayout.Foldout(f2, "content");
            if (f2)
            {
                EditorGUI.indentLevel++;
                serialized["nose"].vector3Value = EditorGUILayout.Vector3Field(new GUIContent("Nose", "鼻"),
                    serialized["nose"].vector3Value);
                serialized["leftEye"].vector3Value =
                    EditorGUILayout.Vector3Field(new GUIContent("Left eye", "左目"),
                        serialized["leftEye"].vector3Value);
                serialized["rightEye"].vector3Value =
                    EditorGUILayout.Vector3Field(new GUIContent("Right eye", "右目"),
                        serialized["rightEye"].vector3Value);
                serialized["leftEar"].vector3Value =
                    EditorGUILayout.Vector3Field(new GUIContent("Left ear", "左耳"),
                        serialized["leftEar"].vector3Value);
                serialized["rightEar"].vector3Value = EditorGUILayout.Vector3Field(
                    new GUIContent("Right ear", "右耳"),
                    serialized["rightEar"].vector3Value);
                serialized["leftShoulder"].vector3Value =
                    EditorGUILayout.Vector3Field(new GUIContent("Left shoulder", "左肩"),
                        serialized["leftShoulder"].vector3Value);
                serialized["rightShoulder"].vector3Value =
                    EditorGUILayout.Vector3Field(new GUIContent("Right shoulder", "右肩"),
                        serialized["rightShoulder"].vector3Value);
                serialized["leftElbow"].vector3Value = EditorGUILayout.Vector3Field(
                    new GUIContent("Left elbow", "左肘"),
                    serialized["leftElbow"].vector3Value);
                serialized["rightElbow"].vector3Value =
                    EditorGUILayout.Vector3Field(new GUIContent("Right elbow", "右肘"),
                        serialized["rightElbow"].vector3Value);
                serialized["leftWrist"].vector3Value = EditorGUILayout.Vector3Field(
                    new GUIContent("Left wrist", "左手首"),
                    serialized["leftWrist"].vector3Value);
                serialized["rightWrist"].vector3Value =
                    EditorGUILayout.Vector3Field(new GUIContent("Right wrist", "右手首"),
                        serialized["rightWrist"].vector3Value);
                serialized["leftHip"].vector3Value = EditorGUILayout.Vector3Field(
                    new GUIContent("Left hip", "左股関節"),
                    serialized["leftHip"].vector3Value);
                serialized["rightHip"].vector3Value =
                    EditorGUILayout.Vector3Field(new GUIContent("Right hip", "右股関節"),
                        serialized["rightHip"].vector3Value);
                serialized["leftKnee"].vector3Value = EditorGUILayout.Vector3Field(
                    new GUIContent("Left knee", "左膝"),
                    serialized["leftKnee"].vector3Value);
                serialized["rightKnee"].vector3Value = EditorGUILayout.Vector3Field(
                    new GUIContent("Right knee", "右膝"),
                    serialized["rightKnee"].vector3Value);
                serialized["leftAnkle"].vector3Value = EditorGUILayout.Vector3Field(
                    new GUIContent("Left ankle", "左足首"),
                    serialized["leftAnkle"].vector3Value);
                serialized["rightAnkle"].vector3Value =
                    EditorGUILayout.Vector3Field(new GUIContent("Right ankle", "右足首"),
                        serialized["rightAnkle"].vector3Value);
                EditorGUILayout.HelpBox("体の各部位の初期位置。\t\tThe initial position of each part of the body.",
                    MessageType.None);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Max speed setting", leftbold);
            EditorGUI.indentLevel++;
            f3 = EditorGUILayout.Foldout(f3, "content");
            if (f3)
            {
                EditorGUI.indentLevel++;
                serialized["nose_max_speed"].floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Nose max speed", "鼻の最高速"),
                        serialized["nose_max_speed"].floatValue);
                serialized["eye_max_speed"].floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Eye max speed", "目の最高速"),
                        serialized["eye_max_speed"].floatValue);
                serialized["ear_max_speed"].floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Ear max speed", "耳の最高速"),
                        serialized["ear_max_speed"].floatValue);
                serialized["shoulder_max_speed"].floatValue = EditorGUILayout.FloatField(
                    new GUIContent("Shoulder max speed", "肩の最高速"), serialized["shoulder_max_speed"].floatValue);
                serialized["hip_max_speed"].floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Hip max speed", "股関節の最高速"),
                        serialized["hip_max_speed"].floatValue);
                serialized["knee_max_speed"].floatValue =
                    EditorGUILayout.FloatField(new GUIContent("Knee max speed", "膝の最高速"),
                        serialized["knee_max_speed"].floatValue);
                serialized["ankle_max_speed"].floatValue = EditorGUILayout.FloatField(
                    new GUIContent("Ankle max speed", "足首の最高速"), serialized["ankle_max_speed"].floatValue);
                EditorGUILayout.HelpBox(
                    "体の各部位の制限速度。一フレーム当たり。\t\tThe speed limit for each part of the body. per frame.",
                    MessageType.None);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Other setting", leftbold);
            EditorGUI.indentLevel++;
            serialized["smooth"].intValue =
                EditorGUILayout.IntSlider(new GUIContent("Smooth", "数値が大きいほどスムーズだが、動きの反映が遅い。"),
                    serialized["smooth"].intValue, 1, 64);
            serialized["z_offset"].floatValue = EditorGUILayout.FloatField(new GUIContent("Z offset", "カメラと原点との距離"),
                serialized["z_offset"].floatValue);
            serialized["camera_angle"].floatValue =
                EditorGUILayout.FloatField(new GUIContent("Camera angle", "カメラの縦の片側の視野角"),
                    serialized["camera_angle"].floatValue);
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }

}