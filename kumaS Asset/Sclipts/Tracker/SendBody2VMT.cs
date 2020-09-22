
#if uOSC_EXIST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uOSC;
using kumaS.PoseNet;

namespace kumaS.Tracker
{
    public class SendBody2VMT : SendDataBase
    {
        private BodyTracking body = default;
        private uOscClient cli = default;
        private Quaternion[] rotation = new Quaternion[10];
        private Vector3[] pos = new Vector3[17];
        private Vector3[] position = new Vector3[10];

        private Vector3 offset = default;
        private Vector3[] offset_each = new Vector3[10];

        public SendBody2VMT(BodyTracking tracker, uOscClient client)
        {
            body = tracker;
            cli = client;
        }

        public override void Send()
        {
            if (body && cli)
            {
                pos = body.Pose3d;
                RotationMaker();
                PositionMaker();

                for (int i = 0; i < 10; i++)
                {
                    SendContent(i, position[i] + offset + offset_each[i], rotation[i]);
                }

            }
        }

        public override void SetOffsets(Vector3[] offsets)
        {
            offset = offsets[0];
            for(int i = 1; i < 11; i++)
            {
                offset_each[i - 1] = offsets[i];
            }
        }

        void PositionMaker()
        {
            position[0] = body.Pose3d[15];
            position[1] = body.Pose3d[16];
            position[2] = (body.Pose3d[11] * 3 + body.Pose3d[12] * 3 + body.Pose3d[5] + body.Pose3d[6]) / 8;
            position[3] = body.Pose3d[7];
            position[4] = body.Pose3d[8];
            position[5] = body.Pose3d[13];
            position[6] = body.Pose3d[14];
            position[7] = body.Pose3d[0];
            position[8] = body.Pose3d[9];
            position[9] = body.Pose3d[10];
        }

        void RotationMaker()
        {
            rotation[0] = Quaternion.LookRotation(Vector3.up, Vector3.Cross(Vector3.up, Vector3.Cross(pos[15] - pos[13], pos[11] - pos[13])));
            rotation[1] = Quaternion.LookRotation(Vector3.up, Vector3.Cross(Vector3.up, Vector3.Cross(pos[16] - pos[14], pos[12] - pos[14])));
            rotation[2] = body.Rotation[0];
            rotation[3] = Quaternion.LookRotation(pos[7] * 2 - pos[9] - pos[5], Vector3.Cross(pos[7] * 2 - pos[9] - pos[5], Vector3.Cross(pos[9] - pos[7], pos[5] - pos[7])));
            rotation[4] = Quaternion.LookRotation(pos[8] * 2 - pos[10] - pos[6], Vector3.Cross(pos[8] * 2 - pos[10] - pos[6], Vector3.Cross(pos[10] - pos[8], pos[6] - pos[8])));
            rotation[5] = Quaternion.LookRotation(pos[13] * 2 - pos[15] - pos[11], Vector3.Cross(pos[13] * 2 - pos[15] - pos[11], Vector3.Cross(pos[15] - pos[13], pos[11] - pos[13])));
            rotation[6] = Quaternion.LookRotation(pos[14] * 2 - pos[16] - pos[12], Vector3.Cross(pos[14] * 2 - pos[16] - pos[12], Vector3.Cross(pos[16] - pos[14], pos[12] - pos[14])));

            var local = (body.Rotation[1] * Quaternion.Inverse(body.Rotation[0])).eulerAngles;
            if (Mathf.Abs(local.x) < 90 && Mathf.Abs(local.y) < 90 && Mathf.Abs(local.z) < 90)
            {
                rotation[7] = body.Rotation[1];
            }

            rotation[8] = Quaternion.LookRotation(Vector3.Cross(pos[9] - pos[7], Vector3.Cross(pos[9] - pos[7], pos[5] - pos[7])), pos[7] - pos[9]);
            rotation[9] = Quaternion.LookRotation(Vector3.Cross(pos[10] - pos[8], Vector3.Cross(pos[10] - pos[8], pos[6] - pos[8])), pos[8] - pos[10]);
        }

        void SendContent(int idx, Vector3 pos, Quaternion rot)
        {
            cli.Send("/VMT/Room/Unity", idx, 1, 0f, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);
        }
    }
}

#endif