using System;
using System.Buffers;
using System.Collections.Generic;

namespace Sudoku
{
    public class FishStrategy : ISolveStrategy
    {
        private List<(Region region, SudokuValues positions)> _potentialRegions = new List<(Region region, SudokuValues positions)>(Puzzle.LineLength);

        private readonly int _size;
        private List<Region> _regions;

        public FishStrategy(int size)
        {
            _size = size;
            _regions = new List<Region>(_size);
        }

        public void Apply(MutablePuzzle puzzle, RegionQueue changedRegions)
        {
            var changedRows = changedRegions.Rows;
            var changedCols = changedRegions.Columns;

            for (var digit = 1; digit <= Puzzle.LineLength; digit++)
            {
                var value = SudokuValues.FromHumanValue(digit);

                if (changedRows != SudokuValues.None)
                {
                    if (FindSwordfish(puzzle, RegionType.Row, value))
                    {
                        return;
                    }
                }

                if (changedCols != SudokuValues.None)
                {
                    if (FindSwordfish(puzzle, RegionType.Column, value))
                    {
                        return;
                    }
                }
            }

            changedRegions.Clear();
        }

        private bool FindSwordfish(MutablePuzzle puzzle, RegionType type, SudokuValues digit)
        {
            _potentialRegions.Clear();
            _regions.Clear();

            var perpendicularType = type == RegionType.Row ? RegionType.Column : RegionType.Row;

            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                var region = puzzle.Puzzle.GetRegion(type, i);
                var positions = region.GetPositions(digit);
                var count = positions.GetOptionCount();

                if (count > 1 && count <= _size)
                {
                    _potentialRegions.Add((region, positions));
                }
            }

            // cannot find a swordfish, because we don't have enough rows/columns
            if (_potentialRegions.Count < _size)
            {
                return false;
            }

            var combinations = Helpers.GetCombinationIndices(_potentialRegions.Count, _size);
            var combinedPositions = SudokuValues.None;
            var foundFish = false;

            for (int i = 0; i < combinations.Length; i++)
            {
                var combination = ArrayPool<int>.Shared.Rent(_size);

                combinations[i].CopyIndices(combination);

                combinedPositions = SudokuValues.None;
                _regions.Clear();

                for (var index = 0; index < _size; index++)
                {
                    var regionInfo = _potentialRegions[index];
                    combinedPositions = combinedPositions.AddOptions(regionInfo.positions);
                    _regions.Add(regionInfo.region);
                }

                ArrayPool<int>.Shared.Return(combination);

                if (combinedPositions.GetOptionCount() == _size)
                {
                    foundFish = true;
                    break;
                }
            }

            // no 3 rows/columns had options in exactly 3 spots
            if (!foundFish)
            {
                return false;
            }

            // we found a swordfish!
            var options = combinedPositions.ToIndices();

            foreach (var option in options)
            {
                var perpendicularRegion = puzzle.Puzzle.GetRegion(perpendicularType, option);

                for (int i = 0; i < Puzzle.LineLength; i++)
                {
                    var coords = perpendicularRegion.GetCoordinate(i);
                    var cell = perpendicularRegion[i];

                    if (!cell.HasAnyOptions(digit))
                    {
                        continue;
                    }

                    var originalRegionsContains = false;
                    foreach (var region in _regions)
                    {
                        if (region.Index == i)
                        {
                            originalRegionsContains = true;
                            break;
                        }
                    }

                    if (originalRegionsContains)
                    {
                        continue;
                    }

                    var update = new CellUpdate(digit, coords);
                    puzzle.RemoveOptions(update);
#if DEBUG
                    Program.Debugger.AddAction($"Fish of size {_size} in {string.Join(", ", _regions)}.");
#endif
                    return true;
                }
            }

            return false;

        }
    }
}
