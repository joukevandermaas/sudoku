using System;
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

        public override int GetHashCode()
        {
            return HashCode.Combine(_cells);
        }
    }
}
