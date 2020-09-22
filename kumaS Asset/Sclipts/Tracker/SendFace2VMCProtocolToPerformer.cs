#if uOSC_EXIST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uOSC;
using kumaS.FaceTrack;

namespace kumaS.Tracker
{
    public class SendFace2VMCProtocolToPerformer : SendDataBase
    {
        private uOscClient cli = default;
        private FaceTracking face = default;
        private Vector3 offset = default;

        public SendFace2VMCProtocolToPerformer(FaceTracking tracker, uOscClient client)
        {
            cli = client;
            face = tracker;
        }

        public override void Send()
        {
            if (cli && face)
            {
                SendHeadInfo(face.Position + offset, face.Rotation);

                if(face.mode == DetectMode.Dlib68 || face.mode == DetectMode.Mixed)
                {
                    if (face.Eye_tracking)
                    {
                        SendEyeLookPosition(CalcEyeLookPosition(face.LeftEyeRotation, face.RightEyeRotation));
                    }

                    if (face.Blink_tracking)
                    {
                        SendBlendShape("Blink_L", face.LeftEyeCloseness);
                        SendBlendShape("Blink_R", face.RightEyeCloseness);
                        SendBlendShapeApply();
                    }
                }
            }
        }

        private void SendBlendShape(string name, float val)
        {
            cli.Send("/VMC/Ext/Blend/Val", name, val);
        }

        private void SendBlendShapeApply()
        {
            cli.Send("/VMC/Ext/Blend/Apply");
        }

        private Vector3 CalcEyeLookPosition(Vector3 left, Vector3 right)
        {
            return new Vector3(5 * Mathf.Tan((left.y + right.y) / 2 * Mathf.Deg2Rad), -5 * Mathf.Tan((left.x + right.x) / 2 * Mathf.Deg2Rad), 5);
        }

        private void SendEyeLookPosition(Vector3 position)
        {
            cli.Send("/VMC/Ext/Set/Eye", 1, position.x, position.y, position.z);
        }

        private void SendHeadInfo(Vector3 pos, Quaternion rot)
        {
            cli.Send("VMC/Ext/Tra/Pos", "kumaS_VirtualTracker_7", pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);
        }

        public override void SetOffsets(Vector3[] offsets)
        {
            offset = offsets[0];
        }
    }
}

#endif