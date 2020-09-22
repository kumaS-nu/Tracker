
#if LIVE2D_EXIST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Core;

namespace kumaS.FaceTrack
{
    [RequireComponent(typeof(FaceTracking))]
    public partial class ApplyToLive2D : MonoBehaviour
    {
        private FaceTracking tracking;
        private Dictionary<cuParam, CubismParameter> cubismparams = new Dictionary<cuParam, CubismParameter>();
        public CubismModel model;
        public string ParamAngleX;
        public string ParamAngleY;
        public string ParamAngleZ;
        public string ParamEyeLOpen;
        public string ParamEyeROpen;
        public string ParamEyeBallX;
        public string ParamEyeBallY;
        public string ParamBodyAngleX;
        public string ParamBodyAngleY;
        public string ParamBodyAngleZ;

        private float rot_x;
        private float rot_y;
        private float rot_z;

        private bool ready = false;

        void Start()
        {
            tracking = gameObject.GetComponent<FaceTracking>();
            cubismparams.Add(cuParam.H_AngleX, model.Parameters.FindById(ParamAngleX));
            cubismparams.Add(cuParam.H_AngleY, model.Parameters.FindById(ParamAngleY));
            cubismparams.Add(cuParam.H_AngleZ, model.Parameters.FindById(ParamAngleZ));
            cubismparams.Add(cuParam.EyeLOpen, model.Parameters.FindById(ParamEyeLOpen));
            cubismparams.Add(cuParam.EyeROpen, model.Parameters.FindById(ParamEyeROpen));
            cubismparams.Add(cuParam.EyeBallX, model.Parameters.FindById(ParamEyeBallX));
            cubismparams.Add(cuParam.EyeBallY, model.Parameters.FindById(ParamEyeBallY));
            cubismparams.Add(cuParam.B_AngleX, model.Parameters.FindById(ParamBodyAngleX));
            cubismparams.Add(cuParam.B_AngleY, model.Parameters.FindById(ParamBodyAngleY));
            cubismparams.Add(cuParam.B_AngleZ, model.Parameters.FindById(ParamBodyAngleZ));

            ready = true;
        }

        void LateUpdate()
        {
            if (ready)
            {
                rot_x = Resharp(tracking.Rotation.eulerAngles.x);
                rot_y = Resharp(tracking.Rotation.eulerAngles.y);
                rot_z = Resharp(tracking.Rotation.eulerAngles.z);
                cubismparams[cuParam.H_AngleX].Value = rot_y;
                cubismparams[cuParam.H_AngleY].Value = -rot_x;
                cubismparams[cuParam.H_AngleZ].Value = -rot_z;
                cubismparams[cuParam.EyeLOpen].Value = 1.0f - tracking.LeftEyeCloseness;
                cubismparams[cuParam.EyeROpen].Value = 1.0f - tracking.RightEyeCloseness;
                cubismparams[cuParam.EyeBallX].Value = (tracking.LeftEyeRotation.y + tracking.RightEyeRotation.y) / 2.0f / 10.0f;
                cubismparams[cuParam.EyeBallY].Value = (tracking.LeftEyeRotation.x + tracking.RightEyeRotation.x) / 2.0f / 5.0f;
                cubismparams[cuParam.B_AngleX].Value = rot_y / 10.0f;
                cubismparams[cuParam.B_AngleY].Value = Mathf.Atan2(-tracking.Position.z, 0.7f) * Mathf.Rad2Deg;
                cubismparams[cuParam.B_AngleZ].Value = -Mathf.Atan2(-tracking.Position.x, 0.7f) * Mathf.Rad2Deg;
            }
        }

        private float Resharp(float val)
        {
            if(val > 90)
            {
                val -= 360;
            }

            if(val < -90)
            {
                val += 360;
            }

            return val;
        }

        enum cuParam
        {
            H_AngleX,
            H_AngleY,
            H_AngleZ,
            EyeLOpen,
            EyeROpen,
            EyeBallX,
            EyeBallY,
            B_AngleX,
            B_AngleY,
            B_AngleZ
        }
    }
}

#endif