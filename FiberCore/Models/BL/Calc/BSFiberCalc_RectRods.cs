using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Mat;

namespace BSFiberCore.Models.BL.Calc
{   
    /// <summary>
    ///  Расчет изгибаемого прямоугольного элемента с рабочей арматурой
    /// </summary>
    public class BSFiberCalc_RectRods : BSFibCalc_Rect
    {
        // продольная арматура
        private double[] m_LRebar;
        // поперечная арматура
        private double[] m_TRebar;

        // Материал стержня
        private BSMatRod MatRod;
        // Стержни (итого)
        private BSRod  Rod;

        public double[] LRebar { get => m_LRebar; set => m_LRebar = value; }
        public double[] TRebar { get => m_TRebar; set => m_TRebar = value; }

        public double Dzeta(double _x, double _h0) => (_h0 != 0) ? _x / _h0 : 0;

        public override Dictionary<string, double> PhysParams {
            get {
                return new Dictionary<string, double> { { "Rfbt3n", Rfbt3n }, { "B", B }, { "Rfbn", Rfbn } };
            }
        }

        public override BeamSection BeamSectionType() => BeamSection.Rect;

        public override bool UseRebar() => true;

        public override Dictionary<string, double> PhysicalParameters()
        {
            Dictionary<string, double> phys = new Dictionary<string, double>
            {
                { DN(typeof(BSFiberCalculation), "Rfbt3n"), Rfbt3n },
                { DN(typeof(BSFiberCalculation), "B"), B },
                { DN(typeof(BSFiberCalculation), "Rfbn"), Rfbn }
            };

            return phys;
        }


        public override Dictionary<string, double> Coeffs => new Dictionary<string, double>() {
            { "Yft", Yft }, { "Yb", Yb }, { "Yb1", Yb1 }, { "Yb2", Yb2 }, { "Yb3", Yb3 }, { "Yb5", Yb5 }
        };

        /// <summary>
        /// получить параметы арматурных стержней
        /// </summary>
        /// <param name="_MatRod"></param>
        public void SetLTRebar( double[] _MatRod)
        {                        
            int idx = -1;

            MatRod = new BSMatRod
            {
                Rs  = _MatRod[++idx], // кг/см2
                Rsc = _MatRod[++idx], // кг/см2
                As  = _MatRod[++idx], // см2
                As1 = _MatRod[++idx], // см2
                Es  = _MatRod[++idx]
            };

            Rod = new BSRod
            {
                a  = _MatRod[++idx],
                a1 = _MatRod[++idx]
            };
        }

        //6.5
        // предельный изгибающий момент, который может быть воспринят сечением элемента
        protected double Mult_arm(double _b, double _h0, double _x, double _h, double _a, double _a1)
        {
            double res = Rfb * _b * _x * (_h0 - 0.5 * _x) - Rfbt3 * _b * (_h - _x) * ((_h - _x) / 2 - _a) + MatRod.Rsc * MatRod.As1 * (_h0 - _a1);
            return res;
        }

        public double Dzeta_R()
        {
            double eps_s = MatRod.epsilon_s();
            double dz_r =  Omega / (1 +  eps_s/ MatFiber.e_b2);
            return dz_r;
        }

        public override bool Calculate()
        {          
            // Расчетная высота сечения
            double h0 = h - Rod.a;

            double _x = (MatRod.Rs * MatRod.As - MatRod.Rsc * MatRod.As1 + Rfbt3 * b * h ) / ((Rfb + Rfbt3) * b);

            double dzeta = Dzeta(_x, h0);
                       
            //граничная относительная высота сжатой зоны
            double dzeta_R = Dzeta_R();
                        
            string info;
            bool checkOK = dzeta <= dzeta_R;
            if (checkOK)
            {
                info = string.Format("Условие ξ ({0}) <= ξR ({1}) выполнено ", Math.Round(dzeta, 2), Math.Round(dzeta_R, 2));
                Msg.Add(info);
            }
            else
            {
                info = string.Format("Условие ξ ({0}) <= ξR ({1}) не выполнено ", Math.Round(dzeta, 2), Math.Round(dzeta_R, 2));
                info += "Требуется увеличить высоту элемента.";
                Msg.Add(info);
            }

            Mult = Mult_arm(b, h0, _x, h, Rod.a, Rod.a1);

            //Коэффициент использования
            UtilRateCalc();

            InfoCheckM(Mult);
            
            return true;
        }

        public override Dictionary<string, double> Results()
        {
            return new Dictionary<string, double>() {         
                    { DN(typeof(BSFibCalc_Rect), "Mult"), Mult},
                    { DN(typeof(BSFibCalc_Rect), "UtilRate"), UtilRate}
            };
        }
    }    
}
