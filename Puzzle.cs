﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace Sudoku
{
    struct Puzzle : IEquatable<Puzzle>
    {
        public const int LineLength = 9;
        public const int BoxLength = 3;

        private readonly Cell[] _cells;

        public Puzzle(string puzzle)
        {
            _cells = new Cell[LineLength * LineLength];

            for (var i = 0; i < _cells.Length; i++)
            {
                var cell = new Cell(SudokuValues.FromCharacter(puzzle[i]), i);
                _cells[i] = cell;
            }
        }

        private Puzzle(Cell[] cells)
        {
            _cells = cells;
        }

        public bool IsSolved => _cells.All(c => c.IsResolved);

        public bool IsValid
        {
            get
            {
                var allCellsValid = _cells.All(c => c.IsValid);

                if (!allCellsValid)
                {
                    return false;
                }

                return GetRows().Concat(GetColumns()).Concat(GetBoxes()).All(c => c.IsValid);
            }
        }

        public IEnumerable<Cell> Cells => _cells;


        public Puzzle UpdateCell(Cell newValue)
        {
            var newCells = new Cell[LineLength * LineLength];
            Array.Copy(_cells, newCells, _cells.Length);

            newCells[newValue.Index] = newValue;

            return new Puzzle(newCells);
        }

        public Puzzle UpdateCells(IEnumerable<Cell> newValues)
        {
            if (!newValues.Any())
                return this;

            var newCells = new Cell[LineLength * LineLength];
            Array.Copy(_cells, newCells, _cells.Length);

            foreach (var cell in newValues)
            {
                newCells[cell.Index] = cell;
            }

            return new Puzzle(newCells);
        }

        public IEnumerable<Region> GetRows()
        {
            for (int i = 0; i < LineLength; i++)
            {
                yield return new Region(_cells, RegionType.Row, i);
            }
        }

        public IEnumerable<Region> GetColumns()
        {
            for (int i = 0; i < LineLength; i++)
            {
                yield return new Region(_cells, RegionType.Column, i);
            }
        }

        public IEnumerable<Region> GetBoxes()
        {
            for (int i = 0; i < LineLength; i++)
            {
                yield return new Region(_cells, RegionType.Box, i);
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Puzzle puzzle && Equals(puzzle);
        }

        public bool Equals(Puzzle other)
        {
            for (int i = 0; i < _cells.Length; i++)
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

            const bool printIndices = true;
            const bool printPossibilities = false;

            if (printIndices)
            {
                builder.Append(" ");
                for (int i = 0; i < LineLength; i++)
                {
                    builder.AppendFormat(" {0}  ", i);
                }

                builder.AppendLine();
            }

            builder.Append("╔");
            builder.Append(string.Join('╦',
                Enumerable.Repeat("═══╤═══╤═══", BoxLength))); // todo this only works for 3x3
            builder.AppendLine("╗");
            builder.Append("║");

            var rowNr = 0;

            for (var i = 0; i < _cells.Length; i++)
            {
                if (i != 0)
                {
                    if (i % (BoxLength * LineLength) == 0)
                    {
                        AddEndOfLine();

                        builder.Append("╠");
                        builder.Append(string.Join('╬',
                            Enumerable.Repeat("═══╪═══╪═══", BoxLength))); // todo this only works for 3x3
                        builder.AppendLine("╣");
                    }
                    else if (i % LineLength == 0)
                    {
                        AddEndOfLine();

                        builder.Append("╟");
                        builder.Append(string.Join('╫',
                            Enumerable.Repeat("───┼───┼───", BoxLength))); // todo this only works for 3x3
                        builder.AppendLine("╢");
                    }

                    if (i % BoxLength == 0)
                    {
                        builder.Append("║");
                    }
                    else
                    {
                        builder.Append("│");
                    }
                }

                builder.AppendFormat(" {0} ", _cells[i].ToString(full: true));
            }

            AddEndOfLine();

            builder.Append("╚");
            builder.Append(string.Join('╩',
                Enumerable.Repeat("═══╧═══╧═══", BoxLength))); // todo this only works for 3x3
            builder.AppendLine("╝");

            if (printPossibilities)
            {
                foreach (var cell in _cells)
                {
                    builder.AppendFormat("R{0}C{1}: {2}", cell.Row, cell.Column, string.Join("", cell.Value.ToHumanOptions()));
                    builder.AppendLine();
                }
            }

            return builder.ToString();

            void AddEndOfLine()
            {
                builder.Append("║");

                if (printIndices)
                {
                    builder.AppendFormat(" {0}", rowNr);
                }

                builder.AppendLine();

                rowNr += 1;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_cells);
        }
    }
}
