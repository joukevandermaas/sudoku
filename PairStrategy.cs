using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    /// <summary>
    /// Removes options based on placed digits.
    /// </summary>
    internal class PairStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            return puzzle.ForEveryRegion(ScanRegion);
        }

        private static List<Cell> ScanRegion(Region region)
        {
            var lists = new List<Cell>[Puzzle.LineLength];

            for (var i = 0; i < Puzzle.LineLength; i++)
            {
                lists[i] = new List<Cell>();

                var value = SudokuValues.FromHumanValue(i + 1);

                foreach (var cell in region)
                {
                    if (cell.IsResolved)
                    {
                        continue;
                    }

                    if (cell.Value.HasAnyOptions(value))
                    {
                        lists[i].Add(cell);
                    }
                }

                //Console.WriteLine("{0}: {1}", i + 1, string.Join(", ", lists[i].Select(c => $"R{c.Row}C{c.Column}")));
            }

            // todo implement
            return new List<Cell>();
        }
    }
}
