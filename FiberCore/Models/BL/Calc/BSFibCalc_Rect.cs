using BSFiberCore.Models.BL.Beam;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace BSFiberCore.Models.BL.Calc
{
    [DisplayName("Расчет прочности изгибаемого элемента прямоугольного сечения")]
    public class BSFibCalc_Rect : BSFiberCalculation
    {
        //размеры, см
        [DisplayName("Высота сечения, [см]")]
        public double h { get; private set; }
        [DisplayName("Ширина сечения, [см]")]
        public double b { get; private set; }
        [DisplayName("Упругопластический момент сопротивления")]
        public double Wpl { get; private set; }
        [DisplayName("Предельный момент сечения для изгибаемых сталефибробетонных элементов, [кг*см]")]
        public double Mult { get; protected set; }

        [DisplayName("Коэффициент, учитывающий неупругие свойства фибробетона растянутой зоны")]
        public double cGamma { get; protected set; }

        [DisplayName("Коэффициент использования по усилию, [П6.1.7 П6.1.8]")]
        public double UtilRate { get; protected set; }

        public override BeamSection BeamSectionType() => BeamSection.Rect;
        
        /// <summary>
        /// Коэффициенты надежности, применяемые в расчете
        /// </summary>
        public override Dictionary<string, double> Coeffs => new Dictionary<string, double>() { { "Yft", Yft }, { "Yb1", Yb1 }, { "Yb5", Yb5 } };

        /// <summary>
        /// Геометрия сечения
        /// </summary>        
        public override Dictionary<string, double> GeomParams()
        {
            Dictionary<string, double> geom = base.GeomParams();
            geom.Add(DN(typeof(BSFibCalc_Rect), "b"), b);
            geom.Add(DN(typeof(BSFibCalc_Rect), "h"), h);
            return geom;
        }

        /// <summary>
        /// результаты расчета
        /// </summary>        
        public override Dictionary<string, double> Results()
        {
            return new Dictionary<string, double>() {
                    { DN(typeof(BSFibCalc_Rect), "Wpl"), Wpl},
                    { DN(typeof(BSFibCalc_Rect), "Mult"), Mult},
                    { DN(typeof(BSFibCalc_Rect), "UtilRate"), UtilRate}
            };
        }

        public override Dictionary<string, double> PhysicalParameters()
        {
            Dictionary<string, double> phys = new Dictionary<string, double>
            {
                { DN(typeof(BSFiberCalculation), "Rfbt"), Rfbt },
                { DN(typeof(BSFiberCalculation), "B"), B },
                { DN(typeof(BSFibCalc_Rect), "cGamma"), cGamma }
            };

            return phys;
        }

        public override void SetParams(double[] _t)
        {
            base.SetParams(_t);

            // need refactoring
            ( Yft, Yb, Yb1, Yb2, Yb3, Yb5) = (_t[0], _t[1], _t[2], _t[3], _t[4], _t[5]);
        }

        public override void SetSize(double[] _t)
        {
            (b, h) = (_t[0], _t[1]);
        }
       
        public override bool Validate()
        {
            bool ret = base.Validate();

            if (Rfbt == 0)
            {
                Msg.Add("Требуется задать класса фибробетона на осевое растяжение");
                ret = false;
            }

            if (MatFiber.B < 15)
            {
                Msg.Add("Требуется увеличение класса бетона-матрицы (менее B15 не используется)");
                ret = false;
            }

            if (MatFiber.B > 60)
            {
                Msg.Add("Для бетона классом более B60 расчет вести на основе нелинейной деформационной модели");

                ret = false;
            }


            return ret;
        }

        /// <summary>
        /// Коэффициент использования
        /// </summary>
        protected void UtilRateCalc()
        {
            //Коэффициент использования
            UtilRate = (Mult != 0) ? m_Efforts["My"] / Mult : 0;
        }

        public override bool Calculate()
        {
            if (!Validate())            
                return false;
                        
            // Изменение 1 к СП 360
            cGamma = Gamma(MatFiber.B);

            //Упругопластический момент сопротивления  Ф.(6.3)
            Wpl = BSBeam_Rect.Wx(b, h) * cGamma;

            //Значение предельного момента сечения для изгибаемых сталефибробетонных элементов определяют по формуле (6.3) (кг*см)
            Mult = Rfbt * Wpl;
            
            UtilRateCalc();

            InfoCheckM(Mult);
            
            return true;
        }       
    }
}
