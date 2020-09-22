using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace kumaS.PoseNet
{
    partial class BodyTracking
    {

#pragma warning disable 0649

        //公開メンバ     public member
        //なお、各インデックスの対応部位はこちらにのっている。(https://www.tensorflow.org/lite/models/pose_estimation/overview)                    The corresponding parts of the body of each index are shown here.
        public Vector2[] Pose2d { get; private set; } = new Vector2[17];            /// <param name="Pose2d">2dでの体の各部位の場所。              The location of each part of the body in 2D. </param>
        public Vector3[] Pose3d { get; private set; } = new Vector3[17];            /// <param name="Pose3d">3dでの体の各部位の場所。              The location of each part of the body in 3D.</param>
        public Quaternion[] Rotation { get; private set; } = new Quaternion[10];    /// <param name="Rotation">各ボーンの回転。                    Rotation of each bone.</param>
        public Vector3 Position { get; private set; } = default;                    /// <param name="Position">人物の位置。                        Position of the person.</param>

        //デバッグ用     for debug  ---------------------------------------------------------------------------------
        [SerializeField]
        private bool debug_2d_pos = false;                                          /// <param name="debug_2d_pos">二次元のデバッグ用              For 2d debugging.</param>
        [SerializeField]
        private bool debug_3d_pos = false;                                          /// <param name="debug_3d_pos">三次元のデバッグ用              For 3d debugging.</param>
        [SerializeField]
        private bool debug_fps = false;                                             /// <param name="debug_fps">fpsのデバッグ用                    For fps debugging.</param>
        [SerializeField]
        private bool debug_logfile = false;                                         /// <param name="debug_logfile">デバッグデータをファイルに書き出すか。     Wherether write the debug data to a file.</param>
        //デバッグのキャッシュ用 for debug cache  --------------------------------------------------------------------
        private LineRenderer[] debug_2d_lines = default;                            
        private LineRenderer[] debug_3d_lines = default;
        private StreamWriter fps_writer;
        private StreamWriter pos_writer;
        private StreamWriter rot_writer;
        private LinkedList<DateTime> fin_time = new LinkedList<DateTime>();
        private int[] frame_time = new int[7];

        //Unityでの設定     setting in Unity --------------------------------------------------------------------------
        [SerializeField]
        private Video capture;                                                      /// <param name="capture">ビデオ                               Video</param>
        [SerializeField]
        private int thread = 2;                                                     /// <param name="thread">使うCPUのスレッド数。                 Number of CPU threads to use.</param>
        [SerializeField]
        private int maxfps = 80;                                                    /// <param name="maxfps">最大のfps。                           Maximum FPS.</param>
        [SerializeField]
        private int smooth = 16;                                                    /// <param name="smooth">動作の滑らかさ。                      Smoothness of movement.</param>
        [SerializeField]
        private string tflie = default;                                             /// <param name="tflie">.tflieの場所                           Path of .tflie</param>
        [SerializeField]
        private float z_offset = 2;                                                 /// <param name="z_offset">z軸のオフセット値。                 Offset value of the z-axis.</param>
        [SerializeField]
        private float camera_angle = 22;                                            /// <param name="camera_angle">カメラの縦の視野角。片側。度。         The vertical viewing angle of the camera. One side. degree.</param>
        [SerializeField]
        private float shoulderline2nose = 0.20f;                                    /// <param name="shoulderline2nose">両肩の中心から鼻までの距離。      Distance from the center of both shoulders to the nose.</param>
        [SerializeField]
        private float nose2eye = 0.055f;                                            /// <param name="nose2eye">鼻から目までの距離。                Distance from nose to eye.</param>
        [SerializeField]
        private float nose2ear = 0.13f;                                             /// <param name="nose2ear">鼻から耳までの距離。                Distance from nose to ear.</param>
        [SerializeField]
        private float shoulder2hip = 0.46f;                                         /// <param name="shoulder2hip">肩から股関節までの距離。        Distance from the shoulder to the hip.</param>
        [SerializeField]
        private float hip2knee = 0.44f;                                             /// <param name="hip2knee">股関節から膝までの距離。            Distance from the hip to the knee.</param>
        [SerializeField]
        private float knee2ankle = 0.48f;                                           /// <param name="knee2ankle">膝から足首までの距離。            Distance from the knee to the ankle.</param>
        [SerializeField]
        private float shoulder2elbow = 0.275f;                                      /// <param name="shoulder2elbow">肩から肘までの距離。          Distance from the shoulder to the elbow.</param>
        [SerializeField]
        private float elbow2wrist = 0.26f;                                          /// <param name="elbow2wrist">肘から手首までの距離。           Distance from the elbow to the wrist.</param>
        //---------------------------------------------------------------------------------------
        [SerializeField]
        private Vector3 nose = new Vector3(0, 1.53f, 0.11f);                        /// <param name="nose">鼻の初期位置。                          Initial position of nose.</param>
        [SerializeField]
        private Vector3 leftEye = new Vector3(0.0315f, 1.57f, 0.08f);              /// <param name="leftEye">左目の初期位置。                      Initial position of left eye.</param>
        [SerializeField]
        private Vector3 rightEye = new Vector3(-0.0315f, 1.57f, 0.08f);             /// <param name="rightEye">右目の初期位置。                    Initial position of right eye.</param>
        [SerializeField]
        private Vector3 leftEar = new Vector3(0.09f, 1.53f, 0);                     /// <param name="leftEar">左耳の初期位置。                     Initial position of left ear.</param>
        [SerializeField]
        private Vector3 rightEar = new Vector3(-0.09f, 1.53f, 0);                   /// <param name="rightEar">右耳の初期位置。                    Initial position of right ear.</param>
        [SerializeField]
        private Vector3 leftShoulder = new Vector3(0.18f, 1.35f, 0);                /// <param name="leftShoulder">左肩の初期位置。                Initial position of left sholder.</param>
        [SerializeField]
        private Vector3 rightShoulder = new Vector3(-0.18f, 1.35f, 0);              /// <param name="rightShoulder">右肩の初期位置。               Initial position of right sholder.</param>
        [SerializeField]
        private Vector3 leftElbow = new Vector3(0.43f, 1.35f, 0);                   /// <param name="leftElbow">左肘の初期位置。                   Initial position of left elbow.</param>
        [SerializeField]
        private Vector3 rightElbow = new Vector3(-0.43f, 1.35f, 0);                 /// <param name="rightElbow">右肘の初期位置。                  Initial position of right elbow.</param>
        [SerializeField]    
        private Vector3 leftWrist = new Vector3(0.73f, 1.35f, 0);                   /// <param name="leftWrist">左手首の初期位置。                 Initial position of left wrist.</param>
        [SerializeField]
        private Vector3 rightWrist = new Vector3(-0.73f, 1.35f, 0);                 /// <param name="rightWrist">右手首の初期位置。                Initial position of right wrist.</param>
        [SerializeField]
        private Vector3 leftHip = new Vector3(0.13f, 0.8f, 0);                      /// <param name="leftHip">左股関節の初期位置。                 Initial position of left hip.</param>
        [SerializeField]
        private Vector3 rightHip = new Vector3(-0.13f, 0.8f, 0);                    /// <param name="rightHip">右股関節の初期位置。                Initial position of right hip.</param>
        [SerializeField]
        private Vector3 leftKnee = new Vector3(0.13f, 0.4f, 0);                     /// <param name="leftKnee">左膝の初期位置。                    Initial position of left knee.</param>
        [SerializeField]
        private Vector3 rightKnee = new Vector3(-0.13f, 0.4f, 0);                   /// <param name="rightKnee">右膝の初期位置。                   Initial position of right knee.</param>
        [SerializeField]
        private Vector3 leftAnkle = new Vector3(0.13f, 0, 0);                       /// <param name="leftAnkle">右足首の初期位置。                 Initial position of left ankle.</param>
        [SerializeField]
        private Vector3 rightAnkle = new Vector3(-0.13f, 0, 0);                     /// <param name="rightAnkle">左足首の初期位置。                Initial position of right ankle.</param>

        [SerializeField]
        private float nose_max_speed = 1;                                           /// <param name="nose_max_speed">鼻の制限速度。                Speed limit of nose.</param>
        [SerializeField]
        private float eye_max_speed = 1;                                            /// <param name="eye_max_speed">目の制限速度。                 Speed limit of eye.</param>
        [SerializeField]
        private float ear_max_speed = 1;                                            /// <param name="ear_max_speed">耳の制限速度。                 Speed limit of ear.</param>
        [SerializeField]
        private float shoulder_max_speed = 1;                                       /// <param name="shoulder_max_speed">肩の制限速度。            Speed limit of sholder.</param>
        [SerializeField]
        private float elbow_max_speed = 1;                                          /// <param name="elbow_max_speed">肘の制限速度。               Speed limit of elbow.</param>
        [SerializeField]
        private float wrist_max_speed = 1;                                          /// <param name="wrist_max_speed">手首の制限速度。             Speed limit of wrist.</param>
        [SerializeField]
        private float hip_max_speed = 1;                                            /// <param name="hip_max_speed">股関節の制限速度。             Speed limit of hip.</param>
        [SerializeField]
        private float knee_max_speed = 1;                                           /// <param name="knee_max_speed">膝の制限速度。                Speed limit of knee.</param>
        [SerializeField]
        private float ankle_max_speed = 1;                                          /// <param name="ankle_max_speed">足首の制限速度。             Speed limit of left ankle.</param>

        //キャッシュ用        for cache  --------------------------------------------------
        private List<PoseNet> posenet = new List<PoseNet>();                        
        private LinkedList<Vector2[]> pose2d_chain = new LinkedList<Vector2[]>();   
        private LinkedList<Vector3[]> pose3d_chain = new LinkedList<Vector3[]>();   
        private LinkedList<int> elapt_time = new LinkedList<int>();                 
        private int diff_time = default;
        private bool running = false;                                              
        private System.Diagnostics.Stopwatch[] stopwatches = default;
        private float[] max_speeds = new float[17];

#pragma warning restore 0649

    }
}
