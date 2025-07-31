using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Uom;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BSFiberCore.Models.BL.Ndm
{
    /// <summary>
    /// Обработка результатов расчета по НДМ
    /// </summary>
    public class BSCalcResultNDM
    {
        [DisplayName("Количество итераций, [.]")]
        public double ItersCnt { get; private set; }

        #region 1 группа предельных состояний
        [DisplayName("Радиус кривизны продольной оси в плоскости действия моментов, Rx, [см]")]
        public double rx { get; private set; }

        [DisplayName("Радиус кривизны продольной оси в плоскости действия моментов, Ry, [см]")]
        public double ry { get; private set; }

        [DisplayName("Относительная деформация волокна в Ц.Т. сечения, e0, [.]")]
        public double eps_0 { get; private set; }

        [DisplayName("Кривизна 1/Rx, [1/см]")]
        public double Kx { get; private set; }

        [DisplayName("Кривизна 1/Ry, [1/см]")]
        public double Ky { get; private set; }

        // Растяжение >>

        [DisplayName("Напряжение в бетоне, [кг/см2]")]
        public double sigmaB { get; private set; }

        [DisplayName("Напряжение в арматуре, [кг/см2]")]
        public double sigmaS { get; private set; }

        [DisplayName("Максимальная относительная деформация в фибробетоне, [.]")]
        public double e_fbt_max { get; private set; }

        [DisplayName("---Коэффициент использования по деформации в фибробетоне (растяжение), [П6.1.24]")]
        public double UtilRate_e_fbt { get; private set; }

        [DisplayName("Максимальная относительная деформация в арматуре, [.]")]
        public double e_s_max { get; private set; }

        [DisplayName("---Коэффициент использования по деформации в арматуре (растяжение), [П6.1.21]")]
        public double UtilRate_e_st { get; private set; }

        // Cжатие >>
        [DisplayName("Напряжение в бетоне (сжатие), [кг/см2]")]
        public double sigmaB_p { get; private set; }

        [DisplayName("Напряжение в арматуре (сжатие), [кг/см2]")]
        public double sigmaS_p { get; private set; }

        [DisplayName("Максимальная относительная деформация в фибробетоне (сжатие) , [.]")]
        public double e_fb_max_p { get; private set; }

        [DisplayName("---Коэффициент использования по деформации в фибробетоне (сжатие), [П6.1.25]")]
        public double UtilRate_e_fb_p { get; private set; }

        [DisplayName("Максимальная относительная деформация в арматуре (сжатие), [.]")]
        public double e_s_max_p { get; private set; }

        [DisplayName("---Коэффициент использования по деформации в арматуре (сжатие), [П6.1.21]")]
        public double UtilRate_e_s_p { get; private set; }

        [DisplayName("---Площадь растянутой арматуры, см2 ")]
        public double As_t { get; private set; }

        [DisplayName("---Площадь сжатой арматуры, см2 ")]
        public double As1_p { get; private set; }

        [DisplayName("---Расстояние до Ц.Т. растянутой арматуры, см")]
        public double h0_t { get; private set; }

        [DisplayName("---Расстояние до Ц.Т. сжатой арматуры, см")]
        public double h01_p { get; private set; }


        // проверка по усилиям

        [DisplayName("Момент Мx, [кг*см]")]
        public double Mx_calc { get; private set; }
        [DisplayName("Момент Мy, [кг*см]")]
        public double My_calc { get; private set; }
        [DisplayName("Сила N, [кг]")]
        public double N_calc { get; private set; }

        // напряжение
        public List<double> Sig_B { get; set; } // бетон
        public List<double> Sig_S { get; set; } // арматура

        // деформации
        public List<double> Eps_B { get; set; } // бетон
        public List<double> Eps_S { get; set; } // арматура

        /// <summary>
        /// диаметры арматуры соответствующие номерам стержней арматуры
        /// </summary>
        public List<double> RebarDiametersByIndex { get; set; }

        #endregion

        #region 2 группа предельных состояний
        [DisplayName("П6.2.13. Момент возникновения трещины, [кг*см]")]
        public double M_crc { get; set; }

        [DisplayName("Коэффициент использования по M_crc")]
        public double UtilRate_M_crc { get; set; }


        [DisplayName("П6.2.24. Кривизна 1/Rx, [1/см]")]
        public double Kx_crc { get; set; }

        [DisplayName("П6.2.31. Кривизна 1/Ry, [1/см]")]
        public double Ky_crc { get; set; }

        [DisplayName("П6.2.31. Напряжение в арматуре, [кг/см2]")]
        public double sig_s_crc { get; set; }

        [DisplayName("П6.2.31. Ширина раскрытия трещины, [см]")]
        public double a_crc { get; set; }

        [DisplayName("Предельная ширина раскрытия трещины, [см]")]
        public double a_crc_ult { get; set; }

        #endregion

        public List<string> Msg { get; set; }

        private Dictionary<string, double> Res1Group { get; set; }
        private Dictionary<string, double> Res2Group { get; set; }

        public List<int> ErrorIdx { get; set; }

        #region Параметры расчета

        [DisplayName("Ширина сечения, b [см]")]
        public double b { get; set; }

        [DisplayName("Высота сечения, h [см]")]
        public double h { get; set; }

        [DisplayName("Mx [кг*см]")]
        public double Mx { get; set; }

        [DisplayName("My [кг*см]")]
        public double My { get; set; }

        [DisplayName("N [кг]")]
        public double N { get; set; }

        [DisplayName("Qx [кг]")]
        public double Qx { get; set; }

        [DisplayName("Qy [кг]")]
        public double Qy { get; set; }

        [DisplayName("Модуль упругости фибробетона Eb, [кг/см2]")]
        public double Eb { get; set; }

        [DisplayName("Нормативное сопротивление фибробетона на сжатие R,fbn, [кг/см2]")]
        public double Rfbn { get; set; }

        [DisplayName("Нормативное сопротивление фибробетона на растяжение R,fbtn, [кг/см2]")]
        public double Rfbtn { get; set; }

        [DisplayName("Расчетное сопротивление фибробетона на сжатие R,fbn, [кг/см2]")]
        public double Rfb { get; set; }

        [DisplayName("Расчетное сопротивление фибробетона на растяжение R,fbtn, [кг/см2]")]
        public double Rfbt { get; set; }

        [DisplayName("Модуль упругости арматуры Es, [кг/см2]")]
        public double Es { get; set; }

        [DisplayName("Нормативное сопротивление арматуры R,sn, [кг/см2]")]
        public double Rs { get; set; }

        [DisplayName("Количество стержней арматуры, [шт]")]
        public double Rods_qty { get; set; }

        [DisplayName("Общая площадь продольной арматуры, [см2]")]
        public double Rods_area { get; set; }

        [DisplayName("Максимально допустимая относительная деформация в бетоне, [.]")]
        public double Eps_fb_ult { get; set; }

        [DisplayName("Максимально допустимая относительная деформация в бетоне, [.]")]
        public double Eps_fbt_ult { get; set; }

        [DisplayName("Максимально допустимая относительная деформация в арматуре, [.]")]
        public double Eps_s_ult { get; set; }

        [DisplayName("Максимальный прогиб балки [мм]")]
        public double Deflexion_max { get; set; }

        #endregion

        private Dictionary<string, double> m_Beam;


        private string DN(string _attr) => BSFiberCalculation.DsplN(typeof(BSCalcResultNDM), _attr);

        private Dictionary<int, string> DictErrors;


        /// <summary>
        /// внешние усилия
        /// </summary>
        public Dictionary<string, double> Efforts
        {
            get
            {
                return new Dictionary<string, double>
                {
                    { DN("Mx"), Mx },
                    { DN("My"), My },
                    { DN("N"), N  },
                    { DN("Qx"), Qx  },
                    { DN("Qy"), Qy  }
                };
            }
        }
        /// <summary>
        /// физические свойства бетона и арматуры 
        /// </summary>
        public Dictionary<string, double> PhysParams
        {
            get
            {
                return new Dictionary<string, double>
                {
                    { DN("Eb"),  Eb },
                    // норм
                    { DN("Rfbn"), Rfbn },
                    { DN("Rfbtn"), Rfbtn },
                    // расч
                    { DN("Rfb"), Rfb },
                    { DN("Rfbt"), Rfbt },
                    { DN("Es"),    Es },
                    { DN("Rs"),    Rs },
                    { DN("Eps_s_ult"), Eps_s_ult }
                };
            }
        }
        /// <summary>
        ///  количество арматуры
        /// </summary>
        public Dictionary<string, double> Reinforcement
        {
            get
            {
                return new Dictionary<string, double>
                {
                    { DN("Rods_qty"), Rods_qty },
                    { DN("Rods_area"), Rods_area }
                };
            }
        }

        public Dictionary<string, double> GeomParams => new Dictionary<string, double>
        {
            { DN("b"), b },
            { DN("h"), h }
        };

        public Dictionary<string, double> Beam => m_Beam;

        /// <summary>
        /// На действие поперечных сил
        /// </summary>
        public Dictionary<string, double> ResQxQy { get; internal set; }
        public MemoryStream ImageStream { get; internal set; }
        public List<string> Message { get; internal set; }
        public List<string> PictureForHeaderReport { get; internal set; }
        public List<string> PictureForBodyReport { get; internal set; }


        public Dictionary<string, double> Coeffs { get; internal set; }
        public LameUnitConverter UnitConverter { get; internal set; }
        public double Area { get; internal set; }
        public double W_s { get; internal set; }
        public double I_s { get; internal set; }
        public double Jy { get; internal set; }
        public double Jx { get; internal set; }
        public double X_c { get; internal set; } // центр тяжести
        public double Y_c { get; internal set; } // центр тяжести
        public double Sy { get; internal set; }
        public double Sx { get; internal set; }


        // параметры сечения
        private void SectionParams(Dictionary<string, double> _D)
        {
            if (_D.ContainsKey("Area"))
                Area = _D["Area"];
            if (_D.ContainsKey("W_s"))
                W_s  = _D["W_s"];
            if (_D.ContainsKey("I_s"))
                I_s  = _D["I_s"];
            if (_D.ContainsKey("Jy"))
                Jy   = _D["Jy"];
            if (_D.ContainsKey("Jx"))
                Jx   = _D["Jx"];

            if (_D.ContainsKey("X_c"))
                X_c = _D["X_c"];
            if (_D.ContainsKey("Y_c"))
                Y_c = _D["Y_c"];

            if (_D.ContainsKey("Sy"))
                Sy = _D["Sy"];
            if (_D.ContainsKey("Sx"))
                Sx = _D["Sx"];

        }

        private double InitBeamLength(double _lgth, double _coeflgth)
        {
            m_Beam = new Dictionary<string, double>
            {
                { "Длина элемента, см", _lgth },
                { "Коэффициет расчетной длины", _coeflgth }
            };

            return (_coeflgth != 0) ? _lgth * _coeflgth : _lgth;
        }


        /// <summary>
        /// Параметры расчета
        /// </summary>
        /// <param name="_D"></param>
        public void InitCalcParams(Dictionary<string, double> _D)
        {
            b  = _D["b"];
            h  = _D["h"];

            Mx = _D["Mx"];
            My = _D["My"];
            N =  _D["N"];
            Qx = _D["Qx"];
            Qy = _D["Qy"];
            
            // норм
            Rfbn  = _D["Rbcn"];
            Rfbtn = _D["Rbtn"];
            // расч
            Rfb   = _D["Rbc"];
            Rfbt  = _D["Rbt"];

            Eb    = _D["Eb0"];
            Es    = _D["Es0"];
            Rs    = _D["Rscn"];

            //Eps_fbt_ult = _D["ebt_ult"];
            //Eps_fb_ult  = _D["eb_ult"];
            //Eps_s_ult = _D["es_ult"];

            if (_D.ContainsKey("rods_qty"))
                Rods_qty = _D["rods_qty"];
            if (_D.ContainsKey("rods_area"))
                Rods_area = _D["rods_area"];

            if (_D.ContainsKey("lgth") && _D.ContainsKey("coeflgth"))
                InitBeamLength(_D["lgth"], _D["coeflgth"]);
        }

        private void DErr()
        {
            DictErrors = new Dictionary<int, string>()
            {
                [-1] = "Достигнута заданная сходимость метода [+]",
                [0] = "",
                [1] = "Превышен максимально допустимый предел деформации [-]",
                [2] = "Достигнуто максимальное число итераций [-]",
                [3] = "Деформации превысили разумный предел [-]"
            };
        }


        public BSCalcResultNDM()
        {
            DErr();    
        }

        /// <summary>
        /// Результаты расчета по 1 группе предельных состояний
        /// </summary>
        /// <param name="_D1gr"></param>
        public BSCalcResultNDM(Dictionary<string, double> _D1gr)
        {
            // итерации
            ItersCnt = _D1gr["ItersCnt"];

            // деформации
            eps_0 = _D1gr["ep0"];
            Ky = _D1gr["Ky"];
            ry = _D1gr["ry"];
            Kx = _D1gr["Kx"];
            rx = _D1gr["rx"];

            // растяжение
            sigmaB = BSHelper.SigU2U(_D1gr["sigB"]);
            sigmaS = BSHelper.SigU2U(_D1gr["sigS"]);
            e_fbt_max = _D1gr["epsB"];
            e_s_max = _D1gr["epsS"];

            // сжатие
            sigmaB_p = BSHelper.SigU2U(_D1gr["sigB_p"]);
            sigmaS_p = BSHelper.SigU2U(_D1gr["sigS_p"]);
            e_fb_max_p = _D1gr["epsB_p"];
            e_s_max_p = _D1gr["epsS_p"];

            // предел
            Eps_s_ult = _D1gr["esc0"];

            // проверка усилий
            Mx_calc = BSHelper.SigU2U(_D1gr["Mx"]);
            My_calc = BSHelper.SigU2U(_D1gr["My"]);            
            N_calc  = BSHelper.NU2U(_D1gr["N"]);

            // использование
            // сжатие
            UtilRate_e_fb_p = _D1gr["UR_fb_p"];
            UtilRate_e_s_p = _D1gr["UR_s_p"];
            // растяжение
            UtilRate_e_fbt = _D1gr["UR_fb_t"];
            UtilRate_e_st = _D1gr["UR_s_t"];

            // арматура
            As_t = _D1gr["As_t"];
            h0_t = _D1gr["h0_t"];
            As1_p = _D1gr["As1_p"];
            h01_p = _D1gr["h01_p"];

            SectionParams(_D1gr);

            Msg = new List<string>();
            Res1Group = new Dictionary<string, double>();
            Res2Group = new Dictionary<string, double>();
            ErrorIdx = new List<int>();

            DErr();
        }

        /// <summary>
        /// Результаты расчета по 2 группе предельных состояний. 
        /// Преобразуем данные для окончательного вывода в отчет
        /// </summary>
        /// <param name="_D2gr">Словарь с результатами</param>
        public void SetRes2Group(Dictionary<string, double> _D2gr, bool _showM = true, bool _show_a = false)
        {            
            // кривизна
            if (_D2gr.ContainsKey("Ky"))
                Ky_crc = _D2gr["Ky"];
            if (_D2gr.ContainsKey("Kx"))
                Kx_crc = _D2gr["Kx"];

            if (Rods_qty > 0)
            {
                if (_showM)
                {
                    // момент трещинообразования
                    if (_D2gr.ContainsKey("My_crc"))
                        M_crc = _D2gr["My_crc"];
                    if (_D2gr.ContainsKey("UtilRate_M_crc"))
                        UtilRate_M_crc = _D2gr["UtilRate_M_crc"];


                    // напряжение в арматуре (нужно реализовать для каждого стержня)
                    if (_D2gr.ContainsKey("sig_s_crc"))
                        sig_s_crc = _D2gr["sig_s_crc"];
                }

                if (_show_a)
                {
                    // ширина раскрытия трещины
                    if (_D2gr.ContainsKey("a_crc"))
                        a_crc = _D2gr["a_crc"];
                    if (_D2gr.ContainsKey("a_crc_ult"))
                        a_crc_ult = _D2gr["a_crc_ult"];
                }
            }
        }

        private void AddToResult(string _attr, double _value, int _group = 1, bool _truncate = true)
        {
            Dictionary<string, double> res = (_group == 1) ? Res1Group : Res2Group;
            
            if (_truncate && (Math.Abs(_value) < 10e-15 || Math.Abs(_value) > 10e15))
            {
                return;
            }

            try
            {
                var KEY = BSFiberCalculation.DsplN(typeof(BSCalcResultNDM), _attr);
                if (!res.ContainsKey(KEY))
                {
                    res.Add(KEY, _value);
                }
            }
            catch (Exception _E)
            {
                MessageBox.Show(_E.Message);
            }
        }


        /// <summary>
        ///  Отформатированные результаты расчета по 1 группе предельных состояний для вывода в отчет
        /// </summary>
        public Dictionary<string, double> GetResults1Group()
        {
            Res1Group = new Dictionary<string, double>();
            Res1Group.Add("<b>--------Изгиб:--------</b>", double.NaN);
            AddToResult("eps_0", eps_0);
            AddToResult("rx", rx);
            AddToResult("Kx", Kx);
            AddToResult("ry", ry);
            AddToResult("Ky", Ky);
            if (!(Deflexion_max is double.NaN))
                AddToResult("Deflexion_max", Deflexion_max);

            // растяжение
            Res1Group.Add("<b>--------Растяжение:--------</b>", double.NaN);
            AddToResult("sigmaB", sigmaB);            
            AddToResult("e_fbt_max", e_fbt_max);
            AddToResult("UtilRate_e_fbt", UtilRate_e_fbt);
            
            AddToResult("sigmaS", sigmaS);
            AddToResult("e_s_max", e_s_max);
            AddToResult("UtilRate_e_st", UtilRate_e_st);
            // сжатие
            Res1Group.Add("<b>--------Сжатие:-------</b>", double.NaN);
            // - бетон
            AddToResult("sigmaB_p", sigmaB_p);
            AddToResult("e_fb_max_p", e_fb_max_p);
            AddToResult("UtilRate_e_fb_p", UtilRate_e_fb_p);
            // - арматура
            AddToResult("sigmaS_p", sigmaS_p);
            AddToResult("e_s_max_p", e_s_max_p);
            AddToResult("UtilRate_e_s_p", UtilRate_e_s_p);

            // усилия
            Res1Group.Add("<b>--------Проверка по усилиям:-------</b>", double.NaN);
            AddToResult("Mx_calc", Mx_calc);
            AddToResult("My_calc", My_calc);
            AddToResult("N_calc", N_calc);
            AddToResult("ItersCnt", ItersCnt);

            // -поперечные силы
            if (ResQxQy != null)
            {                
                Res1Group.Add("<b>--------Проверка на действие поперечныx сил:-------</b>", double.NaN);
                foreach (var _resQ in ResQxQy)
                {
                    Res1Group.Add(_resQ.Key, _resQ.Value);                 
                }
            }

            return  Res1Group;
        }

        /// <summary>
        ///  результаты расчета по 1 группе пр сост
        /// </summary>
        /// <param name="_Message"></param>
        public void ResultsMsg1Group(ref List<string> _Message)
        {
            // растяжение (e > 0)
            bool res_fbt = e_fbt_max <= Eps_fbt_ult;
            if (res_fbt)
                Msg.Add(string.Format("Проверка сечения по фибробетону на растяжение пройдена e_fbt_max <= e_fbt_ult : {0} <= {1} ", Math.Round(e_fbt_max, 8), Math.Round(Eps_fbt_ult,8)));
            else
                Msg.Add(string.Format("Не пройдена проверка сечения по фибробетону на растяжение e_fbt_max <= e_fbt_ult: {0} > {1} ", Math.Round(e_fbt_max, 8), Math.Round(Eps_fbt_ult,8)));

            // сжатие (e < 0)
            bool res_fb = e_fb_max_p >= Eps_fb_ult;
            if (res_fb)
                Msg.Add(string.Format("Проверка сечения по фибробетону на сжатие пройдена e_fb_max >= e_fb_ult: {0} >= {1} ", Math.Round(e_fb_max_p, 8), Math.Round(Eps_fb_ult, 8)));
            else
                Msg.Add(string.Format("Не пройдена проверка сечения по фибробетону на сжатие. e_fb_max >= e_fb_ult: {0} < {1} ", Math.Round(e_fb_max_p, 8), Math.Round(Eps_fb_ult,8)));

            bool res_s = e_s_max <= Eps_s_ult;
            if (res_s)
                Msg.Add(string.Format("Проверка сечения по арматуре пройдена. e_s_max={0} <= e_s_ult={1} ", Math.Round(e_s_max, 6), Eps_s_ult));
            else
                Msg.Add(string.Format("Не пройдена проверка сечения по арматуре. e_s_max={0} <= e_s_ult={1}", Math.Round(e_s_max, 6), Eps_s_ult));

            Msg.Add("---Предупреждения:------");
            foreach (int errid in ErrorIdx) 
            {
                if (DictErrors.TryGetValue(errid, out string _errval))
                    Msg.Add($"{_errval}");
            }

            _Message = Msg;
        }

        /// <summary>
        ///  Результаты расчета по 2 группе предельных состояний
        /// </summary>
        public Dictionary<string, double> GetResults2Group()
        {
            Res2Group = new Dictionary<string, double>();            

            Res2Group.Add("<b>--------Определение момента образования трещин [П6.2.8..]:--------</b>", double.NaN);

            AddToResult("M_crc", BSHelper.MU2U(M_crc), BSFiberLib.CG2, false);

            AddToResult("UtilRate_M_crc", BSHelper.MU2U(UtilRate_M_crc), BSFiberLib.CG2, false);

            if (!double.IsNaN(sig_s_crc))
                AddToResult("sig_s_crc", sig_s_crc, BSFiberLib.CG2, false);

            Res2Group.Add("<b>--------Расчет ширины раскрытия трещин [П6.2.14..]:--------</b>", double.NaN);
            if (!double.IsNaN(a_crc))
            {
                AddToResult("a_crc", a_crc, BSFiberLib.CG2, false);

                AddToResult("a_crc_ult", a_crc_ult, BSFiberLib.CG2, false);
            }

            Res2Group.Add("<b>--------Кривизна фибробетонных элементов [П6.2.22..]:--------</b>", double.NaN);
            if (!double.IsNaN(Kx_crc))
                AddToResult("Kx_crc", Kx_crc, BSFiberLib.CG2, false);
            if (!double.IsNaN(Ky_crc))
                AddToResult("Ky_crc", Ky_crc, BSFiberLib.CG2, false);



            return Res2Group;
        }

        public void InitFromCalcNDM(BSCalcNDM bsCalc1)
        {
            /*
            Sig_B = bsCalc1.SigmaBResult;
            Sig_S = bsCalc1.SigmaSResult;
            Eps_B = bsCalc1.EpsilonBResult;
            Eps_S = bsCalc1.EpsilonSResult;

            Eps_fbt_ult = bsCalc1.Eps_fbt_ult; // раст
            Eps_fb_ult  = bsCalc1.Eps_fb_ult; // сжатие

            RebarDiametersByIndex = bsCalc1.RebarDiametersByIndex;
            ErrorIdx.Add(bsCalc1.Err);
            */
        }
    }


    /// <summary>
    /// Стержень арматуры
    /// </summary>
    public class ReinforcementBar
    {
        /// <summary>
        /// Номер стержня арматуры
        /// </summary> 
        public int IndexOfBar { get; set; }

        /// <summary>
        /// Напряжение в арматуре
        /// </summary>
        public double Sig { get; set; }

        /// <summary>
        /// Относительная деформация в арматуре
        /// </summary>
        public double Eps { get; set; }

        /// <summary>
        /// Диаметр стержня арматуры
        /// </summary>
        public double Diameter { get; set; }

        /// <summary>
        /// Класс Арматуры
        /// </summary>
        public string Type { get; set; }

    }
}

