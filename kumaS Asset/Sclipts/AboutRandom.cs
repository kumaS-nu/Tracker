
#if VRM_EXIST

using System;
using System.Collections;
using UnityEngine;
using VRM;
using kumaS.HandMove;

namespace kumaS
{
    public class AboutRandom : MonoBehaviour
    {
        /// <param name="anim">対象のanimator                 target animator</param>
        /// <param name="term">アニメーション遷移の間の長さ   the length between animation transitions</param>
        /// <param name="term2">表情遷移の間の長さ            the length between facial expression transitions</param>
        /// <param name="proxy">対象のblendshape              target blendshape</param>

        private Animator anim;
        private System.Random r, r2;
        public int term = 40;
        public int term2 = 32;
        private VRMBlendShapeProxy proxy;
        private int num, num2;


        /// <summary>
        /// 初期化  initializ
        /// </summary>
        void Start()
        {
            anim = GetComponent<Animator>();
            num = -term / 2;
            num2 = -term2 / 2;
            r = new System.Random();
            r2 = new System.Random((int)DateTime.Now.Ticks);
        }

        /// <summary>
        /// ランダムな遷移をさせ続ける   keep judgeing random animation transition
        /// </summary>
        void Update()
        {
            if (!proxy)
            {
                proxy = GetComponent<VRMBlendShapeProxy>();
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("kubi"))
            {
                anim.SetBool("random", true);
                Debug.Log("random is true");
                num = term * 3 / 2;
            }

            if (r.Next(2) == 1)
            {
                if (num < term)
                {
                    num++;
                }

                if (num == term / 2)
                {
                    anim.SetBool("random", true);
                    Debug.Log("random is true");
                    num = term * 3 / 2;
                }
            }
            else
            {
                if (num > 0)
                {
                    num--;
                }

                if (num == term / 2)
                {
                    anim.SetBool("random", false);
                    Debug.Log("random is false");
                    num = -term / 2;
                }
            }

            if (r2.Next(2) == 1)
            {
                if (num2 < term2)
                {
                    num2++;
                }

                if (num2 == term2 / 2)
                {
                    StartCoroutine(Change(proxy, 0f, 0.1f));
                    Debug.Log("random2 is true");
                    num2 = term2 * 3 / 2;
                }
            }
            else
            {
                if (num2 > 0)
                {
                    num2--;
                }

                if (num2 == term2 / 2)
                {
                    StartCoroutine(Change(proxy, 0.1f, 0f));
                    Debug.Log("random2 is false");
                    num2 = -term2 / 2;
                }
            }

        }
        /// <summary>
        /// 表情遷移 transition facial expression
        /// </summary>
        /// <param name="proxy">対象のブレンドシェイプ  target blendshape</param>
        /// <param name="start">始まりの表情の値        emotion parameter when transition start</param>
        /// <param name="end">終わりの表情の値          emotion parameter when transition end</param>
        /// <returns></returns>
        IEnumerator Change(VRMBlendShapeProxy proxy, float start, float end)
        {
            while (Animtest.isRunning)
            {
                yield return new WaitForEndOfFrame();
            }
            float Parameter = 0;

            while (Parameter <= 0.2)
            {
                try
                {
                    proxy.ImmediatelySetValue("SMILE1", Mathf.Lerp(start, end, Parameter * 5));
                }
                catch (Exception)
                {
                    proxy.ImmediatelySetValue("fun", Mathf.Lerp(start, end, Parameter * 5));
                }
                yield return null;

                Parameter += Time.deltaTime;
            }

            try
            {
                proxy.ImmediatelySetValue("SMILE1", end);
            }
            catch (Exception)
            {
                proxy.ImmediatelySetValue("fun", end);
            }
        }
    }
}

#endif