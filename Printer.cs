using System.Linq;
using System.Text;

namespace Sudoku
{
    static class Printer
    {
        public static string ForConsole(Puzzle puzzle, bool printIndices = false, bool printPossibilities = false)
        {
            // only works for sudokus where the size is a cube
            // │ ┤ ╡ ╢ ╖ ╕ ╣ ║ ╗ ╝ ╜ ╛ ┐ └ ┴ ┬ ├ ─ ┼ ╞ ╟ ╚ ╔ ╩ ╦ ╠ ═ ╬ ╧ ╨ ╤ ╥ ╙ ╘ ╒ ╓ ╫ ╪ ┘ ┌

            var builder = new StringBuilder();
            var cells = puzzle.Cells.ToArray();

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

            for (var i = 0; i < cells.Length; i++)
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

                if (cells[i].IsResolved)
                {
                    builder.AppendFormat(" {0} ", cells[i].ToString());
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

            if (printPossibilities)
            {
                foreach (var cell in cells)
                {
                    builder.AppendFormat("R{0}C{1}: {2}", cell.Row, cell.Column, string.Join("", cell.Value.ToHumanOptions()));
                    builder.AppendLine();
                }
            }

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

        public static string ForBrowser(Puzzle puzzle)
        {
            var builder = new StringBuilder();
            int indentation = 0;

            Indent();
            builder.AppendLine("<div class='container'>");
            indentation++;

            foreach (var box in puzzle.GetBoxes())
            {
                Indent();
                builder.AppendLine("<section class='box'>");
                indentation++;

                foreach (var cell in box)
                {
                    Indent();
                    builder.AppendFormat("<div class='cell {0}'>", cell.IsResolved ? "resolved" : "options");
                    builder.AppendLine();
                    indentation++;

                    var options = cell.Value.GetOptions();
                    var manyOptions = options.Count() > 5;

                    foreach (var opt in options)
                    {
                        Indent();
                        builder.AppendFormat("<span class='number{0}'>{1}</span>", manyOptions ? " many" : "", opt.ToHumanValue());
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
