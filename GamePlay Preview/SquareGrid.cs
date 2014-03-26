using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GamePlay_Preview
{
    public class SquareGrid : Grid
    {
        /*
        * This solution is based on: 
        * http://social.msdn.microsoft.com/Forums/vstudio/en-US/adb046b5-a9e1-4f9e-bd10-314c235e5fa1/how-to-make-hexagon-grid-
        */

        #region SquareSideLength

        /// <summary>
        /// SquareSideLength Dependency Property
        /// </summary>
        public static readonly DependencyProperty SqaureSideLengthProperty =
            DependencyProperty.Register("SquareSideLength", typeof(double), typeof(SquareGrid),
                new FrameworkPropertyMetadata((double)0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the SquareSideLength property. This dependency property 
        /// represents the length of 1 side of the hexagon.
        /// </summary>
        public double SquareSideLength
        {
            get { return (double)GetValue(SqaureSideLengthProperty); }
            set { SetValue(SqaureSideLengthProperty, value); }
        }

        #endregion

        #region Rows

        /// <summary>
        /// Rows Dependency Property
        /// </summary>
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(SquareGrid),
                new FrameworkPropertyMetadata((int)1,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the Rows property.
        /// </summary>
        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        #endregion

        #region Columns

        /// <summary>
        /// Columns Dependency Property
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(SquareGrid),
                new FrameworkPropertyMetadata((int)1,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the Columns property.
        /// </summary>
        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        #endregion



        protected override Size MeasureOverride(Size constraint)
        {
            double side = SquareSideLength;
            double width =side;
            double height =  side;
            double colWidth = width;
            double rowHeight = height;

            Size availableChildSize = new Size(width, height);

            foreach (FrameworkElement child in this.InternalChildren)
            {
                child.Measure(availableChildSize);
            }

            double totalHeight = Rows * rowHeight;
            //if (Columns > 1)
            //    totalHeight += (0.5 * rowHeight);
            double totalWidth = Columns * colWidth;
                //+ (0.5 * side);

            Size totalSize = new Size(totalWidth, totalHeight);

            return totalSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {

            double side = SquareSideLength;
            double width = side;
            double height = side;
            double colWidth = 0.98 * width;
            double rowHeight = 0.98 * height;

            Size childSize = new Size(width, height);

            foreach (FrameworkElement child in this.InternalChildren)
            {
                int row = GetRow(child);
                int col = GetColumn(child);

                double left = col * colWidth;
                double top = row * rowHeight;
                
                //    left += colWidth;

                child.Arrange(new Rect(new Point(left, top), childSize));
            }

            return arrangeSize;
        }
    }
}
