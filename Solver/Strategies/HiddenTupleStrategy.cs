
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.ComTypes;

namespace Sudoku
{
    public class HiddenTupleStrategy : ISolveStrategy
    {
        private readonly int _size;

        public HiddenTupleStrategy(int size)
        {
            _size = size;
        }

        public ChangeSet Apply(in Puzzle puzzle, RegionQueue unprocessedRegions)
        {
            var updatedCells = new List<Cell>();

            while (unprocessedRegions.TryDequeue(puzzle, out var region))
            {
                var placedDigits = region.GetPlacedDigits();

                FindTuple(puzzle, updatedCells, region, placedDigits);

                if (updatedCells.Count > 0)
                {
                    return new ChangeSet(updatedCells);
                }
            }

            return ChangeSet.Empty;
        }

        private void FindTuple(in Puzzle puzzle, List<Cell> updatedCells, Region region, SudokuValues placedDigits)
        {
            var combinations = Helpers.GetCombinationIndices(Puzzle.LineLength, _size);

            for (int i = 0; i < combinations.Length; i++)
            {
                var comb = combinations[i];

                if (placedDigits.HasAnyOptions(comb))
                {
                    // skip any tuples that include placed digits
                    continue;
                }

                var positions = region.GetPositions(comb);

                if (positions.GetOptionCount() != _size)
                {
                    // these digits are placeable in more than tupleSize spots, so not hidden tuple
                    continue;
                }

                // we found a hidden tuple!
                var options = ArrayPool<int>.Shared.Rent(Puzzle.LineLength);
                var count = positions.CopyIndices(options);
                var opposite = SudokuValues.Invert(comb);

                for (var index = 0; index < count; index++)
                {
                    var cell = region[options[index]];

                    if (cell.HasOptions(opposite))
                    {
                        updatedCells.Add(cell.RemoveOptions(opposite));
                    }
                }

                ArrayPool<int>.Shared.Return(options);

                if (updatedCells.Count > 0)
                {
#if DEBUG
                    Program.AddDebugText($"Hidden {comb} tuple in {region}");
#endif
                    return;
                }
            }
        }
    }
}
