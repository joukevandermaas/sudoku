using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sudoku
{
    class Program
    {
        static void Main(string[] args)
        {
            // left to right, top to bottom. 0 means empty
            SolveSudoku("340600000007000000020080570000005000070010020000400000036020010000000900000007082");
        }

        static void SolveSudoku(string sudoku)
        {
            var puzzle = new Puzzle(sudoku);

            Console.WriteLine(puzzle);
        }
    }

    struct Puzzle : IEquatable<Puzzle>
    {
        public const int Size = 9 * 9;

        private Cell[] _cells { get; }

        public Puzzle(string puzzle)
        {
            _cells = new Cell[Size];

            for (var i = 0; i < Size; i++)
            {
                var digit = (int)(puzzle[i] - 48); // 48 is 0 in ascii
                var cell = digit == default ? default : new Cell(1 << digit);

                _cells[i] = cell;
            }
        }

        public bool Equals(Puzzle other)
        {
            for (int i = 0; i < Size; i++)
            {
                if (_cells[i] != other._cells[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            // only works for sudokus where the size is a cube
            // │ ┤ ╡ ╢ ╖ ╕ ╣ ║ ╗ ╝ ╜ ╛ ┐ └ ┴ ┬ ├ ─ ┼ ╞ ╟ ╚ ╔ ╩ ╦ ╠ ═ ╬ ╧ ╨ ╤ ╥ ╙ ╘ ╒ ╓ ╫ ╪ ┘ ┌

            var builder = new StringBuilder();

            var lineLength = (int)Math.Sqrt(Size);
            var boxLength = (int)Math.Sqrt(lineLength);

            builder.Append("╔");
            builder.Append(string.Join('╦',
                Enumerable.Repeat("═══╤═══╤═══", boxLength))); // todo this only works for 3x3
            builder.AppendLine("╗");
            builder.Append("║");

            for (var i = 0; i < Size; i++)
            {
                if (i != 0)
                {
                    if (i % (boxLength * lineLength) == 0)
                    {
                        builder.AppendLine("║");
                        builder.Append("╠");
                        builder.Append(string.Join('╬',
                            Enumerable.Repeat("═══╪═══╪═══", boxLength))); // todo this only works for 3x3
                        builder.AppendLine("╣");
                    }
                    else if (i % lineLength == 0)
                    {
                        builder.AppendLine("║");
                        builder.Append("╟");
                        builder.Append(string.Join('╫',
                            Enumerable.Repeat("───┼───┼───", boxLength))); // todo this only works for 3x3
                        builder.AppendLine("╢");
                    }

                    if (i % boxLength == 0)
                    {
                        builder.Append("║");
                    }
                    else
                    {
                        builder.Append("│");
                    }
                }

                builder.AppendFormat(" {0} ", _cells[i]);
            }

            builder.AppendLine("║");
            builder.Append("╚");
            builder.Append(string.Join('╩',
                Enumerable.Repeat("═══╧═══╧═══", boxLength))); // todo this only works for 3x3
            builder.AppendLine("╝");

            return builder.ToString();
        }
    }

    struct Cell : IEquatable<Cell>
    {
        // powers of two, e.g. the digit 3 is represented as 2^3
        public int Value { get; }

        public bool IsResolved => (Value & (Value - 1)) == 0 && Value != 0;

        public Cell RemovePossibleValues(int possibleValues)
        {
            return new Cell(Value & (~possibleValues));
        }

        public Cell(int value)
        {
            Value = value;
        }

        public static bool operator ==(Cell left, Cell right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Cell left, Cell right)
        {
            return !left.Equals(right);
        }

        public bool Equals(Cell other)
        {
            return other.Value == Value;
        }

        public override string ToString()
        {
            return IsResolved ? FriendlyNumber(Value).ToString() : " ";

            static int FriendlyNumber(int val)
            {
                return (int)Math.Log2(val);
            }
        }
    }
}
