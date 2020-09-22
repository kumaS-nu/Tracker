using DlibDotNet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace kumaS.FaceTrack
{
    public partial class FaceTracking
    {

#pragma warning disable 0649

        private bool inputR = false;        /// <param name="inputR"> Rボタンが押された     pushed R button</param>
        private bool running = true;        /// <param name="running">  非同期停止用        for stop ansync</param>
        //ロックトークン       locktoken --------------------------------------------------
        private object lock_fps = new object();
        private object lock_pos_rot = new object();
        private object lock_eye_ratio_file = new object();
        private object lock_eye_blink = new object();
        private object lock_eye_rotL = new object();
        private object lock_eye_rotR = new object();
        private object lock_capture = new object();
        private object lock_landmarks = new object();
        private object lock_isSuccess = new object();
        private object[] lock_out_mat;
        private object[] lock_imagebytes;
        //----------------------------------------------------------------------------------
        private int diff_time = 1000 / 60;  /// <param name="diff_time">非同期開始時間のずれ          time of diff async start</param>
        private IntPtr[] ptr;               /// <param name="ptr">カメラに保存された画像のポインタ    pointer of image taken by camera</param>
        //出力        output  --------------------------------------------------------------
        public Vector3 Position { get; private set; } = new Vector3();
        public Quaternion Rotation { get; private set; } = new Quaternion();
        public float LeftEyeCloseness { get; private set; } = default;
        public float RightEyeCloseness { get; private set; } = default;
        public Vector3 LeftEyeRotation { get; private set; } = new Vector3();
        public bool IsSuccess { get; private set; } = false;
        private Vector3 lefteyerotation = default;
        public Vector3 RightEyeRotation { get; private set; } = new Vector3();
        public Vector2[] Landmarks { 
            get
            {
                if (mode != DetectMode.OpenCV)
                {
                    return landmarks;
                }
                else
                {
                    throw new InvalidOperationException("This mode can't use this propaty");
                }
            }
            private set { landmarks = value; } 
        }
        private Vector2[] landmarks;
        private DlibDotNet.Point[] landmark_detection;
        private Vector3 righteyerotation = default;
        private SynchronizationContext _mainContext;
        //ログをcvs出力するときのストリーム        Stream when outputting cvs log --------
        private StreamWriter fps_writer;
        private StreamWriter pos_rot_writer;
        private StreamWriter eye_rot_writer;
        private StreamWriter eye_ratio_writer;
        private StreamWriter final_writer;
        private int last_mat = -1;
        //キャッシュ用の変数         Variable for cache -----------------------------------
        private Vector3 pos;                                                             /// <param name="pos">最終的に計算された位置      Final calculated position</param>
        private Vector3 rot;                                                             /// <param name="rot">最終的に計算された回転      Final calculated rotation</param>
        private LinkedList<Vector3> pos_chain = new LinkedList<Vector3>();               /// <param name="pos_chain">検出位置の配列   Array of detected positions</param>
        private LinkedList<Vector3> rot_chain = new LinkedList<Vector3>();               /// <param name="rot_chain">検出回転の配列   Array of detected rotations</param>
        private LinkedList<float> eye_L = new LinkedList<float>();                       /// <param name="eye_L">左目の開き具合の配列      Array of left eye opening</param>
        private LinkedList<float> eye_R = new LinkedList<float>();                       /// <param name="eye_R">右目の開き具合の配列      Array of right eye opening</param>
        private LinkedList<Vector3> eye_rot_L = new LinkedList<Vector3>();               /// <param name="eye_rot_L">左目の回転の配列      Array of left eye rotation</param>
        private LinkedList<Vector3> eye_rot_R = new LinkedList<Vector3>();               /// <param name="eye_rot_R">右目の回転の配列      Array of right eye rotation</param>
        private LinkedList<DateTime> fin_time = new LinkedList<DateTime>();              /// <param name="fin_time">検出が終わった時間の配列   Array of detect time finished</param>
        private LinkedList<int> elapt_time = new LinkedList<int>();                      /// <param name="elapt_time">検出にかかった時間の配列 Array of detecting time</param>
        private int[] frame_time = new int[7];                                           /// <param name="frame_time">フレームが切り替わる時間の配列 Array of frame time</param>
        private CascadeClassifier cascade;                                               /// <param name="cascade">  カスケード分類機        Cascade classifier</param>
        private Mat[] out_mat;                                                           /// <param name="out_mat">  出力用Mat               output Mat
        private Texture2D out_texture2D;                                                 /// <param name="out_texture2D">出力テクスチャ      output texture</param>
        private FrontalFaceDetector[] detector;                                          /// <param name="detector"> 顔検出器                face detector</param>
        private ShapePredictor shape = new ShapePredictor();                             /// <param name="shape">    顔の特徴点検出器        Facial landmark detector</param>
        private DlibDotNet.Point[][] eye_point_L;                                        /// <param name="eye_point_L">左目の検出された点    Point of left eye</param>
        private float[] eye_ratio_L;                                                     /// <param name="eye_ratio_L">左目の縦横比          Aspect ratio of left eye</param>
        private DlibDotNet.Point[][] eye_point_R;                                        /// <param name="eye_point_R">右目の検出された点    Point of right eye</param>
        private float[] eye_ratio_R;                                                     /// <param name="eye_ratio_L">右目の縦横比          Aspect ratio of right eye</param>
        private float eye_L_c = default;                                                 /// <param name="eye_L_c">左目の開き具合            Left eye openness</param>
        private float eye_R_c = default;                                                 /// <param name="eye_R_c">右目の開き具合            Right eye openness</param>
        //微小量       a little number ----------------------------------------------------
        private const float threshold = 0.0001f;

        //顔検出に関する変数     Variable for face detecting ------------------------------
        private byte[][] bytes;                             /// <param name="bytes">Mat→array2Dへの変換時のbyte配列   Byte array when converting from Mat to array2D</param>
        private Mat model_points_mat;                       /// <param name="model_points_mat">3Dモデルの点のMat       3D model points Mat</param>
        private Mat camera_matrix_mat;                      /// <param name="camera_matrix_mat">カメラ行列のMat        Camera matrix Mat</param>
        private Mat dist_coeffs_mat;                        /// <param name="dist_coeffes_mat">レンズの補正のためのMat Mat for lens correction</param>
        private double[][] proj;                            /// <param name="proj">投影行列の配列                      Array of projected matrix</param>
        private double[][] pos_double;                      /// <param name="pos_double">位置の配列(x,y,z)             Array of positions (x, y, z)</param>
        //検出の成功・失敗      success or fail detect ------------------------------------
        private int suc = 0;
        private int fail = 0;

        //Unityで設定するやつ  setting parameter on Unity ---------------------------------
        [SerializeField]
        private Video caputure;                             /// <param name="caputure"> ビデオキャプチャー      Video capture</param>
        [SerializeField]
        private bool logToFile;                             /// <param name="logToFile">ログをファイルに書き込むか Whether to write logs to a file
        [SerializeField]
        private bool debug_face_image;                      /// <param name="debug_face_image">顔の画像のデバッグ  Debug of face image</param>
        [SerializeField]
        private bool debug_eye_image;                       /// <param name="debug_eye_image">目の画像のデバッグ   Debug of eye image</param>
        [SerializeField]
        private GameObject ins;                             /// <param name="ins">複製されるCubeの元               Cube source to be instantiate</param>
        [SerializeField]
        private Transform p0;                               /// <param name="p0">複製されたCubeの親                The parent of the instantiate Cube</param>
        [SerializeField]
        private Transform p1;                               /// <param name="p1">複製されたCubeの親                The parent of the instantiate Cube</param>
        [SerializeField]
        private Transform p2;                               /// <param name="p2">複製されたCubeの親                The parent of the instantiate Cube</param>
        [SerializeField]
        private Transform p3;                               /// <param name="p3">複製されたCubeの親                The parent of the instantiate Cube</param>
        [SerializeField]
        private bool debug_pos_rot;                         /// <param name="debug_pos_rot">位置と回転のデバッグ   Debug of position and rotation</param>
        [SerializeField]
        private bool debug_fps;                             /// <param name="debug_fps">fpsのデバッグ              Debug of fps</param>
        [SerializeField]
        private bool debug_eye_closeness;                   /// <param name="debug_eye_closeness">目の閉じ具合のデバッグ       Debug of eye closeness</param>
        [SerializeField]
        private bool debug_eye_open_ratio;                  /// <param name="debug_eye_open_ratio">目の縦横比のデバッグ        Debug of aspect ratio of eye</param>
        [SerializeField]
        private bool debug_eye_center_ratio;                /// <param name="debug_eye_center_ratio">目玉の横の回転のデバッグ  Debug of eye rotation</param>    
        [SerializeField]
        private string cascade_file = default;              /// <param name="cascade_file">カスケードファイルのあるパス        Path with cascade file</param>
        [SerializeField]
        private string shape_file_5 = default;              /// <param name="shape_file_5">shape_file_5のあるパス              Path with shape_file_5</param>
        [SerializeField]
        private string shape_file_68 = default;             /// <param name="shape_file_68">shape_file_68のあるパス            Path with shape_file_68</param>
                                                            /// 
        public DetectMode mode = DetectMode.Dlib68;        /// <param name="mode">顔検出のモード                  Face detection mode</param>
        [SerializeField]
        private bool blink_tracking = true;                 /// <param name="blink_tracking">まばたき検知をするか  Whether to detect blinking</param>
        public bool Blink_tracking { get { return blink_tracking; } }
        [SerializeField]
        private bool eye_tracking = true;                   /// <param name="eye_tracking">目のトラッキングをするか            Whether to track eyes</param>
        public bool Eye_tracking { get { return eye_tracking; } }
        [Range(30, 288)]
        [SerializeField]
        private int fps_limit;                              /// <param name="fps_limit">fps制限                    limit of fps</param>
        [Range(-10, 0)]
        [SerializeField]
        private float z_scale = 1;                          /// <param name="scall">OpenCV、Dlib5時の位置の変化量の補正(10のn乗オーダー)     Correction of position change at OpenCV, Dlib5(10 to the nth order)</param>
        [Range(0, 2)]
        [SerializeField]
        private float z_offset = 1;                         /// <param name="z_offset">OpenCV、Dlib5時のz軸方向の補正(10のn乗オーダー)       Correction in the z-axis direction at OpenCV, Dlib 5 (nth power of 10)</param>
        [Range(0, 2)]
        [SerializeField]
        private float position_scale = 1;                   /// <param name="position_scall">Dlib68時の位置の変化量の補正     Correction of position change at Dlib68</param>
        [Range(0, 2)]
        [SerializeField]
        private float rotation_scale = 1;                   /// <param name="rotation_scall">Dlib68時の回転量の補正           Correction of rotation amount at Dlib 68</param>
        [SerializeField]
        private float radius = 0.2f;                        /// <param name="radius">頭の動ける半径                Moveable radius of head</param>
        [SerializeField]
        private Vector3 rotation_verocity_ristrict = new Vector3(30, 30, 30);                        /// <param name="rotation_verocity_ristrict">回転速度の制限        Speed ​​limit of rotation</param>
        [SerializeField]
        private float position_verocity_ristrict = 0.1f;                                             /// <param name="position_verocity_ristrict">移動速度の制限        Speed ​​limit of position</param>
        [SerializeField]
        private Vector3 rotation_range = new Vector3(45, 90, 45);                                    /// <param name="rotation_range">頭の回転できる範囲                Range of head rotation</param>
        [SerializeField]
        private Vector3 center = new Vector3(0, 0, 0);                     /// <param name="center">頭の位置の調整                            Head position adjustment</param>
        [SerializeField]
        private Vector3 pos_offset = new Vector3(0, 0, 0);                 /// <param name="pos_offset">3Dモデルからの位置のオフセット        Position offset from 3D model</param>
        [SerializeField]
        private Vector3 rot_offset = new Vector3(0, 0, 0);                 /// <param name="rot_offset">3Dモデルからの回転のオフセット        rotation offset from 3D model</param>
        public Transform model;                                            /// <param name="model">3Dモデル                                   3Dmodel</param>                                                               /// <param name="shape_file_68">shape_file_68のあるパス            Path with shape_file_68</param>
        [SerializeField]
        private SmoothingMethod smoothing;
        [Range(1, 30)]
        [SerializeField]
        private int smooth = 16;                                            /// <param name="smooth">動きのスムーズさ。大きいとスムーズになるが遅くなる。 Smoothness of movement. It will be smooth if it is large, but it will be slow</param>
        [Range(0, 1)]
        [SerializeField]
        private float alpha = 0.85f;
        [Range(1, 10)]
        [SerializeField]
        private int resolution = 1;
        [Range(2, 16)]
        [SerializeField]
        private int thread = 2;                                            /// <param name="thread">このプログラムをいくつのスレッドでやるか   How many threads use
        [SerializeField]
        private bool un_safe = false;
        [SerializeField]
        private Vector2 left_eye_range_high = new Vector2(10, 10);         /// <param name="left_eye_range_high">右目の回転の範囲（高)         Rotation range of left eye (high)</param>
        [SerializeField]
        private Vector2 left_eye_range_low = new Vector2(-10, -20);        /// <param name="left_eye_range_low"> 右目の回転の範囲（低)         Rotation range of left eye (low)</param>
        [SerializeField]
        private Vector2 right_eye_range_high = new Vector2(10, 20);        /// <param name="right_eye_range_high">左目の回転の範囲（高)        Rotation range of right eye (high)</param>
        [SerializeField]
        private Vector2 right_eye_range_low = new Vector2(-10, -10);       /// <param name="right_eye_range_low"> 左目の回転の範囲（低)        Rotation range of right eye (low)</param>
        [Range(0, 1)]
        [SerializeField]
        private float eye_center = 0.35f;                   /// <param name="eye_center">目の中心がどこにあるか        Where is the center of the eye</param>
        [Range(0, 1)]
        [SerializeField]
        private float eye_ratio_h = 0.3f;                   /// <param name="eye_ratio_h">目を開けているときの縦横比   Aspect ratio when eye open</param>
        [Range(0, 1)]
        [SerializeField]
        private float eye_ratio_l = 0.2f;                   /// <param name="eye_ratio_l">目を閉じているときの縦横比   Aspect ratio when eye close</param>
        [SerializeField]
        private Vector3 eye_rot_offset_L = new Vector3();                  /// <param name="eye_rot_offset_L">右目の回転の調整                Adjust left eye rotation</param>
        [SerializeField]
        private Vector3 eye_rot_offset_R = new Vector3();                  /// <param name="eye_rot_offset_R">左目の回転の調整                Adjust right eye rotation</param>
        [SerializeField]
        private Vector2 eye_rot_sensitivity_L = new Vector2(1, 1);         /// <param name="eye_rot_sensitivity_L">左目の回転の倍率           magnification of left eye rotation</param>
        [SerializeField]
        private Vector2 eye_rot_sensitivity_R = new Vector2(1, 1);         /// <param name="eye_rot_sensitivity_R">右目の回転の倍率           magnification of right eye rotation</param>

#pragma warning restore 0649
    }
}