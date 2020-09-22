using OpenCvSharp;
using UnityEngine;

namespace kumaS
{
    public class DebugVideo : MonoBehaviour
    {
        public Video capture = default;
        Renderer material = default;
        public float scale = 1;
        private bool ready = false;
        // Start is called before the first frame update
        async void Start()
        {
            material = GetComponent<Renderer>();
            await capture.WaitOpen();
            transform.localScale = new Vector3((float)capture.width / capture.height * scale, 1, 1 * scale);
            ready = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (ready)
            {

                using (Mat copy = capture.Read())
                {
                    material.material.mainTexture = MatTexture.Mat2Texture(copy);
                }

                if (capture.ended)
                {
                    ready = false;
                }
            }
        }
        private void OnDestroy()
        {
            capture.Dispose();
        }
    }
}
