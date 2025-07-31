using System.ComponentModel;

namespace BSFiberCore.Models.BL.Beam
{
    /// <summary>
    /// Кольцо
    /// </summary>
    [Description("size")]
    public class BSBeam_Ring : BSBeam
    {
        [DisplayName("Радиус внутренней грани, [см]")]
        public double r1 { get; set; }

        [DisplayName("Радиус наружней грани, [см]")]
        public double r2 { get; set; }

        [DisplayName("Радиус срединной поверхности стенки кольцевого элемента")]
        public double r_m { get { return (r1 + r2) / 2d; } /*private set { r_m = value; }*/ }

        [DisplayName("толщина стенки кольца, см")]
        public double t_r { get => r2 - r1; }

        [DisplayName("Общая площадь кольцевого сечения")]
        public double A_r => Area();

        public double A_s => 9.04;

        // TODO выяснить алгоритм
        public double r_s => 36;

        public override double Width => r2;
        public override double Height => r2;


        public override double Area()
        {
            double t = t_r;
            if (t <= 0) return 0;

            double area = 2 * Math.PI * r_m * t;
            return area;
        }

        // диаметр наружней грани
        public double D { get => 2 * r2; }

        // диаметр внутренней грани
        public double d { get => 2 * r1; }

        public double alfa => d / D;

        public double F => (Math.PI * D * D / 4d) * (1 - Math.Pow(alfa, 4));

        public override double b { get => D - d; }

        public override double h { get => D - d; }

        public override double Jx()
        {
            double jx = Math.PI * Math.Pow(D, 4) / 64d * (1 - Math.Pow(alfa, 4));
            return jx;
        }

        public override double Jy()
        {
            return Jx();
        }

        public override double W_s()
        {
            double wx = Math.PI * Math.Pow(D, 3) / 32d * (1 - Math.Pow(alfa, 4));
            return wx;
        }

        //Момент инерции тонкого кольца РТ СП
        public override double I_s()
        {
            double i_s = A_s * Math.Pow(2 * r_s, 2) / 8d;
            return i_s;
        }

        // статические моменты относительно осей
        public override double Sy() => Area() * r2;
        public override double Sx() => Sy();


        /// <summary>
        /// Возращает габаритные размеры сечения
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, double> GetDimension()
        {
            Dictionary<string, double> dimensionOfSection = new Dictionary<string, double>()
            {
                { DN(typeof(BSBeam_Ring), "r1"), r1 },
                { DN(typeof(BSBeam_Ring), "r2"), r2 }
            };
            return dimensionOfSection;
        }


        public double A_red(double _Es, double _Efb) => A_r + (_Es / _Efb) * A_s;

        public double Is_red(double _Es, double _Efb) => (_Es / _Efb) * I_s();

        public static Exception RadiiError()
        {
            return new Exception("Внутренний радиус больше внешнего");
        }
        public override void SetSizes(double[] _t)
        {
            (r1, r2, Length) = (_t[0], _t[1], _t[2]);
        }
    }
}
