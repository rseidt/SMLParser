using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SMLReader
{
    public class IobClient
    {
        private readonly HttpClient client;
        private readonly NumberFormatInfo decimalInfo = System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat;

        public IobClient(string ioBrokerBaseUrl)
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

        public async Task<bool> UpdatePowerData(decimal yield, decimal delivery, decimal load)
        {
            string contentPattern = "knx.0.Sensorik.Zentral.Einspeisung={0}&knx.0.Sensorik.Zentral.Ertrag={1}&knx.0.Sensorik.Zentral.Gesamtleistung={2}";
            var stringContent = new StringContent(string.Format(contentPattern, delivery.ToString(decimalInfo), yield.ToString(decimalInfo), load.ToString(decimalInfo)));

            var response = await client.PostAsync("setBulk", stringContent);

            if (response.IsSuccessStatusCode)
            {
                return true;
            } else
            {
                throw new ApplicationException("Could not update power data due to unexpected http status: " + response.StatusCode);
            }






                /*
                 * http://ip:8087/setBulk?hm-rpc.0.FEQ1234567:1.LEVEL=0.7&Anwesenheit=0&prettyPrint
      [
        {
          "id": "hm-rpc.0.FEQ1234567:1.LEVEL",
          "val": "0.7"
        },
        {
          "error": "error: datapoint \"Anwesenheit\" not found"
        }
      ]
    You can send this request as POST too.
                */
            }
    }
}
