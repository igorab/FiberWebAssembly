using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Tri;

namespace BSFiberCore.Models.BL.Ndm
{
    public partial class BSCalcNDM
    {
        /// <summary>
        /// Привязка арматуры 
        /// (экранные координаты ц.т. стержней привязываем к с.к. сечения балки)
        /// </summary>
        public static (List<double>, List<double>, List<double>, double, double) 
            ReinforcementBinding(BeamSection _BeamSection, double _leftX, double _leftY, bool _useRebar = true)
        {
            // диаметры
            List<double> rodD = new List<double>();
            //привязка по ширине
            List<double> bY = new List<double>();
            //привязка по высоте
            List<double> hX = new List<double>();
            // количество стержней
            int d_qty = 0;
            // площадь арматуры
            double area_total = 0;

            if (_useRebar)
            {
                // значения из БД
                List<BSRod> _rods = BSData.LoadBSRod(_BeamSection);
                d_qty = _rods.Count;
                foreach (BSRod lr in _rods)
                {
                    area_total += BSHelper.AreaCircle(lr.D);
                }

                foreach (BSRod lrod in _rods)
                {
                    rodD.Add(lrod.D);
                    hX.Add(lrod.CG_Y - _leftY);
                    bY.Add(lrod.CG_X - _leftX);
                }
            }

            return (rodD, hX, bY, d_qty, area_total);
        }

        /// <summary>
        /// Массивы координат продольной арматуры
        /// </summary>        
        public int InitReinforcement(double _y0 = 0, double _z0 = 0 )
        {                        
            // заполнить  массив площадей арматуры            
            foreach (double d in ds)
            {
                As.Add(Math.PI * Math.Pow(d, 2) / 4.0);
            }
            // координаты стержней
            for (int l = 0; l < ds.Count; l++)
            {
                y0s[l] += _y0;
                x0s[l] += _z0;
            }
            // количество элементов арматуры
            int m = As.Count;
            return m;
        }

        /// <summary>
        /// разбить прямоугольное сечение на элементы
        /// </summary>
        /// <param name="_b">ширина</param>
        /// <param name="_h">высота</param>
        /// <param name="_y0">начало координат</param>
        /// <param name="_z0">начало координат</param>
        /// <returns></returns>
        public int InitRectangleSection(double _b, double _h, double _y0 = 0, double _z0 = 0)
        {
            // количество элементов сечения
            int n = ny * nx;
            
            double sy = _b / ny;
            double sx = _h / nx;

            // площадь 1 элемента
            double Ab1 = sy * sx;

            //заполнить массив площадей элементов            
            for (int i = 0; i < n; i++)
                Ab.Add(Ab1);

            //заполнить массив привязок бетонных эл-в к вспомогательной оси y0            
            for (int ix = 0; ix < nx; ix++)
                for (int iy = 0; iy < ny; iy++)
                    y0b.Add( iy * sy + sy / 2.0 + _y0);

            //заполнить массив привязок бетонных эл-в к вспомогательной оси z0            
            for (int ix = 0; ix < nx; ix++)
                for (int iy = 0; iy < ny; iy++)
                    x0b.Add( ix * sx + sx / 2.0 + _z0);
           
            return n;
        }


        // двутавровое сечение
        public int InitIBeamSection(double _bf, double _hf, double _bw, double _hw, double _b1f, double _h1f)
        {
            int n1 = 0, n2 = 0, n3 = 0;

            if (_bf>0 && _hf>0)
                n1 = InitRectangleSection(_bf, _hf, -_bf/2.0, 0);

            n2 = InitRectangleSection(_bw, _hw, -_bw / 2.0, _hf);

            if (_b1f > 0 && _h1f > 0)
                n3 = InitRectangleSection(_b1f, _h1f, -_b1f / 2.0, _hf + _hw);

            return n1+n2+n3;
        }
        
        // кольцевое сечение
        private int InitRingSection(double _r1, double _R2)
        {            
            if (r1 >= R2) throw BSBeam_Ring.RadiiError();              
            
            List<object> Tr = Tri.Tri.CalculationScheme(false);
            // площади треугольников
            var triAreas = Tri.Tri.triAreas;
            // ц.т. треугольников
            var triCGs = Tri.Tri.triCGs;             
            //заполнить массив площадей элементов            
            foreach (var _area in triAreas)
                Ab.Add(_area);

            //заполнить массив привязок бетонных эл-в к вспомогательной оси y0            
            //заполнить массив привязок бетонных эл-в к вспомогательной оси x0            
            foreach (var triCG in triCGs)
            {
                y0b.Add(triCG.X);
                x0b.Add(triCG.Y);
            }
            
            return triAreas.Count; 
        }

        /// <summary>
        /// произвольное сечение
        /// </summary>        
        private int InitAnySection()
        {
            _ = Tri.Tri.CalculationScheme(false);

            // площади треугольников
            var triAreas = Tri.Tri.triAreas;

            // ц.т. треугольников
            var triCGs = Tri.Tri.triCGs;

            //заполнить массив площадей элементов            
            foreach (var _area in triAreas)
                Ab.Add(_area);

            //заполнить массив привязок бетонных эл-в к вспомогательной оси y0            
            //заполнить массив привязок бетонных эл-в к вспомогательной оси x0            
            foreach (var triCG in triCGs)
            {
                y0b.Add(triCG.X);
                x0b.Add(triCG.Y);
            }

            return triAreas.Count;
        }
    }
}
