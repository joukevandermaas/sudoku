﻿using System.Buffers;
using System.Net.Sockets;

namespace Sudoku
{
    /// <summary>
    /// Finds pairs, triples and quadruples of digits, removes those from other cells in the region
    /// note that quintuples and higher always correspond to one of the above tuples in the other cells
    /// </summary>
    public class TupleStrategy : ISolveStrategy
    {
        private readonly int _size;

        public TupleStrategy(int size)
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

                        var newCell = region.RemoveOptions(i, possibleValues);
                        puzzle.RemoveOptions(newCell);

                        regions.Enqueue(RegionType.Row, newCell.Coordinate.Row);
                        regions.Enqueue(RegionType.Column, newCell.Coordinate.Column);
                        regions.Enqueue(RegionType.Box, newCell.Coordinate.Box);
#if DEBUG
                        Program.Debugger.AddAction($"{possibleValues} tuple in {region}.");
#endif
                    }

                }
            }
        }
    }
}
