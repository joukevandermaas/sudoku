using System;
using System.Collections.Generic;
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
        const string ctc = "000000012000000345000003670000081500000754000004230000067900000312000000850000000";

        static void Main(string[] args)
        {
            // left to right, top to bottom. 0 means empty
            SolveSudoku(easy2);
        }

        static void SolveSudoku(string sudoku)
        {
            var puzzle = new Puzzle(sudoku);

            var solver = new Solver();

            var builder = new StringBuilder();
            builder.AppendLine(_htmlStart);

            while (!puzzle.IsSolved && puzzle.IsValid)
            {
                var (success, newPuzzle, strat) = solver.Advance(puzzle);

                if (success)
                {
                    builder.AppendFormat("<h3>{0}</h3>", strat!.GetType());
                    builder.AppendLine();

                    builder.AppendLine("<div class='step'>");
                    builder.AppendLine(Printer.ForBrowser(puzzle));
                    builder.AppendLine(Printer.ForBrowser(newPuzzle));
                    builder.AppendLine("</div>"); // step

                    puzzle = newPuzzle;

                    builder.AppendLine("<hr>");
                }
                else
                {
                    builder.AppendLine("<div class='step'>");
                    builder.AppendLine(Printer.ForBrowser(puzzle));
                    builder.AppendLine("</div>"); // step
                    builder.AppendLine("<hr>");

                    break;
                }
            }

            builder.AppendLine(puzzle.IsSolved ? "Solved" : puzzle.IsValid ? "Failed" : "Invalid");
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
  margin: 60px;
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
