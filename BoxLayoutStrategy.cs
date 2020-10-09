﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    /// <summary>
    /// Finds cases where all options for a digit in a row/column are in the
    /// same box. Other occurences of that digit can be removed from the box.
    /// </summary>
    internal class BoxLayoutStrategy : ISolveStrategy
    {
        public (bool, Puzzle) Apply(Puzzle puzzle)
        {
            foreach (var box in puzzle.Boxes)
            {
                var boxIndex = box[0].Box;

                for (int digit = 1; digit <= Puzzle.LineLength; digit++)
                {
                    var value = SudokuValues.FromHumanValue(digit);
                    var rowUnique = true;
                    var colUnique = true;
                    var row = -1;
                    var col = -1;

                    for (var i = 0; i < Puzzle.LineLength;i++)
                    {
                        var cell = box[i];
                        
                        if (!cell.IsResolved && cell.Value.HasAnyOptions(value))
                        {
                            if (row == -1 || cell.Row == row)
                            {
                                row = cell.Row;
                            }
                            else
                            {
                                rowUnique = false;
                            }

                            if (col == -1 || cell.Column == col)
                            {
                                col = cell.Column;
                            }
                            else
                            {
                                colUnique = false;
                            }
                        }
                    }

                    if (rowUnique && row != -1)
                    {
                        var region = puzzle.GetRow(row);
                        var otherCells = region.Where(c => c.Box != boxIndex);
                        var updates = new List<Cell>();

                        foreach (var cell in otherCells)
                        {
                            if (cell.HasOptions(value))
                            {
                                updates.Add(cell.RemoveOptions(value));
                            }
                        }

                        if (updates.Any())
                        {
                            Program.HighlightDigit = digit;
                            Program.DebugText = $"{digit}s in {box} remove others in {region}.";
                            return (true, puzzle.UpdateCells(updates));
                        }
                    }
                    if (colUnique && col != -1)
                    {
                        var region = puzzle.GetColumn(col);
                        var otherCells = region.Where(c => c.Box != boxIndex);
                        var updates = new List<Cell>();

                        foreach (var cell in otherCells)
                        {
                            if (cell.HasOptions(value))
                            {
                                updates.Add(cell.RemoveOptions(value));
                            }
                        }

                        if (updates.Any())
                        {
                            Program.HighlightDigit = digit;
                            Program.DebugText = $"{digit}s in {box} remove others in {region}.";
                            return (true, puzzle.UpdateCells(updates));
                        }
                    }
                }
            }

            return (false, puzzle);
        }
    }
}
