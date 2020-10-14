using System;
using System.Collections;
using System.Collections.Generic;

namespace Sudoku
{
    enum RegionType
    {
        None,
        Row,
        Column,
        Box
    }

    internal readonly struct Region : IEnumerable<Cell>, IEquatable<Region>
    {
        private readonly Cell[] _allCells;

        public Region(Cell[] cells, RegionType type, int index)
        {
            _allCells = cells;
            Type = type;
            Index = index;
        }

        public RegionType Type { get; }

        public int Index { get; }

        public ref Cell this[int i]
        {
            get
            {
                int offset;

                switch (Type)
                {
                    case RegionType.Row:
                        offset = (Index * Puzzle.LineLength) + i;
                        break;

                    case RegionType.Column:
                        offset = Index + (i * Puzzle.LineLength);
                        break;

                    case RegionType.Box:
                        // note: integer division!
                        var insideBoxRow = i / Puzzle.BoxLength;
                        var insideBoxCol = i % Puzzle.BoxLength;

                        var outsideBoxRow = Index / Puzzle.BoxLength;
                        var outsideBoxCol = Index % Puzzle.BoxLength;

                        var row = outsideBoxRow * Puzzle.BoxLength + insideBoxRow;
                        var col = outsideBoxCol * Puzzle.BoxLength + insideBoxCol;

                        offset = (row * Puzzle.LineLength) + col;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return ref _allCells[offset];
            }
        }

        public bool Contains(Cell cell)
        {
            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                if (this[i] == cell)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<Cell> GetCells()
        {
            for (var i = 0; i < Puzzle.LineLength; i++)
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
                    var cell = this[i];

                    if (cell.IsResolved)
                    {
                        if (values.HasAnyOptions(cell.Value))
                        {
                            // already found that number, so not valid
                            return false;
                        }

                        values = values.AddOptions(cell.Value);
                    }
                }

                return true;
            }
        }

        public override string ToString()
        {
            return $"{Type} {Index + 1}";
        }

        public IEnumerator<Cell> GetEnumerator() => GetCells().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetCells().GetEnumerator();

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
