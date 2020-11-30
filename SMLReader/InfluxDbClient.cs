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
        private readonly string bucket;
        private readonly string org;
        private readonly HttpClient client;
        private readonly StringBuilder pointSet;
        public bool QueueClear = true;
        public SMLPowerInfluxDBClient(string InfluxDbUrl, string Token, string Bucket, string Org)
        {
            pointSet = new StringBuilder();

            InfluxDbUrl = InfluxDbUrl.TrimEnd('/');
            InfluxDbUrl += "/api/v2/";

            this.token = Token;
            this.bucket = Bucket;
            this.org = Org;

            this.client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);
            client.BaseAddress = new Uri(InfluxDbUrl);
            

        }

        public void AddPoint(string Measurement, int effective, int buy, int load, int production){
            long timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            var point = $"{Measurement} effective={effective}i,buy={buy}i, load={load}i,production={production}i {timestamp}";
            if (pointSet.Length > 0)
                pointSet.Append("\n");
            pointSet.Append(point);
            QueueClear = false;
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<PersistenceResult> Persist()
        {
            var pointString = pointSet.ToString();
            pointSet.Clear();
            QueueClear = true;
            var response = await client.PostAsync($"write?bucket={bucket}&org={org}&precision=ms", new StringContent(
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
