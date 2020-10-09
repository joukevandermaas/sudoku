using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    struct Puzzle : IEquatable<Puzzle>
    {
        public const int LineLength = 9;
        public const int BoxLength = 3;

        private readonly Cell[] _cells;
        private List<Region>? _rows;
        private List<Region>? _columns;
        private List<Region>? _boxes;

        public Puzzle(string puzzle)
        {
            _cells = new Cell[LineLength * LineLength];

            for (var i = 0; i < _cells.Length; i++)
            {
                var cell = new Cell(SudokuValues.FromCharacter(puzzle[i]), i);
                _cells[i] = cell;
            }

            _rows = null;
            _columns = null;
            _boxes = null;
        }

        public Puzzle(Cell[] cells)
        {
            _cells = cells;
            _rows = null;
            _columns = null;
            _boxes = null;
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
            var newCells = new Cell[LineLength * LineLength];
            Array.Copy(_cells, newCells, _cells.Length);

            foreach (var cell in newValues)
            {
                newCells[cell.Index] = cell;
            }

            return new Puzzle(newCells);
        }

        public IEnumerable<Region> Regions => Rows.Concat(Columns).Concat(Boxes);

        public Region GetRow(int index) => GetRegion(RegionType.Row, index);
        public Region GetColumn(int index) => GetRegion(RegionType.Column, index);
        public Region GetBox(int index) => GetRegion(RegionType.Box, index);

        public Region GetRegion(RegionType type, int index) => new Region(_cells, type, index);
        public IEnumerable<Region> GetRegions(RegionType type) => type switch
        {
            RegionType.Row => Rows,
            RegionType.Column => Columns,
            RegionType.Box => Boxes,
            _ => throw new NotImplementedException(),
        };

        public IEnumerable<Region> Rows
        {
            get
            {
                if (_rows != null)
                {
                    return _rows;
                }

                _rows = new List<Region>(LineLength);
                for (int i = 0; i < LineLength; i++)
                {
                    _rows.Add(GetRow(i));
                }

                return _rows;
            }
        }

        public IEnumerable<Region> Columns
        {
            get
            {
                if (_columns != null)
                {
                    return _columns;
                }

                _columns = new List<Region>(LineLength);
                for (int i = 0; i < LineLength; i++)
                {
                    _columns.Add(GetColumn(i));
                }

                return _columns;
            }
        }

        public IEnumerable<Region> Boxes
        {
            get
            {
                if (_boxes != null)
                {
                    return _boxes;
                }

                _boxes = new List<Region>(LineLength);
                for (int i = 0; i < LineLength; i++)
                {
                    _boxes.Add(GetBox(i));
                }

                return _boxes;
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
