using BasicRender.Engine;
using BasicRender.MathPrim;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace BasicRender {

    public struct OneLineCacheStruct {
        public long data1;
        public long data2;
        public long data3;
        public long data4;
        public long data5;
        public long data6;
        public long data7;
        public long data8;
    }

    public struct pLine {
        public pLine(int x0, int y0, int x1, int y1) {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
        }
        public int x0;
        public int y0;
        public int x1;
        public int y1;
    }

    public struct pBGRA {
        public pBGRA(int blue, int green, int red, int alpha) {
            this.blue = blue;
            this.green = green;
            this.red = red;
            this.alpha = alpha;
        }
        public int blue;
        public int green;
        public int red;
        public int alpha;
    }

    public partial class MainWindow : Window {

        private GameTimer _timer = new GameTimer();

        private WriteableBitmap _wbStat;
        private Int32Rect _rectStat;
        private byte[] _pixelsStat;
        private int _strideStat;
        private int _pixelWidthStat;
        private int _pixelHeightStat;

        private WriteableBitmap _wb;
        private Int32Rect _rect;
        private byte[] _pixels;
        private int _stride;
        private int _pixelWidth;
        private int _pixelHeight;

        public MainWindow() {
            InitializeComponent();
        }

        static void printLine(byte[] buf, pLine lineCoords, pBGRA color, int pixelWidth) {

            int stride = (pixelWidth * 32) / 8;
            int pixelHeight = buf.Length / stride;

            int x0 = lineCoords.x0;
            int y0 = lineCoords.y0;
            int x1 = lineCoords.x1;
            int y1 = lineCoords.y1;

            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;

            int dy = Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;

            int err = (dx > dy ? dx : -dy) / 2;
            int e2;

            for (; ; ) {

                if (!(x0 >= pixelWidth || y0 >= pixelHeight || x0 < 0 || y0 < 0))
                    printPixel(buf, x0, y0, color, pixelWidth);

                if (x0 == x1 && y0 == y1)
                    break;

                e2 = err;

                if (e2 > -dx) {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dy) {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        static void printPixel(byte[] buf, int x, int y, pBGRA color, int pixelWidth) {

            int blue = color.blue;
            int green = color.green;
            int red = color.red;
            int alpha = color.alpha;

            int pixelOffset = (x + y * pixelWidth) * 32 / 8;
            buf[pixelOffset] = (byte)blue;
            buf[pixelOffset + 1] = (byte)green;
            buf[pixelOffset + 2] = (byte)red;
            buf[pixelOffset + 3] = (byte)alpha;
        }

        static void fillScreen(byte[] buf, pBGRA color, int pixelWidth) {

            int stride = (pixelWidth * 32) / 8;
            int pixelHeight = buf.Length / stride;

            for (int y = 0; y < pixelHeight; y++)
                for (int x = 0; x < pixelWidth; x++)
                    printPixel(buf, x, y, color, pixelWidth);
        }

        static void lmoveScreen(byte[] buf, pBGRA fillColor, int moveAmt, int pixelWidth) {

            int stride = (pixelWidth * 32) / 8;
            int pixelHeight = buf.Length / stride;

            for (int y = 0; y < pixelHeight; y++) {
                for (int x = 0; x < pixelWidth; x++) {

                    int nextPixel = x + moveAmt;
                    if (nextPixel < pixelWidth) {
                        int pixelOffset = (nextPixel + y * pixelWidth) * 32 / 8;
                        printPixel(buf, x, y, new pBGRA(buf[pixelOffset], buf[pixelOffset + 1], buf[pixelOffset + 2], buf[pixelOffset + 3]), pixelWidth);
                    }
                    else {
                        printPixel(buf, x, y, fillColor, pixelWidth);
                    }
                }
            }
        }

        private void Window_Initialized(object sender, EventArgs e) {

            _timer.reset();
            _timer.start();

            _pixelWidth = (int)img.Width;
            _pixelHeight = (int)img.Height;

            _wb = new WriteableBitmap(_pixelWidth, _pixelHeight, 96, 96, PixelFormats.Bgra32, null);
            _rect = new Int32Rect(0, 0, _pixelWidth, _pixelHeight);
            _pixels = new byte[_pixelWidth * _pixelHeight * _wb.Format.BitsPerPixel / 8];

            fillScreen(_pixels, new pBGRA(128, 128, 128, 255), _pixelWidth);
            printLine(_pixels, new pLine(0, 0, 64, 56), new pBGRA(195, 94, 65, 255), _pixelWidth);

            _stride = (_wb.PixelWidth * _wb.Format.BitsPerPixel) / 8;
            _wb.WritePixels(_rect, _pixels, _stride, 0);

            img.Source = _wb;

            InitializeStats();

            CompositionTarget.Rendering += UpdateChildren;
        }

        private void InitializeStats() {

            _pixelWidthStat = (int)statImg.Width;
            _pixelHeightStat = (int)statImg.Height;

            _wbStat = new WriteableBitmap(_pixelWidthStat, _pixelHeightStat, 96, 96, PixelFormats.Bgra32, null);
            _rectStat = new Int32Rect(0, 0, _pixelWidthStat, _pixelHeightStat);
            _pixelsStat = new byte[_pixelWidthStat * _pixelHeightStat * _wbStat.Format.BitsPerPixel / 8];

            fillScreen(_pixelsStat, new pBGRA(32, 32, 32, 255), _pixelWidthStat);

            _strideStat = (_wbStat.PixelWidth * _wbStat.Format.BitsPerPixel) / 8;
            _wbStat.WritePixels(_rectStat, _pixelsStat, _strideStat, 0);

            statImg.Source = _wbStat;
        }

        private float _tt = 0.0f;
        private float _angle = 0.0f;

        protected void UpdateChildren(object sender, EventArgs e) {

            RenderingEventArgs renderingArgs = e as RenderingEventArgs;
            _timer.tick();

            float duration = _timer.deltaTime();

            _tt += duration;
            if (_tt > 1.0f)
                _tt = 0.0f;

            int blue = (int)(255.0 * _tt);
            int green = (int)(255.0 * _tt);
            int red = (int)(255.0 * _tt);

            fillScreen(_pixels, new pBGRA(blue, green, red, 255), _pixelWidth);

            Vec4f[] cube = new Vec4f[8] {
                new Vec4f(-1.0f,  1.0f, 1.0f, 0.0f),
                new Vec4f( 1.0f,  1.0f, 1.0f, 0.0f),
                new Vec4f( 1.0f, -1.0f, 1.0f, 0.0f),
                new Vec4f(-1.0f, -1.0f, 1.0f, 0.0f),
                new Vec4f(-1.0f,  1.0f, -1.0f, 0.0f),
                new Vec4f( 1.0f,  1.0f, -1.0f, 0.0f),
                new Vec4f( 1.0f, -1.0f, -1.0f, 0.0f),
                new Vec4f(-1.0f, -1.0f, -1.0f, 0.0f)
            };

            Mat4f model = new Mat4f(
                new Vec4f(1.0f, 0.0f, 0.0f, 0.0f),
                new Vec4f(0.0f, 1.0f, 0.0f, 0.0f),
                new Vec4f(0.0f, 0.0f, 1.0f, 0.0f),
                new Vec4f(0.0f, 0.0f, 0.0f, 1.0f)
            );

            model = model * Mat4f.RotationXMatrix(_angle);
            //model = model * Mat4f.RotationYMatrix(_angle);
            model = model * Mat4f.RotationZMatrix(_angle);

            Mat4f view = new Mat4f(
                new Vec4f(0.625f, 0.0f, 0.0f, 0.0f),
                new Vec4f(0.0f, 1.0f, 0.0f, 0.0f),
                new Vec4f(0.0f, 0.0f, 1.0f, 0.0f),
                new Vec4f(0.0f, 0.0f, 4.0f, 1.0f)
            );

            Mat4f modelView = model * view;

            Mat4f proj = Mat4f.ProjectionMatrix4(60.0f, 0.1f, 1000.0f);

            Vec4f point1 = modelView * cube[0];
            point1 = proj * point1;
            point1.x = (point1.x + 1.0f) / 2.0f * 320.0f;
            point1.y = (point1.y + 1.0f) / 2.0f * 200.0f;

            Vec4f point2 = modelView * cube[1];
            point2 = proj * point2;
            point2.x = (point2.x + 1.0f) / 2.0f * 320.0f;
            point2.y = (point2.y + 1.0f) / 2.0f * 200.0f;

            Vec4f point3 = modelView * cube[2];
            point3 = proj * point3;
            point3.x = (point3.x + 1.0f) / 2.0f * 320.0f;
            point3.y = (point3.y + 1.0f) / 2.0f * 200.0f;

            Vec4f point4 = modelView * cube[3];
            point4 = proj * point4;
            point4.x = (point4.x + 1.0f) / 2.0f * 320.0f;
            point4.y = (point4.y + 1.0f) / 2.0f * 200.0f;

            Vec4f point5 = modelView * cube[4];
            point5 = proj * point5;
            point5.x = (point5.x + 1.0f) / 2.0f * 320.0f;
            point5.y = (point5.y + 1.0f) / 2.0f * 200.0f;

            Vec4f point6 = modelView * cube[5];
            point6 = proj * point6;
            point6.x = (point6.x + 1.0f) / 2.0f * 320.0f;
            point6.y = (point6.y + 1.0f) / 2.0f * 200.0f;

            Vec4f point7 = modelView * cube[6];
            point7 = proj * point7;
            point7.x = (point7.x + 1.0f) / 2.0f * 320.0f;
            point7.y = (point7.y + 1.0f) / 2.0f * 200.0f;

            Vec4f point8 = modelView * cube[7];
            point8 = proj * point8;
            point8.x = (point8.x + 1.0f) / 2.0f * 320.0f;
            point8.y = (point8.y + 1.0f) / 2.0f * 200.0f;

            printLine(
                _pixels,
                new pLine(
                    (int)point1.x,
                    (int)point1.y,
                    (int)point2.x,
                    (int)point2.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point2.x,
                    (int)point2.y,
                    (int)point3.x,
                    (int)point3.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point3.x,
                    (int)point3.y,
                    (int)point4.x,
                    (int)point4.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point4.x,
                    (int)point4.y,
                    (int)point1.x,
                    (int)point1.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point5.x,
                    (int)point5.y,
                    (int)point6.x,
                    (int)point6.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point6.x,
                    (int)point6.y,
                    (int)point7.x,
                    (int)point7.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point7.x,
                    (int)point7.y,
                    (int)point8.x,
                    (int)point8.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point8.x,
                    (int)point8.y,
                    (int)point5.x,
                    (int)point5.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point1.x,
                    (int)point1.y,
                    (int)point5.x,
                    (int)point5.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point2.x,
                    (int)point2.y,
                    (int)point6.x,
                    (int)point6.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point3.x,
                    (int)point3.y,
                    (int)point7.x,
                    (int)point7.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            printLine(
                _pixels,
                new pLine(
                    (int)point4.x,
                    (int)point4.y,
                    (int)point8.x,
                    (int)point8.y
                ),
                new pBGRA(195, 94, 65, 255),
                _pixelWidth
            );

            _angle += (float)(Math.PI / 32.0f);


            _wb.WritePixels(_rect, _pixels, _stride, 0);
            updateStats();
        }

        private void updateStats() {

            float duration = _timer.deltaTime();
            float totalTime = _timer.gameTime();
            int iduration = (int)(duration * 1000.0f);

            statsText.Text = $"RenderDuration: {duration * 1000.0f:F2}ms; FPS: {1.0f / duration:F0}; TotalTime: {totalTime:F3}sec";

            lmoveScreen(_pixelsStat, new pBGRA(32, 32, 32, 255), 1, _pixelWidthStat);
            if (iduration < 32)
                printPixel(_pixelsStat, 319, iduration, new pBGRA(0, 255, 0, 255), _pixelWidthStat);
            _wbStat.WritePixels(_rectStat, _pixelsStat, _strideStat, 0);
        }
    }
}
