using Newtonsoft.Json;

namespace Twin2Twin.Common
{
    public class ElevatorTwin
    {
        [JsonProperty("TwinId")]
        public string TwinId { get; set; }

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
}