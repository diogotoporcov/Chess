// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.ObjectModel;
using Chess.Core.Board;
using Chess.Core.Games.Variants;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Games;

public sealed class StandardInitialState
{
    private const int MaximumPiecesPerSide = 16;
    private const int MaximumPawnsPerSide = 8;

    private readonly ReadOnlyCollection<InitialPiecePlacement> _placementsView;

    public static StandardInitialState Default { get; } = CreateDefault();

    public IReadOnlyList<InitialPiecePlacement> Placements => _placementsView;

    public Side SideToMove { get; }

    public CastlingRights CastlingRights { get; }

    public Square? EnPassantTarget { get; }

    public int HalfmoveClock { get; }

    public int FullmoveNumber { get; }

    public StandardInitialState(
        IEnumerable<InitialPiecePlacement> placements,
        Side sideToMove,
        CastlingRights castlingRights,
        Square? enPassantTarget,
        int halfmoveClock,
        int fullmoveNumber)
    {
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(sideToMove);

        var copiedPlacements = placements.ToArray();

        ValidatePlacements(copiedPlacements);
        ValidateSideToMove(sideToMove);

        ArgumentOutOfRangeException.ThrowIfNegative(halfmoveClock);

        if (fullmoveNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fullmoveNumber),
                "Fullmove number must be at least one.");
        }

        ValidateCastlingRights(copiedPlacements, castlingRights);
        ValidateEnPassantTarget(
            copiedPlacements,
            sideToMove,
            enPassantTarget,
            halfmoveClock);

        var normalizedPlacements = copiedPlacements
            .Select(placement => new InitialPiecePlacement(
                placement.Square,
                GetCanonicalSide(placement.Side),
                placement.Definition))
            .ToArray();
        _placementsView = Array.AsReadOnly(normalizedPlacements);
        SideToMove = GetCanonicalSide(sideToMove);
        CastlingRights = castlingRights;
        EnPassantTarget = enPassantTarget;
        HalfmoveClock = halfmoveClock;
        FullmoveNumber = fullmoveNumber;
    }

    private static void ValidatePlacements(
        InitialPiecePlacement[] placements)
    {
        if (Array.IndexOf(placements, null!) >= 0)
        {
            throw new ArgumentException(
                "Placements cannot contain null elements.",
                nameof(placements));
        }

        if (placements
            .GroupBy(placement => placement.Square)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Placements cannot contain multiple pieces on one square.",
                nameof(placements));
        }

        foreach (var placement in placements)
        {
            if (placement.Square.Id is < 0 or >= BoardGeometry.SideDimension *
                                                 BoardGeometry.SideDimension)
            {
                throw new ArgumentException(
                    "A placement references a square outside the standard board.",
                    nameof(placements));
            }

            if (!IsStandardSide(placement.Side))
            {
                throw new ArgumentException(
                    "A placement uses an unsupported side.",
                    nameof(placements));
            }

            if (!IsCanonicalPieceDefinition(placement.Definition))
            {
                throw new ArgumentException(
                    "A placement uses an unsupported piece definition.",
                    nameof(placements));
            }

            if (ReferenceEquals(placement.Definition, PieceDefinitions.Pawn) &&
                BoardLayout.GetRow(placement.Square) is 0 or 7)
            {
                throw new ArgumentException(
                    "Pawns cannot occupy the first or eighth rank.",
                    nameof(placements));
            }
        }

        ValidateSideMaterial(placements, SideDefinitions.White);
        ValidateSideMaterial(placements, SideDefinitions.Black);
    }

    private static void ValidateSideMaterial(
        IReadOnlyList<InitialPiecePlacement> placements,
        Side side)
    {
        var sidePlacements = placements
            .Where(placement => placement.Side == side)
            .ToArray();
        var kingCount = sidePlacements.Count(placement =>
            ReferenceEquals(placement.Definition, PieceDefinitions.King));
        var pawnCount = sidePlacements.Count(placement =>
            ReferenceEquals(placement.Definition, PieceDefinitions.Pawn));

        if (kingCount != 1)
        {
            throw new ArgumentException(
                "Each standard side must have exactly one king.",
                nameof(placements));
        }

        if (sidePlacements.Length > MaximumPiecesPerSide)
        {
            throw new ArgumentException(
                "A standard side cannot have more than 16 pieces.",
                nameof(placements));
        }

        if (pawnCount > MaximumPawnsPerSide)
        {
            throw new ArgumentException(
                "A standard side cannot have more than eight pawns.",
                nameof(placements));
        }
    }

    private static void ValidateSideToMove(
        Side sideToMove)
    {
        if (!IsStandardSide(sideToMove))
        {
            throw new ArgumentException(
                "Side to move must be White or Black.",
                nameof(sideToMove));
        }
    }

    private static void ValidateCastlingRights(
        IReadOnlyList<InitialPiecePlacement> placements,
        CastlingRights rights)
    {
        ValidateCastlingRight(
            placements,
            rights.WhiteKingSide,
            SideDefinitions.White,
            "e1",
            "h1");
        ValidateCastlingRight(
            placements,
            rights.WhiteQueenSide,
            SideDefinitions.White,
            "e1",
            "a1");
        ValidateCastlingRight(
            placements,
            rights.BlackKingSide,
            SideDefinitions.Black,
            "e8",
            "h8");
        ValidateCastlingRight(
            placements,
            rights.BlackQueenSide,
            SideDefinitions.Black,
            "e8",
            "a8");
    }

    private static void ValidateCastlingRight(
        IReadOnlyList<InitialPiecePlacement> placements,
        bool hasRight,
        Side side,
        string kingCoordinate,
        string rookCoordinate)
    {
        if (!hasRight)
        {
            return;
        }

        if (!HasPiece(
                placements,
                GetSquare(kingCoordinate),
                side,
                PieceDefinitions.King) ||
            !HasPiece(
                placements,
                GetSquare(rookCoordinate),
                side,
                PieceDefinitions.Rook))
        {
            throw new ArgumentException(
                "Castling rights require the canonical king and rook on their home squares.",
                nameof(placements));
        }
    }

    private static void ValidateEnPassantTarget(
        IReadOnlyList<InitialPiecePlacement> placements,
        Side sideToMove,
        Square? enPassantTarget,
        int halfmoveClock)
    {
        if (enPassantTarget is not { } target)
        {
            return;
        }

        if (target.Id is < 0 or >= BoardGeometry.SideDimension *
                                   BoardGeometry.SideDimension)
        {
            throw new ArgumentException(
                "En passant target must be on the standard board.",
                nameof(enPassantTarget));
        }

        if (placements.Any(placement => placement.Square == target))
        {
            throw new ArgumentException(
                "En passant target square must be empty.",
                nameof(enPassantTarget));
        }

        var targetRow = BoardLayout.GetRow(target);
        var targetColumn = BoardLayout.GetColumn(target);
        var isWhiteToMove = sideToMove == SideDefinitions.White;
        var expectedTargetRow = isWhiteToMove ? 2 : 5;
        var pawnRow = isWhiteToMove ? 3 : 4;
        var startingRow = isWhiteToMove ? 1 : 6;
        var pawnSide = isWhiteToMove
            ? SideDefinitions.Black
            : SideDefinitions.White;

        if (targetRow != expectedTargetRow ||
            !HasPiece(
                placements,
                BoardGeometry.SquareAt(pawnRow, targetColumn),
                pawnSide,
                PieceDefinitions.Pawn) ||
            placements.Any(placement =>
                placement.Square ==
                BoardGeometry.SquareAt(startingRow, targetColumn)))
        {
            throw new ArgumentException(
                "En passant target is not compatible with the preceding double pawn advance.",
                nameof(enPassantTarget));
        }

        if (halfmoveClock != 0)
        {
            throw new ArgumentException(
                "Halfmove clock must be zero when an en passant target exists.",
                nameof(halfmoveClock));
        }
    }

    private static bool HasPiece(
        IEnumerable<InitialPiecePlacement> placements,
        Square square,
        Side side,
        PieceDefinition definition)
    {
        return placements.Any(placement =>
            placement.Square == square &&
            placement.Side == side &&
            ReferenceEquals(placement.Definition, definition));
    }

    private static bool IsStandardSide(
        Side side)
    {
        return side == SideDefinitions.White || side == SideDefinitions.Black;
    }

    private static Side GetCanonicalSide(
        Side side)
    {
        return side == SideDefinitions.White
            ? SideDefinitions.White
            : SideDefinitions.Black;
    }

    private static bool IsCanonicalPieceDefinition(
        PieceDefinition definition)
    {
        return ReferenceEquals(definition, PieceDefinitions.Pawn) ||
               ReferenceEquals(definition, PieceDefinitions.Knight) ||
               ReferenceEquals(definition, PieceDefinitions.Bishop) ||
               ReferenceEquals(definition, PieceDefinitions.Rook) ||
               ReferenceEquals(definition, PieceDefinitions.Queen) ||
               ReferenceEquals(definition, PieceDefinitions.King);
    }

    private static StandardInitialState CreateDefault()
    {
        return new StandardInitialState(
            CreateDefaultPlacements(),
            SideDefinitions.White,
            new CastlingRights(true, true, true, true),
            null,
            0,
            1);
    }

    private static InitialPiecePlacement[] CreateDefaultPlacements()
    {
        var placements = new List<InitialPiecePlacement>();

        AddPawns(placements, SideDefinitions.Black, 1);
        AddPawns(placements, SideDefinitions.White, 6);
        AddBackRank(placements, SideDefinitions.Black, 0);
        AddBackRank(placements, SideDefinitions.White, 7);

        return [.. placements];
    }

    private static void AddPawns(
        ICollection<InitialPiecePlacement> placements,
        Side side,
        int row)
    {
        for (var column = 0; column < BoardGeometry.SideDimension; column++)
        {
            placements.Add(
                new InitialPiecePlacement(
                    BoardGeometry.SquareAt(row, column),
                    side,
                    PieceDefinitions.Pawn));
        }
    }

    private static void AddBackRank(
        ICollection<InitialPiecePlacement> placements,
        Side side,
        int row)
    {
        PieceDefinition[] definitions =
        [
            PieceDefinitions.Rook, PieceDefinitions.Knight,
            PieceDefinitions.Bishop, PieceDefinitions.Queen,
            PieceDefinitions.King, PieceDefinitions.Bishop,
            PieceDefinitions.Knight, PieceDefinitions.Rook
        ];

        for (var column = 0; column < definitions.Length; column++)
        {
            placements.Add(
                new InitialPiecePlacement(
                    BoardGeometry.SquareAt(row, column),
                    side,
                    definitions[column]));
        }
    }

    private static Square GetSquare(
        string coordinate)
    {
        var column = coordinate[0] - 'a';
        var row = '8' - coordinate[1];

        return BoardGeometry.SquareAt(row, column);
    }
}
