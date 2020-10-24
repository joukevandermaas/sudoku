using System;
using System.Collections.Generic;
using System.Threading.Channels;

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
        public MutablePuzzle Puzzle { get; private set; }

        private readonly List<RegionQueue> _regionQueues;
        private readonly List<(ISolveStrategy strat, string name)> _strategies;

        public Solver(Puzzle puzzle, bool allowBruteForce)
        {
            Puzzle = puzzle.AsMutable();

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

            if (allowBruteForce)
            {
                _strategies.Add((new GuessStrategy(this), "guess"));
            }

            _regionQueues = new List<RegionQueue>(_strategies.Count);

            for (int i = 0; i < _strategies.Count; i++)
            {
                _regionQueues.Add(RegionQueue.GetFull());
            }
        }

        internal Solver(IChangeSet changeSet, Solver other)
        {
            Puzzle = other.Puzzle.Clone().AsMutable();

            changeSet.ApplyToPuzzle(Puzzle);

#if DEBUG
            Program.Debugger.AddComparison(other.Puzzle.Puzzle, Puzzle.Puzzle);
#endif

            _strategies = other._strategies;

            _regionQueues = new List<RegionQueue>(_strategies.Count);

            for (int i = 0; i < _strategies.Count; i++)
            {
                var queue = other._regionQueues[i].Clone();
                changeSet.ApplyToRegionQueue(queue);
                _regionQueues.Add(queue);
            }
        }

        public (SolveResult, Puzzle) Solve()
        {
            var actualSteps = 0;

            while (!Puzzle.Puzzle.IsSolved && Puzzle.Puzzle.IsValid)
            {
#if DEBUG
                if (actualSteps >= 100) // avoid infinite loops
                {
                    break;
                }
#endif

                var (success, _) = Advance();

                if (!success)
                {
                    break;
                }
                actualSteps += 1;
            }

            SolveResult result;

            if (Puzzle.Puzzle.IsSolved)
            {
                result = SolveResult.Success;
            }
            else
            {
                result = !Puzzle.Puzzle.IsValid ? SolveResult.Invalid : SolveResult.Failure;
            }

            return (result, Puzzle.Puzzle);
        }

        public (bool success, string? method) Advance()
        {
            for (int i = 0; i < _strategies.Count; i++)
            {
                var (strat, name) = _strategies[i];
                var workQueue = _regionQueues[i];
#if DEBUG
                Program.Debugger.AddComment($"{name} is considering regions: {workQueue}");
                var oldPuzzle = Puzzle.Clone();
#endif

                var changeSet = Puzzle.Snapshot();
                strat.Apply(Puzzle, workQueue);

                if (changeSet.IsEmpty)
                {
                    continue;
                }
#if DEBUG
                Program.Debugger.AddComment($"{name} changed: {changeSet}");
                Program.Debugger.AddComparison(oldPuzzle, Puzzle.Puzzle);
#endif

                for (int j = 0; j < _regionQueues.Count; j++)
                {
                    changeSet.ApplyToRegionQueue(_regionQueues[j]);
                }

                return (true, name);
            }

            return (false, null);
        }
    }
}
