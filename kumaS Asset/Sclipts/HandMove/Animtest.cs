
#if VRM_EXIST

using System.Collections;
using UnityEngine;
using VRM;

namespace kumaS.HandMove
{
    public class Animtest : MonoBehaviour
    {
        /// <param name="isRunning">表情遷移中かどうか whether the facial expression is transitioning or not</param>
        /// <param name="anim">対象のanimator          target animator</param>
        /// <param name="proxy">対象のblendshape　     target blendshape</param>
        /// <param name="chair">椅子の保持             gameobject of chair</param>
        static public bool isRunning;
        private Animator anim;
        private VRMBlendShapeProxy proxy;
        public GameObject chair;


        void Start()
        {
            anim = GetComponent<Animator>();
            isRunning = false;
        }


        void Update()
        {
            if (!proxy)
            {
                proxy = GetComponent<VRMBlendShapeProxy>();
            }

            anim.ResetTrigger("bow");            ///アニメーションの予約状態を防止 
            anim.ResetTrigger("suprize");        ///prevent reservation of animation
            anim.ResetTrigger("kubi");
            anim.ResetTrigger("by");

            ///アニメーションボタン      button animation sets
            ///<animation>座る・立つ     sit or stand</animation>
            ///<key>return</key>
            if (Input.GetKeyDown("return"))
            {

                if (anim.GetBool("sit") == true)
                {
                    anim.SetBool("sit", false);
                    chair.SetActive(false);
                }
                else
                {
                    anim.SetBool("sit", true);
                    chair.SetActive(true);
                }
            }

            if (!Input.GetKey(KeyCode.LeftShift))
            {
                ///<animation>お辞儀    bow</animation>
                ///<key>1(Alpha1)</key>
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("wait"))
                    {
                        anim.SetTrigger("bow");
                        Debug.Log("bow");
                        Invoke("Bow", 2f);
                    }
                }
                ///<animation>驚く    suprized</animation>
                ///<key>2(Alpha2)</key>
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    anim.SetTrigger("suprize");
                    StartCoroutine(EmotionCoroutine(proxy, "SUPRISED", 0.8f, 0.05f, 0.2f, 0.25f));
                    Debug.Log("suprize");
                }
                ///<animation>首をかしげる    put one's head on one side</animation>
                ///<key>3(Alpha3)</key>
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    anim.SetTrigger("kubi");
                    StartCoroutine(EmotionCoroutine(proxy, "SMILE2", 0.5f, 0.5f, 1f, 1.5f));
                    Debug.Log("kubi");
                }
                ///<animation>手を振る    swing hand</animation>
                ///<key>0(Alpha0)</key>
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("wait2"))
                    {
                        anim.SetTrigger("by");
                        StartCoroutine(EmotionCoroutine(proxy, "SMILE1", 0.7f, 0.8f, 2.2f, 2.8f));
                        Debug.Log("goodby");
                    }
                }
            }

            ///マウスでのほほえみ操作  controle of smille by mouse
            proxy.ImmediatelySetValue("FUN2", Input.GetAxis("Fire1") * 2 / 3);
        }

        // Bowからkubiの遷移
        void Bow()
        {
            StartCoroutine(EmotionCoroutine(proxy, "SMILE2", 0.5f, 0.5f, 1f, 1.5f));
            Debug.Log("kubi");
        }

        /// <summary>
        /// 表情変更 transition facial expression 
        /// </summary>
        /// <param name="proxy">対象のblendshape      target blendshape</param>
        /// <param name="bs">表情の名前               name of facial expression</param>
        /// <param name="max">最大値                  maximum of parametor of facial expression</param>
        /// <param name="time1">最大まで行く時間      the time from start to maximum of facial expression</param>
        /// <param name="time2">最大でいる時間(累積)  the time from start to end of maximum of facial expression</param>
        /// <param name="time3">もとに戻る時間(累積)  the time of this transition</param>
        IEnumerator EmotionCoroutine(VRMBlendShapeProxy proxy, string bs, float max, float time1, float time2, float time3)
        {
            if (isRunning)
            {
                Debug.Log("break");
                yield break;
            }

            isRunning = true;
            float Parameter = 0;

            while (Parameter <= time1)
            {
                proxy.ImmediatelySetValue(bs, Mathf.Lerp(0, max, Parameter / time1));
                yield return null;

                Parameter += Time.deltaTime;
            }
            Debug.Log("1");

            yield return new WaitForSeconds(time2 - time1);
            Parameter = time2;
            Debug.Log("2");

            while (Parameter <= time3)
            {
                proxy.ImmediatelySetValue(bs, Mathf.Lerp(max, 0, (Parameter - time2) / (time3 - time2)));
                yield return null;

                Parameter += Time.deltaTime;
            }
            Debug.Log("3");

            proxy.ImmediatelySetValue(bs, 0f);

            isRunning = false;
        }
    }
}

#endif