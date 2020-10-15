using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sudoku
{
    public class HiddenSingleStrategy : ISolveStrategy
    {
        public ChangeSet Apply(in Puzzle puzzle, RegionQueue unprocessedRegions)
        {
            var cells = puzzle.Cells.ToArray();
            var changedIndices = new HashSet<int>();

            var anySuccess = false;

            // keep scanning regions for naked singles, removing
            // options when digits are placed. when a digit is placed,
            // its box, row and column are added back to 'regions' so
            // they can be scanned again.
            while (unprocessedRegions.TryDequeue(cells, out var region))
            {
                anySuccess = ScanRegion(unprocessedRegions, cells, region, changedIndices) || anySuccess;
            }

            if (anySuccess)
            {
                var changes = new List<Cell>(changedIndices.Count);
                foreach (var index in changedIndices)
                {
                    changes.Add(cells[index]);
                }
                return new ChangeSet(changes);
            }

            return ChangeSet.Empty;
        }

        private bool ScanRegion(RegionQueue regions, Cell[] cells, Region region, HashSet<int> changedIndices)
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
                    changedIndices.Add(currentCell.Index);

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
