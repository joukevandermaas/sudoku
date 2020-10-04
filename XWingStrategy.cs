using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    internal class XWingStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {

            for (var digit = 1; digit <= Puzzle.LineLength; digit++)
            {
                var (success, newPuzzle) = FindXWing(puzzle, RegionType.Column, digit);

                if (success)
                {
                    return (true, newPuzzle);
                }

                (success, newPuzzle) = FindXWing(puzzle, RegionType.Row, digit);

                if (success)
                {
                    return (true, newPuzzle);
                }
            }

            return (false, puzzle);
        }

        private (bool, Puzzle) FindXWing(Puzzle puzzle, RegionType type, int digit)
        {
            var perpendicularType = type == RegionType.Row ? RegionType.Column : RegionType.Row;
            var value = SudokuValues.FromHumanValue(digit);

            for (var i = 0; i < Puzzle.LineLength; i++)
            {
                var region = puzzle.GetRegion(type, i);
                var positions = GetPositions(region, value);

                if (positions.GetOptionCount() != 2)
                {
                    continue;
                }

                for (var j = i + 1; j < Puzzle.LineLength; j++)
                {
                    var otherRegion = puzzle.GetRegion(type, j);
                    var otherPositions = GetPositions(otherRegion, value);

                    if (otherPositions != positions)
                    {
                        continue;
                    }

                    var options = positions.ToHumanOptions();

                    var updatedCells = new List<Cell>();
                    foreach (var option in options)
                    {
                        var row = puzzle.GetRegion(perpendicularType, option - 1);

                        foreach (var cell in row)
                        {
                            if (cell.Column != i && cell.Column != j && cell.HasOptions(value))
                            {
                                updatedCells.Add(cell.RemoveOptions(value));
                            }
                        }
                    }

                    if (updatedCells.Any())
                    {
                        return (true, puzzle.UpdateCells(updatedCells));
                    }
                }
            }

            return (false, puzzle);
        }

        private SudokuValues GetPositions(Region region, SudokuValues digits)
        {
            var positions = SudokuValues.None;

            for (var i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];
                if (cell.HasOptions(digits))
                {
                    positions = positions.AddOptions(SudokuValues.FromHumanValue(i + 1));
                }
            }

            return positions;
        }
    }
}
