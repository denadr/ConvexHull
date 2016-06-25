using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SharpGL;
using SharpGL.Enumerations;
using SharpGL.SceneGraph;

namespace ConvexHull
{
    public partial class MainWindow : Window
    {
        private Vertex[] pointCloud;
        private List<Vertex> hullPoints;

        private bool continueExecution;
        private bool executionStarted;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private Vertex[] GetArbitraryPointCloud()
        {
            int nPoints = 15;
            var points = new Vertex[nPoints];
            var random = new Random();

            for (int n = 0; n < nPoints; n++)
            {
                points[n] = new Vertex(random.Next(100, (int)openGLControl.ActualWidth - 100), random.Next(100, (int)openGLControl.ActualHeight - 100), 0);
            }
            
            return points;
        }

        public async Task<List<Vertex>> ProcessAlgorithm()
        {
            int k = 0;
            var hull = new Vertex[2 * pointCloud.Length];

            // Sort points
            pointCloud = pointCloud.OrderBy(p => p.X).ThenBy(p => p.Y).ToArray();
            
            // Build upper hull
            for (int n = 0; n < pointCloud.Length; n++)
            {
                while (k >= 2 && Cross(hull[k - 2], hull[k - 1], pointCloud[n]) <= 0)
                {
                    await Continue();
                    RemoveHullPoint();

                    k--;
                } 

                hull[k] = pointCloud[n];

                await Continue();
                AddHullPoint(hull[k]);

                k++;
            }

            // Build lower hull
            for (int n = pointCloud.Length - 2, m = k + 1; n >= 0; n--)
            {
                while (k >= m && Cross(hull[k - 2], hull[k - 1], pointCloud[n]) <= 0)
                {
                    await Continue();
                    RemoveHullPoint();

                    k--;
                }

                hull[k] = pointCloud[n];

                await Continue();
                AddHullPoint(hull[k]);

                k++;
            }

            var convexHull = new List<Vertex>();
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

        private void AddHullPoint(Vertex vertex)
        {
            lock (this)
            {
                hullPoints.Add(vertex);
            }
        }

        private void RemoveHullPoint()
        {
            lock (this)
            {
                hullPoints.RemoveAt(hullPoints.Count - 1);
            }
        }

        private double Cross(Vertex o, Vertex a, Vertex b) =>
            (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);

        private void Window_KeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Space)
            {
                if (!executionStarted)
                {
                    executionStarted = true;
                    continueExecution = true;

                    pointCloud = GetArbitraryPointCloud();
                    hullPoints = new List<Vertex>();

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
        
        private void OpenGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            var gl = args.OpenGL;

            // Create an orthographic projection
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadIdentity();
            gl.Ortho(0, openGLControl.ActualWidth, openGLControl.ActualHeight, 0, -10, 10);

            // Back to the modelview
            gl.MatrixMode(MatrixMode.Modelview);
        }

        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            var gl = args.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            // Draw point cloud
            if (pointCloud != null && pointCloud.Length > 0)
            {
                gl.PointSize(10.0f);
                gl.Color(0.99, 0.99, 0.99);
                gl.Begin(BeginMode.Points);
                foreach (var point in pointCloud)
                {
                    gl.Vertex(point);
                }
                gl.End();
            }

            // Draw hull segments
            if (hullPoints != null && hullPoints.Count > 1)
            {
                gl.LineWidth(3.0f);
                gl.Color(0.99, 0.99, 0.99);
                gl.Begin(BeginMode.Lines);
                for (int n = 0; n < hullPoints.Count - 1; n++)
                {
                    gl.Vertex(hullPoints[n]);
                    gl.Vertex(hullPoints[n + 1]);
                }
                gl.End();
            }
        }
    }
}
