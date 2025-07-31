using BSFiberCore.Models.BL.Beam;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BSFiberCore.Models.BL.Calc
{
    /// <summary>
    /// Прямоугольная балка
    /// </summary>
    public class BSFiberCalc_MNQ_Rect : BSFiberCalc_MNQ
    {
        private string m_ImgCalc;        

        public BSFiberCalc_MNQ_Rect()
        {
            m_Beam = new BSBeam_Rect();            
        }

        public override BeamSection BeamSectionType() => BeamSection.Rect;

        /// <summary>
        /// Вернуть изрображение расчетной схемы
        /// </summary>
        /// <returns></returns>
        public override string ImageCalc()
        {
            if (!string.IsNullOrEmpty(m_ImgCalc))
                return m_ImgCalc;

            return (N_Out) ? "Rect_N_out.PNG" : "Rect_N.PNG";
        } 
        
        public override void SetSize(double[] _t)
        {
            m_Beam.SetSizes(_t);
            (b, h, LngthCalc0) = (_t[0], _t[1], _t[2]);

            A = m_Beam.Area();

            I = m_Beam.Jy();
            
            y_t = m_Beam.y_t;
        }

        /// <summary>
        /// Расчет внецентренно сжатых элементов (6.1.13)
        /// </summary>
        public new void Calculate_N()
        {
            N_In = true;
            base.Calculate_N();            
        }

        /// <summary>
        /// Расчет внецентренно сжатых сталефибробетонных
        /// элементов прямоугольного сечения с рабочей арматурой
        /// </summary>
        protected new void Calculate_N_Rods()
        {
            m_ImgCalc = "Rect_Rods_N_out.PNG";

            base.Calculate_N_Rods();            
        }

        /// <summary>
        /// Расчет внецентренно сжатых сталефибробетонных элементов без рабочей арматуры при
        /// расположении продольной сжимающей силы за пределами поперечного сечения элемента и внецентренно сжатых сталефибробетонных элементов без рабочей арматуры при расположении продольной
        /// сжимающей силы в пределах поперечного сечения элемента, в которых по условиям эксплуатации не
        /// допускается образование трещин
        /// </summary>
        private new void Calculate_N_Out()
        {
            base.Calculate_N_Out();            
        }

        /// <summary>
        /// Расчет элементов по полосе между наклонными сечениями
        /// </summary>
        private void CalculateQ()
        {
            m_ImgCalc = "Incline_Q.PNG";

            base.Calculate_Qcx(b, h);

            base.Calculate_Qy(h, b);
        }
       
        /// <summary>
        ///  Расчет элементов по наклонным сечениям на действие моментов
        /// </summary>
        private void CalculateM()
        {
            base.Calculate_Mc(b, h);

            base.Calculate_Mx(h, b);
        }

        /// <summary>
        ///  Вычислить
        /// </summary>
        public override bool Calculate()
        {            
            if (Shear)
            {                
                // Расчет на действие поперечной силы
                CalculateQ();

                // Расчет на действие моментов
                CalculateM();
            }
            else if (UseRebar)
            {
                Calculate_N_Rods();
            }
            else
            {                
                if (N_Out)
                {
                    Calculate_N_Out();
                }
                else
                {
                    Calculate_N();
                }
            }

            return true;
        }

        public override Dictionary<string, double> Results()
        {            
            Dictionary<string, double>  dictRes =  new Dictionary<string, double>() 
            {
                { DN(typeof(BSFiberCalc_MNQ), "M_ult"), M_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_My"), UtilRate_My },

                { DN(typeof(BSFiberCalc_MNQ), "N_ult"), N_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_N"), UtilRate_N },

                { DN(typeof(BSFiberCalc_MNQ), "Qx_ult"), Qx_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_Qx"), UtilRate_Qx },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_Qс"), UtilRate_Qс },
            };
                        
            return dictRes;
        }
    }
}
