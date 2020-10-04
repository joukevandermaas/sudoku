using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    enum RegionType
    {
        None,
        Row,
        Column,
        Box
    }

    struct Region : IEnumerable<Cell>
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

        public Cell this[int i]
        {
            get
            {
                switch (Type)
                {
                    case RegionType.Row:
                        var rowIndex = (Index * Puzzle.LineLength) + i;
                        return _allCells[rowIndex];

                    case RegionType.Column:
                        var colIndex = Index + (i * Puzzle.LineLength);
                        return _allCells[colIndex];

                    case RegionType.Box:
                        var insideBoxRow = i / Puzzle.BoxLength;
                        var insideBoxCol = i % Puzzle.BoxLength;

                        var outsideBoxRow = Index / Puzzle.BoxLength;
                        var outsideBoxCol = Index % Puzzle.BoxLength;

                        var row = outsideBoxRow * Puzzle.BoxLength + insideBoxRow;
                        var col = outsideBoxCol * Puzzle.BoxLength + insideBoxCol;

                        var boxIndex = (row * Puzzle.LineLength) + col;
                        return _allCells[boxIndex];

                    default:
                        throw new NotImplementedException();
                }
            }
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

                foreach (var cell in GetCells())
                {
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

        public IEnumerator<Cell> GetEnumerator() => GetCells().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetCells().GetEnumerator();
    }
}
