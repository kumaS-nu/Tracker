using DlibDotNet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace kumaS.FaceTrack
{
    public partial class FaceTracking
    {
        ///<summary>初期設定後、非同期で顔検出を始める           Start face detection asynchronously after initialization</summary>
        async void Start()
        {
            if (caputure == null)
            {
                Debug.LogError("Video is null");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }

            //最初に設定しなければいけないもの      valiable that have to set first
            if (debug_eye_image)
            {
                thread = 2;
            }
            if (debug_face_image)
            {
#if VRM_EXIST
                if (gameObject.GetComponent<ApplyToVRM>() != null)
                {
                    gameObject.GetComponent<ApplyToVRM>().enabled = false;
                }
#endif
#if LIVE2D_EXIST
                if (gameObject.GetComponent<ApplyToLive2D>() != null)
                {
                    gameObject.GetComponent<ApplyToLive2D>().enabled = false;
                }
#endif
            }
            else
            {
#if VRM_EXIST
                if (gameObject.GetComponent<ApplyToVRM>() != null)
                {
                    gameObject.GetComponent<ApplyToVRM>().enabled = true;
                }
#endif
#if LIVE2D_EXIST
                if (gameObject.GetComponent<ApplyToLive2D>() != null)
                {
                    gameObject.GetComponent<ApplyToLive2D>().enabled = true;
                }
#endif
            }
            _mainContext = SynchronizationContext.Current;
            System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            Mat image_r = new Mat();
            await caputure.WaitOpen();
            image_r = caputure.Read();
            Mat image = new Mat();
            if (resolution == 1)
            {
                image = image_r.Clone();
            }
            else
            {
                Cv2.Resize(image_r, image, new Size(image_r.Cols / resolution, image_r.Rows / resolution));
            }
            //モードごとによって設定するもの   variable that set each mode
            switch (mode)
            {
                case DetectMode.OpenCV:
                    if (!File.Exists(cascade_file))
                    {
                        Debug.LogError("Path for cascade file is invalid");
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                        return;
                    }

                    cascade = new CascadeClassifier();
                    ptr = new IntPtr[thread - 1];
                    try
                    {
                        cascade.Load(cascade_file);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                        Quit();
                        while (true) { }
                    }
                    break;
                case DetectMode.Dlib5:
                    if (!File.Exists(shape_file_5))
                    {
                        Debug.LogError("Path for 5 face landmarks is invalid");
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                        return;
                    }
                    ptr = new IntPtr[thread - 1];
                    if (un_safe)
                    {
                        detector = new FrontalFaceDetector[1];
                        detector[0] = Dlib.GetFrontalFaceDetector();
                    }
                    else
                    {
                        detector = new FrontalFaceDetector[thread - 1];
                        for (int i = 0; i < thread - 1; i++)
                        {
                            detector[i] = Dlib.GetFrontalFaceDetector();
                        }
                    }
                    try
                    {
                        shape = ShapePredictor.Deserialize(shape_file_5);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                        Quit();
                        while (true) { }
                    }

                    break;

                case DetectMode.Dlib68:
                    if (!File.Exists(shape_file_68))
                    {
                        Debug.LogError("Path for 68 face landmarks is invalid");
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                        return;
                    }
                    if (un_safe)
                    {
                        detector = new FrontalFaceDetector[1];
                        detector[0] = Dlib.GetFrontalFaceDetector();
                    }
                    else
                    {
                        detector = new FrontalFaceDetector[thread - 1];
                        for (int i = 0; i < thread - 1; i++)
                        {
                            detector[i] = Dlib.GetFrontalFaceDetector();
                        }
                    }
                    landmark_detection = new DlibDotNet.Point[68];
                    landmarks = new Vector2[68];
                    ptr = new IntPtr[thread - 1];
                    proj = new double[thread - 1][];
                    pos_double = new double[thread - 1][];
                    eye_point_L = new DlibDotNet.Point[thread - 1][];
                    eye_ratio_L = new float[thread - 1];
                    eye_point_R = new DlibDotNet.Point[thread - 1][];
                    eye_ratio_R = new float[thread - 1];
                    try
                    {
                        shape = ShapePredictor.Deserialize(shape_file_68);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                        Quit();
                        while (true) { }
                    }
                    dist_coeffs_mat = new Mat(4, 1, MatType.CV_64FC1, 0);
                    var focal_length = image.Cols;
                    var center = new Point2d(image.Cols / 2, image.Rows / 2);
                    var camera_matrix = new double[3, 3] { { focal_length, 0, center.X }, { 0, focal_length, center.Y }, { 0, 0, 1 } };
                    camera_matrix_mat = new Mat(3, 3, MatType.CV_64FC1, camera_matrix);
                    SetmodelPoints();
                    for (int i = 0; i < thread - 1; i++)
                    {
                        proj[i] = new double[9];
                        pos_double[i] = new double[3];
                        eye_point_L[i] = new DlibDotNet.Point[6];
                        eye_point_R[i] = new DlibDotNet.Point[6];
                    }
                    break;
                case DetectMode.Mixed:
                    ptr = new IntPtr[thread - 1];
                    cascade = new CascadeClassifier();
                    try
                    {
                        cascade.Load(cascade_file);
                        shape = ShapePredictor.Deserialize(shape_file_68);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                        Quit();
                        while (true) { }
                    }
                    landmark_detection = new DlibDotNet.Point[68];
                    landmarks = new Vector2[68];
                    proj = new double[thread - 1][];
                    pos_double = new double[thread - 1][];
                    eye_point_L = new DlibDotNet.Point[thread - 1][];
                    eye_ratio_L = new float[thread - 1];
                    eye_point_R = new DlibDotNet.Point[thread - 1][];
                    eye_ratio_R = new float[thread - 1];

                    dist_coeffs_mat = new Mat(4, 1, MatType.CV_64FC1, 0);
                    var focal_length2 = image.Cols;
                    var center2 = new Point2d(image.Cols / 2, image.Rows / 2);
                    var camera_matrix2 = new double[3, 3] { { focal_length2, 0, center2.X }, { 0, focal_length2, center2.Y }, { 0, 0, 1 } };
                    camera_matrix_mat = new Mat(3, 3, MatType.CV_64FC1, camera_matrix2);
                    SetmodelPoints();
                    for (int i = 0; i < thread - 1; i++)
                    {
                        proj[i] = new double[9];
                        pos_double[i] = new double[3];
                        eye_point_L[i] = new DlibDotNet.Point[6];
                        eye_point_R[i] = new DlibDotNet.Point[6];
                    }
                    break;
            }
            //上記以外の設定       other setting
            ptr[0] = image_r.Data;
            if (logToFile)
            {
                if (!Directory.Exists(Application.dataPath + "/DebugData"))
                {
                    Directory.CreateDirectory(Application.dataPath + "/DebugData");
                }
                fps_writer = new StreamWriter(Application.dataPath + "/DebugData/F_FPS_LOG" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");
                pos_rot_writer = new StreamWriter(Application.dataPath + "/DebugData/F_POS_ROT_LOG" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");
                final_writer = new StreamWriter(Application.dataPath + "/DebugData/F_FINAL_LOG" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");
                fps_writer.WriteLine("FPS");
                pos_rot_writer.WriteLine("POS_X,POS_Y,POS_Z,ROT_X,ROT_Y,ROT_Z");
                final_writer.WriteLine("POS_X,POS_Y,POS_Z,ROT_X,ROT_Y,ROT_Z,EYE_CLOSE_L,EYE_CLOSE_R,EYE_ROT_L_X,EYE_ROT_L_Y,EYE_ROT_R_X,EYE_ROT_R_Y");
                if (mode == DetectMode.Dlib68 || mode == DetectMode.Mixed)
                {
                    if (eye_tracking)
                    {
                        eye_rot_writer = new StreamWriter(Application.dataPath + "/DebugData/F_EYE_ROT_LOG" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");
                        eye_rot_writer.WriteLine("EYE_ROT_L_X,EYE_ROT_L_Y,EYE_ROT_R_X,EYE_ROT_R_Y");
                    }

                    if (blink_tracking)
                    {
                        eye_ratio_writer = new StreamWriter(Application.dataPath + "/DebugData/F_EYE_RATIO_LOG" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");
                        eye_ratio_writer.WriteLine("EYE_RATIO_L,EYE_RATIO_R");
                    }
                }
            }

            pos = transform.position - pos_offset;
            rot = transform.eulerAngles;

            for (int i = 0; i < smooth; i++)
            {
                pos_chain.AddLast(pos);
                rot_chain.AddLast(rot);
            }

            for (int i = 0; i < 8; i++)
            {
                eye_L.AddLast(0.0f);
                eye_R.AddLast(0.0f);
                eye_rot_L.AddLast(Vector3.zero);
                eye_rot_R.AddLast(Vector3.zero);
            }
            if (debug_face_image)
            {
                out_mat = new Mat[thread - 1];
                out_texture2D = new Texture2D(image.Width, image.Height);
            }
            bytes = new byte[thread - 1][];
            lock_imagebytes = new object[thread - 1];
            lock_out_mat = new object[thread - 1];
            for (int i = 0; i < thread - 1; i++)
            {
                bytes[i] = new byte[image.Width * image.Height * image.ElemSize()];
                lock_imagebytes[i] = new object();
                lock_out_mat[i] = new object();
                if (debug_face_image)
                {
                    out_mat[i] = new Mat();
                }
            }
            if (image.IsEnabledDispose)
            {
                image.Dispose();
            }
            if (model == null)
            {
                model = transform;
            }

            //フェイストラッキング開始      start face tracking
            _ = Task.Run(DetectAsync);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 強制的に停止させる関数。ファイルを読み込めなかった時用。    Function that this process stop. For occer error in reading file.
        /// </summary>
        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 3Dモデルの点を設定
        /// </summary>
        private void SetmodelPoints()
        {
            var model_points = new Point3f[8];
            model_points[0] = new Point3f(0.0f, 0.03f, 0.11f);
            model_points[1] = new Point3f(0.0f, -0.06f, 0.08f);
            model_points[2] = new Point3f(-0.048f, 0.07f, 0.066f);
            model_points[3] = new Point3f(0.048f, 0.07f, 0.066f);
            model_points[4] = new Point3f(-0.03f, -0.007f, 0.088f);
            model_points[5] = new Point3f(0.03f, -0.007f, 0.088f);
            model_points[6] = new Point3f(-0.015f, 0.07f, 0.08f);
            model_points[7] = new Point3f(0.015f, 0.07f, 0.08f);

            model_points_mat = new Mat(model_points.Length, 1, MatType.CV_32FC3, model_points);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                inputR = true;
            }

            if (debug_face_image)
            {
                //デバック時、テクスチャも生成し、どこを検出しているか確認できる   During debugging, you can see where they are detected
                if (lock_out_mat != null)
                {
                    if (last_mat != -1)
                    {
                        lock (lock_out_mat[last_mat])
                        {
                            MatTexture.Mat2Texture(out_mat[last_mat], out_texture2D);
                        }

                        gameObject.GetComponent<Renderer>().material.mainTexture = out_texture2D;
                    }
                }

            }
            else
            {
                //位置・回転を反映          adapt position and rotation
                if (model != transform)
                {
                    Position = model.position + pos * position_scale + pos_offset;
                    Rotation = Quaternion.Euler(model.eulerAngles + rot * rotation_scale);
                }

                transform.position = Position;
                transform.rotation = Rotation;

            }
            try
            {
                if (mode == DetectMode.Dlib68 || mode == DetectMode.Mixed)
                {
                    if (blink_tracking)
                    {
                        lock (lock_eye_blink)
                        {
                            if (eye_L_c < 0)
                            {
                                eye_L_c = 0;
                            }
                            else if (eye_L_c > 1)
                            {
                                eye_L_c = 1;
                            }

                            if (eye_R_c < 0)
                            {
                                eye_R_c = 0;
                            }
                            else if (eye_R_c > 1)
                            {
                                eye_R_c = 1;
                            }

                            LeftEyeCloseness = eye_L_c;
                            RightEyeCloseness = eye_R_c;
                        }
                    }

                    if (eye_tracking)
                    {

                        lock (lock_eye_rotL)
                        {
                            LeftEyeRotation = lefteyerotation;
                        }

                        lock (lock_eye_rotR)
                        {
                            RightEyeRotation = righteyerotation;
                        }
                    }
                }
                lock (lock_fps)
                {
                    if (fin_time.Count == 8)
                    {
                        LinkedListNode<DateTime> node = fin_time.First;
                        for (int i = 0; i < 7; i++)
                        {
                            frame_time[i] = (int)(node.Next.Value - node.Value).TotalMilliseconds;
                            node = node.Next;
                        }
                        //非同期で走らせてるやつのずらす時間を生成  Clac time which make diff of async roop.
                        diff_time = (int)elapt_time.Average() / (thread - 1);
                        if (diff_time < 1000 / fps_limit)
                        {
                            diff_time = 1000 / fps_limit;
                        }
                    }
                }

                if (mode != DetectMode.OpenCV)
                {
                    if (mode == DetectMode.Dlib5)
                    {
                        lock (lock_landmarks)
                        {
                            Vector2[] parts = new Vector2[5];
                            for (uint i = 0; i < 5; i++)
                            {
                                parts[i].x = landmark_detection[i].X;
                                parts[i].y = landmark_detection[i].Y;
                            }

                            Landmarks = parts;
                        }
                    }
                    else
                    {
                        lock (lock_landmarks)
                        {
                            Vector2[] parts = new Vector2[68];
                            for (uint i = 0; i < 68; i++)
                            {
                                parts[i].x = landmark_detection[i].X;
                                parts[i].y = landmark_detection[i].Y;
                            }

                            Landmarks = parts;
                        }
                    }
                }

                if (fin_time.Count == 8)
                {
                    if (logToFile)
                    {
                        fps_writer.WriteLine(1000 / frame_time.Average());
                        final_writer.WriteLine(Position.x + "," + Position.y + "," + Position.z + "," + Rotation.x + "," + Rotation.y + "," + Rotation.z +
                       "," + LeftEyeCloseness + "," + RightEyeCloseness + "," + LeftEyeRotation.x + "," + LeftEyeRotation.y + "," + RightEyeRotation.x + "," + RightEyeRotation.y);
                    }
                    else
                    {
                        if (debug_fps)
                        {
                            Debug.Log("FPS = " + (1000 / frame_time.Average()) + " or " + (1000 / diff_time));
                        }
                    }
                }

            }
            catch (Exception) { }
        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 成功率をログに流す       write success rate in log
        /// </summary>
        private void OnDestroy()
        {
            Debug.Log("suc = " + suc + ",fail= " + fail + ": " + (float)suc / (suc + fail));
            running = false;
            if (logToFile)
            {
                fps_writer.Close();
                if (mode == DetectMode.Dlib68 || mode == DetectMode.Mixed)
                {
                    if (blink_tracking)
                    {
                        eye_ratio_writer.Close();
                    }
                    if (eye_tracking)
                    {
                        eye_rot_writer.Close();
                    }
                }
                final_writer.Close();
            }
        }
    }
}