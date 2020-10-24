
using System.Buffers;

namespace Sudoku
{
    public class HiddenTupleStrategy : ISolveStrategy
    {
        private readonly int _size;

        public HiddenTupleStrategy(int size)
        {
            _size = size;
        }

        public void Apply(MutablePuzzle puzzle, RegionQueue changedRegions)
        {
            while (changedRegions.TryDequeue(puzzle, out var region))
            {
                var placedDigits = region.GetPlacedDigits();

                FindTuple(puzzle, changedRegions, region, placedDigits);
            }
        }

        private void FindTuple(MutablePuzzle puzzle, RegionQueue regions, Region region, SudokuValues placedDigits)
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
                var opposite = comb.Invert();
                var anyChanged = false;

                for (var index = 0; index < count; index++)
                {
                    var cell = region[options[index]];

                    if (!cell.IsSingle && cell.HasAnyOptions(opposite))
                    {
                        var update = region.RemoveOptions(options[index], opposite);
                        puzzle.RemoveOptions(update);
                        anyChanged = true;

                        regions.Enqueue(RegionType.Row, update.Coordinate.Row);
                        regions.Enqueue(RegionType.Column, update.Coordinate.Column);
                        regions.Enqueue(RegionType.Box, update.Coordinate.Box);
                    }
                }

                ArrayPool<int>.Shared.Return(options);

                if (anyChanged)
                {
#if DEBUG
                    Program.Debugger.AddAction($"Hidden {comb} tuple in {region}");
#endif
                    return;
                }
            }
        }
    }
}
