using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sudoku
{
    public class Program
    {
        const string moderate = "340600000007000000020080570000005000070010020000400000036020010000000900000007082";
        const string easy2 = "094000130000000000000076002080010000032000000000200060000050400000008007006304008";

        const string veryEasy = "090000070200070005500106008008907100000050000009308400600705004900060002040000010";
        const string ctc1 = "000000012000000345000003670000081500000754000004230000067900000312000000850000000";
        const string sunset = "000000000009800007080060050050040030007900002000000000002700009040050060300006200";
        const string trex = "800000200010000090003000008001700003090010060500020100007060004060090010400008500";
        const string ctc3 = "000490000008020004060500070054000600000000000009000580030002090500080300000073000";
        const string ctc4 = "000000000100002300040050060060070010200003800000000007009500000050060070300008200";
        const string xwing = "600090007040007100002800050800000090000070000030000008050002300004500020900030504";
        const string hard = "020500700600090000000000100010400002000083000070000000309000080000100000800000000";
        const string nyt = "000000000000001269200050001000086900050049000000000070038070600005000097090005004";
        const string swordfish = "070040200000300079506090400000400050007000300030008000001060703760009000002010080";
        const string slow = "340600000007000000020080570000005000070010020000400000036020010000000900000007082";
        const string bruteforce = "300060005020000040007000200000607000900080006000901000002000700040000010600050009";
        const string ctc5 = "701030000080706000003050900000402090000070105000005080100000309030000060905000001";

        const string currentPuzzle = "009001087000000400001080060000836020006000000450007006500008009010060002000420000";

#if DEBUG
        internal static PuzzleDebugger Debugger = new PuzzleDebugger();
#endif

        private const bool _profiling = false;

        static void Main(string[] args)
        {
            if (args.Any(a => a.Contains("debug")) || System.Diagnostics.Debugger.IsAttached)
            {
                const string puzzle = currentPuzzle;
                SolveDebug(puzzle);
            }
            else
            {
                var fileName = "top100.txt";

                if (args.Length > 0)
                {
                    fileName = args[0];
                }

                var puzzles = File.ReadAllLines(fileName).Select(p => Puzzle.FromString(p)).ToList();

                Console.WriteLine("Warmup run:");
                SolveMany(puzzles, parallel: false);

                Benchmark("No parallelism", puzzles, false);
                // Benchmark("With parallelism", puzzles, true);
            }
        }

        private static void Benchmark(string title, List<Puzzle> puzzles, bool parallel)
        {
            Console.WriteLine("{0}:", title);
            const int count = _profiling ? 1 : 10;
            var totalTimes = new List<TimeSpan>(count);
            var wallTimes = new List<TimeSpan>(count);

            for (int i = 0; i < count; i++)
            {
                var (total, wall) = SolveMany(puzzles, parallel: parallel);
                totalTimes.Add(total);
                wallTimes.Add(wall);
            }

            var totalMillis = totalTimes.Aggregate(TimeSpan.Zero, (total, current) => total + current).TotalMilliseconds;
            var wallMillis = wallTimes.Aggregate(TimeSpan.Zero, (total, current) => total + current).TotalMilliseconds;
            Console.WriteLine("Average time: {0:000.00}ms (wall time {1:000.00}ms)", totalMillis / count, wallMillis / count);
        }

        private static (TimeSpan total, TimeSpan wall) SolveMany(List<Puzzle> puzzles, bool parallel)
        {
            var results = new (TimeSpan time, bool success)[puzzles.Count];

            var watch = Stopwatch.StartNew();
            if (parallel)
            {
                Parallel.For(0, puzzles.Count, (i) =>
                {
                    var puzzle = puzzles[i];
                    var (time, success, solvedPuzzle) = SolveFast(puzzle);
                    results[i] = (time, success);
                });
            }
            else
            {
                for (var i = 0; i < puzzles.Count; i++)
                {
                    var puzzle = puzzles[i];
                    var (time, success, solvedPuzzle) = SolveFast(puzzle);
                    results[i] = (time, success);
                }
            }
            watch.Stop();

            var totalTime = results.Aggregate(TimeSpan.Zero, (total, current) => total + current.time);
            var successCount = results.Aggregate(0, (total, current) => total += (current.success ? 1 : 0));

            Console.WriteLine("Solved {0:00}% ({1}/{2}) in {3:000.00}ms (wall time {4:000.00}ms)",
                ((double)successCount / puzzles.Count) * 100,
                successCount,
                puzzles.Count,
                totalTime.TotalMilliseconds,
                watch.Elapsed.TotalMilliseconds);

            return (totalTime, watch.Elapsed);
        }

        static (TimeSpan, bool, Puzzle) SolveFast(in Puzzle puzzle)
        {
            var solver = new Solver(puzzle, allowBruteForce: true);

            var time = Stopwatch.StartNew();
            var (result, solvedPuzzle) = solver.Solve();
            time.Stop();

            // if (result != SolveResult.Success) Console.WriteLine(puzzle);

            return (time.Elapsed, result == SolveResult.Success, solvedPuzzle);
        }

        static void SolveDebug(string sudoku)
        {
#if DEBUG
            var puzzle = Puzzle.FromString(sudoku);

            Debugger.Start();

            var solver = new Solver(puzzle, allowBruteForce: true);
            var (result, endState) = solver.Solve();

            Debugger.AddResult(result, endState);
            Debugger.End();

            File.WriteAllText("test.html", Debugger.ToString());

            Console.WriteLine(Printer.ForConsole(endState));
            Console.WriteLine(result);
#endif
        }
    }
}
