using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ScannerBridge
{
    public static class ScannerNative
    {
        private const string DllName = "DevCapture.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDeviceCount();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int GetDeviceName(int index, StringBuilder nameBuffer);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetResolutionCount(int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetResolution(int rIndex, out int width, out int height);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetStillResolutionCount(int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetStillResolution(int rIndex, out int width, out int height);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenDevice(int index, int width, int height, nint hwnd, bool isDisplay);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenDeviceEx(int index, int width, int height, nint hwnd, bool isDisplay);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CloseDevice();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern nint CaptureFromPreview(string path, int isBarcode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int CaptureFromStill(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFormatType(int type);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetJpgQuality(int val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetColorType(int type);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCutType(int type);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetRotateAngle(int angle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TrueSize();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void BestSize();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ZoomIn();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ZoomOut();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetDelBgColor(int flag);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetDelBlackEdge(int flag);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ManualFocus();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int RecogQrBarCode(int type, StringBuilder result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetDpi(int type, int val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int AddImagePath(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int MergeImages(string destPath, int direction, int sp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int AddImageFileToPDF(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int CombineToPDF(string destPath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetVideoProcParms(int parmID, out long min, out long max, out long def, out long current);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetVideoProcParms(int parmID, int value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCameraCotrolParms(int parmID, out long min, out long max, out long def, out long current, out long flag);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetCameraCotrolParms(int parmID, int value, int flag);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ShowCameraSettingWindow();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReadCard();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint GeCardInfo(int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetAutoExp(bool flag);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetAutoCutRectNum();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern nint CaptureMultiCutImage(int index, string path);

        public delegate void WiseCaptureCallback(int steadyFlag, string imagePath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int WiseCapture(WiseCaptureCallback callback, int onOff, string folderPath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetAddMark(int isAdd);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetWaterMark(string info, int wType, int wTransp, int wPos, int wSize, int wColor);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ImageToPdf(string imgPath, string pdfPath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PicBoxMouseDown(int px, int py);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PicBoxMouseUp(int px, int py);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PicBoxMouseMove(int px, int py);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int StartAudioRecord(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int StopAudioRecord();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int StartPlayAudio(string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int StopPlayAudio();
    }

}
