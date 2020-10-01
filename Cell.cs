using System;

namespace Sudoku
{
    struct Cell : IEquatable<Cell>
    {
        public const int Mask = 0x1FF; // only first 9 bits, we don't care about the rest

        // powers of two, e.g. the digit 3 is represented as 2^3
        public int Value { get; }

        public int Row { get; }
        public int Column { get; }

        public bool IsResolved => (Value & (Value - 1)) == 0 && !IsInvalid;
        public bool IsInvalid => Value == 0;

        public Cell RemovePossibleValues(int possibleValues)
        {
            if (IsResolved) throw new InvalidOperationException("Trying to remove possibility from resolved cell");

            return new Cell(Value & (~possibleValues), Row, Column);
        }

        public Cell(int value, int row, int column)
        {
            Value = value & Mask; // discards anything besides the first 9 bits
            Row = row;
            Column = column;
        }

        public static bool operator ==(Cell left, Cell right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Cell left, Cell right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object? obj)
        {
            return obj is Cell other && Equals(other);
        }

        public bool Equals(Cell other)
        {
            return other.Value == Value;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool full)
        {
            var empty = full ? " " : ".";

            return IsResolved ? FriendlyNumber(Value).ToString() : empty;

            static int FriendlyNumber(int val)
            {
                return (int)Math.Log2(val) + 1;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
}
