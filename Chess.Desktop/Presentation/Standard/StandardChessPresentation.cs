// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Globalization;
using Chess.Core.Games.Status;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Desktop.Presentation.Standard;

public sealed class StandardChessPresentation : IGamePresentation
{
    private const double SquareSize = 72;

    private const string PieceAssetRoot =
        "/Chess.Desktop;component/Assets/Variants/Standard/Pieces/sepia";

    private readonly IReadOnlyDictionary<Side, string> _sideNames =
        new Dictionary<Side, string>
        {
            [SideDefinitions.White] = "White",
            [SideDefinitions.Black] = "Black"
        };

    private readonly
        IReadOnlyDictionary<(Side Side, PieceDefinitionId DefinitionId), string>
        _pieceAssets =
            new Dictionary<(Side Side, PieceDefinitionId DefinitionId), string>
            {
                [(SideDefinitions.White, PieceDefinitions.King.Id)] =
                    $"{PieceAssetRoot}/white-king.png",
                [(SideDefinitions.White, PieceDefinitions.Queen.Id)] =
                    $"{PieceAssetRoot}/white-queen.png",
                [(SideDefinitions.White, PieceDefinitions.Rook.Id)] =
                    $"{PieceAssetRoot}/white-rook.png",
                [(SideDefinitions.White, PieceDefinitions.Bishop.Id)] =
                    $"{PieceAssetRoot}/white-bishop.png",
                [(SideDefinitions.White, PieceDefinitions.Knight.Id)] =
                    $"{PieceAssetRoot}/white-knight.png",
                [(SideDefinitions.White, PieceDefinitions.Pawn.Id)] =
                    $"{PieceAssetRoot}/white-pawn.png",
                [(SideDefinitions.Black, PieceDefinitions.King.Id)] =
                    $"{PieceAssetRoot}/black-king.png",
                [(SideDefinitions.Black, PieceDefinitions.Queen.Id)] =
                    $"{PieceAssetRoot}/black-queen.png",
                [(SideDefinitions.Black, PieceDefinitions.Rook.Id)] =
                    $"{PieceAssetRoot}/black-rook.png",
                [(SideDefinitions.Black, PieceDefinitions.Bishop.Id)] =
                    $"{PieceAssetRoot}/black-bishop.png",
                [(SideDefinitions.Black, PieceDefinitions.Knight.Id)] =
                    $"{PieceAssetRoot}/black-knight.png",
                [(SideDefinitions.Black, PieceDefinitions.Pawn.Id)] =
                    $"{PieceAssetRoot}/black-pawn.png"
            };

    public BoardPresentation Board { get; } = CreateBoard();

    public string GetSideName(
        Side side)
    {
        ArgumentNullException.ThrowIfNull(side);

        return _sideNames.TryGetValue(side, out var name)
            ? name
            : FormatIdentifier(side.Id);
    }

    public string GetStatusName(
        GameStatusId statusId)
    {
        ArgumentNullException.ThrowIfNull(statusId);

        return FormatIdentifier(statusId.Value);
    }

    public string GetPieceImageSource(
        Piece piece)
    {
        ArgumentNullException.ThrowIfNull(piece);

        if (_pieceAssets.TryGetValue(
                (piece.Side, piece.Definition.Id),
                out var source))
        {
            return source;
        }

        throw new InvalidOperationException(
            $"No presentation asset is registered for piece " +
            $"'{piece.Definition.Id}' on side '{piece.Side.Id}'.");
    }

    private static BoardPresentation CreateBoard()
    {
        var squares = new List<BoardSquarePresentation>();

        for (var row = 0; row < BoardLayout.Rows; row++)
        {
            for (var column = 0; column < BoardLayout.Columns; column++)
            {
                squares.Add(
                    new BoardSquarePresentation(
                        BoardLayout.GetSquare(row, column),
                        column * SquareSize,
                        row * SquareSize,
                        (row + column) % 2 == 0));
            }
        }

        return new BoardPresentation(
            $"{BoardLayout.Columns}×{BoardLayout.Rows}",
            SquareSize,
            squares);
    }

    private static string FormatIdentifier(
        string value)
    {
        var separatorIndex = value.LastIndexOf(':');

        var identifier = separatorIndex >= 0
            ? value[(separatorIndex + 1)..]
            : value;

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(identifier);
    }
}
