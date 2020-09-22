using UnityEngine;

namespace kumaS.HandMove
{
    /// <summary>
    /// 目の動きを制御するクラス    controle eye move
    /// </summary>
    public class Eyemove : Headmove
    {
        /// <param name="state">どこ向くか            where see</param>
        /// <param name="cam_obj">カメラ              camera</param>
        /// <param name="l_r">右目か左目か            left eye or right eye</param>
        private int state = 1;
        public GameObject cam_obj;
        private Vector3 cam_pos;
        public bool l_r;


        private void Update()
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

                active = !active;

            }

            //目の位置を正面に戻す    reset eye position
            if (Input.GetKeyUp(KeyCode.R))
            {
                StartCoroutine(Reset_move());
            }

            //SHIFT + 数字でどこ向くか決める  detamin where eye see with Shift and number
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyUp(KeyCode.Alpha0))
                {
                    state = 0;
                }

                if (Input.GetKeyUp(KeyCode.Alpha1))
                {
                    state = 1;
                }
            }


        }

        // Update is called once per frame
        override protected void LateUpdate()
        {
            if (Can_move())
            {
                switch (state)
                {
                    case 0: MouseMoveEye(); break;
                    case 1: SeeCamera(); break;
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
        /// マウスで目を動かすときの関数      function eye move with mouse
        /// </summary>
        protected void MouseMoveEye()
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
                    rot += new Vector3(0, -tempx * range.y, 0);
                }
            }

        }

        /// <summary>
        /// カメラを見る時の関数      function see camera
        /// </summary>
        protected void SeeCamera()
        {
            cam_pos = cam_obj.transform.position;
            cam_pos.y -= 0.1f; //カメラの場所を見させたら多少おかしかったので調整  magic number

            //右目と左目で多少差を出す see diferent point left or right
            if (l_r)
            {
                cam_pos.x += 0.08f;
            }
            else
            {
                cam_pos.x -= 0.08f;
            }
            transform.LookAt(cam_pos);
            rot = transform.localRotation.eulerAngles;

            //正の数で返してくるので-180～180になるよう調整    transform.LookAt returns plus number. So adjust to -180 to 180
            if (rot.x > 180)
            {
                rot.x -= 360;
            }

            if (rot.y > 180)
            {
                rot.y -= 360;
            }

            //なんかこんぐらいのほうがよかった      magic number
            rot /= 4;

            //目の動く範囲を制限     restrict eye move 
            if (rot.x > max.x)
            {
                rot.x = max.x;
            }
            else if (rot.x < min.x)
            {
                rot.x = min.x;
            }

            if (rot.y > max.y)
            {
                rot.y = max.y;
            }
            else if (rot.y < min.y)
            {
                rot.y = min.y;
            }

            rot.z = 0;

        }
    }
}