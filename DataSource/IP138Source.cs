using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

using PhoneNumberLocation.LocationService;

namespace PhoneNumberLocation.DataSource
{
    /// <summary>
    /// 用下列网站提供的解析服务：http://www.ip138.com:8080
    /// </summary>
    public class IP138Source : IPNLocationSource
    {
        private WebClient mWebClient = new WebClient();
        private HtmlAgilityPack.HtmlDocument mHtmlDoc = new HtmlAgilityPack.HtmlDocument();

        #region IPhoneNumberSource 成员

        public void FullLocation(PNLocationInfo localInfo)
        {
            string queryUrl = "http://www.ip138.com:8080/search.asp?action=mobile&mobile=" + localInfo.PhoneNumber;
            Uri queryUri = new Uri(queryUrl);

            try
            {
                Stream stream = this.mWebClient.OpenRead(queryUri);
                StreamReader sr = new StreamReader(stream, Encoding.Default);
                string result = sr.ReadToEnd();
                this.mHtmlDoc.LoadHtml(result);
            }
            catch (Exception ex)
            {
                localInfo.Result = "访问服务时出错：" + ex.Message;
                return;
            }

            HtmlAgilityPack.HtmlNodeCollection nodeCol = this.mHtmlDoc.DocumentNode.SelectNodes("//table/tr/td[@class='tdc2']");
            if (nodeCol.Count <= 0)
            {
                localInfo.Result = "服务返回结构不正确";
                return;
            }

            localInfo.PhoneNumber = nodeCol[0].FirstChild.InnerText;
            localInfo.Segment.Province = nodeCol[1].FirstChild.InnerText;
            localInfo.Segment.CardType = nodeCol[2].FirstChild.InnerText;
            localInfo.Segment.AreaCode = nodeCol[3].FirstChild.InnerText;
            localInfo.Segment.PostCode = nodeCol[4].FirstChild.InnerText;

            try
            {
                string[] pc = localInfo.Segment.Province.Split(new string[] { "&nbsp;" }, StringSplitOptions.None);

                //为了保留原始信息，用这个顺序
                localInfo.Segment.City = pc[1];
                localInfo.Segment.Province = pc[0];
            }
            catch (Exception ex)
            {
                localInfo.Segment.City = ex.Message;
            }

            localInfo.Result = string.Empty;
        }

        #endregion
    }
}
