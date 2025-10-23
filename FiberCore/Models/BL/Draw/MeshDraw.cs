using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Tri;
using ScottPlot;
using ScottPlot.Colormaps;
using System.Drawing;
using TriangleNet;
using TriangleNet.Topology;

namespace BSFiberCore.Models.BL.Draw
{
    /// <summary>
    /// Класс для отрисовки элементов связанных с сеткой сечения балки.
    /// Для отрисовки используется NuGet ScottPlot
    /// </summary>
    public class MeshDraw
    {
        private ScottPlot.Plot _formsPlot;

        // шаг сетки
        private int Ny; // горизонтальная ось
        private int Nz; // вертикальная ось
        internal double e_st_ult;
        internal double e_s_ult;

        public int MosaicMode { private get; set; }

        /// верхняя граница
        public double UltMax { private get; set; }
        /// нижняя граница
        public double UltMin { private get; set; }
        /// <summary>
        /// предел по арматуре
        /// </summary>
        public double Rs_Ult { private get; set; }

        /// <summary>
        /// Сечение
        /// </summary>
        public List<double> Values_B { private get; set; }
        /// <summary>
        /// Значения для стержней арматуры
        /// </summary>
        public List<double> Values_S { private get; set; }

        /// <summary>
        /// Сетки из треугольников
        /// </summary>
        public Mesh TriangleMesh { get; private set; }

        /// <summary>
        /// ширина для сохранения
        /// </summary>
        private int _widthToSave;
        /// <summary>
        /// высота для сохранения
        /// </summary>
        private int _heightToSave;

        public ColorScale colorsAndScale;

        public MeshDraw(Mesh _triangleMesh)
        {
            _widthToSave = 500;
            _heightToSave = 500;

            TriangleMesh = _triangleMesh;
        }

        public MeshDraw(int _Ny, int _Nz)
        {
            _widthToSave = 500;
            _heightToSave = 500;
            Ny = _Ny;
            Nz = _Nz;

        }
        
        /// <summary>
        /// сохранение объекта FormsPlot на картинке
        /// </summary>
        public bool SaveToPNG(string title = null, string fullPath = null)
        {
            bool save_ok = false;

            try
            {
                if (colorsAndScale != null)
                {
                    Plot myPlot = null;

                    if (title == "Напряжения")
                        myPlot = colorsAndScale.CreateColorScale(MosaicMode, "кг/см2");
                    else
                        myPlot = colorsAndScale.CreateColorScale(MosaicMode);

                    string pathToPicture = "ColorScale.png";

                    myPlot.SavePng(pathToPicture, 100, _heightToSave);

                    // нужно повернуть картинку, иначе она не встает в Plot.Axes.Left.Label.Image
                    Bitmap image = new Bitmap(pathToPicture);

                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    image.Save(pathToPicture, System.Drawing.Imaging.ImageFormat.Png);

                    ScottPlot.Image img1 = new ScottPlot.Image(pathToPicture);
                    
                    save_ok = img1 != null;
                }                
            }
            catch
            {
                save_ok = false;
            }

            return save_ok;
        }
        
        /// <summary>
        /// Треугольные полигоны закрвашиваются цветом в соответсии с maxTension и minTension
        /// </summary>
        /// <param name="_tension"> Значения напряжений для треугольников в соответсвии с TriangleMesh.Triangles</param>
        /// <param name="_maxTension">Предельное значение напряжения</param>
        /// <param name="_minTension">Предельное значение напряжения</param>
        /// <returns></returns>
        public ScottPlot.Plot PaintSectionMesh()
        {
            ScottPlot.Plot formsPlt = new ScottPlot.Plot() { /*Dock = DockStyle.Fill*/ };

            int numOfSegments = 50;
            int maxValueColor = 255;
            double maxValue;
            double minValue;

            if (UltMax >= Values_B.Max())
                maxValue = Values_B.Max(); 
            else 
                maxValue = UltMax; 

            if (UltMin <= Values_B.Min())
                minValue = Values_B.Min(); 
            else 
                minValue = UltMin; 

            double deltaPositive = maxValue / numOfSegments;
            double deltaNegative = minValue / numOfSegments;
            int deltaRGB = maxValueColor / (numOfSegments - 1);

            for (int i = 0; i < TriangleMesh.Triangles.Count; i++)
            {
                // отрисовка гемеотри треугольника
                Triangle tr = TriangleMesh.Triangles.ToArray()[i];

                Coordinates[] points = new Coordinates[3];

                for (int j = 0; j < points.Length; j++)
                {
                    points[j] = new Coordinates(tr.GetVertex(j).X, tr.GetVertex(j).Y);
                }
                ScottPlot.Plottables.Polygon poly = null;// formsPlt.Plot.Add.Polygon(points);

                if (Values_B != null)
                {
                    colorsAndScale.ColorThePolygon(poly, Values_B[i], MosaicMode);
                }
            }

            // formsPlt.Plot.Axes.SquareUnits(); // нет !!! Иначе будут проблемы с определением диапазона отображаемого простроанства 
            _formsPlot = formsPlt;
            return formsPlt;
        }

        /// <summary>
        ///  Покрытие прямоугольниками
        /// </summary>
        /// <param name="sz"></param>
        /// <param name="_bs"></param>
        /// <returns></returns>
        public ScottPlot.Plot CreateRectanglePlot1(double[] sz, BeamSection _bs)
        {
            var msh = new MeshRect(Ny, Nz);

            if (_bs == BeamSection.Rect)
            {               
                msh.Rectangle(sz[0], sz[1], -sz[0]/2.0);
            }
            else if (BSHelper.IsITL(_bs))
            {
                msh.IBeamSection(sz[0], sz[1], sz[2], sz[3], sz[4], sz[5]);
            }
            ScottPlot.Plot formsPlot = new  ScottPlot.Plot() { };
            formsPlot.Axes.SquareUnits();

            int idx = 0;
            int cnt = msh.rectangleFs.Count;

            foreach (RectangleF tr in msh.rectangleFs)
            {
                Coordinates[] points = new Coordinates[]
                {
                    new Coordinates(tr.Left, tr.Bottom),
                    new Coordinates(tr.Right, tr.Bottom),
                    new Coordinates(tr.Right, tr.Top),
                    new Coordinates(tr.Left, tr.Top)
                };

                ScottPlot.Plottables.Polygon poly = formsPlot.Add.Polygon(points);

                if (Values_B != null && Values_B.Count == cnt)
                {
                    double measured_value = Values_B[idx];
                    colorsAndScale.ColorThePolygon(poly, Values_B[idx], MosaicMode);
                }
                idx++;
            }
            _formsPlot = formsPlot;
            return formsPlot;
        }

        /// <summary>
        /// Отрисовать стержень арматуры
        /// </summary>
        public void DrawReinforcementBar(BeamSection numOfBeamSection)
        { 
            // получение арматуры 
            List<BSRod> bsRods = BSData.LoadBSRod(numOfBeamSection);

            if (bsRods ==null || Values_S == null || bsRods.Count != Values_S.Count)
                return; 

            if (_formsPlot == null)
                _formsPlot = new ScottPlot.Plot();

            Plot myPlot = new ScottPlot.Plot();

            for (int i = 0; bsRods.Count > i; i ++)
            {
                double x = bsRods[i].CG_X;
                double y = bsRods[i].CG_Y;

                // place a marker at the point
                var marker = myPlot.Add.Marker(x, y);
                marker.Color = Colors.Black;
                marker.MarkerSize = 19;
                // place a styled text label at the point
                var txt = myPlot.Add.Text($"{i+1}", x, y);
                               
                txt.LabelBold = true;
                txt.LabelFontColor = Colors.White;
              
                // смещение текстовой метки на заданное количество пикселей
                txt.OffsetY = -8;
                if (i < 9)
                {
                    txt.OffsetX = -4;
                }
                else if (i >= 9 && i < 99)
                { 
                    txt.OffsetX = -8;
                }
            }
        }
    }
}
