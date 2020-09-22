using System;
using System.Runtime.InteropServices;

namespace kumaS.PoseNet
{
    public static class PoseNetPInvok
    {
        [DllImport("tensorflowlite_c")]
        public static extern IntPtr TfLiteModelCreateFromFile(string path);

        [DllImport("tensorflowlite_c")]
        public static extern IntPtr TfLiteInterpreterOptionsCreate();

        [DllImport("tensorflowlite_c")]
        public static extern void TfLiteInterpreterOptionsSetNumThreads(IntPtr option, int num);

        [DllImport("tensorflowlite_c")]
        public static extern IntPtr TfLiteInterpreterCreate(IntPtr model, IntPtr option);

        [DllImport("posenet")]
        public static extern IntPtr EstimatePose(IntPtr model, IntPtr data, int width, int height, out int length);

        [DllImport("tensorflowlite_c")]
        public static extern IntPtr Test(IntPtr model, IntPtr data, int width, int height, out int length);

        [DllImport("tensorflowlite_c")]
        public static extern void TfLiteInterpreterDelete(IntPtr interpreter);

        [DllImport("tensorflowlite_c")]
        public static extern void TfLiteInterpreterOptionsDelete(IntPtr option);

        [DllImport("tensorflowlite_c")]
        public static extern void TfLiteModelDelete(IntPtr model);

        [DllImport("tensorflowlite_c")]
        public static extern int TfLiteTensorDim(IntPtr tensor, int dim);

        [DllImport("tensorflowlite_c")]
        public static extern IntPtr TfLiteInterpreterGetInputTensor(IntPtr interpreter, int dim);

        [DllImport("posenet")]
        public static extern void DeleateResult(IntPtr result);
    }
}
