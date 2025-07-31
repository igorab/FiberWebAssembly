using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.IO;
using TriangleNet.Meshing;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Rendering.Text;
using TriangleNet.Topology;

namespace BSFiberCore.Models.BL.Tri
{
    public class BSMesh
    {
        public static int Nx { get; set; }
        public static int Ny { get; set; }
        public static double MinAngle { get; set; }
        public static double MaxArea { get; set; }
        public static Mesh? Mesh { get; set; }
        public static Point Center { get; set; }
        public static string FilePath { get; set; }

        public BSMesh()
        {
            Nx = 2;
            Ny = 2;
            MinAngle = 30;
            MaxArea = 10;
            Center = new Point(0, 0);

            FilePath = Path.Combine(Environment.CurrentDirectory, "Templates");
        }


        static BSMesh()
        {
            Nx = 2;
            Ny = 2;
            MinAngle = 30;
            MaxArea = 10;
            Center = new Point(0, 0);

            FilePath = Path.Combine(Environment.CurrentDirectory, "Templates");
        }

        public static string GenerateRectangle(List<double> _points)
        {
            double x, y, w, h;
            (x, y, w, h) = (_points[0], _points[1], _points[2], _points[3]);

            try
            {
                // Create unit square.
                Rectangle bounds = new Rectangle(x, y, w, h);

                // Generate mesh.
                Mesh = GenericMesher.StructuredMesh(bounds, Nx, Ny) as Mesh;

                string svgPath = Path.Combine(FilePath, "rectangle1.svg");

                SvgImage.Save(Mesh, svgPath, 800);

                int cnt = Mesh.Triangles.Count;

                foreach (Triangle tr in Mesh.Triangles)
                {
                    var a = tr.Area;
                }

                return svgPath;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Create a circular contour.
        /// </summary>
        /// <param name="r">The radius.</param>
        /// <param name="center">The center point.</param>
        /// <param name="h">The desired segment length.</param>
        /// <param name="label">The boundary label.</param>
        /// <returns>A circular contour.</returns>
        public static Contour Circle(double r, Point center, double h, int label = 0)
        {
            int n = (int)h; // (int)(2 * Math.PI * r / h);

            var points = new List<Vertex>(n);

            double x, y, dphi = 2 * Math.PI / n;

            for (int i = 0; i < n; i++)
            {
                x = center.X + r * Math.Cos(i * dphi);
                y = center.Y + r * Math.Sin(i * dphi);

                points.Add(new Vertex(x, y, label));
            }

            return new Contour(points, label, true);
        }

        public static string GenerateRing(double _R, double _r, bool print = false)
        {
            // Generate the input geometry.
            double h = Nx;  //(_R - _r) / 2.0;
            var poly = CreateRing(_R, _r, h);

            // Set minimum angle quality option.
            var quality = new QualityOptions()
            {
                MinimumAngle = MinAngle,
                //MaximumArea = MaxArea 
            };

            // Generate mesh using the polygons Triangulate extension method.
            Mesh = poly.Triangulate(quality) as TriangleNet.Mesh;

            string svgPath = "";
            if (print)
            {
                svgPath = Path.Combine(FilePath, "Ring.svg");
                SvgImage.Save(Mesh, svgPath, 500);
            }

            return svgPath;
        }

        public static IPolygon CreateRing(double _R, double _r, double h = 0.2)
        {
            // Generate the input geometry.
            Polygon poly = new Polygon();

            // Center point.
            Point center = new Point(Center.X, Center.Y);

            // Inner contour (hole).
            poly.Add(Circle(_r, center, h, 1), center);

            // Internal contour.
            //poly.Add(Circle((_R + _r)/2.0, center, h, 2));

            // Outer contour.
            poly.Add(Circle(_R, center, h, 3));

            return poly;
        }

    }
}
