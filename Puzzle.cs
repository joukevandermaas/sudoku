using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sudoku
{
    internal readonly struct Puzzle : IEquatable<Puzzle>
    {
        public const int LineLength = 9;
        public const int BoxLength = 3;

        private readonly Cell[] _cells;

        public ReadOnlyCollection<Region> Rows { get; }
        public ReadOnlyCollection<Region> Columns { get; }
        public ReadOnlyCollection<Region> Boxes { get; }

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

            var rows = new Region[LineLength];
            var columns = new Region[LineLength];
            var boxes = new Region[LineLength];

            for (int i = 0; i < LineLength; i++)
            {
                rows[i] = new Region(_cells, RegionType.Row, i);
                columns[i] = new Region(_cells, RegionType.Column, i);
                boxes[i] = new Region(_cells, RegionType.Box, i);
            }

            Rows = Array.AsReadOnly(rows);
            Columns = Array.AsReadOnly(columns);
            Boxes = Array.AsReadOnly(boxes);
        }

        public bool IsSolved
        {
            get
            {
                for (int i = 0; i < _cells.Length; i++)
                {
                    var cell = _cells[i];

                    if (!cell.IsResolved)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public Cell this[int i] => _cells[i];
        public Cell this[int row, int col] => _cells[row * LineLength + col];

        public bool IsValid
        {
            get
            {
                for (int i = 0; i < _cells.Length; i++)
                {
                    var cell = _cells[i];

                    if (!cell.IsValid)
                    {
                        return false;
                    }
                }

                for (int i = 0; i < Rows.Count; i++)
                {
                    var row = Rows[i];
                    if (!row.IsValid)
                    {
                        return false;
                    }
                }

                for (int i = 0; i < Columns.Count; i++)
                {
                    var col = Columns[i];
                    if (!col.IsValid)
                    {
                        return false;
                    }
                }
                
                for (int i = 0; i < Boxes.Count; i++)
                {
                    var box = Boxes[i];
                    if (!box.IsValid)
                    {
                        return false;
                    }
                }

                return true;
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
