using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace kumaS
{

    public class ImportedAssetDetector : AssetPostprocessor
    {
        private static string path2applyToLive2D = "Assets/kumaS Asset/Sclipts/FaceTracking/ApplyToLive2D.cs";
        private static string path2applyToVRM = "Assets/kumaS Asset/Sclipts/FaceTracking/ApplyToVRM.cs";
        private static string path2virtualTracker = "Assets/kumaS Asset/Sclipts/PoseNet/VirtualTracker.cs";
        private static string applyToLive2D_GUID = "e43849abd8f5a4d44b4257e36ed47243";
        private static string applyToVRM_GUID = "13dc3b6f1a0133a48a702b98aafcc277";
        private static string virtualTracker_GUID = "22d8765668f09664c9cc46044857e874";
        private static bool haveLive2D = false;
        private static bool haveVRM = false;
        private static bool haveuOSC = false;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            SetCorrectDefine();
        }

        [InitializeOnLoadMethod]
        static void SetCorrectDefine()
        {
            bool haveChange = false;
            var folders = Directory.GetDirectories(Application.dataPath);
            var directorys = folders.Select((name) => name.Replace(Application.dataPath, "").Replace("\\", "").Replace("/", ""));

            haveLive2D = directorys.Contains("Live2D") ? true : false;
            haveVRM = directorys.Contains("VRM") ? true : false;
            haveuOSC = directorys.Contains("uOSC") ? true : false;

            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';').ToList();

            if (haveLive2D)
            {
                if (!symbols.Exists(str => str == "LIVE2D_EXIST"))
                {
                    symbols.Add("LIVE2D_EXIST");
                    haveChange = true;
                }
            }
            else
            {
                if (symbols.Exists(str => str == "LIVE2D_EXIST"))
                {
                    symbols.Remove("LIVE2D_EXIST");
                    haveChange = true;
                }
            }

            if (haveVRM)
            {
                if (!symbols.Exists(str => str == "VRM_EXIST"))
                {
                    symbols.Add("VRM_EXIST");
                    haveChange = true;
                }
            }
            else
            {
                if (symbols.Exists(str => str == "VRM_EXIST"))
                {
                    symbols.Remove("VRM_EXIST");
                    haveChange = true;
                }
            }

            if (haveuOSC)
            {
                if (!symbols.Exists(str => str == "uOSC_EXIST"))
                {
                    symbols.Add("uOSC_EXIST");
                    haveChange = true;
                }
            }
            else
            {
                if (symbols.Exists(str => str == "uOSC_EXIST"))
                {
                    symbols.Remove("uOSC_EXIST");
                    haveChange = true;
                }
            }

            if (haveChange)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, string.Join(";", symbols));
            }

        }


        [InitializeOnLoadMethod]
        static void ValidStarter()
        {
            applyToLive2D_GUID = AssetDatabase.AssetPathToGUID(path2applyToLive2D);
            applyToVRM_GUID = AssetDatabase.AssetPathToGUID(path2applyToVRM);
            virtualTracker_GUID = AssetDatabase.AssetPathToGUID(path2virtualTracker);
            EditorApplication.projectWindowItemOnGUI += ScriptValidate;
        }

        static void ScriptValidate(string guid, Rect point)
        {
            var e = Event.current;

            if (e.type == EventType.DragExited)
            {

                if (!haveLive2D && DragAndDrop.paths.First() == path2applyToLive2D && guid == applyToLive2D_GUID)
                {
                    DisplayDialoger.DisplayDialog(
                    "Live2DのSDKが導入されていないので利用できません。\nThis is not available, because you are not import the Live2D SDK."
                    );
                }

                if (!haveVRM && DragAndDrop.paths.First() == path2applyToVRM && guid == applyToVRM_GUID)
                {
                    DisplayDialoger.DisplayDialog(
                    "VRMのSDKが導入されていないので利用できません。\nThis is not available, because you are not import the VRM SDK."
                    );
                }

                if (!haveuOSC && DragAndDrop.paths.First() == path2virtualTracker && guid == virtualTracker_GUID)
                {
                    DisplayDialoger.DisplayDialog(
                    "uOSCが導入されていないので利用できません。\nThis is not available, because you are not import the uOSC."
                    );
                }
            }
        }

    }

    public class Reseter : EditorWindow
    {

        [MenuItem("Tools/kumaS Asset/Reset")]
        private static void Open()
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, string.Empty);
        }

    }


    [InitializeOnLoad]
    static class DisplayDialoger
    {
        private static bool on = false;
        private static string msg = "";

        static DisplayDialoger()
        {
            EditorApplication.update += Loop;
        }

        private static async void Loop()
        {
            if (on)
            {
                MousePInvok.mouse_event(MousePInvok.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                on = false;
                await Task.Delay(50);
                EditorUtility.DisplayDialog("エラー", msg, "OK");
                msg = "";
            }
        }

        public static void DisplayDialog(string message)
        {
            msg = message;
            on = true;
        }
    }

    public class MousePInvok
    {
        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public static readonly int MOUSEEVENTF_LEFTUP = 0x4;
    }
}
