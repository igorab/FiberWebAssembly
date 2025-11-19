using System.Data;
using System.Data.SQLite;
using Dapper;

namespace BSFiberCore.Models.BL.Lib
{
    public class BSFiberLib
    {
        /// <summary>
        /// Фибробетон с металлической фиброй и металлической арматурой, расчет по СП360
        /// </summary>        
        public const string ConfigDefault = "Fiber";
        /// <summary>
        /// Фибробетон с неметаллической фиброй и металлической арматурой, расчет по СП297
        /// </summary>
        public const string Config297 = "Fiber297";
        /// <summary>
        /// Фибробетон с неметаллической фиброй и полимерной арматурой, расчет по СП405
        /// </summary>
        public const string Config405 = "Fiber405";        

        /// <summary>
        /// 1 группа предельных состояний
        /// </summary>
        public const int CG1 = 1;

        /// <summary>
        /// 2 группа пределеных состояний
        /// </summary>
        public const int CG2 = 2;

        public const string TypeOfDiagram = BSHelper.ThreeLineDiagram;

        public const string TypeOfMaterial = BSHelper.FiberConcrete;

        public const string TitleTension = "Напряжения";
        public const string TitleRelativeDeformation = "Относительные деформации";

        public const string RebarClassDefault = "A400";

        public const double Fi = 0.9;

        public const string TxtStaticEqCalc = "Расчет изгибаемых элементов, внецентренно сжатых элементов по методу статического равновесия. Расчет элементов по наклонным сечениям";
        public const string TxtCalc_Deform = "Расчет по прочности нормальных сечений на основе нелинейной деформационной модели";

        /// <summary>
        /// Вычислить модуль упругости фибробетона на растяжение
        /// </summary>
        /// <param name="_Eb">Модуль упругости бетона</param>
        /// <param name="_Ef">Модуль упругости фибры</param>
        /// <param name="_mu_fv">Коэффициент фиброового армирования</param>
        /// <returns>Модуль упругости фибробетона на растяжение</returns>
        public static double E_fb(double _Eb, double _Ef, double _mu_fv)
        {
            double e_fb = _Eb * (1 - _mu_fv) + _Ef * _mu_fv;
            return e_fb;
        }

        /// <summary>
        /// Прочности фибробетона на растяжение
        /// </summary>
        public static List<BSFiberBeton> BetonList => new List<BSFiberBeton>(/*Lib.BSQuery.LoadBSFiberBeton()*/);
        

        // перенести в БД
        public static Dictionary<int, double> Fi_b_cr_75 = new Dictionary<int, double>
        {
            [10] = 2.8, [15] = 2.4, [20] = 2.0, [25] = 1.8, [30] = 1.6, [35] = 1.5, [40] = 1.4, [45] = 1.3, [50] = 1.2, [55] = 1.1, [60] = 1.0
        };

        // перенести в БД
        public static Dictionary<int, double> Fi_b_cr_45_75 = new Dictionary<int, double>
        {
            [10] = 3.9,
            [15] = 3.4,
            [20] = 2.8,
            [25] = 2.5,
            [30] = 2.3,
            [35] = 2.1,
            [40] = 1.9,
            [45] = 1.8,
            [50] = 1.6,
            [55] = 1.5,
            [60] = 1.4
        };

        // перенести в БД
        public static Dictionary<int, double> Fi_b_cr_40 = new Dictionary<int, double>
        {
            [10] = 5.6,
            [15] = 4.8,
            [20] = 4.0,
            [25] = 3.6,
            [30] = 3.2,
            [35] = 3.0,
            [40] = 2.8,
            [45] = 2.6,
            [50] = 2.4,
            [55] = 2.2,
            [60] = 2.0
        };

        //СП63 6.1.15
        public static double CalcFi_b_cr(int _airHumidityId, int _betonClassId)
        {
            Dictionary<int, double> DFi = new Dictionary<int, double>();

            if (_airHumidityId == 1)
            {
                DFi = Fi_b_cr_75;
            }
            else if (_airHumidityId == 2)
            {
                DFi = Fi_b_cr_45_75;
            }
            else if (_airHumidityId == 3)
            {
                DFi = Fi_b_cr_45_75;
            }
            else
            {
                return 0;
            }

            if (_betonClassId >= 10)
            {
                int bClassId = _betonClassId;
                if (_betonClassId > 60) bClassId = 60;

                if (DFi.TryGetValue(bClassId, out double fivalue))
                    return fivalue;
            }

            return 0;
        }

        /// <summary>
        /// Значения по-умолчанию для коэффициентов на форме
        /// </summary>
        public static Elements PhysElements
        {
            get
            {
                try
                {
                    using (IDbConnection cnn = new SQLiteConnection(Lib.BSData.LoadConnectionString()))
                    {
                        IEnumerable<Elements> output = new List<Elements>();// cnn.Query<Elements>("select * from FiberConcrete where id = 1", new DynamicParameters());
                        Elements elements = output?.Count() > 0 ? output.First() : new Elements();
                        return elements;                        
                    }
                }
                catch
                {
                    return new Elements { Rfbt3n = 30.58, Yb = 1.3, Rfbn = 224.0, Yft = 1.3, Yb1 = 0.9, Yb2 = 0.9, Yb3 = 0.9, Yb5 = 1, i_B = "a" };
                }               
            }
        }

        /// <summary>
        /// Значения по-умолчанию для коэффициентов на форме
        /// </summary>
        public static StrengthFactors StrengthFactors()
        {            
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(Lib.BSData.LoadConnectionString()))
                {
                    string sql = "select * from StrengthFactors where id = 1";
                    IEnumerable<StrengthFactors> rec = new List<StrengthFactors>(); // = cnn.Query<StrengthFactors>(sql, new DynamicParameters());

                    StrengthFactors elements = rec?.Count() > 0 ? rec.First() : new StrengthFactors();
                    return elements;
                }
            }
            catch
            {
                return new StrengthFactors { Yft = 1.3, Yb = 1.3, Yb1 = 0.9, Yb2 = 0.9, Yb3 = 0.9, Yb5 = 1 };
            }            
        }

    }
}
