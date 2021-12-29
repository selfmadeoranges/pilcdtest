using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusStopPi
{
    //                 실패(NotUsed)  알수없음     타임아웃     버스없음      성공!!
    public enum RECV { FAILED = -3, UNKNOWN = -2, TIMEOUT = -1, NO_BUS = 0, SUCCESS = 1 };

    class BusAPI
    {

        public int arrival1 = 0;
        public int arrival2 = 0;
        public string busNum = "";

        public async Task<RECV> GetBusStopData(string busstopid, string buslines)
        {
            //https://map.kakao.com/bus/stop.json?busstopid=11050601005&buslines=1100061527
            HttpWebRequest getReq = (HttpWebRequest)WebRequest.Create($"https://map.kakao.com/bus/stop.json?busstopid={busstopid}&buslines={buslines}");
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
                    return RECV.TIMEOUT;
                }
                else
                {
                    return RECV.UNKNOWN;
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
                return RECV.UNKNOWN;
            }

            string html = null;

            using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                html = sr.ReadToEnd();
            }

            JsonDocument jdom = JsonDocument.Parse(html);
            JsonElement jroot = jdom.RootElement;
            JsonElement jLines = jroot.GetProperty("lines");
            int jBusLength = jLines.GetArrayLength();

            if (jBusLength < 1)
            {
                return RECV.NO_BUS;
            }

            for (int i = 0; i < jBusLength; i++)
            {
                busNum = jLines[i].GetProperty("name").GetString();
                if (busNum.Equals("302"))
                {
                    JsonElement jBusData = jLines[i].GetProperty("arrival");
                    arrival1 = jBusData.GetProperty("arrivalTime").GetInt32();
                    arrival2 = jBusData.GetProperty("arrivalTime2").GetInt32();

                    return RECV.SUCCESS;
                }
            }

            return RECV.NO_BUS;
        }
    }
}
