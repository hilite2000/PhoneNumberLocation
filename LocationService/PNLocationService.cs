using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoneNumberLocation.LocationService
{
    public delegate void ProcessProgressDelegate(ProcessProgressEventArgs args);


    /// <summary>
    /// 查询进度反馈事件
    /// </summary>
    public class ProcessProgressEventArgs : EventArgs
    {
        public ProcessProgressEventArgs()
        {
            this.Msg = "";
        }

        public int Max
        { get; set; }

        public int Cur
        { get; set; }

        public string Msg
        { get; set; }

        /// <summary>
        /// 号码归属地信息，如果仅更新总数量时，此值为null
        /// </summary>
        public PNLocationInfo PhoneNumberLocation
        { get; set; }

        /// <summary>
        /// 缓存的长度
        /// </summary>
        public int CatchLength
        { get; set; }

        /// <summary>
        /// 目前命中次数
        /// </summary>
        public int HitCount
        { get; set; }

        /// <summary>
        /// 发生的动作：0缓存增长；1命中缓存
        /// </summary>
        public int Action
        { get; set; }
    }


    /// <summary>
    /// 获取号码归属地服务
    /// </summary>
    class PNLocationService
    {
        /// <summary>
        /// 处理进度事件
        /// </summary>
        public event ProcessProgressDelegate ProcessProgressEvent;

        private bool mRunning = false;

        public bool IsRuning
        {
            get
            { return this.mRunning; }
        }

        /// <summary>
        /// 待查查询的手机号码
        /// </summary>
        private List<string> mPNList = new List<string>();

        /// <summary>
        /// 等待查询的号码
        /// </summary>
        private Queue<string> mPNQueue = new Queue<string>();

        /// <summary>
        /// 号码归属地查询结果
        /// </summary>
        private List<PNLocationInfo> mPNLocationResultList = new List<PNLocationInfo>();

        /// <summary>
        /// 号段缓存
        /// </summary>
        private Dictionary<string, SegmengtLocationInfo> mSegmengInfoCache = new Dictionary<string, SegmengtLocationInfo>();

        /// <summary>
        /// 缓存命中计数
        /// </summary>
        private int mCacheHitCount = 0;

        /// <summary>
        /// 是否已经完成
        /// </summary>
        public bool IsFinished
        { get { return this.mPNQueue.Count == 0; } }


        /// <summary>
        /// 添加要查询的号码
        /// </summary>
        /// <param name="fileName"></param>
        public void SetPhoneNumber(string fileName)
        {
            string[] pnList = System.IO.File.ReadAllLines(fileName);
            this.mPNList.AddRange(pnList);

            foreach (var item in this.mPNList)
                this.mPNQueue.Enqueue(item);

            OnProcessProgress(null, 0);
        }

        public void Stop()
        {
            this.mRunning = false;
        }

        public void Run()
        {
            this.mRunning = true;

            int hitType = 0;

            IPNLocationSource locationSource = CreateLocationSource();
            while (this.mPNQueue.Count > 0)
            {
                if (this.mRunning == false) break;

                string pn = this.mPNQueue.Dequeue();

                PNLocationInfo pnInfo = new PNLocationInfo();
                pnInfo.PhoneNumber = pn;
                pnInfo.Result = "还未查询";

                if (this.mSegmengInfoCache.ContainsKey(pnInfo.Segment.NumberSegment))
                {
                    pnInfo.Segment = this.mSegmengInfoCache[pnInfo.Segment.NumberSegment];
                    pnInfo.Result = "";

                    this.mCacheHitCount++;

                    hitType = 1;
                }
                else
                {
                    locationSource.FullLocation(pnInfo);
                    this.mSegmengInfoCache[pnInfo.Segment.NumberSegment] = pnInfo.Segment;

                    hitType = 0;
                }

                this.mPNLocationResultList.Add(pnInfo);

                OnProcessProgress(pnInfo, hitType);
            }
        }


        private void OnProcessProgress(PNLocationInfo pnInfo, int hitType)
        {
            if (this.ProcessProgressEvent == null) return;

            ProcessProgressEventArgs args = new ProcessProgressEventArgs();

            args.Cur = this.mPNLocationResultList.Count;
            args.Max = this.mPNList.Count;
            args.PhoneNumberLocation = pnInfo;
            args.Msg = "";

            args.Action = hitType;
            args.CatchLength = this.mSegmengInfoCache.Count;
            args.HitCount = this.mCacheHitCount;

            this.ProcessProgressEvent(args);
        }


        /// <summary>
        /// 创建数据源对象
        /// </summary>
        /// <returns></returns>
        private IPNLocationSource CreateLocationSource()
        {
            string typeStr = System.Configuration.ConfigurationManager.AppSettings["sourceType"];
            Type sourceType = Type.GetType(typeStr);
            IPNLocationSource source = Activator.CreateInstance(sourceType) as IPNLocationSource;

            //IPNLocationSource source = new DataSource.ShowJiSource();
            //IPNLocationSource source = new DataSource.IP138Source();
            //IPNLocationSource source = new DataSource.BaiDuSource();

            return source;
        }

    }
}
