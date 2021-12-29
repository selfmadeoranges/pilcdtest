using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusStopPi
{

    public enum RECVW { FAILED = -3, UNKNOWN = -2, TIMEOUT = -1, NO_BUS = 0, SUCCESS = 1 };

    class WeatherAPI
    {

        public string temp = "0";
        public string wetCd = "0";
        public string busNum = "";

        public async Task<RECVW> GetNowWeather()
        {
            string nowtime = DateTime.Now.ToString("yyyyMMdd");
            HttpWebRequest getReq = (HttpWebRequest)WebRequest.Create($"https://weather.naver.com/today/api/nation/{nowtime}/now");
            try
            {
                getReq.Method = "GET";
                getReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:35.0) Gecko/20100101 Firefox/35.0";
                getReq.ContentType = "application/json";
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    return RECVW.TIMEOUT;
                }
                else
                {
                    return RECVW.UNKNOWN;
                }
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)await getReq.GetResponseAsync();
            }
            catch (WebException ex)
            {
                WebExceptionStatus rtrn = ex.Status;
                return RECVW.UNKNOWN;
            }

            string html = null;

            using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                html = sr.ReadToEnd();
            }

            JsonDocument jdom = JsonDocument.Parse(html);
            JsonElement jroot = jdom.RootElement;
            JsonElement jLines = jroot.GetProperty("N09140104");
            temp = jLines.GetProperty("tmpr").GetDecimal().ToString();
            wetCd = jLines.GetProperty("wetrCd").GetString();

            return RECVW.SUCCESS;
        }

    }
}
