using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;

namespace ConvexHull
{
    public partial class MainWindow : Window
    {
        private OpenGL gl;

        private Point[] pointCloud;
        private List<Line> hullSegments;

        private bool continueExecution;
        private bool executionStarted;
        
        public MainWindow()
        {
            InitializeComponent();

            pointCloud = GetArbitraryPointCloud();
            hullSegments = new List<Line>();
        }

        private Point[] GetArbitraryPointCloud()
        {
            int nPoints = 15;
            var points = new Point[nPoints];
            var random = new Random();

            for (int n = 0; n < nPoints; n++)
            {
                points[n] = new Point(random.Next(100, 1100), random.Next(100, 700));
            }
            
            return points;
        }

        private void AddLine(Point a, Point b)
        {
            var line = new Line()
            {
                X1 = a.X,
                Y1 = a.Y,
                X2 = b.X,
                Y2 = b.Y,
                Stroke = Brushes.Red,
                StrokeThickness = 5
            };
            hullSegments.Add(line);
            //canvas.Children.Add(line);
        }

        private void RemoveLine()
        {
            var line = hullSegments[hullSegments.Count - 1];
            hullSegments.Remove(line);
            RemoveLine(line);
        }

        private void RemoveLine(Line line) =>
            RemoveLine(new Point(line.X1, line.Y1), new Point(line.X2, line.Y2));

        private void RemoveLine(Point a, Point b)
        {
            //foreach (var child in canvas.Children)
            //{
            //    var line = child as Line;
            //    if (null != line)
            //    {
            //        if ((line.X1 == a.X && line.Y1 == a.Y && line.X2 == b.X && line.Y2 == b.Y) ||
            //            (line.X1 == b.X && line.Y1 == b.Y && line.X2 == a.X && line.Y2 == a.Y))
            //        {
            //            canvas.Children.Remove(line);
            //            break;
            //        }
            //    }
            //}
        }

        public async Task<List<Point>> ProcessAlgorithm()
        {
            int k = 0;
            var hull = new Point[2 * pointCloud.Length];

            // Sort points
            pointCloud = pointCloud.OrderBy(p => p.X).ThenBy(p => p.Y).ToArray();
            
            // Build upper hull
            for (int n = 0; n < pointCloud.Length; n++)
            {
                while (k >= 2 && Cross(hull[k - 2], hull[k - 1], pointCloud[n]) <= 0)
                {
                    await Continue();
                    RemoveLine();

                    k--;
                } 

                hull[k] = pointCloud[n];

                await Continue();
                if (k > 0)
                {
                    AddLine(hull[k - 1], hull[k]);
                }

                k++;
            }

            // Build lower hull
            for (int n = pointCloud.Length - 2, m = k + 1; n >= 0; n--)
            {
                while (k >= m && Cross(hull[k - 2], hull[k - 1], pointCloud[n]) <= 0)
                {
                    await Continue();
                    RemoveLine();

                    k--;
                }

                hull[k] = pointCloud[n];

                await Continue();
                if (k > 0)
                {
                    AddLine(hull[k - 1], hull[k]);
                }

                k++;
            }

            var convexHull = new List<Point>();
            if (k > 1)
            { // Remove non-hull vertices after k, remove k - 1 which is a duplicate
                for (int n = 0; n < k - 1; n++)
                {
                    convexHull.Add(hull[n]);
                }
            }

            executionStarted = false;
            return convexHull;
        }

        private double Cross(Point o, Point a, Point b) =>
            (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);

        private void Window_KeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Space)
            {
                if (!executionStarted)
                {
                    executionStarted = true;
                    continueExecution = true;
                    var ignore = ProcessAlgorithm();
                }
                else
                {
                    lock (this)
                    {
                        continueExecution = true;
                    }
                }
            }
        }

        private async Task<bool> Continue()
        {
            while (!continueExecution)
            {
                await Task.Delay(200);
            }
            continueExecution = false;
            return true;
        }

        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            gl = args.OpenGL;

            //gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            //gl.Enable(OpenGL.GL_DEPTH_TEST);

            //float[] global_ambient = new float[] { 0.5f, 0.5f, 0.5f, 1.0f };
            //float[] light0pos = new float[] { 0.0f, 5.0f, 10.0f, 1.0f };
            //float[] light0ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            //float[] light0diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            //float[] light0specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            //float[] lmodel_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            //gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);

            //gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            //gl.Enable(OpenGL.GL_LIGHTING);
            //gl.Enable(OpenGL.GL_LIGHT0);

            //gl.ShadeModel(OpenGL.GL_SMOOTH);
        }

        //float rotation;
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            //if (null == gl)
            //{
            //    return;
            //}

            //// Clear The Screen And The Depth Buffer
            //gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            //// Move Left And Into The Screen
            //gl.LoadIdentity();
            //gl.Translate(0.0f, 0.0f, -6.0f);

            //gl.Rotate(rotation, 0.0f, 1.0f, 0.0f);

            //Teapot tp = new Teapot();
            //tp.Draw(gl, 14, 1, OpenGL.GL_FILL);

            //rotation += 3.0f;
            var gl = args.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
            gl.Translate(-1.5f, 0.0f, -6.0f);
            gl.PointSize(5.0f);
            gl.Color(0, 1, 0);
            gl.Begin(BeginMode.Points);
            foreach (var point in pointCloud)
            {
                gl.Vertex(point.X, point.Y);
            }
            gl.End();
            
        }
    }
}
