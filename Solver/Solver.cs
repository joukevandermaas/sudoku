using System.Buffers;
using System.Collections.Generic;

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
        private readonly int _maxSteps;
        private readonly int _maxBruteForceDepth;

        public Puzzle CurrentPuzzle { get; private set; }

        private readonly RegionQueue[] _regionQueues;
        private readonly (ISolveStrategy strat, string name)[] _strategies = new (ISolveStrategy, string)[]
        {
            (new NakedSingleStrategy(), "digits"),
            (new HiddenSingleStrategy(), "hidden single"),
            (new BoxLayoutStrategy(), "box layout"),
            (new HiddenTupleStrategy(2), "hidden pair"),
            (new TupleStrategy(2), "pair"),
            (new FishStrategy(2), "xwing"),
            (new HiddenTupleStrategy(3), "hidden tripple"),
            (new TupleStrategy(3), "tripple"),
            (new FishStrategy(3), "swordfish"),
            (new HiddenTupleStrategy(4), "hidden quadruple"),
            (new TupleStrategy(4), "quadruple"),
        };

        public Solver(Puzzle puzzle, int maxSteps, int maxBruteForceDepth)
        {
            CurrentPuzzle = puzzle;
            _maxSteps = maxSteps;
            _maxBruteForceDepth = maxBruteForceDepth;

            _regionQueues = new RegionQueue[_strategies.Length];

            for (int i = 0; i < _regionQueues.Length; i++)
            {
                _regionQueues[i] = RegionQueue.GetFull();
            }
        }


        public (SolveResult, Puzzle) Solve()
        {
            return Solve(1);
        }

        private (SolveResult, Puzzle) Solve(int bruteForceDepth)
        {
            var actualSteps = 0;

            while (!CurrentPuzzle.IsSolved && CurrentPuzzle.IsValid)
            {
                if (actualSteps >= _maxSteps)
                {
                    break;
                }

                var (success, _) = Advance();

                if (!success)
                {
                    success = AttemptBruteForce(bruteForceDepth);

                    if (!success)
                    {
                        break;
                    }
                }
                actualSteps += 1;
            }

            SolveResult result;

            if (CurrentPuzzle.IsSolved)
            {
                result = SolveResult.Success;
            }
            else
            {
                result = !CurrentPuzzle.IsValid ? SolveResult.Invalid : SolveResult.Failure;
            }

            return (result, CurrentPuzzle);
        }

        public (bool success, string? method) Advance()
        {
            for (int i = 0; i < _strategies.Length; i++)
            {
                var (strat, name) = _strategies[i];
                var workQueue = _regionQueues[i];
#if DEBUG
                    Program.AddDebugText($"{name} is considering regions: {workQueue}");
#endif

                var changeSet = strat.Apply(CurrentPuzzle, workQueue);

                if (changeSet != ChangeSet.Empty)
                {
#if DEBUG
                    Program.AddDebugText($"<br>{name} changed regions: {changeSet}");
#endif

                    var newPuzzle = changeSet.ApplyTo(CurrentPuzzle);
                    CurrentPuzzle = newPuzzle;

                    for (int j = 0; j < _regionQueues.Length; j++)
                    {
                        changeSet.ApplyTo(_regionQueues[j]);
                    }

                    return (true, name);
                }
            }

            return (false, null);
        }

        private bool AttemptBruteForce(int depth)
        {
            if (depth > _maxBruteForceDepth)
            {
                return false;
            }

            var optionsArray = ArrayPool<int>.Shared.Rent(Puzzle.LineLength);

            foreach (var region in CurrentPuzzle.Regions)
            {
                var placedDigits = region.GetPlacedDigits();

                for (int i = 1; i < Puzzle.LineLength; i++)
                {
                    var value = SudokuValues.FromHumanValue(i);

                    if (placedDigits.HasAnyOptions(value))
                    {
                        continue;
                    }

                    var success = EvaluateOptions(depth, optionsArray, region, value);

                    if (success)
                    {
                        ArrayPool<int>.Shared.Return(optionsArray);

                        return true;
                    }
                }
            }

            ArrayPool<int>.Shared.Return(optionsArray);
            return false;
        }

        private bool EvaluateOptions(int depth, int[] optionsArray, Region region, SudokuValues value)
        {
            var options = region.GetPositions(value);
            var count = options.CopyIndices(optionsArray);

            if (count != 2)
            {
                return false;
            }

            var newCell = region[optionsArray[0]].SetValue(value);

            var possiblePuzzle = CurrentPuzzle.UpdateCell(newCell);

            var otherSolver = new Solver(possiblePuzzle, _maxSteps, _maxBruteForceDepth);
            var (result, resultPuzzle) = otherSolver.Solve(depth + 1);

            if (result == SolveResult.Success)
            {
                CurrentPuzzle = resultPuzzle;
                return true;
            }
            else if (result == SolveResult.Invalid)
            {
                // it must be the other option
                var newPuzzle = CurrentPuzzle.UpdateCell(region[optionsArray[1]].SetValue(value));
                CurrentPuzzle = newPuzzle;

                return true;
            }

            return false;
        }
    }
}
