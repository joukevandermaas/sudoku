using System.Buffers;
using System.Collections.Generic;

namespace Sudoku
{
    /// <summary>
    /// Finds pairs, triples and quadruples of digits, removes those from other cells in the region
    /// note that quintuples and higher always correspond to one of the above tuples in the other cells
    /// </summary>
    public class TupleStrategy : ISolveStrategy
    {
        private List<Cell> _updatedCells = new List<Cell>(Puzzle.LineLength);
        private readonly int _size;

        public TupleStrategy(int size)
        {
            _size = size;
        }

        public ChangeSet Apply(in Puzzle puzzle, RegionQueue unprocessedRegions)
        {
            _updatedCells.Clear();

            while (unprocessedRegions.TryDequeue(puzzle, out var region))
            {
                var placedDigits = region.GetPlacedDigits();

                FindTuple(puzzle, region, placedDigits);

                if (_updatedCells.Count > 0)
                {
                    return new ChangeSet(_updatedCells);
                }
            }

            return ChangeSet.Empty;
        }

        private void FindTuple(in Puzzle puzzle, Region region, SudokuValues placedDigits)
        {
            var combinations = Helpers.GetCombinationIndices(Puzzle.LineLength, _size);

            for (int j = 0; j < combinations.Length; j++)
            {
                var comb = combinations[j];

                if (placedDigits.HasAnyOptions(comb))
                {
                    // skip any tuples that include placed digits
                    continue;
                }

                var indices = ArrayPool<int>.Shared.Rent(_size);
                var count = comb.CopyIndices(indices);
                var possibleValues = SudokuValues.None;

                for (int i = 0; i < count; i++)
                {
                    var cell = region[indices[i]];
                    possibleValues = possibleValues.AddOptions(cell.Value);
                }

                ArrayPool<int>.Shared.Return(indices);

                var optionsCount = possibleValues.GetOptionCount();

                if (optionsCount == _size)
                {
                    // we found a tuple!

                    for (var i = 0; i < Puzzle.LineLength; i++)
                    {
                        var otherCell = region[i];

                        if (otherCell.IsResolved)
                        {
                            continue;
                        }

                        if (!otherCell.Value.HasAnyOptions(possibleValues))
                        {
                            continue;
                        }

                        if (comb.HasAnyOptions(SudokuValues.FromIndex(i)))
                        {
                            continue;
                        }

                        _updatedCells.Add(otherCell.RemoveOptions(possibleValues));
                    }

                    if (_updatedCells.Count > 0)
                    {
#if DEBUG
                        Program.AddDebugText($"{possibleValues} tuple in {region}.");
#endif
                        return;
                    }
                }
            }
        }
    }
}
