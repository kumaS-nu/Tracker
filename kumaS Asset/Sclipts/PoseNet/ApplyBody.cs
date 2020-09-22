using System.Collections.Generic;
using UnityEngine;

namespace kumaS.PoseNet
{
    [RequireComponent(typeof(BodyTracking))]
    public class ApplyBody : MonoBehaviour
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
        }


        void Update()
        {
            if (body == null) return;

            tmp = body.Position;
            tmp.y = 0;
            Position.localPosition = tmp;
            tmp = Vector3.zero;
            tmp.y = body.Position.y;
            Bip_C_Hips.localPosition = tmp;
            Bip_C_Hips.rotation = body.Rotation[0];
            var local = (body.Rotation[1] * Quaternion.Inverse(body.Rotation[0])).eulerAngles;
            if (Mathf.Abs(local.x) < 90 && Mathf.Abs(local.y) < 90 && Mathf.Abs(local.z) < 90)
            {
                Bip_C_Head.rotation = body.Rotation[1];
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
