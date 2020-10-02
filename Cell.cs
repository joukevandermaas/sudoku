using System;

namespace Sudoku
{
    struct Cell : IEquatable<Cell>
    {
        public SudokuValues Value { get; }

        public int Index { get; }

        public int Row => Index / Puzzle.LineLength;
        public int Column => Index % Puzzle.LineLength;

        public bool IsResolved => Value.IsSingle;
        public bool IsValid => Value != SudokuValues.None;

        public Cell RemoveOptions(SudokuValues options)
        {
            if (IsResolved) throw new InvalidOperationException("Trying to remove possibility from resolved cell");

            return new Cell(Value.RemoveOptions(options), Index);
        }

        public Cell SetValue(SudokuValues value)
        {
            if (IsResolved) throw new InvalidOperationException("Trying to set value of resolved cell");

            return new Cell(value, Index);
        }

        public Cell(SudokuValues value, int index)
        {
            Value = value;
            Index = index;
        }

        public static bool operator ==(Cell left, Cell right) => left.Equals(right);
        public static bool operator !=(Cell left, Cell right) => !left.Equals(right);

        public override bool Equals(object? obj) => obj is Cell other && Equals(other);

        public bool Equals(Cell other) => other.Value == Value && other.Index == Index;

        public override string ToString() => ToString(false);

        public string ToString(bool full)
        {
            var empty = full ? " " : ".";

            return IsResolved ? Value.ToString() : empty;
        }

        public override int GetHashCode() => HashCode.Combine(Value, Index);
    }
}
