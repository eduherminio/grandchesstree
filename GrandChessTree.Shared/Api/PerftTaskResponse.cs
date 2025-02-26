﻿using System.Text.Json.Serialization;

namespace GrandChessTree.Shared.Api;
public class PerftTaskResponse
{
    [JsonPropertyName("perft_task_id")]
    public required long PerftTaskId { get; set; }

    [JsonPropertyName("perft_item_hash")]
    public required ulong PerftItemHash { get; set; }

    [JsonPropertyName("depth")]
    public required int Depth { get; set; }
}
