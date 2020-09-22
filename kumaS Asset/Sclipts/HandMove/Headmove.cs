using System.Collections;
using UnityEngine;

namespace kumaS.HandMove
{
    public class Headmove : MonoBehaviour
    {
        /// <param name="rot">頭を傾ける角度                           target angle of head (and neck)</param>
        /// <param name="anim">対象のAnimator                          target Animator</param>
        /// <param name="anim_name">動いていいアニメーションの名前     animation name allow to head move</param>
        /// <param name="key_name">動くかの切り替えのキー              key name of change allow to head move or not</param>
        /// <param name="start">始めアクティブかどうか                 in start, this is active or not</param>
        /// <param name="set_start_rot">始めの角度                     first angle</param>

        public HumanBodyBones bone;
        protected Vector3 rot;
        protected Vector3 anim_rot;
        protected float tempx;
        protected float tempy;
        public Animator anim;
        public string[] anim_name;
        public string key_name;
        protected bool active;
        public bool start;
        protected float spend_time;
        public Vector3 max;
        public Vector3 min;
        protected Vector3 range;
        protected bool keep;

        protected void Start()
        {
            var t = Resize(anim.GetBoneTransform(bone).localEulerAngles);
            rot = t;
            range = (max - min) / 20;
            anim_rot = t;
            Cursor.visible = false;
            active = start;
            spend_time = 0;
            keep = false;
        }

        /// <summary>
        /// マウスによって頭を動かす     move head by mouse
        /// </summary>
        virtual protected void LateUpdate()
        {
            anim_rot = Resize(anim.GetBoneTransform(bone).localEulerAngles);
            if (Input.GetKeyUp(key_name))
            {
                if (Input.GetKey(KeyCode.LeftControl) && active)
                {
                    keep = true;
                }
                else if (active)
                {
                    rot = anim_rot;
                    keep = false;
                }
                else
                {
                    keep = false;
                }

                active = !active;

            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                StartCoroutine(Reset_move());
            }

            if (Can_move())
            {
                tempx = Input.GetAxis("Mouse X");
                tempy = Input.GetAxis("Mouse Y");

                if (rot.x > min.x || tempy < 0)
                {
                    if (rot.x < max.x || tempy > 0)
                    {
                        rot += new Vector3(-tempy * range.x, 0, 0);
                    }
                }

                if (rot.y > min.y || tempx < 0)
                {
                    if (rot.y < max.y || tempx > 0)
                    {
                        rot += new Vector3(0, -tempx * range.y, -tempx * range.z / 4);
                    }
                }

                transform.localRotation = Quaternion.Euler(rot);
            }
            else if (!keep)
            {
                rot = anim_rot;
            }

            if (keep)
            {
                transform.localRotation = Quaternion.Euler(rot);
            }
        }
        /// <summary>
        /// 動いていいアニメーションか判定  judge whether it is permitted to head move
        /// </summary>
        /// <returns>動ける: true           can move: true</returns>
        protected bool Can_move()
        {
            for (int i = 0; i < anim_name.Length; i++)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName(anim_name[i]))
                {
                    if (active)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 最初の状態に戻す。   reset first state
        /// </summary>
        protected IEnumerator Reset_move()
        {
            Vector3 temp = rot;
            while (spend_time < 1.0 / 3.0)
            {
                rot = Vector3.Slerp(temp, anim_rot, spend_time * 3);

                yield return null;
                spend_time += Time.deltaTime;
            }
            spend_time = 0;
        }

        /// <summary>
        /// アニメーションからとったロテーションを使える形として返す    returen value from born rotation in animation
        /// </summary>
        /// <param name="root">アニメーションからとったロテーション     born rotation in animation</param>
        /// <returns>使えるロテーション                                 usable rotation</returns>
        protected Vector3 Resize(Vector3 root)
        {
            Vector3 res = root;
            if (res.x > 180)
            {
                res.x -= 360;
            }
            if (res.y > 180)
            {
                res.y -= 360;
            }
            if (res.z > 180)
            {
                res.z -= 360;
            }

            return res;
        }

    }
}