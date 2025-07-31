using System.Drawing;

namespace BSFiberCore.Models.BL.Sec
{
    public class BSSectionChart
    {
        public PointF Center { get; set; }

        private double NumArea;
        private float width;
        private float height;
        public double CF_X; // ц.т. фигуры
        public double CF_Y; // ц.т. фигуры
        public double J_X;
        public double J_Y;
        public double W_X_top;
        public double W_X_low;
        public double W_Y_left;
        public double W_Y_right;

        public string GenerateMesh(double maxArea)
        {
            List<PointF> pts = new List<PointF>();

            BeamSectionFromPoints(ref pts, Center);

            string pathToSvgFile = Tri.Tri.CreateSectionContour(pts, maxArea);

            _ = Tri.Tri.CalculationScheme(false);

            // центры тяжести треугольников
            int? nTri = Tri.Tri.triCGs?.Count();

            if (nTri > 0)
            {
                // площади треугольников
                NumArea = Tri.Tri.triAreas?.Sum() ?? 0;

                width = (float)Tri.Tri.WidthOfFigure();

                height = (float)Tri.Tri.HeightOfFigure();

                (CF_X, CF_Y) = Tri.Tri.СenterOfFigure();

                (J_X, J_Y) = Tri.Tri.MomentOfInertia();

                (W_X_low, W_X_top, W_Y_left, W_Y_right) = Tri.Tri.ModulusOfSection();
            }

            return pathToSvgFile;
        }

        private void BeamSectionFromPoints(ref List<PointF> pts, object center)
        {
            throw new NotImplementedException();
        }
    }
}
