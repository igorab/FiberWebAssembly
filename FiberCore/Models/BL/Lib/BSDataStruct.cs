using BSFiberCore.Models.BL.Beam;

namespace BSFiberCore.Models.BL.Lib
{
    /// <summary>
    ///  Конфигурация
    /// </summary>
    public struct ProgConfig
    {
        /// <summary>
        /// Конфигурация соответствует базе данных
        /// </summary>
        public string ConfigId { get; set; }
        /// <summary>
        /// Описание
        /// </summary>
        public string  Name { get; set; } 
        public  System.Drawing.Color BackColor { get; set; }
        /// <summary>
        /// Нормативный документ
        /// </summary>
        public string   NormDoc { get; set; }
    }

    /// <summary>
    /// доступ к таблице Params
    /// </summary>
    public class FormParams
    {
        public int ID { get; set; }
        public double Length { get; set; }
        public double LengthCoef { get; set; }
        public string BetonType { get; set; }
        public string Fib_i { get; set; }
        public string Bft3n { get; set; }
        public string Bfn { get; set; }
        public string Bftn { get; set; }
        public string Eb { get; set; }
        public string Efbt { get; set; }
        public string Rs { get; set; }
        public string Rsw { get; set; }
        public double Area_s { get; set; }
        public double Area1_s { get; set; }
        public double a_s { get; set; }
        public double a1_s { get; set; }
    }

    /// <summary>
    /// Параметры расчета ширины раскрытия трещины
    /// </summary>
    public class NdmCrc
    {
        public int Id { get; set; }
        public double fi1 { get; set; }
        public double fi2 { get; set; }
        public double fi3 { get; set; }
        public double mu_fv { get; set; }
        public double psi_s { get; set; }
        public double kf { get;  set; }

        // СП 63 6.2.16
        public void InitFi2(string _RebarType)
        {
            if (_RebarType == "A240")            
                fi2 = 0.8;
            else
                fi2 = 0.5;
        }

        // СП 63 6.2.12
        public void InitFi3(double _N)
        {
            if (_N < 0)
                fi3 = 1.0; // для растянутых элементов
            else
                fi3 = 0.5;
        }
    }


    /// <summary>
    /// Параметры расчета по НДМ
    /// </summary>
    public class NDMSetup
    {
        public int Id { get; set; }
        public int Iters { get; set; }
        public int N { get; set; }
        public int M { get; set; }
        public double MinAngle { get; set; }
        public double MaxArea { get; set; }
        public int BetonTypeId { get; set; }
        public bool UseRebar { get; set; }
        public string RebarType { get; set; }
        public int FractureStressType { get; set; }
    }


    /// <summary>
    /// Единицы измерения
    /// </summary>
    public class Units
    {
        public static string R { get; set; }
        public static string E { get; set; }
        public static string L { get; set; }
        public static string D { get; set; }
        public static string A { get; set; }

    }

    /// <summary>
    ///  Бетон - матрица
    /// Данные из таблицы Beton
    /// 6.7 , 6.8 СП63.13330.2018
    /// </summary>
    public class Beton
    {
        public  int Id { get; set; }
        /// <summary>
        /// Класс бетона 
        /// </summary>
        public  string BT { get; set; }              
        /// <summary>
        /// Нормативное сопротивление сжатию
        /// </summary>
        public  double Rbn { get; set; }
        /// <summary>
        /// Расчетное сопротивление сжатию
        /// </summary>                
        public double Rb { get; set; }

        /// <summary>
        /// Растяжение осевое расчетное Rbt
        /// </summary>
        public double Rbt { get; set; }
        /// <summary>
        /// Растяжение осевое нормативное (Rbtn; Rbt,ser)
        /// </summary>
        public double Rbtn { get; set; }

        /// <summary>
        /// Модуль упругости 
        /// </summary>
        public  double Eb { get; set; }
        /// <summary>
        /// Номер в классе бетона (используется в расчетах) 
        /// </summary>
        public double B { get; set; }
        public double BetonType { get; set; }
    }


    /// <summary>
    /// Данные из таблицы FiberFbt - класс фибробетона по прочности на растяжение
    /// </summary>
    public class FiberBft
    {  
        // Класс
        public string ID { get; set; }        
        // Расчетное
        public double Rfbt { get; set; }
        // Нормативное
        public double Rfbtn { get; set; }
    }

    /// <summary>
    /// Данные из таблицы BetonType
    /// </summary>
    public class BetonType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Omega { get; set; }        
        public double Eps_fb2 { get; set; }
    }

    /// <summary>
    /// Фибробетон, параметры
    /// </summary>
    public class FiberBeton : ICloneable
    {
        //TODO refactoring полный эксцентриситет приложения силы
        public double e_tot { get; set; }
        /// <summary>
        /// начальный модуль упругости стальной фибры
        /// </summary>
        public double Ef { get; set; }
        /// <summary>
        /// начальный модуль упругости бетона-матрицы
        /// </summary>
        public double Eb { get; set; }
        public double mu_fv { get; set; }
        public double omega { get; set; }

        /// <summary>
        /// Модуль упругости фибробетона расчитанный
        /// </summary>
        public double Efb => Eb * (1 - mu_fv) + Ef * mu_fv;

        /// <summary>
        /// Модуль упругости фибробетона на растяжение - c формы
        /// </summary>
        public double E_fbt { get; set; }

        public FiberBeton() { }

        public FiberBeton(FiberBeton _fiber)
        {
            this.e_tot = _fiber.e_tot;
            this.Ef    = _fiber.Ef;
            this.Eb    = _fiber.Eb;
            this.mu_fv = _fiber.mu_fv;
            this.omega = _fiber.omega;
            this.E_fbt  = _fiber.E_fbt;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    /// <summary>
    /// Арматура, параметры
    /// </summary>
    public class Rebar : ICloneable
    {
        public string ID { get; set; }

        //Нормативное сопротивление арматуры растяжению
        public double Rsn { get; set; }
        //Расчетное сопротивление арматуры растяжению
        public double Rs { get; set; }

        //Расчетное сопротивление арматуры сжатию
        public double Rsc { get; set; }
        //Значения модуля упругости арматуры
        public double Es { get; set; }
       
        // Площадь растянутой арматуры
        public double As { get; set; }
        // Площадь сжатой арматуры
        public double As1 { get; set; }

        // Поперечная арматура:
        // Вдоль оси X:
        //Значения модуля упругости поперечной арматуры
        public double Esw_X { get; internal set; }
        // Расчетное сопротивление поперечной арматуры растяжению;
        public double Rsw_X { get; set; }
        public double Asw_X { get; set; }
        /// <summary>
        /// Шаг по оси X
        /// </summary>
        public double Sw_X { get; set; }
        /// <summary>
        /// количество стержней по оси X
        /// </summary>
        public int N_X { get; set; }
        /// <summary>
        /// номинальный диаметр
        /// </summary>
        public int Dw_X { get; set; }
        /// <summary>
        /// Шаг по оси Y
        /// </summary>
        public double Sw_Y { get; set; }
        /// <summary>
        /// количество стержней по оси Y
        /// </summary>
        public int N_Y { get; set; }
        public int Dw_Y { get; set; }
        public double Esw_Y { get; set; }
        public double Rsw_Y { get; set; }
        public double Asw_Y { get; set; }

        public TypeYieldStress typeYieldStress { get; set; }
        public double k_s { get; set; }
        public double ls { get; set; }
        
        //Растояние до цента тяжести растянутой арматуры см
        public double a { get; set; }
        //Растояние до цента тяжести сжатой арматуры см
        public double a1 { get; set; }
        public double Epsilon_s => (Es > 0 ) ? Rs / Es : 0;

        public string DiagramType
        { 
            get
            {
                string res = BSHelper.TwoLineDiagram;
                if (typeYieldStress == TypeYieldStress.Physical)
                    res = BSHelper.TwoLineDiagram;
                if (typeYieldStress == TypeYieldStress.Offset)
                    res = BSHelper.ThreeLineDiagram;
                return res;
            }
        }

        /// <summary>
        /// П 6.2.13 СП 63
        /// </summary>
        public double Epsilon_s_ult
        {
            get
            {
                double res = 0;

                // А240–А500
                if (typeYieldStress == TypeYieldStress.Physical)
                {
                    res = 0.025; //СП63  П 6.2.14 , СП 360 П 6.1.25
                }
                else if (typeYieldStress == TypeYieldStress.Offset) //А600–А1000
                {
                    res = 0.015;
                }

                return res;
            }
        }
        
        public double Dzeta_R(double omega, double eps_fb2) => (eps_fb2!=0) ?omega / (1 + Epsilon_s / eps_fb2)  :0 ;

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }

    /// <summary>
    /// Таблица характеристики бетонов (СП 63.13)
    /// </summary>
    public class Beton2
    {
        public string Cls_b { get; set; }
        public double Rb_ser { get; set; }
        public double Rb { get; set; }
        public double Eb { get; set; }
        public double eps_b1 { get; set; }
        public double eps_b1_red { get; set; }
        public double eps_b2 { get; set; }
    }

    /// <summary>
    /// Характеристики арматуры СП 63.13
    /// </summary>
    public class Rod2
    {
        public string Cls_s { get; set; }
        public double Rs_ser { get; set; }
        public double Rs { get; set; }
        public double Rsc { get; set; }
        public double Es { get; set; }
        public double eps_s0 { get; set; }
        public double eps_s2 { get; set; }
    }


    /// <summary>
    /// Параметры бетонов и арматуры, считываем из json 
    /// </summary>
    public class BSFiberParams
    {
        public Units Units { get; set; }
        public FiberBeton Fiber { get; set; }
        public Rebar Rebar { get; set; }
        public Beton2 Beton2 { get; set; }
        public Rod2 Rod2 { get; set; }
    }


    /// <summary>
    /// Таблица 2 СП360
    /// </summary>
    public class BSFiberBeton
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }

        public double Rfbt3n { get; set; }

        public double Rfbt3_ser => Rfbt3n;

        public double Rfbt2n { get; set; }

        public double Rfbt2_ser => Rfbt2n;

        /// <summary>
        /// значения сопротивления сталефибробетона растяжению при классе сталефибробетона по остаточной прочности на растяжение, МПа
        /// </summary>
        public double Rfbt3 { get; set; }

        public double Rfbt2 { get; set; }

        /// <summary>
        /// Сжатие осевое
        /// </summary>
        public double Rfbn { get; set; }
    }

    public class StrengthFactors
    {
        public int Id { get; set; }
        public double Yb { get; set; }
        public double Yft { get; set; }
        public double Yb1 { get; set; }
        public double Yb2 { get; set; }
        public double Yb3 { get; set; }
        public double Yb5 { get; set; }
    }


    public class Elements
    {
        public int Id { get; set; }
        public string BT { get; set; }
        public double Rfbt3n { get; set; }
        public double Rfbt2n { get; set; }

        public double Rfbn { get; set; }
        public double Yb { get; set; }
        public double Yft { get; set; }
        public double Yb1 { get; set; }
        public double Yb2 { get; set; }
        public double Yb3 { get; set; }
        public double Yb5 { get; set; }
        public string i_B { get; set; }
        public double Rfbt3 { get; set; }
        public double Rfbt2 { get; set; }
    }



    /// <summary>
    /// Классы фибробетона по прочности на растяжение
    /// </summary>
    public class FiberConcreteClass
    {
        public int ID { get; set; }
        public string Bft { get; set; }
        public double Rfbt_n { get; set; }
    }

    /// <summary>
    /// Усилия
    /// </summary>
    public class Efforts
    {
        public int Id { get; set; }
        public double Mx { get; set; }
        public double My { get; set; }
        public double Mz { get; set; }
        public double N { get; set; }
        public double Qx { get; set; }
        public double Qy { get; set; }       
    }

    /// <summary>
    /// данные, для формы расчет балки
    /// </summary>
    public class InitForBeamDiagram
    {
        public double Force { get; set; }
        public double LengthX { get; set; }
    }

    /// <summary>
    /// Коэффициенты
    /// </summary>
    public class Coefficients
    {
        public int ID { get; set; }
        public string Y { get; set; }
        public double Val { get; set; }
        public string Name { get; set; }
        public string Descr { get; set; }
    }


    public class RFiber
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Rfser { get; set; }
        public double Rf { get; set; }
        public double G1 { get; set; }
        public double G2 { get; set; }
        public double Ef { get; set; }
        /// <summary>
        /// Коэф. Анкеровки фибры
        /// </summary>
        public double Hita_f { get; set; }
        /// <summary>
        /// Коэф. условной работы в зависимсоти от материала фибры
        /// </summary>
        public double Gamma_fb1 { get; set; }
        /// <summary>
        /// номер из таблицы FiberGeometry, которому соответсвует список стандартных значений диаметров для данного типа фибры
        /// </summary>
        public int IndexForGeometry { get; set; }
    }



    public class FiberType
    {
        public int ID { get; set; }
        /// <summary>
        /// Коэф. Анкеровки фибры
        /// </summary>
        public double Hita_f { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Коэф. условной работы в зависимсоти от материала фибры
        /// </summary>
        public double Gamma_fb1 { get; set; }

    }

    public class FiberKind
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Rfser { get; set; }
        public double Rf { get; set; }
        public double G1 { get; set; }
        public double G2 { get; set; }
        public double Ef { get; set; }

        /// <summary>
        /// Идентификатор из таблицы FiberType
        /// </summary>
        public int TypeID { get; set; }
        /// <summary>
        /// номер из таблицы FiberGeometry, которому соответсвует список стандартных значений диаметров для данного типа фибры
        /// </summary>
        public int IndexForGeometry { get; set; }
    }



    public class FiberGeometry
    {
        public int ID { get; set; }
        /// <summary>
        /// номер группы строк, соответсвующий значению из таблицы RFiber
        /// </summary>
        public int GeometryIndex { get; set; }
        /// <summary>
        /// Площадь сечеия фибры
        /// </summary>
        public double Square { get; set; }
        /// <summary>
        /// Диаметр фибры
        /// </summary>
        public double Diameter{ get; set; }
        /// <summary>
        /// номер из таблицы FiberLength, которому соответсвует список стандартных значений длины для данного диаметра
        /// </summary>
        public int IndexForLength { get; set; }
    }



    public class FiberLength
    {
        public int ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int LenghtIndex { get; set; }
        public double Length { get; set; }
    }


    public class RFibKor
    {
        public int ID { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
        public double E { get; set; }
        public double F { get; set; }
        public double G { get; set; }
        public double H { get; set; }
        public double I { get; set; }
    }


    /// <summary>
    /// структура описывает коэффициенты Kor и Kn для фибры
    /// </summary>
    public class Fiber_K
    {
        public int ID { get; set; }
        public double HL { get; set; }
        public double BL_05 { get; set; }
        public double BL_1 { get; set; }
        public double BL_2 { get; set; }
        public double BL_3 { get; set; }
        public double BL_5 { get; set; }
        public double BL_10 { get; set; }
        public double BL_20 { get; set; }
        public double BL_21 { get; set; }
    }


    /// <summary>
    /// Приложение Б. График aF(F)
    /// </summary>
    public class FaF
    {
        /// <summary>
        /// Номер измерения
        /// </summary>
        public  int Num { get; set; }
        /// <summary>
        /// перемещение надреза
        /// </summary>
        public  double aF { get; set; }
        /// <summary>
        /// усилие, Н
        /// </summary>
        public  double F { get; set; }

        public string LabId { get; set; }

        public FaF()
        {
        }
    }
 
    /// <summary>
    /// Результаты испытаний образцов
    /// </summary>
    public class FibLab
    {
        /// <summary>
        /// Идентификатор образца / испытания 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// максимальное значение нагрузки в интервале значений перемещения внешних граней надреза 0 < aF ≤ 0,05 мм
        /// </summary>
        public double Fel { get; set; }
        /// <summary>
        /// значение нагрузки, соответствующее перемещению внешних граней надреза aF = 0,5 мм
        /// </summary>
        public double F05 { get; set; }
        /// <summary>
        /// значение нагрузки, соответствующее перемещению внешних граней надреза aF = 2,5 мм;
        /// </summary>
        public double F25 { get; set; }

        /// <summary>
        /// длина пролета, мм
        /// </summary>
        public double L { get; set; }
        /// <summary>
        /// ширина образца, мм
        /// </summary>
        public double B { get; set; }
        /// <summary>
        /// расстояние между вершиной надреза и верхней гранью образца
        /// </summary>
        public double H_sp { get; set; }

        public FibLab()
        {
        }
    }


    /// <summary>
    /// связь между aF и прогибом f
    /// </summary>
    public class Deflection_f_aF
    {
        /// <summary>
        /// Номер образца
        /// </summary>
        public string Id  { get; set; }

        /// <summary>
        /// Номер испытания
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// перемещение надреза, мм
        /// </summary>
        public double aF { get; set; }

        /// <summary>
        /// прогиб, мм
        /// </summary>
        public double f { get; set; }
    }

    /// <summary>
    /// Местная прочность, таблица для расчета
    /// </summary>
    public class LocalStress
    {
        /// <summary>
        /// Номер переменной
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя переменной
        /// </summary>
        public string VarName { get; set; }

        /// <summary>
        /// Описание переменной
        /// </summary>
        public string VarDescr { get; set; }

        /// <summary>
        /// Числовое значение переменной
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Тип параметра
        /// </summary>
        public int Type { get; set; }
    }

    /// <summary>
    /// зависимость относительных диформаций от влажности воздуха
    /// </summary>
    public class EpsilonFromAirHumidity
    {
        public int Id { get; set; }
        /// <summary>
        /// строковое название диапазона влажности
        /// </summary>
        public string AirHumidityStr { get; set; }
        /// <summary>
        /// Левая граница диапазона влажности
        /// </summary>
        public double FirstBorder { get; set; }
        /// <summary>
        /// Правая граница диапазона влажногсти
        /// </summary>
        public double SecondBorder { get; set; }
        public double Eps_b0 { get; set; }
        public double Eps_b2 { get; set; }
        public double Eps_bt0 { get; set; }
        public double Eps_bt2 { get; set; }
    }



    /// <summary>
    /// геометрия сечений
    /// </summary>
    public class InitBeamSectionGeometry
    {
        /// <summary>
        /// нумерация соответсвует BeamSection
        /// </summary>
        public BeamSection SectionTypeNum { get; set; }
        /// <summary>
        /// названия сечений string (значение не используется)
        /// </summary>
        public string SectionTypeStr { get; set; }

        #region Габаритные размеры сечения. 
        //Для каждого типа сечения свой набор переменных, определяющий размеры.


        public double? b { get; set; }
        public double? h { get; set; }
        public double? bf { get; set; }
        public double? hf { get; set; }
        public double? bw { get; set; }
        public double? hw { get; set; }
        public double? b1f { get; set; }
        public double? h1f { get; set; }
        public double? r1 { get; set; }
        public double? r2 { get; set; }
        #endregion
    }



    /// <summary>
    /// Класc описывает диаметр[мм] и площадь[мм2] для Класса арматуры
    /// </summary>
    public class RebarDiameters
    { 
        public int ID { get; set; }
    
        public string TypeRebar { get; set; }

        /// <summary>
        /// Диаметр из стандартного ряда, мм
        /// </summary>
        public double Diameter { get; set; }    

        /// <summary>
        /// Площадь в см
        /// </summary>
        public double Square { get; set; }
    }


    /// <summary>
    /// Класс фибры
    /// </summary>
    public class FiberClass
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }


}

