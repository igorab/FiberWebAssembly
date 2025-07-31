using BSFiberCore.Models.BL.Calc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSFiberCore.Models.BL.Rep
{
    public class BSFiberReport_MNQ : BSFiberReport
    {
        private BSFiberCalc_MNQ m_FiberCalc;
        // атрибуты свойств
        private Dictionary<string, string> m_PropAttr;

        /// <summary>
        /// Отчет на действие Q по наклонной полосе 
        /// </summary>        
        public static BSFiberReport_MNQ FiberReport_Qc(BSFiberCalc_MNQ fiberCalc, int _irep)
        {
            BSFiberReport_MNQ report = new BSFiberReport_MNQ()
            {
                BeamSection    = fiberCalc.BeamSectionType(),
                ImageCalc      = fiberCalc.ImageCalc(),
                Messages       = fiberCalc.Msg,
                _unitConverter = fiberCalc.UnitConverter
            };

            report.InitFromFiberCalc(fiberCalc);
            // результаты расчета по 1 гр пред состояний
            report.CalcResults1Group = fiberCalc.CalcResults;
            // для расчета по второй группе пред состояний
            report.CalcResults2Group = fiberCalc.CalcResults2Group;

            return report;

            //string pathToHtmlFile = report.CreateReport(_irep);
            //System.Diagnostics.Process.Start(pathToHtmlFile);
        }

        public BSFiberReportData GetBSFiberReportData()
        {
            BSFiberReportData data = new BSFiberReportData
            {
                BeamSection       = m_BeamSection,
                UseReinforcement  = UseReinforcement,
                Coeffs            = m_Coeffs,
                Efforts           = m_Efforts,
                GeomParams      = m_GeomParams,
                Messages          = m_Messages,
                PhysParams      = m_PhysParams,
                UnitConverter     = _unitConverter,
                // результат расчета по первой группе предельных состояний
                CalcResults1Group = m_CalcResults1Group,
                // результат расчета по второй группе предельных состояний
                CalcResults2Group = m_CalcResults2Group
            };

            return data;
        }
    
        public BSFiberReport_MNQ()
        {
            m_PropAttr = new Dictionary<string, string>();
            m_GeomParams = new Dictionary<string, double>();
            m_Coeffs = new Dictionary<string, double>();
            m_PhysParams = new Dictionary<string, double>();
            m_CalcResults1Group = new Dictionary<string, double>();

            ReportName = typeof(BSFiberCalc_MNQ).GetCustomAttribute<DisplayNameAttribute>().DisplayName;
        }

        public virtual void InitFromFiberCalc(BSFiberCalc_MNQ _fiberCalc)
        {
            m_FiberCalc    = _fiberCalc;
            m_BeamSection  = m_FiberCalc.BeamSectionType();            
            m_Messages     = _fiberCalc.Msg;
            _unitConverter = _fiberCalc.UnitConverter;
            
            m_Efforts = new Dictionary<string, double>()
            {
                {"My,[кг*см]", _fiberCalc.m_Efforts["My"]},
                {"N, [кг]",    _fiberCalc.m_Efforts["N"]},
                {"Qx, [кг]",   _fiberCalc.m_Efforts["Qx"]}
            };

            ImageCalc = _fiberCalc.ImageCalc();

            GetPropertiesAttr();

            InitFromAttr();
        }

        // получить параметры из свойств (атрибутов)
        private void GetPropertiesAttr()
        {
            PropertyInfo[] props = typeof(BSFiberCalc_MNQ).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var attrs = prop.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs.Length > 0)
                { 
                    DescriptionAttribute attrDescr = attrs.Cast<DescriptionAttribute>().Single();
                    string descr = attrDescr.Description;

                    var attributes = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    if (attributes.Length > 0)
                    {
                        DisplayNameAttribute attr = attributes.Cast<DisplayNameAttribute>().Single();
                        string displayName = attr.DisplayName;

                        m_PropAttr.Add(prop.Name, displayName + '@' + descr);
                    }                    
                }
            }            
        }

        private void InitFromAttr()
        {            
            Type myType = typeof(BSFiberCalc_MNQ);
            foreach (var attr in m_PropAttr)
            {
                PropertyInfo prop = myType.GetProperty(attr.Key);

                object value = prop?.GetValue(m_FiberCalc);

                string[] attrDescr = attr.Value.ToString().Split('@');

                string attrValue = attrDescr[0];
                string attrType = attrDescr[1];
                
                if (double.TryParse(value.ToString(), out double _d))
                {
                    switch (attrType)
                    {
                        case "Geom":
                            if (_d > 0)
                                m_GeomParams.Add(attrValue, _d);
                            break;
                        case "Coef":   
                            m_Coeffs.Add(attrValue, _d);
                            break;
                        case "Phys":
                            m_PhysParams.Add(attrValue, _d) ;
                            break;
                        case "Res":
                            m_CalcResults1Group.Add(attrValue, _d);
                            break;
                        case "Beam":
                            m_Beam.Add(attrValue, _d);
                            break;
                    }                        
                }
            }

            /*
         m_Beam;                  
            */
        }
    }
    

}
