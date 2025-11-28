using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImgGrabber
{
    public partial class FormGrabberUnitTest : Form
    {
        private const int BUFFERING_SIZE_MAX = 8;

        MIL_ID MilApplication = MIL.M_NULL;
        MIL_ID MilSystem = MIL.M_NULL;
        MIL_ID MilDigitizer = MIL.M_NULL;
        MIL_ID MilDisplay_Mono = MIL.M_NULL;
        MIL_ID MilDisplay_Color = MIL.M_NULL;

        MIL_ID[] MilGrabBufferList = new MIL_ID[BUFFERING_SIZE_MAX];
        MIL_ID MilImageDisp_Mono = MIL.M_NULL;
        MIL_ID MilImageDisp_Color = MIL.M_NULL;

        IntPtr Data_Mono = IntPtr.Zero;
        IntPtr Data_Color = IntPtr.Zero;

        MIL_INT ProcessFrameCount = 0;

        int SizeX;
        int SizeY;
        int Pitch;

        int MilGrabBufferListSize = 0;



        GCHandle hUserData;
        MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr;

        public FormGrabberUnitTest()
        {
            InitializeComponent();
        }

        private void button_Init_Click(object sender, EventArgs e)
        {
            MIL.MappAlloc(MIL.M_DEFAULT, ref MilApplication);   //Application Module 할당
            //MIL.MsysAlloc("M_SYSTEM_SOLIOS", MIL.M_DEV0, MIL.M_DEFAULT, ref MilSystem);   //System Module 할당
            MIL.MsysAlloc("M_SYSTEM_RADIENTEVCL", MIL.M_DEV0, MIL.M_DEFAULT, ref MilSystem);   //System Module 할당
            MIL.MdigAlloc(MilSystem, MIL.M_DEV0, "Linea_4K.dcf", MIL.M_DEFAULT, ref MilDigitizer);   //Digitizer Module 할당

            SizeX = (int)MIL.MdigInquire(MilDigitizer, MIL.M_SIZE_X, MIL.M_NULL);   //할당된 Digitizer로 부터 가로 해상도 받아오기
            SizeY = (int)MIL.MdigInquire(MilDigitizer, MIL.M_SIZE_Y, MIL.M_NULL);   //할당된 Digitizer로 부터 세로 해상도 받아오기


            for (MilGrabBufferListSize = 0; MilGrabBufferListSize < BUFFERING_SIZE_MAX; MilGrabBufferListSize++)
            {
                MIL.MbufAlloc2d(MilSystem, SizeX, SizeY, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB, ref MilGrabBufferList[MilGrabBufferListSize]);
                if (MilGrabBufferList[MilGrabBufferListSize] != MIL.M_NULL)
                {
                    MIL.MbufClear(MilGrabBufferList[MilGrabBufferListSize], 0xFF);

                    Pitch = (int)MIL.MbufInquire(MilGrabBufferList[MilGrabBufferListSize], MIL.M_PITCH, MIL.M_NULL);   //할당된 Digitizer로 부터 Pitch 받아오기
                }
                else
                {
                    break;
                }
            }

            MIL.MbufAlloc2d(MilSystem, SizeX, SizeY, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_DISP, ref MilImageDisp_Mono);   //Mono Display Buffer 할당
            MIL.MbufClear(MilImageDisp_Mono, 0);   //버퍼 초기화


            MIL.MdispAlloc(MilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref MilDisplay_Mono);   //Display Module 할당
            MIL.MdispSelectWindow(MilDisplay_Mono, MilImageDisp_Mono, panel_Disp.Handle);   //디스플레이와 버퍼를 연결하여 Picture Control에 출력
            MIL.MdispControl(MilDisplay_Mono, MIL.M_FILL_DISPLAY, MIL.M_ENABLE); //디스플레이 화면에 맞춤

            //Digitizer 설정
            MIL.MdigControl(MilDigitizer, MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE);
            MIL.MdigControl(MilDigitizer, MIL.M_GRAB_MODE, MIL.M_ASYNCHRONOUS);

            // get a handle to the HookDataStruct object in the managed heap, we will use this 
            // handle to get the object back in the callback function
            hUserData = GCHandle.Alloc(this);

            ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);
        }

        private void button_SingleGrab_Click(object sender, EventArgs e)
        {
            // Start the processing. The processing function is called with every frame grabbed.
            MIL.MdigProcess(MilDigitizer, MilGrabBufferList, MilGrabBufferListSize, MIL.M_SEQUENCE + MIL.M_COUNT(1), MIL.M_ASYNCHRONOUS, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));
        }

        private void button_Grab_Click(object sender, EventArgs e)
        {
            MIL.MdigProcess(MilDigitizer, MilGrabBufferList, MilGrabBufferListSize, MIL.M_START, MIL.M_ASYNCHRONOUS, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            // Stop the processing.
            MIL.MdigProcess(MilDigitizer, MilGrabBufferList, MilGrabBufferListSize, MIL.M_STOP, MIL.M_DEFAULT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));
        }

        private void button_Free_Click(object sender, EventArgs e)
        {
            // Free the grab buffers.
            while (MilGrabBufferListSize > 0)
            {
                MIL.MbufFree(MilGrabBufferList[--MilGrabBufferListSize]);
            }
            MIL.MbufFree(MilImageDisp_Mono);   //Display Buffer 해제
            MIL.MdispFree(MilDisplay_Mono);  //Display 해제
            MIL.MdigFree(MilDigitizer);  //Digitizer 해제
            MIL.MsysFree(MilSystem);  //System 해제
            MIL.MappFree(MilApplication); //Application 해제
        }

        private void button_Exposure_Click(object sender, EventArgs e)
        {
            double expTime = 1000;

            MIL.MdigControlFeature(MilDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref expTime);

            MIL.MdigInquireFeature(MilDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref expTime);
        }

        static MIL_INT ProcessingFunction(MIL_INT HookType, MIL_ID HookId, IntPtr HookDataPtr)
        {
            MIL_ID ModifiedBufferId = MIL.M_NULL;

            // this is how to check if the user data is null, the IntPtr class
            // contains a member, Zero, which exists solely for this purpose
            if (!IntPtr.Zero.Equals(HookDataPtr))
            {
                // get the handle to the DigHookUserData object back from the IntPtr
                GCHandle hUserData = GCHandle.FromIntPtr(HookDataPtr);

                // get a reference to the DigHookUserData object
                FormGrabberUnitTest UserData = hUserData.Target as FormGrabberUnitTest;

                // Increment the frame counter.
                UserData.ProcessFrameCount++;

                // Retrieve the MIL_ID of the grabbed buffer.
                MIL.MdigGetHookInfo(HookId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref ModifiedBufferId);

                UserData.Data_Mono = MIL.MbufInquire(ModifiedBufferId, MIL.M_HOST_ADDRESS, MIL.M_NULL);

                //unsafe
                //{
                //    byte* Data_MonoArray = (byte*)UserData.Data_Mono.ToPointer();

                //    for (int y = 0; y < UserData.SizeY; y++)
                //    {
                //        for (int x = 0; x < UserData.SizeX; x++)
                //        {

                //        }

                //    }

                //}

                // Execute the processing and update the display.
                MIL.MbufCopy(ModifiedBufferId, UserData.MilImageDisp_Mono);
            }

            return 0;
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            MIL.MbufSave("Image_Save.bmp", MilImageDisp_Mono);
        }
    }
}
