using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace kumaS.FaceTrack
{

    [CustomEditor(typeof(FaceTracking))]
    public class FaceTrackingEditor : Editor
    {
        private bool debug_foldout = true;
        private bool parents_foldout = false;
        Dictionary<string, SerializedProperty> serialized = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            serialized.Add("caputure", serializedObject.FindProperty("caputure"));
            serialized.Add("mode", serializedObject.FindProperty("mode"));
            serialized.Add("logToFile", serializedObject.FindProperty("logToFile"));
            serialized.Add("blink_tracking", serializedObject.FindProperty("blink_tracking"));
            serialized.Add("eye_tracking", serializedObject.FindProperty("eye_tracking"));
            serialized.Add("debug_face_image", serializedObject.FindProperty("debug_face_image"));
            serialized.Add("debug_pos_rot", serializedObject.FindProperty("debug_pos_rot"));
            serialized.Add("debug_fps", serializedObject.FindProperty("debug_fps"));
            serialized.Add("debug_eye_closeness", serializedObject.FindProperty("debug_eye_closeness"));
            serialized.Add("debug_eye_open_ratio", serializedObject.FindProperty("debug_eye_open_ratio"));
            serialized.Add("debug_eye_center_ratio", serializedObject.FindProperty("debug_eye_center_ratio"));
            serialized.Add("debug_eye_image", serializedObject.FindProperty("debug_eye_image"));
            serialized.Add("ins", serializedObject.FindProperty("ins"));
            serialized.Add("p0", serializedObject.FindProperty("p0"));
            serialized.Add("p1", serializedObject.FindProperty("p1"));
            serialized.Add("p2", serializedObject.FindProperty("p2"));
            serialized.Add("p3", serializedObject.FindProperty("p3"));
            serialized.Add("cascade_file", serializedObject.FindProperty("cascade_file"));
            serialized.Add("shape_file_5", serializedObject.FindProperty("shape_file_5"));
            serialized.Add("shape_file_68", serializedObject.FindProperty("shape_file_68"));
            serialized.Add("model", serializedObject.FindProperty("model"));
            serialized.Add("unsafe", serializedObject.FindProperty("un_safe"));
            serialized.Add("fps_limit", serializedObject.FindProperty("fps_limit"));
            serialized.Add("resolution", serializedObject.FindProperty("resolution"));
            serialized.Add("thread", serializedObject.FindProperty("thread"));
            serialized.Add("smoothing", serializedObject.FindProperty("smoothing"));
            serialized.Add("smooth", serializedObject.FindProperty("smooth"));
            serialized.Add("alpha", serializedObject.FindProperty("alpha"));
            serialized.Add("position_verocity_ristrict", serializedObject.FindProperty("position_verocity_ristrict"));
            serialized.Add("rotation_verocity_ristrict", serializedObject.FindProperty("rotation_verocity_ristrict"));
            serialized.Add("radius", serializedObject.FindProperty("radius"));
            serialized.Add("rotation_range", serializedObject.FindProperty("rotation_range"));
            serialized.Add("pos_offset", serializedObject.FindProperty("pos_offset"));
            serialized.Add("rot_offset", serializedObject.FindProperty("rot_offset"));
            serialized.Add("center", serializedObject.FindProperty("center"));
            serialized.Add("position_scale", serializedObject.FindProperty("position_scale"));
            serialized.Add("rotation_scale", serializedObject.FindProperty("rotation_scale"));
            serialized.Add("z_scale", serializedObject.FindProperty("z_scale"));
            serialized.Add("z_offset", serializedObject.FindProperty("z_offset"));
            serialized.Add("eye_ratio_h", serializedObject.FindProperty("eye_ratio_h"));
            serialized.Add("eye_ratio_l", serializedObject.FindProperty("eye_ratio_l"));
            serialized.Add("eye_center", serializedObject.FindProperty("eye_center"));
            serialized.Add("left_eye_range_high", serializedObject.FindProperty("left_eye_range_high"));
            serialized.Add("left_eye_range_low", serializedObject.FindProperty("left_eye_range_low"));
            serialized.Add("right_eye_range_high", serializedObject.FindProperty("right_eye_range_high"));
            serialized.Add("right_eye_range_low", serializedObject.FindProperty("right_eye_range_low"));
            serialized.Add("eye_rot_offset_L", serializedObject.FindProperty("eye_rot_offset_L"));
            serialized.Add("eye_rot_offset_R", serializedObject.FindProperty("eye_rot_offset_R"));
            serialized.Add("eye_rot_sensitivity_L", serializedObject.FindProperty("eye_rot_sensitivity_L"));
            serialized.Add("eye_rot_sensitivity_R", serializedObject.FindProperty("eye_rot_sensitivity_R"));
        }

        private void OnValidate()
        {
            serializedObject.Update();

            if (serialized["fps_limit"].intValue < 30)
            {
                serialized["fps_limit"].intValue = 30;
            }

            if (serialized["thread"].intValue < 2)
            {
                serialized["thread"].intValue = 2;
            }
            else if (serialized["thread"].intValue > 32)
            {
                serialized["thread"].intValue = 32;
            }

            if (serialized["smooth"].intValue < 1)
            {
                serialized["smooth"].intValue = 1;
            }
            else if (serialized["smooth"].intValue > 32)
            {
                serialized["smooth"].intValue = 32;
            }

            if (serialized["alpha"].floatValue <= 0)
            {
                serialized["alpha"].floatValue = 0.001f;
            }
            else if (serialized["alpha"].floatValue >= 1.0f)
            {
                serialized["alpha"].floatValue = 0.999f;
            }

            if (serialized["eye_ratio_h"].floatValue < 0)
            {
                serialized["eye_ratio_h"].floatValue = 0;
            }
            else if (serialized["eye_ratio_h"].floatValue > 1)
            {
                serialized["eye_ratio_h"].floatValue = 1;
            }

            if (serialized["eye_ratio_l"].floatValue < 0)
            {
                serialized["eye_ratio_l"].floatValue = 0;
            }
            else if (serialized["eye_ratio_l"].floatValue > 1)
            {
                serialized["eye_ratio_l"].floatValue = 1;
            }

            if (serialized["eye_center"].floatValue < 0)
            {
                serialized["eye_center"].floatValue = 0;
            }
            else if (serialized["eye_center"].floatValue > 0)
            {
                serialized["eye_center"].floatValue = 1;
            }

            if (serialized["resolution"].intValue < 1)
            {
                serialized["resolution"].intValue = 1;
            }
            else if (serialized["resolution"].intValue > 10)
            {
                serialized["resolution"].intValue = 10;
            }

            serializedObject.ApplyModifiedProperties();
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
            EditorGUILayout.LabelField("Detecting mode settings", leftbold);
            EditorGUI.indentLevel++;
            serialized["mode"].enumValueIndex = EditorGUILayout.Popup(new GUIContent("Mode", "フェイストラッキングの顔を検知する方法\nThe way how detecting face"), serialized["mode"].enumValueIndex, serialized["mode"].enumDisplayNames);
            EditorGUI.indentLevel--;
            EditorGUILayout.HelpBox("OpenCV \t: やや精度悪い。最も軽量。\n\t   Lower accuracy and lightest.\n\n" +
                "Dlib5 \t: 精度良し。機能少なめ。\n\t   Better accuracy and a few functions.\n\n" +
                "Dlib68 \t: 精度良し。高機能。重い。\n\t   Better accuracy, many functions and heavy process.\n\n" +
                "Mixed \t: やや精度悪い。高機能。軽め。\n\t   Lower accuracy,  many functions and lighter.", MessageType.None);
            if (serialized["mode"].enumValueIndex == (int)DetectMode.Dlib68 || serialized["mode"].enumValueIndex == (int)DetectMode.Mixed)
            {
                EditorGUI.indentLevel++;
                serialized["blink_tracking"].boolValue = EditorGUILayout.Toggle(new GUIContent("Blink tracking", "目の開き具合を検知するか\nWhether detect how eye open"), serialized["blink_tracking"].boolValue);
                serialized["eye_tracking"].boolValue = EditorGUILayout.Toggle(new GUIContent("Eye tracking", "目の動きを検知するか\nWhether detect eye movements"), serialized["eye_tracking"].boolValue);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug settings", leftbold);
            EditorGUI.indentLevel++;
            serialized["logToFile"].boolValue = EditorGUILayout.Toggle(new GUIContent("Write log to file", "推定したログをcsvファイルに書き込むか\nWhether write estimated value log to csv file"), serialized["logToFile"].boolValue);
            if (!serialized["logToFile"].boolValue)
            {

                debug_foldout = EditorGUILayout.Foldout(debug_foldout, "Debug options");
                if (debug_foldout)
                {
                    EditorGUI.indentLevel++;
                    serialized["debug_face_image"].boolValue = EditorGUILayout.Toggle("Face image", serialized["debug_face_image"].boolValue);
                    if (serialized["debug_face_image"].boolValue)
                    {
                        EditorGUI.indentLevel--;
                        EditorGUI.indentLevel--;
                        EditorGUILayout.HelpBox("When Debug Face Image\n動きは反映されなくなります。\nThe movement will not be update.", MessageType.Warning);
                        EditorGUI.indentLevel++;
                        EditorGUI.indentLevel++;
                    }
                    serialized["debug_pos_rot"].boolValue = EditorGUILayout.Toggle("Position and Rotation", serialized["debug_pos_rot"].boolValue);
                    serialized["debug_fps"].boolValue = EditorGUILayout.Toggle("fps", serialized["debug_fps"].boolValue);

                    if (serialized["mode"].enumValueIndex == (int)DetectMode.Dlib68 || serialized["mode"].enumValueIndex == (int)DetectMode.Mixed)
                    {
                        serialized["debug_eye_closeness"].boolValue = EditorGUILayout.Toggle("Eye closeness value", serialized["debug_eye_closeness"].boolValue);
                        serialized["debug_eye_open_ratio"].boolValue = EditorGUILayout.Toggle("Eye openness ratio", serialized["debug_eye_open_ratio"].boolValue);
                        serialized["debug_eye_center_ratio"].boolValue = EditorGUILayout.Toggle("Eye center ratio", serialized["debug_eye_center_ratio"].boolValue);
                        EditorGUI.indentLevel--;
                        EditorGUI.indentLevel--;
                        EditorGUILayout.HelpBox("Eye closeness value \t: 0～1の目の閉じ具合。0が目があいている状態。\n\t\t   When value is 0, eye close.\n\n" +
                            "Eye openness ratio \t: 目の縦横比。\n\t\t   Eye aspect ratio.\n\n" +
                            "Eye center ratio \t: 黒目の中心が横軸ではどの位置にあるか。0が内側。\n\t\t   Where the center of the eye is on the horizontal axis. 0 is inner.", MessageType.None);
                        EditorGUI.indentLevel++;
                        EditorGUI.indentLevel++;
                        serialized["debug_eye_image"].boolValue = EditorGUILayout.Toggle("Eye image", serialized["debug_eye_image"].boolValue);
                        if (serialized["debug_eye_image"].boolValue)
                        {
                            EditorGUI.indentLevel--;
                            EditorGUI.indentLevel--;
                            EditorGUILayout.HelpBox("最初の1フレーム目のみの実行となります。\nOnly first frame will do.", MessageType.Warning);
                            EditorGUI.indentLevel++;
                            EditorGUI.indentLevel++;
                        }
                    }
                    EditorGUI.indentLevel--;
                }

                if (serialized["debug_eye_image"].boolValue)
                {
                    EditorGUI.indentLevel++;
                    serialized["ins"].objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Copy cube", "デフォルトのCubeをここに設定してください\nSet default Cube here"), serialized["ins"].objectReferenceValue, typeof(GameObject), true);
                    parents_foldout = EditorGUILayout.Foldout(parents_foldout, "Parents");
                    EditorGUI.indentLevel++;

                    if (parents_foldout)
                    {
                        serialized["p0"].objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Left color", serialized["p0"].objectReferenceValue, typeof(Transform), true);
                        serialized["p1"].objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Left gray", serialized["p1"].objectReferenceValue, typeof(Transform), true);
                        serialized["p2"].objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Right color", serialized["p2"].objectReferenceValue, typeof(Transform), true);
                        serialized["p3"].objectReferenceValue = (Transform)EditorGUILayout.ObjectField("Right gray", serialized["p3"].objectReferenceValue, typeof(Transform), true);

                    }
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                    EditorGUILayout.HelpBox("Copy cube\t: 絵を作るためにコピーされるCubeの元。\n\t  The original Cube copied to make a picture.\n\n" +
                        "Parents \t: 生成される絵の親となるTransform。空のGameObjectでいい。\n\t  Transform that is the parent of the generated picture. That should be an empty GameObject.", MessageType.None);
                    EditorGUI.indentLevel++;
                }

            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Reference settings", leftbold);
            EditorGUI.indentLevel++;
            switch (serialized["mode"].enumValueIndex)
            {
                case (int)DetectMode.OpenCV:
                    serialized["cascade_file"].stringValue = EditorGUILayout.TextField("Pass for cascade file", serialized["cascade_file"].stringValue); DAndD(serialized["cascade_file"]); break;
                case (int)DetectMode.Dlib5:
                    serialized["shape_file_5"].stringValue = EditorGUILayout.TextField("Pass for 5 face landmarks", serialized["shape_file_5"].stringValue); DAndD(serialized["shape_file_5"]); break;
                case (int)DetectMode.Dlib68:
                    serialized["shape_file_68"].stringValue = EditorGUILayout.TextField("Pass for 68 face landmarks", serialized["shape_file_68"].stringValue); DAndD(serialized["shape_file_68"]); break;
                case (int)DetectMode.Mixed:
                    serialized["cascade_file"].stringValue = EditorGUILayout.TextField("Pass for cascade file", serialized["cascade_file"].stringValue); DAndD(serialized["cascade_file"]);
                    serialized["shape_file_68"].stringValue = EditorGUILayout.TextField("Pass for 68 face landmarks", serialized["shape_file_68"].stringValue); DAndD(serialized["shape_file_68"]); break;
            }

            serialized["model"].objectReferenceValue = (Transform)EditorGUILayout.ObjectField(new GUIContent("Target model", "動かす対象の3Dモデル"), serialized["model"].objectReferenceValue, typeof(Transform), true);
            if (serialized["model"].objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Target model は null です。\nTarget model is null.", MessageType.Warning);
            }

            serialized["caputure"].objectReferenceValue = EditorGUILayout.ObjectField("Video",
                serialized["caputure"].objectReferenceValue, typeof(Video), true);
            if (serialized["caputure"].objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Video is null", MessageType.Error);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Performance settings", leftbold);

            EditorGUI.indentLevel++;
            serialized["unsafe"].boolValue = EditorGUILayout.Toggle("unsafe", serialized["unsafe"].boolValue);
            if (serialized["unsafe"].boolValue)
            {
                EditorGUILayout.HelpBox("高確率で立ち上がり時にAccess violationが起きて落ちますが、2倍以上のパフォーマンスがでる...はず...。\nAccess violation occurs at the time of start-up with high probability, but the performance is more than doubled. ...maybe", MessageType.Warning);
            }
            serialized["fps_limit"].intValue = EditorGUILayout.IntSlider("fps limit", serialized["fps_limit"].intValue, 30, 288);
            serialized["thread"].intValue = EditorGUILayout.IntSlider("Work threads", serialized["thread"].intValue, 2, 32);
            serialized["resolution"].intValue = EditorGUILayout.IntSlider(new GUIContent("Resolution", "画像サイズを何分の1にするか\nDecrease the image size"), serialized["resolution"].intValue, 1, 10);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Motion settings", leftbold);
            EditorGUI.indentLevel++;
            serialized["smoothing"].enumValueIndex = EditorGUILayout.Popup(new GUIContent("Smoothing method", "検出値を平滑化する方法\nMethod for smoothing detected values"), serialized["smoothing"].enumValueIndex, serialized["smoothing"].enumDisplayNames);
            if (serialized["smoothing"].enumValueIndex == (int)SmoothingMethod.Average)
            {
                serialized["smooth"].intValue = EditorGUILayout.IntSlider(new GUIContent("Smoothness of movement", "値が大きいほど滑らか\nThe higher the value, the smoother"), serialized["smooth"].intValue, 1, 32);
            }
            else
            {
                serialized["alpha"].floatValue = EditorGUILayout.Slider(new GUIContent("Smoothness of movement", "値が大きいほど滑らか\nThe higher the value, the smoother"), serialized["alpha"].floatValue, 0, 1);
            }
            EditorGUILayout.HelpBox("Smoothing method \t: 平滑化するための方法。\n\t\t   Method for smoothing\n\n" +
                "Average \t: 検出値を平均することで平滑化。\n\t  Smoothing by averaging detected values\n\n" +
                "LPF \t: ローパスフィルタを適応することで中・高周波成分を除去することで平滑化。\n\t  Smoothing by removing mid- and high-frequency components by applying a low-pass filter.", MessageType.None);
            serialized["position_verocity_ristrict"].floatValue = EditorGUILayout.FloatField("Position speed limit", serialized["position_verocity_ristrict"].floatValue);
            serialized["rotation_verocity_ristrict"].vector3Value = EditorGUILayout.Vector3Field("Rotation speed limit", serialized["rotation_verocity_ristrict"].vector3Value);
            serialized["radius"].floatValue = EditorGUILayout.FloatField("Position range (radius)", serialized["radius"].floatValue);
            serialized["rotation_range"].vector3Value = EditorGUILayout.Vector3Field("Rotation range", serialized["rotation_range"].vector3Value);
            serialized["pos_offset"].vector3Value = EditorGUILayout.Vector3Field("Position offset", serialized["pos_offset"].vector3Value);
            serialized["rot_offset"].vector3Value = EditorGUILayout.Vector3Field("Rotation offset", serialized["rot_offset"].vector3Value);
            EditorGUI.indentLevel--;
            EditorGUILayout.HelpBox("Position offset \t: モデルのPositionと頭のPositionの差を入力。\n\t\t   Enter the difference of positions between the model and the head\n\n" +
                "Rotation offset \t: モデルのRotationと頭のRotationの差を入力。\n\t\t   Enter the difference of rotations between the model and the head", MessageType.None);

            EditorGUILayout.Space();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Detecting parameter settings", leftbold);
            EditorGUI.indentLevel++;
            serialized["center"].vector3Value = EditorGUILayout.Vector3Field("Position center", serialized["center"].vector3Value);
            serialized["position_scale"].floatValue = EditorGUILayout.FloatField("Position scale", serialized["position_scale"].floatValue);
            serialized["rotation_scale"].floatValue = EditorGUILayout.FloatField("Rotation scale", serialized["rotation_scale"].floatValue);
            EditorGUI.indentLevel--;
            EditorGUILayout.HelpBox("これらは微調整に使ってください。大幅にずれる時はこの下Scale、z_offsetを調整してください。\nUse these for fine tuning. If there is a significant shift, adjust the Scale or z_offset.\n\n" +
                "Position center \t: 中心としたい位置にいる時に推定された位置を入力してください。\n\t\t   Enter the estimated position when you are at the center position.\n\n" +
                "Position scale \t: Positionが動きすぎる・動かなさすぎる時に調整してください。\n\t\t   Adjust when the position moves too much or less.\n\n" +
                "Rotation scale \t: Rotationが動きすぎる・動かなさすぎる時に調整してください。\n\t\t   Adjust when the rotation moves too much or less.", MessageType.None);
            if (serialized["mode"].enumValueIndex == (int)DetectMode.OpenCV || serialized["mode"].enumValueIndex == (int)DetectMode.Dlib5)
            {
                EditorGUI.indentLevel++;
                serialized["z_scale"].floatValue = EditorGUILayout.FloatField("Z scale", serialized["z_scale"].floatValue);
                serialized["z_offset"].floatValue = EditorGUILayout.FloatField("Z offset", serialized["z_offset"].floatValue);
                EditorGUI.indentLevel--;
                EditorGUILayout.HelpBox("z_scale \t: Z軸のPositionが動きすぎる・動かなさすぎる時に調整してください。\n\t   Adjust when the z axies position moves too much or less.\n\n" +
                    "z_offset\t: Positionのzを調整するときに使ってください。\n\t   Adjust for z of position", MessageType.None);
            }
            else
            {

                if (serialized["blink_tracking"].boolValue)
                {
                    EditorGUI.indentLevel++;
                    serialized["eye_ratio_h"].floatValue = EditorGUILayout.Slider("Eye ratio high", serialized["eye_ratio_h"].floatValue, 0, 1);
                    serialized["eye_ratio_l"].floatValue = EditorGUILayout.Slider("Eye ratio low", serialized["eye_ratio_l"].floatValue, 0, 1);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.HelpBox("Eye ratio high \t: 目を開けてる時のEye openness ratioを入力してください。\n\t\t   Enter the Eye openness ratio when your eyes are open.\n\n" +
                        "Eye ratio low \t: 目を閉じているときのEye openness ratioを入力してください。\n\t\t   Enter the Eye openness ratio when your eyes are closed.", MessageType.None);
                }

                if (serialized["eye_tracking"].boolValue)
                {
                    EditorGUI.indentLevel++;
                    serialized["eye_center"].floatValue = EditorGUILayout.Slider("Eye center", serialized["eye_center"].floatValue, 0, 1);
                    serialized["left_eye_range_high"].vector2Value = EditorGUILayout.Vector2Field("Left eye range high", serialized["left_eye_range_high"].vector2Value);
                    serialized["left_eye_range_low"].vector2Value = EditorGUILayout.Vector2Field("Left eye range low", serialized["left_eye_range_low"].vector2Value);
                    serialized["right_eye_range_high"].vector2Value = EditorGUILayout.Vector2Field("Right eye range high", serialized["right_eye_range_high"].vector2Value);
                    serialized["right_eye_range_low"].vector2Value = EditorGUILayout.Vector2Field("Right eye range low", serialized["right_eye_range_low"].vector2Value);
                    serialized["eye_rot_offset_L"].vector3Value = EditorGUILayout.Vector3Field("Left eye rotation offset", serialized["eye_rot_offset_L"].vector3Value);
                    serialized["eye_rot_offset_R"].vector3Value = EditorGUILayout.Vector3Field("Right eye rotation offset", serialized["eye_rot_offset_R"].vector3Value);
                    serialized["eye_rot_sensitivity_L"].vector2Value = EditorGUILayout.Vector2Field("Left eye rotation sensitivity", serialized["eye_rot_sensitivity_L"].vector2Value);
                    serialized["eye_rot_sensitivity_R"].vector2Value = EditorGUILayout.Vector2Field("Right eye rotation sensitivity", serialized["eye_rot_sensitivity_R"].vector2Value);

                    EditorGUI.indentLevel--;

                    if (serialized["left_eye_range_high"].vector2Value.x < serialized["left_eye_range_low"].vector2Value.x || serialized["left_eye_range_high"].vector2Value.y < serialized["left_eye_range_low"].vector2Value.y)
                    {
                        EditorGUILayout.HelpBox("Left eye range highまたはLeft eye range lowに誤りがあります。\nThere is an error in Left eye range high or Left eye range low.", MessageType.Error);
                    }
                    if (serialized["right_eye_range_high"].vector2Value.x < serialized["right_eye_range_low"].vector2Value.x || serialized["right_eye_range_high"].vector2Value.y < serialized["right_eye_range_low"].vector2Value.y)
                    {
                        EditorGUILayout.HelpBox("Right eye range highまたはRight eye range lowに誤りがあります。\nThere is an error in Right eye range high or Right eye range low.", MessageType.Error);
                    }

                    EditorGUILayout.HelpBox("Eye center \t: 黒目の中心はどこか。0が内側。\n\t\t   Where is eye center. 0 is inner", MessageType.None);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DAndD(SerializedProperty p)
        {
            var evt = Event.current;

            var dropArea = GUILayoutUtility.GetLastRect();
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

                        foreach (var path in DragAndDrop.paths)
                        {
                            p.stringValue = GetFullPath(path);
                        }
                        DragAndDrop.activeControlID = 0;
                    }
                    Event.current.Use();
                    break;
            }
        }

        private string GetFullPath(string path)
        {
            if (IsMyAssetsPath(path))
            {
                path = path.Substring("Assets/".Length);
                path = Application.dataPath + "/" + path;
            }
            else if (Path.GetFileName(path) == path)
            {
                path = Application.dataPath + "/../" + path;
                path = Path.GetFullPath(path);
            }
            return path;
        }

        private bool IsMyAssetsPath(string path)
        {
            if (path.IndexOf("Assets/") == 0)
            {
                return true;
            }
            return false;
        }
    }

}