using System.Text.Json.Serialization;
using GrandChessTree.Shared.Api;

namespace GrandChessTree.Client;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, WriteIndented = true)]
[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(PerftTask))]
[JsonSerializable(typeof(PerftTask[]))]
[JsonSerializable(typeof(PerftTaskResponse[]))]
[JsonSerializable(typeof(PerftTaskResultBatch))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext;
