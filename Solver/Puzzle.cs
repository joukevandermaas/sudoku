using System;
using System.Collections.Generic;
using System.Text;

namespace Sudoku
{

    public readonly struct Puzzle : IEquatable<Puzzle>
    {
        public const int LineLength = 9;
        public const int BoxLength = 3;

        private readonly IndexablePosition[] _cells;
        private readonly Region[] _regions;

        public IReadOnlyList<Region> Rows { get; }
        public IReadOnlyList<Region> Columns { get; }
        public IReadOnlyList<Region> Boxes { get; }

        public static Puzzle FromString(string puzzle)
        {
            var positions = new IndexablePosition[LineLength * LineLength];

            for (var i = 0; i < positions.Length; i++)
            {
                var value = SudokuValues.FromCharacter(puzzle[i]);
                var coord = new Coordinate(RegionType.Row, i);

                positions[coord.GlobalRowIndex] = positions[coord.GlobalRowIndex].SetRowValue(value);
                positions[coord.GlobalColumnIndex] = positions[coord.GlobalColumnIndex].SetColumnValue(value);
                positions[coord.GlobalBoxIndex] = positions[coord.GlobalBoxIndex].SetBoxValue(value);
            }

            return new Puzzle(positions);
        }

        public Puzzle(IndexablePosition[] cells)
        {
            _cells = cells;

            _regions = new Region[LineLength * 3];

            for (int i = 0; i < LineLength; i++)
            {
                _regions[i] = new Region(_cells, RegionType.Row, i);
                _regions[i + LineLength] = new Region(_cells, RegionType.Column, i);
                _regions[i + (LineLength * 2)] = new Region(_cells, RegionType.Box, i);
            }

            Rows = new ArraySegment<Region>(_regions, 0, LineLength);
            Columns = new ArraySegment<Region>(_regions, LineLength, LineLength);
            Boxes = new ArraySegment<Region>(_regions, LineLength * 2, LineLength);
        }

        public MutablePuzzle AsMutable()
        {
            var newCells = new IndexablePosition[_cells.Length];
            Array.Copy(_cells, newCells, _cells.Length);

            return new MutablePuzzle(newCells);
        }

        public SudokuValues this[RegionType type, int index]
            => _cells[index].GetValue(type);
        public SudokuValues this[RegionType type, int regionIndex, int indexInRegion]
            => _cells[(regionIndex * LineLength) + indexInRegion].GetValue(type);

        public bool IsSolved
        {
            get
            {
                for (int i = 0; i < _cells.Length; i++)
                {
                    var cell = _cells[i];

                    if (!cell.RowValue.IsSingle)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool IsValid
        {
            get
            {
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


        public IEnumerable<Region> Regions => _regions;

        public Region GetRegion(RegionType type, int index) => type switch
        {
            RegionType.Row => Rows[index],
            RegionType.Column => Columns[index],
            RegionType.Box => Boxes[index],
            _ => throw new NotImplementedException()
        };

        public IReadOnlyList<Region> GetRegions(RegionType type) => type switch
        {
            RegionType.Row => Rows,
            RegionType.Column => Columns,
            RegionType.Box => Boxes,
            _ => throw new NotImplementedException(),
        };

        public override string ToString()
        {
            var builder = new StringBuilder();

            for (int i = 0; i < _cells.Length; i++)
            {
                var cell = _cells[i];

                var value = cell.RowValue;

                if (value.IsSingle)
                {
                    builder.Append(value.ToHumanValue());
                }
                else
                {
                    builder.Append(0);
                }
            }

            return builder.ToString();
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
