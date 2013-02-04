using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhoneNumberLocation.LocationService;
using System.IO;
using System.Net;

namespace PhoneNumberLocation.DataSource
{
    /// <summary>
    /// 用下列网站提供的解析服务：http://www.ShouJi.com
    /// </summary>
    class ShowJiSource : IPNLocationSource
    {
        private WebClient mWebClient = new WebClient();
        private HtmlAgilityPack.HtmlDocument mHtmlDoc = new HtmlAgilityPack.HtmlDocument();

        #region IPNLocationSource 成员

        public void FullLocation(PNLocationInfo localInfo)
        {
            this.mHtmlDoc.OptionOutputOriginalCase = true;

            string queryUrl = "http://api.showji.com/Locating/www.showji.com.aspx?m=" + localInfo.PhoneNumber;
            Uri queryUri = new Uri(queryUrl);

            try
            {
                Stream stream = this.mWebClient.OpenRead(queryUri);
                StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                string result = sr.ReadToEnd();
                this.mHtmlDoc.LoadHtml(result);
            }
            catch (Exception ex)
            {
                localInfo.Result = "访问服务时出错：" + ex.Message;
                return;
            }

            HtmlAgilityPack.HtmlNode rootNode = this.mHtmlDoc.DocumentNode.SelectSingleNode("//queryresponse");
            if (rootNode == null)
            {
                localInfo.Result = "服务返回结构不正确";
                return;
            }

            HtmlAgilityPack.HtmlNode nodeQueryResult = rootNode.SelectSingleNode("queryresult");
            if (nodeQueryResult == null || string.Equals(nodeQueryResult.InnerText, "false", StringComparison.CurrentCultureIgnoreCase))
            {
                localInfo.Result = "服务没有结果返回";
                return;
            }

            localInfo.PhoneNumber = rootNode.SelectSingleNode("mobile").InnerText;
            localInfo.Segment.Province = rootNode.SelectSingleNode("province").InnerText;
            localInfo.Segment.City = rootNode.SelectSingleNode("city").InnerText;
            localInfo.Segment.AreaCode = rootNode.SelectSingleNode("areacode").InnerText;
            localInfo.Segment.PostCode = rootNode.SelectSingleNode("postcode").InnerText;
            localInfo.Segment.CardType = rootNode.SelectSingleNode("corp").InnerText;
            localInfo.Segment.CardType += " " + rootNode.SelectSingleNode("card").InnerText;

            localInfo.Result = string.Empty;
        }

        #endregion
    }
}
