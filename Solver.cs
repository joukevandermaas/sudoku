using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    enum SolveResult
    {
        None,
        Success,
        Failure,
        Invalid
    }

    class Solver
    {
        private readonly ISolveStrategy[] _strategies;
        private readonly int _maxSteps;
        private readonly int _maxBruteForceDepth;

        private RegionQueue _queue = new RegionQueue();

        public Solver(int maxSteps, int maxBruteForceDepth, IEnumerable<ISolveStrategy> strategies)
        {
            _strategies = strategies.ToArray();
            _maxSteps = maxSteps;
            _maxBruteForceDepth = maxBruteForceDepth;
        }

        public (SolveResult, Puzzle) Solve(in Puzzle puzzle)
        {
            return Solve(puzzle, 1);
        }

        private (SolveResult, Puzzle) Solve(Puzzle puzzle, int bruteForceDepth)
        {
            var actualSteps = 0;

            while (!puzzle.IsSolved && puzzle.IsValid)
            {
                if (actualSteps >= _maxSteps)
                {
                    break;
                }

                var (success, newPuzzle, _) = Advance(puzzle);
                puzzle = newPuzzle;

                if (!success)
                {
                    (success, newPuzzle) = AttemptBruteForce(puzzle, bruteForceDepth);
                    puzzle = newPuzzle;

                    if (!success)
                    {
                        break;
                    }
                }
                actualSteps += 1;
            }

            SolveResult result;

            if (puzzle.IsSolved)
            {
                result = SolveResult.Success;
            }
            else
            {
                result = !puzzle.IsValid ? SolveResult.Invalid : SolveResult.Failure;
            }

            return (result, puzzle);
        }

        public (bool, Puzzle, ISolveStrategy?) Advance(Puzzle puzzle)
        {
            bool digitsSuccess;
            (digitsSuccess, puzzle) = RemoveInvalidOptions(puzzle);

#if DEBUG
            if (digitsSuccess)
            {
                return (digitsSuccess, puzzle, null);
            }
#endif

            foreach (var strat in _strategies)
            {
                var (success, newPuzzle) = strat.Apply(puzzle);
                puzzle = newPuzzle;

                if (success)
                {
                    return (true, puzzle, strat);
                }
            }

            return (false, puzzle, null);
        }

        public (bool, Puzzle) RemoveInvalidOptions(in Puzzle puzzle)
        {
            var cells = puzzle.Cells.ToArray();
            // this puzzle is "mutable" in the sense that we have
            // a reference to its internal cell array. we must
            // not let this reference escape this scope.
            var mutablePuzzle = new Puzzle(cells);

            foreach (var region in mutablePuzzle.Regions)
            {
                _queue.Enqueue(region);
            }

            var anySuccess = false;

            // keep scanning regions for naked singles, removing
            // options when digits are placed. when a digit is placed,
            // its box, row and column are added back to 'regions' so
            // they can be scanned again.
            while (_queue.TryDequeue(out var region))
            {
                anySuccess = ScanRegionForInvalidOptions(mutablePuzzle, cells, region) || anySuccess;
            }

            if (anySuccess)
            {
                // we're done mutating, so we can just return the
                // puzzle now.
                return (true, mutablePuzzle);
            }

            return (false, puzzle);
        }

        private bool ScanRegionForInvalidOptions(Puzzle puzzle, Cell[] cells, Region region)
        {
            var placedDigits = region.GetPlacedDigits();
            var removedOptions = false;

            for (int i = 0; i < Puzzle.LineLength; i++)
            {
                var cell = region[i];

                if (cell.HasOptions(placedDigits))
                {
                    var newCell = cell.RemoveOptions(placedDigits);
                    cells[cell.Index] = newCell;

                    if (newCell.IsResolved)
                    {
                        // since we have placed a digit we must
                        // scan these regions again
                        _queue.Enqueue(puzzle.Rows[newCell.Row]);
                        _queue.Enqueue(puzzle.Columns[newCell.Column]);
                        _queue.Enqueue(puzzle.Boxes[newCell.Box]);
                    }

                    removedOptions = true;
                }

                // we now interpret i as a digit instead and check if it only
                // has one position in the region (then we can place it)
                var digit = SudokuValues.FromIndex(i);

                if (placedDigits.HasAnyOptions(digit) || removedOptions)
                {
                    continue;
                }

                var positions = region.GetPositions(digit);

                if (positions.IsSingle)
                {
                    var index = positions.ToIndex();
                    var currentCell = region[index];

                    cells[currentCell.Index] = currentCell.SetValue(digit);

                    _queue.Enqueue(puzzle.Rows[currentCell.Row]);
                    _queue.Enqueue(puzzle.Columns[currentCell.Column]);
                    _queue.Enqueue(puzzle.Boxes[currentCell.Box]);

                    removedOptions = true;
                }
            }

            return removedOptions;
        }

        private (bool, Puzzle) AttemptBruteForce(in Puzzle puzzle, int depth)
        {
            if (depth > _maxBruteForceDepth)
            {
                return (false, puzzle);
            }

            var optionsArray = ArrayPool<int>.Shared.Rent(Puzzle.LineLength);

            foreach (var region in puzzle.Regions)
            {
                var placedDigits = region.GetPlacedDigits();

                for (int i = 1; i < Puzzle.LineLength; i++)
                {
                    var value = SudokuValues.FromHumanValue(i);

                    if (placedDigits.HasAnyOptions(value))
                    {
                        continue;
                    }

                    var (success, newPuzzle) = EvaluateOptions(puzzle, depth, optionsArray, region, value);

                    if (success)
                    {
                        ArrayPool<int>.Shared.Return(optionsArray);

                        return (true, newPuzzle);
                    }
                }
            }

            ArrayPool<int>.Shared.Return(optionsArray);
            return (false, puzzle);
        }

        private (bool, Puzzle) EvaluateOptions(Puzzle puzzle, int depth, int[] optionsArray, Region region, SudokuValues value)
        {
            var options = region.GetPositions(value);
            var count = options.CopyIndices(optionsArray);

            if (count != 2)
            {
                return (false, puzzle);
            }

            var possiblePuzzle = puzzle.UpdateCell(region[optionsArray[0]].SetValue(value));

            var (result, resultPuzzle) = Solve(possiblePuzzle, depth + 1);

            if (result == SolveResult.Success)
            {
                return (true, resultPuzzle);
            }
            else if (result == SolveResult.Invalid)
            {
                // it must be the other option
                var newPuzzle = puzzle.UpdateCell(region[optionsArray[1]].SetValue(value));

                return (true, newPuzzle);
            }

            return (false, puzzle);
        }
    }
}
