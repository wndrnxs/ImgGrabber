using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public class DeleteFiles
    {
        private Task task;
        private string path;
        private double day;
        private bool terminate;

        public void Start(string path, double day)
        {
            this.path = path;
            this.day = day;

            task = new Task(new Action(MainTask), TaskCreationOptions.LongRunning);
            task.Start();
        }

        public void Close()
        {
            terminate = true;
        }

        private void MainTask()
        {
            while (true)
            {
                if (terminate)
                {
                    break;
                }

                Thread.Sleep(1000);

                DeleteOldFile(path, day);
            }
        }



        private void DeleteOldFile(string dirPath, double Day)
        {
            try
            {
                DateTime day = DateTime.Now.AddDays(-Day);
                //string day = DateTime.Now.AddDays(-Day).ToString("yyyyMMdd");

                DirectoryInfo directory = new DirectoryInfo(dirPath);
                //DateTime cmpTime = DateTime.ParseExact(day, "yyyyMMdd", null);

                if (directory.Exists)
                {
                    foreach (FileInfo file in directory.GetFiles())
                    {
                        Thread.Sleep(50);
                        if (DateTime.Compare(file.LastWriteTime, day) < 0)
                        {
                            if (!IsFileLocked(file))
                            {
                                File.Delete(file.FullName);
                            }
                        }
                    }

                    DirectoryInfo[] dis = directory.GetDirectories();

                    if (dis.Length > 0) //폴더가 있을 경우
                    {
                        foreach (DirectoryInfo directoryInfo in dis)
                        {
                            Thread.Sleep(50);
                            directoryInfo.Attributes = FileAttributes.Normal;

                            FileInfo[] ds = directoryInfo.GetFiles();

                            foreach (FileInfo file in ds)
                            {
                                Thread.Sleep(50);
                                if (DateTime.Compare(file.LastWriteTime, day) < 0)
                                {
                                    if (!IsFileLocked(file))
                                    {
                                        File.Delete(file.FullName);
                                    }
                                }
                            }

                            if (ds.Length == 0)
                            {
                                directoryInfo.Delete();
                            }
                        }
                    }
                }
            }
            catch
            {

            }

        }

        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
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
