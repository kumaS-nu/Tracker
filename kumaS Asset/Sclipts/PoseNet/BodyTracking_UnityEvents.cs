using UnityEngine;
using System.Threading.Tasks;
using kumaS.Extention;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace kumaS.PoseNet {

    public partial class BodyTracking {
        async void Start()
        {
            if (capture == null)
            {
                Debug.LogError("Video is null");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }

            if (!File.Exists(tflie)  || !tflie.Contains(".tflite"))
            {
                Debug.LogError("PoseNet tflite is invalid");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }

            stopwatches = new System.Diagnostics.Stopwatch[thread - 1];
            if (debug_logfile)
            {
                debug_fps = false;
                debug_2d_pos = false;
                debug_3d_pos = false;
                if (!Directory.Exists(Application.dataPath + "/DebugData"))
                {
                    Directory.CreateDirectory(Application.dataPath + "/DebugData");
                }
                fps_writer = new StreamWriter(Application.dataPath + "/DebugData/B_FPS_LOG" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");
                pos_writer = new StreamWriter(Application.dataPath + "/DebugData/B_POS_LOG" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");
                rot_writer = new StreamWriter(Application.dataPath + "/DebugData/B_ROT_LOG" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv");

                fps_writer.WriteLine("FPS");
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < 17; i++)
                {
                    sb.Append("2D[");
                    sb.Append(i);
                    sb.Append("].x,2D[");
                    sb.Append(i);
                    sb.Append("].y,");
                }

                for(int i = 0; i < 17; i++)
                {
                    sb.Append("3D[");
                    sb.Append(i);
                    sb.Append("].x,3D[");
                    sb.Append(i);
                    sb.Append("].y,3D[");
                    sb.Append(i);
                    sb.Append("].z,");
                }
                pos_writer.WriteLine(sb.ToString());

                StringBuilder sb2 = new StringBuilder();

                for(int i = 0; i < 10; i++)
                {
                    sb2.Append("rot[");
                    sb2.Append(i);
                    sb2.Append("].x,rot[");
                    sb2.Append(i);
                    sb2.Append("].y,rot[");
                    sb2.Append(i);
                    sb2.Append("].z,");
                }
                rot_writer.WriteLine(sb2.ToString());
            }
            for (int i = 1; i < thread; i++)
            {
                posenet.Add(new PoseNet(tflie, 1));
                stopwatches[i - 1] = new System.Diagnostics.Stopwatch();
            }
            for (int i = 0; i < smooth; i++)
            {
                pose2d_chain.AddLast(new Vector2[17]);
                pose3d_chain.AddLast(new[]
                {nose, leftEye, rightEye, leftEar, rightEar, leftShoulder, rightShoulder, leftElbow, rightElbow, leftWrist, rightWrist,
                    leftHip, rightHip, leftKnee, rightKnee, leftAnkle, rightAnkle });
                elapt_time.AddLast(1000 / maxfps / (thread - 1));
            }

            for(int i = 0; i < 10; i++)
            {
                Rotation[i] = Quaternion.identity;
            }

            SetMaxSpeeds();

            if (debug_2d_pos)
            {
                debug_2d_lines = new LineRenderer[17];
                for (int i = 0; i < 17; i++)
                {
                    debug_2d_lines[i] = new GameObject().gameObject.AddComponent<LineRenderer>();
                    debug_2d_lines[i].startColor = Color.HSVToRGB(1.0f / 17 * i, 1, 1);
                    debug_2d_lines[i].endColor = Color.HSVToRGB(1.0f / 17 * i, 1, 1);
                    debug_2d_lines[i].startWidth = 0.01f;
                    debug_2d_lines[i].endWidth = 0.01f;
                    debug_2d_lines[i].material = new Material(Shader.Find("Standard"));
                }
            }

            if (debug_3d_pos)
            {
                debug_3d_lines = new LineRenderer[17];
                for (int i = 0; i < 17; i++)
                {
                    debug_3d_lines[i] = new GameObject().AddComponent<LineRenderer>();
                    debug_3d_lines[i].startColor = Color.HSVToRGB(1.0f / 17 * i, 1, 1);
                    debug_3d_lines[i].endColor = Color.HSVToRGB(1.0f / 17 * i, 1, 1);
                    debug_3d_lines[i].startWidth = 0.01f;
                    debug_3d_lines[i].endWidth = 0.01f;
                    Material m = new Material(Shader.Find("Standard"));
                    m.color = Color.HSVToRGB(1.0f / 17 * i, 1, 1);
                    debug_3d_lines[i].material = m;
                }
            }
            await capture.WaitOpen();
            running = true;

            _ = Task.Run(RunAsync);
        }

        void Update()
        {
            if (running)
            {
                lock (elapt_time)
                {
                    diff_time = (int)(elapt_time.Average() / (thread - 1));
                }
                if (diff_time < 1000 / maxfps)
                {
                    diff_time = 1000 / maxfps;
                }

                lock (fin_time)
                {
                    if (fin_time.Count == 8)
                    {
                        LinkedListNode<DateTime> node = fin_time.First;
                        for (int i = 0; i < 7; i++)
                        {
                            frame_time[i] = (int) (node.Next.Value - node.Value).TotalMilliseconds;
                            node = node.Next;
                        }
                    }
                }

                if (debug_fps)
                {
                    Debug.Log("fps=" + frame_time.Average() + " or " + (1000 / diff_time));
                }
                
                lock (pose2d_chain)
                {
                    Pose2d = pose2d_chain.Average();
                }

                lock (pose3d_chain)
                {
                    Pose3d = pose3d_chain.Average();
                }

                SetRotation();
                Position = (Pose3d[11] + Pose3d[12]) / 2;

                if (debug_logfile)
                {
                    fps_writer.WriteLine(frame_time.Average());
                    StringBuilder sb = new StringBuilder();
                    for(int i = 0; i < 10; i++)
                    {
                        Vector3 rot = Rotation[i].eulerAngles;
                        sb.Append(rot.x);
                        sb.Append(",");
                        sb.Append(rot.y);
                        sb.Append(",");
                        sb.Append(rot.z);
                        sb.Append(",");
                    }
                    rot_writer.WriteLine(sb.ToString());
                }

                if (debug_2d_pos)
                {
                    if (debug_2d_lines[0] != null)
                    {
                        Debug2d();
                    }
                }

                if (debug_3d_pos)
                {
                    if (debug_3d_lines[0] != null)
                    {
                        Debug3d();
                    }
                }
            }

        }

        /// <summary>
        /// 各部位の制限速度を設定する。      Set the speed limit for each part.
        /// </summary>
        private void SetMaxSpeeds()
        {
            max_speeds[0] = nose_max_speed * nose_max_speed;
            max_speeds[1] = eye_max_speed * eye_max_speed;
            max_speeds[2] = eye_max_speed * eye_max_speed;
            max_speeds[3] = ear_max_speed * ear_max_speed;
            max_speeds[4] = ear_max_speed * ear_max_speed;
            max_speeds[5] = shoulder_max_speed * shoulder_max_speed;
            max_speeds[6] = shoulder_max_speed * shoulder_max_speed;
            max_speeds[7] = elbow_max_speed * elbow_max_speed;
            max_speeds[8] = elbow_max_speed * elbow_max_speed;
            max_speeds[9] = wrist_max_speed * wrist_max_speed;
            max_speeds[10] = wrist_max_speed * wrist_max_speed;
            max_speeds[11] = hip_max_speed * hip_max_speed;
            max_speeds[12] = hip_max_speed * hip_max_speed;
            max_speeds[13] = knee_max_speed * knee_max_speed;
            max_speeds[14] = knee_max_speed * knee_max_speed;
            max_speeds[15] = ankle_max_speed * ankle_max_speed;
            max_speeds[16] = ankle_max_speed * ankle_max_speed;
        }

        /// <summary>
        /// 推定された３次元座標の体から各ボーンの回転を計算。       Calculate the rotation of each bone from the estimated 3d body of coordinates.
        /// </summary>
        private void SetRotation()
        {
            Vector3 tmp = default;
            Quaternion[] rot = new Quaternion[10];
            rot[0] = Quaternion.FromToRotation(Vector3.right, Pose3d[5] + Pose3d[11] - Pose3d[6] - Pose3d[12]);
            rot[1] = Quaternion.LookRotation(Pose3d[3] - Pose3d[4], 2 * Pose3d[0] - Pose3d[3] - Pose3d[4]) * Quaternion.Inverse(Quaternion.Euler(0,90,90));
            tmp = Vector3.Cross(Pose3d[5] - Pose3d[7], Pose3d[9] - Pose3d[7]);
            rot[2] = Quaternion.LookRotation(Pose3d[7] - Pose3d[5], tmp) * Quaternion.Inverse(Quaternion.Euler(0, 90, 0));
            rot[4] = Quaternion.LookRotation(Pose3d[9] - Pose3d[7], tmp) * Quaternion.Inverse(Quaternion.Euler(0, 90, 0));
            tmp = Vector3.Cross(Pose3d[10] - Pose3d[8], Pose3d[6] - Pose3d[8]);
            rot[3] = Quaternion.LookRotation(Pose3d[8] - Pose3d[6], tmp) * Quaternion.Inverse(Quaternion.Euler(0, -90, 0));
            rot[5] = Quaternion.LookRotation(Pose3d[10] - Pose3d[8], tmp) * Quaternion.Inverse(Quaternion.Euler(0, -90, 0));
            tmp = Vector3.Cross(Pose3d[15] - Pose3d[13], Pose3d[11] - Pose3d[13]);
            rot[6] = Quaternion.LookRotation(Pose3d[13] - Pose3d[11], tmp) * Quaternion.Inverse(Quaternion.Euler(90, 90, 0));
            rot[8] = Quaternion.LookRotation(Pose3d[15] - Pose3d[13], tmp) * Quaternion.Inverse(Quaternion.Euler(90, 90, 0));
            tmp = Vector3.Cross(Pose3d[12] - Pose3d[14], Pose3d[16] - Pose3d[14]);
            rot[7] = Quaternion.LookRotation(Pose3d[14] - Pose3d[12], tmp) * Quaternion.Inverse(Quaternion.Euler(90, -90, 0));
            rot[9] = Quaternion.LookRotation(Pose3d[16] - Pose3d[14], tmp) * Quaternion.Inverse(Quaternion.Euler(90, -90, 0));

            Rotation = rot;
        }

        /// <summary>
        /// 2dのデバッグ時の線を更新。          Updated the lines when debugging 2D.
        /// </summary>
        private void Debug2d()
        {
            debug_2d_lines[0].SetPosition(0, Vector2.one - Pose2d[0]);
            debug_2d_lines[0].SetPosition(1, Vector2.one - Pose2d[1]);
            debug_2d_lines[1].SetPosition(0, Vector2.one - Pose2d[0]);
            debug_2d_lines[1].SetPosition(1, Vector2.one - Pose2d[2]);
            debug_2d_lines[2].SetPosition(0, Vector2.one - Pose2d[1]);
            debug_2d_lines[2].SetPosition(1, Vector2.one - Pose2d[3]);
            debug_2d_lines[3].SetPosition(0, Vector2.one - Pose2d[2]);
            debug_2d_lines[3].SetPosition(1, Vector2.one - Pose2d[4]);
            debug_2d_lines[4].SetPosition(0, Vector2.one - Pose2d[0]);
            debug_2d_lines[4].SetPosition(1, Vector2.one - (Pose2d[5] + Pose2d[6]) / 2);
            debug_2d_lines[5].SetPosition(0, Vector2.one - Pose2d[5]);
            debug_2d_lines[5].SetPosition(1, Vector2.one - Pose2d[6]);
            debug_2d_lines[6].SetPosition(0, Vector2.one - Pose2d[5]);
            debug_2d_lines[6].SetPosition(1, Vector2.one - Pose2d[11]);
            debug_2d_lines[7].SetPosition(0, Vector2.one - Pose2d[6]);
            debug_2d_lines[7].SetPosition(1, Vector2.one - Pose2d[12]);
            debug_2d_lines[8].SetPosition(0, Vector2.one - Pose2d[11]);
            debug_2d_lines[8].SetPosition(1, Vector2.one - Pose2d[12]);
            debug_2d_lines[9].SetPosition(0, Vector2.one - Pose2d[5]);
            debug_2d_lines[9].SetPosition(1, Vector2.one - Pose2d[7]);
            debug_2d_lines[10].SetPosition(0, Vector2.one - Pose2d[6]);
            debug_2d_lines[10].SetPosition(1, Vector2.one - Pose2d[8]);
            debug_2d_lines[11].SetPosition(0, Vector2.one - Pose2d[7]);
            debug_2d_lines[11].SetPosition(1, Vector2.one - Pose2d[9]);
            debug_2d_lines[12].SetPosition(0, Vector2.one - Pose2d[8]);
            debug_2d_lines[12].SetPosition(1, Vector2.one - Pose2d[10]);
            debug_2d_lines[13].SetPosition(0, Vector2.one - Pose2d[11]);
            debug_2d_lines[13].SetPosition(1, Vector2.one - Pose2d[13]);
            debug_2d_lines[14].SetPosition(0, Vector2.one - Pose2d[12]);
            debug_2d_lines[14].SetPosition(1, Vector2.one - Pose2d[14]);
            debug_2d_lines[15].SetPosition(0, Vector2.one - Pose2d[13]);
            debug_2d_lines[15].SetPosition(1, Vector2.one - Pose2d[15]);
            debug_2d_lines[16].SetPosition(0, Vector2.one - Pose2d[14]);
            debug_2d_lines[16].SetPosition(1, Vector2.one - Pose2d[16]);

        }

        /// <summary>
        /// 3dのデバッグ時の線を更新。          Updated the lines when debugging 3D.
        /// </summary>
        private void Debug3d()
        {
            debug_3d_lines[0].SetPosition(0, Pose3d[0]);
            debug_3d_lines[0].SetPosition(1, Pose3d[1]);
            debug_3d_lines[1].SetPosition(0, Pose3d[0]);
            debug_3d_lines[1].SetPosition(1, Pose3d[2]);
            debug_3d_lines[2].SetPosition(0, Pose3d[1]);
            debug_3d_lines[2].SetPosition(1, Pose3d[3]);
            debug_3d_lines[3].SetPosition(0, Pose3d[2]);
            debug_3d_lines[3].SetPosition(1, Pose3d[4]);
            debug_3d_lines[4].SetPosition(0, Pose3d[0]);
            debug_3d_lines[4].SetPosition(1, (Pose3d[5] + Pose3d[6]) / 2);
            debug_3d_lines[5].SetPosition(0, Pose3d[5]);
            debug_3d_lines[5].SetPosition(1, Pose3d[6]);
            debug_3d_lines[6].SetPosition(0, Pose3d[5]);
            debug_3d_lines[6].SetPosition(1, Pose3d[11]);
            debug_3d_lines[7].SetPosition(0, Pose3d[6]);
            debug_3d_lines[7].SetPosition(1, Pose3d[12]);
            debug_3d_lines[8].SetPosition(0, Pose3d[11]);
            debug_3d_lines[8].SetPosition(1, Pose3d[12]);
            debug_3d_lines[9].SetPosition(0, Pose3d[5]);
            debug_3d_lines[9].SetPosition(1, Pose3d[7]);
            debug_3d_lines[10].SetPosition(0, Pose3d[6]);
            debug_3d_lines[10].SetPosition(1, Pose3d[8]);
            debug_3d_lines[11].SetPosition(0, Pose3d[7]);
            debug_3d_lines[11].SetPosition(1, Pose3d[9]);
            debug_3d_lines[12].SetPosition(0, Pose3d[8]);
            debug_3d_lines[12].SetPosition(1, Pose3d[10]);
            debug_3d_lines[13].SetPosition(0, Pose3d[11]);
            debug_3d_lines[13].SetPosition(1, Pose3d[13]);
            debug_3d_lines[14].SetPosition(0, Pose3d[12]);
            debug_3d_lines[14].SetPosition(1, Pose3d[14]);
            debug_3d_lines[15].SetPosition(0, Pose3d[13]);
            debug_3d_lines[15].SetPosition(1, Pose3d[15]);
            debug_3d_lines[16].SetPosition(0, Pose3d[14]);
            debug_3d_lines[16].SetPosition(1, Pose3d[16]);

        }

        /// <summary>
        /// 終わるときにリソースを破棄。      Dispose the resource when destroy.
        /// </summary>
        private void OnDestroy()
        {
            running = false;
            if (debug_logfile)
            {
                fps_writer.Flush();
                fps_writer.Close();
                pos_writer.Flush();
                pos_writer.Close();
                rot_writer.Flush();
                rot_writer.Close();
            }
        }

    }
}