using System.Reflection;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Uom;
using BSFiberCore.Models.BL.Mat;
using BSFiberCore.Models.BL.Lib;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace BSFiberCore.Models.BL.Rep
{
    /// <summary>
    /// Построитель отчета
    /// </summary>
    public class BSFiberReport
    {
        public string ReportName { get; set; }
        public Dictionary<string, double>? Beam { set { m_Beam = value; } }
        public Dictionary<string, double>? Coeffs { set { m_Coeffs = value; } }
        public Dictionary<string, double>? Efforts { set { m_Efforts = value; } }
        public Dictionary<string, double>? PhysParams { set { m_PhysParams = value; } }
        public Dictionary<string, double>? GeomParams { set { m_GeomParams = value; } }
        public Dictionary<string, double>? CalcResults1Group { set { m_CalcResults1Group = value; } }
        public Dictionary<string, double>? CalcResults2Group { set { m_CalcResults2Group = value; } }
        public Dictionary<string, double>? Reinforcement { set { m_Reinforcement = value; } }
        public List<string> Messages { set { m_Messages = value; }}
        public List<string> PictureToHeadReport { set { m_PictureToHeadReport = value; } }
        public List<string> PictureToBodyReport { set { m_PictureToBodyReport = value; } }


        public BeamSection BeamSection { set { m_BeamSection = value; } }
        public bool UseReinforcement { get; set; }

        protected Dictionary<string, double>? m_Beam;
        protected Dictionary<string, double>? m_Coeffs;
        protected Dictionary<string, double>? m_Efforts;
        protected Dictionary<string, double>? m_PhysParams;
        protected Dictionary<string, double>? m_GeomParams;
        protected Dictionary<string, double>? m_CalcResults1Group;
        protected Dictionary<string, double>? m_CalcResults2Group;
        protected Dictionary<string, double>? m_Reinforcement;
        protected List<string> m_Messages;
        protected List<string> m_Path2BeamDiagrams;

        protected List<string> m_PictureToHeadReport { get; set; }
        protected List<string> m_PictureToBodyReport { get; set; }


        public List<ReinforcementBar> ReinforcingBars;



        protected BeamSection m_BeamSection { get; set; }

        public LameUnitConverter? _unitConverter { get; set; }

        public string ImageCalc { get; set; }
        
        public MemoryStream? ImageStream {  get; set; }

        public BSFiberReport()
        {
            ReportName = "Сопротивление сечения из фибробетона";
            UseReinforcement = false;
        }

        private const int bk = 800, bv = 200;

        public void InitFromBSFiberReportData(BSFiberReport_N fiberReport)
        {
            Efforts           = fiberReport.m_Efforts;
            CalcResults1Group = fiberReport.m_CalcResults1Group;
            CalcResults2Group = fiberReport.m_CalcResults2Group;
            m_Messages        = fiberReport.m_Messages;
        }

        public void InitFromBSFiberReportData(BSFiberReportData _ReportData)
        {
            Efforts           = _ReportData.Efforts;
            CalcResults1Group = _ReportData.CalcResults1Group;
            CalcResults2Group = _ReportData.CalcResults2Group;
            m_Messages        = _ReportData.Messages;
        }

        /// <summary>
        ///  Верхняя часть отчета
        /// </summary>        
        public virtual void Header(StreamWriter w)
        {
            // w.WriteLine("<html><body>");
            w.WriteLine($"<H1>{ReportName}</H1>");
            w.WriteLine($"<H4>Расчет выполнен по {BSData.ProgConfig.NormDoc}</H4>");

            string beamDescr = typeof(BeamSection).GetCustomAttribute<DescriptionAttribute > (true).Description;
            string beamSection = BSHelper.EnumDescription(m_BeamSection);
            w.WriteLine($"<H2>{beamDescr}: {beamSection}</H2>");
            
            string _filename = string.IsNullOrWhiteSpace(ImageCalc) ? BSHelper.ImgResource(m_BeamSection, UseReinforcement) : ImageCalc;
            if (ImageStream == null && !string.IsNullOrEmpty(_filename))
            {                
                string path = Lib.BSData.ResourcePath(_filename);
                string img = MakeImageSrcData(path);
                w.WriteLine($"<table><tr><td> <img src={img}/ width=\"500\" height=\"300\"> </td></tr> </table>");
            }            
            else if (ImageStream != null)
            {                
                string img = MakeImageSrcData(ImageStream);
                w.WriteLine($"<table><tr><td> <img src={img}/ width=\"500\" height=\"300\"> </td></tr> </table>");
            }

            if (m_Beam != null)
            {
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                w.WriteLine("<caption>Балка</caption>");
                foreach (var _pair in m_Beam)
                {
                    w.WriteLine("<tr>");
                    w.WriteLine($"<td width={bk}><b>{_pair.Key}</b></td>");
                    w.WriteLine($"<td width={bv} align=center colspan=2>{_pair.Value} </td>");
                    w.WriteLine("</tr>");
                }
                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }
            if (m_GeomParams != null)
            {
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                w.WriteLine("<caption>Геометрия сечения</caption>");

                foreach (var _pair in m_GeomParams)
                {
                    if (_pair.Value != 0)
                    {
                        w.WriteLine("<tr>");
                        w.WriteLine($"<td width={bk}><b>{_pair.Key}</b></td>");
                        w.WriteLine($"<td width={bv} align=center>{_pair.Value}</td>");
                        w.WriteLine("</tr>");
                    }
                }

                w.WriteLine("</tr>");
                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }            
        }

        private double Rnd(double _v) => Math.Round( _v, 2);


        /// <summary>
        /// double 2 string converter
        /// </summary>        
        private string D2SValue(double _value)
        {
            return _value.ToString();
        }


        // Конвертор единиц измерения
        private string UConv(string _s, double _v)
        {            

            if (string.IsNullOrEmpty(_s))
                return "";
            else if ( _s.Contains("кг/см2"))
                return $"{Rnd( BSHelper.Kgsm2MPa(_v))} МПа";
            else if (_s.Contains("кг*см"))
                return $"{Rnd(BSHelper.kgssm2kNsm(_v))} Кн*см";
            else if ( _s.Contains("[кг]"))
                return $"{Rnd(BSHelper.Kgs2kN(_v))} Кн";

            return "";
        }


        /// <summary>
        /// Основная часть отчета
        /// </summary>        
        public virtual void ReportBody(StreamWriter w)
        {
            if (m_PhysParams != null)
            {
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                w.WriteLine("<caption>Физические характеристики</caption>");
                foreach (var _pair in m_PhysParams)
                {
                    w.WriteLine("<tr>");
                    w.WriteLine($"<td width={bk}>{_pair.Key}</td>");
                    w.WriteLine($"<td width={bv} align=center>{Math.Round(_pair.Value, 4)} </td>");
                    w.WriteLine($"<td width={bv} align=center>{UConv(_pair.Key, _pair.Value)} </td>");
                    w.WriteLine("</tr>");
                }

                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }

            if (m_Reinforcement != null)
            {
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                w.WriteLine("<caption>Армирование</caption>");
                foreach (var _pair in m_Reinforcement)
                {
                    w.WriteLine("<tr>");
                    w.WriteLine($"<td width={bk}>{_pair.Key}</td>");
                    w.WriteLine($"<td width={bv} align=center>{Math.Round(_pair.Value, 4)} </td>");
                    w.WriteLine("</tr>");
                }

                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }

            if (m_Coeffs != null)
            {
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                w.WriteLine("<caption>Коэффициенты</caption>");
                foreach (var _pair in m_Coeffs)
                {
                    w.WriteLine("<tr>");
                    w.WriteLine($"<td width={bk}>{_pair.Key}</td>");
                    w.WriteLine($"<td width={bv} align=center>{Math.Round(_pair.Value, 4)} </td>");
                    w.WriteLine("</tr>");
                }
                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }

            if (m_Path2BeamDiagrams != null && m_Path2BeamDiagrams.Count > 0)
            {
                // добавление картинок с эпюрами в отчет
                
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                foreach (string pathToBeamDiagram in m_Path2BeamDiagrams)
                {
                    w.WriteLine("<tr>");
                    w.WriteLine("<td>");
                    w.WriteLine($"<img src =\"{pathToBeamDiagram}\">");
                    w.WriteLine("</td>");
                    w.WriteLine("</tr>");
                }
                w.WriteLine("</Table>");
                w.WriteLine("<br>");

            }

            if (m_PictureToHeadReport != null && m_PictureToHeadReport.Count > 0)
            {
                // добавление картинок в отчет
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                foreach (string pictureToHeader in m_PictureToHeadReport)
                {
                    w.WriteLine("<tr>");
                    w.WriteLine("<td>");
                    w.WriteLine($"{pictureToHeader}");
                    w.WriteLine("</td>");
                    w.WriteLine("</tr>");
                }
                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }
        }

        /// <summary>
        /// Нагрузки
        /// </summary>        
        public void ReportEfforts(StreamWriter w)
        {
            if (m_Efforts != null)
            {
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                w.WriteLine("<caption>Усилия</caption>");
                foreach (var _pair in m_Efforts)
                {
                    // Костыль для ограничения выводимых в отчет нагрузок при выполнении расчета "Экспертиза балки"
                    //if (m_Path2BeamDiagrams != null && m_Path2BeamDiagrams.Count != 0)
                    //{
                    //    bool isWhat = _pair.Key.Contains("Mx") || _pair.Key.Contains("My") || _pair.Key.Contains("Qx");
                    //    if (!isWhat) continue;
                    //}

                    w.WriteLine("<tr>");
                    w.WriteLine($"<td width={bk}>{_pair.Key}</td>");
                    w.WriteLine($"<td width={bv} align=center>{_pair.Value} </td>");

                    string nameCustomUnitMeasure = "";
                    double newValue = _unitConverter?.ConvertEffortsForReport(_pair.Key, _pair.Value, out nameCustomUnitMeasure) ?? 0;
                    if (!string.IsNullOrEmpty(nameCustomUnitMeasure))
                    {
                        w.WriteLine($"<td width={bv} align=center>{newValue + " " + nameCustomUnitMeasure} </td>");
                    }

                    w.WriteLine("</tr>");
                }
                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }
        }

        public virtual void ReportResult(StreamWriter w)
        {            
            w.WriteLine("Расчет:");
            if (m_CalcResults2Group != null)
                w.WriteLine("<H3>Расчет по 1-й группе предельных состояний:</H3>");
            if (m_CalcResults1Group != null)
            {                
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                w.WriteLine("<tr>");

                foreach (var _pair in m_CalcResults1Group)
                {                    
                    w.WriteLine("<tr>");
                    w.WriteLine($"<td width={bk}>{_pair.Key}</td>");
                   
                    if (double.IsNaN(_pair.Value))
                    {
                        w.WriteLine($"<td width={bv} align=center colspan=2></td>");
                        w.WriteLine($"<td width={bv} align=center colspan=2></td>");
                    }
                    else if (Math.Abs(_pair.Value) < 0.00001)
                    {
                        string bgColor = ColorForUtilizationFactor(_pair);
                        w.WriteLine($"<td width={bv} align=center colspan=2 {bgColor}>{_pair.Value.ToString("E")} </td>");
                        w.WriteLine($"<td width={bv} align=center colspan=2>{UConv(_pair.Key, _pair.Value)} </td>");
                    }
                    else
                    {
                        string bgColor = ColorForUtilizationFactor(_pair);
                        w.WriteLine($"<td width={bv} align=center colspan=2 {bgColor}>{Math.Round(_pair.Value, 6)} </td>");
                        w.WriteLine($"<td width={bv} align=center colspan=2>{UConv(_pair.Key, _pair.Value)} </td>");
                    }

                    w.WriteLine("</tr>");
                }
                
                w.WriteLine("</tr>");
                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }
            else
            {
                w.WriteLine("<th>Расчет не выполнен</th>");
            }

            w.WriteLine("<H3>Расчет по 2-й группе предельных состояний:</H3>");
            if (m_CalcResults2Group != null)
            {
                w.WriteLine("<Table border=2 bordercolor = darkblue>");
                w.WriteLine("<tr>");

                foreach (var _pair in m_CalcResults2Group)
                {
                    w.WriteLine("<tr>");
                    w.WriteLine($"<td width={bk}>{_pair.Key}</td>");
                    
                    if (double.IsNaN(_pair.Value))
                    {
                        w.WriteLine($"<td width={bv} align=center colspan=2></td>");
                        w.WriteLine($"<td width={bv} align=center colspan=2></td>");
                    }
                    else if (_pair.Value < 0.001)
                    {
                        string bgColor = ColorForUtilizationFactor(_pair);
                        w.WriteLine($"<td width={bv} align=center colspan=2 {bgColor}>{_pair.Value.ToString("E")} </td>");
                        w.WriteLine($"<td width={bv} align=center colspan=2>{UConv(_pair.Key, _pair.Value)} </td>");
                    }
                    else
                    {
                        string bgColor = ColorForUtilizationFactor(_pair);
                        w.WriteLine($"<td width={bv} align=center colspan=2 {bgColor}>{Math.Round(_pair.Value, 6)} </td>");
                        w.WriteLine($"<td width={bv} align=center colspan=2> {UConv(_pair.Key, _pair.Value)} </td>");
                    }

                    w.WriteLine("</tr>");
                }

                w.WriteLine("</tr>");
                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }
            else
            {
                w.WriteLine("<th>Расчет не выполнен</th>");
            }

            if (m_PictureToBodyReport != null && m_PictureToBodyReport.Count > 0)
            {
                // добавление картинок в отчет
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                w.WriteLine("<tr>");

                foreach (string picToBody in m_PictureToBodyReport)
                {
                    w.WriteLine("<td>");
                    w.WriteLine($"{picToBody}");
                    w.WriteLine("</td>");
                }
                w.WriteLine("</tr>");
                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }

            if (ReinforcingBars != null)
            {
                if (ReinforcingBars.Count != 0)
                {
                    w.WriteLine("<Table border=2 bordercolor = darkblue>");
                    w.WriteLine("<tr>");

                    // Заголовок таблицы
                    w.WriteLine("<tr>");
                    w.WriteLine($"<td width={bk/4}> Номер стержня </td>");
                    w.WriteLine($"<td width={bk/4}> Диаметр </td>");
                    w.WriteLine($"<td width={bk/4}> Относительная деформация</td>");
                    w.WriteLine($"<td width={bk/4}> Напряжение, кг/см2</td>");
                    w.WriteLine($"<td width={bk / 4}> Вид деформации </td>");
                    w.WriteLine("</tr>");

                    foreach (ReinforcementBar value in ReinforcingBars)
                    {
                        w.WriteLine("<tr>");
                        w.WriteLine($"<td width={bk / 4}>{value.IndexOfBar}</td>");
                        w.WriteLine($"<td width={bk / 4}>{value.Diameter}</td>");                        
                        w.WriteLine($"<td width={bk / 4}>{convertDoubleToString(value.Eps)}</td>");
                        w.WriteLine($"<td width={bk / 4}>{convertDoubleToString(value.Sig)}</td>");
                        string deformType = "";
                        if (value.Eps > 0) deformType = "Растяжение"; else if (value.Eps < 0) deformType = "Сжатие"; 
                        w.WriteLine($"<td width={bk / 4}>{deformType}</td>");
                        w.WriteLine("</tr>");
                    }

                    w.WriteLine("</tr>");
                    w.WriteLine("</Table>");
                    w.WriteLine("<br>");

                }

            }
        }

        
        private string MakeImageSrcData(string _filename)
        {
            if (_filename == "") return "";

            string _img = "";
            try
            {
                using (Image img = Image.FromFile(_filename))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Png);

                        byte[] imgBytes = ms.ToArray();
                        string _extension = Path.GetExtension(_filename).Replace(".", "").ToLower();

                        _img = String.Format("\"data:image/{0};base64, {1}\" alt = \"{2}\" ", _extension, Convert.ToBase64String(imgBytes), _filename);
                    }
                }
            }
            catch (Exception _e) 
            {
                _img = _e.Message;
            }
            
            return _img;
        }

        private string MakeImageSrcData(MemoryStream  _ms, string _filename = "section.png")
        {            
            string _img = "";
            try
            {                
                using (MemoryStream ms = _ms)
                {                    
                    byte[] imgBytes = ms.ToArray();
                    string _extension = Path.GetExtension(_filename).Replace(".", "").ToLower();

                    _img = String.Format("\"data:image/{0};base64, {1}\" alt = \"{2}\" ", _extension, Convert.ToBase64String(imgBytes), _filename);
                }                
            }
            catch (Exception _e)
            {
                _img = _e.Message;
            }

            return _img;
        }

        public virtual void Footer(StreamWriter w)
        {
            if (m_Messages != null)
            {
                w.WriteLine("<Table border=1 bordercolor = darkblue>");
                w.WriteLine("<caption>Итог:</caption>");
                foreach (var _value in m_Messages)
                {
                    w.WriteLine("<tr>");                    
                    w.WriteLine($"<td width={bk}>| {_value} </td>");
                    w.WriteLine("</tr>");
                }
                w.WriteLine("</Table>");
                w.WriteLine("<br>");
            }

            //w.WriteLine("</body></html>");            
        }

        /// <summary>
        /// Сформировать отчет
        /// </summary>
        /// <param name="_fileIdx">Присвоить номер</param>
        /// <returns>Путь к файлу</returns>
        public string CreateReport(int _fileIdx = 0)
        {
            string pathToHtmlFile = "";
            string filename = "FiberCalculationReport{0}.htm";
            try
            {
                filename = (_fileIdx == 0) ? string.Format(filename, "") : string.Format(filename, _fileIdx);

                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                    {
                        Header(w);

                        ReportBody(w);

                        ReportEfforts(w);
                        
                        ReportResult(w);

                        Footer(w);
                    }

                    pathToHtmlFile = fs.Name;
                }
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка при формировании отчета: " + _e.Message);
                pathToHtmlFile = "";
            }

            return pathToHtmlFile;
        }


        /// <summary>
        /// Формирует  Header ReportBody и для MultiReport
        /// </summary>
        /// <param name="pathToFile"></param>
        /// <returns></returns>
        public void HeaderForMultiReport(string pathToFile)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(pathToFile, true, Encoding.UTF8))
                {
                    Header(w);
                    ReportBody(w);
                }
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка при формировании отчета: " + _e.Message); ;
            }
        }

        /// <summary>
        /// Формирует  ReportEfforts ReportResult и Footer для MultiReport
        /// </summary>
        /// <param name="pathToFile"></param>
        /// <returns></returns>
        public void BodyForMultiReport(string pathToFile)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(pathToFile, true, Encoding.UTF8))
                {
                    ReportEfforts(w);
                    ReportResult(w);
                    Footer(w);
                }
            }
            catch (Exception _e)
            {
                MessageBox.Show("Ошибка при формировании отчета: " + _e.Message); ;
            }
        }


        /// <summary>
        /// Для Коэффициент использования у которых значение превышает 1 или -1 добавляется строчка изменяющая цвет фона таблицы на красный
        /// </summary>
        /// <param name="pair"></param>
        /// <returns></returns>
        public string ColorForUtilizationFactor(KeyValuePair<string,double> pair)
        {
            string bgColor = "";
            if (pair.Key.Contains("Коэффициент использования"))
            {
                if (pair.Value > 1 || pair.Value < -1)
                {
                    bgColor = "bgcolor=\"#FF3333\"";
                }
                else
                { 
                    bgColor = "bgcolor=\"#33CC00 \"";
                }
            }
            return bgColor; 
        }


        public void CreateRebarTable(List<double> Diameters, List<double> Eps_S, List<double> Sig_S)
        {
            if (Diameters !=null && Eps_S != null && Sig_S != null && (Eps_S?.Count == Sig_S?.Count) && (Diameters?.Count == Eps_S?.Count))
            {
                if (ReinforcingBars == null) { ReinforcingBars = new List<ReinforcementBar>(); }
                ReinforcingBars.Clear();

                for (int i = 0; i < Diameters.Count; i++)
                {
                    ReinforcingBars.Add(new ReinforcementBar()
                    {
                        IndexOfBar = i + 1,
                        Sig = Sig_S[i],
                        Eps = Eps_S[i],
                        Diameter = Diameters[i]
                    });
                 }
            }                
        }

        public static string convertDoubleToString(double value)
        {
            string strValue = "";
            if (Math.Abs(value) < 0.00001)
            {
                strValue = value.ToString("E");
            }
            else
            {
                strValue = Math.Round(value, 6).ToString();
            }
            return strValue;
        }

    }
}
