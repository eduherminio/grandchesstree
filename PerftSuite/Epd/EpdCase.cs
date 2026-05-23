namespace PerftSuite.Epd;

/// <summary>One (FEN, depth, expected node count) row from an EPD file.</summary>
public sealed record EpdCase(
    string Fen,
    int    Depth,
    ulong  Expected,
    string SourceFile,
    int    SourceLine);
