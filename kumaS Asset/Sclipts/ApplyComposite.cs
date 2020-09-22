
#if VRM_EXIST

using System.Collections.Generic;
using kumaS.FaceTrack;
using kumaS.PoseNet;
using UnityEngine;
using VRM;

namespace kumaS
{
    [RequireComponent(typeof(BodyTracking))]
    [RequireComponent(typeof(FaceTracking))]
    public class ApplyComposite: MonoBehaviour
    {
        private BodyTracking body;
        public Transform Position;
        public Transform Bip_C_Hips;
        public Transform Bip_C_Head;
        public Transform Bip_R_UpperArm;
        public Transform Bip_R_LowerArm;
        public Transform Bip_L_UpperArm;
        public Transform Bip_L_LowerArm;
        public Transform Bip_R_UpperLeg;
        public Transform Bip_R_LowerLeg;
        public Transform Bip_L_UpperLeg;
        public Transform Bip_L_LowerLeg;
        private VRMBlendShapeProxy proxy;
        private FaceTracking tracking;
        public Transform left_eye;
        public Transform right_eye;

        private Vector3 tmp;
        private Transform neck = default;
        private Transform R_shoulder = default;
        private Transform L_shoulder = default;

        void Start()
        {
            R_shoulder = Bip_R_UpperArm.GetComponentInParent<Transform>();
            L_shoulder = Bip_L_UpperArm.GetComponentInParent<Transform>();
            neck = Bip_C_Head.GetComponentInParent<Transform>();
            body = GetComponent<BodyTracking>();
            tracking = gameObject.GetComponent<FaceTrack.FaceTracking>();
        }


        void Update()
        {
            if (body == null) return;

            if (!proxy)
            {
                proxy = tracking.model.gameObject.GetComponent<VRMBlendShapeProxy>();
            }
            else
            {
                proxy.ImmediatelySetValue(BlendShapePreset.Blink_L, tracking.LeftEyeCloseness);
                proxy.ImmediatelySetValue(BlendShapePreset.Blink_R, tracking.RightEyeCloseness);
            }

            transform.position = tracking.Position;
            transform.rotation = tracking.Rotation;
            left_eye.localEulerAngles = tracking.LeftEyeRotation;
            right_eye.localEulerAngles = tracking.RightEyeRotation;

            tmp = body.Position;
            tmp.y = 0;
            Position.localPosition = tmp;
            tmp = Vector3.zero;
            tmp.y = body.Position.y;
            Bip_C_Hips.localPosition = tmp;
            Bip_C_Hips.rotation = body.Rotation[0];
            if (tracking.IsSuccess)
            {
                switch (tracking.mode)
                {
                    case DetectMode.Dlib5:
                        var local = (body.Rotation[1] * Quaternion.Inverse(body.Rotation[0])).eulerAngles;
                        Vector3 rot = default; 
                        if (Mathf.Abs(local.x) < 90 && Mathf.Abs(local.y) < 90 && Mathf.Abs(local.z) < 90)
                        {
                            rot = body.Rotation[1].eulerAngles;
                        }

                        rot.z = tracking.Rotation.eulerAngles.z;
                        Bip_C_Head.rotation = Quaternion.Euler(rot);
                        break;

                    case DetectMode.Dlib68:
                    case DetectMode.Mixed:
                        Bip_C_Head.rotation = tracking.Rotation;
                        break;
                }
                Bip_C_Head.rotation = tracking.Rotation;
            }
            else
            {
                var local = (body.Rotation[1] * Quaternion.Inverse(body.Rotation[0])).eulerAngles;
                if (Mathf.Abs(local.x) < 90 && Mathf.Abs(local.y) < 90 && Mathf.Abs(local.z) < 90)
                {
                    Bip_C_Head.rotation = body.Rotation[1];
                }
            }

            Bip_R_UpperArm.rotation = body.Rotation[2];
            Bip_L_UpperArm.rotation = body.Rotation[3];
            Bip_R_LowerArm.rotation = body.Rotation[4];
            Bip_L_LowerArm.rotation = body.Rotation[5];
            Bip_R_UpperLeg.rotation = body.Rotation[6];
            Bip_L_UpperLeg.rotation = body.Rotation[7];
            Bip_R_LowerLeg.rotation = body.Rotation[8];
            Bip_L_LowerLeg.rotation = body.Rotation[9];
        }
    }
}

#endif