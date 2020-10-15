using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sudoku
{
    public readonly struct Puzzle : IEquatable<Puzzle>
    {
        public const int LineLength = 9;
        public const int BoxLength = 3;

        private readonly Cell[] _cells;
        private readonly Region[] _regions;

        public IReadOnlyList<Region> Rows => new ArraySegment<Region>(_regions, 0, LineLength);
        public IReadOnlyList<Region> Columns => new ArraySegment<Region>(_regions, LineLength, LineLength);
        public IReadOnlyList<Region> Boxes => new ArraySegment<Region>(_regions, LineLength * 2, LineLength);

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

            _regions = new Region[LineLength * 3];

            for (int i = 0; i < LineLength; i++)
            {
                _regions[i] = new Region(_cells, RegionType.Row, i);
                _regions[i + LineLength] = new Region(_cells, RegionType.Column, i);
                _regions[i + (LineLength * 2)] = new Region(_cells, RegionType.Box, i);
            }
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

                if (cell.IsResolved)
                {
                    builder.Append(cell.Value.ToHumanValue());
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
