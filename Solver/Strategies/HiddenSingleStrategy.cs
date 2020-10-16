using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class HiddenSingleStrategy : ISolveStrategy
    {
        private List<Cell> _updates = new List<Cell>(Puzzle.LineLength * Puzzle.LineLength);
        private readonly HashSet<int> _changedIndices = new HashSet<int>(Puzzle.LineLength * Puzzle.LineLength);

        public ChangeSet Apply(in Puzzle puzzle, RegionQueue unprocessedRegions)
        {
            _updates.Clear();
            _changedIndices.Clear();

            var cells = puzzle.Cells.ToArray();

            var anySuccess = false;

            // keep scanning regions for naked singles, removing
            // options when digits are placed. when a digit is placed,
            // its box, row and column are added back to 'regions' so
            // they can be scanned again.
            while (unprocessedRegions.TryDequeue(cells, out var region))
            {
                anySuccess = ScanRegion(unprocessedRegions, cells, region) || anySuccess;
            }

            if (anySuccess)
            {
                foreach (var index in _changedIndices)
                {
                    _updates.Add(cells[index]);
                }
                return new ChangeSet(_updates);
            }

            return ChangeSet.Empty;
        }

        private bool ScanRegion(RegionQueue regions, Cell[] cells, Region region)
        {
            var placedDigits = region.GetPlacedDigits();
            var removedOptions = false;

            for (int i = 1; i <= Puzzle.LineLength; i++)
            {
                var digit = SudokuValues.FromHumanValue(i);

                if (placedDigits.HasAnyOptions(digit) || removedOptions)
                {
                    continue;
                }

                var positions = region.GetPositions(digit);

                if (positions.IsSingle)
                {
                    var index = positions.ToIndex();
                    var currentCell = region[index];

                    cells[currentCell.Index] = currentCell.SetValue(digit);
                    _changedIndices.Add(currentCell.Index);

                    regions.Enqueue(RegionType.Row, currentCell.Row);
                    regions.Enqueue(RegionType.Column, currentCell.Column);
                    regions.Enqueue(RegionType.Box, currentCell.Box);

                    removedOptions = true;
                }
            }

            return removedOptions;
        }

    }
}
