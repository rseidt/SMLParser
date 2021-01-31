using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SMLReader
{
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

        public void AddEffectivePoint(string Measurement, int effective, int buy, int load, int production, int charge, int load_wo_charge)
        {
            long timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            var point = $"{Measurement} effective={effective}i,buy={buy}i,load={load}i,production={production}i,charge={charge}i,load_wo_charge={load_wo_charge}i {timestamp}";
            if (pointSet.Length > 0)
                pointSet.Append("\n");
            pointSet.Append(point);
            QueueClear = false;
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


        public async Task<PersistenceResult> PersistCumulative(int Obis280, int Obis180, int yield, int charge)
        {

            long timestamptoday = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var pointString = $"cumulative obis180={Obis180}i,obis280={Obis280}i,yield={yield}i,charge={charge}i {timestamptoday}";

            var response = await client.PostAsync($"write?bucket={cumulativeBucket}&org={org}&precision=s", new StringContent(
                    pointString
                ));
            PersistenceResult result = new PersistenceResult();
            result.IsSuccessMessage = response.IsSuccessStatusCode;
            result.ErrorMessage = await response.Content.ReadAsStringAsync();
            result.ReturnCode = (int)response.StatusCode;
            result.UnwrittenPoints = result.IsSuccessMessage ? "" : pointString;
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
