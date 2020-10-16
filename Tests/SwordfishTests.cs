using Sudoku;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{

    public class SwordfishTests
    {
        [Fact]
        public void XWingInRows()
        {
            var top = 4;
            var bottom = 8;
            var left = 3;
            var right = 7;

            var puzzle = CreateRowXWing(
                TestHelpers.GetEmptyPuzzle(), left, right, top, bottom);

            TestHelpers.OpenAsHtml(puzzle, 2);

            var target = new FishStrategy(2);

            var changeSet = target.Apply(puzzle, RegionQueue.GetFull());
            // todo assert
        }

        private static Puzzle CreateRowXWing(Puzzle puzzle, int left, int right, int top, int bottom)
        {
            var topRow = puzzle.Rows[top];
            var botRow = puzzle.Rows[bottom];

            var updatedCells = new List<Cell>();
            for (int column = 0; column < Puzzle.LineLength; column++)
            {
                if (column == left || column == right) continue;

                updatedCells.Add(topRow[column].RemoveOptions(SudokuValues.FromHumanValue(2)));
                updatedCells.Add(botRow[column].RemoveOptions(SudokuValues.FromHumanValue(2)));
            }

            puzzle = puzzle.UpdateCells(updatedCells);
            return puzzle;
        }
    }
}
