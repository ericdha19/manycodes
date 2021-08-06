using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;

using Twin2Twin.Common;

namespace FunctionTwin2Twin
{
    public static class ReciveFeedFunction
    {
        private static readonly string tenantId = Environment.GetEnvironmentVariable("tenantId");
        private static readonly string clientId = Environment.GetEnvironmentVariable("clientId");
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("adtInstanceUrl");
        //private static readonly string secret = Environment.GetEnvironmentVariable("SecretFromVault");

        [FunctionName("ReciveFeedFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                   .SetBasePath(context.FunctionAppDirectory)
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables()
                   .Build();
            // Access our secret setting, normally as any other setting
            string secret = config["SecretFromVault"];

            string name = req.Query["name"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            log.LogInformation("C# HTTP trigger function processed a request.");
            log.LogInformation(name);
            log.LogInformation(requestBody);
            return await PerformAddUpdate(requestBody, secret);
        }

        private static async Task<IActionResult> PerformAddUpdate(string requestBody, string secret)
        {
            try
            {
                Elevator elevator = Elevator.FromJson(requestBody);
                Azure.Core.TokenCredential token = new ClientSecretCredential(tenantId, clientId, secret);
                DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceUrl), token);
                Azure.Response<string> search = null;
                try
                {
                    search = await client.GetDigitalTwinAsync(elevator.TwinId);
                    if (search.GetRawResponse().Status == 200)
                    {
                        var update = new List<TwinUpdateElevator>();
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/EquipmentStatus", Value = elevator.LastEquipmentStatus.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/CurrentFloor", Value = elevator.CurrentFloor.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/CurrentCall", Value = elevator.CallFloorDestination.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/DoorMovements", Value = elevator.Counters.DoorMovements.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/ErrorCodes", Value = elevator.Counters.ErrorCodes.ToString() });
                        update.Add(new TwinUpdateElevator() { Op = "replace", Path = "/OperationModes", Value = elevator.Counters.OpModes.ToString() });
                        string json = update.ToJson();
                        var response = await client.UpdateDigitalTwinAsync(elevator.TwinId, json);

                        if (response.GetRawResponse().Status == 200 || response.GetRawResponse().Status == 204)
                            return new OkObjectResult("Update Succesfull");
                        else
                        if (response.GetRawResponse().Status == 400)
                            return new BadRequestObjectResult("Bad Request");
                        if (response.GetRawResponse().Status == 404)
                            return new BadRequestObjectResult("There is no digital twin with the provided id.");
                    }
                    else
                        return new InternalServerErrorResult();
                }
                catch (RequestFailedException ex)
                {
                    if (ex.Status == 404)
                    {
                        TwinAddElevator add = new TwinAddElevator();
                        add.CurrentCall = elevator.CallFloorDestination.ToString();
                        add.CurrentFloor = elevator.CurrentFloor.ToString();
                        add.DoorMovements = elevator.Counters.DoorMovements.ToString();
                        add.EquipmentStatus = elevator.LastEquipmentStatus;
                        add.ErrorCodes = elevator.Counters.ErrorCodes.ToString();
                        add.OperationModes = elevator.Counters.OpModes.ToString();
                        add.Metadata = new Metadata() { Model = "dtmi:com:Twin2Twin:Elevator;1" };
                        string json = add.ToJson();
                        var response = await client.CreateDigitalTwinAsync(elevator.TwinId, json);
                        if (response.GetRawResponse().Status == 200)
                            return new OkObjectResult("Create Succesfull");
                        else
                        if (response.GetRawResponse().Status == 400)
                            return new BadRequestObjectResult("Bad Request");
                        if (response.GetRawResponse().Status == 412)
                            return new BadRequestObjectResult("The model is decommissioned or the digital twin already exists");
                    }
                }
                return new InternalServerErrorResult();
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, true);
            }
        }
    }
}