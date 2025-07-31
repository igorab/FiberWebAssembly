using BSFiberCore.Models.BL.Beam;
using System.ComponentModel;

namespace BSFiberCore.Models.BL.Calc
{
    [BSFiberCalculation(Descr = "Расчет балки таврового/двутаврового сечения")]
    public class BSFibCalc_IBeam : BSFiberCalculation
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

        // физ. характеристики бетона
        [DisplayName("Расчетные значения сопротивления на сжатиие по СП63 кг/см2")]
        public new double Rfbn { get; protected set; }
                
        // Результаты
        [DisplayName("Высота сжатой зоны, [см]")]
        public double x { get; protected set; }

        [DisplayName("Предельный момент сечения, [кг*см]")]
        public double Mult { get; protected set; }

        [DisplayName("Коэффициент использования по усилию, [П6.1.9]")]
        public double UtilRate { get; protected set; }

        private double h;

        public override void SetParams(double[] _t)
        {
            base.SetParams(_t);

            (Yft, Yb, Yb1, Yb2, Yb3, Yb5) = (_t[0], _t[1], _t[2], _t[3], _t[4], _t[5]);
        }

        public override BeamSection BeamSectionType() => BeamSection.IBeam;
        
        public override Dictionary<string, double> GeomParams()
        {
            Dictionary<string, double> geom = base.GeomParams();
            geom.Add(DN(typeof(BSFibCalc_IBeam), "bf"), bf);
            geom.Add(DN(typeof(BSFibCalc_IBeam), "hf"), hf);
            geom.Add(DN(typeof(BSFibCalc_IBeam), "bw"), bw);
            geom.Add(DN(typeof(BSFibCalc_IBeam), "hw"), hw);            
            geom.Add(DN(typeof(BSFibCalc_IBeam), "b1f"), b1f);
            geom.Add(DN(typeof(BSFibCalc_IBeam), "h1f"), h1f);
            return geom;
        }

        public override void SetSize(double[] _t)
        {
            (bf, hf, bw, hw, b1f, h1f) = (_t[0], _t[1], _t[2], _t[3], _t[4], _t[5]);
        }

        protected void Calc_Pre()
        {
            h = hf + hw + h1f; 
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

        public override bool Calculate()
        {
            bool calcOk;

            if (!Validate())
                return false;

            Calc_Pre();

            Action calc_a = delegate
            {
                x = Rfbt3 * (b1f * h1f + bw * hw + bf * hf) / (b1f * (Rfbt3 + Rfb));

                Mult = 0.5 * Rfbt3 * (b1f * (h1f - x) * (h1f + x) + bf * hf * (hf - x + 2 * (hw + h1f)) + bw * hw * (hw - x + 2 * h1f));
            };

            Action calc_b = delegate
            {
                x = (Rfbt3 * (bw * h1f + bw * hw + bw * h1f) + Rfb * h1f * (b1f - bw))/ (bw * (Rfbt3 + Rfb));

                Mult = Rfb * bw * (x - h1f) * (x - 0.5 * h1f) + Rfbt3 * (bw * (hw + h1f - x) + bw * hf * (h - 0.5 * (h1f + hf)));
            };
            
            // Расчет Тавра            
            bool cond = Rfbt3 * (bf * hf + bw * hw) < Rfb * b1f * h1f;

            if (cond)
            {
                calc_a();
            }
            else
            {
                calc_b();
            }

            //Коэффициент использования
            UtilRate = (Mult != 0) ? m_Efforts["My"] / Mult : 0;

            calcOk = true;
            
            InfoCheckM(Mult);
                        
            return calcOk;
        }

        public override Dictionary<string, double> Results()
        {
            return new Dictionary<string, double>() {
                { DN(typeof(BSFibCalc_IBeam), "x"), x },
                { DN(typeof(BSFibCalc_IBeam), "Mult") , Mult },
                { DN(typeof(BSFibCalc_IBeam), "UtilRate"), UtilRate}
            };
        }
    }
}
