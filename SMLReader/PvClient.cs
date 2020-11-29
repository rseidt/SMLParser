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
    }
}
