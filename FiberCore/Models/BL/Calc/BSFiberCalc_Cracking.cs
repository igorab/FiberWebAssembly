using BSFiberCore.Models.BL;
using BSFiberCore.Models.BL.Beam;
using BSFiberCore.Models.BL.Mat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace BSFiberCore.Models.BL.Calc
{
    /// <summary>
    /// Класс для расчета сталефибробетоннных элементов по образованию трещин
    /// </summary>
    public class BSFiberCalc_Cracking : ICalc
    {
        public List<string> Msg { get; private set; }
        private BSBeam m_Beam { get; set; }
        private BSMatFiber m_Fiber;
        private BSMatRod m_Rod;

        public BeamSection typeOfBeamSection;

        // балка
        public BSBeam Beam
        {
            get { return m_Beam; }
            set { m_Beam = value; }
        }
        // свойства бетона
        public BSMatFiber MatFiber { get { return m_Fiber; } set { m_Fiber = value; } }
        // свойства арматуры
        public BSMatRod MatRebar { get { return m_Rod; } set { m_Rod = value; } }
        
        public Dictionary<string, double> Efforts;

        private double I_red;

        // заданные нагрузки      
        public double Mx;
        public double My;
        // продольная сила от внешней нагрузки
        public double N;

        /// случайный эксцентриситет
        double e0;
        /// эксцентриситет от продольной силы
        double eN;

        private double _M_crc;
        private double _a_crc;

        private double UtilRate_M_crc;
        // предельно-допустимая ширина раскрытия трещин
        // Принимается в зависимости класса арматуры
        private double a_crc_ult = 0.03;

        private double k_x; // кривизна сечения в плосткости xOz

        // Результаты расчета
        public DataTable resultTable;
        public Dictionary<string, double> resultDictionary;

        public string DN(Type _T, string _property) => _T.GetProperty(_property).GetCustomAttribute<DisplayNameAttribute>().DisplayName;
       
        public BSFiberCalc_Cracking(Dictionary<string, double> MNQ)
        {
            this.Efforts = MNQ;

            MNQ.TryGetValue("Mx",out Mx);
            MNQ.TryGetValue("My", out My);
            MNQ.TryGetValue("N", out N);
            MNQ.TryGetValue("e0", out e0);
            MNQ.TryGetValue("eN", out eN);

            m_Fiber = new BSMatFiber();
            m_Rod = new BSMatRod();

            Msg = new List<string>();
            resultDictionary = new Dictionary<string, double>();
            
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
                { DN(typeof(BSFiberCalculation), "Rfbt3n"), MatFiber.Rfbt3n },
                { DN(typeof(BSFiberCalculation), "B"), MatFiber.B },
                { DN(typeof(BSFiberCalculation), "Rfbn"), MatFiber.Rfbn }
            };
            return phys;
        }

        # region наследие интерфейса IBSFiberCalculation
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

        /// <summary>
        /// Расчет на трещиннообразование с учетом параметров полученными по нелинейной деформационной модели
        /// </summary>
        /// <returns></returns>
        public bool CalculateNDN()
        {
            Calculate_M_crc();

            CalculateWidthCrack();

            CalculateCurvature();

            return true;
        }

        /// <summary>
        /// Расчет кривизны сечения
        /// </summary>
        private void CalculateCurvature()
        {
            double Efb1 = 0.85 * MatFiber.Efb; // 6.133

            double D = I_red * Efb1;

            k_x = (D!=0) ? My / D : 0;
        }

        public virtual bool Calculate()
        {
            if (MatRebar.Reinforcement == false)
            {                
                Msg.Add($"Расчет по предельным состояниям второй группы выполняется только для армированных элементов. \n" +
                    $" Для получения результатов необходимо указать флажок 'Армирование'."); 
                return false;
            }

            if (MatRebar.As <= 0 && MatRebar.As1 <= 0)
            {
                Msg.Add("Для получения результатов по предельным состояниям второй группы необходимо задать площадь арматуры.");
                return false;
            }

            CalculateUltM();

            if (typeOfBeamSection != BeamSection.Rect)
            {
                Msg.Add("Расчет ширины раскрытия трещины выполняется только для прямоугольного сечения");
                return false;
            }

            CalculateWidthCrack();

            return true;
        }

        public virtual Dictionary<string, double> Results()
        {
            return resultDictionary;
        }
        # endregion 

        /// <summary>
        ///  Результаты в расчете по НДМ
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, double> ResGr2()
        {
            return new Dictionary<string, double> {
                ["Ky"] = double.NaN,
                ["Kx"] = k_x,
                ["My_crc"] = _M_crc,
                ["UtilRate_M_crc"] = UtilRate_M_crc,
                ["sig_s_crc"] = double.NaN,
                ["a_crc"] = _a_crc ,
                ["a_crc_ult"] = a_crc_ult,                
            };
        }

        /// <summary>
        ///  расчет момента трещинообразования по данным полученным через НДМ
        /// </summary>
        /// <returns></returns>
        public bool Calculate_M_crc()
        {            
            double B = m_Fiber.B;            
            // Площадь растянутой арматуры
            double A_s = MatRebar.As;
            // Площадь сжатой арматуры
            double A_1s = MatRebar.As1;
            // Расстояние до центра тяжести растянутой арматуры
            double h0_t = MatRebar.h0_t;
            // Расстояние до центра тяжести сжатой арматуры
            double h0_p = MatRebar.h0_p;
            // модуль упрогости арматуры
            double Es = MatRebar.Es; 
            // нормативное остаточное сопротивление осевому растяжению для bft3i
            double R_fbt_ser;
            // модуль упругости фибробетона
            double Efb = MatFiber.Efb;
            // Класс бетона            
            double Y = 1.73 - 0.005 * (B - 15);            
            // Площадь сечения
            double A = Beam.Area();
            // момент инерции сечения 
            double I = Beam.Jy();
            // статический момент сечения фибробетона
            double S = Beam.Sy();
            ///            
            R_fbt_ser = MatFiber.Rfbt_ser;
            ///            
            
            // Коэф Привендения арматуры к стальфибробетону
            double alpha = Es / Efb;                                                                                //  (6.113)
            // Площадь приведенного поперечного сечения элемента 
            double A_red = A + A_s * alpha + A_1s * alpha;                                                          //  (6.112)

            double Y_t;

            if (typeOfBeamSection == BeamSection.Ring)
            {
                BSBeam_Ring tmpBeam = (BSBeam_Ring)Beam;
                Y_t = tmpBeam.r2;
                double r_m = tmpBeam.r_m;

                double SS = (A_s + A_1s) / (Math.PI * 2 * r_m);
                double Is = Math.PI / 64 * (Math.Pow(2 * (r_m + SS / 2), 4) - Math.Pow(2 * (r_m - SS / 2), 4));
                I_red = I + alpha * Is;
            }
            else if (typeOfBeamSection == BeamSection.Any)
            {
                // Статический момент площади приведенного поперечного сечения элемента
                // относительно наиболее растянутого волокна сталефибробетона
                double S_t_red = S + alpha * A_s * h0_t + alpha * A_1s * h0_p;
                //TODO Получить расчетом по ндм
                // Расстояние от центра тяжести приведенного сечения до расстянутой в стадии эксплуатации грани
                Y_t = S_t_red / A_red;                                                                           //  (6.114)

                // расстояние от центра тяжести приведенного сечения до расстянутой арматуры
                double Y_s = h0_t;

                // расстояние от центра тяжести приведенного сечения до сжатой арматуры
                double Y_1s = h0_p;

                // Момент инерции растянутой арматуры
                double I_s = A_s * Y_s * Y_s;

                // Момент инерции сжатой арматуры
                double I_1s = A_1s * Y_1s * Y_1s;

                // в сп формула без упоминания alpha
                I_red = I + alpha * I_s + alpha * I_1s;                                                          // (6.131) не (6.111)
            }
            else
            {                
                // Статический момент площади приведенного поперечного сечения элемента
                // относительно наиболее растянутого волокна сталефибробетона
                double S_t_red = S + alpha * A_s * h0_t + alpha * A_1s * h0_p;

                // Расстояние от центра тяжести приведенного сечения до расстянутой в стадии эксплуатации грани
                Y_t = S_t_red / A_red;                                                                           //  (6.114)

                // расстояние от центра тяжести приведенного сечения до расстянутой арматуры
                double Y_s  = h0_t;

                // расстояние от центра тяжести приведенного сечения до сжатой арматуры
                double Y_1s = h0_p;

                // Момент инерции растянутой арматуры
                double I_s = A_s * Y_s * Y_s;

                // Момент инерции сжатой арматуры
                double I_1s = A_1s * Y_1s * Y_1s;

                // в сп формула без упоминания alpha
                I_red = I + alpha * I_s + alpha * I_1s;                                                          // (6.131) не (6.111)
            }

            double W_red = I_red / Y_t;                                                                               // (6.109)

            double e_x = W_red / A_red;                                                                              // (6.110)

            double e_x_sum = e_x + e0 + eN;

            double W_pl = Y * W_red;                                                                                // (6.108)

            double M_crc = R_fbt_ser * W_pl + N * e_x;                                                              // (6.107)
            
            _M_crc         = M_crc;
            UtilRate_M_crc = (M_crc != 0) ? My / M_crc : 0;

            resultDictionary.Add("Момент образования трещин с учетом неупругих деформаций растянутого сталефибробетона, Mcrc [кг*см2] [П6.2.8]", M_crc);
            resultDictionary.Add("Коэффициент использования по второй группе предельных состояний [П6.2.4]", UtilRate_M_crc);            

            return true;
        }



        /// <summary>
        /// Определение момента образования трещин
        /// Продольная сила расположенная в центре тяжести приведенного элемента
        /// Сжатие       - "+"
        /// Растяжение   - "-"
        /// N
        /// </summary>
        /// <returns></returns>
        public bool CalculateUltM()
        {
            if (!Validate())
                return false;
            
            double B = m_Fiber.B;

            #region Характеристики материала
            // Площадь растянутой арматуры
            double A_s;
            // Площадь сжатой арматуры
            double A_1s;
            // Расстояние до центра тяжести растянутой арматуры
            double a;
            // Расстояние до центра тяжести сжатой арматуры
            double a_1;
            // модуль упрогости арматуры
            double Es;
            // нормативное остаточное сопротивление осевому растяжению для bft3i
            double R_fbt_ser;
            // модуль упругости фибробетона
            double Efb;
            // Класс бетона            
            double Y = 1.73 - 0.005 * (B - 15);            
            #endregion

            #region Геометрические характеристики сечения 
            // Площадь сечения
            double A;
            // момент инерции сечения 
            double I;
            // статический момент сечения фибробетона
            double S;
            #endregion

            ///
            A_s = MatRebar.As;
            A_1s = MatRebar.As1;
            a = MatRebar.a_s;
            a_1 = MatRebar.a_s1;
            Es = MatRebar.Es;
            ///
            Efb = MatFiber.Efb;
            R_fbt_ser = MatFiber.Rfbt_ser;

            ///
            A = Beam.Area();
            I = Beam.Jy();
            S = Beam.Sy();

            #region Расчет
            // Коэф Привендения арматуры к стальфибробетону
            double alpha = Es / Efb;                                                                                //  (6.113)
            // Площадь приведенного поперечного сечения элемента 
            double A_red = A + A_s * alpha + A_1s * alpha;                                                          //  (6.112)

            double Y_t;
            
            if (typeOfBeamSection == BeamSection.Ring)
            {
                BSBeam_Ring tmpBeam = (BSBeam_Ring)Beam;
                Y_t = tmpBeam.r2;
                double r_m = tmpBeam.r_m;

                double SS = (A_s + A_1s) / (Math.PI * 2 * r_m);
                double Is = Math.PI / 64 * (Math.Pow(2 * (r_m + SS / 2), 4) - Math.Pow(2 * (r_m - SS / 2), 4));
                I_red = I + alpha * Is;
            }
            else
            {
                double height = Beam.Height;

                // Статический момент площади приведенного поперечного сечения элемента
                // относительно наиболее растянутого волокна сталефибробетона
                double S_t_red = S + alpha * A_s * alpha + alpha * A_1s * (height - a_1);
                // Расстояние от центра тяжести приведенного сечения до расстянутой в стадии эксплуатации грани
                Y_t = S_t_red / A_red;                                                                           //  (6.114)

                // расстояние от центра тяжести приведенного сечения до расстянутой арматуры
                double Y_s = Y_t - a;
                // расстояние от центра тяжести приведенного сечения до сжатой арматуры
                double Y_1s = height - Y_t - a_1;

                // Момент инерции растянутой арматуры
                double I_s = A_s * Y_s * Y_s;
                // Момент инерции сжатой арматуры
                double I_1s = A_1s * Y_1s * Y_1s;

                // в сп формула без упоминания alpha
                I_red = I + alpha * I_s + alpha * I_1s;                                                          // (6.131) не (6.111)
            }

            double W_red = I_red / Y_t;                                                                               // (6.109)

            double e_x = W_red / A_red;                                                                              // (6.110)

            double e_x_sum = e_x + e0 + eN;

            double W_pl = Y * W_red;                                                                                // (6.108)

            double M_crc = R_fbt_ser * W_pl + N * e_x;                                                              // (6.107)
            #endregion

            _M_crc = M_crc;
            UtilRate_M_crc = (M_crc != 0)? My / M_crc : 0;

            resultDictionary.Add("Момент образования трещин с учетом неупругих деформаций растянутого сталефибробетона, Mcrc [кг*см2] [П6.2.8]", M_crc);
            resultDictionary.Add("Коэффициент использования по второй группе предельных состояний [П6.2.4]", UtilRate_M_crc);            

            return true;
        }


        /// <summary>
        /// Расчет ширины раскрытия трещин 
        /// Продольная сила расположенная в центре тяжести приведенного элемента
        /// Сжатие       - "+"
        /// Растяжение   - "-"
        /// N
        /// </summary>
        /// <returns></returns>
        public bool CalculateWidthCrack()
        {
            if (!Validate())
                return false;
            
            #region Характеристики материала
            // Площадь растянутой арматуры
            double A_s;
            // Площадь сжатой арматуры
            double A_1s;
            // Расстояние до центра тяжести растянутой арматуры
            double a;
            // Расстояние до центра тяжести сжатой арматуры
            double a_1;
            // модуль упрогости арматуры
            double Es;

            // нормативное остаточное сопротивление осевому растяжению для bft3i
            double R_fbt_ser;
            // модуль упругости фибробетона
            double Efb;
            
            #endregion

            #region Геометрические характеристики сечения 
            // Площадь сечения
            double A;                        
            #endregion
            
            A_s  = MatRebar.As;
            A_1s = MatRebar.As1;
            a    = MatRebar.a_s;
            a_1  = MatRebar.a_s1;
            Es   = MatRebar.Es;
            
             
            Efb       = MatFiber.Efb;
            R_fbt_ser = MatFiber.Rfb_ser;
           
            A = Beam.Area();

            double d_rebar = MatRebar.SelectedRebarDiameter;

            #region Расчет

            // Коэф Привендения арматуры к стальфибробетону
            double alpha = Es / Efb;                                                                                //  (6.113)
            // Площадь приведенного поперечного сечения элемента 
            double A_red = A + A_s * alpha + A_1s * alpha;                                                          //  (6.112)
            
            double I_red;
                      
            // Коэффициент, учитывающий продолжительность действия нагрузки
            double fi_1 = 1.4;
            // Коэф, учитывающий характер нагружения
            double fi_3 = 1;
            // Коэф, учитывающий неравномерное распределение относительных
            // деформаций растянутой арматуры между трещинами
            double psi_s = 1;

            // коэф, учитывающий профиль продольной раматуры, для гладкой арматуры: 
            double fi_2  = 0.8;
            double fi_13 = 0.8;
            
            double epsilon_fb1_red = 0.0015;
            double epslion_fbt2 = 0.004;

            // Диаметр арматуры, перевод из мм в cм
            double d_s = d_rebar / 10.0;
                        
            // длина фибры
            double l_f = 50;
            // диаметр фибры
            double d_f = 0.8;
            // коэф фибрового армирования по объему
            double Mu_fv = 0.0174;

            double R_fb_n = MatFiber.Rfbn;

            // Приведенный модуль деформации сжатого сталефибробетона, учитывающий неупругие деформации сжатого сталефибробетона
            double E_fb_red = R_fb_n / epsilon_fb1_red;
            //  Коэф. приведения арматуры
            double alpha_s1 = Es / E_fb_red;
            double alpha_s2 = alpha_s1;
            // Приведенный модуль деформации сжатого стальфибробетона, учитывающий неупругие деформации сжатого стальфибробетона
            double E_fbt_red = R_fbt_ser / epslion_fbt2;
            // Коэф. стальфибробетона растянутой зоны к стальфибробетону сжатой зоны
            double alpha_fbt = E_fbt_red / E_fb_red;

            double b;
            double h;
            double h_0;

            if (typeOfBeamSection == BeamSection.Rect)
            {
                BSBeam_Rect tmpBeam = (BSBeam_Rect)Beam;
                b = tmpBeam.b;
                h = tmpBeam.h;
                h_0 = h - a_1;
            }
            else if (typeOfBeamSection == BeamSection.Any)
            {                
                b = Beam.b;
                h = Beam.h;
                h_0 = h - a_1;
            }
            else
            {
                b = 0;
                h = 0;
                h_0 = 0;
            }
            double Mu_s = A_s / (b * h_0);
            double Mu_1s = A_1s / (b * h_0);


            // для каждого типа сечени своя формаула
            // Высота сжатой зоны
            double Xm = (h_0 / (1 - alpha_fbt)) * ((Math.Sqrt(Math.Pow(Mu_s * alpha_s2 + Mu_1s * alpha_s1 + alpha_fbt, 2) +
                (1 - alpha_fbt) * (2 * Mu_s * alpha_s2 + 2 * Mu_1s * alpha_s1 * (a_1 / h_0) + alpha_fbt))) -
                (Mu_s * alpha_s2 + Mu_1s * alpha_s1 + alpha_fbt));
            // формула 6.140

            double y_c = Xm;

            // момент инерции сжатой зоны
            double I_fb = b * Math.Pow(y_c, 3) / 12 + b * y_c * Math.Pow(h / 2 - y_c / 2, 2);
            // момент инерции растянутой зоны
            double I_fbt = b * Math.Pow(h - y_c, 3) / 12 + b * (h - y_c) * Math.Pow(h / 2 - (h - y_c) / 2, 2);

            double I_1s = A_1s * Math.Pow(y_c - a_1, 2);
            double I_s  = A_s * Math.Pow(h - y_c - a, 2);
            // момент инерции
            I_red = I_fb + I_fbt * alpha_fbt + I_s * alpha_s2 + I_1s * alpha_s1;

            // Напряжение в растянутой арматуре изгибаемых элементов
            double sigma_s = (My * (h_0 - y_c) / I_red + N/ A_red) * alpha_s1;

            double k_f;

            if (l_f / d_f < 50)
                k_f = 50;
            else if (l_f / d_f >= 50 || l_f / d_f <= 100)
                k_f = 50 * d_f / l_f;
            else
            {
                //(l_f / d_f > 100)
                k_f = 0.5;
            }

            // базовое расстояние между смежными нормальными трещинами
            double l_s = k_f * (50 + 0.5 * fi_2 * fi_13 * d_s / Mu_fv);

            // Ширина раскрытия трещин
            double a_crc = fi_1 * fi_3 * psi_s * sigma_s / Es * l_s;

            _a_crc = a_crc;
            #endregion
                       
            resultDictionary.Add("Ширина раскрытия трещин от действия внешней нагрузки, a_crc [см] [П6.2.14]", a_crc);
            resultDictionary.Add("Предельно допустимая ширина раскрытия трещин, acrc_ult [см] [П6.2.6]", a_crc_ult);
            
            return true;
        }
        
        /// <summary>
        /// Создается новая таблица параметра ResultTable
        /// </summary>
        protected void CreateResTable()
        {
            resultTable = new DataTable();
            resultTable.Columns.Add("Описание");
            resultTable.Columns.Add("Параметр");
            resultTable.Columns.Add("Значение");
            resultTable.Columns.Add("Ед. Измерения");
        }

        /// <summary>
        /// Добавление строки в таблицу ResultTable
        /// </summary>
        /// <param name="description"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="units"></param>
        protected void AddRowInResTable(string description, string name, double value, string units)
        {
            double valueRound =Math.Round(value,3);
            DataRow row = resultTable.NewRow();
            row["Описание"] = description;
            row["Параметр"] = name;
            row["Значение"] = valueRound.ToString();
            row["Ед. Измерения"] = units;
            resultTable.Rows.Add(row);
        }

    }    
}
