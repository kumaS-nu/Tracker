using OpenCvSharp;
using System;
using UnityEngine;

namespace kumaS.FaceTrack
{
    public partial class FaceTracking
    {
        /// <summary>
        /// 目の向き推定の計算部分
        /// </summary>
        /// <param name="points">目のPoint</param>
        /// <param name="rect">使ったRect</param>
        /// <param name="x">目の中心点x</param>
        /// <param name="y">目の中心点y</param>
        /// <param name="left">左目か右目か</param>
        /// <returns>計算された回転</returns>
        private Vector3 CalcEyeRot(DlibDotNet.Point[] points, OpenCvSharp.Point rect, float eye_x, float eye_y, bool left)
        {
            Vector2 atob;
            Vector2 atop;
            Vector2 atoc;
            Vector2 atod;
            float xmax, xmin, yin, yout;

            if (left)
            {
                atob = new Vector2((points[3] - points[0]).X, (points[3] - points[0]).Y);
                atop = new Vector2(eye_x + rect.X - points[0].X, eye_y + rect.Y - points[0].Y);
                atoc = new Vector2((points[1] - points[0]).X, (points[1] - points[0]).Y);
                atod = new Vector2((points[5] - points[0]).X, (points[5] - points[0]).Y);
                xmax = left_eye_range_high.x;
                xmin = left_eye_range_low.x;
                yin = left_eye_range_high.y;
                yout = left_eye_range_low.y;
            }
            else
            {
                atob = new Vector2((points[0] - points[3]).X, (points[0] - points[3]).Y);
                atop = new Vector2(eye_x + rect.X - points[3].X, eye_y + rect.Y - points[3].Y);
                atoc = new Vector2((points[2] - points[3]).X, (points[2] - points[3]).Y);
                atod = new Vector2((points[4] - points[3]).X, (points[4] - points[3]).Y);
                xmax = right_eye_range_high.x;
                xmin = right_eye_range_low.x;
                yin = right_eye_range_low.y;
                yout = right_eye_range_high.y;
            }

            float x, y;

            if ((eye_x - points[1].X) * (eye_x - points[1].X) + (eye_y - points[1].Y) * (eye_y - points[1].Y) <
                (eye_x - points[5].X) * (eye_x - points[5].X) + (eye_y - points[5].Y) * (eye_y - points[5].Y))
            {
                x = (atop - atob.normalized * Vector2.Dot(atop, atob) / atob.magnitude).magnitude
                    / (atoc - atob.normalized * Vector2.Dot(atoc, atob) / atob.magnitude).magnitude * xmin;

            }
            else
            {
                x = (atop - atob.normalized * Vector2.Dot(atop, atob) / atob.magnitude).magnitude
                    / (atod - atob.normalized * Vector2.Dot(atod, atob) / atob.magnitude).magnitude * xmax;
            }

            float len = (Vector2.Dot(atop, atob) / atob.sqrMagnitude) - eye_center;

            if (debug_eye_center_ratio)
            {
                if (left)
                {
                    Debug.Log("left eye ratio" + Vector2.Dot(atob, atop) / atob.sqrMagnitude);
                }
                else
                {
                    Debug.Log("right eye ratio" + Vector2.Dot(atob, atop) / atob.sqrMagnitude);
                }
            }

            if (len < 0)
            {
                y = -len / eye_center * yin;
            }
            else
            {
                y = len / (1 - eye_center) * yout;
            }

            if (left)
            {
                x *= eye_rot_sensitivity_L.x;

                if (x < left_eye_range_low.x)
                {
                    x = left_eye_range_low.x;
                }
                else if (x > left_eye_range_high.x)
                {
                    x = left_eye_range_high.x;
                }

                y *= eye_rot_sensitivity_L.y;

                if (y < left_eye_range_low.y)
                {
                    y = left_eye_range_low.y;
                }
                else if (y > left_eye_range_high.y)
                {
                    y = left_eye_range_high.y;
                }
            }
            else
            {
                x *= eye_rot_sensitivity_R.x;

                if (x < right_eye_range_low.x)
                {
                    x = right_eye_range_low.x;
                }
                else if (x > right_eye_range_high.x)
                {
                    x = right_eye_range_high.x;
                }

                y *= eye_rot_sensitivity_R.y;
                if (y < right_eye_range_low.y)
                {
                    y = right_eye_range_low.y;
                }
                else if (y > right_eye_range_high.y)
                {
                    y = right_eye_range_high.y;
                }
            }

            return new Vector3(x, y);
        }

        //--------------------------------------------------------------------------------------------------------
        private float Distance(DlibDotNet.Point point1, DlibDotNet.Point point2)
        {
            return Mathf.Sqrt((point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y));
        }


        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 回転行列からクォータニオンに変換    Convert rotation matrix to quaternion
        /// </summary>  
        /// <param name="projmat">回転行列      rotation matrix</param>
        /// <returns>クォータニオン             quaternion</returns>
        private Quaternion RotMatToQuatanion(double[] projmat)
        {
            Quaternion quaternion = new Quaternion();
            double[] elem = new double[4]; // 0:x, 1:y, 2:z, 3:w
            elem[0] = projmat[0] - projmat[4] - projmat[8] + 1.0f;
            elem[1] = -projmat[0] + projmat[4] - projmat[8] + 1.0f;
            elem[2] = -projmat[0] - projmat[4] + projmat[8] + 1.0f;
            elem[3] = projmat[0] + projmat[4] + projmat[8] + 1.0f;

            uint biggestIndex = 0;
            for (uint i = 1; i < 4; i++)
            {
                if (elem[i] > elem[biggestIndex])
                {
                    biggestIndex = i;
                }
            }

            if (elem[biggestIndex] < 0.0f)
            {
                return quaternion;
            }

            float v = (float)Math.Sqrt(elem[biggestIndex]) * 0.5f;
            float mult = 0.25f / v;

            switch (biggestIndex)
            {
                case 0:
                    quaternion.x = v;
                    quaternion.y = (float)(projmat[1] + projmat[3]) * mult;
                    quaternion.z = (float)(projmat[6] + projmat[2]) * mult;
                    quaternion.w = (float)(projmat[5] - projmat[7]) * mult;
                    break;
                case 1:
                    quaternion.x = (float)(projmat[1] + projmat[3]) * mult;
                    quaternion.y = v;
                    quaternion.z = (float)(projmat[5] + projmat[7]) * mult;
                    quaternion.w = (float)(projmat[6] - projmat[2]) * mult;
                    break;
                case 2:
                    quaternion.x = (float)(projmat[6] + projmat[2]) * mult;
                    quaternion.y = (float)(projmat[5] + projmat[7]) * mult;
                    quaternion.z = v;
                    quaternion.w = (float)(projmat[1] - projmat[3]) * mult;
                    break;
                case 3:
                    quaternion.x = (float)(projmat[5] - projmat[7]) * mult;
                    quaternion.y = (float)(projmat[6] - projmat[2]) * mult;
                    quaternion.z = (float)(projmat[1] - projmat[3]) * mult;
                    quaternion.w = v;
                    break;
            }

            return quaternion;
        }
    }
}