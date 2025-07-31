using BSFiberCore.Models.BL.Calc;
using BSFiberCore.Models.BL.Lib;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace BSFiberCore.Models.BL.Rep
{
    [DisplayName("Расчет элементов на действие продольной силы")]    
    public class BSFiberReport_N : BSFiberReport_MNQ
    {
        private BSFiberReportData? m_ReportData;
        
        private bool UseRebar { get; set; }

        private List<BSFiberReport_N>? ListFiberReportData;

        public BSFiberReport_N()
        {
            ReportName = typeof(BSFiberReport_N).GetCustomAttribute<DisplayNameAttribute>().DisplayName;
        }

        public override void InitFromFiberCalc(BSFiberCalc_MNQ _fiberCalc)
        {
            base.InitFromFiberCalc(_fiberCalc);
           
            m_CalcResults1Group = _fiberCalc.Results();
        }
                

        private void InitReportSections(ref BSFiberReport report)
        {
            report.Beam = null;
            report.Coeffs = m_ReportData?.Coeffs;
            report.Efforts = m_ReportData?.Efforts;
            report.GeomParams = m_ReportData?.GeomParams;
            report.PhysParams = m_ReportData?.PhysParams;
            report.Reinforcement = m_ReportData?.m_Reinforcement;
            report.CalcResults1Group = m_ReportData?.CalcResults1Group;
            report.CalcResults2Group = m_ReportData?.CalcResults2Group;
            report.ImageStream = m_ReportData?.ImageStream;
            report.Messages = m_ReportData.Messages;
            report._unitConverter = m_ReportData?.UnitConverter;
            report.UseReinforcement = m_ReportData.UseReinforcement;
        }


        /// <summary>
        /// сформировать отчет по различным загружениям
        /// </summary>
        public void CreateMultiReport()
        {
            try
            {
                if (m_ReportData == null)
                    throw new Exception("Не выполнен расчет");

                string pathToHtmlFile = "";
                string _reportName = "";
                int fileIdx = 0;

                BSFiberReport report = new BSFiberReport();

                if (_reportName != "")
                    report.ReportName = _reportName;
                report.BeamSection = m_BeamSection;

                var data = ListFiberReportData[0];

                InitReportSections(ref report);

                string filename = "FiberCalculationReport{0}.htm";
                try
                {
                    filename = (fileIdx == 0) ? string.Format(filename, "") : string.Format(filename, fileIdx);

                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                        {
                            report.Header(w);

                            report.ReportBody(w);

                            foreach (var fiberReport in ListFiberReportData)
                            {
                                report.InitFromBSFiberReportData(fiberReport);
                                report.ReportEfforts(w);
                                report.ReportResult(w);
                            }

                            report.Footer(w);
                        }

                        pathToHtmlFile = fs.Name;
                    }
                }
                catch (Exception _e)
                {
                    MessageBox.Show("Ошибка при формировании отчета: " + _e.Message);
                    pathToHtmlFile = "";
                }

                System.Diagnostics.Process.Start(pathToHtmlFile);

                //
                //    string errMsg = "";
                //    foreach (string ms in m_Msg) errMsg += ms + ";\t\n";

                //    MessageBox.Show(errMsg);
                //}
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка в отчете " + _e.Message);
            }
        }       
    }
}
