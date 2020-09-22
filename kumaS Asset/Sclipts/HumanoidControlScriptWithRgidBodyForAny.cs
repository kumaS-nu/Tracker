///<summary>
/// Mecanimのアニメーションデータが、原点で移動しない場合の Rigidbody付きコントローラ
/// UnityChanControlScriptWithRgidBody.csをヽ｜∵｜ゝ Fantomさんが他のキャラクターに対して使えるようにした
/// UnityChanControlScriptWithRgidBodyForAny.csを整理・改変したものです
/// 2014/03/13 N.Kobyasahi
/// 2015/03/11 Revised for Unity5 (only)
///</summary>

using UnityEngine;

namespace UnityChan
{
    // 必要なコンポーネントの列記
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]

    public class HumanoidControlScriptWithRgidBodyForAny : MonoBehaviour
    {
        /// <param name="animSpeed">          アニメーション再生速度設定               animation playing speed</param>
        /// <param name="useCurves">           Mecanimでカーブ調整を使うか設定する      set whether to use curve adjustment with Mecanim</param>
        /// <param name="useCurvesHeight">     カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）
        ///                                     height of afect curve correction (increase when the easy to penetrate the ground)</param>
		public float animSpeed = 1.5f;
        public bool useCurves = true;
        public float useCurvesHeight = 0.5f;

        /// 以下キャラクターコントローラ用パラメーター
        /// <param name="forwardSpeed"> 前進速度       speed of go forward</param>
        /// <param name="backwardSpeed">後退速度      speed of go backward</param>
        /// <param name="rotateSpeed">  旋回速度        turn speed</param>
        /// <param name="jumpPower">    ジャンプ威力      strength of jump</param>
        public float forwardSpeed = 7.0f;
        public float backwardSpeed = 2.0f;
        public float rotateSpeed = 2.0f;
        public float jumpPower = 3.0f;

        ///<param name="col">                   キャラクターコントローラ（カプセルコライダ）の参照   reference to character controller (CapsuleCollider)</param>
        ///<param name="ignoreCollisionLayer">  カプセルコライダを無視するレイヤー(キャラ自身がコライダを持っている場合、
        ///                                           カプセルコライダとの衝突判定を無効にするため)   >=0で有効
        ///                                     Layer which ignore CapsuleCollider(For invalidate the collision with the capsule collider
        ///                                           when the character has a collider) when >=0, valid
        ///                                   </param>
        private CapsuleCollider col;
        private Rigidbody rb;
        public int ignoreCollisionLayer = -1;

        ///<param name="velocity">      キャラクターコントローラ（カプセルコライダ）の移動量  Amount of movement of character controller (CapsuleCollider)</param>
        ///<param name="orgColHight">   CapsuleColliderで設定されているコライダのHeiht、Centerの初期値を収める
        ///                                  hold the initial value of Heiht, Center of the collider set by CapsuleCollider</param>
        ///<param name="anim">          対象のAnimator       target Animator</param> 
        ///<param name="currentBaseState">base layerで使われる、アニメーターの現在の状態の参照
        ///                               reference to currunt state of animator used in base layer</param>
        private Vector3 velocity;
        private float orgColHight;
        private Vector3 orgVectColCenter;
        private Animator anim;
        private AnimatorStateInfo currentBaseState;


        /// アニメーター各ステートへの参照     reference to each state of animator
        static int[] idleState = { Animator.StringToHash("Base Layer.wait"),   Animator.StringToHash("Base Layer.wait2"),
                                   Animator.StringToHash("Base Layer.sit"),    Animator.StringToHash("Base Layer.sit2") };
        static int[] locoState = { Animator.StringToHash("Base Layer.walkf"),  Animator.StringToHash("Base Layer.walkf 1"),
                                   Animator.StringToHash("Base Layer.walkf 0"),Animator.StringToHash("Base Layer.walkf 0 0")};
        static int jumpState = Animator.StringToHash("Base Layer.Jump");

        /// 初期化 initialize
        void Start()
        {
            //カプセルコライダを無視するレイヤーを設定      set layer which ignore CapsuleCollider
            if (ignoreCollisionLayer >= 0)
            {
                Physics.IgnoreLayerCollision(gameObject.layer, ignoreCollisionLayer);
            }

            // 各コンポーネントを取得する    get each component
            anim = GetComponentInChildren<Animator>();
            col = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();
            // Height、Centerの初期値を保存   save initial value height and center
            orgColHight = col.height;
            orgVectColCenter = col.center;
        }



        void FixedUpdate()
        {
            ///Horizontalをh、Verticalをvで一時保存                  temporarily store horizontal axis as h and vertical axis as v
            ///Animatorの"Speed"パラメーターにv、"Direction"パラメタにhを渡す  
            ///Set "v" for "Speed" parameter of Animator and h for "Direction" parameter
            ///Animatorのモーション再生速度に animSpeedを設定する    set animation playing speed as ainmSpeed
            ///参照用のステート変数にBase Layer (0)の現在のステートを設定する   set currunt state in Base Layer (0) in state variable for reference
            ///重力の影響を受けるようにする                          affected by gravity
			float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            anim.SetFloat("Speed", v);
            anim.SetFloat("Direction", h);
            anim.speed = animSpeed;
            currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
            rb.useGravity = true;



            /// 以下、キャラクターの移動処理                    below character movement processing
            /// 前後移動                                        make amount back and forth
            /// キャラクターのローカル空間での方向に変換        Convert character's direction in local space
            velocity = new Vector3(0, 0, v);
            velocity = transform.TransformDirection(velocity);

            ///前後移動の閾値は、Mecanim側のトランジションと一緒に調整
            ///Adjust the threshold of forward and backward movement together with Mecanim's transition
            if (v > 0.1)
            {
                velocity *= forwardSpeed;
            }
            else if (v < -0.1)
            {
                velocity *= backwardSpeed;
            }

            if (Input.GetButtonDown("Jump"))
            {

                ///アニメーションのステートがLocomotionの最中のみジャンプできる    while only animation state is Locomotion, can janp 
                if (CheckLocoState())
                {
                    ///ステート遷移中でなかったらジャンプできる                    can jump if state transition is not in progress            
                    if (!anim.IsInTransition(0))
                    {
                        rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                        /// Animatorにジャンプに切り替えるフラグを送る             send "Jump" flag to Animator
                        anim.SetBool("Jump", true);
                    }
                }
            }


            /// 上下のキー入力でキャラクターを移動させる         move back and forth
            transform.localPosition += velocity * Time.fixedDeltaTime;

            /// 左右のキー入力でキャラクタをY軸で旋回させる      turn left or right
            transform.Rotate(0, h * rotateSpeed, 0);


            /// 以下、Animatorの各ステート中での処理             below processing in each state of Animator
            /// Locomotion中                                     in Locomotion
            /// 現在のベースレイヤーがlocoStateの時              when currunt base layer is locoState
            if (CheckLocoState())
            {
                //カーブでコライダ調整をしている時は、念のためにリセットする   reset during adjust the collidor with a curve 
                if (useCurves)
                {
                    resetCollider();
                }
            }
            /// JUMP中の処理                                       processing in JUMP
            /// 現在のベースレイヤーがjumpStateの時                if currunt base layer is jumpState
            else if (currentBaseState.fullPathHash == jumpState)
            {
                /// ステートがトランジション中でない場合               if state is not transitioning
                if (!anim.IsInTransition(0))
                {

                    /// 以下、カーブ調整をする場合の処理                   below processing  when adjust the collidor with a curve
                    if (useCurves)
                    {
                        /// JumpHeight:JUMP00でのジャンプの高さ（0〜1）             JumpHeight: hight in JUMP00
                        /// GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効   GravityControl: 1→during Jump (invalid gravity) 0→valid gravity
                        float jumpHeight = anim.GetFloat("JumpHeight");
                        float gravityControl = anim.GetFloat("GravityControl");
                        if (gravityControl > 0)
                        {
                            ///ジャンプ中の重力の影響を切る                        invalid gravity while jumping
                            rb.useGravity = false;
                        }

                        ///レイキャストをキャラクターのセンターから落とす      drop ray cast from the center of the character
                        Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
                        RaycastHit hitInfo = new RaycastHit();
                        /// 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整
                        /// adjust the height and center of the collider with the curve attached to the JUMP 00 animation when the height is over useCurves Height
                        if (Physics.Raycast(ray, out hitInfo))
                        {
                            if (hitInfo.distance > useCurvesHeight)
                            {
                                /// 調整されたコライダーの高さ                         height of collider after ajusted
                                col.height = orgColHight - jumpHeight;
                                float adjCenterY = orgVectColCenter.y + jumpHeight;
                                /// 調整されたコライダーのセンター                     center of collider after ajusted
                                col.center = new Vector3(0, adjCenterY, 0);
                            }
                            else
                            {
                                /// 閾値よりも低い時には初期値に戻す（念のため）	   when it is lower than the threshold value, return it to the initial value 		
                                resetCollider();
                            }
                        }
                    }
                    /// Jump bool値をリセットする（ループしないようにする）reset value of jump bool 				
                    anim.SetBool("Jump", false);
                }
            }
            /// IDLE中の処理                                                   processing in IDLE
            /// 現在のベースレイヤーがidleStateの時                            if currunt base layer is idleState
            else if (CheckIdleState())
            {
                ///カーブでコライダ調整をしている時は、念のためにリセットする      reset during adjust the collidor with a curve 
                if (useCurves)
                {
                    resetCollider();
                }
            }
        }

        /// <summary>
        /// キャラクターのコライダーサイズのリセット関数   reset charactor collider size
        /// </summary>
        void resetCollider()
        {
            col.height = orgColHight;
            col.center = orgVectColCenter;
        }

        /// <summary>
        /// 現在のアニメーションがアイドルか確認     wherether currunt animation is idle or not
        /// </summary>
        /// <returns>アイドル: true                  idle: true</returns>
        bool CheckIdleState()
        {
            for (int i = 0; i < idleState.Length; i++)
            {
                if (currentBaseState.fullPathHash == idleState[i])
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 現在のアニメーションがロコモーションか確認     wherether currunt animation is locomotion or not
        /// </summary>
        /// <returns>ロコモーション: true                  locomotion: true</returns>
        bool CheckLocoState()
        {
            for (int i = 0; i < locoState.Length; i++)
            {
                if (currentBaseState.fullPathHash == locoState[i])
                {
                    return true;
                }
            }
            return false;
        }

    }
}