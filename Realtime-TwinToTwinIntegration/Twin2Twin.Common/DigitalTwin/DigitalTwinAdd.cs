using Newtonsoft.Json;

namespace Twin2Twin.Common
{
    public partial class TwinAddElevator
    {
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

    public partial class Metadata
    {
        [JsonProperty("$model")]
        public string Model { get; set; }
    }

    public partial class TwinAddElevator
    {
        public static TwinAddElevator FromJson(string json) => JsonConvert.DeserializeObject<TwinAddElevator>(json);
    }

    public static class SerializeAddElevator
    {
        public static string ToJson(this TwinAddElevator self) => JsonConvert.SerializeObject(self);
    }
}