using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Sudoku
{
    class Program
    {
        const string veryEasy = "090000070200070005500106008008907100000050000009308400600705004900060002040000010";
        const string moderate = "340600000007000000020080570000005000070010020000400000036020010000000900000007082";
        const string easy2 = "094000130000000000000076002080010000032000000000200060000050400000008007006304008";
        const string ctc1 = "000000012000000345000003670000081500000754000004230000067900000312000000850000000";
        const string hard = "020500700600090000000000100010400002000083000070000000309000080000100000800000000";
        const string ctc2 = "000000000009800007080060050050040030007900002000000000002700009040050060300006200";
        const string ctc3 = "000490000008020004060500070054000600000000000009000580030002090500080300000073000";
        const string xwing = "600090007040007100002800050800000090000070000030000008050002300004500020900030504";
        const string swordfish = "070040200000300079506090400000400050007000300030008000001060703760009000002010080";

        public static int HighlightDigit = 0;
        public static string DebugText = string.Empty;

        private static ISolveStrategy[] _strategies = new ISolveStrategy[]
        {
            new SingleStrategy(),
            new OneOptionStrategy(),
            new TupleStrategy(),
            new BoxLayoutStrategy(),
            new SwordfishStrategy(),
        };

        static void Main(string[] args)
        {
            const string puzzle = ctc3;

            if (args.Any(a => a.Contains("debug")) || Debugger.IsAttached)
            {
                SolveDebug(puzzle);
            }
            else
            {
                SolveFast(puzzle);
            }
        }

        static void SolveFast(string sudoku)
        {
            var puzzle = new Puzzle(sudoku);
            var solver = new Solver(maxSteps: 100, maxBruteForceDepth: 3, _strategies);

            var time = Stopwatch.StartNew();
            var (result, solvedPuzzle) = solver.Solve(puzzle);
            time.Stop();

            Console.WriteLine(Printer.ForConsole(solvedPuzzle));
            Console.WriteLine("{0} in {1}", result, time.Elapsed);
        }

        static void SolveDebug(string sudoku)
        {
            var puzzle = new Puzzle(sudoku);

            var solver = new Solver(maxSteps: 100, maxBruteForceDepth: 0, _strategies);

            var builder = new StringBuilder();
            builder.AppendLine(_htmlStart);

            int step = 1;
            while (!puzzle.IsSolved && puzzle.IsValid)
            {
                HighlightDigit = 0;
                DebugText = string.Empty;

                var (success, newPuzzle, strat) = solver.Advance(puzzle);

                if (success)
                {
                    builder.AppendFormat("<h3>{0}. {1}</h3>", step, strat!.GetType());
                    if (!string.IsNullOrEmpty(DebugText))
                    {
                        builder.AppendFormat("<p>{0}</p>", DebugText);
                    }
                    builder.AppendLine();

                    builder.AppendLine("<div class='step'>");
                    builder.AppendLine(Printer.ForBrowser(puzzle, puzzle, HighlightDigit));
                    builder.AppendLine(Printer.ForBrowser(newPuzzle, puzzle));
                    builder.AppendLine("</div>"); // step

                    puzzle = newPuzzle;

                    builder.AppendLine("<hr>");
                }
                else
                {
                    puzzle = newPuzzle;
                    break;
                }

                step++;
            }

            builder.AppendFormat("<h3>{0}</h3>", puzzle.IsSolved ? "Solved" : puzzle.IsValid ? "Failed" : "Invalid");
            for (int i = 1; i <= Puzzle.LineLength; i++)
            {
                builder.AppendLine("<div class='step'>");
                builder.AppendLine(Printer.ForBrowser(puzzle, puzzle, highlightDigit: i));
                builder.AppendLine("</div>"); // step
            }

            builder.AppendLine(_htmlEnd);

            File.WriteAllText("test.html", builder.ToString());
        }

        private const string _htmlStart = @"<!doctype html>

<html>
  <head>
    <title>Sudoku</title>
<style>
* {
  box-sizing: border-box;
}

body {
  font-family: sans-serif;
}

.step {
    display: flex;
    flex-direction: row;
}

.container {
  width: 540px;
  margin: 60px 30px;
  display: grid;
  grid-template-columns: repeat(3, 180px);
  grid-template-rows: repeat(3, 180px);

  border-left: 3px solid black;
  border-top: 3px solid black;
}

.box {
  display: grid;
  grid-template-columns: repeat(3, 60px);
  grid-template-rows: repeat(3, 60px);

  border-right: 3px solid black;
  border-bottom: 3px solid black;
}

.cell {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;

  border-right: 1px solid black;
  border-bottom: 1px solid black;
}

.cell.changed {
  color: #1b74d3;
  font-weight: bold;
}

.highlight {
  color: red;
  font-weight: bold;
}

.resolved .number {
  font-size: 32pt;
}

.options .number {
  font-size: 10pt;
}

.number.many {
  font-size: 6pt;
}

</style>
  </head>
  <body>";

        private const string _htmlEnd = @"
  </body>
</html>";
    }
}
