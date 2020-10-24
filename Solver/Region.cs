using System;
using System.Collections;
using System.Collections.Generic;

namespace Sudoku
{
    public enum RegionType
    {
        None,
        Row,
        Column,
        Box
    }

    public readonly struct Region : IEnumerable<SudokuValues>, IEquatable<Region>
    {
        private readonly IndexablePosition[] _allCells;

        public Region(IndexablePosition[] cells, RegionType type, int index)
        {
            _allCells = cells;
            Type = type;
            Index = index;
        }

        public RegionType Type { get; }

        public int Index { get; }

        public SudokuValues this[int i]
        {
            get
            {
                var pos = _allCells[(Index * Puzzle.LineLength) + i];
                return pos.GetValue(Type);
            }
        }

        public Coordinate GetCoordinate(int index)
        {
            var coordinate = new Coordinate(this.Type, this.Index, index);
            return coordinate;
        }

        public CellUpdate RemoveOptions(int index, SudokuValues newValue)
        {
            var coordinate = GetCoordinate(index);
            return new CellUpdate(newValue, coordinate);
        }

        private IEnumerable<SudokuValues> GetValues()
        {
            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                yield return this[i];
            }
        }

        public bool IsValid
        {
            get
            {
                var values = SudokuValues.None;

                for (int i = 0; i < Puzzle.LineLength; i++)
                {
                    var value = this[i];

                    if (value == SudokuValues.None)
                    {
                        return false;
                    }

                    if (value.IsSingle)
                    {
                        if (values.HasAnyOptions(value))
                        {
                            // already found that number, so not valid
                            return false;
                        }

                        values = values.AddOptions(value);
                    }
                }

                return true;
            }
        }

        public override string ToString()
        {
            return $"{Type} {Index + 1}";
        }

        public IEnumerator<SudokuValues> GetEnumerator() => GetValues().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetValues().GetEnumerator();

        public bool Equals(Region other)
        {
            return other.Index == Index && other.Type == Type && other._allCells == _allCells;
        }

        public override bool Equals(object? obj)
        {
            return obj is Region other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Index, Type, _allCells);
        }
    }
}
