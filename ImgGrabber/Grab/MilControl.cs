using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImgGrabber
{
    public delegate void GrabStartDelegate();
    public delegate void TriggerDelegate(bool isOn);
    public delegate void PRNUDelegate(string status);
    public delegate void GrabEndDelegate();
    public delegate void GrabHookDelegate(IntPtr bufferAddress);

    internal class MilControl
    {
        private readonly FormMain parent;
        private GCHandle hUserData;
        private IntPtr frameData = IntPtr.Zero;

        private MIL_ID MilApplication = MIL.M_NULL;
        private MIL_ID MilSystem = MIL.M_NULL;
        private MIL_ID MilDigitizer = MIL.M_NULL;
        private MIL_ID dstBufferDisp = MIL.M_NULL;
        private MIL_ID parentProcBuffer = MIL.M_NULL;

        private readonly List<MIL_ID> displayList = new List<MIL_ID>();
        private readonly MIL_ID[] displayChild = new MIL_ID[10];
        private readonly MIL_ID[] dstImageList = new MIL_ID[10];
        private MIL_ID[] childProcBufferList;

        private bool triggerReady = false;
        private bool bLive = false;
        private bool triggerTerminate = false;

        private int bitDepth;
        private int childProcBufferListSize = 0;
        private int frameSizeX;
        private int frameSizeY;
        private int processFrameCount = 0;

        private Task taskTrigger;
        private Task taskCheckPRNUStatus;
        private CancellationTokenSource cts;       
        
        private readonly MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr;
        private readonly MIL_APP_HOOK_FUNCTION_PTR ErrorHook;
        //private readonly MIL_DIG_HOOK_FUNCTION_PTR IOChangePtr;

        public event GrabStartDelegate GrabStartEvent;
        public event TriggerDelegate TriggerEvent;
        public event PRNUDelegate PRNUEvent;
        public event GrabEndDelegate GrabEndEvent;
        public event GrabHookDelegate GrabHookEvent;

        public string DcfFileName { get; private set; }
        public string BoardName { get; private set; }
        public int ImageSizeX { get; private set; }
        public int ImageSizeY { get; private set; }
        public bool UseFeature { get; internal set; }
        public bool UseExternalSignal { get; internal set; }
        public int GrabDelay { get; internal set; }
        public int ChildBufCount { get; private set; }
        public bool Simulation { get; internal set; }
        public long FFCTimeout_ms { get; internal set; }
        public int FrameSizeX { get => frameSizeX; set => frameSizeX = value; }
        public int FrameSizeY { get => frameSizeY; set => frameSizeY = value; }

        public MilControl(FormMain parent)
        {
            this.parent = parent;

            ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(CallbackOnFrameProcess);
            ErrorHook = new MIL_APP_HOOK_FUNCTION_PTR(CallbackErrorMsg);
            //IOChangePtr = new MIL_DIG_HOOK_FUNCTION_PTR(CallBackIOChange);
        }

        private MIL_INT CallbackErrorMsg(MIL_INT HookType, MIL_ID HookId, IntPtr HookDataPtr)
        {
            string msg = PrintMilErrorMessage(MilApplication);
            parent.Logging(msg);

            return 0;
        }

        private string PrintMilErrorMessage(MIL_ID MilApplication)
        {
            MIL_INT MilErrorCode;

            string msg = "";
            StringBuilder MilErrorMsg = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            MIL_INT[] MilErrorSubCode = new MIL_INT[3];
            StringBuilder[] MilErrorSubMsg = new StringBuilder[3];

            // Initialize MilErrorSubMsg array.
            for (int i = 0; i < 3; i++)
            {
                MilErrorSubMsg[i] = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            }

            MilErrorCode = MIL.MappGetError(MilApplication, MIL.M_CURRENT + MIL.M_MESSAGE, MilErrorMsg);
            if (MilErrorCode != MIL.M_NULL_ERROR)
            {
                /* Collects Mil error messages and sub-messages */
                MIL_INT subCount = 3;
                MIL.MappGetError(MilApplication, MIL.M_CURRENT_SUB_NB, ref subCount);
                MilErrorSubCode[0] = MIL.MappGetError(MilApplication,
                                                  MIL.M_CURRENT_SUB_1 + MIL.M_MESSAGE,
                                                  MilErrorSubMsg[0]);
                MilErrorSubCode[1] = MIL.MappGetError(MilApplication,
                                                  MIL.M_CURRENT_SUB_2 + MIL.M_MESSAGE,
                                                  MilErrorSubMsg[1]);
                MilErrorSubCode[2] = MIL.MappGetError(MilApplication,
                                                  MIL.M_CURRENT_SUB_3 + MIL.M_MESSAGE,
                                                  MilErrorSubMsg[2]);

                //Console.WriteLine("\nMseqProcess generated a warning or an error:");
                //Console.WriteLine("  {0}", MilErrorMsg.ToString());

                msg = MilErrorMsg.ToString();
                for (int i = 0; i < subCount; i++)
                {
                    if (MilErrorSubCode[i] != 0)
                    {
                        msg += MilErrorSubMsg[i].ToString();
                    }
                    //Console.WriteLine("  {0}", MilErrorSubMsg[i]);
                }
            }

            return msg;
        }

        private MIL_INT CallbackOnFrameProcess(MIL_INT HookType, MIL_ID HookId, IntPtr HookDataPtr)
        {
            MIL_ID ModifiedBufferId = MIL.M_NULL;

            if (IntPtr.Zero.Equals(HookDataPtr))
            {
                parent.Logging("Invaild callback : CallbackOnFrameProcess()");
            }

            processFrameCount++;

            if (processFrameCount >= childProcBufferListSize)
            {
                parent.Logging($"On Frame Processing [FrameCount : {processFrameCount}]");

                processFrameCount = 0;

                MIL.MbufCopy(parentProcBuffer, dstBufferDisp);

                if (!bLive)
                {
                    GrabStop();
                    GrabEndEvent();
                }
                else
                {
                    // Retrieve the MIL_ID of the grabbed buffer.
                    MIL.MdigGetHookInfo(HookId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref ModifiedBufferId);
                    frameData = MIL.MbufInquire(ModifiedBufferId, MIL.M_HOST_ADDRESS, MIL.M_NULL);
                    GrabHookEvent(frameData);
                }
            }


            return 0;
        }

        //private MIL_INT CallBackIOChange(MIL_INT HookType, MIL_ID HookId, IntPtr HookDataPtr)
        //{
        //    if (IntPtr.Zero.Equals(HookDataPtr))
        //    {
        //        parent.Logging("Invaild callback : CallBackIOChange()");
        //    }

        //    //if (!IntPtr.Zero.Equals(HookDataPtr))
        //    {
        //        if (triggerReady)
        //        {
        //            triggerReady = false; //여러번을 타서 예외처리

        //            GrabStart();

        //            parent.Logging("Trigger on");

        //            GrabStartEvent();
        //        }
        //        else
        //        {
        //            parent.Logging("Trigger on when not ready state");
        //        }
        //    }
        //    //else
        //    //{
        //    //    parent.Logging("Invaild callback : CallBackIOChange()");
        //    //}

        //    return 0;
        //}

        internal void StartPRNU()
        {
            taskCheckPRNUStatus = new Task(new Action(PRNUMonitor));
            taskCheckPRNUStatus.Start();
        }

        private void PRNUMonitor()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();

            while (true)
            {
                Thread.Sleep(50);

                StringBuilder stringBuilder = new StringBuilder();
                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, "flatfieldCalibrationPRNUStatus", MIL.M_TYPE_STRING, stringBuilder);

                PRNUEvent(stringBuilder.ToString());

                if (stringBuilder.ToString().Equals("Good"))
                {
                    break;
                }
                else
                {
                    if (stopwatch.ElapsedMilliseconds > FFCTimeout_ms)
                    {
                        PRNUEvent("Timeout!! \nRetry please.");
                        stopwatch.Stop();
                        break;
                    }
                }
            }
        }

        private void TriggerMonitor()
        {
            MIL_INT IOStatus = 0;

            while (true)
            {
                Thread.Sleep(5);

                if (cts.IsCancellationRequested)
                {
                    break;
                }

                if (triggerTerminate)
                {
                    break;
                }

                MIL.MdigInquire(MilDigitizer, MIL.M_AUX_IO6 + MIL.M_IO_STATUS, ref IOStatus);

                //IOStatus = MIL.M_ON; //Test Code

                if (IOStatus == MIL.M_INVALID) // Specifies that the input signal is disabled.
                {
                }
                else if (IOStatus == MIL.M_OFF) // Specifies that the input signal is off.
                {
                    TriggerEvent?.Invoke(false);
                }
                else if (IOStatus == MIL.M_ON) // Specifies that the input signal is on.
                {
                    TriggerEvent?.Invoke(true);

                    //새로 추가 : IOChangeEvent 는 문제가 있음
                    if (triggerReady)
                    {
                        triggerReady = false; //여러번을 타서 예외처리

                        parent.Logging("Trigger on");

                        GrabStart();

                        GrabStartEvent?.Invoke();
                    }
                }
                else if (IOStatus == MIL.M_UNKNOWN) // Specifies that the input signal cannot be inquired with its current configuration.
                {
                }
            }
        }

        internal void SetTriggerReady()
        {
            if (UseExternalSignal)
            {
                triggerReady = true;
            }
            else //내부 처리
            {
                Task.Run(new Action(() =>
                {
                    GrabStart();
                    GrabStartEvent?.Invoke();
                }));
            }
        }

        public void Initialize(string dcfFileName, string grabberBoardName)
        {
            DcfFileName = dcfFileName;
            BoardName = grabberBoardName;

            // get a handle to the HookDataStruct object in the managed heap, we will use this 
            // handle to get the object back in the callback function
            hUserData = GCHandle.Alloc(parent);

            if (!Simulation)
            {
                //Application Module 할당
                MIL.MappAlloc(MIL.M_DEFAULT, ref MilApplication);
                MIL.MappHookFunction(MilApplication, MIL.M_ERROR_CURRENT, ErrorHook, GCHandle.ToIntPtr(GCHandle.Alloc(hUserData)));

                //System Module 할당
                MIL.MsysAlloc("M_SYSTEM_" + BoardName, MIL.M_DEV0, MIL.M_DEFAULT, ref MilSystem);

                //Digitizer Module 할당
                MIL.MdigAlloc(MilSystem, MIL.M_DEV0, DcfFileName, MIL.M_DEFAULT, ref MilDigitizer);
                parent.Logging($"Digitizer Set [{DcfFileName}]");

                // Enable GenICam
                if (UseFeature)
                {
                    MIL.MdigControl(MilDigitizer, MIL.M_GC_CLPROTOCOL_DEVICE_ID, "M_DEFAULT");
                    MIL.MdigControl(MilDigitizer, MIL.M_GC_CLPROTOCOL, MIL.M_ENABLE);
                    //MIL.MdigControl(MilDigitizer, MIL.M_GC_FEATURE_BROWSER, MIL.M_OPEN + MIL.M_ASYNCHRONOUS);
                }

                frameSizeX = (int)MIL.MdigInquire(MilDigitizer, MIL.M_SIZE_X, MIL.M_NULL);   //할당된 Digitizer로 부터 가로 해상도 받아오기 (M_SIZE_X는 코드상으로 변경 불가)
                frameSizeY = (int)MIL.MdigInquire(MilDigitizer, MIL.M_SIZE_Y, MIL.M_NULL);   //할당된 Digitizer로 부터 세로 해상도 받아오기
                bitDepth = (int)MIL.MdigInquire(MilDigitizer, MIL.M_SIZE_BIT, MIL.M_NULL);

                //Digitizer 설정
                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE);
                MIL.MdigControl(MilDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);

                //IO Trigger 세팅
                MIL.MdigControl(MilDigitizer, MIL.M_IO_INTERRUPT_ACTIVATION + MIL.M_AUX_IO6, MIL.M_EDGE_RISING);
                MIL.MdigControl(MilDigitizer, MIL.M_IO_INTERRUPT_STATE + MIL.M_AUX_IO6, MIL.M_ENABLE);
                //MIL.MdigHookFunction(MilDigitizer, MIL.M_IO_CHANGE, IOChangePtr, GCHandle.ToIntPtr(GCHandle.Alloc(hUserData)));

                if (UseExternalSignal)
                {
                    triggerTerminate = false;
                    taskTrigger = new Task(new Action(TriggerMonitor));
                    taskTrigger.Start();
                    cts = new CancellationTokenSource();
                }
            }
            else
            {
                parent.Logging("Grab control is simulation");

            }

            parent.Logging("Grab Manager initialization complete");
        }

        public void AllocateDisplay(Panel display)
        {
            if (display != null && MilSystem != null)
            {
                MIL_ID MilDisplay = MIL.M_NULL;
                //Display Module 할당
                MIL.MdispAlloc(MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref MilDisplay);
                MIL.MdispSelectWindow(MilDisplay, dstBufferDisp, display.Handle);   //디스플레이와 버퍼를 연결하여 Picture Control에 출력
                MIL.MdispControl(MilDisplay, MIL.M_FILL_DISPLAY, MIL.M_ENABLE); //디스플레이 화면에 맞춤

                displayList.Add(MilDisplay);
            }
        }

        public bool SetGrabFrameBuffer(int parentWidth, int parentHeight)
        {
            try
            {
                if (!Simulation)
                {
                    GrabStop();
                    GrabBufferFree();

                    if (parentWidth % frameSizeX != 0)
                    {
                        parentWidth = (parentWidth / frameSizeX + 1) * frameSizeX;
                    }

                    if (parentHeight % frameSizeY != 0)
                    {
                        parentHeight = (parentHeight / frameSizeY + 1) * frameSizeY;
                    }

                    int childProcBufCountY = parentHeight / frameSizeY;

                    ImageSizeX = parentWidth;
                    ImageSizeY = parentHeight;

                    //최종 사용 할 이미지 버퍼 할당 - 최종 ROI 작업 하는 변수 
                    MIL.MbufAlloc2d(MilSystem, ImageSizeX, ImageSizeY, bitDepth + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_DISP, ref dstBufferDisp);
                    MIL.MbufClear(dstBufferDisp, 0);   //버퍼 초기화

                    //수집용 자식 버퍼리스트의 부모 버퍼 할당
                    MIL.MbufAlloc2d(MilSystem, ImageSizeX, ImageSizeY, bitDepth + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_PROC, ref parentProcBuffer);
                    MIL.MbufClear(parentProcBuffer, 0);   //버퍼 초기화

                    childProcBufferList = new MIL_ID[childProcBufCountY];
                    for (childProcBufferListSize = 0; childProcBufferListSize < childProcBufCountY; childProcBufferListSize++)
                    {
                        //자식 버퍼리스트 할당
                        //MIL.MbufAlloc2d(MilSystem, SizeX, SizeY, BitDepth + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB, ref MilGrabBufferList[MilGrabBufferListSize]);
                        MIL.MbufChild2d(parentProcBuffer, 0, frameSizeY * childProcBufferListSize, frameSizeX, frameSizeY, ref childProcBufferList[childProcBufferListSize]);

                        if (childProcBufferList[childProcBufferListSize] != MIL.M_NULL)
                        {
                            MIL.MbufClear(childProcBufferList[childProcBufferListSize], 0xFF);

                            //Pitch = (int)MIL.MbufInquire(MilGrabBufferList[MilGrabBufferListSize], MIL.M_PITCH, MIL.M_NULL);   //할당된 Digitizer로 부터 Pitch 받아오기
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AddChildBufId(int index, int offX, int offY, int sizeX, int sizeY, Panel panel = null)
        {
            try
            {
                if (!Simulation)
                {
                    if (offX + sizeX >= 0 && offX + sizeX <= ImageSizeX
                        &&
                        offY + sizeY >= 0 && offY + sizeY <= ImageSizeY)
                    {
                        offX = (ImageSizeX - sizeX) / 2; //가운데 중심으로 사이즈 조절

                        // Disp Buffer Child 할당 - 3000 Line Buffer 생성
                        MIL.MbufChild2d(dstBufferDisp, offX, offY, sizeX, sizeY, ref dstImageList[index]);
                        // Disp Buffer Child 초기화
                        MIL.MbufClear(dstImageList[index], 0);

                        if (panel != null)
                        {
                            //Display Module 할당
                            MIL.MdispAlloc(MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref displayChild[index]);
                            MIL.MdispSelectWindow(displayChild[index], dstImageList[index], panel.Handle);   //디스플레이와 버퍼를 연결하여 Picture Control에 출력
                            MIL.MdispControl(displayChild[index], MIL.M_FILL_DISPLAY, MIL.M_ENABLE); //디스플레이 화면에 맞춤
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                ChildBufCount++;
                return true;
            }
            catch
            {
                parent.Logging("Add Child Buffer Fail!!");
                return false;
            }

        }

        public void SingleGrab()
        {
            if (!Simulation)
            {
                GrabStop();

                if (!bLive)
                {
                    Delay(GrabDelay);
                }

                if (childProcBufferListSize != 0)
                {
                    // Start the processing. The processing function is called with every frame grabbed.
                    MIL.MdigProcess(MilDigitizer, childProcBufferList, childProcBufferListSize, MIL.M_SEQUENCE + MIL.M_COUNT(childProcBufferListSize), MIL.M_ASYNCHRONOUS, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));
                    parent.Logging($"Single Grab Count : {childProcBufferListSize}");
                }
            }
        }

        public void GrabStart()
        {
            if (!Simulation)
            {
                GrabStop();

                if (!bLive)
                {
                    Delay(GrabDelay);
                }

                if (childProcBufferListSize != 0)
                {
                    MIL.MdigProcess(MilDigitizer, childProcBufferList, childProcBufferListSize, MIL.M_START, MIL.M_ASYNCHRONOUS, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));
                }
            }
        }

        public void GrabStop()
        {
            if (!Simulation)
            {
                if (childProcBufferListSize != 0)
                {
                    // Stop the processing.
                    MIL.MdigProcess(MilDigitizer, childProcBufferList, childProcBufferListSize, MIL.M_STOP, MIL.M_DEFAULT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));
                }
            }

        }

        internal void UseErrorMsg(bool value)
        {
            if (!Simulation)
            {
                if (value)
                {
                    MIL.MappControl(MIL.M_ERROR, MIL.M_PRINT_ENABLE);
                }
                else
                {
                    MIL.MappControl(MIL.M_ERROR, MIL.M_PRINT_DISABLE);
                }
            }

        }

        /// <summary>
        /// if value is null, action execute.
        /// </summary>
        /// <param name="featureName"></param>
        /// <param name="value"></param>
        internal void SetFeature(Feature.Name featureName, object value = null)
        {
            if (!Simulation)
            {
                switch (value)
                {
                    case int iValue:
                        MIL.MdigControlFeature(MilDigitizer, MIL.M_FEATURE_VALUE, featureName.ToString(), MIL.M_TYPE_INT64, ref iValue);
                        break;
                    case double dValue:
                        MIL.MdigControlFeature(MilDigitizer, MIL.M_FEATURE_VALUE, featureName.ToString(), MIL.M_TYPE_DOUBLE, ref dValue);
                        break;
                    case string sValue:
                        MIL.MdigControlFeature(MilDigitizer, MIL.M_FEATURE_VALUE, featureName.ToString(), MIL.M_TYPE_STRING, sValue);
                        break;
                    case null: //execute
                        MIL.MdigControlFeature(MilDigitizer, MIL.M_FEATURE_EXECUTE, featureName.ToString(), MIL.M_DEFAULT, MIL.M_NULL);
                        break;
                    default:
                        break;
                }
            }

        }

        public void GetFeature(ref Feature feature)
        {
            if (Simulation == false)
            {
                double dResult = default;
                int iResult = default;
                StringBuilder stringBuilder = new StringBuilder();

                //value
                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.ExposureTime.ToString(), MIL.M_TYPE_DOUBLE, ref dResult);
                feature.ExposureTime = dResult;

                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.Gain.ToString(), MIL.M_TYPE_DOUBLE, ref dResult);
                feature.Gain = dResult;

                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.AcquisitionLineRate.ToString(), MIL.M_TYPE_INT64, ref iResult);
                feature.LineRate = iResult;

                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.ReverseX.ToString(), MIL.M_TYPE_STRING, stringBuilder);
                feature.ReverseX = stringBuilder.ToString().Equals("On");

                //ffc
                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.flatfieldCorrectionMode.ToString(), MIL.M_TYPE_STRING, stringBuilder);
                feature.FFC_ModeOn = stringBuilder.ToString().Equals("On");

                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.flatfieldCorrectionAlgorithm.ToString(), MIL.M_TYPE_STRING, stringBuilder);
                feature.FFC_Algorithm = stringBuilder.ToString();

                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.flatfieldCalibrationTarget.ToString(), MIL.M_TYPE_INT64, ref iResult);
                feature.FFC_Target = iResult;

                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.flatfieldCalibrationROIOffsetX.ToString(), MIL.M_TYPE_INT64, ref iResult);
                feature.FFC_RoiOffsetX = iResult;

                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.flatfieldCalibrationROIWidth.ToString(), MIL.M_TYPE_INT64, ref iResult);
                feature.FFC_RoiWidth = iResult;

                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.flatfieldCalibrationPRNUStatus.ToString(), MIL.M_TYPE_STRING, stringBuilder);
                feature.FFC_PRNUStatus = stringBuilder.ToString();

                MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, Feature.Name.UserSetDefaultSelector.ToString(), MIL.M_TYPE_STRING, stringBuilder);
                feature.UserSetDefault = stringBuilder.ToString();
            }

        }

        public void GrabBufferFree()
        {
            if (!Simulation)
            {
                if (MilDigitizer != 0)
                {
                    // Free the grab buffers.
                    ChildBufFree();

                    // Free the grab buffers.
                    while (childProcBufferListSize > 0)
                    {
                        MIL.MbufFree(childProcBufferList[--childProcBufferListSize]);
                    }

                    foreach (MIL_ID dip in displayList)
                    {
                        MIL.MdispFree(dip);
                    }
                    displayList.Clear();


                    if (dstBufferDisp != 0)
                    {
                        MIL.MbufFree(dstBufferDisp);
                        dstBufferDisp = 0;
                    }

                    if (parentProcBuffer != 0)
                    {
                        MIL.MbufFree(parentProcBuffer);
                        parentProcBuffer = 0;
                    }
                }
            }

        }
        public void Free()
        {
            if (!Simulation)
            {
                GrabBufferFree();

                if (UseExternalSignal)
                {
                    triggerTerminate = true;
                    cts.Cancel();
                }

                if (MilDigitizer != 0)
                {
                    MIL.MdigFree(MilDigitizer);  //Digitizer 해제
                    MilDigitizer = 0;
                }

                MIL.MsysFree(MilSystem);  //System 해제
                MIL.MappFree(MilApplication); //Application 해제
            }

        }

        public void ChildBufFree()
        {
            if (Simulation == false)
            {
                //Disp Buffer Child 해제
                while (ChildBufCount > 0)
                {
                    if (dstImageList[ChildBufCount - 1] != null)
                    {
                        MIL.MbufFree(dstImageList[ChildBufCount - 1]);
                    }

                    if (displayChild[ChildBufCount - 1] != null)
                    {
                        MIL.MdispFree(displayChild[ChildBufCount - 1]);
                    }

                    ChildBufCount--;
                }
            }


        }

        public void Save(string filename)
        {
            if (MilDigitizer != 0)
            {
                //GrabStop();

                if (false == IsFileLocked(new FileInfo(filename)))
                {
                    MIL.MbufExport(filename, MIL.M_BMP, dstBufferDisp);
                }
            }
        }

        public bool ChildSave(string filename, int index)
        {
            if (MilDigitizer != 0)
            {
                if (ChildBufCount <= index)
                {
                    return false;
                }

                //GrabStop();

                if (false == IsFileLocked(new FileInfo(filename)))
                {
                    MIL.MbufExport(filename, MIL.M_BMP, dstImageList[index]);
                }

                return true;
            }
            else
            {
                return true;
            }
        }

        internal void Live(bool bLive)
        {
            this.bLive = bLive;

            if (bLive)
            {
                GrabStart();
            }
            else
            {
                GrabStop();
            }
        }

        private void Delay(double milliSecond)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while (stopWatch.Elapsed.TotalMilliseconds < milliSecond)
            {
                ;
            }
        }

        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                if (!file.Exists)
                {
                    return false;
                }

                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException ex)
            {
                parent.Logging(ex.Message);

                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)                
                return true;
            }

            //file is not locked
            return false;
        }
    }
}
