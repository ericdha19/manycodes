using Newtonsoft.Json;

namespace Twin2Twin.Common
{
    public partial class DigitalTwinAddMessage
    {
        [JsonProperty("TwinId")]
        public string TwinId { get; set; }

        [JsonProperty("$metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("EquipmentStatus")]
        public string EquipmentStatus { get; set; }

        [JsonProperty("CurrentFloor")]
        public string CurrentFloor { get; set; }

        [JsonProperty("CurrentCall")]
        public string CurrentCall { get; set; }

        [JsonProperty("DoorMovements")]
        public string DoorMovements { get; set; }

        [JsonProperty("ErrorCodes")]
        public string ErrorCodes { get; set; }

        [JsonProperty("OperationModes")]
        public string OperationModes { get; set; }
    }

    public partial class DigitalTwinAddMessage
    {
        public static DigitalTwinAddMessage FromJson(string json) => JsonConvert.DeserializeObject<DigitalTwinAddMessage>(json);
    }

    public static class SerializeDigitalTwinAddMessage
    {
        public static string ToJson(this DigitalTwinAddMessage self) => JsonConvert.SerializeObject(self);
    }
}