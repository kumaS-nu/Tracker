
#if uOSC_EXIST && VRM_EXIST

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using kumaS.FaceTrack;
using VRM;
using uOSC;
using kumaS.PoseNet;

namespace kumaS.Tracker
{
    public class Send2VMCProtocolToMarionette : SendDataBase
    {
        private GameObject _model = default;
        private Animator animator = default;
        private VRMBlendShapeProxy blendShapeProxy = default;
        private uOscClient _client = default;

        public Send2VMCProtocolToMarionette(uOscClient client, GameObject model, SendData type)
        {
            _client = client;
            _model = model;
            switch (type) {
                case SendData.Body:
                    var i = UnityEngine.Object.FindObjectsOfType<ApplyBody>();
                    if(i == null || !i.Select(obj => obj.isActiveAndEnabled).Contains(true))
                    {
                        throw new Exception("ApplyBody is not working.");
                    }
                    break;
                case SendData.Face:
                    var j = UnityEngine.Object.FindObjectsOfType<ApplyToVRM>();
                    if (j == null || !j.Select(obj => obj.isActiveAndEnabled).Contains(true))
                    {
                        throw new Exception("ApplyToVRM is not working.");
                    }
                    break;
                case SendData.Both:
                    var k = UnityEngine.Object.FindObjectsOfType<ApplyComposite>();
                    if (k == null || !k.Select(obj => obj.isActiveAndEnabled).Contains(true))
                    {
                        throw new Exception("ApplyComposite is not working.");
                    }
                    break;
            }

            animator = _model.GetComponent<Animator>();
            blendShapeProxy = _model.GetComponent<VRMBlendShapeProxy>();
            if(animator == null)
            {
                throw new Exception("model に Animator がありません。\nThere is no Animator in the model.");
            }

            if(blendShapeProxy == null)
            {
                throw new Exception("model に VRMBlendShapeProxy がありません。\nThere is no VRMBlendShapeProxy in the model.");
            }
        }

        public override void Send()
        {
            if (_model != null && animator != null && _client != null)
            {
                _client.Send("/VMC/Ext/Root/Pos",
                        "root",
                        _model.transform.position.x, _model.transform.position.y, _model.transform.position.z,
                        _model.transform.rotation.x, _model.transform.rotation.y, _model.transform.rotation.z, _model.transform.rotation.w);

                //Bones
                foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (bone != HumanBodyBones.LastBone)
                    {
                        var Transform = animator.GetBoneTransform(bone);
                        if (Transform != null)
                        {
                            _client.Send("/VMC/Ext/Bone/Pos",
                                bone.ToString(),
                                Transform.localPosition.x, Transform.localPosition.y, Transform.localPosition.z,
                                Transform.localRotation.x, Transform.localRotation.y, Transform.localRotation.z, Transform.localRotation.w);
                        }
                    }
                }

                if (blendShapeProxy != null)
                {
                    foreach (var b in blendShapeProxy.GetValues())
                    {
                        _client.Send("/VMC/Ext/Blend/Val",
                            b.Key.ToString(),
                            (float)b.Value
                            );
                    }
                    _client.Send("/VMC/Ext/Blend/Apply");
                }

                _client.Send("/VMC/Ext/OK", 1);
            }
            else
            {
                _client.Send("/VMC/Ext/OK", 0);
            }
            _client.Send("/VMC/Ext/T", Time.time);
        }

        public override void SetOffsets(Vector3[] offsets)
        {
            throw new System.NotImplementedException();
        }
    }
}


#endif