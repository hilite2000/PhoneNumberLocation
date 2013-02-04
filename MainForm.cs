using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using PhoneNumberLocation.LocationService;
using PhoneNumberLocation.DataSource;

namespace PhoneNumberLocation
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            ResetLocationSvc();
        }

        private PNLocationService mPNLocationSvc;

        private System.Collections.Generic.Queue<ProcessProgressEventArgs> mProgressNotifyCache = new Queue<ProcessProgressEventArgs>();

        private void ResetLocationSvc()
        {
            rtbOutput.Clear();
            this.mPNLocationSvc = new PNLocationService();
            this.mPNLocationSvc.ProcessProgressEvent += new ProcessProgressDelegate(mPNLocationSvc_ProcessProgressEvent);

            //this.mPNLocationSvc.ProcessProgressEvent += delegate(ProcessProgressEventArgs args) { };

            pbWorking.Value = 0;
            lblCacheHit.Text = "缓存命中/长度：0/0";

            this.mProgressNotifyCache.Clear();

            this.bwShowNotify.RunWorkerAsync();
        }


        void mPNLocationSvc_ProcessProgressEvent(ProcessProgressEventArgs args)
        {
            this.mProgressNotifyCache.Enqueue(args);

            UpdateNotifyInfo();
        }



        private void btnAddFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "文本文件|*.csv;*.txt|所有文件|*.*";
            ofd.CheckFileExists = true;
            DialogResult result = ofd.ShowDialog();
            if (result != DialogResult.OK) return;

            this.mPNLocationSvc.SetPhoneNumber(ofd.FileName);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否要重置所有内容和结果，重新添加号码？", "重置程序",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes) return;

            this.mPNLocationSvc.Stop();
            ResetLocationSvc();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.mPNLocationSvc.Run();

            if (this.mPNLocationSvc.IsFinished)
            {
                MessageBox.Show("完成", "消息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            this.mPNLocationSvc.Stop();
        }

        private void btnSaveResult_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "文本文件|*.csv;*.txt|所有文件|*.*";
            sfd.DefaultExt = ".csv";
            DialogResult result = sfd.ShowDialog();
            if (result != DialogResult.OK) return;

            System.IO.File.WriteAllLines(sfd.FileName, this.rtbOutput.Lines, Encoding.Default);
        }


        private void timerShowNotify_Tick(object sender, EventArgs e)
        { }

        private void bwShowNotify_ProgressChanged(object sender, ProgressChangedEventArgs e)
        { }


        private void UpdateNotifyInfo()
        {
            while (this.mProgressNotifyCache.Count > 0)
            {
                ProcessProgressEventArgs processArgs = this.mProgressNotifyCache.Dequeue();

                lblCacheHit.Text = "缓存命中/长度：" + processArgs.HitCount + "/" + processArgs.CatchLength;

                lblWorking.Text = processArgs.Cur + "/" + processArgs.Max;
                pbWorking.Minimum = 0;
                pbWorking.Maximum = processArgs.Max;
                pbWorking.Value = processArgs.Cur;

                if (processArgs.PhoneNumberLocation != null)
                {
                    rtbOutput.AppendText(processArgs.PhoneNumberLocation.ToCSV() + Environment.NewLine);
                    rtbOutput.ScrollToCaret();
                }
            }

            Application.DoEvents();

        }


    }
}
