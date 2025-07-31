using BSFiberCore.Models.BL.Beam;
using System.ComponentModel;

namespace BSFiberCore.Models.BL.Calc
{
    [DisplayName("Расчет прочности изгибаемого элемента кольцевого сечения")]
    public class BSFibCalc_Ring : BSFiberCalculation
    {
        [DisplayName("Радиус внутренней грани, см")]
        public double r1 { get; private set; }

        [DisplayName("Радиус наружней грани, см")]
        public double r2 { get; private set; }
       
        [DisplayName("Предельный момент сечения, кг*см")]
        public double Mult { get; private set; }

        [DisplayName("Коэффициент использования по усилию, [П6.1.10]")]
        public double UtilRate { get; protected set; }

        public override BeamSection BeamSectionType() => BeamSection.Ring;
        
        public override void SetParams(double[] _t)
        {
            base.SetParams(_t);

            // need refactoring
            (Yft, Yb, Yb1, Yb2, Yb3, Yb5) = ( _t[0], _t[1], _t[2], _t[3], _t[4], _t[5]);
        }

        public override Dictionary<string, double> GeomParams()
        {
            Dictionary<string, double> geom = base.GeomParams();
            geom.Add(DN(typeof(BSFibCalc_Ring), "r1"), r1);
            geom.Add(DN(typeof(BSFibCalc_Ring), "r2"), r2);
            return geom;
        }

        public override void SetSize(double[] _t)
        {
            (r1, r2) = (_t[0], _t[1]);
        }

        public override Dictionary<string, double> Results()
        {
            return new Dictionary<string, double>() {
                { DN(typeof(BSFibCalc_Ring), "Rfbt3"), Rfbt3 },
                { DN(typeof(BSFibCalc_Ring), "Mult"), Mult },
                { DN(typeof(BSFibCalc_Ring), "UtilRate"), UtilRate }
            };
        }

        public override bool Validate()
        {
            bool ret = base.Validate();

            if (Rfb == 0 || Rfbt3 == 0)
            {
                Msg.Add("Требуется задать класс фибробетона на осевое сжатие и остаточное растяжение Rfbt3");
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// Расчет сечения
        /// </summary>        
        public override bool Calculate()
        {
            if (!Validate())
                return false;
            
            //толщина стенки кольца см
            double tr = r2 - r1;

            if (tr < 0)
                throw new Exception("r2-r1 < 0");

            //радиус срединной поверхности стенки кольцевого элемента, определяемый по ф. (6.19)
            double rm = (r1 + r2) / 2;

            //Общая площадь кольцевого сечения, определяемая по формуле (6.18)
            double Ar = 2 * Math.PI * rm * tr;

            double ar = (0.73d * Rfbt3) / (Rfb + 2 * Rfbt3);

            //Предельный момент сечения , кг*см
            Mult = Ar * (Rfb * Math.Sin(Math.PI * ar) / Math.PI + 0.234d * Rfbt3) * rm;

            //Коэффициент использования
            UtilRate = (Mult != 0) ? m_Efforts["My"] / Mult : 0;

            InfoCheckM(Mult);            

            return true;
        }
    }
}
