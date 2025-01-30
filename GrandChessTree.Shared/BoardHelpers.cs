using System.Runtime.CompilerServices;
using GrandChessTree.Shared.Helpers;
using GrandChessTree.Shared.Precomputed;

namespace GrandChessTree.Shared;


public partial struct Board
{
       [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool IsAttackedByWhite(int index)
    {
        return (AttackTables.PextBishopAttacks(Occupancy, index) &
                (WhiteBishop | WhiteQueen)) != 0 ||
               (AttackTables.PextRookAttacks(Occupancy, index) & (WhiteRook | WhiteQueen)) !=
               0 ||
               (*(AttackTables.KnightAttackTable + index) & WhiteKnight) != 0 ||
               (*(AttackTables.BlackPawnAttackTable + index) & WhitePawn) != 0 ||
               (*(AttackTables.KingAttackTable + index) & (1ul << WhiteKingPos)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool IsAttackedByBlack(int index)
    {
        return (AttackTables.PextBishopAttacks(Occupancy, index) &
                (BlackBishop | BlackQueen)) != 0 ||
               (AttackTables.PextRookAttacks(Occupancy, index) & (BlackRook | BlackQueen)) !=
               0 ||
               (*(AttackTables.KnightAttackTable + index) & BlackKnight) != 0 ||
               (*(AttackTables.WhitePawnAttackTable + index) & BlackPawn) != 0 ||
               (*(AttackTables.KingAttackTable + index) & (1ul << BlackKingPos)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ulong WhiteCheckers()
    {
        return (AttackTables.PextBishopAttacks(Occupancy, BlackKingPos) &  (WhiteBishop | WhiteQueen)) |
               (AttackTables.PextRookAttacks(Occupancy, BlackKingPos) & (WhiteRook | WhiteQueen)) |
               (*(AttackTables.KnightAttackTable + BlackKingPos) & WhiteKnight) |
               (*(AttackTables.BlackPawnAttackTable + BlackKingPos) & WhitePawn);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ulong BlackCheckers()
    {
        return (AttackTables.PextBishopAttacks(Occupancy, WhiteKingPos) &
                (BlackBishop | BlackQueen)) |
               (AttackTables.PextRookAttacks(Occupancy, WhiteKingPos) & (BlackRook | BlackQueen)) |
               (*(AttackTables.KnightAttackTable + WhiteKingPos) & BlackKnight) |
               (*(AttackTables.WhitePawnAttackTable + WhiteKingPos) & BlackPawn);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ulong WhiteKingDangerSquares()
    {
        var attackers = 0ul;

        var occupancy = Occupancy ^ (1ul << WhiteKingPos);

        var positions = BlackPawn;
        while (positions != 0) attackers |= *(AttackTables.BlackPawnAttackTable + positions.PopLSB());

        positions = BlackKnight;
        while (positions != 0) attackers |= *(AttackTables.KnightAttackTable + positions.PopLSB());

        positions = BlackBishop | BlackQueen;
        while (positions != 0) attackers |= AttackTables.PextBishopAttacks(occupancy, positions.PopLSB());

        positions = BlackRook | BlackQueen;
        while (positions != 0) attackers |= AttackTables.PextRookAttacks(occupancy, positions.PopLSB());

        attackers |= *(AttackTables.KingAttackTable + BlackKingPos);

        return attackers;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ulong BlackKingDangerSquares()
    {
        var attackers = 0ul;

        var occupancy = Occupancy ^ (1ul << BlackKingPos);

        var positions = WhitePawn;
        while (positions != 0) attackers |= *(AttackTables.WhitePawnAttackTable + positions.PopLSB());

        positions = WhiteKnight;
        while (positions != 0) attackers |= *(AttackTables.KnightAttackTable + positions.PopLSB());

        positions = WhiteBishop | WhiteQueen;
        while (positions != 0) attackers |= AttackTables.PextBishopAttacks(occupancy, positions.PopLSB());

        positions = WhiteRook | WhiteQueen;
        while (positions != 0) attackers |= AttackTables.PextRookAttacks(occupancy, positions.PopLSB());

        attackers |= *(AttackTables.KingAttackTable + WhiteKingPos);

        return attackers;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong WhiteKingPinnedRay()
    {
        return AttackTables.DetectPinsDiagonal(WhiteKingPos, (BlackBishop | BlackQueen), Occupancy) | 
               AttackTables.DetectPinsStraight(WhiteKingPos, (BlackRook | BlackQueen), Occupancy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong BlackKingPinnedRay()
    {
        return AttackTables.DetectPinsDiagonal(BlackKingPos, (WhiteBishop | WhiteQueen), Occupancy) | 
               AttackTables.DetectPinsStraight(BlackKingPos, (WhiteRook | WhiteQueen), Occupancy);
    }

}