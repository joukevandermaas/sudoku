using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sudoku
{
    internal readonly struct Puzzle : IEquatable<Puzzle>
    {
        public const int LineLength = 9;
        public const int BoxLength = 3;

        private readonly Cell[] _cells;
        private readonly Region[] _rows;
        private readonly Region[] _columns;
        private readonly Region[] _boxes;

        public ReadOnlyCollection<Region> Rows => Array.AsReadOnly(_rows);
        public ReadOnlyCollection<Region> Columns => Array.AsReadOnly(_columns);
        public ReadOnlyCollection<Region> Boxes => Array.AsReadOnly(_boxes);

        public static Puzzle FromString(string puzzle)
        {
            var cells = new Cell[LineLength * LineLength];

            for (var i = 0; i < cells.Length; i++)
            {
                var cell = new Cell(SudokuValues.FromCharacter(puzzle[i]), i);
                cells[i] = cell;
            }

            return new Puzzle(cells);
        }

        public Puzzle(Cell[] cells)
        {
            _cells = cells;

            _rows = new Region[LineLength];
            _columns = new Region[LineLength];
            _boxes = new Region[LineLength];

            for (int i = 0; i < LineLength; i++)
            {
                _rows[i] = new Region(_cells, RegionType.Row, i);
                _columns[i] = new Region(_cells, RegionType.Column, i);
                _boxes[i] = new Region(_cells, RegionType.Box, i);
            }
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

        public ReadOnlyCollection<Cell> Cells => Array.AsReadOnly(_cells);

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

        public IEnumerable<Region> Regions
        {
            get
            {
                for (int i = 0; i < 3 * LineLength; i++)
                {
                    var index = i % LineLength;
                    var type = (RegionType)((i / LineLength) + 1);

                    yield return GetRegion(type, index);
                }
            }
        }

        public Region GetRow(int index) => GetRegion(RegionType.Row, index);
        public Region GetColumn(int index) => GetRegion(RegionType.Column, index);
        public Region GetBox(int index) => GetRegion(RegionType.Box, index);

        public Region GetRegion(RegionType type, int index) => new Region(_cells, type, index);

        public ReadOnlyCollection<Region> GetRegions(RegionType type) => type switch
        {
            RegionType.Row => Rows,
            RegionType.Column => Columns,
            RegionType.Box => Boxes,
            _ => throw new NotImplementedException(),
        };


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
