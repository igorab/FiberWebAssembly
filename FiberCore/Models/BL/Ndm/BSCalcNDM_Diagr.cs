using BSFiberCore.Models.BL.Lib;

namespace BSFiberCore.Models.BL.Ndm
{
    /// <summary>
    /// Расчет по 2 группе предельных состояний
    /// </summary>
    public partial class BSCalcNDM
    {
        /// <summary>
        /// Диаграмма деформирования арматуры (двухлинейная) 
        /// </summary>
        /// <param name="_e">деформация</param>
        /// <returns>Напряжение</returns>
        private double Diagr_S(double _e)
        {
            double s = 0;
            esc0 = Rsc / Es0;            
            est0 = Rst / Es0;
            
            bool rip = false;

            if (_e > est2)
            {
                if (rip)
                    s = 0;
                else
                    s = Rst + Es0 * (_e - est2);
            }
            else if (_e < -esc2)
            {
                if (rip)
                    s = 0;
                else
                    s = -Rsc + Es0 * (_e + esc2);
            }
            else if (est0 <= _e && _e <= est2)
            {
                s = Rst;
            }
            else if (-esc2 <= _e && _e <= -esc0)
            {
                s = -Rsc;
            }
            else if (0 <= _e && _e <= est0)
            {
                s = Math.Min(_e * Es0, Rst);
            }
            else if (-esc0 <= _e && _e <= 0)
            {
                s = Math.Max(_e * Es0, -Rsc);
            }

            return s;
        }

        /// <summary>
        /// Диаграмма деформирования обычного бетона (трехлинейная) 
        /// </summary>
        /// <param name="_e">деформация</param>
        /// <returns>напряжение</returns>
        private double Diagr_Beton(double _e)
        {
            double s = 0;
            double sc1 = 0.6 * Rbc;
            double st1 = 0.6 * Rfbt;

            double ebc1 = sc1 / Eb0;
            double ebt1 = st1 / Eb0;

            efbt0 = Rfbt / Ebt;

            bool rip = false;

            //DODO ?
            if (_e > efbt2)
            {
                if (rip)
                    s = 0;
                else
                    s = Rfbt2 + Eb0 * (_e - efbt2);
            }
            else if (_e < -ebc2)
            {
                if (rip)
                    s = 0;
                else
                    s = -Rbc + Eb0 * (_e + ebc2);
            }
            else if (-ebc2 <= _e && _e <= -ebc0)
            {
                s = -Rbc;
            }
            else if (efbt0 <= _e && _e <= efbt2)
            {
                s = Rfbt;
            }
            else if (-ebc0 <= _e && _e <= -ebc1)
            {
                s = -Rbc * ((1 - sc1 / Rbc) * (Math.Abs(_e) - ebc1) / (ebc0 - ebc1) + sc1 / Rbc);
            }
            else if (ebt1 <= _e && _e <= efbt0)
            {
                s = Rfbt * ((1 - st1 / Rfbt) * (Math.Abs(_e) - ebt1) / (efbt0 - ebt1) + st1 / Rfbt);
            }
            else if (-ebc1 <= _e && _e <= ebt1)
            {
                s = _e * Eb0;
            }

            return s;
        }

        /// <summary>
        /// Диаграмма деформирования фибробетона (трехлинейная) 
        /// СП360 5.2.9
        /// </summary>
        /// <param name="_e">деформация</param>
        /// <param name="_beton">использовать диаграмму обычного бетона</param>
        /// <returns>напряжение</returns>
        private double Diagr_FB(double _e)
        {
            double s = 0;

            if (Setup.BetonTypeId == 1)
            {
                return Diagr_Beton(_e);
            }

            bool rip = false;
            
            double sc1 = 0.6 * Rbc;
            double ebc1 = sc1 / Eb0;

            // Растяжение - по СП 360
            efbt0 = Rfbt / Ebt;
            efbt1 = efbt0 + 0.0001;            
            efbt3 = 0.02 - 0.0125 * (Rfbt3 / Rfbt2 - 0.5);

            // сжатие: ПО СП 63 6.1.20 (как для обычного бетона)           
            if (_e < -ebc2)
            {
                if (rip)
                    s = 0;
                else
                    s = -Rbc + Eb0 * (_e + ebc2);
            }
            else if (-ebc2 <= _e && _e <= -ebc0)
            {
                s = -Rbc;
            }
            else if (-ebc0 <= _e && _e <= -ebc1)
            {
                s = -Rbc * ((1 - sc1 / Rbc) * (Math.Abs(_e) - ebc1) / (ebc0 - ebc1) + sc1 / Rbc);
            }
            else if (-ebc1 <= _e && _e < 0)
            {
                s = _e * Eb0;
            }
            // растяжение
            else if (0 <= _e && _e <= efbt0)
            {
                s = _e * Ebt;
            }
            else if (efbt0 < _e && _e <= efbt1)
            {
                s = Rfbt;
            }
            else if (efbt1 < _e && _e <= efbt2)
            {
                s = Rfbt * (1 - (1 - Rfbt2 / Rfbt) * (_e - efbt1) / (efbt2 - efbt1));
            }
            else if (efbt2 < _e && _e <= efbt3)
            {
                s = Rfbt2 * (1 - (1 - Rfbt3 / Rfbt2) * (_e - efbt2) / (efbt3 - efbt2));
            }
            else if (_e > efbt3)
            {
                if (rip)
                    s = 0;
                else
                    s = Rfbt3 + Ebt * (_e - efbt3);
            }

            return s;
        }

        /// <summary>
        /// коэффициент для напряжения арматуры для элементов с трещинами в растянутой зоне
        /// </summary>
        /// <returns></returns>
        private double Psi_s(double _e_s)
        {
            if (_e_s == 0 || GroupLSD == BSFiberLib.CG1) return 1;

            double res = 1 / (1 + 0.8 * Eps_s_crc / _e_s);
            return res;
        }


        // секущий модуль
        private double EV_Sec(double _sigma, double _e, double _E0)
        {
            double E_Sec;

            if (_e == 0)
            {
                E_Sec = _E0;
            }
            else
            {
                double sigma = _sigma;
                if (CalcA_crc)
                {
                    sigma = sigma * Psi_s(_e);
                }

                E_Sec = sigma / _e;
            }

            return E_Sec;
        }

        /// <summary>
        /// 6.2.16 значение базового расстояния между трещинами
        /// </summary>
        /// <param name="_ds_nom">номинальный диаметр арматуры</param>
        /// <returns>Расстояние между трещинами</returns>
        private double L_s(double _ds_nom)
        {
            if (NdmCrc.mu_fv == 0) return 0;
            
            double res = NdmCrc.kf * (50 + 0.5 * NdmCrc.fi2 * NdmCrc.fi3) * _ds_nom / NdmCrc.mu_fv;

            if (res > h)
                res = h;

            return res;
        }

        // СП360 6.2.14
        private double A_crc(double _sig_s, double _ls)
        {
            if (_sig_s == 0 || _ls == 0) 
                return 0; 
            
            double psi_s = 1 - 0.8 * sig_s_crc / _sig_s;
            
            double a_crc = NdmCrc.fi1 * NdmCrc.fi3 * psi_s * _sig_s / Es0 * _ls;

            return a_crc;
        }        
    }
}
