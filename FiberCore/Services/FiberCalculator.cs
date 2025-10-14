using BSFiberCore.Models.BL;
using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Ndm;
using BSFiberCore.Models.BL.Rep;
using BSFiberCore.Models.BL.Sec;
using BSFiberCore.Models.BL.Tri;
using System.Threading.Tasks;
using TriangleNet.Geometry;

namespace FiberCore.Services;


/// <summary>
/// Represents a fiber-reinforced concrete beam for structural calculations.
/// </summary>
public class FiberCalculator
{
    #region userparams

    public BSFiberMain fiberMain { get; set; }

    public int Id { get; set; }
    public int CalcType { get; set; } // 0 -static_eq 1 - ndm 
    public string FiberQ { get; set; }        
    public string FiberAns { get; set; }

    // размеры
    public int SectionType { get; set; }
    public double Length { get; set; } = 0;
    public double Width { get; set; } = 30;
    public double Height { get; set; } = 60;
    public double b => Width;
    public double h => Height;
    public double bf { get; set; } = 30;
    public double hf { get; set; } = 60;
    public double bw { get; set; } = 30;
    public double hw { get; set; } = 60;
    public double b1f { get; set; } = 30;
    public double h1f { get; set; } = 60;

    public double R2 { get; set; } = 60;
    public double R1 { get; set; } = 30;

    // класс бетона
    public string BetonType { get; set; } = "Тяжелый";

    public string BetonIndex { get; set; } = "a";

    public string Bft3 { get; set; } = "B3i";

    public string Bft { get; set; } = "Bft3";

    public string Bfb { get; set; } = "B30";

    // усилия внешние
    public double Mx { get; set; } = 0;
    public double My { get; set; } = 1000;
    public double N { get; set; } = 0;
    public double Qx { get; set; } = 0;
    public double Qy { get; set; } = 0;
    // Эксцентриситет
    public double Ml { get; set; }
    public double eN { get; set; }
    public double e0 { get; set; }

    // арматура
    public double As { get; set; } = 0;

    public double A1s { get; set; } = 0;

    public double a_cm { get; set; } = 0;

    public double a1_cm { get; set; } = 0;

    public string A_Rs { get; set; } = "A240";

    public string A_Rsc { get; set; } = "A240";

    public double Rs { get; set; }

    public double Rsc { get; set; }

    // модуль упругости арматуры
    public double Es { get; set; } = 2039432.40;
    // модули упругости 
    public double Ef { get; set; } = 2141404.0200; // фибра
    public double Efbt { get; set; } = 367607.6901; //фибробетон-растяжение
    public double Eb { get; set; } = 331407.7650; // бетон-матрица, сжатие
    public double mu_fv { get; set; } = 0.0200; // коэффициент фибрового армирования

    // коэффициенты надежности
    public double Yft { get; set; } = 1.3;
    public double Yb { get; set; } = 1.3;
    public double Yb1 { get;  set; } = 0.9;
    public double Yb2 { get;  set; } = 0.9;
    public double Yb3 { get;  set; } = 0.9;
    public double Yb5 { get;  set; } = 0.9;

    #endregion
 
    public FiberCalculator()
    {
        //Efb = 2141404.0200;
        //
        fiberMain = new BSFiberMain();
    }

    /// <summary>
    /// Runs the calculation based on the selected calculation type.
    /// </summary>
    /// <returns>HTML content of the calculation report.</returns>
    public async Task<string> RunCalc()
    {
        if (CalcType == 0)
        {
            return await RunCalcStaticEqAsync();
        }
        else if (CalcType == 1)
        {
            return await RunCalcNDMAsync();
        }
        else
            return "Не выбран тип расчета";
    }

    private  async Task<string> RunCalcStaticEqAsync()
    {
        List<BSFiberReportData> calcResults_MNQ = new List<BSFiberReportData>();

        bool use_reinforcement = As > 0 || A1s > 0;

        InitFiberMain(use_reinforcement);

        Task task = fiberMain.SelectMaterialFromList();
        await task;

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

    private BSFiberMain InitFiberMain(bool use_reinforcement)
    {
        fiberMain.UseReinforcement = use_reinforcement;
        fiberMain.BeamSection = (BeamSection)SectionType;
        fiberMain.Fiber = this;
        
        fiberMain.InitSize();
        
        return fiberMain;
    }

    public async Task<string?> RunCalcNDMAsync()
    {
        try
        {
            InitFiberMain(false);

            await fiberMain.SelectMaterialFromList();

            BSSectionChart SectionChart = new BSSectionChart();

            List<BSCalcResultNDM> calcResults = new List<BSCalcResultNDM>();

            GetEffortsFromForm(out List<Dictionary<string, double>> lstMNQ);

            if (!ValidateNDMCalc(lstMNQ))
                return "Err";

            var beamSection = (BeamSection)SectionType;
            foreach (Dictionary<string, double> efforts in lstMNQ)
            {
                fiberMain.MNQ = efforts;

                Point CG = new Point(0, 0);
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
