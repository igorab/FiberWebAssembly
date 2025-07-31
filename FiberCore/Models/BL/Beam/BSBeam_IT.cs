using System.ComponentModel;

namespace BSFiberCore.Models.BL.Beam
{
    /// <summary>
    /// Тавровое-Двутавровое сечение
    /// </summary>
    [Description("size")]
    public class BSBeam_IT : BSBeam
    {
        // размеры:
        [DisplayName("Ширина нижней полки, bf, [см]")]
        public double bf { get; protected set; }
        [DisplayName("Высота нижней полки, hf, [см]")]
        public double hf { get; protected set; }
        [DisplayName("Высота стенки, hw, [см]")]
        public double hw { get; protected set; }
        [DisplayName("Ширина стенки, bw, [см]")]
        public double bw { get; protected set; }
        [DisplayName("Ширина верхней полки, b1f, [см]")]
        public double b1f { get; protected set; }
        [DisplayName("Высота верхней полки, h1f, [см]")]
        public double h1f { get; protected set; }

        [DisplayName("Ширина сечения b, [см]")]
        public override double Width => Math.Max(bf, b1f);
        [DisplayName("Высота сечения , [см]")]
        public override double Height => hf + hw + h1f;

        // Центр тяжести сечения
        public override (double, double) CG() => (Width / 2.0, Height / 2.0);

        /// <summary>
        ///  В обозначениях справочника проектировщика стр 357
        /// </summary>
        public double B => bf;
        public double c_h => hf;
        public override double b => bw;
        public double c_b => b1f;
        public override double h => hf;
        public double a => hw;
        public double H => c_h + c_b + h;
        public double B1 => B - a;
        public double b1 => b - a;

        public override double Area()
        {
            double area = b * c_b + a * h + B * c_h;
            return area;
        }

        public double y_h 
        {
            get => (a*H*H + B1 * c_h*c_h + b1*c_b*(2*H - c_b)) / (2 * (a*H + B1 * c_h + b1 * c_b));
        }

        public double y_b => H - y_h;

        public double h_b => y_b - c_b;

        public double h_n => y_h - c_h;

        public override double Jx()
        {
            double j_x =  1/3d * ( B * Math.Pow(y_h, 3) - B1 * Math.Pow(h_n, 3) + b * Math.Pow(y_b, 3) - b1 * Math.Pow(h_b, 3) );
            return j_x;
        }

        public override double Jy()
        {
            // момент инерции нижнего прямоугольника относительно центральной системы координат
            // Формула вида: Iy1 = Iy + delataY^2 * A,
            // Iy - ОСЕВОЙ момент инерции фигуры;   delataY - осевое смещение;  A - площадь фигуры
            double I_1 = b1f * Math.Pow(h1f,3) / 12 + Math.Pow(hw / 2 + h1f / 2, 2) * b1f * h1f;
            double I_2 = bw * Math.Pow(hw,3) / 12;
            double I_3 = bf * Math.Pow(hf,3) / 12 + Math.Pow(hw / 2 + hf / 2, 2) * bf * hf;
            return I_1 + I_2 + I_3;


        }

        public override double I_s()
        {
            return Jx();
        }


        /// <summary>
        /// Возращает габаритные размеры сечения
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, double> GetDimension()
        {
            Dictionary<string, double> dimensionOfSection = new Dictionary<string, double>()
            {
                { DN(typeof(BSBeam_IT), "bf"), bf },
                { DN(typeof(BSBeam_IT), "hf"), hf },
                { DN(typeof(BSBeam_IT), "hw"), hw },
                { DN(typeof(BSBeam_IT), "bw"), bw },
                { DN(typeof(BSBeam_IT), "b1f"), b1f },
                { DN(typeof(BSBeam_IT), "h1f"), h1f }
            };
            return dimensionOfSection;
        }

        // статические моменты относительно осей 
        // TODO проверить формулы
        public override double Sy() => hf * bf * hf / 2.0 + hw * bw * (hf + hw / 2.0) + h1f * b1f * (Height - h1f / 2.0);
        public override double Sx() => (hf * bf + hw * bw + h1f * b1f) * Width / 2.0; 


        public static Exception SizeError(string _txt)
        {
            return new Exception("Некорректные размеры сечения: " + _txt);
        }

        public override void SetSizes(double[] _t) 
        {
            (bf, hf, bw, hw, b1f, h1f, Length) = (_t[0], _t[1], _t[2], _t[3], _t[4], _t[5], _t[6]);
            
            if (bw <= 0 || hw <= 0) 
                throw SizeError("должен быть положительным");

            if ((bf > 0 &&  bw > bf) || (b1f > 0 && bw > b1f))
                throw SizeError("ширина стенки не может быть больше ширины полки");
        }
    }
}
