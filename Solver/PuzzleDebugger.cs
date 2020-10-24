using System.Text;

namespace Sudoku
{
#if DEBUG
    internal class PuzzleDebugger
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public PuzzleDebugger()
        {
            _builder.Append(_htmlStart);
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        public void Start() => _builder.Append(_htmlStart);

        public void End() => _builder.Append(_htmlEnd);

        public void AddComparison(Puzzle left, Puzzle right)
        {
            _builder.AppendLine("<div class='step'>");
            _builder.AppendLine(Printer.ForBrowser(left, left));
            _builder.AppendLine(Printer.ForBrowser(left, right));
            _builder.AppendLine("</div>"); // step
        }

        public void AddHeader(string text) => _builder.AppendLine($"<h3>{text}</h3>");

        public void AddComment(string comment) =>
            _builder.AppendLine($"<p class='comment'>{comment}</p>");

        public void AddAction(string action) =>
            _builder.AppendLine($"<p class='action'>{action}</p>");

        public void AddResult(SolveResult result, Puzzle endState)
        {
            _builder.AppendFormat("<h3>{0}</h3>", result);

            _builder.AppendLine("<div class='step'>");
            _builder.AppendLine(Printer.ForBrowser(endState, endState));
            _builder.AppendLine("</div>"); // step

            for (int i = 1; i <= Puzzle.LineLength; i++)
            {
                _builder.AppendLine("<div class='step'>");
                _builder.AppendLine(Printer.ForBrowser(endState, endState, highlightDigit: i));
                _builder.AppendLine("</div>"); // step
            }
        }

        public const string _htmlStart = @"<!doctype html>

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

.comment {
  font-size: 8pt;
  font-style: italic;
}

</style>
  </head>
  <body>";

        public const string _htmlEnd = @"
  </body>
</html>";
    }
#endif
}
