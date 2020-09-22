using System;
using System.Runtime.InteropServices;

namespace kumaS.PoseNet
{
    public class PoseNet : IDisposable
    {
        private IntPtr model = default;
        private IntPtr option = default;
        private IntPtr interpreter = default;
        public int width = default;
        public int height = default;

        public PoseNet(string path, int thread)
        {
            model = PoseNetPInvok.TfLiteModelCreateFromFile(path);
            option = PoseNetPInvok.TfLiteInterpreterOptionsCreate();
            PoseNetPInvok.TfLiteInterpreterOptionsSetNumThreads(option, thread);
            interpreter = PoseNetPInvok.TfLiteInterpreterCreate(model, option);
            IntPtr input = PoseNetPInvok.TfLiteInterpreterGetInputTensor(interpreter, 0);
            width = PoseNetPInvok.TfLiteTensorDim(input, 2);
            height = PoseNetPInvok.TfLiteTensorDim(input, 1);
        }

        public int[] Run(IntPtr data)
        {
            IntPtr ptr = PoseNetPInvok.EstimatePose(interpreter, data, width, height, out int length);
            var val = new int[length];
            Marshal.Copy(ptr, val, 0, length);
            PoseNetPInvok.DeleateResult(ptr);
            return val;
        }

        public void Dispose()
        {
            PoseNetPInvok.TfLiteInterpreterDelete(interpreter);
            PoseNetPInvok.TfLiteInterpreterOptionsDelete(option);
            PoseNetPInvok.TfLiteModelDelete(model);
        }
    }
}
