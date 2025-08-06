using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Lib;
using BSFiberCore.Models.BL.Mat;
using BSFiberCore.Models.BL.Uom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;


namespace BSFiberCore.Models.BL.Calc
{    
    [DisplayName("Расчет элементов на действие сил и моментов")]
    ///
    /// Расчеты на действие продольной силы
    /// Расчеты по полосе между наклонными сечениями на дейстиве M и Q
    ///
    public class BSFiberCalc_MNQ : ICalc
    {
        public List<string> Msg;

        public Dictionary<string, double> CalcResults => Results();

        protected string m_ImgCalc { get; set; }

        #region attributes

        [DisplayName("Высота сечения, [см]"), Description("Geom")]
        public double h { get; protected set; }

        [DisplayName("Ширина сечения, [см]"), Description("Geom")]
        public double b { get; protected set; }

        [DisplayName("Радиус внешний, [см]"), Description("Geom")]
        public double r2 { get; protected set; }

        [DisplayName("Радиус внутренний, [см]"), Description("Geom")]
        public double r1 { get; protected set; }

        [DisplayName("Расчетная длина элемента, [см]"), Description("Geom")]
        public double LngthCalc0 { get; protected set; }

        [DisplayName("Площадь сечения, [см2]"), Description("Geom")]
        public double A { get; protected set; }

        [DisplayName("Момент инерции сечения, [см4]"), Description("Geom")]
        public double I { get; protected set; }
        
        [DisplayName("Модуль упругости сталефибробетона, [кг/см2]"), Description("Phys")]
        public double Efb { get; protected set; }
        
        [DisplayName("Нормативное сопротивление осевому сжатию, [кг/см2]"), Description("Phys")]
        public double Rfbn { get => MatFiber?.Rfbn ?? 0; }
        
        [DisplayName("Нормативное сопротивление осевому растяжению, [кг/см2]"), Description("Phys")]
        public double Rfbtn { get => MatFiber?.Rfbtn ?? 0; }
        
        [DisplayName("Нормативное остаточное сопротивления осевому растяжению, [кг/см2]"), Description("Phys")]
        public double Rfbt3n { get => MatFiber?.Rfbt3n ?? 0; }

        [DisplayName("Продольное усилие, [кг]"), Description("Phys")]
        public double N { get; protected set; }

        [DisplayName("Cлучайный эксцентриситет, принимаемый по СП 63, e0"), Description("Phys")]
        public double e0 { get; private set; }

        [DisplayName("Эксцентриситет приложения силы N, eN"), Description("Phys")]
        public double e_N { get; protected set; }

        [DisplayName("Относительное значение эксцентриситета продольной силы"), Description("Phys")]
        public double delta_e { get; protected set; }

        [DisplayName("Изгибающий момент Mx"), Description("Phys")]
        public double Mx { get; protected set; }

        [DisplayName("Изгибающий момент My"), Description("Phys")]
        public double My { get; protected set; }

        [DisplayName("Доля постоянной нагрузки в общей нагрузке на элемент"), Description("Phys")]
        public double Ml1toM1 { get; protected set; }

        [DisplayName("Коэффициент ф.")]
        public double k_b { get; protected set; }

        [DisplayName("Коэффициент, учитывающий влияние длительности действия нагрузки  (6.27)")]
        public double fi1 { get; protected set; }

        [DisplayName("Коэффициент надежности Yft"), Description("Coef")]
        public double Yft { get; protected set; }
        [DisplayName("Коэффициент условия работы Yb"), Description("Coef")]
        public double Yb { get; protected set; }
        [DisplayName("Коэффициент условия работы Yb1"), Description("Coef")]
        public double Yb1 { get; protected set; }
        [DisplayName("Коэффициент условия работы Yb2"), Description("Coef")]
        public double Yb2 { get; protected set; }
        [DisplayName("Коэффициент условия работы Yb3"), Description("Coef")]
        public double Yb3 { get; protected set; }
        [DisplayName("Коэффициент условия работы Yb5"), Description("Coef")]
        public double Yb5 { get; protected set; }

        [DisplayName("Предельный момент My (проверка по наклонному сечению), [кг*см]"), Description("Res")]
        public double M_ult { get; protected set; }

        [DisplayName("Предельная поперечная сила Q, [кг]"), Description("Res")]
        public double Qc_ult { get; protected set; }

        [DisplayName("Предельная поперечная сила Qy, [кг]"), Description("Res")]
        public double Qy_ult { get; protected set; }

        [DisplayName("Предельная поперечная сила Qx, [кг]"), Description("Res")]
        public double Qx_ult { get; protected set; }

        [DisplayName("Коэффициент использования по Qx, [П6.1.28]")]
        public double UtilRate_Qx { get; protected set; }

        [DisplayName("Коэффициент использования по Qy, [П6.1.28]")]
        public double UtilRate_Qy { get; protected set; }

        [DisplayName("Коэффициент использования Q по полосе между наклонными сечениями, [П6.1.27]")]
        public double UtilRate_Qс { get; protected set; }

        [DisplayName("Коэффициент использования по моменту My, [П6.1.30]")]
        public double UtilRate_My { get; protected set; }

        [DisplayName("Коэффициент использования по моменту Mx, [П6.1.30]")]
        public double UtilRate_Mx { get; protected set; }

        [DisplayName("Предельная продольная сила N, [кг]"), Description("Res")]
        public double N_ult { get; protected set; }

        [DisplayName("Коэффициент использования по усилию N, [П6.1.14 П6.1.15]")]
        public double UtilRate_N { get; protected set; }

        public string DN(Type _T, string _property) => _T.GetProperty(_property).GetCustomAttribute<DisplayNameAttribute>().DisplayName;
        #endregion
        public BetonType BetonType { get; set; }
        /// <summary>
        /// Свойства арматуры: продольная/поперечная
        /// </summary>
        public Rebar? Rebar {protected get; set;}
        public bool UseRebar { get; set; }

        /// <summary>
        /// Сила вне сечения
        /// </summary>
        public bool N_Out{ get; set; }
        /// <summary>
        /// Сила внутри сечения
        /// </summary>
        public bool N_In { get; set; }
        public bool Shear { get; set; }

        protected FiberBeton? m_Fiber;

        public BSMatRod MatRod { get; set; }
        public BSMatFiber MatFiber { get; set; }
                
        protected double Rfb;

        //Расчетное остаточное остаточного сопротивления осевому растяжению 
        protected double Rfbt3;

        protected double B;
        protected double y_t;        
        protected double fi;
        protected double Ef; //для фибры из тонкой низкоуглеродистой проволоки МП п.п  кг/см2 
        protected double Eb; //Начальный модуль упругости бетона-матрицы B30 СП63
        //коэффициент фибрового армирования по объему
        protected double mu_fv;
       
        // жесткость элемента в предельной по прочности стадии,определяемая по формуле (6.25)
        protected double D;
        //условная критическая сила, определяемая по формуле (6.24)
        protected double Ncr;
        //коэффициент, учитывающий влияние продольного изгиба (прогиба) элемента на его несущую способность и определяемый по формуле(6.23)
        protected double eta;
        //площадь сжатой зоны бетона ф. (6.22)
        protected double Ab;
        //поперечная сила
        protected double Qx;
        protected double Qy;
        
        public Dictionary<string, double> m_Efforts;

        protected BSBeam m_Beam;

        public BSFiberCalc_MNQ()
        {
            m_Beam = new BSBeam();
            m_Efforts = new Dictionary<string, double>();
            Msg = new List<string>();            
            fi = BSFiberLib.Fi;
        }

        /// <summary>
        ///  Рассчитать случайный эксцентриситет CG 63 п 8.1.7
        /// </summary>        
        /// <param name="_length"> Длина см</param>
        /// <param name="_h"> высота сечения см</param>
        /// <returns>e_а</returns>
        public static double Calc_e_a(double _length, double _h)
        {
            double e_a = 1.0;

            if (_length != 0 && _h != 0)            
                e_a = Math.Max(1.0,  Math.Max(_length/60.0,  _h/ 30.0));

            return e_a; // см
        }

        /// <summary>
        ///  Расчетная схема , рисунок
        /// </summary>
        /// <returns>Наименование файла</returns>
        public virtual string ImageCalc() => !string.IsNullOrEmpty(m_ImgCalc) ? m_ImgCalc : "";
                        
        public double Delta_e(double _d_e)
        {
            double d_e;

            if (_d_e <= 0.15)
            {
                d_e = 0.15;
            }
            else if (_d_e >= 1.5)
            {
                d_e = 1.5;
            }
            else
                d_e = _d_e;

            return d_e;
        }

        public double Fi1() => (Ml1toM1 <= 1) ? 1 + Ml1toM1 : 2.0;

        protected double R_fb() => Rfbn / Yb * Yb1 * Yb2 * Yb3 * Yb5;

        protected double R_fbt() => Rfbtn / Yft * Yb1 * Yb5;

        protected double R_fbt3() => Rfbt3n / Yft * Yb1 * Yb5;

        public double Dzeta_R(double omega) => omega / (1 + MatRod.epsilon_s() / MatFiber.e_b2);

        public double K_b(double _fi1, double _delta_e) => 0.15 / (_fi1 * (0.3d + _delta_e));

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="_BeamSection">Тип сечения</param>
        /// <returns></returns>        
        public static BSFiberCalc_MNQ Construct(BeamSection _BeamSection)
        {
            BSFiberCalc_MNQ fiberCalc;

            if (BeamSection.Rect == _BeamSection)
            {
                fiberCalc = new BSFiberCalc_MNQ_Rect();
            }
            else if (BeamSection.Ring == _BeamSection)
            {
                fiberCalc = new BSFiberCalc_MNQ_Ring();
            }
            else if (BSHelper.IsITL(_BeamSection))                   
            {
                if (BeamSection.TBeam == _BeamSection)
                    fiberCalc = new BSFiberCalc_MNQ_IT_T();
                else if (BeamSection.LBeam == _BeamSection)
                    fiberCalc = new BSFiberCalc_MNQ_IT_L();
                else
                    fiberCalc = new BSFiberCalc_MNQ_IT();
            }
            else
            {
                throw new Exception("Сечение балки не определено");
            }

            return fiberCalc;
        }
                        
        /// <summary>
        /// Информация о результате проверки сечения на действие продольной силы
        /// </summary>                
        public void InfoCheckN(double _N_ult)
        {
            string info;

            if (m_Efforts["N"] <= _N_ult)
                info = "Сечение прошло проверку на действие продольной силы.";
            else
                info = "Сечение не прошло проверку на действие силы N.";
            Msg.Add(info);            
        }

        public void Calculate_N()
        {
            //Коэффициент, учитывающий влияние длительности действия нагрузки, определяют по формуле (6.27)
            fi1 = Fi1();

            //относительное значение эксцентриситета продольной силы
            delta_e = Delta_e(m_Fiber.e_tot / m_Beam.h);

            // Коэфициент ф.(6.26)
            k_b = K_b(fi1, delta_e);

            // Модуль упругости сталефибробетона п.п. (5.2.7)
            Efb = MatFiber.Efb; //  m _Fiber.Eb * (1 - m_Fiber.mu_fv) + m_Fiber.Ef * m_Fiber.mu_fv;

            //жесткость элемента в предельной по прочности стадии,определяемая по формуле (6.25)
            D = k_b * Efb * I;

            // условная критическая сила, определяемая по формуле (6.24)
            Ncr = Math.PI * Math.PI * D / Math.Pow(LngthCalc0, 2);

            eta = (Ncr!=0) ? 1 / (1 - N / Ncr) : 0;

            Ab = m_Beam.b * m_Beam.h * (1 - 2 * m_Fiber.e_tot * eta / m_Beam.h);

            Rfb = R_fb();

            N_ult = fi * Rfb * A;

            double flex = LngthCalc0 / h;

            if (m_Fiber.e_tot <= h / 30d && flex <= 20)
            {
                N_ult = fi * Rfb * A;
            }
            else
            {
                N_ult = Rfb * Ab;
            }

            //Коэффициент использования
            UtilRate_N = (N_ult != 0) ? m_Efforts["N"] / N_ult : 0;
            
            InfoCheckN(N_ult);            
        }

        /// <summary>
        ///  Расчет внецентренно сжатых сталефибробетонных элементов без рабочей арматуры при
        /// расположении продольной сжимающей силы за пределами поперечного сечения элемента и внецентренно сжатых сталефибробетонных элементов без рабочей арматуры при расположении продольной
        /// сжимающей силы в пределах поперечного сечения элемента, в которых по условиям эксплуатации не
        /// допускается образование трещин
        /// </summary>
        public void Calculate_N_Out()
        {
            //Коэффициент, учитывающий влияние длительности действия нагрузки (6.27)
            fi1 = (Ml1toM1 <=1) ? 1 + Ml1toM1 : 2.0;

            //относительное значение эксцентриситета продольной силы
            delta_e = Delta_e(m_Fiber.e_tot / m_Beam.h);

            // Коэфициент ф.(6.26)
            k_b = K_b(fi1, delta_e);

            // Модуль упругости сталефибробетона п.п. (5.2.7)
            Efb = MatFiber.Efb;   // m_Fiber.Eb * (1 - m_Fiber.mu_fv) + m_Fiber.Ef * m_Fiber.mu_fv;

            //Модуль упругости арматуры
            double? Es = Rebar?.Es;
            // Момент инерции продольной арматуры соответственно относительно оси, проходящей через центр тяжести поперечного сечения элемента
            double? ls = Rebar?.ls;
            //жесткость элемента в предельной по прочности стадии, определяемая по формуле (6.31)
            D = k_b * Efb * I + 0.7 * (Es??0) * (ls??0);

            // условная критическая сила, определяемая по формуле (6.24)
            Ncr = Math.PI * Math.PI * D / Math.Pow(m_Beam.Length, 2);

            //коэффициент, учитывающий влияние продольного изгиба элемента на его несущую способность (6.23) 6.1.13
            eta = 1 / (1 - N / Ncr);

            Ab = m_Beam.b * m_Beam.h * (1 - 2 * m_Fiber.e_tot * eta / m_Beam.h);

            //Расчетные значения сопротивления осевому растяжению
            double Rfbt = R_fbt();
            
            double denom = A / I * m_Fiber.e_tot * eta * m_Beam.y_t - 1; 

            // Предельная сила сечения
            N_ult = (denom != 0) ? 1/denom * Rfbt * A : 0;

            //Коэффициент использования
            UtilRate_N = (N_ult != 0) ? m_Efforts["N"] / N_ult : 0;

            string info;

            if (N <= N_ult)
                info = "Прочность на действие продольной силы обеспечена";
            else
                info = "Прочность не обеспечена. Продольная сила превышает допустимое значение.";

            Msg.Add(info);
            
        }

        // жесткость элемента в предельной по прочности стадии, определяемая по формуле (6.31)
        protected double D_stiff(double _Is) => k_b* Efb * I + Rebar.k_s * Rebar.Es * _Is;

        protected double DStiff(double _I, double _Is) => k_b * Efb * _I + Rebar.k_s * Rebar.Es * _Is;

        // условная критическая сила, определяемая по формуле (6.24)
        protected  double N_cr(double _D) => (Math.PI* Math.PI) * _D / Math.Pow(LngthCalc0, 2);

        // коэффициент, учитывающий влияние продольного изгиба (прогиба) элемента
        // на его несущую способность и определяемый по формуле(6.23)6.1.13
        protected double Eta(double _N, double _Ncr) => 1 / (1 - _N / _Ncr);


        /// <summary>
        /// Расчет внецентренно сжатых сталефибробетонных
        /// элементов прямоугольного сечения с рабочей арматурой
        /// </summary>
        public void Calculate_N_Rods()
        {
            if (Rebar == null) return;

            string info;

            // Расчетное остаточное остаточного сопротивления осевому растяжению
            Rfbt3 = R_fbt3();

            // Расчетные значения сопротивления  на сжатиие по B30 СП63
            Rfb = R_fb();

            // Расчетная высота сечения см
            double h0 = h - Rebar.a;

            // Высота сжатой зоны
            double x = (N + Rebar.Rs * Rebar.As - Rebar.Rsc * Rebar.As1 + Rfbt3 * b * h) / ((Rfb + Rfbt3) * b);

            // относительной высоты сжатой зоны сталефибробетона
            double dzeta = x / h0;

            // характеристика сжатой зоны сталефибробетона, принимаемая для
            // сталефибробетона из тяжелого бетона классов до В60 включительно равной 0,8

            //Значения относительных деформаций арматуры для арматуры с физическим пределом текучести СП 63 п.п. 6.2.11
            double eps = Rebar.Epsilon_s;

            double dz_R = Rebar.Dzeta_R(BetonType?.Omega??0, BetonType?.Eps_fb2??0);

            double x_denom = (Rfb + Rfbt3) * b + 2 * Rebar.Rs * Rebar.As / (h0 * (1 - dz_R));

            delta_e = Delta_e(m_Fiber.e_tot / m_Beam.h);

            fi1 = Fi1();

            k_b = K_b(fi1, delta_e);

            if (dzeta > dz_R)
            {
                x = (x_denom > 0) ? (N + Rebar.Rs * Rebar.As * ((1 + dz_R) / (1 - dz_R)) - Rebar.Rsc * Rebar.As1 + Rfbt3 * b * h) / x_denom : 0;
            }

            double alfa = Rebar.Es / Efb;

            double A_red = A + alfa * Rebar.As + alfa * Rebar.As1;

            // Статический момент сечения фибробетона относительно растянутой грани
            double S = A * h / 2;
            // расстояние от центра тяжести приведенного сечения до растянутой в стадии эксплуатации грани Пособие к СП 52-102-2004 ф.2.12 (см)
            double y = (A_red > 0) ? (S + alfa * Rebar.As * Rebar.a + alfa * Rebar.As1 * (h - Rebar.a1)) / A_red : 0;
            // расстояние от центра тяжести приведенного сечения до сжатой
            double ys = y - Rebar.a;
            // расстояние от центра тяжести приведенного сечения до растянутой арматуры
            double y1s = h - Rebar.a1 - y;
            // момент инерции
            double Is = Rebar.As * ys * ys + Rebar.As1 * y1s * y1s;
            // жесткость элемента в предельной по прочности стадии, определяемая по формуле (6.31)
            D = DStiff(I, Is);
            // условная критическая сила, определяемая по формуле (6.24)
            Ncr = N_cr(D);
            // коэффициент, учитывающий влияние продольного изгиба (прогиба) элемента
            // на его несущую способность и определяемый по формуле(6.23)6.1.13
            eta = Eta(N, Ncr);
            // расстояние отточки приложения продольной силы N до центра тяжести сечения растянутой арматуры ф.6.33 см
            double e = e0 * eta + (h0 - Rebar.a) / 2 + e_N;

            M_ult = Rfb * b * x * (h0 - 0.5 * x) - Rfbt3 * b * (h - x) * ((h - x) / 2 - Rebar.a) + Rebar.Rsc * Rebar.As1 * (h0 - Rebar.a1);

            N_ult = M_ult / e;

            //Коэффициент использования
            UtilRate_N = (N_ult != 0) ? m_Efforts["N"] / N_ult : 0;

            if (N * e <= M_ult)
                info = "Прочность обеспечена";
            else
                info = "Прочность не обеспечена";

            Msg.Add(info);            
        }

        protected void InitC(ref List<double> _lst, double _from, double _to, double _dx)
        {
            double val = _from;
            while (val <= _to)
            {
                _lst.Add(val);
                val += _dx;
            }
        }

        /// <summary>
        /// Расчет элементов по полосе между наклонными сечениями
        /// </summary>
        protected virtual (double, double) Calculate_Qy(double _b, double _h)
        {
            return (0, 0);
        }

        /// <summary>
        /// Расчет элементов по полосе между наклонными сечениями
        /// </summary>
        public virtual (double, double) Calculate_Qcx(double _b, double _h)
        {            
            // Растояние до цента тяжести арматуры растянутой арматуры, см
            double a = Rebar?.a ?? 0; 

            // рабочая высота сечения по растянутой арматуре
            double h0 = _h - a;

            // Расчетные значения сопротивления  на сжатиие по B30 СП63
            Rfb = R_fb();

            // Предельная перерезывающая сила по полосе между наклонными сечениями
            double c_Q_ult = 0.3 * Rfb * _b * h0; // (6.74)
            
            // Расчет элементов по наклонным сечениям на действие поперечных сил
            // Минимальная длина проекции(см)
            double c_min = h0;
            // Максимальная длина проекции(см)
            double c_max = 4 * h0;
            double dC = 1;
            // ?? Минимальная длина проекции для формулы
            double c0_max = 2 * h0;

            List<double> lst_C = new List<double>();
            InitC(ref lst_C, c_min, c_max, dC);

            // Расчетное сопротивление сталефибробетона осевому растяжению
            double Rfbt = Rfbtn / Yft * Yb1 * Yb5;

            // поперечная сила, воспр сталефибробетоном
            double Qfb_i;

            List<double> lstQ_fb = new List<double>();

            foreach (double _c in lst_C)
            {
                if (_c == 0) continue;

                Qfb_i = 1.5d * Rfbt * _b * h0 * h0 / _c; // 6.76

                // условие на 0.5..2.5
                if (Qfb_i >= 2.5 * Rfbt * _b * h0)
                    Qfb_i = 2.5 * Rfbt * _b * h0;
                else if (Qfb_i <= 0.5 * Rfbt * _b * h0)
                    Qfb_i = 0.5 * Rfbt * _b * h0;

                lstQ_fb.Add(Qfb_i);
            }

            // Qfb - максимальная поперечная сила, воспринимаемая сталефибробетоном в наклонном сечении
            double Qfb = (lstQ_fb.Count > 0) ? lstQ_fb.Max() : 0;

            // Максимальный шаг поперечной арматуры см
            double s_w_max = (Qx > 0) ? Rfbt * _b * h0 * h0 / Qx : 0;

            string res;
            // поперечная сила, воспринимаемая поперечной арматурой в наклонном сечении
            double Qsw = 0;
            List<double> lst_Qsw = new List<double>();

            if (Rebar != null)
            {
                if (Rebar.Sw_X <= s_w_max)
                {
                    res = "Условие выполнено, шаг удовлетворяет требованию 6.1.28";
                    Msg.Add(res);
                }
                else
                {
                    res = "Условие не выполнено, требуется уменьшить шаг поперечной арматуры";
                    Msg.Add(res);
                }

                // усилие в поперечной арматуре на единицу длины элемента
                double q_sw = (Rebar.Sw_X != 0) ? Rebar.Rsw_X * Rebar.Asw_X / Rebar.Sw_X : 0; // 6.78 

                // условие учета поперечной арматуры
                if (q_sw < 0.25 * Rfbt * _b)
                    q_sw = 0;
                
                foreach (double _c in lst_C)
                {
                    if (_c > c0_max)
                        Qsw = 0.75 * q_sw * c0_max;
                    else
                        Qsw = 0.75 * q_sw * _c;  // 6.77

                    lst_Qsw.Add(Qsw);
                }

            }
                                    
            List<double> lst_Q_ult = new List<double>();
            for (int i = 0; i < lst_Qsw.Count; i++)
            {
                lst_Q_ult.Add(lstQ_fb[i] + lst_Qsw[i]);
            }

            Qx_ult = Qfb + Qsw; // 6.75

            //Коэффициент использования
            UtilRate_Qx = (Qx_ult != 0) ? m_Efforts["Qx"] / Qx_ult : 0;

            UtilRate_Qс = (c_Q_ult != 0) ? Qx_ult / c_Q_ult : 0;

            if (c_Q_ult <= Qx_ult)
            {
                res = "Перерезываюзщая сила превышает предельно допустимую в данном сечении";
                Msg.Add(res);
            }
            else
            {
                res = "Проверка по наклонному сечению на действие поперечной силы Qx пройдена";
                Msg.Add(res);
            }

            return (s_w_max, Qx_ult);
        }


        /// <summary>
        /// проверка  на действие сжимающей силы
        /// </summary>
        /// <returns></returns>
        public (double, double) Calculate_Nz()
        {
            if (N_Out)
            {
                Calculate_N_Out();
            }
            else
            {
                if (UseRebar)
                    Calculate_N_Rods();
                else
                    Calculate_N();
            }
            
            return (N_ult, UtilRate_N);
        }


        /// <summary>
        /// проверка по полосе накл сечения на Q
        /// </summary>
        /// <returns></returns>
        public (double, double) Calculate_Qcx()
        {
            return Calculate_Qcx(b, h);
        }

        /// <summary>
        /// проверка по полосе накл сечения на M
        /// </summary>
        /// <returns></returns>
        public (double, double) Calculate_Mc()
        {
            return Calculate_Mc(b, h);
        }

        /// <summary>
        ///  6.1.30 Расчет элементов по наклонным сечениями на действие моментов My
        /// </summary>
        public virtual (double, double) Calculate_Mc(double _b, double _h)
        {
            // Растояние до цента тяжести арматуры растянутой арматуры, см
            double a = Rebar?.a ?? 0;
            // рабочая высота сечения по растянутой арматуре
            double h0 = _h - a;
            // Нормативное остаточное сопротивления осевому растяжению кг/см2
            double _Rfbt3 = R_fbt3();
            // Площадь растянутой арматуры см2
            double As = Rebar?.As ?? 0;
            // Расчетное сопротивление поперечной арматуры  
            double Rsw = Rebar?.Rsw_X ?? 0;
            // Площадь арматуры
            double Asw = Rebar?.Asw_X ?? 0;
            // шаг попреречной арматуры
            double sw = Rebar?.Sw_X ?? 0;
            // усилие в поперечной арматуре на единицу длины элемента
            double q_sw = (sw !=0) ? Rsw * Asw / sw : 0;
            // условие учета поперечной арматуры
            if (q_sw < 0.25 * _Rfbt3 * b)
                q_sw = 0;

            double c_min = h0;
            double c_max = 4 * h0;
            double c0_max = 2 * h0;
            List<double> С_x = new List<double>();
            InitC(ref С_x, c_min, c_max, 1);
            double Q_sw,
                   M_sw; // момент, воспр поперечной арматурой
            double Q_fbt3 = (c_min!=0) ? 1.5d * _Rfbt3 * _b * h0 * h0 / c_min : 0;
            // усилие в продольной растянутой арматуре
            double N_s = Rebar?.Rs??0 * Rebar?.As??0;
            // плечо внутренней пары сил
            double z_S = 0.9 * h0;
            // момент, воспринимаемый продольной арматурой, пересекающей наклонное сечение, относительно противоположного конца наклонного сечения
            double Ms = N_s * z_S; // 6.80
            //  Усилие в поперечной арматуре:
            List<double> lst_Q_sw = new List<double>();
            // момент, воспринимаемый поперечной арматурой, пересекающей наклонное сечение, относительно противоположного конца наклонного сечения
            List<double> lst_M_sw = new List<double>();
            List<double> lst_Q_fbt3 = new List<double>();
            List<double> lst_M_fbt = new List<double>();
            List<double> lst_M_ult = new List<double>();

            foreach (double ci in С_x)
            {
                if (ci > c0_max)
                {
                    Q_sw = q_sw * c0_max;
                    M_sw = 0.5 * Q_sw * c0_max;
                }
                else
                {
                    Q_sw = q_sw * ci; // усилие в поперечной арматуре
                    M_sw = 0.5 * Q_sw * ci; // (6.81)
                }

                lst_Q_sw.Add(Q_sw);
                lst_M_sw.Add(M_sw);

                Q_fbt3 = (ci != 0) ? 1.5d * _Rfbt3 * _b * h0 * h0 / ci : 0;

                if (Q_fbt3 >= 2.5d * _Rfbt3 * _b * h0)
                {
                    Q_fbt3 = 2.5d * _Rfbt3 * _b * h0;
                }
                else if (Q_fbt3 <= 0.5d * _Rfbt3 * _b * h0)
                {
                    Q_fbt3 = 0.5d * _Rfbt3 * _b * h0;
                }

                double M_fbt = 0.5 * Q_fbt3 * ci;

                lst_Q_fbt3.Add(Q_fbt3);

                lst_M_fbt.Add(M_fbt);

                // расчет по наклонным сечениям; условие : M <= M_ult (Ms - продольная, Msw - поперечная, М_fbt - фибробетон)
                M_ult = Ms + M_sw + M_fbt; // 6.79

                lst_M_ult.Add(M_ult);
            }

            M_ult = (lst_M_ult.Count > 0) ? lst_M_ult.Min() : 0;

            //Коэффициент использования
            UtilRate_My = (M_ult != 0) ? m_Efforts["My"] / M_ult : 0;

            return (M_ult, UtilRate_My);           
        }

        /// <summary>
        ///  6.1.30 Расчет элементов по наклонным сечениями на действие моментов Mx
        /// </summary>
        protected virtual (double, double) Calculate_Mx(double _b, double _h)
        {
            return (0, 0);
        }

        public virtual bool Calculate() 
        {
            return true;
        }

        public Dictionary<string, double> GeomParams()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Коэффициенты надежности
        /// </summary>
        /// <param name="_t">..в виде массива</param>
        public virtual void SetParams(double[] _t)
        {
            // TODO Refactoring
            (Yft, Yb, Yb1, Yb2, Yb3, Yb5) = (_t[0], _t[1], _t[2], _t[3], _t[4], _t[5]);            
        }

        public virtual void SetSize(double[] _t) {}


        public double Get_e_tot => m_Fiber?.e_tot ?? 0;

        public LameUnitConverter UnitConverter { get; internal set; }
        public Dictionary<string, double> CalcResults2Group { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_efforts"></param>
        /// <returns>полный эксцентриситет </returns>
        public void SetEfforts(Dictionary<string, double> _efforts)
        {            
            m_Efforts = new Dictionary<string, double>(_efforts);

            //Момент от действия полной нагрузки            
            Mx = m_Efforts.ContainsKey("Mx")? m_Efforts["Mx"] : 0;
            My = m_Efforts.ContainsKey("My")? m_Efforts["My"] : 0;
            //Продольное усилие кг
            N = m_Efforts.ContainsKey("N") ? m_Efforts["N"] : 0;
            // Поперечная сила
            Qx = m_Efforts.ContainsKey("Qx")? m_Efforts["Qx"] :0;
            Qy = m_Efforts.ContainsKey("Qy")? m_Efforts["Qy"] :0 ;
            //Момент от действия постянных и длительных нагрузок нагрузок
            Ml1toM1 = m_Efforts.ContainsKey("Ml") ? m_Efforts["Ml"]: 0;
            // случайный эксцентриситет
            e0  = m_Efforts.ContainsKey("e0")? m_Efforts["e0"]: 0;
            // Эксцентриситет приложения силы N
            e_N = m_Efforts.ContainsKey("eN") ? m_Efforts["eN"]: 0;           
            
            // эксцентриситет от момента
            double e_MN = (N != 0) ? My / N : 0;
            e_N += e_MN;
            
            // полный эксцентриситет приложения силы
            if (m_Fiber != null)
                m_Fiber.e_tot = e0 + e_N + e_MN ;
        }

        public virtual Dictionary<string, double> Results()
        {
            Dictionary<string, double> dictRes = new Dictionary<string, double>()
            {
                { DN(typeof(BSFiberCalc_MNQ), "M_ult"), M_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_My"), UtilRate_My },

                { DN(typeof(BSFiberCalc_MNQ), "N_ult"), N_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_N"), UtilRate_N },

                { DN(typeof(BSFiberCalc_MNQ), "Qx_ult"), Qx_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_Qx"), UtilRate_Qx },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_Qс"), UtilRate_Qс },
            };

            return dictRes;
        }

        public Dictionary<string, double> Results_Mc()
        {
            Dictionary<string, double> dictRes = new Dictionary<string, double>()
            {
                { DN(typeof(BSFiberCalc_MNQ), "M_ult"), M_ult },
                { DN(typeof(BSFiberCalc_MNQ), "UtilRate_My"), UtilRate_My },                
            };

            return dictRes;
        }


        public virtual BeamSection BeamSectionType()
        {
            return BeamSection.Any;
        }

        /// <summary>
        /// сила вне сечения
        /// </summary>
        public void SetN_Out()
        {
            N_Out = h / 2.0 < Get_e_tot; 
        }
    }
}
