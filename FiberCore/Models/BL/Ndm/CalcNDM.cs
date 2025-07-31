using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Lib;
using System;
using System.Collections.Generic;

namespace BSFiberCore.Models.BL.Ndm
{
    public class CalcNDM
    {
        const int GR1 = BSFiberLib.CG1;
        const int GR2 = BSFiberLib.CG2;

        private BeamSection m_BeamSection;

        private double My0, Mx0, N0;

        // данные с формы
        public Dictionary<string, double> Dprm { get; set; }

        public NDMSetup setup { get; set; }

        public BSCalcResultNDM CalcRes => m_CalcRes;
        private BSCalcResultNDM m_CalcRes;

        //привязка арматуры(по X - высота, по Y ширина балки)
        private double LeftX;

        private List<double> Xs { get; set; }
        private List<double> Ys { get; set; }

        private List<double> lD;
        private List<double> lX;
        private List<double> lY;

        /// <summary>
        /// Статусы расчета, отражаемые в отчете
        /// </summary>
        private List<string> m_Message;

        public CalcNDM(BeamSection _BeamSection)
        {
            Xs = new List<double>();
            Ys = new List<double>();

            m_BeamSection = _BeamSection;
            setup = BSData.LoadNDMSetup();
            LeftX = 0;
        }

        /// <summary>
        ///  Интерполяция
        /// </summary>
        /// <param name="_Y">Усилия (моменты)</param>
        /// <param name="_X">Коэфф использования материала </param>
        /// <param name="_x">искомый коэфф использования</param>
        /// <returns>Определяем момент, при котром коэф использования = 1  </returns>
        public double Y_interpolate(double[] _Y, double[] _X, double _x)
        {             
            Lagrange.Lagrange lagrange = new Lagrange.Lagrange();

            double value = lagrange.GetValue(_X, _Y, _x);
         
            return value;
        }

        private void Init()
        {
            if (Dprm == null) return;

            // для прямоугольных и тавровых сечений привязка к центу нижней грани 
            if (BSHelper.IsRectangled(m_BeamSection))
            {
                LeftX = Dprm.ContainsKey("b") ? -Dprm["b"] / 2.0 : 0;
            }

            double _qty, _area;
            
            (lD, lX, lY, _qty, _area) = BSCalcNDM.ReinforcementBinding(m_BeamSection, LeftX, 0, setup.UseRebar);

            if (!Dprm.ContainsKey("rods_qty"))
                Dprm.Add("rods_qty", _qty);
            if (!Dprm.ContainsKey("rods_area"))
                Dprm.Add("rods_area", _area);

            My0 = Dprm["My"];
            Mx0 = Dprm["Mx"];
            N0  = Dprm["N"];
        }

        ///
        /// выполнить расчет по 1 группе предельных состояний
        ///
        private BSCalcNDM BSCalcGr1(double _coefM = 1.0)
        {
            Init();
            BSCalcNDM bsCalcGR1 = new BSCalcNDM(GR1, m_BeamSection, setup);
            bsCalcGR1.SetMN(Mx0, My0, N0);
            bsCalcGR1.SetParamsGroup1(Dprm);
            bsCalcGR1.MxMyNUp(_coefM);
            bsCalcGR1.SetRods(lD, lX, lY);
            bsCalcGR1.Run();

            return bsCalcGR1;
        }

        ///
        /// выполнить расчет по 2 группе предельных состояний
        ///
        private BSCalcNDM BSCalcGr2(double _Mx, double _My, double _N)
        {
            BSCalcNDM bscalc = new BSCalcNDM(GR2, m_BeamSection, setup);
            bscalc.SetParamsGroup2(Dprm);
            bscalc.SetMN(_Mx, _My, _N);
            bscalc.MxMyNUp(1.0); 
            bscalc.SetRods(lD, lX, lY);
            bscalc.Run();

            return bscalc;
        }

        
        // Расчет по 2 группе предельных состояний - ширина раскрытия трещины           
        BSCalcNDM BSCalcGr2_a_Crc(double _coefM, List<double> _E_s_crc = null)
        {
            NdmCrc ndmCrc = BSData.LoadNdmCrc();
            ndmCrc.InitFi2(setup.RebarType);
            ndmCrc.InitFi3(Dprm["N"]);
           
            BSCalcNDM bscalc = new BSCalcNDM(GR2, m_BeamSection, setup);
            bscalc.SetParamsGroup2(Dprm);
            bscalc.SetMN(Mx0, My0, N0);
            bscalc.MxMyNUp(_coefM);
            bscalc.NdmCrc = ndmCrc;
            bscalc.SetRods(lD, lX, lY);
            bscalc.SetE_S_Crc(_E_s_crc);
            bscalc.Run();

            m_CalcRes.ErrorIdx.Add(bscalc.Err);
            m_CalcRes.SetRes2Group(bscalc.Results, false, true);
            return bscalc;
        }

        /// <summary>
        /// Для определения прогиба балки
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, double> RunMy(double _My)
        {            
            Init();
            BSCalcNDM bsCalcGR1 = new BSCalcNDM(GR1, m_BeamSection, setup);
            bsCalcGR1.SetParamsGroup1(Dprm);
            bsCalcGR1.SetMN(0, _My, 0);
            bsCalcGR1.SetRods(lD, lX, lY);
            bsCalcGR1.Run();
            
            return bsCalcGR1.Results;            
        }

        /// <summary>
        /// Выполнить расчет по 1 г пред сост
        /// </summary>
        /// <returns></returns>
        public bool RunGroup1()
        {
            BSCalcNDM bsCalcGR1 = BSCalcGr1();
            
            m_CalcRes = new BSCalcResultNDM(bsCalcGR1.Results);
            m_CalcRes.InitCalcParams(Dprm);            
            m_CalcRes.InitFromCalcNDM(bsCalcGR1);           
            m_CalcRes.ResultsMsg1Group(ref m_Message);

            return true;
        }

        private bool Validate()
        {
            bool res = true;

            if (Dprm["Mz"] == 0 && Dprm["My"] == 0 && Dprm["N"] == 0)
            {
                res = false;
            }            
            return res;
        }

        /// <summary>
        ///  GO!
        /// </summary>
        public void Run()
        {
            if (!Validate())
                return;

            Init();

            bool ok = RunGroup1();

            if (ok)
            {                
                if (setup.UseRebar && setup.FractureStressType > 0) 
                {
                    // расчет по 2 группе с арматурой:
                    double mx0 = Mx0, my0= My0, n0 = N0;
                    // определение момента трещинообразования
                    double ur = RunGroup2_UtilRate();
                    if (ur > 1.0)
                    {
                        mx0 = Mx0 / ur;
                        my0 = My0 / ur;
                        n0 = N0 / ur;
                    }
                    BSCalcNDM bsCalc_Mcrc = RunGroup2_Mcrc(mx0, my0, n0);

                    // определение ширины раскрытия трещины 
                    //-- параметр трещинообразования, для расчета ширины раскрытия трещины
                    List<double> E_S_crc = bsCalc_Mcrc.EpsilonSResult;                    
                    //-- расчитываем на заданные моменты и силы
                    BSCalcNDM bsCalc_crc = BSCalcGr2_a_Crc(1.0, E_S_crc);
                }
                else
                {
                    // расчет по 2 группе без арматуры
                    // Трещины не допускаются, расчет по ширине раскрытия трещины не производится
                    BSCalcNDM bscalc = BSCalcGr2(Mx0, My0, N0);
                    m_CalcRes.ErrorIdx.Add(bscalc.Err);
                    m_CalcRes.SetRes2Group(bscalc.Results);
                }
            }            
        }

        // Расчет по 2 группе предельных состояний - момент трещинообразования          
        private BSCalcNDM bsсalcgr2_Mcrc(double _coefM, double _Mx, double _My, double _N)
        {
            BSCalcNDM bscalc = BSCalcGr2(_Mx* _coefM, _My* _coefM, _N* _coefM);
            m_CalcRes.ErrorIdx.Add(bscalc.Err);
            m_CalcRes.SetRes2Group(bscalc.Results);

            // Определение момента образования трещины            
            if (bscalc.UtilRate_fb_t <= 1.0)
            {
                if (!Ys.Contains(_coefM))
                {
                    Xs.Add(bscalc.UtilRate_fb_t); // увеличение усилия
                    Ys.Add(_coefM);  // коэф использования по материалу
                }
            }
            return bscalc;
        }

        /// <summary>
        ///  Определетить предварительно коэффициент использования сечения по 2 гр пр сост
        /// </summary>
        /// <returns></returns>
        private double RunGroup2_UtilRate()                        
        {
            BSCalcNDM bsCalc1 = BSCalcGr2(Mx0, My0, N0);
            double ur = bsCalc1.UtilRate_fb_t;
            return ur;                
        }

        ///
        /// выполнить расчет по 2 группе предельных состояний
        /// 
        public BSCalcNDM RunGroup2_Mcrc(double mx0, double my0, double n0)
        {            
            // 1 этап
            // определяем моменты трещинообразования от кратковременных и длительных нагрузок (раздел X)                        
            // используем заданные усилия и определяем коэфф использования по 2-гр пр сост                                                                       
            double coef = 1;

            BSCalcNDM bscalc0 = bsсalcgr2_Mcrc(coef, mx0, my0, n0);
            double ur = bscalc0.UtilRate_fb_t;

            // Если же хотя бы один из моментов трещинообразования оказывается меньше
            // соответствующего действующего момента, выполняют второй этап расчета.
            double dH = 1;
            // применяем переменный шаг
            int iters = 0;
            
            while (ur < 0.8)
            {               
                BSCalcNDM bscalc = bsсalcgr2_Mcrc(coef, mx0, my0, n0);

                iters++;
                if (iters > 100) break;
                if (bscalc.UtilRate_fb_t > 0.8) break;
                coef += dH;
                ur = bscalc.UtilRate_fb_t;
            }
            if (coef >1)
                coef -= dH;

            dH = 0.2;
            for (int N = 1; N <= 100; N++)
            {
                coef += dH;
                BSCalcNDM _bsCalc = bsсalcgr2_Mcrc(coef, mx0 , my0 , n0);
                ur = _bsCalc.UtilRate_fb_t;
                if (_bsCalc.UtilRate_fb_t > 1)
                    break;
            }

            double y_coef = coef; // Y_interpolate(Ys.ToArray(), Xs.ToArray(), 1.0);                
            BSCalcNDM bsCalc_Mcrc = bsсalcgr2_Mcrc(y_coef, mx0, my0, n0);
            ur = bsCalc_Mcrc.UtilRate_fb_t;
            if (ur > 1.2) //коэффициент использования
            {
                coef = y_coef - dH / 2.0;
                bsCalc_Mcrc = bsсalcgr2_Mcrc(coef, mx0, my0, n0);
                ur = bsCalc_Mcrc.UtilRate_fb_t;
                double My_crc = bsCalc_Mcrc.My_crc;  //  момент трещинообразования                                                         
            }

            return bsCalc_Mcrc;                            
        }
    }
}
