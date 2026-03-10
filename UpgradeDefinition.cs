using System;

namespace ProgressionPlus;

public sealed class UpgradeDefinition
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required int MaxRank { get; init; }

    public required Func<int, int> GetCostForNextRank { get; init; }
}