namespace BSFiberCore.Models.BL.Calc
{
    /// <summary>
    /// Расчеты на действие поперечной силы
    /// </summary>
    public class BSFiberCalc_QxQy : BSFiberCalc_MNQ
    {
        private const string msg1 = "Условие выполнено, шаг удовлетворяет требованию 6.1.28";
        private const string msg2 = "Условие не выполнено, требуется уменьшить шаг поперечной арматуры";
        private const string msg3 = "Перерезываюзщая сила превышает предельно допустимую в данном сечении";
        private const string msg4 = "Проверка по наклонному сечению на действие поперечной силы Qx пройдена";
        public override void SetSize(double[] _t)
        {
            (b, h) = (_t[0], _t[1]);
        }

        /// <summary>
        ///  допустимая поперечная сила в сечении [СП6.1.27]
        /// </summary>        
        private double Q_Ult(double _b, double _h)
        {            
            // рабочая высота сечения по растянутой арматуре
            double h0 = _h - Rebar.a;
            // Расчетные значения сопротивления  на сжатиие по B30 СП63
            Rfb = R_fb();
            // Предельная перерезывающая сила по полосе между наклонными сечениями
            double _Q_ult = 0.3 * Rfb * _b * h0; // (6.74)

            return _Q_ult;
        }
                
        public override bool Calculate()
        {
            double c_Q_ult = Q_Ult(b, h); // [СП6.1.27]
            double _Q_x = m_Efforts["Qx"];
            double _Q_y = m_Efforts["Qy"];
            
            if (_Q_x != 0)
            {
                double s_w_max;
                
                (s_w_max, Qx_ult) = Calculate_Qcx(b, h);
                
                UtilRate_Qx = (Qx_ult != 0) ? _Q_x / Qx_ult : 0;

                if (Rebar.Sw_X <= s_w_max) Msg.Add(msg1); else Msg.Add(msg2);
                
                // по полосе СП 6.1.27
                if (c_Q_ult <= Qx_ult) Msg.Add(msg3); else Msg.Add(msg4);                
            }

            if (_Q_y != 0)
            {
                double s_w_max;
                
                (s_w_max, Qy_ult)  = Calculate_Qy(h, b);

                UtilRate_Qy = (Qy_ult != 0) ? _Q_y / Qy_ult : 0;

                if (Rebar.Sw_Y <= s_w_max) Msg.Add(msg1); else Msg.Add(msg2);

                // по полосе СП 6.1.27
                if (c_Q_ult <= Qy_ult) Msg.Add(msg3); else Msg.Add(msg4);
            }
            
            // проверка по наклонному сечению при действии поперечных сил по двум осям
            Qc_ult = Math.Sqrt(Qx_ult * Qx_ult + Qy_ult * Qy_ult);

            UtilRate_Qс = (c_Q_ult != 0) ? Qc_ult / c_Q_ult : 0;

            return true;
        }
       
        public override (double, double) Calculate_Qcx(double _b, double _h)
        {
            return base.Calculate_Qcx(_b, _h);            
        }

        /// <summary>
        /// Расчет элементов по полосе между наклонными сечениями
        /// </summary>
        protected override (double, double) Calculate_Qy(double _b, double _h)
        {
            // Растояние до цента тяжести растянутой арматуры, см
            double a = Rebar.a;
            // рабочая высота сечения по растянутой арматуре
            double h0 = _h - a;
            // Расчетные значения сопротивления  на сжатиие по B30 СП63
            Rfb = R_fb();

            // Расчет элементов по наклонным сечениям на действие поперечных сил
            // Минимальная длина проекции(см)
            double c_min = h0;
            // Максимальная длина проекции(см)
            double c_max = 4 * h0;
            double dC = 1;
            // Минимальная длина проекции для формулы
            double c0_max = 2 * h0;

            List<double> lst_C = new List<double>();
            InitC(ref lst_C, c_min, c_max, dC);

            // Расчетное сопротивление сталефибробетона осевому растяжению
            double Rfbt = Rfbtn / Yft * Yb1 * Yb5;

            // поперечная сила, воспр сталефибробетоном
            double Qfb_i;

            List<double> lstQ_fb = new List<double>();

            foreach (double _c in lst_C)
            {
                if (_c == 0) continue;

                Qfb_i = 1.5d * Rfbt * _b * h0 * h0 / _c; // 6.76

                // условие на 0.5..2.5
                var Qult25 = 2.5 * Rfbt * _b * h0;
                var Qult05 = 0.5 * Rfbt * _b * h0;

                if (Qfb_i >= Qult25)
                {
                    Qfb_i = Qult25;
                }
                else if (Qfb_i <= Qult05)
                {
                    Qfb_i = Qult05;
                }

                lstQ_fb.Add(Qfb_i);
            }

            // Qfb - максимальная поперечная сила, воспринимаемая сталефибробетоном в наклонном сечении
            double Qfb = (lstQ_fb.Count > 0) ? lstQ_fb.Max() : 0;

            // Максимальный шаг поперечной арматуры см
            double s_w_max = (Qx > 0) ? Rfbt * _b * h0 * h0 / Qx : 0;

            // усилие в поперечной арматуре на единицу длины элемента
            double q_sw = (Rebar.Sw_Y != 0) ? Rebar.Rsw_Y * Rebar.Asw_Y / Rebar.Sw_Y : 0; // 6.78 

            // условие учета поперечной арматуры
            if (q_sw < 0.25 * Rfbt * _b)
                q_sw = 0;

            // поперечная сила, воспринимаемая поперечной арматурой в наклонном сечении
            double Qsw = 0;
            List<double> lst_Qsw = new List<double>();
            foreach (double _c in lst_C)
            {
                if (_c > c0_max)
                    Qsw = 0.75 * q_sw * c0_max;
                else
                    Qsw = 0.75 * q_sw * _c;  // 6.77

                lst_Qsw.Add(Qsw);
            }

            List<double> lst_Q_ult = new List<double>();
            for (int i = 0; i < lst_Qsw.Count; i++)
            {
                lst_Q_ult.Add(lstQ_fb[i] + lst_Qsw[i]);
            }

            double Qy_ult = Qfb + Qsw; // 6.75

            return (s_w_max, Qy_ult);
        }

        public BSFiberCalc_QxQy()
        {
        }

        public override Dictionary<string, double> Results()
        {
            Dictionary<string, double> dictRes = new Dictionary<string, double>()
            {
                { DN(typeof(BSFiberCalc_MNQ), "Qx_ult"), Qx_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_Qx"), UtilRate_Qx},

                { DN(typeof(BSFiberCalc_MNQ), "Qy_ult"), Qy_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_Qy"), UtilRate_Qy},

                { DN(typeof(BSFiberCalc_MNQ), "Qc_ult"), Qc_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_Qс"), UtilRate_Qс},                
            };

            return dictRes;
        }              
    }
}
