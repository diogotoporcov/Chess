using Chess.Core.Board.Topology;
using Chess.Core.Pieces;

namespace Chess.Core.Board;

public sealed class BoardStateBuilder
{
    private readonly Dictionary<Square, Piece> _pieces = [];

    private readonly HashSet<Piece> _placedPieces = new(ReferenceEqualityComparer.Instance);

    public BoardTopology Topology { get; }

    public BoardStateBuilder(
        BoardTopology topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        Topology = topology;
    }

    public BoardStateBuilder PlacePiece(
        Square square,
        Piece piece)
    {
        ArgumentNullException.ThrowIfNull(piece);

        if (!Topology.Contains(square))
        {
            throw new ArgumentException(
                "Square is not part of the board.",
                nameof(square));
        }

        if (_pieces.ContainsKey(square))
        {
            throw new InvalidOperationException(
                "Square is already occupied.");
        }

        if (!_placedPieces.Add(piece))
        {
            throw new InvalidOperationException(
                "Piece is already placed on the board.");
        }

        _pieces.Add(
            square,
            piece);

        return this;
    }

    public BoardState Build()
    {
        return new BoardState(
            Topology,
            _pieces);
    }
}