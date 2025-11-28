using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public class Logger
    {
        private ConcurrentQueue<string> queue;
        private Task task;
        private string path;
        private DirectoryInfo logPath;
        private bool isExit;
        private DateTime day;
        private int deleteDay = 7;
        private readonly RichTextBox logMessageDisplay;
        private string filesExtension = "log";
        private bool dateDisplay = true;
        private readonly DeleteFiles deleteFiles = new DeleteFiles();
        private int maxDisplayLine = 100;

        public int DeleteDay { get => deleteDay; set => deleteDay = value; }
        public string FilesExtension { get => filesExtension; set => filesExtension = value; }
        public bool DateDisplay { get => dateDisplay; set => dateDisplay = value; }
        public int MaxDisplayLine { get => maxDisplayLine; set => maxDisplayLine = value; }

        public Logger()
        {
            //logMessageDisplay = new RichTextBox();
        }

        public Logger(RichTextBox logMessage)
        {
            logMessageDisplay = logMessage;
        }

        public void Logging(string msg)
        {
            if (logMessageDisplay != null)
            {
                if(logMessageDisplay.InvokeRequired)
                {
                    logMessageDisplay.Invoke(new Action(() =>
                    {
                        if (logMessageDisplay.Lines.Length > maxDisplayLine)
                        {
                            logMessageDisplay.Select(0, logMessageDisplay.GetFirstCharIndexFromLine(logMessageDisplay.Lines.Length - maxDisplayLine));
                            logMessageDisplay.SelectedText = "";
                        }

                        logMessageDisplay.AppendText(msg + '\n');
                        logMessageDisplay.ScrollToCaret();
                    }));
                }
                else
                {
                    if (logMessageDisplay.Lines.Length > maxDisplayLine)
                    {
                        logMessageDisplay.Select(0, logMessageDisplay.GetFirstCharIndexFromLine(logMessageDisplay.Lines.Length - maxDisplayLine));
                        logMessageDisplay.SelectedText = "";
                    }

                    logMessageDisplay.AppendText(msg + '\n');
                    logMessageDisplay.ScrollToCaret();
                }
            }

            Enqueue(msg);
        }

        public void Open(string logPath)
        {
            this.logPath = new DirectoryInfo(logPath);
            if (!this.logPath.Exists) { this.logPath.Create(); }

            queue = new ConcurrentQueue<string>();
            task = new Task(new Action(MainLogger));

            day = DateTime.Today;

            path = $@"{this.logPath.FullName}\{DateTime.Now:MM-dd}.{filesExtension}";

            deleteFiles.Start(this.logPath.FullName, deleteDay);
        }

        public void Close()
        {
            isExit = true;


            deleteFiles.Close();
        }

        public int GetQueueCount()
        {
            lock (queue)
            {
                return queue.Count;
            }
        }

        private async Task WriteStream(string path, string text)
        {
            using (StreamWriter writer = new StreamWriter(File.Open(path, FileMode.Append)))
            {
                await writer.WriteAsync(text);
            }
        }

        private async void MainLogger()
        {
            try
            {
                while (true)
                {
                    if (!queue.IsEmpty)
                    {
                        if (!queue.TryDequeue(out string x))
                        {
                            continue;
                        }

                        Thread.Sleep(1);

                        if (isExit)
                        {
                            break;  //종료
                        }

                        await WriteStream(path, x + "\n");
                    }
                    else
                    {
                        lock (queue)
                        {
                            if (queue.Count == 0) // Queue 에 어떠한것도 담기지 않을때만 접근 가능
                            {
                                if (DateTime.Today > day)
                                {
                                    day = DateTime.Today;
                                    path = $@"{logPath.FullName}\{DateTime.Now:MM-dd}.{filesExtension}";
                                }

                                break;  //종료
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (IOException E)
            {
                Thread.Sleep(10);
                Enqueue(E.Message);
            }

        }

        private void Enqueue(string obj)
        {
            queue.Enqueue(obj);
            //쓰레드 스타트 
            if (task.Status != TaskStatus.Running)
            {
                task = new Task(new Action(MainLogger));
                task.Start();
            }
        }
    }
}
