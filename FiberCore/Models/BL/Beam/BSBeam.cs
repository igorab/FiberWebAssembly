using BSFiberCore.Models.BL.Mat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BSFiberCore.Models.BL.Beam
{
    /// <summary>
    /// Балка
    /// </summary>
    public class BSBeam : IBeamGeometry
    {
        // количество стержней арматуры
        public int RodsQty { get { return (Rods != null) ? Rods.Count : 0; } set { RodsQty = value; } }
        public List<BSRod> Rods { get; set; }

        public BSMatRod MatRod { get { return Rods?.First().MatRod; } }

        // Материал балки (фибробетон, переделать на универсальный)
        public BSMatFiber Mat { get; set; }

        // Координаты Ц.Т.
        public double Zfb_X { get; set; }
        public double Z_fb_Y { get; set; }

        public virtual double h { get; set; }
        public virtual double b { get; set; }

        public double Length { get; set; }

        public virtual double Width { get; }
        public virtual double Height { get; }

        public BSBeam()
        {
        }

        public BSBeam(double _Area, double _W_s, double _I_s, double _Jy, double _Jx, double _Sy, double _Sx)
        {
            any_Area = _Area;
            any_W_s  = _W_s;
            any_I_s  = _I_s;
            any_Jy   = _Jy;
            any_Jx   = _Jx;
            any_Sy   = _Sy;
            any_Sx   = _Sx;
        }

        /// <summary>
        /// Центр тяжести сечения
        /// </summary>
        /// <returns>X, Y</returns>
        public virtual (double, double) CG() => (Width / 2.0, Height / 2.0);


        [DisplayName("Площадь армирования, см2")]
        public double AreaS()
        {
            double? _As = Rods?.Sum(x => x.As);
            return Convert.ToDouble(_As);
        }

        private readonly double any_Area;
        private readonly double any_W_s;
        private readonly double any_I_s;
        private readonly double any_Jy;
        private readonly double any_Jx;
        private readonly double any_Sy;
        private readonly double any_Sx;


        public virtual double Area()
        {
            return any_Area;
        }

        public virtual double W_s()
        {
            return any_W_s;
        }

        public virtual double I_s()
        {
            return any_I_s;
        }

        public virtual double Jy()
        {
            return any_Jy;
        }

        public virtual double Jx()
        {
            return any_Jx;
        }

        /// <summary>
        /// Метода возращает Высоту и Ширину для всех сечений 
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, double> GetDimension()
        {
            Dictionary<string, double> dimensionOfSection = new Dictionary<string, double>()
            {
                { "Высота сечения, h, [см]", h },
                { "Ширина сечения, b, [см]", b }
            };
            return dimensionOfSection;
        }


        /// <summary>
        /// Статический момент относительно оси oY
        /// </summary>
        /// <returns></returns>
        public virtual double Sy()
        {
            return any_Sy;
        }
        /// <summary>
        /// Статический момент относительно оси oX
        /// </summary>
        /// <returns></returns>
        public virtual double Sx()
        {
            return any_Sx;
        }
        
        public virtual double y_t => h / 2; 
               
        public virtual void SetSizes(double[] _t) { }

        /// <summary>
        /// Нормальные напряжения в сечении
        /// </summary>
        /// <param name="_N">кг</param>
        /// <param name="_Mx">кг*см</param>
        /// <param name="_My">кг*см</param>
        /// <param name="_X">см</param>
        /// <param name="_Y">см</param>
        /// <returns>кг/см2</returns>       
        public double Sigma_Z(double _N, double _Mx, double _My, double _X, double _Y)
        {
            double _Jx = Jx();
            double _Jy = Jy();
            double _Area = Area();

            double sgm_z = (_Area >0) ? _N / _Area : 0;
            sgm_z += (_Jx != 0) ? _Mx / _Jx * _X : 0;
            sgm_z += (_Jy != 0) ?  _My / _Jy * _Y : 0 ;

            return sgm_z;
        }

        /// <summary>
        /// Создать экземпляр балки
        /// </summary>
        /// <param name="_BeamSection">Тип сечения</param>
        /// <returns>Балка</returns>
        public static BSBeam construct(BeamSection _BeamSection)
        {
            switch (_BeamSection)
            {
                case BeamSection.Rect:
                    return new BSBeam_Rect();
                case BeamSection.IBeam:
                case BeamSection.LBeam:
                case BeamSection.TBeam:
                    return new BSBeam_IT();
                case BeamSection.Ring:
                    return new BSBeam_Ring();
            }
            return new BSBeam();
        }


        public string DN(Type _T, string _property) => _T.GetProperty(_property).GetCustomAttribute<DisplayNameAttribute>().DisplayName;



    }           
}
