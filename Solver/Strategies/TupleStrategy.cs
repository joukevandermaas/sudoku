using System.Buffers;
using System.Net.Sockets;

namespace Sudoku
{
    /// <summary>
    /// Finds pairs, triples and quadruples of digits, removes those from other cells in the region
    /// note that quintuples and higher always correspond to one of the above tuples in the other cells
    /// </summary>
    public class TupleStrategy : ISolveStrategy
    {
        private BasicChangeSet _updatedCells = new BasicChangeSet();
        private readonly int _size;

        public TupleStrategy(int size)
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

            for (int j = 0; j < combinations.Length; j++)
            {
                var comb = combinations[j];

                if (placedDigits.HasAnyOptions(comb) || !changedDigits.HasAnyOptions(comb))
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
                    possibleValues = possibleValues.AddOptions(cell);
                }

                ArrayPool<int>.Shared.Return(indices);

                var optionsCount = possibleValues.GetOptionCount();

                if (optionsCount == _size)
                {
                    // we found a tuple!

                    for (var i = 0; i < Puzzle.LineLength; i++)
                    {
                        var otherCell = region[i];

                        if (otherCell.IsSingle)
                        {
                            continue;
                        }

                        if (!otherCell.HasAnyOptions(possibleValues))
                        {
                            continue;
                        }

                        if (comb.HasAnyOptions(SudokuValues.FromIndex(i)))
                        {
                            continue;
                        }

                        var newCell = region.UpdateCell(i, possibleValues);
                        _updatedCells.Add(newCell);
                    }

#if DEBUG
                    if (!_updatedCells.IsEmpty)
                    {
                        Program.AddDebugText($"{possibleValues} tuple in {region}.");
                    }
#endif
                }
            }
        }
    }
}
