﻿using System;

namespace Sudoku
{
    class Program
    {
        const string veryEasy = "090000070200070005500106008008907100000050000009308400600705004900060002040000010";
        const string veryEasy_solved = "896502371210873965573196208068927153132650789759318026621735890987061532305289617";
        const string moderate = "340600000007000000020080570000005000070010020000400000036020010000000900000007082";
        const string easy2 = "094000130000000000000076002080010000032000000000200060000050400000008007006304008";

        static void Main(string[] args)
        {
            // left to right, top to bottom. 0 means empty
            SolveSudoku(moderate);
        }

        static void SolveSudoku(string sudoku)
        {
            var puzzle = new Puzzle(sudoku);
            Console.WriteLine(puzzle);

            var solver = new Solver(puzzle);
            var (result, _) = solver.Solve();

            Console.WriteLine(result);
        }
    }
}
