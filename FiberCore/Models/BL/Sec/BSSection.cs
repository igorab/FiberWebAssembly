using System.Drawing;

namespace BSFiberCore.Models.BL.Sec
{
    public class BSPoint
    {
        public int Num { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public BSPoint()
        {

        }

        public BSPoint(NdmSection _NdmSection)
        {
            Num = _NdmSection.N;
            X = (float) _NdmSection.X;
            Y = (float) _NdmSection.Y;
        }

        public BSPoint(Point _point)
        {
            X = _point.X;
            Y = _point.Y;   
        }

        public BSPoint(PointF _pointF)
        {
            X = _pointF.X;
            Y = _pointF.Y;
        }
    }

    public class BSSection
    {
        public static float bf, hf, bw, hw, b1f, h1f;

        // Защитный слой
        public static float a; 

        static BSSection()
        {
            RodPoints = new List<PointF>();
            a = 4;
        }

        /// <summary>
        /// Геометрия сечения
        /// </summary>
        public static List<PointF> SectionPoints;

        /// <summary>
        /// Расстановка стержней
        /// </summary>
        public static List<PointF> RodPoints;

        /// <summary>
        /// Прямоугольное сечение
        /// </summary>
        /// <param name="_Sz">размеры</param>
        public static void RectangleBeam(double[] _Sz, out PointF _originLeft)
        {
            float[] Sz = Array.ConvertAll(_Sz, element => (float)element);

            float w = Sz[0];
            float h = Sz[1];

            _originLeft = new PointF(-w /2.0f, 0);

            SectionPoints = new List<PointF>()
            {
                new PointF(-w/2.0f, 0) ,
                new PointF( w/2.0f, 0),
                new PointF(w/2.0f, h),
                new PointF(-w/2.0f, h),
                new PointF(-w/2.0f, 0)
            };

            RodPoints = new List<PointF>()
            {
                new PointF(-w/2.0f+a, a),
                new PointF(0, a) ,
                new PointF(w/2.0f-a, a),
            };
        }

        /// <summary>
        ///  Тавровое сечение - геометрия сечения
        /// </summary>        
        public static void IBeam(double[] _Sz, out List<PointF> _PointsSection, out PointF _Center, out PointF _OriginLeft)
        {
            float[] Sz = Array.ConvertAll(_Sz, element => (float)element);

            float bf = Sz[0], hf = Sz[1], bw = Sz[2], hw = Sz[3], b1f = Sz[4], h1f = Sz[5];

            _Center = new PointF(0, (hf + hw + h1f) / 2.0f);

            _OriginLeft = new PointF(-bw / 2.0f, 0);

            _PointsSection = new List<PointF>()
            {
                new PointF(bf/2f, 0),
                new PointF(bf/2f, hf) ,
                new PointF(bw/2f, hf),
                new PointF(bw/2f, hf + hw),
                new PointF(b1f/2f, hf + hw),
                new PointF(b1f/2f, hf + hw + h1f),
                new PointF(-b1f/2f, hf + hw + h1f),
                new PointF(-b1f/2f, hf + hw),
                new PointF(-bw/2f, hf + hw),
                new PointF(-bw/2f, hf),
                new PointF(-bf/2f, hf),
                new PointF(-bf/2f, 0),
                new PointF(bf/2f, 0),
            };

            RodPoints = new List<PointF>()
            {
                new PointF(-bf/2f+a, a),
                new PointF(0, a) ,
                new PointF(bf/2f-a, a),
            };
        }    
    }
}
