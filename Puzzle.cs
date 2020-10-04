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

        public Puzzle(Cell[] cells)
        {
            _cells = cells;
        }

        public bool IsSolved => _cells.All(c => c.IsResolved);

        public Cell this[int i] => _cells[i];
        public Cell this[int row, int col] => _cells[row * LineLength + col];

        public bool IsValid
        {
            get
            {
                var allCellsValid = _cells.All(c => c.IsValid);

                if (!allCellsValid)
                {
                    return false;
                }

                return Rows.Concat(Columns).Concat(Boxes).All(c => c.IsValid);
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

        public IEnumerable<Region> Regions => Rows.Concat(Columns).Concat(Boxes);

        public Region GetRow(int index) => new Region(_cells, RegionType.Row, index);
        public Region GetColumn(int index) => new Region(_cells, RegionType.Column, index);
        public Region GetBox(int index) => new Region(_cells, RegionType.Box, index);

        public IEnumerable<Region> Rows
        {
            get
            {
                for (int i = 0; i < LineLength; i++)
                {
                    yield return GetRow(i);
                }
            }
        }

        public IEnumerable<Region> Columns
        {
            get
            {
                for (int i = 0; i < LineLength; i++)
                {
                    yield return GetColumn(i);
                }
            }
        }

        public IEnumerable<Region> Boxes
        {
            get
            {
                for (int i = 0; i < LineLength; i++)
                {
                    yield return GetBox(i);
                }
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
