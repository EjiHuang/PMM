using PMM.Framwork;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PMM.View
{
    /// <summary>
    /// CaptureMarkView.xaml 的交互逻辑
    /// </summary>
    public partial class CaptureMarkView : Window
    {
        private double x;
        private double y;
        private double width;
        private double height;
        private bool isMouseDown = false;
        public Bitmap bitmap;


        public CaptureMarkView()
        {
            InitializeComponent();
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            x = e.GetPosition(null).X;
            y = e.GetPosition(null).Y;
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                double curx = e.GetPosition(null).X;
                double cury = e.GetPosition(null).Y;

                System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle
                {
                    Stroke = System.Windows.Media.Brushes.White,
                    Fill = System.Windows.Media.Brushes.White,
                    StrokeThickness = 1,
                    Width = Math.Abs(curx - x),
                    Height = Math.Abs(cury - y)
                };

                cnv.Children.Clear();
                cnv.Children.Add(r);
                Canvas.SetLeft(r, Math.Min(x, curx));
                Canvas.SetTop(r, Math.Min(y, cury));

                if (e.LeftButton == MouseButtonState.Released)
                {
                    cnv.Children.Clear();
                    Hide();
                    width = Math.Abs(curx - x);
                    height = Math.Abs(cury - y);

                    bitmap = ScreenShotMaker.CaptureScreen(width, height, Math.Min(x, curx) - 7, Math.Min(y, cury) - 7);

                    x = y = 0;
                    isMouseDown = false;
                    Close();
                }
            }
        }
    }
}
