using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.IO;
using TriangleNet.Meshing;
using TriangleNet.Rendering.Text;
using TriangleNet.Tools;
using TriangleNet.Topology;

namespace BSFiberCore.Models.BL.Tri
{
    public abstract class Tri
    {
        public static string FilePath { get; set; }
        public static double MinAngle { get; set; }
        
        public static List<double> triAreas;

        public static List<Point> triCGs;

        /// <summary>
        /// смещение начала координат
        /// </summary>
        public static Point Oxy { get; set; } 

        public static Mesh Mesh { get; set; }

        static Tri()
        {
            triAreas = new List<double>();
            triCGs = new List<Point>();
            MinAngle = 25.0;            
            Oxy = new Point() {ID = 0, X = 0, Y = 0 };
            FilePath = Path.Combine(Environment.CurrentDirectory, "Templates");
        }

        public bool Contains(double x, double y)
        {
            return false;
        }

        public static double CalculateTriangleArea(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            return Math.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0;
        }

        public static double HeightOfFigure()
        {            
            double h = Mesh.Bounds.Height;                               
            return h;
        }

        public static double WidthOfFigure()
        {
            double w = Mesh.Bounds.Width;
            return w;
        }

        public static (double, double, double, double) Bounds()
        {
            var bounds = Mesh.Bounds;
            
            return (bounds.Left, bounds.Bottom, bounds.Right, bounds.Top);
        }

        /// <summary>
        ///  Центр тяжести
        /// </summary>
        /// <returns>X, Y</returns>
        public static (double, double) СenterOfFigure()
        {
            if (triCGs == null || triCGs.Count == 0) return (0, 0);

            int idx = 0; 
            // статичесеие моменты:
            double Sy= 0, Sx=0;
            // Общая площадь
            double figArea = triAreas.Sum();
            foreach (var _area in triAreas) 
            {                
                Sy += _area * triCGs[idx].X;
                Sx += _area * triCGs[idx].Y;
                idx++;
            }

            double cgX = (figArea != 0)? Sy /figArea  : 0;
            double cgY = (figArea != 0)? Sx /figArea : 0;

            return (cgX, cgY);
        }

        /// <summary>
        /// Момент инерции сечения
        /// </summary>
        /// <returns></returns>
        public static (double, double) MomentOfInertia()
        {
            if (triCGs == null || triCGs.Count == 0) return (0, 0);

            int idx = 0;            
            double figArea = triAreas.Sum();

            double Jx = 0, Jy = 0;
            double cmX, cmY;
            (cmX, cmY) = СenterOfFigure();

            foreach (var _area in triAreas)
            {
                Jx += _area * Math.Pow(triCGs[idx].Y, 2);
                Jy += _area * Math.Pow(triCGs[idx].X, 2);
                idx++;
            }

            double Jx_c = Jx - Math.Pow(cmY, 2) * figArea;
            double Jy_c = Jy - Math.Pow(cmX, 2) * figArea;

            return (Jx_c, Jy_c);
        }

        /// <summary>
        ///  Момент сопротивления сечения 
        /// </summary>
        /// <returns>Wx нижнее, Wx верхнее  Wy левое,  Wy правое</returns>
        public static (double, double, double, double) ModulusOfSection()
        {
            double c_x, c_y;
            (c_x, c_y) = СenterOfFigure();
            
            double h = HeightOfFigure();
            double w = WidthOfFigure();

            double Jx, Jy;
            (Jx, Jy) = MomentOfInertia();

            double left, bottom, right, top;
           (left, bottom, right, top) =  Bounds();

            double Wx_t, Wx_l;
            
            Wx_t = (top - c_y != 0) ?  Jx / (top- c_y) : 0;

            Wx_l = (c_y - bottom != 0) ? Jx / (c_y - bottom) : 0;

            double Wy_l, Wy_r; 

            Wy_l = (c_x- left != 0) ? Jy / (c_x - left) : 0;

            Wy_r = (right - c_x != 0) ? Jy / (right - c_x) : 0;

            return (Wx_t, Wx_l, Wy_l, Wy_r);
        }


        /// <summary>
        /// Расчетная схема сечения
        /// </summary>
        /// <returns>ц.т. треугольников и их площади</returns>
        public static List<object> CalculationScheme(bool bOxy = true)
        {            
            List<object> result = new List<object> { new object() };
            if (Mesh is null) return result;

            HashSet<Rectangle> bounds = new HashSet<Rectangle>();
            triAreas = new List<double>();
            triCGs = new List<Point>();

            int triIdx = 0;
            foreach (Triangle tri in Mesh.Triangles)
            {
                Rectangle rect = tri.Bounds();  // !исправить              
                bounds.Add(rect);
                                
                int vId0 = tri.GetVertexID(0);
                int vId1 = tri.GetVertexID(1);
                int vId2 = tri.GetVertexID(2);

                Vertex v0 = tri.GetVertex(0);
                Vertex v1 = tri.GetVertex(1);
                Vertex v2 = tri.GetVertex(2);

                // ц.т. треугольника - смещение начала координат
                double cg_X;
                double cg_Y;

                if (bOxy)
                {
                    cg_X = Oxy.X - (v0.X + v1.X + v2.X) / 3.0;
                    cg_Y = Oxy.Y - ((v0.Y + v1.Y + v2.Y) / 3.0);
                }
                else
                {
                    cg_X =  (v0.X + v1.X + v2.X) / 3.0;
                    cg_Y =  (v0.Y + v1.Y + v2.Y) / 3.0;
                }

                // Центр тяжести треугольника
                Point triCG = new Point() 
                {
                    ID = triIdx,  
                    X = cg_X, 
                    Y = cg_Y 
                };
                
                triCGs.Add(triCG);

                double trArea = CalculateTriangleArea(v0.X, v0.Y, v1.X, v1.Y, v2.X, v2.Y);

                triAreas.Add(trArea);

                triIdx++;
            }

            result.Add(triAreas);
            result.Add(triCGs);
           
            return result;
        }

        public static Polygon MakePolygon(List<System.Drawing.PointF> _points)
        {
            Polygon poly = new Polygon();

            Contour contour = MakeContour(_points);

            poly.Add(contour);

            return poly;
        }

        private static Contour MakeContour(List<System.Drawing.PointF> _points)
        {
            Vertex[] vrtx = new Vertex[_points.Count];

            int vIdx = 0;
            foreach (var point in _points)
            {
                vrtx[vIdx] = new Vertex(point.X, point.Y, 1);
                vIdx++;
            };

            var contour = new Contour(vrtx, 1);
            return contour;
        }


        /// <summary>
        /// Сформировать контур сечения
        /// </summary>
        /// <param name="_points">координаты точек</param>
        /// <returns>Путь к файлу</returns>
        public static string CreateSectionContour(List<System.Drawing.PointF> _points, double _MaxArea)
        {
            if (_points.Count == 0) return "";

            Polygon poly = MakePolygon(_points);
            
            ConstraintOptions options = new ConstraintOptions() { ConformingDelaunay = true };
            
            QualityOptions quality = new QualityOptions()
            {
                MinimumAngle = MinAngle
                //,VariableArea = false
            };
            quality.UseLegacyRefinement = true;
            quality.MaximumAngle = 180;
            if (_MaxArea > 0)
                quality.MaximumArea = _MaxArea;
            
            Mesh = poly.Triangulate(options, quality) as Mesh;

            var statistic = new Statistic();
            statistic.Update(Mesh, 1);

            // Refine by setting a custom maximum area constraint.
            
            Mesh.Refine(quality);
            
            //var smoother = new SimpleSmoother();
            //smoother.Smooth(Mesh, 5);
            string svgPath = Path.Combine(FilePath, "IBeam.svg");

            SvgImage.Save(Mesh, svgPath, 800);

            string polyPath = Path.Combine(FilePath, "IBeam.poly");
            FileProcessor.Write(Mesh, polyPath);

            return svgPath;
        }
    }

    //Polygon

    // Creating a polygon

    // Using contours
    public class TriPoly : Tri
    {
        public static void Example()
        {
            var p = new Polygon();

            Contour cont_outer = new Contour(new Vertex[4]
                {
                    new Vertex(0.0, 0.0, 1),
                    new Vertex(3.0, 0.0, 1),
                    new Vertex(3.0, 3.0, 1),
                    new Vertex(0.0, 3.0, 1)
                });

            //cont_outer.FindInteriorPoint()


            // Add the outer box contour with boundary marker 1.
            p.Add(cont_outer, 1);

            // Add the inner box contour with boundary marker 2.
            p.Add(new Contour(new Vertex[4]
            {
                new Vertex(1.0, 1.0, 2),
                new Vertex(2.0, 1.0, 2),
                new Vertex(2.0, 2.0, 2),
                new Vertex(1.0, 2.0, 2)
                }, 2)
            , new Point(1.5, 1.5)); // Make it a hole.
            
        }
    }

    // Using segments
    public class TriSegment : Tri
    {
        public static void Example()
        {
            var p = new Polygon();

            var v = new Vertex[4]
            {
                new Vertex(0.0, 0.0, 1),
                new Vertex(3.0, 0.0, 1),
                new Vertex(3.0, 3.0, 1),
                new Vertex(0.0, 3.0, 1)
            };

            // Add segments of the outer box.
            p.Add(new Segment(v[0], v[1], 1), 0);
            p.Add(new Segment(v[1], v[2], 1), 0);
            p.Add(new Segment(v[2], v[3], 1), 0);
            p.Add(new Segment(v[3], v[0], 1), 0);

            v = new Vertex[4]
            {
                new Vertex(1.0, 1.0, 2),
                new Vertex(2.0, 1.0, 2),
                new Vertex(2.0, 2.0, 2),
                new Vertex(1.0, 2.0, 2)
            };

            // Add segments of the inner box.
            p.Add(new Segment(v[0], v[1], 2), 0);
            p.Add(new Segment(v[1], v[2], 2), 0);
            p.Add(new Segment(v[2], v[3], 2), 0);
            p.Add(new Segment(v[3], v[0], 2), 0);

            // Add the hole.
            p.Holes.Add(new Point(1.5, 1.5));
        }
    }
}
