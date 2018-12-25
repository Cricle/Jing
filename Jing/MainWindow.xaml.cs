using Ptg.Drawing;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jing
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Scene Scene;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Ske_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (Scene?.RenderContext?.Bitmap!=null)
            {
                e.Surface.Canvas.Clear();
                e.Surface.Canvas.DrawBitmap(Scene.RenderContext.Bitmap, SKPoint.Empty);
            }
        }

        private void Ske_Loaded(object sender, RoutedEventArgs e)
        {
            Scene = new Scene();
            var l = new SnokeLayout();
            Scene.Layouts.Add(l);
            using (var g=Graphics.FromHwnd(IntPtr.Zero))
            {
                Scene.Dpix = g.DpiX;
                Scene.Dpiy = g.DpiY;
            }
            var snke = File.Open(System.IO.Path.Combine(Environment.CurrentDirectory, "snoke.png"), FileMode.Open);
            var bit= SKBitmap.Decode(snke);
            Scene.TextureManager.Add(new TextureInfo("snoke", bit));
            snke.Dispose();
            Scene.SetSize(Ske.CanvasSize.Width, Ske.CanvasSize.Height);
            Scene.RenderContext.LoadContent();
            new Task(Frame).RunSynchronously();
        }
        private TimeSpan sleep = TimeSpan.FromSeconds(1 / 60f);
        async void Frame()
        {
            byte y = 255;
            byte x = 0;
            var col = new SKColor(y,y,y,x);
            while (true)
            {
                await Task.Delay(sleep);
                Scene.PrepareDraw();
                Scene.RenderContext.Canvas.Clear(SKColor.Empty);
                Scene.Draw();
                Scene.EndDraw();
                Ske.InvalidateVisual();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Application.Current.Shutdown(-1);

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown(-1);
        }
        
    }
    public class SnokeLayout:Layout
    {
        public SnokeLayout()
        {
        }
        private Random Random=new Random();
        public int MaxCount { get; set; } = 500;
        public long WaitTime { get; set; } = TimeSpan.FromMilliseconds(150).Ticks;
        private long beginTime = 0;
        private string key = "snoke";
        protected override void OnPrepareDraw()
        {
            if (Drawings.Count< MaxCount&&Stopwatch.GetTimestamp()-beginTime>=WaitTime)
            {
                var s = Scene.GetSize();
                var sn = new Snoke()
                {
                    X = Random.Next(0, (int)s.Width),
                    Speed = (float)(Random.NextDouble()+1)%3,
                    Size = Random.Next(10, 50),
                    Key=key
                };
                sn.PtgPen.Color.Alpha = (byte)Random.Next(50, 255);
                Drawings.Add(sn);
                beginTime = Stopwatch.GetTimestamp();
            }
            foreach (Snoke item in Drawings)
            {
                item.Y+=item.Speed;
            }
            base.OnPrepareDraw();
        }
        protected override void OnEndDraw()
        {
            var s = Scene.GetSize();
            foreach (Snoke item in Drawings)
            {
                if (item.bitmap!=null)
                {
                    if (item.Y-item.Size>s.Height)
                    {
                        item.Y = -item.Size;
                        item.X = Random.Next(item.Size, (int)s.Width);
                    }
                }
            }
            base.OnEndDraw();
        }
    }
    public class Snoke:DrawingObject
    {
        internal SKBitmap bitmap;
        public float X { get; set; }
        public float Y { get; set; }
        public float Speed { get; set; } = 1;
        public int Size { get; set; } = 40;
        public string Key { get; set; }

        protected override void OnPrepareDraw()
        {
            bitmap = Layout.Scene.TextureManager[Key].Data;
        }
        protected override void OnDraw()
        {
            if (bitmap!=null)
            {
                LayoutCanvas.Canvas.DrawBitmap(bitmap, new SKRect(X, Y, Size+X, Size+Y),Pen);
            }
        }
    }
}
