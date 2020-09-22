using UnityEngine;

namespace kumaS.FaceTrack
{
    public partial class FaceTracking
    {
        /// <summary>
        /// 推定した回転に異常がないか検査           check error in estemated rotation
        /// </summary>
        /// <param name="check">チェックされる対象   checking Vecter3</param>
        /// <param name="range">動ける範囲           moveable range</param>
        /// <param name="root">前回の回転            before rotation</param>
        /// <returns>チェック後の回転。エラーの場合前のものが出される。      rotation after checked. If error exist, return before Vector3.</returns>
        private Vector3 EstimateErrCheck(Vector3 check, Vector3 range, Vector3 root)
        {

            var delta = check - root;

            if (root == Vector3.zero)
            {
                return check;
            }


            if (Mathf.Abs(delta.x) > range.x * 3)
            {
                check.x = root.x;
            }
            else if (delta.x > range.x)
            {
                check.x = root.x + range.x;
            }
            else if (-delta.x > range.x)
            {
                check.x = root.x - range.x;
            }
            else if (delta.x < range.x * 0.05)
            {
                check.x = root.x;
            }

            if (Mathf.Abs(delta.y) > range.y * 3)
            {
                check.y = root.y;
            }
            else if (delta.y > range.y)
            {
                check.y = root.y + range.y;
            }
            else if (-delta.y > range.y)
            {
                check.y = root.y - range.y;
            }
            else if (delta.y < range.y * 0.05)
            {
                check.y = root.y;
            }

            if (Mathf.Abs(delta.z) > range.z * 3)
            {
                check.z = root.z;
            }
            else if (delta.z > range.z)
            {
                check.z = root.z + range.z;
            }
            else if (-delta.z > range.z)
            {
                check.z = root.z - range.z;
            }
            else if (delta.z < range.z * 0.05)
            {
                check.z = root.z;
            }
            return check;

        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 推定した位置に異常がないか検査           check error in estimated position
        /// </summary>
        /// <param name="check">チェックされる対象   checking Vecter3</param>
        /// <param name="range">動ける範囲           moveable range</param>
        /// <param name="root">前回の位置            before position</param>
        /// <returns>チェック後の位置。エラーの場合前のものが出される。      position after checked. If error exist, return before Vector3.</returns>
        private Vector3 EstimateErrCheck(Vector3 check, float dist, Vector3 root)
        {
            var delta = check - root;

            if (root == Vector3.zero)
            {
                return check;
            }
            if (delta.sqrMagnitude > dist * dist * 3)
            {
                check = root;
            }
            else if (delta.sqrMagnitude > dist * dist)
            {
                check = root + delta.normalized * dist;
            }
            else if (delta.sqrMagnitude < dist * dist * 0.05)
            {
                check = root;
            }

            return check;
        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 回転を正しく直す        resharpen rotation
        /// </summary>
        /// <param name="rotSample">回転      rotation</param>
        /// <param name="posSample">位置      position</param>
        /// <returns>直された後の回転         reshaped rotation</returns>
        private Vector3 RotReSharp(Vector3 rotSample, Vector3 posSample)
        {
            if (rotSample.x > 270)
            {
                rotSample.x -= 360;
            }
            else if (rotSample.x < -270)
            {
                rotSample.x += 360;
            }

            if (rotSample.y > 270)
            {
                rotSample.y -= 360;
            }
            else if (rotSample.y < -270)
            {
                rotSample.y += 360;
            }

            if (rotSample.z > 270)
            {
                rotSample.z -= 360;
            }
            else if (rotSample.z < -270)
            {
                rotSample.z += 360;
            }
            else if (rotSample.z > 90)
            {
                rotSample.z -= 180;
                posSample.x *= -1;
                posSample.y *= -1;
                posSample.z *= -1;

            }
            else if (rotSample.z < -90)
            {
                rotSample.z += 180;
                posSample.x *= -1;
                posSample.y *= -1;
                posSample.z *= -1;
            }

            return rotSample;
        }

        //--------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 範囲を飛び出してないかチェック     range check
        /// </summary>
        /// <param name="check">チェックされる位置       checking position</param>
        /// <param name="rad">動かせる範囲               range can move</param>
        /// <returns>チェック後の位置                    checked position</returns>
        private Vector3 RangeCheck(Vector3 check, float rad)
        {
            if (check.sqrMagnitude > rad * rad)
            {
                check = check.normalized * rad;
            }

            return check;
        }
        private Vector3 RangeCheck(Vector3 check, Vector3 range)
        {
            if (Mathf.Abs(check.x) > range.x)
            {
                if (check.x > 0) { check.x = range.x; } else { check.x = -range.x; }
            }

            if (Mathf.Abs(check.y) > range.y)
            {
                if (check.y > 0) { check.y = range.y; } else { check.y = -range.y; }
            }

            if (Mathf.Abs(check.z) > range.z)
            {
                if (check.z > 0) { check.z = range.z; } else { check.z = -range.z; }
            }

            return check;
        }

    }
}