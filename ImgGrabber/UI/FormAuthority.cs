using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ImgGrabber
{
    public partial class FormAuthority : Form
    {
        private readonly List<Tuple<Level, string>> listPassword = new List<Tuple<Level, string>>();

        private Level level = Level.Operator;
        private Level setLevel = Level.Operator;
        private string passwordOperator = "";
        private string passwordMaintenance = "sdv";
        private string passwordEngineer = "keoc";

        public delegate void ChangeAuthority(Level level);
        public event ChangeAuthority ChangeAuthorityEvent;

        public enum Level
        {
            Operator,
            Maintenance,
            Engineer
        }

        public FormAuthority()
        {
            InitializeComponent();
        }

        public Level GetLevel()
        {
            return setLevel;
        }

        public void SetLevel(Level level)
        {
            this.level = level;
            setLevel = level;
        }

        private void FormAuthority_Load(object sender, EventArgs e)
        {
            SelectAuthority(setLevel);
        }

        public void Initialize(string path)
        {
            DirectoryInfo IniInfo = new DirectoryInfo(path);
            if (!IniInfo.Exists) { IniInfo.Create(); }

            string iniPath = path + @"\Password.ini";
            if (!File.Exists(iniPath))
            {
                using (FileStream fs = new FileStream(iniPath, FileMode.Create))
                {
                    fs.Close();
                }
            }

            IniFile iniFile = new IniFile();
            iniFile.Load(iniPath);

            listPassword.Clear();

            passwordOperator = iniFile.GetValue("Password", "Operator", passwordOperator);
            passwordMaintenance = iniFile.GetValue("Password", "Maintenance", passwordMaintenance);
            passwordEngineer = iniFile.GetValue("Password", "Engineer", passwordEngineer);

            listPassword.Add(new Tuple<Level, string>(Level.Operator, passwordOperator));
            listPassword.Add(new Tuple<Level, string>(Level.Maintenance, passwordMaintenance));
            listPassword.Add(new Tuple<Level, string>(Level.Engineer, passwordEngineer));
        }

        private void labelOperator_Click(object sender, EventArgs e)
        {
            level = Level.Operator;
            SelectAuthority(level);
            textBoxPassword.Focus();
        }

        private void labelMaintenance_Click(object sender, EventArgs e)
        {
            level = Level.Maintenance;
            SelectAuthority(level);
            textBoxPassword.Focus();
        }

        private void labelEngineer_Click(object sender, EventArgs e)
        {
            level = Level.Engineer;
            SelectAuthority(level);
            textBoxPassword.Focus();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string password = textBoxPassword.Text;
            if (CheckPassword(level, password))
            {
                setLevel = level;
                ChangeAuthorityEvent(setLevel);

                Close();
            }
            else
            {
                labelWrongPassword.Visible = true;
            }

            textBoxPassword.Text = "";
        }

        private bool CheckPassword(Level level, string password)
        {
            string comparePassword = "";

            foreach (Tuple<Level, string> pw in listPassword.Where(pw => pw.Item1 == level))
            {
                comparePassword = pw.Item2;
            }

            return password.Equals(comparePassword);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            textBoxPassword.Text = "";

            Close();
        }

        private void textBoxPassword_Click(object sender, EventArgs e)
        {
            labelWrongPassword.Visible = false;
        }

        private void SelectAuthority(Level level)
        {
            switch (level)
            {
                case Level.Operator:
                    labelOperator.BorderStyle = BorderStyle.Fixed3D;
                    labelMaintenance.BorderStyle = BorderStyle.None;
                    labelEngineer.BorderStyle = BorderStyle.None;
                    break;
                case Level.Maintenance:
                    labelOperator.BorderStyle = BorderStyle.None;
                    labelMaintenance.BorderStyle = BorderStyle.Fixed3D;
                    labelEngineer.BorderStyle = BorderStyle.None;
                    break;
                case Level.Engineer:
                    labelOperator.BorderStyle = BorderStyle.None;
                    labelMaintenance.BorderStyle = BorderStyle.None;
                    labelEngineer.BorderStyle = BorderStyle.Fixed3D;
                    break;
            }

        }

        private void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(sender, e);
            }
        }
    }
}
