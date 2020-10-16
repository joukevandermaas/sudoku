using Sudoku;
using System.IO;
using Xunit;

namespace Tests
{
    public class IndexablePositionTests
    {
        [Fact]
        public void GetValuesOutAsTheyCameIn()
        {
            var values = GetValues(3, 4, 5);
            var target = new IndexablePosition(values.row, values.col, values.box);

            Assert.Equal(values.row, target.RowValue);
            Assert.Equal(values.col, target.ColumnValue);
            Assert.Equal(values.box, target.BoxValue);

            Assert.Equal(values.row, target.GetValue(RegionType.Row));
            Assert.Equal(values.col, target.GetValue(RegionType.Column));
            Assert.Equal(values.box, target.GetValue(RegionType.Box));
        }

        [Fact]
        public void SetValueUpdatesOnlyThatValue()
        {
            var values = GetValues(4, 5, 6);
            var newValues = GetValues(1, 2, 3);
            var target = new IndexablePosition(values.row, values.col, values.box);

            target = target.SetRowValue(newValues.row);

            Assert.Equal(newValues.row, target.RowValue);
            Assert.Equal(values.col, target.ColumnValue);
            Assert.Equal(values.box, target.BoxValue);

            target = target.SetColumnValue(newValues.col);

            Assert.Equal(newValues.row, target.RowValue);
            Assert.Equal(newValues.col, target.ColumnValue);
            Assert.Equal(values.box, target.BoxValue);

            target = target.SetBoxValue(newValues.box);

            Assert.Equal(newValues.row, target.RowValue);
            Assert.Equal(newValues.col, target.ColumnValue);
            Assert.Equal(newValues.box, target.BoxValue);

            target = target
                .SetValue(RegionType.Row, values.row)
                .SetValue(RegionType.Column, values.col)
                .SetValue(RegionType.Box, values.box);

            Assert.Equal(values.row, target.RowValue);
            Assert.Equal(values.col, target.ColumnValue);
            Assert.Equal(values.box, target.BoxValue);
        }

        private (SudokuValues row, SudokuValues col, SudokuValues box) GetValues(int rowVal, int colVal, int boxVal)
        {
            return (
                SudokuValues.FromHumanValue(rowVal),
                SudokuValues.FromHumanValue(colVal),
                SudokuValues.FromHumanValue(boxVal)
            );
        }
    }
}
