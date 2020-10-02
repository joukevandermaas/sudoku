using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    /// <summary>
    /// Finds pairs, triples and quadruples of digits, removes those from other cells in the region
    /// note that quintuples and higher always correspond to one of the above tuples in the other cells
    /// </summary>
    internal class TupleStrategy : ISolveStrategy
    {
        private static List<Cell> EmptyList = new List<Cell>();


        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            var tupleSuccess = false;
            var tripleSuccess = false;
            var quadrupleSuccess = false;

            (tupleSuccess, puzzle) = puzzle.ForEveryRegion(r => ScanRegion(r, 2));
            (tripleSuccess, puzzle) = puzzle.ForEveryRegion(r => ScanRegion(r, 2));
            (quadrupleSuccess, puzzle) = puzzle.ForEveryRegion(r => ScanRegion(r, 2));

            return (tupleSuccess || tripleSuccess || quadrupleSuccess, puzzle);
        }

        private static List<Cell> ScanRegion(Region region, int tupleSize)
        {
            var result = new List<Cell>();

            var emptyCells = region.Where(c => !c.IsResolved).ToArray();

            if (emptyCells.Length <= tupleSize)
            {
                return EmptyList;
            }

            var combinations = GetCombinations(emptyCells, tupleSize);

            foreach (var combination in combinations)
            {
                var possibleValues = SudokuValues.None;

                foreach (var cell in combination)
                {
                    possibleValues = possibleValues.AddOptions(cell.Value);
                }

                var optionsCount = possibleValues.GetOptionCount();

                if (optionsCount == tupleSize)
                {
                    // we found a tuple!

                    foreach (var otherCell in region)
                    {
                        if (otherCell.IsResolved || combination.Contains(otherCell))
                        {
                            continue;
                        }

                        if (!otherCell.Value.HasAnyOptions(possibleValues))
                        {
                            continue;
                        }

                        result.Add(otherCell.RemoveOptions(possibleValues));
                    }
                }
            }

            return result;
        }

        // copied from stack overflow.. it can use some improvement but it's fine for now
        public static IEnumerable<Cell[]> GetCombinations(Cell[] items, int m)
        {
            Cell[] result = new Cell[m];
            foreach (int[] j in GetIndices(m, items.Length))
            {
                for (int i = 0; i < m; i++)
                {
                    result[i] = items[j[i]];
                }
                yield return result;
            }
        }
        private static IEnumerable<int[]> GetIndices(int m, int n)
        {
            int[] result = new int[m];
            Stack<int> stack = new Stack<int>(m);
            stack.Push(0);
            while (stack.Count > 0)
            {
                int index = stack.Count - 1;
                int value = stack.Pop();
                while (value < n)
                {
                    result[index++] = value++;
                    stack.Push(value);
                    if (index != m) continue;
                    yield return result;
                    break;
                }
            }
        }
    }
}
