
using System.Text.Json.Serialization;
public class CustomEvent
{
    [JsonInclude]
    public string? WarehouseId;
    [JsonInclude]
    public string? ConveyorId ;
    [JsonInclude]
    public int RPM;
}