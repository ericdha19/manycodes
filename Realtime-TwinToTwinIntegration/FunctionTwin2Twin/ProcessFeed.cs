using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Twin2Twin.Common;

namespace Twin2Twin.Function
{
    public static class ProcessFeed
    {
        private static readonly string endpointUrl = Environment.GetEnvironmentVariable("feed.endpoint");
        private static readonly string key = Environment.GetEnvironmentVariable("feed.authKey");
        private static readonly string databaseName = Environment.GetEnvironmentVariable("feed.database");
        private static readonly string collectionName = Environment.GetEnvironmentVariable("feed.collection");

        private static readonly string connectionStringEventHub = Environment.GetEnvironmentVariable("feed.connectionEvent");
        private static readonly string eventHubName = Environment.GetEnvironmentVariable("feed.HubName");

        [FunctionName("ProcessFeed")]
        public static async Task RunAsync([CosmosDBTrigger(
            databaseName: "realtimetwindb",
            collectionName: "realtimetwinstate",
            ConnectionStringSetting = "feed.endpoint",
            LeaseCollectionName = "leases", CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
                var listChanges = ProcessToJson(input, log);
                await PublishChanges(listChanges, log);
            }
        }

        private static List<string> ProcessToJson(IReadOnlyList<Document> input, ILogger log)
        {
            var listChanges = new List<string>();
            foreach (var document in input)
            {
                dynamic data = JsonConvert.DeserializeObject(document.ToString());
                try
                {
                    var elevator = Elevator.FromJson(document.ToString());
                    var add = new DigitalTwinAddMessage
                    {
                        TwinId = elevator.TwinId,
                        CurrentCall = elevator.CallFloorDestination.ToString(),
                        CurrentFloor = elevator.CurrentFloor.ToString(),
                        DoorMovements = elevator.Counters.DoorMovements.ToString(),
                        EquipmentStatus = elevator.LastEquipmentStatus,
                        ErrorCodes = elevator.Counters.ErrorCodes.ToString(),
                        OperationModes = elevator.Counters.OpModes.ToString(),
                        Metadata = new Metadata() { Model = "dtmi:com:Twin2Twin:Elevator;1" }
                    };
                    string json = add.ToJson();
                    listChanges.Add(json);
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message);
                }
            }
            return listChanges;
        }

        private static async Task PublishChanges(List<string> changes, ILogger log)
        {
            await using (var producerClient = new EventHubProducerClient(connectionStringEventHub, eventHubName))
            {
                try
                {
                    using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
                    foreach (var ch in changes)
                    {
                        eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(ch)));
                    }
                    await producerClient.SendAsync(eventBatch);
                    log.LogInformation("A batch of events has been published.");
                }
                catch (Exception ex)
                {
                    log.LogInformation(ex.Message);
                }
            }
        }
    }
}