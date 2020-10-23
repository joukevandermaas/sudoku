using System;
using System.Collections.Generic;

namespace Sudoku
{
    public enum SolveResult
    {
        None,
        Success,
        Failure,
        Invalid
    }

    public class Solver
    {
        private readonly int _maxSteps;

        public Puzzle CurrentPuzzle { get; private set; }

        public int MaxBruteForceDepth { get; }

        private readonly List<SudokuValues> _digits;
        private readonly List<RegionQueue> _regionQueues;
        private readonly List<(ISolveStrategy strat, string name)> _strategies;

        public Solver(Puzzle puzzle, int maxSteps, int maxBruteForceDepth)
        {
            CurrentPuzzle = puzzle;
            _maxSteps = maxSteps;
            MaxBruteForceDepth = maxBruteForceDepth;

            _strategies = new List<(ISolveStrategy, string)>
            {
                (new NakedSingleStrategy(), "digits"),
                (new HiddenSingleStrategy(), "hidden single"),
                (new BoxLayoutStrategy(), "box layout"),
                (new HiddenTupleStrategy(2), "hidden pair"),
                (new TupleStrategy(2), "pair"),
                (new HiddenTupleStrategy(3), "hidden tripple"),
                (new TupleStrategy(3), "tripple"),
                (new FishStrategy(2), "xwing"),
                (new HiddenTupleStrategy(4), "hidden quadruple"),
                (new TupleStrategy(4), "quadruple"),
                (new FishStrategy(3), "swordfish"),
            };

            if (MaxBruteForceDepth > 0)
            {
                _strategies.Add((new BruteForceStrategy2(this), "brute force (options in cell)"));
                _strategies.Add((new BruteForceStrategy(this), "brute force (options in region)"));
            }

            _regionQueues = new List<RegionQueue>(_strategies.Count);
            _digits = new List<SudokuValues>(_strategies.Count);

            for (int i = 0; i < _strategies.Count; i++)
            {
                _regionQueues.Add(RegionQueue.GetFull());
                _digits.Add(SudokuValues.None);
            }
        }

        internal Solver(IChangeSet changeSet, Solver other)
        {
            CurrentPuzzle = changeSet.ApplyToPuzzle(other.CurrentPuzzle);
            _maxSteps = other._maxSteps;
            MaxBruteForceDepth = other.MaxBruteForceDepth;

            _strategies = other._strategies;

            _regionQueues = new List<RegionQueue>(_strategies.Count);
            _digits = new List<SudokuValues>(_strategies.Count);

            for (int i = 0; i < _strategies.Count; i++)
            {
                var queue = other._regionQueues[i].Clone();
                changeSet.ApplyToRegionQueue(queue);
                _regionQueues.Add(queue);

                _digits.Add(changeSet.AddModifiedDigits(other._digits[i]));
            }
        }

        public (SolveResult, Puzzle) Solve()
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
                    break;
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
            for (int i = 0; i < _strategies.Count; i++)
            {
                var (strat, name) = _strategies[i];
                var workQueue = _regionQueues[i];
                var digits = _digits[i];
#if DEBUG
                Program.AddDebugText($"{name} is considering regions: {workQueue}", "small");
                Program.AddDebugText($"{name} is considering digits: {digits}", "small");
#endif

                var changeSet = strat.Apply(CurrentPuzzle, workQueue, digits);

                _digits[i] = SudokuValues.None;

                if (changeSet.IsEmpty)
                {
                    continue;
                }
#if DEBUG
                Program.AddDebugText($"<br>{name} changed: {changeSet}", "small");
#endif

                var newPuzzle = changeSet.ApplyToPuzzle(CurrentPuzzle);
                CurrentPuzzle = newPuzzle;

                for (int j = 0; j < _regionQueues.Count; j++)
                {
                    changeSet.ApplyToRegionQueue(_regionQueues[j]);
                    _digits[j] = changeSet.AddModifiedDigits(_digits[j]);
                }

                return (true, name);
            }

            return (false, null);
        }
    }
}
