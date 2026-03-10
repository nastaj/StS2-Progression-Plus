using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProgressionPlus;

public sealed class ProgressionSaveData
{
    [JsonPropertyName("character_essence")]
    public Dictionary<string, int> CharacterEssence { get; set; } = new();

    [JsonPropertyName("character_upgrades")]
    public Dictionary<string, Dictionary<string, int>> CharacterUpgrades { get; set; } = new();
}