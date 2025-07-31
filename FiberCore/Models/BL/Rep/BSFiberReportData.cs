using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Uom;

namespace BSFiberCore.Models.BL.Rep
{
    public class BSFiberReportData
    {
        public bool UseReinforcement { get; set; }
        public Dictionary<string, double> m_Beam { get; private set; }
        public Dictionary<string, double> Coeffs { get; set; }
        public Dictionary<string, double> Efforts { get; set; }
        public Dictionary<string, double> PhysParams { get; set; }
        public Dictionary<string, double> GeomParams { get; set; }
        public Dictionary<string, double> CalcResults1Group { get; set; }
        public Dictionary<string, double> CalcResults2Group { get; set; }
        public Dictionary<string, double> m_Reinforcement { get; private set; }
        public List<string> Messages { get; set; }
        public List<string> m_Path2BeamDiagrams;
        public BeamSection BeamSection { get; set; }
        public LameUnitConverter? UnitConverter { get; set; }
        public string ImageCalc { get; set; }
        public MemoryStream? ImageStream {  get; set; }

        public BSFiberReportData()
        {
            Messages = [];
            m_Beam = [];
            Coeffs = [];
            Efforts = [];
            PhysParams = [];
            GeomParams = [];
            CalcResults1Group = [];
            CalcResults2Group = [];
            m_Reinforcement = [];
            m_Path2BeamDiagrams = [];
            ImageStream = null;
            ImageCalc = "";
        }


        public void InitFromBSFiberCalculation(BSFiberCalculation _BSFibCalc, LameUnitConverter _UnitConverter)
        {
            BeamSection = _BSFibCalc.BeamSectionType();
            UseReinforcement = _BSFibCalc.UseRebar();
            Coeffs = _BSFibCalc.Coeffs;
            Efforts = _BSFibCalc.Efforts;
            GeomParams = _BSFibCalc.GeomParams();
            CalcResults1Group = _BSFibCalc.Results();
            Messages = _BSFibCalc.Msg;
            PhysParams = _BSFibCalc.PhysicalParameters();
            UnitConverter = _UnitConverter;
        }        
    }
}
