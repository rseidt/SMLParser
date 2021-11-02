using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SMLReader
{
    public class IntValue
    {
        public string Name;
        public int Value;
    }

    class SMLPowerInfluxDBClient : IDisposable
    {
        private readonly string token;
        private readonly string effectiveBucket;
        private readonly string cumulativeBucket;
        private readonly string org;
        private readonly HttpClient client;
        private readonly StringBuilder pointSet;
        public bool QueueClear = true;
        public SMLPowerInfluxDBClient(string InfluxDbUrl, string Token, string EffectiveBucket, string CumulativeBucket, string Org)
        {
            pointSet = new StringBuilder();

            InfluxDbUrl = InfluxDbUrl.TrimEnd('/');
            InfluxDbUrl += "/api/v2/";

            this.token = Token;
            this.effectiveBucket = EffectiveBucket;
            this.cumulativeBucket = CumulativeBucket;
            this.org = Org;

            this.client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);
            client.BaseAddress = new Uri(InfluxDbUrl);


        }

        public StringBuilder AddEffectivePoint(string Measurement, IEnumerable<IntValue> values) //string Measurement, int effective, int buy, int load, int production, int charge, int load_wo_charge
        {
            long timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            StringBuilder pointString = new StringBuilder();
            pointString.Append(Measurement + " ");
            for (int i = 0; i < values.Count(); i++)
            {
                var val = values.ElementAt(i);
                pointString.Append(val.Name + "=" + val.Value + "i");
                if (i < values.Count() - 1)
                    pointString.Append(',');
            }
            pointString.Append(" " + timestamp.ToString());
            var writeString = pointString.ToString();

            if (pointSet.Length > 0)
                pointSet.Append('\n');
            pointSet.Append(writeString);
            QueueClear = false;
            return pointSet;
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<PersistenceResult> PersistEffective()
        {
            var pointString = pointSet.ToString();
            pointSet.Clear();
            QueueClear = true;
            var response = await client.PostAsync($"write?bucket={effectiveBucket}&org={org}&precision=ms", new StringContent(
                pointString
                ));
            PersistenceResult result = new PersistenceResult();
            result.IsSuccessMessage = response.IsSuccessStatusCode;
            result.ErrorMessage = await response.Content.ReadAsStringAsync();
            result.ReturnCode = (int)response.StatusCode;
            result.UnwrittenPoints = result.IsSuccessMessage ? "" : pointString;
            return result;
        }


        public async Task<PersistenceResult> PersistCumulative(IEnumerable<IntValue> values) //int Obis280, int Obis180, int yield, int charge
        {

            long timestamptoday = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            StringBuilder pointString = new StringBuilder();
            pointString.Append("cumulative ");
            for (int i = 0; i < values.Count(); i++)
            {
                var val = values.ElementAt(i);
                pointString.Append(val.Name + "=" + val.Value + "i");
                if (i < values.Count() - 1)
                    pointString.Append(',');
            }
            pointString.Append(" " + timestamptoday.ToString());
            var writeString = pointString.ToString();
            var response = await client.PostAsync($"write?bucket={cumulativeBucket}&org={org}&precision=s", new StringContent(
                    writeString
                ));
            PersistenceResult result = new PersistenceResult();
            result.IsSuccessMessage = response.IsSuccessStatusCode;
            result.ErrorMessage = await response.Content.ReadAsStringAsync();
            result.ReturnCode = (int)response.StatusCode;
            result.UnwrittenPoints = result.IsSuccessMessage ? "" : writeString;
            return result;
        }
    }
    public class PersistenceResult
    {
        public string ErrorMessage { get; set; }
        public int ReturnCode { get; set; }
        public bool IsSuccessMessage { get; set; }
        public string UnwrittenPoints { get; set; }
    }
}
