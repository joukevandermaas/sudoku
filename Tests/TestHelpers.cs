using Sudoku;
using System.Diagnostics;
using System.IO;

namespace Tests
{
    public static class TestHelpers
    {
        public static void OpenAsHtml(Puzzle puzzle, int highlight = 0)
        {
            var html = Program._htmlStart + Printer.ForBrowser(puzzle, puzzle, highlight) + Program._htmlEnd;
            var file = Path.Combine(Path.GetTempPath(), "__sudoku.html");

            File.WriteAllText(file, html);

            var proc = new Process();
            proc.StartInfo.FileName = "explorer";
            proc.StartInfo.Arguments = $"\"{file}\"";
            proc.Start();
        }

        public static Puzzle GetEmptyPuzzle()
        {
            return Puzzle.FromString(new string('0', Puzzle.LineLength * Puzzle.LineLength));
        }
    }
}
