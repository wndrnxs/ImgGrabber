namespace ImgGrabber
{
    internal class Feature
    {
        internal enum Name
        {
            ExposureTime,
            Gain,
            AcquisitionLineRate,
            ReverseX,

            flatfieldCorrectionMode,
            flatfieldCorrectionAlgorithm,
            flatfieldCalibrationTarget,
            flatfieldCalibrationROIOffsetX,
            flatfieldCalibrationROIWidth,
            flatfieldCalibrationFPN,
            flatfieldCalibrationPRNU,
            flatfieldCalibrationPRNUStatus,
            flatfieldCalibrationSampleSize,
            UserSetDefaultSelector,
            UserSetSelector,
            UserSetLoad,
            UserSetSave

        }

        private double exposureTime = 0;
        private int lineRate = 0;
        private bool reverseX = false;
        private bool fFC_ModeOn = false;
        private string fFC_Algorithm = "";
        private int fFC_Target = 0;
        private int fFC_RoiOffsetX = 0;
        private int fFC_RoiWidth = 0;
        private string fFC_PRNUStatus = "";
        private string userSetDefault = "";

        public double ExposureTime { get => exposureTime; internal set => exposureTime = value; }
        public double Gain { get => exposureTime; internal set => exposureTime = value; }
        public int LineRate { get => lineRate; internal set => lineRate = value; }
        public bool ReverseX { get => reverseX; internal set => reverseX = value; }
        public bool FFC_ModeOn { get => fFC_ModeOn; internal set => fFC_ModeOn = value; }
        public string FFC_Algorithm { get => fFC_Algorithm; internal set => fFC_Algorithm = value; }
        public int FFC_Target { get => fFC_Target; internal set => fFC_Target = value; }
        public int FFC_RoiOffsetX { get => fFC_RoiOffsetX; internal set => fFC_RoiOffsetX = value; }
        public int FFC_RoiWidth { get => fFC_RoiWidth; internal set => fFC_RoiWidth = value; }
        public string FFC_PRNUStatus { get => fFC_PRNUStatus; internal set => fFC_PRNUStatus = value; }
        public string UserSetDefault { get => userSetDefault; internal set => userSetDefault = value; }
    }
}