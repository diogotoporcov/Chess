using Chess.Core.Board;

namespace Chess.Core.Movement;

public readonly record struct Move(
    Square From,
    Square To);