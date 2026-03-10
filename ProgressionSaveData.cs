using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProgressionPlus;

public sealed class ProgressionSaveData
{
    [JsonPropertyName("character_essence")]
    public Dictionary<string, int> CharacterEssence { get; set; } = new();
}