using System.ComponentModel;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace BSFiberCore.Models.BL.Beam
{
    /// <summary>
    /// Прямоугольник
    /// </summary>
    [Description("size")]
    public class BSBeam_Rect : BSBeam
    {
        //размеры, см
        [DisplayName("Высота сечения, [см]")]
        public override double h { get; set; }
        [DisplayName("Ширина сечения, [см]")]
        public override double b { get; set; }

        [DisplayName("Площадь сечения элемента, [см2]")]
        public override double Area() => b * h;

        [DisplayName("Момент инерции прямоугольного сечения")]
        public override double I_s() => b * Math.Pow(h, 3) / 12;

        [DisplayName("Расстояние от центра тяжести сечения сталефибробетонного элемента до наиболее растянутого волокна, [см]")]
        public double y_t() => h / 2.0;

        public override double Width => b;
        public override double Height => h;
       
        public BSBeam_Rect(double _b = 0, double _h = 0)
        {
            b = _b;
            h = _h;
            Zfb_X = _b / 2.0;
            Z_fb_Y = _h / 2.0;
        }

        /// <summary>
        /// Возращает габаритные размеры сечения
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, double> GetDimension()
        {
            Dictionary<string, double> dimensionOfSection = new Dictionary<string, double>()
            {
                { DN(typeof(BSBeam_Rect), "h"), h },
                { DN(typeof(BSBeam_Rect), "b"), b }
            };
            return dimensionOfSection; 
        }

        // Моменты инерции сечения
        public override double Jy() => b * (h * h * h) / 12.0;

        public override double Jx() => (b * b * b) * h / 12.0;

        //   Моменты сопротивления сечения
        public static double Wx(double _b, double _h) => _b * _h * _h / 6.0;
        public static double Wy(double _b, double _h) => _b * _b * _h / 6.0;

        // статические моменты относительно осей
        public override double Sy() => Area() * h / 2.0;
        public override double Sx() => Area() * b / 2.0;

        public override void SetSizes(double[] _t)
        {
            (b, h, Length) = (_t[0], _t[1], _t[2]);
        }
    }
}
