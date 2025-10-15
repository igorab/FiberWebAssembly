using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Mat;
using System.ComponentModel;
using System.Reflection;

namespace BSFiberCore.Models.BL.Calc
{
    [DisplayName("Сталефибробетонные конструкции без предварительного напряжения арматуры")]
    public class BSFiberCalculation : ICalc
    {
        public List<string> Msg = new List<string>();
        public BSMatFiber MatFiber { get; set; }

        [DisplayName("Нормативное сопротивления осевому растяжению, Rfbt,n, [кг/см2]")]
        public double Rfbtn { get => MatFiber.Rfbtn; }

        [DisplayName("Сопротивление сталефибробетона осевому растяжению, Rfbt, [кг/см2]")]
        public double Rfbt { get => R_fbt(); }

        [DisplayName("Нормативное остаточное сопротивления осевому растяжению Rfbt3,n, [кг/см2]")]
        public double Rfbt3n { get => MatFiber.Rfbt3n; }

        [DisplayName("Остаточное сопротивление сталефибробетона осевому растяжению, Rfbt3, [кг/см2]")]
        public double Rfbt3 { get => R_fbt3(); }

        [DisplayName("Числовая характеристика класса фибробетона по прочности на осевое сжатие, B")]
        public double B { get => MatFiber.B; }

        [DisplayName("Нормативное значение сопротивления сталефибробетона на осевое сжатие Rfb,n, [кг/см2]")]
        public double Rfbn { get => MatFiber.Rfbn; }

        [DisplayName("Расчетные значения сопротивления  на сжатиие по B30 СП63, [кг/см2]")]
        public double Rfb { get => R_fb(); }

        public double Gamma(double _B) => 1.73 - 0.005 * (_B - 15);

        protected double Omega { get => MatFiber.Omega; }
        
        protected double Yft;        
        protected double Yb;
        protected double Yb1;
        protected double Yb2;
        protected double Yb3;
        protected double Yb5;
                
        public virtual Dictionary<string, double> Coeffs
        {
            get
            {
                return new Dictionary<string, double>() { { "Yft", Yft }, { "Yb", Yb }, { "Yb1", Yb1 }, { "Yb2", Yb2 }, { "Yb3", Yb3 }, { "Yb5", Yb5 } };
            }
        }
        public virtual Dictionary<string, double> PhysParams
        {
            get
            {
                return new Dictionary<string, double> { { "Rfbt3n", Rfbt3n }, { "B", B }, { "Rfbn", Rfbn } };
            }
        }

        public Dictionary<string, double> Efforts
        {
            get
            {
                Dictionary<string, double> efforts = new Dictionary<string, double>();
                if (m_Efforts.ContainsKey("My"))
                    efforts.Add("My, [кг*см] ", m_Efforts["My"]);
                if (m_Efforts.ContainsKey("Mx"))
                    efforts.Add("Mx, [кг*см]", m_Efforts["Mx"]);
                if (m_Efforts.ContainsKey("N"))
                    efforts.Add("N, [кг]", m_Efforts["N"]);
                if (m_Efforts.ContainsKey("Q"))
                    efforts.Add("Q, [кг]", m_Efforts["Q"]);

                return efforts;
            }
            set { m_Efforts = new Dictionary<string, double>(value); }
        }

        protected Dictionary<string, double> m_Efforts;

        // Расчетные значения сопротивления на сжатиие по B30 СП63
        public double R_fb() => (Yb != 0) ? Rfbn / Yb * Yb1 * Yb2 * Yb3 * Yb5 : 0;

        //Расчетное остаточное сопротивление осевому растяжению R_fbt
        public double R_fbt() => (Yft != 0) ? Rfbtn / Yft * Yb1 * Yb5 : 0;

        //Расчетное остаточное сопротивление осевому растяжению R_fbt3
        public double R_fbt3() => (Yft != 0) ? Rfbt3n / Yft * Yb1 * Yb5 : 0;

        public string DN(Type _T, string _property) => _T.GetProperty(_property).GetCustomAttribute<DisplayNameAttribute>().DisplayName;

        public static string DsplN(Type _T, string _property) => new BSFiberCalculation().DN(_T, _property);

        public BSFiberCalculation()
        {
        }

        /// <summary>
        /// Возвращает результаты расчета геометрических характеристик балки
        /// </summary>
        /// <returns>Описание геометрии балки</returns>
        public virtual Dictionary<string, double> GeomParams()
        {
            return new Dictionary<string, double>() { };
        }

        /// <summary>
        /// Физические свойства материала
        /// </summary>        
        public virtual Dictionary<string, double> PhysicalParameters()
        {
            Dictionary<string, double> phys = new Dictionary<string, double>
            {
                { DN(typeof(BSFiberCalculation), "Rfbt3n"), Rfbt3n },
                { DN(typeof(BSFiberCalculation), "B"), B },
                { DN(typeof(BSFiberCalculation), "Rfbn"), Rfbn }
            };

            return phys;
        }

        /// <summary>
        /// Принимает характерные размеры сечения
        /// </summary>
        /// <param name="_t">Массив - размеры сечения</param>
        public virtual void SetSize(double[] _t)
        {
        }

        public virtual void SetParams(double[] _t)
        {
        }

        public virtual bool Validate()
        {
            return true;
        }

        public virtual bool Calculate()
        {
            if (!Validate())
                return false;

            return true;
        }

        public virtual Dictionary<string, double> Results()
        {
            return new Dictionary<string, double>() { };
        }

        /// <summary>
        /// Информация о результате проверки сечения на действие изгибающего момента
        /// </summary>                
        public void InfoCheckM(double _M_ult)
        {
            string info;

            if (m_Efforts["My"] <= _M_ult)
                info = "Сечение прошло проверку на действие изгибающего момента.";
            else
                info = "Сечение не прошло проверку. Рассчитанный предельный момент сечения превышает предельно допустимый.";
            Msg.Add(info);

            info = "Расчет успешно выполнен!";
            Msg.Add(info);
        }

        public virtual BeamSection BeamSectionType()
        {
            return BeamSection.Any;
        }

        public virtual bool UseRebar()
        {
            return false;
        }


        /// <summary>
        /// Расчет прочности сечения
        /// </summary>
        /// <param name="_profile">Профиль сечения</param>
        /// <param name="_reinforcement">Используется ли арматура</param>
        /// <returns>Экземпляр класса расчета</returns>
        public static BSFiberCalculation Construct(BeamSection _profile, bool _reinforcement = false)
        {
            switch (_profile)
            {                
                case BeamSection.TBeam:
                case BeamSection.LBeam:
                case BeamSection.IBeam:
                    if (_reinforcement)
                        return new BSFiberCalc_IBeamRods();
                    else
                        return new BSFibCalc_IBeam();
                case BeamSection.Ring:
                    return new BSFibCalc_Ring();
                case BeamSection.Rect:
                    if (_reinforcement)
                        return new BSFiberCalc_RectRods();
                    else
                        return new BSFibCalc_Rect();                
                default:
                    return new BSFibCalc_Rect();
            }            
        }
    }
}
