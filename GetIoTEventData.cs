using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;


namespace Company.Function
{
    public static class GetIoTEventData
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("index")]
        public static IActionResult GetHomePage([HttpTrigger(AuthorizationLevel.Anonymous)]HttpRequest req, ExecutionContext context)
        {
            var path = Path.Combine(context.FunctionAppDirectory, "content", "index.html");
            
            return new ContentResult
            {
                Content = File.ReadAllText(path),
                ContentType = "text/html",
            };
        }
        
        [FunctionName("GetIoTEventData")]
        public static async Task Run([
            IoTHubTrigger("messages/events", Connection = "ConnectionString")]EventData message, 
            [SignalR(HubName = "IOTHub")]IAsyncCollector<SignalRMessage> signalRMessages,      
            ILogger log)
        {
    
            Telemetry telemetry = JsonConvert.DeserializeObject<Telemetry>(Encoding.UTF8.GetString(message.Body.Array));

            await signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "iotClient",
                Arguments = new[] { telemetry.temperature.ToString() }
            })
            .ConfigureAwait(false);
        }

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "IOTHub")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }

    public class Telemetry{
        public int messageId {get; set;}
        public string deviceId {get; set;}

        public long temperature {get; set;}
        public long humidity {get; set;}
    }
}