using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SMLReader
{
    public class ChargerClient
    {
        private readonly HttpClient client;

        public ChargerClient(string ioBrokerBaseUrl)
        {
            client = new HttpClient();
            if (!ioBrokerBaseUrl.EndsWith("/"))
                ioBrokerBaseUrl += "/";
            client.BaseAddress = new Uri(ioBrokerBaseUrl);
        }

        public async Task<int> GetCurrentChargingPower()
        {
            var allChargingPhaseTasks = new Task<HttpResponseMessage>[3];
            allChargingPhaseTasks[0] = client.GetAsync("getPlainValue/knx.0.Sensorik.Zentral.Wallbox-Leistung_L3");
            allChargingPhaseTasks[1] = client.GetAsync("getPlainValue/knx.0.Sensorik.Zentral.Wallbox-Leistung_L2");
            allChargingPhaseTasks[2] = client.GetAsync("getPlainValue/knx.0.Sensorik.Zentral.Wallbox-Leistung_L1");

            var responses = await Task.WhenAll(allChargingPhaseTasks);
            decimal totalPower = 0;
            foreach (var response in responses)
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    decimal watts;
                    
                    if (Decimal.TryParse(responseString, out watts))
                    {
                        totalPower += watts;
                    } else
                    {
                        throw new ApplicationException("Current Charging Power is unavailable due to non-devimal value '" + responseString + "'");
                    }
                }
                else
                {
                    throw new ApplicationException("Current Charging Power is unavailable due to http status " + (int)response.StatusCode + " :" + await response.Content.ReadAsStringAsync());
                }
            }
            return Convert.ToInt32(totalPower * 1000);
        }

        public async Task<Int64> GetTotalchargingConsumption()
        {
            var allChargingPhaseTasks = new Task<HttpResponseMessage>[3];
            var response = await client.GetAsync("getPlainValue/knx.0.Sensorik.Zentral.Wallbox-Z%C3%A4hler");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                Int64 kwh;
                if (Int64.TryParse(responseString, out kwh))
                {
                    return kwh;
                }
                else
                {
                    throw new ApplicationException("Total Charging Consumption is unavailable due to non-Int64 value '" + responseString + "'");
                }
            }
            else
            {
                throw new ApplicationException("Total Charging Consumtion is unavailable due to http status " + (int)response.StatusCode + " :" + await response.Content.ReadAsStringAsync());
            }
        }
    }
}
