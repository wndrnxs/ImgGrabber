using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ImgGrabber
{
    public enum ChildBufferID : int
    {
        SourceImage,
        RoiArea
    }

    public partial class FormSetup : Form
    {
        private readonly FormMain parent;
        private MilControl grabControl;
        private ILightControl lightControl;
        private Recipe recipe;
        private Feature feature = new Feature();
        private readonly ImageViewer sourceImageViewer = new ImageViewer();
        private readonly ImageViewer roiImageViewer = new ImageViewer();
        //private readonly ImagePattern sourceImagePattern;
        private ROI FFCAreaRectPattern;
        private bool bLive;
        private string imagePath;
        private string recipePath;
        private bool comboBoxAlgorithmSelectedIndexFlag;
        private bool bDisplayFfcRoi;
        private int setLightValue = 0;
        private readonly ColorPalette Gpal = new Bitmap(1, 1, PixelFormat.Format8bppIndexed).Palette;

        private int SetLightValue
        {
            get => setLightValue;
            set
            {
                if (value < 0)
                {
                    setLightValue = 0;
                }
                else if (value > lightControl.MaxValue)
                {
                    setLightValue = lightControl.MaxValue;
                }
                else
                {
                    setLightValue = value;
                }

                trackBarLightValue.Value = setLightValue;
                textBoxLightValue.Text = setLightValue.ToString();
            }
        }

        public string ImagePath { get => imagePath; set => imagePath = value; }
        public string RecipePath { get => recipePath; set => recipePath = value; }
        public bool LiveOn { get => bLive; set => bLive = value; }

        public FormSetup(FormMain parent)
        {
            InitializeComponent();
            this.parent = parent;

            Controls.Add(sourceImageViewer);
            sourceImageViewer.Parent = panelMainDisplay;
            roiImageViewer.Parent = pictureBoxSizeConfig;
            roiImageViewer.EventSelectedROI += RoiImageViewer_SelectedROI;
            roiImageViewer.EventCreateROI += RoiImageViewer_CreateRemoveROI;
            roiImageViewer.EventRemoveROI += RoiImageViewer_CreateRemoveROI;

            for (int i = 0; i < 256; i++)
            {
                Gpal.Entries[i] = Color.FromArgb(i, i, i);
            }
        }

        private void RoiImageViewer_CreateRemoveROI()
        {
            labelRoiCount.Text = recipe.ListChildImageRoi.Count.ToString();
            propertyGridRoi.SelectedObject = null;
        }

        private void RoiImageViewer_SelectedROI(ROI roi)
        {
            propertyGridRoi.SelectedObject = roi;
        }

        private void GrabControl_PRNUEvent(string status)
        {
            labelFfcStatus.Invoke(new Action(() =>
            {
                labelFfcStatus.Text = status;
                feature.FFC_PRNUStatus = status;
            }));
        }
        internal void Init(MilControl grabControl, ILightControl lightControl, Recipe recipe)
        {
            this.grabControl = grabControl;
            this.grabControl.GrabEndEvent += new GrabEndDelegate(GrabEnd);
            this.grabControl.GrabHookEvent += new GrabHookDelegate(GrabProcess);
            this.grabControl.PRNUEvent += new PRNUDelegate(GrabControl_PRNUEvent);

            textBoxDcfFile.Text = grabControl.DcfFileName;

            this.lightControl = lightControl;

            for (int i = 1; i <= lightControl.LightChannelCount; i++)
            {
                RadioButton Channel = Controls.Find("radioButtonChannel" + i, true).FirstOrDefault() as RadioButton;
                Channel.Visible = true;
                Channel.Text = parent.UIChange("radioButtonChannel" + i);

                Label LightState = Controls.Find("labelLightState" + i, true).FirstOrDefault() as Label;
                LightState.Visible = true;
                LightState.Text = parent.UIChange("labelLightState" + i);

                TextBox LightTemp = Controls.Find("textBoxLightTemp" + i, true).FirstOrDefault() as TextBox;
                LightTemp.Visible = true;

                TextBox LightSetValue = Controls.Find("textBoxLightSetValue" + i, true).FirstOrDefault() as TextBox;
                LightSetValue.Visible = true;
            }

            trackBarLightValue.Minimum = 0;
            trackBarLightValue.Maximum = lightControl.MaxValue;
            trackBarLightValue.Value = 0;
            textBoxLightValue.Text = "0";
            radioButtonChannelAll.Checked = true;

            roiImageViewer.ListRoi = recipe.ListChildImageRoi;
            
            SetRecipe(recipe);

            if (grabControl.Simulation)
            {
                labelGrabberSimulation.Visible = true;
            }
            else
            {
                labelGrabberSimulation.Visible = false;

                if (grabControl.UseFeature)
                {
                    grabControl.GetFeature(ref feature);
                    RefreshFeature(feature);
                    groupBoxCameraFeature.Visible = true;
                }
                else
                {
                    groupBoxCameraFeature.Visible = false;
                }
            }
        }
        private void buttonLightOn_Click(object sender, EventArgs e)
        {
            lightControl.LightOnOff(GetLightChannel(), true);
            parent.Logging("Light On");
        }
        private void buttonLightOff_Click(object sender, EventArgs e)
        {
            lightControl.LightOnOff(GetLightChannel(), false);
            parent.Logging("Light Off");
        }
        private void buttonValueChange_Click(object sender, EventArgs e)
        {
            int ch = GetLightChannel();
            if (ch == 0)
            {
                for (int i = 1; i <= lightControl.LightChannelCount; i++)
                {
                    lightControl.SetLightValue(i, SetLightValue);
                }
            }
            else
            {
                lightControl.SetLightValue(ch, SetLightValue);
            }
        }
        private void trackBarLightValue_Scroll(object sender, EventArgs e)
        {
            textBoxLightValue.Text = trackBarLightValue.Value.ToString();
        }

        private void buttonDcfFileOpen_Click(object sender, EventArgs e)
        {
            if (!grabControl.Simulation)
            {
                using (FileDialog fileDialog = new OpenFileDialog()
                {
                    Title = "DCF file Load",
                    FileName = "DCF",
                    Filter = "DCF file (*.dcf) | *.dcf; | 모든 파일 (*.*) | *.*",
                    InitialDirectory = Directory.GetCurrentDirectory()
                })
                {
                    DialogResult dr = fileDialog.ShowDialog();

                    if (dr == DialogResult.OK)
                    {
                        string grabberDcfFileName = Path.GetFileName(fileDialog.FileName);
                        textBoxDcfFile.Text = grabberDcfFileName;

                        grabControl.Free();
                        grabControl.Initialize(grabberDcfFileName, grabControl.BoardName);
                        SetRecipe(recipe);
                    }
                }
            }
        }

        private void SetRecipe(Recipe recipe)
        {
            textBoxRecipeFileName.Text = recipe.FileName;

            textBoxGrabDelay.Text = recipe.StartDelay.ToString();
            textBoxSetGrabSizeX.Text = recipe.ParentWidth.ToString();
            textBoxSetGrabSizeY.Text = recipe.ParentHeight.ToString();

            //if (!grabControl.Simulation)
            {
                grabControl.GrabDelay = recipe.StartDelay;

                if (grabControl.SetGrabFrameBuffer(recipe.ParentWidth, recipe.ParentHeight))
                {
                    if (SetChildBufImage(recipe))
                    {
                        textBoxGrabUnitSizeX.Text = grabControl.FrameSizeX.ToString();
                        textBoxGrabUnitSizey.Text = grabControl.FrameSizeY.ToString();
                        textBoxGrabBufferSizeX.Text = grabControl.ImageSizeX.ToString();
                        textBoxGrabBufferSizeY.Text = grabControl.ImageSizeY.ToString();
                    }
                    else
                    {
                        parent.Logging("Child image size out of Source image size.");
                        parent.Process(FormMain.ProcessState.Alarm);
                    }
                }
                else
                {
                    parent.Logging("Grab Controller is not Intialize!!");
                    parent.Process(FormMain.ProcessState.Alarm);
                }
            }


            lightControl.ChangeValue(recipe.ListLightValue);
            sourceImageViewer.ResetSize();

            labelRoiCount.Text = recipe.ListChildImageRoi.Count.ToString();

            this.recipe = recipe;

        }

        private bool SetChildBufImage(Recipe recipe)
        {
            grabControl.ChildBufFree();

            parent.MainImageViewerCreate(recipe);

            int index = 0;
            foreach (ROI child in recipe.ListChildImageRoi)
            {
                var viewer = parent.GetMainViewer(index);
                grabControl.AddChildBufId(index, child.X, child.Y, child.Width, child.Height, viewer.Viewer);
                viewer.SetInfo(child.Index, child.Name, child.Size);
                index++;
            }

            sourceImageViewer.SetSize(new Size(recipe.ParentWidth, recipe.ParentHeight));
            roiImageViewer.SetSize(new Size(recipe.ParentWidth, recipe.ParentHeight));

            grabControl.AddChildBufId(recipe.ListChildImageRoi.Count + (int)ChildBufferID.SourceImage, 0, 0, recipe.ParentWidth, recipe.ParentHeight, sourceImageViewer.Viewer);
            grabControl.AddChildBufId(recipe.ListChildImageRoi.Count + (int)ChildBufferID.RoiArea, 0, 0, recipe.ParentWidth, recipe.ParentHeight, roiImageViewer.Viewer);
            sourceImageViewer.SetInfo(0, "Source Image", new Size(recipe.ParentWidth, recipe.ParentHeight));

            return true;
        }
        private void buttonGrabDelaySet_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxGrabDelay.Text, out int result))
            {
                recipe.StartDelay = result;
                grabControl.GrabDelay = result;
            }
        }
        private void buttonRoiRecipeApply_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBoxSetGrabSizeX.Text, out int parentWidth))
            {
                MessageBox.Show("Invalid value [Grab Size X]");
                return;
            }

            if (!int.TryParse(textBoxSetGrabSizeY.Text, out int parentHeight))
            {
                MessageBox.Show("Invalid value [Grab Size Y]");
                return;
            }

            int bufferSizeX;
            int bufferSizeY;
            if (grabControl.Simulation)
            {
                bufferSizeX = recipe.ParentWidth;
                bufferSizeY = recipe.ParentHeight;
            }
            else
            {
                bufferSizeX = grabControl.ImageSizeX;
                bufferSizeY = grabControl.ImageSizeY;
            }

            int index = 0;
            foreach (var child in recipe.ListChildImageRoi)
            {
                if (child.Width > bufferSizeX)
                {
                    MessageBox.Show($"Invalid value Child {index} ROI Height [child.Width : {child.Width} > bufferSizeY :{bufferSizeX}]");
                    return;
                }

                if (child.Height > bufferSizeY)
                {
                    MessageBox.Show($"Invalid value Child {index} ROI Height [child.Height : {child.Height} > bufferSizeY :{bufferSizeY}]");
                    return;
                }

                index++;
            }

            parentHeight = parentHeight % bufferSizeY != 0 ? (parentHeight / bufferSizeY + 1) * bufferSizeY : parentHeight;
            textBoxSetGrabSizeY.Text = parentHeight.ToString();

            recipe.ParentWidth = parentWidth;
            recipe.ParentHeight = parentHeight;

            SetRecipe(recipe);
        }
        private void buttonOpenFilePath_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(ImagePath);
        }
        private void buttonSavedImageLoad_Click(object sender, EventArgs e)
        {
            using (FileDialog fileDialog = new OpenFileDialog()
            {
                Title = "Image file Save",
                FileName = "",
                Filter = "bmp file (*.bmp) | *.bmp; | 모든 파일 (*.*) | *.*",
                InitialDirectory = imagePath
            })
            {
                DialogResult dr = fileDialog.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    using (Bitmap image = new Bitmap(fileDialog.FileName))
                    {
                        //if(sourceImagePattern != null)
                        //{
                        //    sourceImageViewer.RemoveRoi(sourceImagePattern);
                        //    sourceImagePattern = null;
                        //}
                        //sourceImagePattern = new ImagePattern((Bitmap)new Bitmap(image).Clone(), new Point(0, 0));
                        //sourceImageViewer.AddPattern(sourceImagePattern);
                        //sourceImageViewer.ResetSize();
                    }
                }
            }
        }
        private void buttonSourceImageSave_Click(object sender, EventArgs e)
        {
            using (FileDialog fileDialog = new SaveFileDialog()
            {
                Title = "Image file Save",
                FileName = DateTime.Now.ToString("yyMMdd_HHmmss"),
                Filter = "bmp file (*.bmp) | *.bmp; | 모든 파일 (*.*) | *.*",
                InitialDirectory = imagePath
            })
            {
                DialogResult dr = fileDialog.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    //if (sourceImagePattern != null)
                    //{
                    //    if (sourceImageViewer.ContainsPattern(sourceImagePattern))
                    //    {
                    //        sourceImagePattern.Image.Save(fileDialog.FileName);
                    //    }
                    //}
                    //else
                    //{
                    //    if (!grabControl.Simulation)
                    //    {
                    //        grabControl.Save(fileDialog.FileName);
                    //    }
                    //}
                }
            }
        }
        private void buttonExposureTimeSet_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBoxExposureTime.Text, out double ExposureTime))
            {
                feature.ExposureTime = ExposureTime;
                if (!grabControl.Simulation)
                {
                    grabControl.SetFeature(Feature.Name.ExposureTime, ExposureTime);
                }               
            }
        }
        private void buttonGainSet_Click(object sender, EventArgs e)
        {
            if (double.TryParse(textBoxGain.Text, out double Gain))
            {
                feature.Gain = Gain;
                if (!grabControl.Simulation)
                {
                    grabControl.SetFeature(Feature.Name.Gain, Gain);
                }
            }
        }
        private void buttonLineRateSet_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxLineRate.Text, out int LineRate))
            {
                feature.LineRate = LineRate;
                if (!grabControl.Simulation)
                {
                    grabControl.SetFeature(Feature.Name.AcquisitionLineRate, LineRate);
                }
            }
        }
        private void buttonReverseX_Click(object sender, EventArgs e)
        {
            if (feature.ReverseX == false)
            {
                feature.ReverseX = true;
                grabControl.SetFeature(Feature.Name.ReverseX, "On");

                buttonReverseX.Text = "On";
                buttonReverseX.BackColor = Color.Lime;
            }
            else
            {
                feature.ReverseX = false;
                grabControl.SetFeature(Feature.Name.ReverseX, "Off");

                buttonReverseX.Text = "Off";
                buttonReverseX.BackColor = Color.Gray;
            }
        }
        private void buttonFfcUse_Click(object sender, EventArgs e)
        {
            if (feature.FFC_ModeOn)
            {
                feature.FFC_ModeOn = false;
                panelFFC.Visible = false;
                buttonFfcUse.Text = "Off";
                buttonFfcUse.BackColor = Color.Gray;
                grabControl.SetFeature(Feature.Name.flatfieldCorrectionMode, "Off");
            }
            else
            {
                feature.FFC_ModeOn = true;
                panelFFC.Visible = true;
                buttonFfcUse.Text = "On";
                buttonFfcUse.BackColor = Color.Lime;
                grabControl.SetFeature(Feature.Name.flatfieldCorrectionMode, "On");
            }

        }
        private void buttonGVTargetSet_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxGVTarget.Text, out int GVTarget))
            {    
                if (0 > GVTarget || 255 < GVTarget)
                {
                    MessageBox.Show("Out of range for the target value [Range : 0 ~ 255]");
                    return;
                }
                else
                {
                    feature.FFC_Target = GVTarget;
                    if (!grabControl.Simulation)
                    {
                        grabControl.SetFeature(Feature.Name.flatfieldCalibrationTarget, GVTarget);
                    }
                }
            }
        }
        private void buttonROIOffsetXSet_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxROIOffsetX.Text, out int ROIOffsetX))
            {
                if (grabControl.Simulation)
                {
                    feature.FFC_RoiOffsetX = ROIOffsetX;
                }
                else
                {
                    if (grabControl.ImageSizeX < feature.FFC_RoiWidth + ROIOffsetX - 1)
                    {
                        MessageBox.Show($"Out of range for the Size X [Size: {grabControl.ImageSizeX}]");
                        textBoxROIOffsetX.Text = (grabControl.ImageSizeX - feature.FFC_RoiWidth + 1).ToString();
                        return;
                    }
                    else
                    {
                        if (1 > ROIOffsetX)
                        {
                            MessageBox.Show("Offset value must be greater than 0.");
                            textBoxROIOffsetX.Text = "1";
                            return;
                        }

                        feature.FFC_RoiOffsetX = ROIOffsetX;
                        grabControl.SetFeature(Feature.Name.flatfieldCalibrationROIOffsetX, ROIOffsetX);
                    }
                }
                DrawFFCArea();
            }
        }

        private void buttonROIWidthSet_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxROIWidth.Text, out int ROIWidth))
            {
                if (grabControl.Simulation)
                {
                    feature.FFC_RoiWidth = ROIWidth;
                }
                else
                {
                    if (grabControl.ImageSizeX + 1 < feature.FFC_RoiOffsetX + ROIWidth)
                    {
                        MessageBox.Show($"Out of range for the Size X [Size: {grabControl.ImageSizeX}]");
                        textBoxROIWidth.Text = (grabControl.ImageSizeX - feature.FFC_RoiOffsetX + 1).ToString();
                        return;
                    }
                    else
                    {
                        feature.FFC_RoiWidth = ROIWidth;
                        grabControl.SetFeature(Feature.Name.flatfieldCalibrationROIWidth, ROIWidth); 
                    }
                }

                DrawFFCArea();
            }
        }
        private void buttonPrnuExecute_Click(object sender, EventArgs e)
        {
            grabControl.UseErrorMsg(false);
            grabControl.SetFeature(Feature.Name.flatfieldCalibrationPRNU);
            WaitingForFFC();
            grabControl.UseErrorMsg(true);
        }
        private void buttonFpnExecute_Click(object sender, EventArgs e)
        {
            grabControl.UseErrorMsg(false);
            grabControl.SetFeature(Feature.Name.flatfieldCalibrationFPN);
            //WaitingForFFC();
            grabControl.UseErrorMsg(true);
        }
        private void buttonInitialize_Click(object sender, EventArgs e)
        {
            grabControl.UseErrorMsg(false);
            grabControl.SetFeature(Feature.Name.flatfieldCorrectionMode, "Initialize");
            //labelFfcStatus.Text = "Reset";
            //WaitingForFFC();
            grabControl.UseErrorMsg(true);
        }
        private void comboBoxAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxAlgorithmSelectedIndexFlag == false)
            {
                comboBoxAlgorithmSelectedIndexFlag = true;
                return;
            }

            if (comboBoxAlgorithm.SelectedIndex == 0)
            {
                grabControl.SetFeature(Feature.Name.flatfieldCorrectionAlgorithm, "Basic");
            }
            else
            {
                grabControl.SetFeature(Feature.Name.flatfieldCorrectionAlgorithm, "LowPass");
            }
        }
        private void WaitingForFFC()
        {
            grabControl.StartPRNU();
        }
        private void buttonRefreshFeature_Click(object sender, EventArgs e)
        {
            grabControl.UseErrorMsg(false);

            grabControl.GetFeature(ref feature);

            grabControl.UseErrorMsg(true);
            RefreshFeature(feature);
        }
        private void buttonFeatureSave_Click(object sender, EventArgs e)
        {
            string cameraData = comboBoxCameraData.Text;
            grabControl.UseErrorMsg(false);

            grabControl.SetFeature(Feature.Name.UserSetSelector, cameraData);
            grabControl.SetFeature(Feature.Name.UserSetSave);

            grabControl.SetFeature(Feature.Name.UserSetDefaultSelector, cameraData);

            grabControl.UseErrorMsg(true);
        }
        private void buttonFeatureLoad_Click(object sender, EventArgs e)
        {
            string cameraData = comboBoxCameraData.Text;
            grabControl.UseErrorMsg(false);

            grabControl.SetFeature(Feature.Name.UserSetSelector, cameraData);
            grabControl.SetFeature(Feature.Name.UserSetLoad);

            grabControl.SetFeature(Feature.Name.UserSetDefaultSelector, cameraData);

            grabControl.UseErrorMsg(true);

            grabControl.GetFeature(ref feature);

            RefreshFeature(feature);
        }
        private void buttonLive_Click(object sender, EventArgs e)
        {
            Live(!bLive);
        }
        private void buttonSingleShot_Click(object sender, EventArgs e)
        {
            lightControl.LightAllOnOff(true);

            if (bLive)
            {
                bLive = false;
                buttonLive.Text = "Live";
                grabControl.Live(false);
            }

            grabControl.SingleGrab();
        }
        
        private void buttonDisplayFfcRoi_Click(object sender, EventArgs e)
        {
            DisplayFfcRoi(!bDisplayFfcRoi);
        }
        private void DisplayFfcRoi(bool value)
        {
            bDisplayFfcRoi = value;

            buttonDisplayFfcRoi.Text = bDisplayFfcRoi ? "Clear" : "Display";
            DrawFFCArea();
        }
        private void RefreshFeature(Feature feature)
        {
            textBoxExposureTime.Text = feature.ExposureTime.ToString();
            textBoxGain.Text = feature.Gain.ToString();
            textBoxLineRate.Text = feature.LineRate.ToString();

            bool reverseXOn = feature.ReverseX;
            buttonReverseX.Text = reverseXOn ? "On" : "Off";
            buttonReverseX.BackColor = reverseXOn ? Color.Lime : Color.Gray;

            ///5.ffc featureValue Update
            bool ffcOn = feature.FFC_ModeOn;
            buttonFfcUse.Text = ffcOn ? "On" : "Off";
            buttonFfcUse.BackColor = ffcOn ? Color.Lime : Color.Gray;

            panelFFC.Visible = ffcOn;
            comboBoxAlgorithmSelectedIndexFlag = false;
            comboBoxAlgorithm.SelectedIndex = feature.FFC_Algorithm.ToString().Equals("Basic") ? 0 : 1;
            textBoxGVTarget.Text = feature.FFC_Target.ToString();
            textBoxROIOffsetX.Text = feature.FFC_RoiOffsetX.ToString();
            textBoxROIWidth.Text = feature.FFC_RoiWidth.ToString();
            comboBoxCameraData.Text = feature.UserSetDefault;
            labelFfcStatus.Text = feature.FFC_PRNUStatus;

            if (feature.ReverseX)
            {
                buttonReverseX.Text = "On";
                buttonReverseX.BackColor = Color.Lime;
            }
            else
            {
                buttonReverseX.Text = "Off";
                buttonReverseX.BackColor = Color.Gray;
            }
        }

        public void Live(bool bLive)
        {
            this.bLive = bLive;

            if (bLive)
            {
                buttonLive.Text = "Stop";
                grabControl.Live(true);
            }
            else
            {
                buttonLive.Text = "Live";
                grabControl.Live(false);
            }
        }
        internal void SetAuthority(FormAuthority.Level level)
        {
            switch (level)
            {
                case FormAuthority.Level.Operator:
                case FormAuthority.Level.Maintenance:
                    {
                        groupBoxCameraFeature.Visible = false;
                        roiImageViewer.UseRoiEdit(false);
                    }
                    break;
                case FormAuthority.Level.Engineer:
                    {
                        groupBoxCameraFeature.Visible = true;
                        roiImageViewer.UseRoiEdit(true);
                    }
                    break;
            }
        }
        //private void ImageScaleReset(Size imageSize, Size panelSize)
        //{           
        //    if (customImageViewSource != null)
        //    {
        //        float compareIamgeXYScale = imageSize.Height / (float)imageSize.Width;
        //        float comparePanelXYScale = panelSize.Height / (float)panelSize.Width;

        //        if (grabControl.ImageSizeX != 0 && grabControl.ImageSizeY != 0)
        //        {
        //            customImageViewSource.InitScale =   compareIamgeXYScale > comparePanelXYScale ?
        //                                                panelSize.Height / (float)imageSize.Height : panelSize.Width / (float)imageSize.Width;
        //            customImageViewSource.MapReset();
        //        }
        //    }
        //}

        private void GrabEnd()
        {
            if (Enabled)
            {
                //if (sourceImagePattern != null)
                //{
                //    sourceImageViewer.RemovePattern(sourceImagePattern);
                //    sourceImagePattern = null;
                //}
                //sourceImageViewer.ResetSize();
            }
        }

        private void GrabProcess(IntPtr bufferAddress)
        {
            if (Enabled)
            {
                //int width = grabControl.BufferSizeX;
                //int height = grabControl.BufferSizeY;

                //using (Bitmap temp = new Bitmap(width, height, width, PixelFormat.Format8bppIndexed, bufferAddress))
                //{
                //    temp.Palette = Gpal;
                //    sourceImage = new Bitmap(temp);

                //}

                //if (sourceImagePattern != null)
                //{
                //    sourceImageViewer.RemoveRoi(sourceImagePattern);
                //    sourceImagePattern = null;
                //}
                //sourceImageViewer.ResetSize();
            }
        }

        internal void UpdateLightTemp(int index, object value)
        {
            if (!Enabled)
            {
                return;
            }

            if (value is int temp)
            {
                TextBox textBoxLightTemp = Controls.Find("textBoxLightTemp" + index, true).FirstOrDefault() as TextBox;
                textBoxLightTemp.Invoke(new Action(() =>
                {
                    textBoxLightTemp.Text = temp.ToString();
                }));
            }
        }
        internal void UpdateLightState(int index, object value)
        {
            if (!Enabled)
            {
                return;
            }

            if (value is bool onoff)
            {
                Label labelLightState = Controls.Find("labelLightState" + index, true).FirstOrDefault() as Label;
                labelLightState.Invoke(new Action(() =>
                {
                    labelLightState.BackColor = onoff ? Color.Lime : Color.Gray;
                }));
            }
        }

        internal void UpdateLightSetValue(int index, object value)
        {
            if (!Enabled)
            {
                return;
            }

            if (value is int setValue)
            {
                recipe.ListLightValue[index - 1] = setValue;

                TextBox textBoxLightSetValue = Controls.Find("textBoxLightSetValue" + index, true).FirstOrDefault() as TextBox;
                textBoxLightSetValue.Invoke(new Action(() =>
                {
                    textBoxLightSetValue.Text = setValue.ToString();
                }));
            }
        }

        private int GetLightChannel()
        {
            for (int index = 1; index <= lightControl.LightChannelCount; index++)
            {
                RadioButton radioButtonChannel = Controls.Find("radioButtonChannel" + index, true).FirstOrDefault() as RadioButton;
                if (radioButtonChannel.Checked)
                {
                    return index;
                }
            }

            return 0;//all
        }

        private void buttonLightValueUp_Click(object sender, EventArgs e)
        {
            SetLightValue++;
        }

        private void buttonLightValueDown_Click(object sender, EventArgs e)
        {
            SetLightValue--;
        }

        private void radioButtonChannel_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton.Checked)
            {
                int ch = GetLightChannel();
                if (ch != 0)
                {
                    if (lightControl.IsOpen)
                    {
                        SetLightValue = lightControl.GetDimmingValue(ch);
                    }
                }
            }

        }

        private void textBoxLightValue_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxLightValue.Text, out int value))
            {
                SetLightValue = value;
            }
        }

        private void buttonRecipeFileOpen_Click(object sender, EventArgs e)
        {
            //if (!grabControl.Simulation)
            {
                FileDialog fileDialog = new OpenFileDialog()
                {
                    Title = "Recipe file Load",
                    FileName = "",
                    Filter = "Recipe file (*.ini) | *.ini; | 모든 파일 (*.*) | *.*",
                    InitialDirectory = recipePath
                };

                DialogResult dr = fileDialog.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    recipe.Load(fileDialog.FileName);
                    SetRecipe(recipe);
                }
            }
        }

        private void buttonRecipeFileSave_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Do you save the file? [{recipe.FileName}]\nIf you save it, the existing file will be overwrite.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //if (!grabControl.Simulation)
                {
                    FileDialog fileDialog = new SaveFileDialog()
                    {
                        Title = "Recipe file Save",
                        FileName = recipe.FileName,
                        Filter = "Recipe file (*.ini) | *.ini; | 모든 파일 (*.*) | *.*",
                        InitialDirectory = recipePath
                    };

                    DialogResult dr = fileDialog.ShowDialog();

                    if (dr == DialogResult.OK)
                    {
                        recipe.Save(fileDialog.FileName);
                        recipe.FileName = Path.GetFileName(fileDialog.FileName);
                        textBoxRecipeFileName.Text = recipe.FileName;
                    }
                }
            }
        }

        private void DrawFFCArea()
        {
            if (bDisplayFfcRoi)
            {
                int left, right;
                if (feature.ReverseX == false)
                {
                    left = feature.FFC_RoiOffsetX;
                    right = feature.FFC_RoiOffsetX + feature.FFC_RoiWidth;
                }
                else
                {
                    right = grabControl.ImageSizeX - feature.FFC_RoiOffsetX;
                    left = right - feature.FFC_RoiWidth;
                }

                if (FFCAreaRectPattern != null)
                {
                    sourceImageViewer.RemoveRoi(FFCAreaRectPattern);
                }

                FFCAreaRectPattern = new ROI(null, new Rectangle(left, 0, right - left, sourceImageViewer.ImageSize.Height));
                sourceImageViewer.AddRoi(FFCAreaRectPattern);
            }
            else
            {
                if (FFCAreaRectPattern != null)
                {
                    sourceImageViewer.RemoveRoi(FFCAreaRectPattern);
                }
            }
        }
        private void propertyGridRoi_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            roiImageViewer.ReDraw();
        }
    }
}
