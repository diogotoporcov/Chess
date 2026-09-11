using Chess.Core.Board;

namespace Chess.Core.Movement;

public readonly record struct DisplacementComponent(
    Direction Direction,
    int Distance);