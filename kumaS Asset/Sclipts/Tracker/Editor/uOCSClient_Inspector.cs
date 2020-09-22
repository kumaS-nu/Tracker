
#if uOSC_EXIST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using uOSC;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace kumaS.Tracker
{
    [CustomEditor(typeof(uOscClient))]
    public class uOCSClient_Inspector : Editor
    {
        private int[] ports = new int[] { 39570, 39539, 39540 };

        private void OnEnable()
        {
            if(!ports.Contains(serializedObject.FindProperty("port").intValue))
            {
                serializedObject.FindProperty("port").intValue = 39570;
            }
            BridgeVirtualTrackerAnduOCSClient.Function += Modify;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            BridgeVirtualTrackerAnduOCSClient.Function -= Modify;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }

        private void Modify(GameObject tar, SendTo to)
        {
            if(tar == ((uOscClient)target).gameObject)
            {
                switch (to) {
                    case SendTo.VMT:
                        serializedObject.FindProperty("port").intValue = 39570; break;
                    case SendTo.VMCProtocolToMarionette:
                        serializedObject.FindProperty("port").intValue = 39539; break;
                    case SendTo.VMCProtocolToPerformer:
                        serializedObject.FindProperty("port").intValue = 39540; break;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }


    public static class BridgeVirtualTrackerAnduOCSClient {
        public delegate void Applyer(GameObject tar,SendTo to);
        public static event Applyer Function;

        public static void Apply(GameObject tar, SendTo to)
        {
            Function(tar, to);
        }
    } 
}

#endif