using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.Video;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine.Rendering;

namespace kumaS
{
    [DefaultExecutionOrder(-1)]
    public class Video : MonoBehaviour, IDisposable
    {
        private WebCamTexture webcam;
        private VideoPlayer unity_video;
        private VideoCapture video;
        private RenderTexture texture = default;

        private int mode = -1;
        [NonSerialized] public int width = default;
        [NonSerialized] public int height = default;
        [NonSerialized] public bool ended = false;
        private SynchronizationContext context = default;
        private Mat cash = new Mat();

        [SerializeField] private bool useUnity = false;
        [SerializeField] private bool isFile = false;
        [SerializeField] private int sourse = 0;
        [SerializeField] private string filename = default;

        void Awake()
        {
            if (isFile)
            {
                if (useUnity)
                {
                    unity_video = gameObject.AddComponent<VideoPlayer>();
                    unity_video.url = "file://" + filename;
                    unity_video.renderMode = VideoRenderMode.RenderTexture;

                    context = SynchronizationContext.Current;
                    mode = 2;
                }
                else
                {
                    video = new VideoCapture(filename);
                    mode = 3;
                }
            }
            else
            {
                if (useUnity)
                {
                    webcam = new WebCamTexture(WebCamTexture.devices[sourse].name);
                    context = SynchronizationContext.Current;
                    mode = 1;
                    if (!webcam.isPlaying)
                    {
                        webcam.Play();
                    }
                }
                else
                {
                    video = new VideoCapture(sourse);
                    mode = 0;
                }
            }
        }

        public async Task WaitOpen()
        {
            int t = 0;
            while (mode == -1)
            {
                await Task.Delay(100);
                t++;
                if (t > 600)
                {
                    throw new Exception("カメラが設定されていません。\nCan't set camera!");
                }
            }

            switch (mode)
            {
                case 0:
                    while (!video.IsOpened())
                    {
                        await Task.Delay(100);
                        t++;
                        if (t > 600)
                        {
                            throw new Exception("カメラを開けませんでした。\nCan't camera open!");
                        }
                    }
                    Mat tmp = new Mat();
                    video.Read(tmp);
                    while (tmp.Cols <= 0)
                    {
                        await Task.Delay(100);
                        t++;
                        if (t > 600)
                        {
                            throw new Exception("カメラを開けませんでした。\nCan't camera open!");
                        }

                        video.Read(tmp);
                    }

                    width = video.FrameWidth;
                    height = video.FrameHeight;
                    break;
                case 1:
                    while (Math.Abs(webcam.GetPixel(0, 0).r) < 0.0001f)
                    {
                        await Task.Delay(100);
                        t++;
                        if (t > 600)
                        {
                            throw new Exception("カメラを開けませんでした。\nCan't camera open!");
                        }
                    }

                    width = webcam.width;
                    height = webcam.height;
                    break;
                case 2:
                    while (!unity_video.isPrepared)
                    {
                        await Task.Delay(100);
                        t++;
                        if (t > 600)
                        {
                            throw new Exception("カメラを開けませんでした。\nCan't camera open!");
                        }
                    }
                    unity_video.Play();
                    width = (int)unity_video.width;
                    height = (int)unity_video.height;
                    texture = unity_video.texture as RenderTexture;
                    break;
                case 3:
                    while (!video.IsOpened())
                    {
                        await Task.Delay(100);
                        t++;
                        if (t > 600)
                        {
                            throw new Exception("カメラを開けませんでした。\nCan't camera open!");
                        }
                    }

                    width = video.FrameWidth;
                    height = video.FrameHeight;
                    _ = Task.Run(() => PlayLoop((int)(1000 / video.Fps)));
                    while (cash == null || cash.Cols <= 0)
                    {
                        await Task.Delay(100);
                        t++;
                        if (t > 600)
                        {
                            throw new Exception("カメラを開けませんでした。\nCan't camera open!");
                        }
                    }
                    break;
            }
        }

        private async void PlayLoop(int frame_interval)
        {
            int frame = video.FrameCount;
            for (int i = 0; i < frame; i++)
            {
                Mat tmp = new Mat();
                video.Read(tmp);
                lock (cash)
                {
                    cash = tmp.Clone();
                }

                await Task.Delay(frame_interval);
            }

            ended = true;
        }

        public Mat Read()
        {
            Mat mat = new Mat(height, width, MatType.CV_8UC3);
            switch (mode)
            {
                case 0:
                    Mat tmp0 = new Mat();
                    video.Read(tmp0);
                    mat = tmp0.Clone();

                    break;
                case 1:
                    Color32[] tex1 = new Color32[width * height];

                    context.Send((_) => { if (webcam != null) webcam.GetPixels32(tex1); }, null);

                    byte[] tmp1 = new byte[width * height * 3];
                    for (int h = 0; h < height; h++)
                    {
                        for (int w = 0; w < width; w++)
                        {
                            tmp1[3 * (h * width + w)] = tex1[tex1.Length - (h * width + width - w)].b;
                            tmp1[3 * (h * width + w) + 1] = tex1[tex1.Length - (h * width + width - w)].g;
                            tmp1[3 * (h * width + w) + 2] = tex1[tex1.Length - (h * width + width - w)].r;
                        }
                    }

                    Marshal.Copy(tmp1, 0, mat.Data, width * height * 3);
                    break;
                case 2:
                    AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(texture);
                    request.WaitForCompletion();
                    var cl = request.GetData<Color32>();
                    Color32[] tex2 = cl.ToArray();
                    byte[] tmp2 = new byte[width * height * 3];
                    for (int h = 0; h < height; h++)
                    {
                        for (int w = 0; w < width; w++)
                        {
                            tmp2[3 * (h * width + w)] = tex2[tex2.Length - (h * width + width - w)].b;
                            tmp2[3 * (h * width + w) + 1] = tex2[tex2.Length - (h * width + width - w)].g;
                            tmp2[3 * (h * width + w) + 2] = tex2[tex2.Length - (h * width + width - w)].r;
                        }
                    }

                    Marshal.Copy(tmp2, 0, mat.Data, width * height * 3);
                    break;
                case 3:
                    lock (cash)
                    {
                        mat = cash.Clone();
                    }

                    break;
            }

            return mat;
        }

        public void Dispose()
        {
            switch (mode)
            {
                case 0:
                    video.Dispose();
                    break;
                case 1:
                    webcam.Stop();
                    break;
                case 3:
                    video.Dispose();
                    break;
            }
        }

        void OnDestroy()
        {
            Dispose();
        }
    }
}
