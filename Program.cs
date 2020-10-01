using System;

namespace Sudoku
{
    class Program
    {
        const string veryEasy = "090000070200070005500106008008907100000050000009308400600705004900060002040000010";
        const string moderate = "340600000007000000020080570000005000070010020000400000036020010000000900000007082";

        static void Main(string[] args)
        {
            // left to right, top to bottom. 0 means empty
            SolveSudoku(veryEasy);
        }

        static void SolveSudoku(string sudoku)
        {
            var puzzle = new Puzzle(sudoku);
            Console.WriteLine(puzzle);

            var solver = new Solver(puzzle);
            solver.Solve();
        }
    }
}
