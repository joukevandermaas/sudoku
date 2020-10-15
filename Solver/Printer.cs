﻿using System.Linq;
using System.Text;

namespace Sudoku
{
    public static class Printer
    {
        public static string ForConsole(Puzzle puzzle, bool printIndices = false)
        {
            // only works for sudokus where the size is a cube
            // │ ┤ ╡ ╢ ╖ ╕ ╣ ║ ╗ ╝ ╜ ╛ ┐ └ ┴ ┬ ├ ─ ┼ ╞ ╟ ╚ ╔ ╩ ╦ ╠ ═ ╬ ╧ ╨ ╤ ╥ ╙ ╘ ╒ ╓ ╫ ╪ ┘ ┌

            var builder = new StringBuilder();

            if (printIndices)
            {
                builder.Append(" ");
                for (int i = 0; i < Puzzle.LineLength; i++)
                {
                    builder.AppendFormat(" {0}  ", i);
                }

                builder.AppendLine();
            }

            builder.Append("╔");
            builder.Append(string.Join('╦',
                Enumerable.Repeat("═══╤═══╤═══", Puzzle.BoxLength))); // todo this only works for 3x3
            builder.AppendLine("╗");
            builder.Append("║");

            var rowNr = 0;

            for (var i = 0; i < Puzzle.LineLength * Puzzle.LineLength; i++)
            {
                if (i != 0)
                {
                    if (i % (Puzzle.BoxLength * Puzzle.LineLength) == 0)
                    {
                        AddEndOfLine();

                        builder.Append("╠");
                        builder.Append(string.Join('╬',
                            Enumerable.Repeat("═══╪═══╪═══", Puzzle.BoxLength))); // todo this only works for 3x3
                        builder.AppendLine("╣");
                    }
                    else if (i % Puzzle.LineLength == 0)
                    {
                        AddEndOfLine();

                        builder.Append("╟");
                        builder.Append(string.Join('╫',
                            Enumerable.Repeat("───┼───┼───", Puzzle.BoxLength))); // todo this only works for 3x3
                        builder.AppendLine("╢");
                    }

                    if (i % Puzzle.BoxLength == 0)
                    {
                        builder.Append("║");
                    }
                    else
                    {
                        builder.Append("│");
                    }
                }

                if (puzzle[i].IsResolved)
                {
                    builder.AppendFormat(" {0} ", puzzle[i].ToString());
                }
                else
                {
                    builder.Append("   ");
                }
            }

            AddEndOfLine();

            builder.Append("╚");
            builder.Append(string.Join('╩',
                Enumerable.Repeat("═══╧═══╧═══", Puzzle.BoxLength))); // todo this only works for 3x3
            builder.AppendLine("╝");

            return builder.ToString();

            void AddEndOfLine()
            {
                builder.Append("║");

                if (printIndices)
                {
                    builder.AppendFormat(" {0}", rowNr);
                }

                builder.AppendLine();

                rowNr += 1;
            }
        }

        public static string ForBrowser(Puzzle puzzle, Puzzle previous, int highlightDigit = 0)
        {
            var builder = new StringBuilder();
            int indentation = 0;

            Indent();
            builder.AppendLine("<div class='container'>");
            indentation++;

            foreach (var box in puzzle.Boxes)
            {
                Indent();
                builder.AppendLine("<section class='box'>");
                indentation++;

                foreach (var cell in box)
                {
                    var hasChanged = cell != previous[cell.Index];

                    Indent();
                    builder.AppendFormat("<div class='cell {0} {1}'>",
                        cell.IsResolved ? "resolved" : "options",
                        hasChanged ? "changed" : "");
                    builder.AppendLine();
                    indentation++;

                    var options = cell.Value.GetOptions();
                    var hasManyOptions = options.Count() > 5;

                    foreach (var opt in options)
                    {
                        var digit = opt.ToHumanValue();

                        if (highlightDigit != 0 && highlightDigit != digit && !cell.IsResolved)
                        {
                            continue;
                        }
                        else if (highlightDigit != 0)
                        {
                            hasManyOptions = false;
                        }

                        Indent();
                        builder.AppendFormat(
                            "<span class='number {0} {1}'>{2}</span>",
                            hasManyOptions ? "many" : "",
                            digit == highlightDigit ? "highlight" : "",
                            digit);
                        builder.AppendLine();
                    }

                    indentation--;
                    Indent();
                    builder.AppendLine("</div>"); // cell
                }

                indentation--;
                Indent();
                builder.AppendLine("</section>"); // box
            }

            indentation--;
            Indent();
            builder.AppendLine("</div>"); // container

            return builder.ToString();

            void Indent()
            {
                builder!.Append(new string(' ', indentation * 2));
            }
        }
    }
}
