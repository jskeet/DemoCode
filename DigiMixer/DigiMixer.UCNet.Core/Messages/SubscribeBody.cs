using Newtonsoft.Json;

namespace DigiMixer.UCNet.Core.Messages;

/// <summary>
/// The body for a <see cref="JsonMessage"/> "Subscribe" request.
/// </summary>
public class SubscribeBody
{
    [JsonProperty("id")]
    public string Id { get; set; } = "Subscribe";

    [JsonProperty("clientName")]
    public string ClientName { get; set; } = "DigiMixer";

    [JsonProperty("clientInternalName")]
    public string ClientInternalName { get; set; } = "ucapp";

    [JsonProperty("clientType")]
    public string ClientType { get; set; } = "PC";

    [JsonProperty("clientDescription")]
    public string ClientDescription { get; set; } = "Description";

    [JsonProperty("clientIdentifier")]
    public string ClientIdentifier { get; set; } = "Identifier";

    [JsonProperty("clientOptions")]
    public string ClientOptions { get; set; } = "perm users levl redu rtan";

    [JsonProperty("clientEncoding")]
    public int ClientEncoding { get; set; } = 23106;
}
