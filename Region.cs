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
        private readonly Cell[] _cells;

        public Region(Cell[] cells, RegionType type, int index)
        {
            _cells = (type switch
            {
                RegionType.Row => GetRow(cells, index),
                RegionType.Column => GetColumn(cells, index),
                RegionType.Box => GetBox(cells, index),
                _ => throw new NotImplementedException(),
            }).ToArray();
        }

        public Cell this[int i] => _cells[i];

        public bool IsValid
        {
            get
            {
                var values = SudokuValues.None;

                foreach (var cell in _cells)
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

        private static IEnumerable<Cell> GetRow(Cell[] cells, int number) => GetRange(
            cells,
            start: number * Puzzle.LineLength,
            end: (number + 1) * Puzzle.LineLength,
            increment: 1);

        private static IEnumerable<Cell> GetColumn(Cell[] cells, int number) => GetRange(
            cells,
            start: number,
            end: Puzzle.LineLength * Puzzle.LineLength + number,
            increment: Puzzle.LineLength);

        private static IEnumerable<Cell> GetBox(Cell[] cells, int number)
        {
            var result = Enumerable.Empty<Cell>();

            var boxRow = (number / Puzzle.BoxLength) * Puzzle.BoxLength; // note: integer division
            var boxCol = (number % Puzzle.BoxLength) * Puzzle.BoxLength;

            for (var i = 3; i > 0; i--)
            {
                var start = (boxRow + Puzzle.BoxLength - i) * Puzzle.LineLength + boxCol;

                var slice = GetRange(cells, start, start + Puzzle.BoxLength, 1);

                result = result.Concat(slice);
            }

            return result;
        }

        private static IEnumerable<Cell> GetRange(Cell[] cells, int start, int end, int increment)
        {
            for (var i = start; i < end; i += increment)
            {
                yield return cells[i];
            }
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            return (_cells as IEnumerable<Cell>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cells.GetEnumerator();
        }
    }
}
