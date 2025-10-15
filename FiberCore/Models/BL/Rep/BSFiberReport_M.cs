using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Uom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Windows;

namespace BSFiberCore.Models.BL.Rep
{
    public class BSFiberReport_M
    {
        private BeamSection m_BeamSection;
        private IEnumerable<string> m_Msg;
        private BSFiberReportData m_ReportData;
        private bool UseRebar { get; set; }

        private List<BSFiberReportData> ListFiberReportData;

        /// <summary>
        /// данные для формирование общей части отчета
        /// </summary>
        public BSFiberReportData BSFibCalc
        {
            set
            {
                m_ReportData = value;
                m_BeamSection = value.BeamSection;
                m_Msg = value.Messages;
                UseRebar = value.UseReinforcement;
                UnitConverter = value.UnitConverter;
            }
        }

        public LameUnitConverter? UnitConverter { get; set; }

        /// <summary>
        /// Отчет по нескольким загружениям 
        /// </summary>
        /// <param name="iRep"></param>
        /// <param name="_calcResults"></param>
        public static string RunMultiReport(List<BSFiberReportData> _calcResults)
        {
            if (_calcResults != null && _calcResults.Count > 0)
            {
                BSFiberReport_M fiberReport_M = new BSFiberReport_M { ListFiberReportData = _calcResults };
                fiberReport_M.BSFibCalc = _calcResults[0];
                return fiberReport_M.CreateMultiReport();
            }
            else
            {
                return MessageBox.Show("Нет данных для отчета!", "Проверка сечения" /*, MessageBoxButton.OK, MessageBoxImage.Error*/);
            }
        }

        /// <summary>
        /// сформировать отчет по различным загружениям
        /// </summary>
        public string CreateMultiReport()
        {
            try
            {
                if (m_ReportData == null)
                    throw new Exception("Не выполнен расчет");

                string pathToHtmlFile = "";
                string _reportName = "";
                int LoadIdx = 0;

                BSFiberReport report = new BSFiberReport();

                if (_reportName != "")
                    report.ReportName = _reportName;
                report.BeamSection = m_BeamSection;
                InitReportSections(ref report);

                string filename = "FiberCalculationReport.htm";
                try
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                        {
                            report.Header(w);

                            report.ReportBody(w);

                            foreach (BSFiberReportData fiberReport in ListFiberReportData)
                            {
                                w.WriteLine($"<H2>Расчет по комбинации загружений: {++LoadIdx}</H2>");

                                report.InitFromBSFiberReportData(fiberReport);
                                report.ReportEfforts(w);
                                report.ReportResult(w);
                                report.Footer(w);
                            }
                        }

                        pathToHtmlFile = fs.Name;
                    }
                }
                catch (Exception _e)
                {
                    MessageBox.Show("Ошибка при формировании отчета: " + _e.Message);
                    pathToHtmlFile = "";
                    return pathToHtmlFile;
                }

                //System.Diagnostics.Process.Start(pathToHtmlFile);

                var htmlContent = System.IO.File.ReadAllText(pathToHtmlFile);
                return htmlContent;
            }
            catch (Exception _e)
            {
                return MessageBox.Show("Ошибка в отчете " + _e.Message);
            }
        }

        private void InitReportSections(ref BSFiberReport report)
        {
            report.Beam = null;
            report.Coeffs = m_ReportData.Coeffs;
            report.Efforts = m_ReportData.Efforts;
            report.GeomParams = m_ReportData.GeomParams;
            report.PhysParams = m_ReportData.PhysParams;
            report.Reinforcement = m_ReportData.m_Reinforcement;
            report.CalcResults1Group = m_ReportData.CalcResults1Group;
            report.CalcResults2Group = m_ReportData.CalcResults2Group;
            report.ImageStream = m_ReportData.ImageStream;
            report.Messages = m_ReportData.Messages;
            report._unitConverter = m_ReportData.UnitConverter;
            report.UseReinforcement = m_ReportData.UseReinforcement;
        }
    }
}
