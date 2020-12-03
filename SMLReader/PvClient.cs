using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SMLReader
{
    public class PvClient : IDisposable
    {
        private readonly HttpClient client;
        private System.Globalization.NumberFormatInfo parseFormat = new System.Globalization.CultureInfo("en-US").NumberFormat;
        public PvClient(string PvCurrentUrl)
        {
            client = new HttpClient();
            if (!PvCurrentUrl.EndsWith("/"))
                PvCurrentUrl += "/";
            client.BaseAddress = new Uri(PvCurrentUrl);
            
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<int> GetCurrentProduction()
        {
            var response = await client.GetAsync("realtime.csv");
            if(response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var data = responseString.Split(';');
                var watts = Convert.ToInt32(Convert.ToDecimal(data[data.Length - 3]) / (65535m / 100000m));
                return watts;

            } else
            {
                throw new ApplicationException("Current Photovoltaic Production is unavailable due to http status " + (int)response.StatusCode + " :" + await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<int> GetTotalYield()
        {
        
            var response = await client.GetAsync("eternal.CSV");
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var lines = responseString.Split('\r');
                if (lines.Length < 2)
                    throw new ApplicationException("Current Photovoltaic yield is unavailable due to response wich cannot be parsed : " + responseString);
                var headers = lines[0].Split(";");
                var yieldIndex = 0;
                foreach (string s in headers)
                {
                    if (s.StartsWith("Ertrag"))
                        break;
                    yieldIndex++;
                }
                var data = lines[1].Split(';');
                var yield = Decimal.Parse(data[yieldIndex].Trim(), parseFormat);
                return Convert.ToInt32(yield*1000);

            }
            else
            {
                throw new ApplicationException("Current Photovoltaic Production is unavailable due to http status " + (int)response.StatusCode + " :" + await response.Content.ReadAsStringAsync());
            }
        }
    }
}
