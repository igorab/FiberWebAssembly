using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Beam;
using MathNet.Numerics.Statistics;

namespace BSFiberCore.Models.BL.Ndm
{    
    public partial class BSCalcNDM
    {        
        /// <summary>
        ///  рассчитать
        /// </summary>
        private void Calculate()
        {
            #region Enforces initialization
            My = new List<double> { My0 };
            Mx = new List<double> { Mx0 };
            #endregion

            InitDeformParams();

            #region Section initialization
            InitSectionsLists();
            int n, m;

            if (m_BeamSection == BeamSection.Rect)
            {
                n = InitRectangleSection(b, h, -b / 2.0);                
                m = InitReinforcement(-b / 2.0);
            }
            else if (BSHelper.IsITL(m_BeamSection))
            {
                n = InitIBeamSection(bf, hf, bw, hw, b1f, h1f);
                m = InitReinforcement(-b / 2.0);
            }
            else if (m_BeamSection == BeamSection.Ring)
            {
                n = InitRingSection(r1, R2);
                m = InitReinforcement();
            }
            else if (m_BeamSection == BeamSection.Any)
            {
                n = InitAnySection();
                m = InitReinforcement();
            }
            else
            {
                throw new Exception($"Тип сечения {m_BeamSection} не поддерживается в данном расчете ");
            }
            #endregion

            #region Iteration 0

            //Массивы секущих модулей
            List<List<double>> Eb = new List<List<double>>() { new List<double>() };
            List<List<double>> Es = new List<List<double>>() { new List<double>() };
            List<List<double>> Ebs = new List<List<double>>() { new List<double>() };
            // Заполняем секущие модули для нулевой итерации
            for (int i = 0; i < n; i++)
            {                
                Eb[0].Add(Ebt);
            }

            for (int i = 0; i < As.Count; i++)
            {
                Es[0].Add(Es0);             
                Ebs[0].Add(Ebt);
            }

            // Массив привязок центра тяжести
            List<double> y_cm = new List<double>();
            List<double> x_cm = new List<double>();

            // Вычисляем положение начального (геометрического) центра тяжести
            double num_c_y = Ab.Zip(y0b, (A, y) => A * y).Sum();
            double num_c_x = Ab.Zip(x0b, (A, x) => A * x).Sum();

            double area_b = Ab.Sum(A => A);
            double cy = num_c_y / area_b;
            y_cm.Add(cy);

            double cx = num_c_x / area_b;
            x_cm.Add(cx);

            // массив привязок бетонных элементов к ц.т., см                
            List<List<double>> yb = new List<List<double>>() { new List<double>() };
            List<List<double>> xb = new List<List<double>>() { new List<double>() };

            // массив привязок арматурных элементов к ц.т., см                
            List<List<double>> ys = new List<List<double>>() { new List<double>() };
            List<List<double>> xs = new List<List<double>>() { new List<double>() };

            //заполняем массивы привязок относительно ц.т. на нулевой итерации    
            for (int k = 0; k < n; k++)
            {
                yb[0].Add(y0b[k] - y_cm[0]);
                xb[0].Add(x0b[k] - x_cm[0]);
            }

            for (int l = 0; l < m; l++)
            {
                ys[0].Add(y0s[l] - y_cm[0]);
                xs[0].Add(x0s[l] - x_cm[0]);
            }

            /// Создаем массивы упруго-геометрических характеристик
            // осевая геометрическая жесткость
            List<double> Dxx = new List<double>();
            // упругогеометрический осевой момент отн оси Y
            List<double> Dyy = new List<double>();

            // упругогеометрический осевой момент отн оси Z
            List<double> Dzz = new List<double>();
            // упругогеометрический центробежный момент
            List<double> Dyz = new List<double>();

            // Вычисляем упруго-геометрические характеристики на нулевой итерации
            double dxx0 = Eb[0].Zip(Ab, (E, A) => E * A).Sum() +
                            Es[0].Zip(As, (E, A) => E * A).Sum() -
                            Ebs[0].Zip(As, (E, A) => E * A).Sum();
            Dxx.Add(dxx0);

            double dyy0 = Eb[0].ZipThree(Ab, xb[0], (E, A, z) => E * A * z * z).Sum() +
                            Es[0].ZipThree(As, xs[0], (E, A, z) => E * A * z * z).Sum() -
                            Ebs[0].ZipThree(As, xs[0], (E, A, z) => E * A * z * z).Sum();
            Dyy.Add(dyy0);

            double dzz0 = Eb[0].ZipThree(Ab, yb[0], (E, A, y) => E * A * y * y).Sum() +
                            Es[0].ZipThree(As, ys[0], (E, A, y) => E * A * y * y).Sum() -
                            Ebs[0].ZipThree(As, ys[0], (E, A, y) => E * A * y * y).Sum();
            Dzz.Add(dzz0);

            double dyz0 = Eb[0].ZipFour(Ab, yb[0], xb[0], (E, A, y, z) => E * A * y * z).Sum() +
                            Es[0].ZipFour(As, ys[0], xs[0], (E, A, y, z) => E * A * y * z).Sum() -
                            Ebs[0].ZipFour(As, ys[0], xs[0], (E, A, y, z) => E * A * y * z).Sum();
            Dyz.Add(dyz0);

            // Создаем массивы параметров деформаций
            List<double> ep0 = new List<double>();
            List<double> Ky = new List<double>();
            List<double> Kx = new List<double>();

            //Вычисляем параметры деформаций на нулевой итерации
            ep0.Add(N0 / Dxx[0]);
            double denomK = Dyy[0] * Dzz[0] - Math.Pow(Dyz[0], 2);

            Ky.Add((Mx[0] * Dyy[0] + My[0] * Dyz[0]) / denomK);
            Kx.Add(-(My[0] * Dzz[0] + Mx[0] * Dyz[0]) / denomK);

            //создаем массивы для деформаций бетона и арматуры   
            List<List<double>> epB = new List<List<double>>() { new List<double>() };
            List<List<double>> epS = new List<List<double>>() { new List<double>() };

            //Вычисляем деформации на нулевой итерации   
            for (int k = 0; k < n; k++)
                epB[0].Add(ep0[0] + yb[0][k] * Ky[0] + xb[0][k] * Kx[0]);

            for (int l = 0; l < m; l++)
                epS[0].Add(ep0[0] + ys[0][l] * Ky[0] + xs[0][l] * Kx[0]);

            // Создаем массивы для напряжений
            List<List<double>> sigB = new List<List<double>>() { new List<double>() };
            List<List<double>> sigS = new List<List<double>>() { new List<double>() };
            List<List<double>> sigBS = new List<List<double>>() { new List<double>() };

            // Заполняем напряжения на нулевой итерации
            for (int k = 0; k < n; k++)
            {
                sigB[0].Add(Diagr_FB(epB[0][k]));
            }

            for (int l = 0; l < m; l++)
            {
                sigS[0].Add(Diagr_S(epS[0][l]));

                sigBS[0].Add(Diagr_FB(epS[0][l]));
            }
            #endregion

            // итерации 
            for (int j = 1; j <= jmax; j++)
            {
                // пересчитываем секущие модули
                Eb.Add(new List<double>());
                for (int k = 0; k < n; k++)
                {
                    Eb[j].Add(EV_Sec(sigB[j - 1][k], epB[j - 1][k], Eb0));
                }

                Es.Add(new List<double>());
                Ebs.Add(new List<double>());
                for (int l = 0; l < m; l++)
                {
                    Es[j].Add(EV_Sec(sigS[j - 1][l], epS[j - 1][l], Es0));
                    Ebs[j].Add(EV_Sec(sigBS[j - 1][l], epS[j - 1][l], Eb0));
                }

                // пересчитываем упруго-геометрические характеристики
                double _dxx = Eb[j].Zip(Ab, (E, A) => E * A).Sum() +
                                Es[j].Zip(As, (E, A) => E * A).Sum() -
                                Ebs[j].Zip(As, (E, A) => E * A).Sum();
                Dxx.Add(_dxx);
                if (Dxx[j] == 0)
                {
                    err = 1;
                    break;
                }

                // пересчитываем положение упруго-геометрического ц.т.
                num_c_y = Eb[j].ZipThree(Ab, y0b, (E, A, y0) => E * A * y0).Sum() +
                        Es[j].ZipThree(As, y0s, (E, A, y0) => E * A * y0).Sum() -
                        Ebs[j].ZipThree(As, y0s, (E, A, y0) => E * A * y0).Sum();

                num_c_x = Eb[j].ZipThree(Ab, x0b, (E, A, x0) => E * A * x0).Sum() +
                        Es[j].ZipThree(As, x0s, (E, A, x0) => E * A * x0).Sum() -
                        Ebs[j].ZipThree(As, x0s, (E, A, x0) => E * A * x0).Sum();

                y_cm.Add(num_c_y / Dxx[j]);
                x_cm.Add(num_c_x / Dxx[j]);

                // пересчитываем привязки бетона и арматуры к центру тяжести    
                yb.Add(new List<double>());
                xb.Add(new List<double>());
                for (int k = 0; k < n; k++)
                {
                    yb[j].Add(y0b[k] - y_cm[j]);
                    xb[j].Add(x0b[k] - x_cm[j]);
                }

                ys.Add(new List<double>());
                xs.Add(new List<double>());
                for (int l = 0; l < m; l++)
                {
                    ys[j].Add(y0s[l] - y_cm[j]);
                    xs[j].Add(x0s[l] - x_cm[j]);
                }

                // пересчитываем жесткости
                double dyy = Eb[j].ZipThree(Ab, xb[j], (E, A, x) => E * A * x * x).Sum() +
                                Es[j].ZipThree(As, xs[j], (E, A, x) => E * A * x * x).Sum() -
                                Ebs[j].ZipThree(As, xs[j], (E, A, x) => E * A * x * x).Sum();
                Dyy.Add(dyy);

                double dxx = Eb[j].ZipThree(Ab, yb[j], (E, A, y) => E * A * y * y).Sum() +
                                Es[j].ZipThree(As, ys[j], (E, A, y) => E * A * y * y).Sum() -
                                Ebs[j].ZipThree(As, ys[j], (E, A, y) => E * A * y * y).Sum();
                Dzz.Add(dxx);

                double dyx = Eb[j].ZipFour(Ab, yb[j], xb[j], (E, A, y, x) => E * A * y * x).Sum() +
                                Es[j].ZipFour(As, ys[j], xs[j], (E, A, y, x) => E * A * y * x).Sum() -
                                Ebs[j].ZipFour(As, ys[j], xs[j], (E, A, y, x) => E * A * y * x).Sum();
                Dyz.Add(dyx);

                denomK = Dyy[j] * Dzz[j] - Math.Pow(Dyz[j], 2);
                if (denomK == 0 || double.IsNaN(denomK))
                {
                    err = 1;
                    break;
                }
                // Пересчитываем моменты
                My.Add(My[0] + N0 * (x_cm[j] - x_cm[0]));
                Mx.Add(Mx[0] - N0 * (y_cm[j] - y_cm[0]));

                // Пересчитываем параметры деформаций
                ep0.Add(N0 / Dxx[j]);
                Ky.Add((Mx[j] * Dyy[j] + My[j] * Dyz[j]) / denomK);
                Kx.Add(-(My[j] * Dzz[j] + Mx[j] * Dyz[j]) / denomK);

                // Пересчитываем деформации
                epB.Add(new List<double>());
                for (int k = 0; k < n; k++)
                    epB[j].Add(ep0[j] + yb[j][k] * Ky[j] + xb[j][k] * Kx[j]);

                epS.Add(new List<double>());
                for (int l = 0; l < m; l++)
                    epS[j].Add(ep0[j] + ys[j][l] * Ky[j] + xs[j][l] * Kx[j]);

                // Пересчитываем напряжения
                sigB.Add(new List<double>());
                for (int k = 0; k < n; k++)
                    sigB[j].Add(Diagr_FB(epB[j][k]));

                sigS.Add(new List<double>());
                sigBS.Add(new List<double>());
                for (int l = 0; l < m; l++)
                {
                    sigS[j].Add(Diagr_S(epS[j][l]));
                    sigBS[j].Add(Diagr_FB(epS[j][l]));
                }

                // Проверка - выполняются ли условия в равновестия?
                Nint = sigB[j].Zip(Ab, (s, A) => s * A).Sum() + sigS[j].Zip(As, (s, A) => s * A).Sum() -
                        sigBS[j].Zip(As, (s, A) => s * A).Sum();

                Myint = -(sigB[j].ZipThree(Ab, xb[j], (s, A, x) => s * A * x).Sum() + sigS[j].ZipThree(As, xs[j], (s, A, x) => s * A * x).Sum() -
                            sigBS[j].ZipThree(As, xs[j], (s, A, x) => s * A * x).Sum());

                Mxint = sigB[j].ZipThree(Ab, yb[j], (s, A, y) => s * A * y).Sum() + sigS[j].ZipThree(As, ys[j], (s, A, y) => s * A * y).Sum() -
                        sigBS[j].ZipThree(As, ys[j], (s, A, y) => s * A * y).Sum();

                //  Расчеты по 2 группе предельных состояний
                if (GroupLSD == BSFiberLib.CG2)
                {
                    //1. проверка на возникновение трещины
                    //2. определение ширины раскрытия трещины, если трещина возникла

                    CalcM_crc();

                    // максимальная деформация в сечении
                    double epsBt = epB[j].Maximum();
                    // условие возникновения трещины
                    if (ebt_crc == 0 && (epsBt > 0 && epsBt >= efbt1)) { ebt_crc = epsBt; }

                    // Трещина возникла:
                    //-- определяем моменты трещинообразования
                    My_crc = My.Max(); 
                    Mx_crc = Mx.Max();
                    N_crc = 0;
                    
                    //-- рассчитываем ширину раскрытия трещины
                    if (CalcA_crc)
                    {
                        // деформация арматуры
                        es_crc = epS[j].Maximum();

                        // напряжение в арматуре
                        sig_s_crc = sigS[j].Maximum(); 

                        // цикл по стержням арматуры
                        foreach (double _d in d_nom)
                        {
                            double ls = L_s(_d);

                            a_crc = Math.Max(a_crc,  A_crc(sig_s_crc, ls)); // ширина раскрытия трещины
                        }
                    }
                }

                // Вычисление погрешностей
                double tol_ep0 = Math.Abs(ep0[j] - ep0[j - 1]); // вычисление в серединной линии
                double tol_Ky = Math.Abs(Ky[j] - Ky[j - 1]);
                double tol_Kx = Math.Abs(Kx[j] - Kx[j - 1]);

                double tol = new double[] { tol_ep0, tol_Ky, tol_Kx }.Max();

                if (tol < tolmax)
                {
                    err = -1;
                    break;
                }

                if (j == jmax - 1)
                {
                    err = 2;  // Достигнуто максимальное число итераций
                    break;
                }

                if (epB[j].Max() > 1)
                {
                    err = 3; // Деформации превысили разумный предел
                    break;
                }
            }

            int jend = sigB.Count - 1;

            // Проверка - выполняются ли условия в равновестия?
            Nint = sigB[jend].Zip(Ab, (s, A) => s * A).Sum() + sigS[jend].Zip(As, (s, A) => s * A).Sum() -
                    sigBS[jend].Zip(As, (s, A) => s * A).Sum();

            Myint = -(sigB[jend].ZipThree(Ab, xb[jend], (s, A, x) => s * A * x).Sum() + sigS[jend].ZipThree(As, xs[jend], (s, A, x) => s * A * x).Sum() -
                        sigBS[jend].ZipThree(As, xs[jend], (s, A, z) => s * A * z).Sum());

            Mxint = sigB[jend].ZipThree(Ab, yb[jend], (s, A, y) => s * A * y).Sum() + sigS[jend].ZipThree(As, ys[jend], (s, A, y) => s * A * y).Sum() -
                    sigBS[jend].ZipThree(As, ys[jend], (s, A, y) => s * A * y).Sum();

            // растяжение: 
            // напряжения:
            double sigB_t = NuNTo0(sigB[jend].Maximum());
            double sigS_t = NuNTo0(sigS[jend].Maximum());
            // деформации:
            double epsB_t = NuNTo0(epB[jend].Maximum());
            double epsS_t = NuNTo0(epS[jend].Maximum());
            
            // сжатие: 
            // напряжения:
            double sigB_p = NuNTo0(sigB[jend].Minimum());
            double sigS_p = NuNTo0(sigS[jend].Minimum());
            // деформации:
            double epsB_p = NuNTo0(epB[jend].Minimum());
            double epsS_p = NuNTo0(epS[jend].Minimum());
             
            // Площадь растянутой арматуры
            double As_t = 0;
            // расст то ц.т. растянутой арматуры
            double h0_t = 0;
            // Площадь сжатой арматуры
            double As1_p = 0;
            // расст то ц.т. сжатой арматуры
            double h01_p = 0;
            // момент инерции арматуры
            double I_s = 0;
            // момент сопротивления сечения
            double W_s = 0;

            // положение центра тяжести растянутой и сжатой арматуры
            double s_t_xcm = 0; 
            double s_p_xcm = 0;

            // момент инерции сечения относительно его ц.т.
            double Jx = Ab.Zip(y0b, (A, y) => A * y * y).Sum() - area_b * Math.Pow(y_cm[0], 2) ;
            double Jy = Ab.Zip(x0b, (A, x) => A * x * x).Sum() - area_b * Math.Pow(x_cm[0], 2);

            // расстояние от ц.т. до наиболее растянутого волокна (y_t обозначение в формулах СП)
            double y_t = 0;
            y_t = x_cm[jend];
                        
            // моменты сопротивления сечения
            double W_t = Jy / y_t;
           
            int i_t = 0, i_p = 0;
            for (int i = 0; i < As.Count; i++)
            {
                double _xs_cm = xs[jend][i];

                if (epS[jend][i] >= 0)
                {
                    i_t++;
                    As_t += As[i];                    
                    s_t_xcm += _xs_cm;                    
                }

                if (epS[jend][i] < 0)
                {
                    i_p++;
                    As1_p += As[i];
                    s_p_xcm += xs[jend][i];
                }

                I_s += As[i] * _xs_cm * _xs_cm;
            }
            if (i_t > 0)
                s_t_xcm /= i_t;
            if (i_p > 0)
                s_p_xcm /= i_p;

            if (s_t_xcm != 0)
            {
                W_s = I_s / Math.Abs(s_t_xcm);
            }

            // момент инерции приведенного сечения относительно его ц.т.
            double I_red = Jy + I_s;
            double W_red = I_red / y_t;

            //рабочая высота сечения (расст от ц.т. сечения до ц.т. арматуры)            
            h0_t = Math.Abs(s_t_xcm);
            h01_p = Math.Abs(s_p_xcm);

            for (int l = 0; l < m; l++)
            {
                epS[0].Add(ep0[0] + ys[0][l] * Ky[0] + xs[0][l] * Kx[0]);
            }

            // СП 6.1.25 для эпюры с одним знаком
            if (Setup.UseRebar == false && Math.Sign(sigB_t) == Math.Sign(sigB_p))
            {
                e_fb_ult = (epsB_p != 0) ? ebc2 - (ebc2 - ebc0) * epsB_t / epsB_p : 0;
                e_fbt_ult = (epsB_p != 0) ? efbt3 - (efbt3 - efbt2) * epsB_t / epsB_p : 0;
            }
            else
            {
                e_fb_ult = 0;
                //если не допускаются трещины
                e_fbt_ult = efbt1; //  efbt3;                 
            }

            // определяем коэффициенты использоввания:
            // -- по деформациям на растяжение            
            //UtilRate_fb_t = (Rfbt3 != 0) ? sigB_t / Rfbt : 0.0;

            UtilRate_fb_t = (efbt1 != 0) ? epsB_t / efbt1 : 0.0;
            UtilRate_s_t = (esc0 != 0) ? epsS_t / esc0 : 0.0;
            // -- по деформациям на cжатие

            //UtilRate_fb_p = (Rbc != 0) ? sigB_p / Rbc : 0.0;
            UtilRate_fb_p = (ebc0 != 0) ? epsB_p / ebc0 : 0.0;
            UtilRate_s_p = (esc0 != 0) ? epsS_p / esc0 : 0.0;

            m_Results = new Dictionary<string, double>
            {
                // деформация
                ["ep0"] = ep0[jend],
                //  кривизна
                ["Ky"] = Ky[jend],
                ["ry"] = 1 / Ky[jend],

                ["Kx"] = Kx[jend],
                ["rx"] = 1 / Kx[jend],

                // растяжение
                ["sigB"] = sigB_t,
                ["sigS"] = sigS_t,
                ["epsB"] = epsB_t,
                ["epsS"] = epsS_t,

                // сжатие
                ["sigB_p"] = sigB_p,
                ["sigS_p"] = sigS_p,
                ["epsB_p"] = epsB_p,
                ["epsS_p"] = epsS_p,

                // предел
                ["esc0"]      = esc0,
                ["e_fb_ult"]  = e_fb_ult,
                ["e_fbt_ult"] = e_fbt_ult,

                // проверка усилий
                ["My"] = Myint,
                ["Mx"] = Mxint,
                ["N"]  = Nint,

                // использование материала:
                // -- растяжение:
                ["UR_fb_t"] = UtilRate_fb_t,
                ["UR_s_t"]  = UtilRate_s_t,
                // -- сжатие
                ["UR_fb_p"] = UtilRate_fb_p,
                ["UR_s_p"]  = UtilRate_s_p,

                // трещиностойкость
                // --моменты трещинообразования
                ["My_crc"] = My_crc,
                ["Mx_crc"] = Mx_crc,
                // -- ширина раскрытия трещины
                ["es_crc"]    = es_crc,
                ["sig_s_crc"] = sig_s_crc,
                ["a_crc"]     = a_crc,

                // арматура
                ["As_t"]  = As_t,
                ["h0_t"]  = h0_t,
                ["As1_p"] = As1_p,
                ["h01_p"] = h01_p,

                // положение Ц.Т.
                ["X0_cm"] = x_cm[0],
                ["Y0_cm"] = y_cm[0],

                ["X_cm"] = x_cm[jend],
                ["Y_cm"] = y_cm[jend],

                // площадь сечения
                ["Area"] = area_b,

                // моменты инерции сечения
                ["Jx"]    = Jx,
                ["Jy"]    = Jy,
                ["I_red"] = I_red,
                ["I_s"]   = I_s,

                // момент сопротивления сечения
                ["W_t"]   = W_t,
                ["W_red"] = W_red,
                ["W_s"]   = W_s,

                // число итераций:
                ["ItersCnt"] = jend
            };
            
            SigmaBResult = new List<double>(sigB[jend]);
            SigmaSResult = new List<double>(sigS[jend]);

            EpsilonBResult = new List<double>(epB[jend]);
            EpsilonSResult = new List<double>(epS[jend]);
        }

        /// <summary>
        /// Определение момента трещинообразования 
        /// формула 2.20
        /// </summary>
        /// <returns>M_crc</returns>        
        private double CalcM_crc()
        {
            double Mcrc = 0;
            double S = 0;
            double khi = 0;  // 1/ro

            Mcrc = Rfbt * b * S / (6 * khi * khi) + Eb0 * b;

            return Mcrc;
        }

        private double NuNTo0(double _value)
        {
            if (double.IsNaN(_value))
                return 0;
            else
                return _value;
        }
    }
   
}
