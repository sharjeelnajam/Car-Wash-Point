using System.IO.Pipes;

namespace CarWashApp.Services.Scanner
{
    public static class ScannerMethods
    {
        public const string GetDeviceCount = "GetDeviceCount";
        public const string GetDeviceName = "GetDeviceName";
        public const string GetResolutionCount = "GetResolutionCount";
        public const string GetResolution = "GetResolution";
        public const string GetStillResolutionCount = "GetStillResolutionCount";
        public const string GetStillResolution = "GetStillResolution";
        public const string OpenDevice = "OpenDevice";
        public const string OpenDeviceEx = "OpenDeviceEx";
        public const string CloseDevice = "CloseDevice";
        public const string CaptureFromPreview = "CaptureFromPreview";
        public const string CaptureFromStill = "CaptureFromStill";
        public const string SetFormatType = "SetFormatType";
        public const string SetJpgQuality = "SetJpgQuality";
        public const string SetColorType = "SetColorType";
        public const string SetCutType = "SetCutType";
        public const string SetRotateAngle = "SetRotateAngle";
        public const string TrueSize = "TrueSize";
        public const string BestSize = "BestSize";
        public const string ZoomIn = "ZoomIn";
        public const string ZoomOut = "ZoomOut";
        public const string SetDelBgColor = "SetDelBgColor";
        public const string SetDelBlackEdge = "SetDelBlackEdge";
        public const string ManualFocus = "ManualFocus";
        public const string RecogQrBarCode = "RecogQrBarCode";
        public const string SetDpi = "SetDpi";
        public const string AddImagePath = "AddImagePath";
        public const string MergeImages = "MergeImages";
        public const string AddImageFileToPDF = "AddImageFileToPDF";
        public const string CombineToPDF = "CombineToPDF";
        public const string GetVideoProcParms = "GetVideoProcParms";
        public const string SetVideoProcParms = "SetVideoProcParms";
        public const string GetCameraCotrolParms = "GetCameraCotrolParms";
        public const string SetCameraCotrolParms = "SetCameraCotrolParms";
        public const string ShowCameraSettingWindow = "ShowCameraSettingWindow";
        public const string ReadCard = "ReadCard";
        public const string GeCardInfo = "GeCardInfo";
        public const string SetAutoExp = "SetAutoExp";
        public const string GetAutoCutRectNum = "GetAutoCutRectNum";
        public const string CaptureMultiCutImage = "CaptureMultiCutImage";
        public const string WiseCapture = "WiseCapture";
        public const string SetAddMark = "SetAddMark";
        public const string SetWaterMark = "SetWaterMark";
        public const string ImageToPdf = "ImageToPdf";
        public const string PicBoxMouseDown = "PicBoxMouseDown";
        public const string PicBoxMouseUp = "PicBoxMouseUp";
        public const string PicBoxMouseMove = "PicBoxMouseMove";
        public const string StartAudioRecord = "StartAudioRecord";
        public const string StopAudioRecord = "StopAudioRecord";
        public const string StartPlayAudio = "StartPlayAudio";
        public const string StopPlayAudio = "StopPlayAudio";
    }
    public class ScannerService
    {
        private readonly NamedPipeClientStream _client;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public ScannerService()
        {
            _client = new NamedPipeClientStream(".", "ScannerPipe", PipeDirection.InOut);
            _client.Connect(2000);
            _reader = new StreamReader(_client);
            _writer = new StreamWriter(_client) { AutoFlush = true };
        }

        public async Task<string> CallFunctionAsync(string funcCommand)
        {
            await _writer.WriteLineAsync(funcCommand);
            return await _reader.ReadLineAsync();
        }

        public void Dispose()
        {
            _reader.Dispose();
            _writer.Dispose();
            _client.Dispose();
        }
        //public async static Task<string> CallFunctionAsync(string funcCommand)
        //{
        //    using var client = new NamedPipeClientStream(".", "ScannerPipe", PipeDirection.InOut);
        //    await client.ConnectAsync(2000);

        //    using var writer = new StreamWriter(client) { AutoFlush = true };
        //    using var reader = new StreamReader(client);

        //    await writer.WriteLineAsync(funcCommand);
        //    return await reader.ReadLineAsync();
        //}
    }
}
