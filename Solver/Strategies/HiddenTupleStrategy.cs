
using System.Buffers;
using System.Collections.Generic;

namespace Sudoku
{
    public class HiddenTupleStrategy : ISolveStrategy
    {
        private BasicChangeSet _updatedCells = new BasicChangeSet();
        private readonly int _size;

        public HiddenTupleStrategy(int size)
        {
            _size = size;
        }

        public IChangeSet Apply(in Puzzle puzzle, RegionQueue changedRegions, SudokuValues changedDigits)
        {
            _updatedCells.Clear();

            while (changedRegions.TryDequeue(puzzle, out var region))
            {
                var placedDigits = region.GetPlacedDigits();

                FindTuple(puzzle, region, placedDigits, changedDigits);
            }

            return _updatedCells;
        }

        private void FindTuple(in Puzzle puzzle, Region region, SudokuValues placedDigits, SudokuValues changedDigits)
        {
            var combinations = Helpers.GetCombinationIndices(Puzzle.LineLength, _size);

            for (int i = 0; i < combinations.Length; i++)
            {
                var comb = combinations[i];

                if (placedDigits.HasAnyOptions(comb) || !changedDigits.HasAnyOptions(comb))
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

                for (var index = 0; index < count; index++)
                {
                    var cell = region[options[index]];

                    if (!cell.IsSingle && cell.HasAnyOptions(opposite))
                    {
                        var update = region.UpdateCell(options[index], opposite);
                        _updatedCells.Add(update);
                    }
                }

                ArrayPool<int>.Shared.Return(options);

                if (!_updatedCells.IsEmpty)
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
