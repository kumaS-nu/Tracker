
#if VRM_EXIST

using UnityEngine;
using VRM;

namespace kumaS.FaceTrack
{
    [RequireComponent(typeof(FaceTracking))]
    public class ApplyToVRM : MonoBehaviour
    {
        private VRMBlendShapeProxy proxy;
        private FaceTracking tracking;
        public Transform left_eye;
        public Transform right_eye;

        void Start()
        {
            tracking = gameObject.GetComponent<FaceTracking>();
        }

        void Update()
        {
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
        }
    }
}

#endif