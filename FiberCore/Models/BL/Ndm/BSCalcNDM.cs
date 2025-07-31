using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Lib;
using MathNet.Numerics.Statistics;

namespace BSFiberCore.Models.BL.Ndm
{
    public partial class BSCalcNDM
    {
        /// <summary>
        /// группа предельных состояний
        /// </summary>
        private readonly int GroupLSD;
        /// <summary>
        /// Настройки расчета
        /// </summary>
        private readonly NDMSetup Setup;
        /// <summary>
        /// коэффициенты для расчета по трещиностойкости
        /// </summary>
        public NdmCrc NdmCrc { private get; set; }
        /// <summary>   
        /// рассчитывать ширину раскрыттия трещины
        /// </summary>
        public double Eps_s_crc { get; set; }
        // рассчитывать ли ширину раскрытия трещины
        private bool CalcA_crc => Eps_s_crc != 0;


        /// <summary>
        /// Деформационные параметры СП 6.1.25
        /// </summary>
        private void InitDeformParams()
        {
            // Деформационные параметры бетона на сжатие
            ebc0 = 0.002;
            ebc2 = 0.0035;

            // Деформационные параметры фибробетона на растяжение
            efbt0 = 0.0;
            efbt1 = 0.0;
            efbt2 = 0.00015;
            efbt2 = 0.004;
            efbt3 = 0.02;

            // Деформационные параметры арматуры
            // сжатие
            esc2 = 0.025;
            // растяжение
            est2 = 0.025;

            //Предельные деформационные параметры арматуры
            //e_s_ult = 0.025;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="_groupLSD"> группа предельных состояний</param>
        public BSCalcNDM(int _groupLSD)
        {
            GroupLSD = _groupLSD;
        }

        /// <summary>
        ///  основной конструктор 
        /// </summary>        
        public BSCalcNDM(int _groupLSD, BeamSection _BeamSection, NDMSetup _Setup)
        {
            GroupLSD = _groupLSD;
            BeamSection = _BeamSection;
            Setup = _Setup;
            NdmCrc = new NdmCrc();
            //Mesh
            ny = Setup.N;
            nx = Setup.M;
        }

        /// <summary>
        /// усилия переводятся :  кг, кг*см -> кН, кН*см
        /// </summary>
        /// <param name="_Mx">сейчас в расчете по ндм - ось  Z</param>
        /// <param name="_My"></param>
        /// <param name="_N"></param>
        public void SetMN(double _Mx, double _My, double _N)
        {
            Mx0 = BSHelper.MU2U(_Mx);
            My0 = BSHelper.MU2U(_My);
            N0 = BSHelper.NU2U(_N);
        }

        public void SetSizes(Dictionary<string, double> _D)
        {
            // size
            b = _D["b"];
            h = _D["h"];

            bf = _D["bf"];
            hf = _D["hf"];
            bw = _D["bw"];
            hw = _D["hw"];
            b1f = _D["b1f"];
            h1f = _D["h1f"];

            r1 = _D["r1"];
            R2 = _D["R2"];
        }

        public void SetE(Dictionary<string, double> _D)
        {
            // fiber beton - pressure
            Eb0 = BSHelper.RU2U(_D["Eb0"]);
            // fiber beton - tension
            Ebt = BSHelper.RU2U(_D["Ebt"]);
            // steel / rebar
            Es0 = BSHelper.RU2U(_D["Es0"]);
        }

        public void Deform_e(Dictionary<string, double> _D)
        {
            // предельные деформации - фибробетон
            //  сжатие
            ebc0 = _D["ebc0"];
            ebc2 = _D["ebc2"];

            // растяжение
            efbt0 = _D["ebt0"];
            efbt2 = _D["ebt2"];
            efbt3 = _D["ebt3"];

            // арматура
            // cжатие
            esc2 = _D["esc2"];
            // растяжение
            est2 = _D["est2"];
        }

        /// <summary>
        /// передаем параметр e_crc, полученный на предыдущем этапе при расчете момента трещинообразования
        /// для определения ширины раскрытия трещины
        /// </summary>
        /// <param name="_es">деформации арматуры</param>
        public void SetE_S_Crc(List<double> _es)
        {
            Eps_s_crc = _es.Maximum();
        }

        public void SetRGroup1(Dictionary<string, double> _D)
        {
            // сжатие
            Rbc = BSHelper.RU2U(_D["Rbc"]);
            // растяжение
            Rfbt = BSHelper.RU2U(_D["Rbt"]);
            Rfbt2 = BSHelper.RU2U(_D["Rbt2"]);
            Rfbt3 = BSHelper.RU2U(_D["Rbt3"]);

            Rsc = BSHelper.RU2U(_D["Rsc"]);
            Rst = BSHelper.RU2U(_D["Rst"]);
        }

        public void SetRGroup2(Dictionary<string, double> _D)
        {
            // сжатие
            Rbc = BSHelper.RU2U(_D["Rbcn"]);
            // растяжение
            Rfbt = BSHelper.RU2U(_D["Rbtn"]);
            Rfbt2 = BSHelper.RU2U(_D["Rbt2n"]);
            Rfbt3 = BSHelper.RU2U(_D["Rbt3n"]);

            Rsc = BSHelper.RU2U(_D["Rscn"]);
            Rst = BSHelper.RU2U(_D["Rstn"]);
        }

        // параметры для расчета
        public void SetParamsGroup1(Dictionary<string, double> _D)
        {
            SetSizes(_D);
            SetE(_D);
            Deform_e(_D);
            SetRGroup1(_D);
        }

        public void SetParamsGroup2(Dictionary<string, double> _D)
        {
            SetSizes(_D);
            SetE(_D);
            Deform_e(_D);
            SetRGroup2(_D);
        }

        /// <summary>
        /// увеличить усилия на занданный коэффициент
        /// цель=найти такое усилие при котором деформации превысят критические,
        /// тем самым будет найден момент трещинообразования по НДМ
        /// </summary>
        /// <param name="_coef">коэффициент</param>
        public void MxMyNUp(double _coef)
        {
            if (_coef == 0)
                return;

            Mx0 *= _coef;
            My0 *= _coef;
            N0 *= _coef;
        }

        /// <summary>
        /// Привязки арматуры
        /// </summary>
        /// <param name="_bD"></param>
        /// <param name="_bX"></param>
        /// <param name="_bY"></param>
        public void SetRods(List<double> _bD, List<double> _bX, List<double> _bY)
        {
            RebarDiametersByIndex = _bD;
            if (_bD == null) return;

            ds.Clear();
            d_nom.Clear();
            y0s.Clear();
            x0s.Clear();

            int idx = 0;
            foreach (var d in _bD)
            {
                ds.Add(d);
                d_nom.Add(d * 10); // mm

                x0s.Add(_bX[idx]);
                y0s.Add(_bY[idx]);
                idx++;
            }
        }

        public BeamSection BeamSection
        {
            set
            {
                m_BeamSection = value;
            }
        }

        private BeamSection m_BeamSection;

        #region Поля, свойства  - данные для расчета
        // Продольная сила, кН, - сжатие
        private double N0 = 0;
        // Момент отн. оси Y, кН*см
        private double My0 = 0;
        // Момент отн. оси X, кН*см
        private double Mx0 = 0;
        // Ширина сечения, см
        private double b = 0;
        // высота сечения, см
        private double h = 0;
        // тавр-двутавр
        private double bf, hf, bw, hw, b1f, h1f;
        // кольцо
        private double r1, R2;

        // число элементов вдоль y, шт
        private int ny = 0;
        // число элементов вдоль x шт.
        private int nx = 0;

        // диаметры арматурных стержней
        private readonly List<double> d_nom = new List<double>() { };
        private readonly List<double> ds = new List<double>() { };

        // привязки арматуры
        private readonly List<double> y0s = new List<double>() { };
        private readonly List<double> x0s = new List<double>() { };

        // Параметры материалов        
        //Начальный модуль бетона, кН/см2
        private double Eb0 = 0;
        //Начальный модуль упругости фибробетона, кН/см2
        private double Ebt = 0;
        // Расчетное сопротивление бетона на сжатие, кН/см2
        private double Rbc = 0;
        // Расчетное сопротивление фибробетона на растяжение, кН/см2
        private double Rfbt = 0;
        // Расчетное сопротивление фибробетона на сжатие, кН/см2
        private double Rfbt2 = 0;
        // Расчетное сопротивление фибробетона на сжатие, кН/см2
        private double Rfbt3 = 0;

        // Деформационные параметры бетона на сжатие
        private double ebc0;
        private double ebc2;

        // Деформационные параметры фибробетона на растяжение
        private double efbt0;
        private double efbt1;
        private double efbt2;
        private double efbt3;

        //СП 6.1.24
        //private double e_s_ult;
        private double e_fb_ult = 0;
        private double e_fbt_ult = 0;

        // Арматура кН/см2
        //Начальный модуль арматуры, кН/см2       
        private double Es0 = 0;
        // Прочность арматуры на растяжение        
        private double Rst = 0;
        // Прочность арматуры на сжатие
        private double Rsc = 0;

        // сжатие        
        private double esc0 = 0;
        private double esc2 = 0;
        // растяжение
        private double est0 = 0;
        private double est2 = 0;

        private List<double> My;
        private List<double> Mx;

        // проверка сечения на усилия                
        public double Mxint { get; private set; }
        public double Myint { get; private set; }
        public double Nint { get; private set; }

        // моменты образования трещины        
        public double Mx_crc { get; private set; }
        public double My_crc { get; private set; }
        public double N_crc { get; private set; }

        // деформация в момент образования трещины
        // - в арматуре:
        public double es_crc { get; private set; }
        // - в бетоне:
        public double ebt_crc { get; private set; }

        // напряжение в арматуре в сечении с трещиной
        public double sig_s_crc { get; private set; }

        // ширина раскрытия трещины
        public double a_crc { get; private set; }
        public List<double> A_Crc { get; private set; }
        #endregion

        // предельный деформации фибробетона по СП 
        public double Eps_fbt_ult => efbt1;
        public double Eps_fb_ult => -ebc0;

        // коэффициенты использоввания:
        //-- по деформациям фибробетона на сжатие
        public double UtilRate_fb_p { get; private set; }
        //-- по деформациям фибробетона на растяжение
        public double UtilRate_fb_t { get; private set; }
        //-- по деформациям арматуры на растяжение
        public double UtilRate_s_p { get; private set; }
        //-- по деформациям арматуры на растяжение
        public double UtilRate_s_t { get; private set; }

        // максимальное число итераций
        private int jmax = 20000;
        // Максимальная абсолютная погрешность
        private double tolmax = Math.Pow(10, -9);
        private int err = 0;

        private Dictionary<string, double> m_Results = new Dictionary<string, double>();

        public int Err => err;
        public Dictionary<string, double> Results => m_Results;

        /// <summary>
        /// напряжения в элементах сечения 
        /// </summary>
        public List<double> SigmaBResult { get; private set; }
        /// <summary>
        /// напряжения в сечении арматуры 
        /// </summary>
        public List<double> SigmaSResult { get; private set; }
        /// <summary>
        /// деформации в элементах сечения 
        /// </summary>
        public List<double> EpsilonBResult { get; private set; }
        /// <summary>
        /// деформации в арматуре 
        /// </summary>
        public List<double> EpsilonSResult { get; private set; }

        /// <summary>
        /// диаметры арматуры соответствующие номерам стержней арматуры
        /// </summary>
        public List<double> RebarDiametersByIndex { get; set; }

        #region разбивка сечения на элементы
        // массив привязок бетонных эл-в к вспомогательной оси y0
        private List<double> y0b = new List<double>();

        // массив привязок бетонных эл-в к вспомогательной оси x0
        private List<double> x0b = new List<double>();

        // массив площадей элементов
        private List<double> Ab = new List<double>();

        // массив площадей арматуры
        private List<double> As = new List<double>();
        #endregion

        private void InitSectionsLists()
        {
            Ab = new List<double>();
            y0b = new List<double>();
            x0b = new List<double>();
            As = new List<double>();
            A_Crc = new List<double>();
        }

        /// <summary>
        ///  Запустить расчет
        /// </summary>
        public bool Run()
        {
            bool ok;
            try
            {
                Calculate();
                ok = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ok = false;
            }
            return ok;
        }
    }
}
