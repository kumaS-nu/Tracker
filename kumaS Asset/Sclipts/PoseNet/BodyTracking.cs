using System;
using UnityEngine;
using OpenCvSharp;
using System.Threading.Tasks;
using System.Text;

namespace kumaS.PoseNet
{
    public partial class BodyTracking : MonoBehaviour
    {
        /// <summary>
        /// 等間隔に複数スレッドを走らす。     Runs multiple threads at equal intervals.
        /// </summary>
        private async void RunAsync()
        {
            await Task.Delay(100);
            while (running)
            {
                Task[] tasks = new Task[thread - 1];
                for (int i = 0; i < thread - 1; i++)
                {
                    var i1 = i;
                    tasks[i] = Task.Run(() => Estimate(i1));
                    await Task.Delay(diff_time);
                }

                await Task.WhenAny(tasks);
            }
        }

        /// <summary>
        /// 2d、3dの座標を推定する。      Estimate 2D and 3D coordinates.
        /// </summary>
        /// <param name="i">スレッド番号  Thread Number.</param>
        private void Estimate(int i)
        {
            stopwatches[i].Restart();
            Mat image_r = new Mat();
            image_r = capture.Read();
            int image_width = image_r.Width;
            int image_height = image_r.Height;
            Mat resized = new Mat();
            try
            {
                Cv2.Resize(image_r, resized, new Size(posenet[i].width, posenet[i].height));
                var list = posenet[i].Run(resized.Data);
                var data2d = new Vector2[17];
                for (int j = 0; j < 17; j++)
                {
                    data2d[j] = new Vector2((float)(posenet[i].width - list[2 * j]) / posenet[i].width * image_width / image_height, list[2 * j + 1] / (float)posenet[i].height);
                }

                lock (pose2d_chain)
                {
                    pose2d_chain.AddLast(data2d);
                    pose2d_chain.RemoveFirst();
                }

                ConvertTo3d(data2d, (float) image_width / image_height * 0.5f);

                stopwatches[i].Stop();
                
                lock (elapt_time)
                {
                    elapt_time.AddLast((int) stopwatches[i].ElapsedMilliseconds);
                    elapt_time.RemoveFirst();
                }

                lock (fin_time)
                {
                    fin_time.AddLast(DateTime.Now);
                    if (fin_time.Count > 8)
                    {
                        fin_time.RemoveFirst();
                    }
                }
            }
            finally
            {
                if (resized.IsEnabledDispose)
                {
                    resized.Dispose();
                }
            }
        }

        /// <summary>
        /// 2dから3dに変換。          Convert 2D to 3D.
        /// </summary>
        /// <param name="data2d">推定された2d座標。     Estimated 2D coordinates.</param>
        /// <param name="x_center">横の中心座標。       Horizontal center coordinates.</param>
        private void ConvertTo3d(Vector2[] data2d, float x_center)
        {
            Vector3[] data3d = new Vector3[17];
            Vector3 vert = default;
            float tan = Mathf.Tan(camera_angle * Mathf.Deg2Rad);

            data3d[5].z = z_offset - shoulder2hip / (2 * (data2d[5] - data2d[11]).magnitude * tan);
            data3d[5] = Set3dXY(data3d[5], data2d[5], tan, x_center);
            data3d[11].z = data3d[5].z;
            data3d[11] = Set3dXY(data3d[11], data2d[11], tan, x_center);
            data3d[6].z = z_offset - shoulder2hip / (2 * (data2d[6] - data2d[12]).magnitude * tan);
            data3d[6] = Set3dXY(data3d[6], data2d[6], tan, x_center);
            data3d[12].z = data3d[6].z;
            data3d[12] = Set3dXY(data3d[12], data2d[12], tan, x_center);
            vert = Vector3.Cross(data3d[12] - data3d[6], data3d[5] - data3d[6]);

            data3d[13] = SetVector3(vert, data3d[11], data2d[13], tan, x_center, hip2knee, 11, 13);
            data3d[14] = SetVector3(vert, data3d[12], data2d[14], tan, x_center, hip2knee, 12, 14);

            vert = Vector3.Cross(data3d[13] - data3d[11], data3d[13] - data3d[11]);
            data3d[15] = SetVector3(vert, data3d[13], data2d[15], tan, x_center, knee2ankle, 13, 15);

            vert = Vector3.Cross(data3d[11] - data3d[12], data3d[14] - data3d[12]);
            data3d[16] = SetVector3(vert, data3d[14], data2d[16], tan, x_center, knee2ankle, 14, 16);

            vert = data3d[5] - data3d[6];
            data3d[7] = SetVector3(vert, data3d[5], data2d[7], tan, x_center, shoulder2elbow, 5, 7);

            vert = data3d[6] - data3d[5];
            data3d[8] = SetVector3(vert, data3d[6], data2d[8], tan, x_center, shoulder2elbow, 6, 8);

            vert = Vector3.Cross(data3d[11] - data3d[5], data3d[7] - data3d[5]);
            data3d[9] = SetVector3(vert, data3d[7], data2d[9], tan, x_center, elbow2wrist, 7, 9);

            vert = Vector3.Cross(data3d[8] - data3d[6], data3d[12] - data3d[6]);
            data3d[10] = SetVector3(vert, data3d[8], data2d[10], tan, x_center, elbow2wrist, 8, 10);


            vert = data3d[5] - data3d[11];
            Vector3 before = default;
            lock (pose3d_chain)
            {
                before = pose3d_chain.Last.Value[0] - (pose3d_chain.Last.Value[5] + pose3d_chain.Last.Value[6]) / 2;
            }

            data3d[0] = SetVector3(vert, (data3d[5] + data3d[6]) / 2, data2d[0], tan, x_center, shoulderline2nose,
                before);

            vert = data3d[5] - data3d[6];
            data3d[1] = SetVector3(vert, data3d[0], data2d[1], tan, x_center, nose2eye, 0, 1);

            data3d[3] = SetVector3(vert, data3d[0], data2d[3], tan, x_center, nose2ear, 0, 3);

            vert = data3d[6] - data3d[5];
            data3d[2] = SetVector3(vert, data3d[0], data2d[2], tan, x_center, nose2eye, 0, 2);

            data3d[4] = SetVector3(vert, data3d[0], data2d[4], tan, x_center, nose2ear, 0, 4);


            CalibY(data3d);
            var sharpedData = ReSharp3d(data3d);

            lock (pose3d_chain)
            {
                pose3d_chain.AddLast(sharpedData);
                pose3d_chain.RemoveFirst();
            }

            if (debug_logfile)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < 17; j++)
                {
                    sb.Append(data2d[j].x);
                    sb.Append(",");
                    sb.Append(data2d[j].y);
                    sb.Append(",");
                }

                for (int j = 0; j < 17; j++)
                {
                    sb.Append(sharpedData[j].x);
                    sb.Append(",");
                    sb.Append(sharpedData[j].y);
                    sb.Append(",");
                    sb.Append(sharpedData[j].z);
                    sb.Append(",");
                }

                lock (pos_writer)
                {
                    pos_writer.WriteLine(sb.ToString());
                }
            }
        }

        /// <summary>
        /// 3次元の点を設定する。     Set up a 3d point.
        /// </summary>
        /// <param name="vert">あり得る方向の中心。               The center of the possible direction.</param>
        /// <param name="root">元の3d座標。                       Root 3D coordinates.</param>
        /// <param name="picture">推定された2d座標。              Estimated 2D coordinates.</param>
        /// <param name="tan">カメラの視野角のタンジェント値。    The tangent value of the camera's viewing angle.</param>
        /// <param name="x_center">横の中心座標。                 Horizontal center coordinates.</param>
        /// <param name="bone_langth">ボーンの長さ。              Length of the bone.</param>
        /// <param name="before">この前の推定されたベクトル。     Estimated vector in front of this.</param>
        /// <returns>3次元の点                                    3d point.</returns>
        private Vector3 SetVector3(Vector3 vert, Vector3 root, Vector2 picture, float tan, float x_center,
            float bone_langth, Vector3 before)
        {
            var z = CalcZ(root, picture, x_center, tan, bone_langth);
            if (Math.Abs(z.Item1 - z.Item2) < 0.0001f)
            {
                return Set3dXY(new Vector3(0, 0, z.Item1), picture, tan, x_center);
            }

            Vector3 i1 = new Vector3(0, 0, z.Item1);
            Vector3 i2 = new Vector3(0, 0, z.Item2);
            i1 = Set3dXY(i1, picture, tan, x_center);
            i2 = Set3dXY(i2, picture, tan, x_center);

            if (Vector3.Dot(vert, i1 - root) < 0)
            {
                return i2;
            }

            if (Vector3.Dot(vert, i2 - root) < 0)
            {
                return i1;
            }

            return Vector3.Dot(before.normalized, (i1 - root).normalized) >
                   Vector3.Dot(before.normalized, (i2 - root).normalized)
                ? i1
                : i2;
        }

        /// <summary>
        /// 3次元の点を設定する。     Set up a 3d point.
        /// </summary>
        /// <param name="vert">あり得る方向の中心。               The center of the possible direction.</param>
        /// <param name="root">元の3d座標。                       Root 3D coordinates.</param>
        /// <param name="picture">推定された2d座標。              Estimated 2D coordinates.</param>
        /// <param name="tan">カメラの視野角のタンジェント値。    The tangent value of the camera's viewing angle.</param>
        /// <param name="x_center">横の中心座標。                 Horizontal center coordinates.</param>
        /// <param name="bone_length">ボーンの長さ。              Length of the bone.</param>
        /// <param name="from">ベクトルの始点のインデックス。     Index of the starting point of the vector.</param>
        /// <param name="to">ベクトルの終点のインデックス。       The index of the endpoint of the vector.</param>
        /// <returns>3次元の点                                    3d point.</returns>
        private Vector3 SetVector3(Vector3 vert, Vector3 root, Vector2 picture, float tan, float x_center,
            float bone_length, int from, int to)
        {
            Vector3 before = default;
            lock (pose3d_chain)
            {
                before = pose3d_chain.Last.Value[to] - pose3d_chain.Last.Value[from];
            }

            return SetVector3(vert, root, picture, tan, x_center, bone_length, before);
        }

        /// <summary>
        /// Zからx,yを計算する。
        /// </summary>
        /// <param name="data3d">z座標の決まった３次元の点。     A 3d point with a fixed z-coordinate.</param>
        /// <param name="data2d">推定された2d座標。              Estimated 2D coordinates.</param>
        /// <param name="tan">カメラの視野角のタンジェント値。   The tangent value of the camera's viewing angle.</param>
        /// <param name="x_center">横の中心座標。                Horizontal center coordinates.</param>
        /// <returns>3次元の点                                   3d point.</returns>
        private Vector3 Set3dXY(Vector3 data3d, Vector2 data2d, float tan, float x_center)
        {
            data3d.x = 2 * (-data3d.z + z_offset) * tan * (x_center - data2d.x);

            data3d.y = 2 * (-data3d.z + z_offset) * tan * (0.5f - data2d.y);

            return data3d;
        }

        /// <summary>
        /// 距離に関する二次方程式を解く。
        /// </summary>
        /// <param name="root">元の3d座標。                       Root 3D coordinates.</param>
        /// <param name="picture">推定された2d座標。              Estimated 2D coordinates.</param>
        /// <param name="x_center">横の中心座標。                 Horizontal center coordinates.</param>
        /// <param name="tan">カメラの視野角のタンジェント値。    The tangent value of the camera's viewing angle.</param>
        /// <param name="bone_length">ボーンの長さ。              Length of the bone.</param>
        /// <returns>推定されたz座標。                            Estimated z-coordinates.</returns>
        private (float, float) CalcZ(Vector3 root, Vector2 picture, float x_center, float tan, float bone_length)
        {
            float x_ratio = 2 * (x_center - picture.x) * tan;
            float y_ratio = 2 * (0.5f - picture.y) * tan;
            float x_distans = z_offset * x_ratio - root.x;
            float y_distans = z_offset * y_ratio - root.y;

            float a = x_ratio * x_ratio + y_ratio * y_ratio + 1;
            float b = x_ratio * x_distans + y_ratio * y_distans + root.z;
            float c = x_distans * x_distans + y_distans * y_distans + root.z * root.z - bone_length * bone_length;

            if(b * b - a * c < 0)
            {
                return (b / a, b / a);
            }

            return ((b - Mathf.Sqrt(b * b - a * c)) / a, (b + Mathf.Sqrt(b * b - a * c)) / a);
        }

        /// <summary>
        /// 足を0とした座標に較正する。      Calibrate to a coordinate with the foot as 0.
        /// </summary>
        /// <param name="data3d">推定された3d座標。     Estimated 3D coordinates.</param>
        private void CalibY(Vector3[] data3d)
        {
            float foot = (data3d[15].y + data3d[16].y) * 0.5f;

            for (int i = 0; i < 17; i++)
            {
                data3d[i].y -= foot;
            }
        }
    }
}