using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WebDown
{
    public partial class Form1 : Form
    {
        private bool isRun = false;
        private string[] strList = null;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isRun)
            {
                backgroundWorker1.CancelAsync();

                button1.Text = "下载";
                isRun = false;
                return;
            }

            if (textBox1.Text == "")
            {
                textBox1.Focus();
                return;
            }

            strList = textBox1.Text.Split(new char[] { '\n' });

            backgroundWorker1.RunWorkerAsync();
            button1.Text = "暂停";
            isRun = true;
        }

        ///<summary>
        /// 下载文件
        /// </summary>
        /// <param name="URL">下载文件地址</param>
        /// <param name="Filename">下载后另存为（全路径）</param>
        private bool DownloadFile(string URL, string filename)
        {
            try
            {
                HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                Stream st = myrp.GetResponseStream();
                Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
                myrp.Close();
                Myrq.Abort();

                return true;
            }
            catch (System.Exception e)
            {
                return false;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int successCount = 0;
            int errorCount = 0;
            for (int i = 0; i < strList.Length; i++)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                string str = strList[i];
                str = str.Trim();
                // curl "https://www.wjceo.com/examples/demo/infinitown/assets/scenes/data/main.bin" -H 
                str = str.Substring(6, str.IndexOf('"', 6) - 6);

                string s = str.Replace("http://", "");
                s = s.Replace("https://", "");
                string[] arr = s.Split(new char[] { '/' });
                string path = "";
                string fileName = "";
                for (int j = 0; j < arr.Length; j++)
                {
                    if (j == arr.Length - 1)
                    {
                        fileName = arr[j];
                        break;
                    }
                    path += arr[j] + "\\";
                }

                fileName = fileName.Split(new char[] { '?' })[0];
                if (fileName == "")
                {
                    fileName = "index.html";
                }

                path = Application.StartupPath + "\\data\\" + path;
                Directory.CreateDirectory(path);
                fileName = path + fileName;

                bool bo = DownloadFile(str, fileName);
                if (bo)
                {
                    successCount++;
                }
                else
                {
                    errorCount++;
                }

                label2.Text = successCount + "/" + errorCount + "/" + strList.Length;

                backgroundWorker1.ReportProgress((int)Math.Round(1.0 * i / strList.Length * 100));
                if (numericUpDown1.Value > 0)
                {
                    Thread.Sleep((int)numericUpDown1.Value * 1000);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 100;
            button1.Text = "下载";
            isRun = false;
        }
    }
}
