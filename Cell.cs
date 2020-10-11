using System;

namespace Sudoku
{
    internal readonly struct Cell : IEquatable<Cell>
    {
        public SudokuValues Value { get; }

        public int Index { get; }

        public int Row => Index / Puzzle.LineLength;
        public int Column => Index % Puzzle.LineLength;
        public int Box => ((Row / 3) * Puzzle.BoxLength) + (Column / 3);

        public bool IsResolved => Value.IsSingle;
        public bool IsValid => Value != SudokuValues.None;

        public Cell RemoveOptions(SudokuValues options)
        {
            if (IsResolved) throw new InvalidOperationException("Trying to remove possibility from resolved cell");

            return new Cell(Value.RemoveOptions(options), Index);
        }

        public Cell SetValue(SudokuValues value)
        {
#if DEBUG
            if (IsResolved) throw new InvalidOperationException("Trying to set value of resolved cell");
#endif

            return new Cell(value, Index);
        }

        public bool HasOptions(SudokuValues options) => !IsResolved && Value.HasAnyOptions(options);

        public Cell(SudokuValues value, int index)
        {
            Value = value;
            Index = index;
        }

        public static bool operator ==(Cell left, Cell right) => left.Equals(right);
        public static bool operator !=(Cell left, Cell right) => !left.Equals(right);

        public override bool Equals(object? obj) => obj is Cell other && Equals(other);

        public bool Equals(Cell other) => other.Value == Value && other.Index == Index;

        public override string ToString() => IsResolved ? Value.ToString() : ".";

        public override int GetHashCode() => HashCode.Combine(Value, Index);
    }
}
