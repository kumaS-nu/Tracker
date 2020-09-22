
#if uOSC_EXIST

using System;
using System.Collections;
using System.Collections.Generic;
using kumaS.FaceTrack;
using kumaS.PoseNet;
using Live2D.Cubism.Framework.Json;
using UnityEngine;
using uOSC;

namespace kumaS.Tracker
{
    [RequireComponent(typeof(uOscClient))]
    [RequireComponent(typeof(uOscServer))]
    public class VirtualTracker : MonoBehaviour
    {
        [SerializeField]
        private BodyTracking body = default;
        [SerializeField]
        private FaceTracking face = default;
        [SerializeField]
        private GameObject model = default;
        [SerializeField]
        private SendTo to = SendTo.VMT;
        [SerializeField]
        private SendData data = SendData.Body;

        private uOscClient client = default;
        private uOscServer server = default;

        private SendDataBase sender = default;

        public Vector3 offset;
        public Vector3 offset_right_leg;
        public Vector3 offset_left_leg;
        public Vector3 offset_abdomen;
        public Vector3 offset_right_elbow;
        public Vector3 offset_left_elbow;
        public Vector3 offset_right_knee;
        public Vector3 offset_left_knee;
        public Vector3 offset_head;
        public Vector3 offset_right_wrist;
        public Vector3 offset_left_wrist;

        void Start()
        {
            client = GetComponent<uOscClient>();
            server = GetComponent<uOscServer>();
            switch ((int)to * 3 + (int)data)
            {
                case 0:
                    if (!body)
                    {
                        Quite("Body Tracking is null!");
                    }
                    sender = new SendBody2VMT(body, client);
                    sender.SetOffsets(GetOffsets());
                    break;

                case 3:
                    if (!body)
                    {
                        Quite("Body Tracking is null!");
                    }
                    sender = new SendBody2VMCProtocolToPerformer(body, client);
                    sender.SetOffsets(GetOffsets());
                    break;

                case 4:
                    if (!face)
                    {
                        Quite("Face Tracking is null!");
                    }
                    sender = new SendFace2VMCProtocolToPerformer(face, client);
                    sender.SetOffsets(GetOffsets());
                    break;

                case 5:
                    if(!face || !body)
                    {
                        Quite("Face Tracking or Body Tracking is null!");
                    }
                    sender = new SendBoth2VMCProtocolToPerformer(body, face, client);
                    sender.SetOffsets(GetOffsets());
                    break;
#if VRM_EXIST
                case 6:
                    if (!model)
                    {
                        Quite("Target model is null!");
                    }
                    try
                    {
                        sender = new Send2VMCProtocolToMarionette(client, model, SendData.Body);
                    }catch(Exception e)
                    {
                        Quite(e.Message);
                    }break;

                case 7:
                    if (!model)
                    {
                        Quite("Target model is null!");
                    }
                    try
                    {
                        sender = new Send2VMCProtocolToMarionette(client, model, SendData.Face);
                    }
                    catch (Exception e)
                    {
                        Quite(e.Message);
                    }
                    break;

                case 8:
                    if (!model)
                    {
                        Quite("Target model is null!");
                    }
                    try
                    {
                        sender = new Send2VMCProtocolToMarionette(client, model, SendData.Both);
                    }
                    catch (Exception e)
                    {
                        Quite(e.Message);
                    }
                    break;
#endif
            }


        }

        private void Quite(string error)
        {
            Debug.LogError(error);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void Update()
        {
            sender?.Send();
        }

        public void SetOffsets()
        {
            sender?.SetOffsets(GetOffsets());
        }

        private Vector3[] GetOffsets()
        {
            return new Vector3[] { offset, offset_right_leg, offset_left_leg, offset_abdomen, offset_right_elbow, offset_left_elbow, offset_right_knee, offset_left_knee, offset_head, offset_right_wrist, offset_left_wrist };
        }

    }

    public enum SendTo
    {
        VMT,
        VMCProtocolToPerformer,
        VMCProtocolToMarionette
    }

    public enum SendData
    {
        Body,
        Face,
        Both
    }

}

#endif