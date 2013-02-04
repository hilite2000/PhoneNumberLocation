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
    public class BaiDuSource : IPNLocationSource
    {
        private WebClient mWebClient = new WebClient();
        private HtmlAgilityPack.HtmlDocument mHtmlDoc = new HtmlAgilityPack.HtmlDocument();

        #region IPNLocationSource 成员

        public void FullLocation(PNLocationInfo localInfo)
        {
            string queryUrl = "http://www.baidu.com/s?wd=" + localInfo.PhoneNumber;
            try
            {
                Stream stream = this.mWebClient.OpenRead(queryUrl);
                StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                string responseString = sr.ReadToEnd();
                this.mHtmlDoc.LoadHtml(responseString);
            }
            catch (Exception ex)
            {
                localInfo.Result = "访问服务时出错：" + ex.Message;
                return;
            }

            HtmlAgilityPack.HtmlNode selectedNode = this.mHtmlDoc.DocumentNode.SelectSingleNode("//div[@class='op_mp_r']/span[2]");
            if (selectedNode == null)
            {
                localInfo.Result = "服务返回结构不正确";
                return;
            }

            string[] result = selectedNode.InnerText.Split(new string[] { "&nbsp;" }, StringSplitOptions.None);

            localInfo.Segment.City = result[1];
            localInfo.Segment.Province = result[0] == "" ? localInfo.Segment.City : result[0]; //直辖市
            localInfo.Segment.CardType = result[3];
            localInfo.Segment.AreaCode = "";
            localInfo.Segment.PostCode = "";



            localInfo.Result = string.Empty;
        }

        #endregion
    }
}
