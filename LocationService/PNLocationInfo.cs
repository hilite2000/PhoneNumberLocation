using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoneNumberLocation.LocationService
{
    /// <summary>
    /// 号段归属地信息
    /// </summary>
    public class SegmengtLocationInfo
    {
        /// <summary>
        /// 手机号段
        /// </summary>
        public string NumberSegment
        {
            get;
            set;
        }

        public string Province
        { get; set; }


        public string City
        { get; set; }

        /// <summary>
        /// 手机号码类型
        /// </summary>
        public string CardType
        { get; set; }


        /// <summary>
        /// 区号
        /// </summary>
        public string AreaCode
        { get; set; }


        /// <summary>
        /// 邮编
        /// </summary>
        public string PostCode
        { get; set; }

        public override string ToString()
        {
            string result = "";
            result += "[" + this.Province + "]";
            result += "[" + this.City + "]";
            result += "[" + this.CardType + "]";
            result += "[" + this.AreaCode + "]";
            result += "[" + this.PostCode + "]";

            return result;
        }

        public string ToCSV()
        {
            string csv = "";
            csv += this.Province;
            csv += "," + this.City;
            csv += "," + this.CardType;
            csv += ",\"" + this.AreaCode + "\"";
            csv += ",\"" + this.PostCode + "\"";

            return csv;
        }

    }

    /// <summary>
    /// 手机号码归属地信息
    /// </summary>
    public class PNLocationInfo
    {
        public override string ToString()
        {
            string result = this.Segment.ToString();
            result = "[" + this.PhoneNumber + "]" + result + "[" + this.Result + "]";
            return result;
        }

        public string ToCSV()
        {
            string csv = this.Segment.ToCSV();
            csv = "\"" + this.PhoneNumber + "\"," + csv + "," + this.Result;
            return csv;
        }

        /// <summary>
        /// 查询结果：为空成功，否则返回错误信息
        /// </summary>
        public string Result
        { get; set; }


        private string mPhoneNumber;


        public string PhoneNumber
        {
            get
            { return this.mPhoneNumber; }

            set
            {
                this.mPhoneNumber = value;
                this.Segment.NumberSegment = GetSegmeng(value);
            }
        }


        private SegmengtLocationInfo mSegment = new SegmengtLocationInfo();

        /// <summary>
        /// 所属号段信息
        /// </summary>
        public SegmengtLocationInfo Segment
        {
            get
            {
                return this.mSegment;
            }
            set
            {
                if (string.IsNullOrEmpty(value.NumberSegment))
                    value.NumberSegment = GetSegmeng(this.PhoneNumber);

                this.mSegment = value;
            }
        }

        private string GetSegmeng(string pn)
        {
            if (string.IsNullOrEmpty(pn) || pn.Length < 7)
                return "0000000";
            else
                return pn.Substring(0, 7);
        }
    }
}
