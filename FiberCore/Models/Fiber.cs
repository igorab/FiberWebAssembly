using BSFiberCore.Models.BL;
using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Draw;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Ndm;
using BSFiberCore.Models.BL.Rep;
using BSFiberCore.Models.BL.Sec;
using BSFiberCore.Models.BL.Tri;
using TriangleNet.Geometry;

namespace BSFiberCore.Models
{
    public class Fiber
    {
        #region userparams
        
        public int Id { get; set; }
        public int CalcType { get; set; } // 0 -static_eq 1 - ndm 
        public string FiberQ { get; set; }        
        public string FiberAns { get; set; }

        // размеры
        public int SectionType { get; set; }
        public double Length { get; set; }       
        public double Width { get; set; }
        public double Height { get; set; }
        public double b => Width;
        public double h => Height;
        public double bf { get; set; }
        public double hf { get; set; }
        public double bw { get; set; }
        public double hw { get; set; }
        public double b1f { get; set; }
        public double h1f { get; set; }

        public double R2 { get; set; }
        public double R1 { get; set; }

        // класс бетона
        public string BetonType { get; set; }

        public string BetonIndex { get; set; }

        public string Bft3 { get; set; }

        public string Bft { get; set; }

        public string Bfb { get; set; }

        // усилия внешние
        public double Mx { get; set; }
        public double My { get; set; }
        public double N { get; set; }
        public double Qx { get; set; }
        public double Qy { get; set; }
        // Эксцентриситет
        public double Ml { get; set; }
        public double eN { get; set; }
        public double e0 { get; set; }

        // арматура
        public double As { get; set; }

        public double A1s { get; set; }

        public double a_cm { get; set; }

        public double a1_cm { get; set; }

        public string A_Rs { get; set; }

        public string A_Rsc { get; set; }

        public double Rs { get; set; }

        public double Rsc { get; set; }

        // модуль упругости арматуры
        public double Es { get; set; }
        // модули упругости 
        public double Ef { get; set; } // фибра
        public double Efbt { get; set; } //фибробетон-растяжение
        public double Eb { get; set; } // бетон-матрица, сжатие
        public double mu_fv { get; set; } // коэффициент фибрового армирования

        // коэффициенты надежности
        public double Yft { get;  set; }
        public double Yb { get;  set; }
        public double Yb1 { get;  set; }
        public double Yb2 { get;  set; }
        public double Yb3 { get;  set; }
        public double Yb5 { get;  set; }

        #endregion

        public Fiber()
        {
            FiberQ = "";
            FiberAns = "";
            Bft3 = "";
            Bft = "";
            Bfb = "";
            BetonType = "";
            BetonIndex = "";
            A_Rs = "";
            A_Rsc = "";
            //Efb = 2141404.0200;            
        }

        /// <summary>
        ///  Расчеты по методу предельных усилий
        /// </summary>
        public string RunCalc()
        {
            if (CalcType == 0)
                return RunCalcStaticEq();
            else if (CalcType == 1)
                return RunCalcNDM();
            else
                return "";
        }

        private string RunCalcStaticEq()
        {
            List<BSFiberReportData> calcResults_MNQ = new List<BSFiberReportData>();

            bool use_reinforcement = As > 0 || A1s > 0;
            BSFiberMain fiberMain = FiberMain(use_reinforcement);

            double[] prms = { Yft, Yb, Yb1, Yb2, Yb3, Yb5 };

            Dictionary<string, double> mnq = new Dictionary<string, double>() { ["My"] = My, ["N"] = N, ["Qx"] = Qx };

            double Mc_ult, UtilRate_Mc;
            double N_ult, UtilRate_N;

            int iRep = 0;

            if (My > 0 && N == 0 && Qx == 0)
            {
                // расчет на чистый изгиб
                BSFiberReportData fibCalc_M = fiberMain.FiberCalculate_M(My, prms);
                calcResults_MNQ.Add(fibCalc_M);
            }
            else if (N > 0 && Qx == 0)
            {
                // Расчет по 1 гр пред. сост                    
                BSFiberCalc_MNQ fiberCalc_N = fiberMain.FibCalc_MNQ(mnq, prms);

                // расчет на действие сжимающей силы (учитывает заданный изгибающий момент + момент от эксцентриситета N)
                (N_ult, UtilRate_N) = fiberCalc_N.Calculate_Nz();

                // [6.1.13] [6.1.30] + проверка на момент по наклонному сечению
                if (My != 0) // + M = N*e учесть
                {
                    (Mc_ult, UtilRate_Mc) = fiberCalc_N.Calculate_Mc();
                }

                BSFiberReport_N report = new BSFiberReport_N() { };
                report.InitFromFiberCalc(fiberCalc_N);

                BSFiberCalc_Cracking calcResults2Group = fiberMain.FiberCalculate_Cracking(mnq);
                report.CalcResults2Group = calcResults2Group.Results();

                calcResults_MNQ.Add(report.GetBSFiberReportData());

            }
            else if (Qx != 0)
            {
                // Расчет на действие поперечных сил     
                // учитывает расчет по накл полосе надействие момента                    
                BSFiberCalc_MNQ fiberCalc_Qc = fiberMain.FibCalc_MNQ(mnq, prms);

                // [6.1.27] [6.1.28]
                (double Qc_ult, double UtilRate_Qc) = fiberCalc_Qc.Calculate_Qcx();
                if (N > 0)
                {
                    // [6.1.13] [6.1.30]
                    (N_ult, UtilRate_N) = fiberCalc_Qc.Calculate_Nz();
                }

                if (My > 0) // + M = N*e учесть
                {
                    (Mc_ult, UtilRate_Mc) = fiberCalc_Qc.Calculate_Mc();
                }

                BSFiberCalc_Cracking calcResults2Group = fiberMain.FiberCalculate_Cracking(mnq);
                fiberCalc_Qc.CalcResults2Group = calcResults2Group.Results();

                var report = BSFiberReport_MNQ.FiberReport_Qc(fiberCalc_Qc, ++iRep);

                calcResults_MNQ.Add(report.GetBSFiberReportData());

            }

            // расчет по наклонной полосе на действие момента [6.1.7]
            string htmlcontent = BSFiberReport_M.RunMultiReport(calcResults_MNQ);
            return htmlcontent;
        }

        private BSFiberMain FiberMain(bool use_reinforcement)
        {
            BSFiberMain fiberMain = new BSFiberMain()
            {
                UseReinforcement = use_reinforcement,
                BeamSection = (BeamSection)SectionType,
                Fiber = this
            };

            fiberMain.InitSize();
            fiberMain.InitMaterials();
            fiberMain.SelectMaterialFromList();
            
            return fiberMain;
        }

        public string RunCalcNDM()
        {
            try
            {
                BSFiberMain fiberMain = FiberMain(false);
                
                BSSectionChart SectionChart = new BSSectionChart();

                List<BSCalcResultNDM> calcResults = new List<BSCalcResultNDM>();

                GetEffortsFromForm(out List<Dictionary<string, double>> lstMNQ);

                if (!ValidateNDMCalc(lstMNQ))
                    return "Err";

                var beamSection = (BeamSection)SectionType;
                foreach (Dictionary<string, double> efforts in lstMNQ)
                {
                    fiberMain.MNQ = efforts;

                    TriangleNet.Geometry.Point CG = new TriangleNet.Geometry.Point(0, 0);
                    BSCalcResultNDM calcRes = null;
                   
                    if (beamSection == BeamSection.Rect)
                    {
                        calcRes = fiberMain.CalcNDM(BeamSection.Rect);
                    }
                    else if (BSHelper.IsITL(beamSection))
                    {
                        calcRes = fiberMain.CalcNDM(beamSection);
                    }
                    else if (beamSection == BeamSection.Ring)
                    {
                        fiberMain.GenerateMesh(ref CG);
                        calcRes = fiberMain.CalcNDM(BeamSection.Ring);
                    }
                    else if (beamSection == BeamSection.Any)
                    {
                        MeshSectionSettings meshSettings = new MeshSectionSettings();
                        SectionChart.GenerateMesh(meshSettings.MaxArea);

                        calcRes = fiberMain.CalcNDM(BeamSection.Any);
                    }

                    if (calcRes != null)
                    {
                        //calcRes.ImageStream = m_SectionChart.GetImageStream;
                        calcResults.Add(calcRes);
                    }
                }

                //CreatePictureForHeaderReport(calcResults);
                fiberMain.CreatePictureForBodyReport(calcResults);

                // формирование отчета
                string html = BSReport.RunReport(beamSection, calcResults);

                return html;

            }
            catch (Exception _e)
            {
                return MessageBox.Show(_e.Message);
            }            
        }
                
        private bool ValidateNDMCalc(List<Dictionary<string, double>> lstMNQ)
        {            
            return true;
        }

        private void GetEffortsFromForm(out List<Dictionary<string, double>> lstMNQ)
        {
            Dictionary<string, double> mnq = new Dictionary<string, double>()
            {
                ["Mx"] = Mx,
                ["My"] = My,
                ["N"]  = N,
                ["Qx"] = Qx,
                ["Qy"] = Qy,
                ["Ml"] = Ml,
                ["eN"] = eN,
                ["e0"] = e0,
            };

            lstMNQ = new List<Dictionary<string, double>> { mnq };           
        }
    }
}
