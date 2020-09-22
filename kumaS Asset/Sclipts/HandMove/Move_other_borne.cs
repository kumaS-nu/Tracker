using UnityEngine;

namespace kumaS.HandMove
{
    /// <summary>
    /// 腕とかのボーンをとか動かすクラス        move born like arm
    /// </summary>
    public class Move_other_borne : Headmove
    {

        /// <param name="l_r">右か左か                                                  right or left</param>
        /// <param name="sec_key">二番目のボーンか(SHIFT押すやつか押さないやつか)       this born is secondaly or not</param>
        private float tempz;
        public int l_r;
        public bool sec_key;

        override protected void LateUpdate()
        {
            anim_rot = Resize(anim.GetBoneTransform(bone).localEulerAngles);

            if (Input.GetKeyUp(key_name) && Seckey())
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

            if (Input.GetKeyUp(KeyCode.R))
            {
                StartCoroutine(Reset_move());
            }

            if (Can_move())
            {
                tempx = Input.GetAxis("Mouse X");
                tempy = Input.GetAxis("Mouse Y");
                tempz = Input.GetAxis("Mouse ScrollWheel");

                if (rot.x > min.x || tempz < 0)
                {
                    if (rot.x < max.x || tempz > 0)
                    {
                        rot += new Vector3(-tempz * range.x * 10, 0, 0);
                    }
                }

                if (rot.y > min.y || l_r * tempx < 0)
                {
                    if (rot.y < max.y || l_r * tempx > 0)
                    {
                        rot += new Vector3(0, l_r * -tempx * range.y, 0);
                    }
                }

                if (rot.z > min.z || l_r * tempy < 0)
                {
                    if (rot.z < max.z || l_r * tempy > 0)
                    {
                        rot += new Vector3(0, 0, l_r * -tempy * range.z);
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
        /// セカンダリーかそうじゃないか判断して切り替えるか判断      judge can change move or not with secondaly or not
        /// </summary>
        /// <returns>対象ならtrue                                     if ok, return true</returns>
        private bool Seckey()
        {
            if (sec_key == Input.GetKey(KeyCode.LeftShift))
            {
                return true;
            }
            return false;
        }

    }
}