using DlibDotNet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using kumaS.Extention;

namespace kumaS.FaceTrack
{
    public partial class FaceTracking : MonoBehaviour
    {
        //----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 検出をする非同期メゾットを実行     Execute asynchronous method to detect
        /// </summary>
        private async void DetectAsync()
        {
            await Task.Delay(100);
            while (running)
            {
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < thread - 1; i++)
                {
                    tasks.Add(Task.Run(() => { FaceTrack(i); }));
                    await Task.Delay(diff_time);
                }
                if (debug_eye_image)
                {
                    running = false;
                }
                await Task.WhenAny(tasks);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 顔検出＆計算      Face detection & calculation
        /// </summary>
        private void FaceTrack(int threadNo)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            bool result = false;
            Vector3 est_pos = pos;
            Vector3 est_rot = rot;

            switch (mode)
            {
                case DetectMode.OpenCV: result = Cascade(threadNo, out est_pos); break;

                case DetectMode.Dlib5: result = Dlib5(threadNo, out est_pos, out est_rot); break;

                case DetectMode.Dlib68: result = Dlib68(threadNo, out est_pos, out est_rot); break;

                case DetectMode.Mixed: result = Mixed(threadNo, out est_pos, out est_rot); break;
            }
            if (logToFile)
            {
                pos_rot_writer.WriteLine(est_pos.x + "," + est_pos.y + "," + est_pos.z + "," + est_rot.x + "," + est_rot.y + "," + est_rot.z);
            }
            else if (debug_pos_rot)
            {
                Debug.Log("est_pos = " + est_pos.ToString("F6") + ", est_rot = " + est_rot.ToString("F6"));
            }

            est_rot = RotReSharp(est_rot + rot_offset, est_pos);

            //中心を初期化    initialize center
            if (inputR)
            {
                lock (lock_pos_rot)
                {
                    for (int i = 0; i < smooth; i++)
                    {
                        pos_chain.AddLast(Vector3.zero); rot_chain.AddLast(Vector3.zero);
                        pos_chain.RemoveFirst(); rot_chain.RemoveFirst();
                    }
                }
                center = est_pos;
                inputR = false;
            }
            est_pos -= center;
            est_pos = EstimateErrCheck(est_pos, position_verocity_ristrict, pos_chain.Skip(smooth - thread - 1).Average());
            est_rot = EstimateErrCheck(est_rot, rotation_verocity_ristrict, rot_chain.Skip(smooth - thread - 1).Average());
            est_pos = RangeCheck(est_pos, radius);
            est_rot = RangeCheck(est_rot, rotation_range);

            lock (lock_pos_rot)
            {
                if (smoothing == SmoothingMethod.Average)
                {
                    pos_chain.AddLast(est_pos); rot_chain.AddLast(est_rot);
                    pos_chain.RemoveFirst(); rot_chain.RemoveFirst();
                    pos = pos_chain.Average();
                    rot = rot_chain.Average();
                }
                else
                {
                    pos = (1.0f - alpha) * est_pos + alpha * pos;
                    rot = (1.0f - alpha) * est_rot + alpha * rot;
                }
            }

            lock (lock_isSuccess)
            {
                IsSuccess = result;
            }

            stopwatch.Stop();
            if (result)
            {
                ++suc;
                lock (lock_fps)
                {

                    if (fin_time.Count > 7)
                    {
                        fin_time.RemoveFirst();
                        elapt_time.RemoveFirst();
                    }
                    fin_time.AddLast(DateTime.Now);
                    elapt_time.AddLast((int)stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                ++fail;
            }

        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// カスケードファイルを使用した顔検出       Face detection using cascade file
        /// </summary>
        /// <param name="threadNo">スレッド番号      Thread number</param>
        /// <param name="est_pos">推定された位置     Estimated position</param>
        /// <returns>推定できたか                    Whether it could be estimated</returns>
        private bool Cascade(int threadNo, out Vector3 est_pos)
        {
            Mat image_r = new Mat();
            Mat image = new Mat();
            try
            {
                lock (lock_capture)
                {
                    image_r = caputure.Read();
                    if (image_r.Data == null || image_r.Cols == -1)
                    {
                        throw new NullReferenceException("capture is null");
                    }
                }
                if (resolution == 1)
                {
                    image = image_r.Clone();
                }
                else
                {
                    Cv2.Resize(image_r, image, new Size(image_r.Cols / resolution, image_r.Rows / resolution));
                }
                GC.KeepAlive(image_r);

                var faces = cascade.DetectMultiScale(image);
                if (!faces.Any())
                {
                    throw new InvalidOperationException("faces has no elements");
                }
                est_pos.x = -(image.Width / 2 - (faces.First().X + faces.First().Width * 0.5f)) / image.Width;
                est_pos.y = (image.Height / 2 - (faces.First().Y + faces.First().Height * 0.5f)) / image.Height;
                est_pos.z = (faces.First().Width / (float)image.Width + faces.First().Height / (float)image.Height - z_offset) * z_scale;
                if (debug_face_image)
                {
                    DetectDebug(threadNo, image, faces);
                }
                GC.KeepAlive(image);
            }
            catch (Exception e) { Debug.Log(e); est_pos = pos; ++fail; if (image.IsEnabledDispose) { image.Dispose(); } return false; }
            lock (lock_imagebytes[threadNo])
            {
                if (image.IsEnabledDispose)
                {
                    image.Dispose();
                }
            }
            return true;
        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Dlib5を利用した顔検出                    Face detection using Dlib5
        /// </summary>
        /// <param name="threadNo">スレッド番号      Thread number</param>
        /// <param name="est_pos">推定された位置     Estimated position</param>
        /// <param name="est_rot">推定された回転     Estimated quotation</param>
        /// <returns>推定できたか                    Whether it could be estimated</returns>
        private bool Dlib5(int threadNo, out Vector3 est_pos, out Vector3 est_rot)
        {
            est_rot = rot;
            Mat image_r = new Mat();
            Array2D<RgbPixel> array2D = new Array2D<RgbPixel>();
            Mat image = new Mat();
            try
            {
                lock (lock_capture)
                {
                    image_r = caputure.Read();
                    if (image_r.Data == null)
                    {
                        throw new NullReferenceException("capture is null");
                    }

                    if (ptr.Contains(image_r.Data))
                    {
                        throw new InvalidOperationException("taken same data");
                    }
                    else
                    {
                        ptr[threadNo] = image_r.Data;
                    }
                }
                if (resolution == 1)
                {
                    image = image_r.Clone();
                }
                else
                {
                    Cv2.Resize(image_r, image, new Size(image_r.Cols / resolution, image_r.Rows / resolution));
                }

                GC.KeepAlive(image_r);

                lock (lock_imagebytes[threadNo])
                {
                    Marshal.Copy(image.Data, bytes[threadNo], 0, bytes[threadNo].Length);
                    array2D = Dlib.LoadImageData<RgbPixel>(bytes[threadNo], (uint)image.Height, (uint)image.Width, (uint)(image.Width * image.ElemSize()));
                }

                Rectangle rectangles = default;
                if (un_safe)
                {
                    rectangles = detector[0].Operator(array2D).FirstOrDefault();
                }
                else
                {
                    rectangles = detector[threadNo].Operator(array2D).FirstOrDefault();
                }

                DlibDotNet.Point[] points = new DlibDotNet.Point[5];
                if (rectangles == default)
                {
                    throw new InvalidOperationException("this contains no elements.");
                }

                using (FullObjectDetection shapes = shape.Detect(array2D, rectangles))
                {

                    for (uint i = 0; i < 5; i++)
                    {
                        points[i] = shapes.GetPart(i);
                    }
                }

                est_pos.x = -(image.Width / 2 - points[4].X) / (float)image.Width;
                est_pos.y = (image.Height / 2 - points[4].Y) / (float)image.Height;
                est_pos.z = (points[0].X - points[2].X) / (float)image.Width + (points[0].Y - points[2].Y) / (float)image.Height - z_offset;

                try
                {
                    est_rot.z = Mathf.Rad2Deg * Mathf.Atan2(points[0].Y - points[2].Y, points[0].X - points[2].X);
                }
                catch (DivideByZeroException)
                {
                    est_rot.z = points[0].Y - points[2].Y < 0 ? -90 : 90;
                }
                if (debug_face_image)
                {
                    DetectDebug(threadNo, image, points: points);
                }
                GC.KeepAlive(image);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                est_pos = pos;
                if (array2D.IsEnableDispose)
                {
                    array2D.Dispose();
                }
                if (image.IsEnabledDispose)
                {
                    image.Dispose();
                }

                return false;

            }

            if (array2D.IsEnableDispose)
            {
                array2D.Dispose();
            }
            lock (lock_imagebytes[threadNo])
            {
                if (image.IsEnabledDispose)
                {
                    image.Dispose();
                }
            }
            return true;
        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Dlib68を利用した顔検出                   Face detection using Dlib68
        /// </summary>
        /// <param name="threadNo">スレッド番号      Thread number</param>
        /// <param name="est_pos">推定された位置     Estimated position</param>
        /// <param name="est_rot">推定された回転     Estimated quotation</param>
        /// <returns>推定できたか                    Whether it could be estimated</returns>
        private bool Dlib68(int threadNo, out Vector3 est_pos, out Vector3 est_rot)
        {
            Mat image_r = new Mat();
            Array2D<RgbPixel> array2D = new Array2D<RgbPixel>();
            Mat image = new Mat();
            try
            {
                lock (lock_capture)
                {
                    image_r = caputure.Read();
                    if (image_r.Data == null)
                    {
                        throw new NullReferenceException("capture is null");
                    }

                    if (ptr.Contains(image_r.Data))
                    {
                        throw new InvalidOperationException("taken same data");
                    }
                    else
                    {
                        ptr[threadNo] = image_r.Data;
                    }
                }

                if (resolution == 1)
                {
                    image = image_r.Clone();
                }
                else
                {
                    Cv2.Resize(image_r, image, new Size(image_r.Cols / resolution, image_r.Rows / resolution));
                }

                GC.KeepAlive(image_r);

                lock (lock_imagebytes[threadNo])
                {
                    Marshal.Copy(image.Data, bytes[threadNo], 0, bytes[threadNo].Length);
                    array2D = Dlib.LoadImageData<RgbPixel>(bytes[threadNo], (uint)image.Height, (uint)image.Width, (uint)(image.Width * image.ElemSize()));
                }

                Rectangle rectangles = default;
                if (un_safe)
                {
                    rectangles = detector[0].Operator(array2D).FirstOrDefault();
                }
                else
                {
                    rectangles = detector[threadNo].Operator(array2D).FirstOrDefault();
                }

                DlibDotNet.Point[] points = new DlibDotNet.Point[68];

                if (rectangles == default)
                {
                    throw new InvalidOperationException("rectangles has no elements");
                }

                using (FullObjectDetection shapes = shape.Detect(array2D, rectangles))
                {
                    for (uint i = 0; i < 68; i++)
                    {
                        points[i] = shapes.GetPart(i);
                    }
                    lock (lock_landmarks)
                    {
                        landmark_detection = points;
                    }
                }

                Point2f[] image_points = new Point2f[8];
                image_points[0] = new Point2f(points[30].X, points[30].Y);
                image_points[1] = new Point2f(points[8].X, points[8].Y);
                image_points[2] = new Point2f(points[45].X, points[45].Y);
                image_points[3] = new Point2f(points[36].X, points[36].Y);
                image_points[4] = new Point2f(points[54].X, points[54].Y);
                image_points[5] = new Point2f(points[48].X, points[48].Y);
                image_points[6] = new Point2f(points[42].X, points[42].Y);
                image_points[7] = new Point2f(points[39].X, points[39].Y);
                var image_points_mat = new Mat(image_points.Length, 1, MatType.CV_32FC2, image_points);
                eye_point_R[threadNo][0] = points[42]; eye_point_L[threadNo][0] = points[39];
                eye_point_R[threadNo][1] = points[45]; eye_point_L[threadNo][1] = points[36];
                eye_point_R[threadNo][2] = points[43]; eye_point_L[threadNo][2] = points[38];
                eye_point_R[threadNo][3] = points[47]; eye_point_L[threadNo][3] = points[40];
                eye_point_R[threadNo][4] = points[44]; eye_point_L[threadNo][4] = points[37];
                eye_point_R[threadNo][5] = points[46]; eye_point_L[threadNo][5] = points[41];

                Mat rvec_mat = new Mat();
                Mat tvec_mat = new Mat();
                Mat projMatrix_mat = new Mat();
                Cv2.SolvePnP(model_points_mat, image_points_mat, camera_matrix_mat, dist_coeffs_mat, rvec_mat, tvec_mat);
                Marshal.Copy(tvec_mat.Data, pos_double[threadNo], 0, 3);
                Cv2.Rodrigues(rvec_mat, projMatrix_mat);
                Marshal.Copy(projMatrix_mat.Data, proj[threadNo], 0, 9);

                est_pos.x = -(float)pos_double[threadNo][0];
                est_pos.y = (float)pos_double[threadNo][1];
                est_pos.z = (float)pos_double[threadNo][2];

                est_rot = RotMatToQuatanion(proj[threadNo]).eulerAngles;
                est_rot.x *= -1;

                if (blink_tracking)
                {
                    BlinkTracker(threadNo, eye_point_L[threadNo], eye_point_R[threadNo], est_rot);
                }
                if (eye_tracking)
                {
                    EyeTracker(threadNo, image, points.Skip(42).Take(6), points.Skip(36).Take(6));
                }

                image_points_mat.Dispose();
                rvec_mat.Dispose();
                tvec_mat.Dispose();
                projMatrix_mat.Dispose();
                if (debug_face_image)
                {
                    DetectDebug(threadNo, image, points: points);
                }
                GC.KeepAlive(image);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                est_pos = pos; est_rot = rot;
                if (array2D.IsEnableDispose)
                {
                    array2D.Dispose();
                }
                if (image.IsEnabledDispose)
                {
                    image.Dispose();
                }
                return false;
            }
            if (array2D.IsEnableDispose)
            {
                array2D.Dispose();
            }
            lock (lock_imagebytes[threadNo])
            {
                if (image.IsEnabledDispose)
                {
                    image.Dispose();
                }
            }

            return true;
        }


        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// OpenCVとDlib68を併用した推定。Dlib68より高速だが、顔の検出率は低め。
        /// </summary>
        /// <param name="threadNo">走らせるスレッド番号</param>
        /// <param name="est_pos">推定した位置</param>
        /// <param name="est_rot">推定した回転</param>
        /// <returns>推定できたか</returns>
        private bool Mixed(int threadNo, out Vector3 est_pos, out Vector3 est_rot)
        {
            Mat image_r = new Mat();
            Mat image = new Mat();
            try
            {
                lock (lock_capture)
                {
                    image_r = caputure.Read();
                    if (image_r.Data == null)
                    {
                        throw new NullReferenceException("capture is null");
                    }

                    if (ptr.Contains(image_r.Data))
                    {
                        throw new InvalidOperationException("taken same data");
                    }
                    else
                    {
                        ptr[threadNo] = image_r.Data;
                    }
                }

                if (resolution == 1)
                {
                    image = image_r.Clone();
                }
                else
                {
                    Cv2.Resize(image_r, image, new Size(image_r.Cols / resolution, image_r.Rows / resolution));
                }

                GC.KeepAlive(image_r);

                var faces = cascade.DetectMultiScale(image);

                if (!faces.Any())
                {
                    throw new InvalidOperationException("this contains no elements");
                }

                Array2D<RgbPixel> array2D = new Array2D<RgbPixel>();

                lock (lock_imagebytes[threadNo])
                {
                    Marshal.Copy(image.Data, bytes[threadNo], 0, bytes[threadNo].Length);
                    array2D = Dlib.LoadImageData<RgbPixel>(bytes[threadNo], (uint)image.Height, (uint)image.Width, (uint)(image.Width * image.ElemSize()));
                }

                var rectangles = new Rectangle(faces.First().Left, faces.First().Top, faces.First().Right, faces.First().Bottom);

                DlibDotNet.Point[] points = new DlibDotNet.Point[68];

                using (FullObjectDetection shapes = shape.Detect(array2D, rectangles))
                {
                    for (uint i = 0; i < 68; i++)
                    {
                        points[i] = shapes.GetPart(i);
                    }
                    lock (lock_landmarks)
                    {
                        landmark_detection = points;
                    }
                }

                array2D.Dispose();

                Point2f[] image_points = new Point2f[6];
                image_points[0] = new Point2f(points[30].X, points[30].Y);
                image_points[1] = new Point2f(points[8].X, points[8].Y);
                image_points[2] = new Point2f(points[45].X, points[45].Y);
                image_points[3] = new Point2f(points[36].X, points[36].Y);
                image_points[4] = new Point2f(points[54].X, points[54].Y);
                image_points[5] = new Point2f(points[48].X, points[48].Y);
                var image_points_mat = new Mat(image_points.Length, 1, MatType.CV_32FC2, image_points);
                eye_point_R[threadNo][0] = points[42]; eye_point_L[threadNo][1] = points[36];
                eye_point_R[threadNo][2] = points[43]; eye_point_L[threadNo][2] = points[38];
                eye_point_R[threadNo][3] = points[47]; eye_point_L[threadNo][3] = points[40];
                eye_point_R[threadNo][4] = points[44]; eye_point_L[threadNo][4] = points[37];
                eye_point_R[threadNo][5] = points[46]; eye_point_L[threadNo][5] = points[41];

                Mat rvec_mat = new Mat();
                Mat tvec_mat = new Mat();
                Mat projMatrix_mat = new Mat();
                Cv2.SolvePnP(model_points_mat, image_points_mat, camera_matrix_mat, dist_coeffs_mat, rvec_mat, tvec_mat);
                Marshal.Copy(tvec_mat.Data, pos_double[threadNo], 0, 3);
                Cv2.Rodrigues(rvec_mat, projMatrix_mat);
                Marshal.Copy(projMatrix_mat.Data, proj[threadNo], 0, 9);

                est_pos.x = -(float)pos_double[threadNo][0];
                est_pos.y = (float)pos_double[threadNo][1];
                est_pos.z = (float)pos_double[threadNo][2];

                est_rot = RotMatToQuatanion(proj[threadNo]).eulerAngles;

                if (blink_tracking)
                {
                    BlinkTracker(threadNo, eye_point_L[threadNo], eye_point_R[threadNo], est_rot);
                }
                if (eye_tracking)
                {
                    EyeTracker(threadNo, image, points.Skip(42).Take(6), points.Skip(36).Take(6));
                }

                image_points_mat.Dispose();
                rvec_mat.Dispose();
                tvec_mat.Dispose();
                projMatrix_mat.Dispose();
                GC.KeepAlive(image);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                est_pos = pos; est_rot = rot;
                if (image.IsEnabledDispose)
                {
                    image.Dispose();
                }
                return false;
            }
            lock (lock_imagebytes[threadNo])
            {
                if (image.IsEnabledDispose)
                {
                    image.Dispose();
                }
            }

            return true;
        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 目のRect生成。
        /// </summary>
        /// <param name="points">目の特徴点6つ</param>
        /// <returns>目のRect</returns>
        private OpenCvSharp.Rect MakeRect(IEnumerable<DlibDotNet.Point> points)
        {
            int left = points.Min(t => t.X);
            int top = points.Min(t => t.Y);
            int width = points.Max(t => t.X) - left;
            int height = points.Max(t => t.Y) - top;

            return new OpenCvSharp.Rect(left, top, width, height);
        }

        /// <summary>
        /// まばたきを推定
        /// </summary>
        /// <param name="threadNo">走らせるスレッド番号</param>
        /// <param name="left">左目のPoint</param>
        /// <param name="right">右目のPoint</param>
        /// <param name="est_rot">推定された回転</param>
        private void BlinkTracker(int threadNo, DlibDotNet.Point[] left, DlibDotNet.Point[] right, Vector3 est_rot)
        {
            if (Mathf.Cos(est_rot.x) > Mathf.Sqrt(0.5f) && Mathf.Cos(est_rot.y) > Mathf.Sqrt(0.5f))
            {
                eye_ratio_L[threadNo] = 1.0f - ((Distance(left[2], left[3]) + Distance(left[4], left[5])) / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(left[0], left[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad) / 2.0f - eye_ratio_l) / (eye_ratio_h - eye_ratio_l);
                eye_ratio_R[threadNo] = 1.0f - ((Distance(right[2], right[3]) + Distance(right[4], right[5])) / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(right[0], right[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad) / 2.0f - eye_ratio_l) / (eye_ratio_h - eye_ratio_l);
            }
            else
            {
                eye_ratio_L[threadNo] = 1.0f - ((Distance(left[2], left[3]) + Distance(left[4], left[5])) / Distance(left[0], left[1]) / 2.0f - eye_ratio_l) / (eye_ratio_h - eye_ratio_l);
                eye_ratio_R[threadNo] = 1.0f - ((Distance(right[2], right[3]) + Distance(right[4], right[5])) / Distance(right[0], right[1]) / 2.0f - eye_ratio_l) / (eye_ratio_h - eye_ratio_l);
            }

            lock (lock_eye_blink)
            {
                if (smoothing == SmoothingMethod.Average)
                {
                    eye_L.RemoveFirst(); eye_L.AddLast(eye_ratio_L[threadNo]);
                    eye_R.RemoveFirst(); eye_R.AddLast(eye_ratio_R[threadNo]);
                    eye_L_c = eye_L.Average();
                    eye_R_c = eye_R.Average();
                }
                else
                {
                    eye_L_c = (1.0f - alpha) * eye_ratio_L[threadNo] + alpha * eye_L_c;
                    eye_R_c = (1.0f - alpha) * eye_ratio_R[threadNo] + alpha * eye_R_c;
                }
            }

            if (logToFile)
            {
                lock (lock_eye_ratio_file)
                {
                    if (Mathf.Cos(est_rot.x) > Mathf.Sqrt(0.5f) && Mathf.Cos(est_rot.y) > Mathf.Sqrt(0.5f))
                    {
                        eye_ratio_writer.WriteLine(((Distance(left[2], left[3]) + Distance(left[4], left[5])) / 2 / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(left[0], left[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad)) + "," + ((Distance(right[2], right[3]) + Distance(right[4], right[5])) / 2 / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(right[0], right[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad)));
                    }
                    else
                    {
                        eye_ratio_writer.WriteLine(((Distance(left[2], left[3]) + Distance(left[4], left[5])) / 2 / Distance(left[0], left[1])) + "," + ((Distance(right[2], right[3]) + Distance(right[4], right[5])) / 2 / Distance(right[0], right[1])));
                    }
                }
            }
            else
            {
                if (debug_eye_closeness)
                {
                    Debug.Log("Eye Closeness L = " + eye_L.Last.Value + ", R = " + eye_R.Last.Value);
                }
                if (debug_eye_open_ratio)
                {
                    if (Mathf.Cos(est_rot.x) > Mathf.Sqrt(0.5f) && Mathf.Cos(est_rot.y) > Mathf.Sqrt(0.5f))
                    {
                        Debug.Log("Eye Open Ratio L = " + ((Distance(left[2], left[3]) + Distance(left[4], left[5])) / 2 / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(left[0], left[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad)) + ", R = " + ((Distance(right[2], right[3]) + Distance(right[4], right[5])) / 2 / Mathf.Cos(est_rot.x * Mathf.Deg2Rad) / Distance(right[0], right[1]) * Mathf.Cos(est_rot.y * Mathf.Deg2Rad)));
                    }
                    else
                    {
                        Debug.Log("Eye Open Ratio L = " + ((Distance(left[2], left[3]) + Distance(left[4], left[5])) / 2 / Distance(left[0], left[1])) + ", R = " + ((Distance(right[2], right[3]) + Distance(right[4], right[5])) / 2 / Distance(right[0], right[1])));
                    }
                }
            }
        }


        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 目玉の向き推定
        /// </summary>
        /// <param name="threadNo">走らせるスレッド番号</param>
        /// <param name="image">とれた画像</param>
        /// <param name="left_points">左目のPoint</param>
        /// <param name="right_points">右目のPoint</param>
        private void EyeTracker(int threadNo, Mat image, IEnumerable<DlibDotNet.Point> left_points, IEnumerable<DlibDotNet.Point> right_points)
        {
            OpenCvSharp.Rect left = MakeRect(left_points);
            OpenCvSharp.Rect right = MakeRect(right_points);

            if (eye_ratio_L[threadNo] < 0.5)
            {
                Mat left_eye_i = new Mat();

                lock (lock_imagebytes[threadNo])
                {
                    left_eye_i = image.Clone(left);
                }
                //R成分のみ抽出
                Mat left_eye_gr = left_eye_i.Split().Skip(2).First();
                GC.KeepAlive(left_eye_i);
                Mat left_th = new Mat();
                Cv2.BitwiseNot(left_eye_gr.Threshold(0, 255, ThresholdTypes.Otsu), left_th);
                try
                {
                    var moments = left_th.Moments(true);
                    var x = moments.M10 / moments.M00;
                    var y = moments.M01 / moments.M00;

                    lock (lock_eye_rotL)
                    {

                        if (smoothing == SmoothingMethod.Average)
                        {
                            eye_rot_L.RemoveFirst(); eye_rot_L.AddLast(CalcEyeRot(left_points.ToArray(), left.TopLeft, (float)x, (float)y, true));
                            lefteyerotation = eye_rot_L.Average() - eye_rot_offset_L;
                        }
                        else
                        {
                            lefteyerotation = (1.0f - alpha) * (CalcEyeRot(left_points.ToArray(), left.TopLeft, (float)x, (float)y, true) - eye_rot_offset_L) + alpha * lefteyerotation;
                        }

                    }
                    //デバッグ用
                    if (debug_eye_image)
                    {
                        byte[] a = new byte[left_eye_gr.Width * left_eye_gr.Height * left_eye_gr.ElemSize()];
                        Marshal.Copy(left_eye_gr.Data, a, 0, a.Length);
                        _mainContext.Post(_ =>
                        {
                            for (int i = 0; i < left.Width; i++)
                            {
                                for (int j = 0; j < left.Height; j++)
                                {
                                    Instantiate(ins, p0.position + new Vector3(i, j, 0), Quaternion.identity, p0).GetComponent<Renderer>().material.color = new Color(a[j * left.Width + i] / 255f, a[j * left.Width + i] / 255f, a[j * left.Width + i] / 255f);
                                }
                            }
                        }, null);
                        byte[] b = new byte[left_th.Width * left_th.Height * left_th.ElemSize()];
                        Marshal.Copy(left_th.Data, b, 0, b.Length);
                        _mainContext.Post(_ =>
                        {
                            for (int i = 0; i < left.Width; i++)
                            {
                                for (int j = 0; j < left.Height; j++)
                                {
                                    Instantiate(ins, p1.position + new Vector3(i, j, 0), Quaternion.identity, p1).GetComponent<Renderer>().material.color = new Color(b[j * left.Width + i] / 255f, b[j * left.Width + i] / 255f, b[j * left.Width + i] / 255f);
                                }
                            }

                            Instantiate(ins, p1.position + new Vector3((float)x, (float)y), Quaternion.identity);

                            foreach(var points in left_points)
                            {
                                Instantiate(ins, p1.position + new Vector3(points.X - left.X, points.Y - left.Y), Quaternion.identity);
                            }

                        }, null);
                    }
                    GC.KeepAlive(left_eye_gr);
                }
                finally
                {
                    if (left_eye_i.IsEnabledDispose)
                    {
                        left_eye_i.Dispose();
                    }

                    if (left_eye_gr.IsEnabledDispose)
                    {
                        left_eye_gr.Dispose();
                    }

                    if (left_th.IsEnabledDispose)
                    {
                        left_th.Dispose();
                    }

                }
            }

            if (eye_ratio_R[threadNo] < 0.5)
            {
                Mat right_eye_i = new Mat();

                lock (lock_imagebytes[threadNo])
                {
                    right_eye_i = image.Clone(right);
                }
                //R成分のみ抽出
                Mat right_eye_gr = right_eye_i.Split().Skip(2).First();
                Mat right_th = new Mat();
                Cv2.BitwiseNot(right_eye_gr.Threshold(0, 255, ThresholdTypes.Otsu), right_th);
                GC.KeepAlive(right_eye_i);
                try
                {
                    var moments = right_th.Moments(true);
                    var x = moments.M10 / moments.M00;
                    var y = moments.M01 / moments.M00;

                    lock (lock_eye_rotR)
                    {

                        if (smoothing == SmoothingMethod.Average)
                        {
                            eye_rot_R.RemoveFirst(); eye_rot_R.AddLast(CalcEyeRot(right_points.ToArray(), right.TopLeft, (float)x, (float) y, false));
                            righteyerotation = eye_rot_R.Average() - eye_rot_offset_R;
                        }
                        else
                        {
                            righteyerotation = (1.0f - alpha) * (CalcEyeRot(right_points.ToArray(), right.TopLeft, (float)x, (float)y, false) - eye_rot_offset_R) + alpha * righteyerotation;
                        }

                    }
                    //デバッグ用
                    if (debug_eye_image)
                    {
                        byte[] a = new byte[right_eye_gr.Width * right_eye_gr.Height * right_eye_gr.ElemSize()];
                        Marshal.Copy(right_eye_gr.Data, a, 0, a.Length);
                        _mainContext.Post(_ =>
                        {
                            for (int i = 0; i < right.Width; i++)
                            {
                                for (int j = 0; j < right.Height; j++)
                                {
                                    Instantiate(ins, p2.position + new Vector3(i, j, 0), Quaternion.identity, p2).GetComponent<Renderer>().material.color = new Color(a[j * right.Width + i] / 255f, a[j * right.Width + i] / 255f, a[j * right.Width + i] / 255f);
                                }
                            }

                            Instantiate(ins, p2.position + new Vector3((float)x, (float)y), Quaternion.identity);
                            foreach (var points in right_points)
                            {
                                Instantiate(ins, p2.position + new Vector3(points.X - right.X, points.Y - right.Y), Quaternion.identity);
                            }

                        }, null);
                        byte[] b = new byte[right_th.Width * right_th.Height * right_th.ElemSize()];
                        Marshal.Copy(right_th.Data, b, 0, b.Length);
                        _mainContext.Post(_ =>
                        {
                            for (int i = 0; i < right.Width; i++)
                            {
                                for (int j = 0; j < right.Height; j++)
                                {
                                    Instantiate(ins, p3.position + new Vector3(i, j, 0), Quaternion.identity, p3).GetComponent<Renderer>().material.color = new Color(b[j * right.Width + i] / 255f, b[j * right.Width + i] / 255f, b[j * right.Width + i] / 255f);
                                }
                            }
                        }, null);
                    }
                    GC.KeepAlive(right_eye_gr);
                }
                finally
                {
                    if (right_eye_i.IsEnabledDispose)
                    {
                        right_eye_i.Dispose();
                    }

                    if (right_eye_gr.IsEnabledDispose)
                    {
                        right_eye_gr.Dispose();
                    }

                    if (right_th.IsEnabledDispose)
                    {
                        right_th.Dispose();
                    }

                }
            }

            if (logToFile)
            {
                eye_rot_writer.WriteLine(eye_rot_L.Last.Value.x + "," + eye_rot_L.Last.Value.y + "," + eye_rot_R.Last.Value.x + "," + eye_rot_R.Last.Value.y);
            }
        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary> 
        /// デバック時の処理       Process at debugging 
        /// </summary>
        private void DetectDebug(int threadNo, Mat image, OpenCvSharp.Rect[] faces = null, DlibDotNet.Point[] points = null)
        {
            if (!image.IsDisposed)
            {
                int i = 0;
                switch (mode)
                {
                    case DetectMode.OpenCV:
                        if (faces.Length != 0)
                        {
                            Cv2.Rectangle(image, new OpenCvSharp.Point(faces.First().X, faces.First().Y),
                                          new OpenCvSharp.Point(faces.First().X + faces.First().Width, faces.First().Y + faces.First().Height),
                                          new Scalar(0, 255, 255), lineType: LineTypes.AntiAlias);
                        }
                        break;

                    case DetectMode.Dlib5:
                        foreach (var point in points)
                        {
                            Cv2.Circle(image, point.X, point.Y, 10, new Scalar(255, 255, 0), 5, LineTypes.AntiAlias);

                            Cv2.PutText(image, i.ToString(), new OpenCvSharp.Point(point.X, point.Y), HersheyFonts.HersheyPlain,
                                        0.6, new Scalar(0, 255, 0));
                            i++;
                        }
                        break;

                    case DetectMode.Dlib68:
                        foreach (var point in points)
                        {
                            Cv2.Circle(image, point.X, point.Y, 3, new Scalar(255, 255, 0), 1, LineTypes.AntiAlias);

                            Cv2.PutText(image, i.ToString(), new OpenCvSharp.Point(point.X, point.Y), HersheyFonts.HersheyPlain,
                                        0.6, new Scalar(0, 255, 0));
                            i++;
                        }
                        break;
                }

                lock (lock_out_mat)
                {
                    if (out_mat[threadNo].IsEnabledDispose)
                    {
                        out_mat[threadNo].Dispose();
                    }

                    out_mat[threadNo] = new Mat();

                    lock (lock_imagebytes[threadNo])
                    {
                        out_mat[threadNo] = image.Clone();
                    }
                }

                last_mat = threadNo;
            }
        }
    }

    public enum DetectMode
    {
        OpenCV,
        Dlib5,
        Dlib68,
        Mixed
    }

    public enum SmoothingMethod
    {
        Average,
        LPF
    }
}