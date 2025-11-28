using Common;
using System.Collections.Generic;
using System.IO;

namespace ImgGrabber
{


    public class Recipe
    {
        private readonly IniFile iniFile = new IniFile();
        private const int lightCount = 10;

        private string fileName = "Base.ini";
        private int startDelay = 500;
        private List<int> listLightValue = new List<int>();

        private int parentWidth = 0;
        private int parentHeight = 0;

        private string cameraData = "UserSet1";

        private ListROI listChildImageRoi = new ListROI();


        public string FileName { get => fileName; set => fileName = value; }
        public int StartDelay { get => startDelay; set => startDelay = value; }
        public ListROI ListChildImageRoi { get => listChildImageRoi; set => listChildImageRoi = value; }


        public string CameraData { get => cameraData; set => cameraData = value; }
        public List<int> ListLightValue { get => listLightValue; set => listLightValue = value; }
        public int ParentWidth { get => parentWidth; set => parentWidth = value; }
        public int ParentHeight { get => parentHeight; set => parentHeight = value; }

        public void Save(string fileName)
        {
            if (!File.Exists(fileName))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    fs.Close();
                }
            }

            iniFile.Load(fileName);

            int i = 1;
            foreach (int value in listLightValue)
            {
                iniFile.SetValue("Light", $"CH{i++}_Value", value);
            }

            iniFile.SetValue("Grabber", "GrabStartDelay", StartDelay);
            //iniFile.SetValue("Grabber", "CameraData", CameraData);

            iniFile.SetValue("ParentImage", "GrabPixelWidth", ParentWidth);
            iniFile.SetValue("ParentImage", "GrabPixelHeight", ParentHeight);

            iniFile.SetValue("ChildImage", "ImageCount", ListChildImageRoi.Count);

            int index = 0;
            foreach (ROI child in ListChildImageRoi)
            {
                iniFile.SetValue($"ChildImage{index}", "CellIndex", child.CellNo);
                iniFile.SetValue($"ChildImage{index}", "Name", child.Name);
                iniFile.SetValue($"ChildImage{index}", "X", child.X);
                iniFile.SetValue($"ChildImage{index}", "Y", child.Y);
                iniFile.SetValue($"ChildImage{index}", "Width", child.Width);
                iniFile.SetValue($"ChildImage{index}", "Height", child.Height);
                index++;
            }
        }

        public bool Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                iniFile.Load(fileName);

                FileName = Path.GetFileName(fileName);

                listLightValue.Clear();

                for (int i = 1; i <= lightCount;)
                {
                    int value = 0;
                    value = iniFile.GetValue("Light", string.Format("CH{0}_Value", i++), value);
                    listLightValue.Add(value);
                }

                StartDelay = iniFile.GetValue("Grabber", "GrabStartDelay", StartDelay);
                //CameraData = iniFile.GetValue("Grabber", "CameraData", CameraData);

                ParentWidth = iniFile.GetValue("ParentImage", "GrabPixelWidth", ParentWidth);
                ParentHeight = iniFile.GetValue("ParentImage", "GrabPixelHeight", ParentHeight);

                int childBufferCount = iniFile.GetValue("ChildImage", "ImageCount", 1);

                ListChildImageRoi.Clear();
                for (int i = 0; i < childBufferCount; i++)
                {
                    ROI child = new ROI(ListChildImageRoi);
                    child.CellNo = iniFile.GetValue($"ChildImage{i}", "CellIndex", child.CellNo);
                    child.Name = iniFile.GetValue($"ChildImage{i}", "Name", child.Name);
                    child.X = iniFile.GetValue($"ChildImage{i}", "X", 0);
                    child.Y = iniFile.GetValue($"ChildImage{i}", "Y", (ParentHeight / childBufferCount) * i);
                    child.Width = iniFile.GetValue($"ChildImage{i}", "Width", ParentWidth);
                    child.Height = iniFile.GetValue($"ChildImage{i}", "Height", ParentHeight / childBufferCount);

                    ListChildImageRoi.Add(child);
                }

                return true;
            }
            else
            {
                Save(fileName);

                return false;
            }
        }
    }
}
