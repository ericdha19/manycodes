using Azure.DigitalTwins.Core;
using Azure.Identity;

using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Twin2Twin.Common;

namespace Twin2Twin.Function
{
    public static class ReciveDtdl
    {
        private static readonly string tenantId = Environment.GetEnvironmentVariable("tenantId");
        private static readonly string clientId = Environment.GetEnvironmentVariable("clientId");
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("adtInstanceUrl");

        [FunctionName("ReciveDtdL")]
        public static async Task Run([EventHubTrigger("integration",
            Connection = "feed.connectionEvent")]
        EventData[] events, ILogger logger, ExecutionContext context)
        {
            var exceptions = new List<Exception>();
            var config = new ConfigurationBuilder()
                  .SetBasePath(context.FunctionAppDirectory)
                  .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables()
                  .Build();
            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    string secret = config["SecretFromVault"];
                    // Replace these two lines with your processing logic.
                    Azure.Core.TokenCredential token = new ClientSecretCredential(tenantId, clientId, secret);
                    DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceUrl), token);
                    await PerformAddUpdate(messageBody, secret, logger);
                    logger.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        private static async Task<bool> PerformAddUpdate(string requestBody, string secret, ILogger log)
        {
            try
            {
                var elevator = DigitalTwinAddMessage.FromJson(requestBody);
                if (!RedisStorage.QueryWhiteList(elevator.TwinId))
                    return false;
                Azure.Core.TokenCredential token = new ClientSecretCredential(tenantId, clientId, secret);
                DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceUrl), token);
                Azure.Response<string> search = null;
                try
                {
                    search = await client.GetDigitalTwinAsync(elevator.TwinId);
                    if (search.GetRawResponse().Status == 200)
                    {
                        var update = new List<TwinUpdateElevator>();
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/EquipmentStatus", Value = elevator.EquipmentStatus.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/CurrentFloor", Value = elevator.CurrentFloor.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/CurrentCall", Value = elevator.CurrentCall.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/DoorMovements", Value = elevator.DoorMovements.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/ErrorCodes", Value = elevator.ErrorCodes.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/OperationModes", Value = elevator.OperationModes.ToString() });
                        string json = update.ToJson();
                        var response = await client.UpdateDigitalTwinAsync(elevator.TwinId, json);

                        if (response.GetRawResponse().Status == 200 || response.GetRawResponse().Status == 204)
                        {
                            log.LogInformation("Update Succesfull");
                            return true;
                        }
                        else
                        if (response.GetRawResponse().Status == 400)
                        {
                            log.LogInformation("Bad Request");
                            return false;
                        }

                        if (response.GetRawResponse().Status == 404)
                        {
                            log.LogInformation("There is no digital twin with the provided id.");
                            return false;
                        }
                    }
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    TwinAddElevator add = new TwinAddElevator();
                    add.CurrentCall = elevator.CurrentCall.ToString();
                    add.CurrentFloor = elevator.CurrentFloor.ToString();
                    add.DoorMovements = elevator.DoorMovements.ToString();
                    add.EquipmentStatus = elevator.EquipmentStatus;
                    add.ErrorCodes = elevator.ErrorCodes.ToString();
                    add.OperationModes = elevator.OperationModes.ToString();
                    add.Metadata = new Metadata() { Model = "dtmi:com:Twin2Twin:Elevator;1" };
                    string json = add.ToJson();
                    var response = await client.CreateDigitalTwinAsync(elevator.TwinId, json);
                    if (response.GetRawResponse().Status == 200)
                    {
                        log.LogInformation("Create Succesfull");
                        return true;
                    }
                    else
                        if (response.GetRawResponse().Status == 400)
                    {
                        log.LogInformation("Bad Request");
                        return false;
                    }
                    if (response.GetRawResponse().Status == 412)
                    {
                        log.LogInformation("BThe model is decommissioned or the digital twin already exists");
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return false;
            }
        }
    }
}