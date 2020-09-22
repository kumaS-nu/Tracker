using System;
using UnityEngine;

namespace kumaS.PoseNet
{
    public partial class BodyTracking
    {
        /// <summary>
        /// 異常値をはじく。        Abnormal values are repelled.
        /// </summary>
        /// <param name="data3ds">対象の数値。        Target value.</param>
        /// <returns>正常な値。                       Normal values.</returns>
        private Vector3[] ReSharp3d(Vector3[] data3ds)
        {
            Vector3[] before;
            lock (pose3d_chain)
            {
                if(Math.Abs(pose3d_chain.Last.Value[0].y - nose.y) < 0.0001f)
                {
                    return data3ds;
                }
                before = pose3d_chain.Last.Value;
            }

            if (float.IsNaN(data3ds[11].y))
            {
                return before;
            }

            for (int i = 0; i < 17; i++)
            {
                if ((data3ds[i] - before[i]).sqrMagnitude > max_speeds[i])
                {
                    return before;
                }
            }

            return data3ds;
        }
    }
}