using System.Data;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BSFiberCore.Models.BL.Beam;
using Dapper;
using System.Linq;
using System.Data.SQLite;
using BSFiberCore.Models.BL.Lib;

namespace FiberSrv.Data;

public class BSData
{
    public static string ConfigId { get; set; }

    public static ProgConfig ProgConfig { get; set; }

    public static string ResourcePath(string _file) => Path.Combine(Environment.CurrentDirectory, "Resources", _file);  

    public static string DataPath(string _file)  => Path.Combine(Environment.CurrentDirectory, "Data", _file);

    public static string? connectionString { get; set; } = "Data Source=.\\DB\\Fiber.db";
            

    public static bool Connect()
    {
        bool ok = false;

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            ok = true;
        }         
        
        return ok;
    }

    static BSData()
    {
        ConfigId = "DefaultConnection";// BSFiberLib.ConfigDefault;            
    }

    /// <summary>
    /// Подключение к БД
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Строка подключения</returns>
    public static string  LoadConnectionString()
    {            
        string? s = System.Configuration.ConfigurationManager.ConnectionStrings[ConfigId]?.ConnectionString;
        if (string.IsNullOrEmpty(s))
            s = connectionString;
        return s;
    }

    /// <summary>
    /// Данные формы
    /// </summary>
    /// <returns>Список</returns>
    public static FormParams LoadFormParams()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<FormParams>("select * from Params where ID = 1", new DynamicParameters());
                if (output != null && output.Count() > 0)                    
                    return output.ToList()[0];                    
                else
                    return new FormParams();
            }
        }
        catch
        {
            return new FormParams();
        }
    }

    /// <summary>
    ///  Сохранить введенные пользователем значения с формы
    /// </summary>
    /// <param name="_prms"></param>
    public static void UpdateFormParams(FormParams _prms)
    {

        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {                        
                    int cnt = cnn.Execute(@"update Params set Length = @Length,
                                                        LengthCoef = @LengthCoef, BetonType = @BetonType, Fib_i = @Fib_i, Bft3n = @Bft3n,
                                                        Bfn = @Bfn, Bftn = @Bftn, Eb = @Eb, Efbt = @Efbt, 
                                                        Rs = @Rsw, Area_s = @Area_s, Area1_s = @Area1_s, a_s = @a_s, a1_s = @a1_s    
                                                    where ID=@ID ", _prms , tr);
                    
                    tr.Commit();
                }
            }
        }
        catch (Exception _e)
        {
            MessageBox.Show(_e.Message);
        }
    }


    /// <summary>
    /// Параметры расчета по НДМ
    /// </summary>
    /// <returns>Список</returns>
    public static NDMSetup LoadNDMSetup(int Id = 1)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<NDMSetup>($"select * from NDMSetup where Id = {Id}", new DynamicParameters());
                return output.ToList()[0];
            }
        }
        catch
        {
            return new NDMSetup() {Id = 0, Iters = 1000, M = 20, N = 20, MinAngle = 40, MaxArea = 10, BetonTypeId = 0 };
        }
    }

    /// <summary>
    /// Сохранить параметры расчета по НДМ
    /// </summary>        
    public static void SaveNDMSetup(NDMSetup _ndmSetup)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {
                    cnn.Execute($"update NDMSetup set " +
                        " Id = @Id, Iters = @Iters, N = @N, M = @M," +
                        " BetonTypeId = @BetonTypeId, MinAngle = @MinAngle, MaxArea = @MaxArea, " +
                        " UseRebar = @UseRebar, RebarType = @RebarType " +
                        " where Id = @Id",
                        _ndmSetup, tr);

                    tr.Commit();
                }
            }
        }
        catch
        {
            throw new Exception("Не удалось сохранить значения в БД");
        }
    }
  
    /// <summary>
    /// Наименования типов бетона
    /// </summary>
    /// <returns>Список</returns>
    public static List<string> LoadBetonTypeName()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("select Name from BetonType", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Классы бетона по сопротивлению на растяжение Bft 
    /// </summary>
    /// <returns>Список</returns>
    public static List<FiberBft> LoadFiberBft()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {                
                var output = cnn.Query<FiberBft>("select * from FiberBft order by Rfbtn", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<FiberBft>();
        }
    }

    /// <summary>
    /// Загрузка бетона по типу
    /// </summary>
    /// <returns>Список</returns>
    public static List<Beton> LoadBetonData(int _betonTypeId)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Beton>($"select * from Beton where BetonType = {_betonTypeId}", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<Beton>();
        }
    }

    /// <summary>
    /// Загрузка бетона
    /// </summary>
    /// <returns>Список</returns>
    public static List<Beton> LoadBetonData()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Beton>("select * from Beton", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<Beton>();
        }
    }

    /// <summary>
    /// Коэффициенты
    /// </summary>
    /// <returns>Список</returns>
    public static List<Coefficients> LoadCoeffs()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Coefficients>("select * from Coefficients", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<Coefficients>();
        }
    }

    /// <summary>
    /// Тип арматуры
    /// </summary>
    /// <returns></returns>
    public static List<Rebar> LoadRebar()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Rebar>("select * from Rebar", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<Rebar>();
        }
    }

    /// <summary>
    /// Армирование
    /// </summary>
    /// <returns></returns>
    public static List<BSRod> LoadBSRod(BeamSection _SectionType)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<BSRod>(string.Format("select * from BSRod where SectionType = {0}", (int)_SectionType), 
                                            new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<BSRod>();
        }
    }

    /// <summary>
    /// Сечение произвольной формы для расчета по НДМ
    /// </summary>
    /// <returns></returns>
    public static List<NdmSection> LoadNdmSection(string _SectionNum)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<NdmSection>(string.Format("select * from NdmSection where Num = '{0}'", _SectionNum),
                                            new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<NdmSection>();
        }
    }

    /// <summary>
    /// сохранить данные для формы "расчет балки"
    /// </summary>
    /// <param name="values"></param>
    public static void SaveForBeamDiagram(InitForBeamDiagram values)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {
                    cnn.Execute("DELETE FROM InitForBeamDiagram");

                    int cnt = cnn.Execute($"insert into InitForBeamDiagram (Force, LengthX) " +
                        $"values (@Force, @LengthX)", values, tr);

                    tr.Commit();
                }
            }
        }
        catch (Exception _e)
        {
            MessageBox.Show(_e.Message);
        }
    }

    /// <summary>
    /// Данные по фибробетону из БД
    /// </summary>
    /// <returns></returns>
    public static InitForBeamDiagram LoadForBeamDiagram()
    {
        try
        {
            string query = string.Format("select * from InitForBeamDiagram");
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                IEnumerable<InitForBeamDiagram> output = cnn.Query<InitForBeamDiagram>(query, new DynamicParameters());
                List<InitForBeamDiagram> res = output.ToList();
                if (res.Count == 0)
                { return null; }

                return res[0];
            }
        }
        catch (Exception _e)
        {
            MessageBox.Show(_e.Message);
            return null;
        }
    }

    /// <summary>
    /// Усилия 
    /// </summary>
    /// <returns>Список</returns>
    public static List<Efforts> LoadEfforts()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //var output = cnn.Query<Efforts>("select * from Efforts where id = 1", new DynamicParameters());
                var output = cnn.Query<Efforts>("select * from Efforts", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<Efforts>();
        }
    }

    // сохранить данные в бд по усилиям
    public static void SaveEfforts(Efforts _efforts)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {                        
                    int cnt = cnn.Execute("update Efforts set Mx = @Mx, My = @My, N = @N, Qx = @Qx, Qy = @Qy where Id = @Id ", _efforts, tr);
                    tr.Commit();
                }                    
            }
        }
        catch(Exception _e)
        {
            MessageBox.Show(_e.Message);
        }
    }



    // сохранить данные в бд по усилиям
    public static void SaveEfforts(List<Efforts> _efforts, bool _clear = true)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {
                    if (_clear)
                    {
                        cnn.Execute("DELETE FROM Efforts");
                    }    

                    for (int i = 0; _efforts.Count > i; i++)
                    {
                        Efforts tmpEfforts = _efforts[i];
                        
                        int cnt = cnn.Execute($"insert into Efforts (Id, Mx, Mx, My, N, Qx, Qy) " +
                            $"values (@Id, @Mx, @Mx, @My, @N, @Qx, @Qy)", tmpEfforts, tr);
                    }
                    tr.Commit();
                }
            }
        }
        catch (Exception _e)
        {
            MessageBox.Show(_e.Message);
        }
    }


    // удалить содержимое таблицы
    public static void ClearEfforts()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {
                    int cnt = cnn.Execute("DELETE FROM Efforts");
                    tr.Commit();
                }
            }
        }
        catch (Exception _e)
        {
            MessageBox.Show(_e.Message);
        }
    }


    /// <summary>
    /// Данные по фибробетону из БД
    /// </summary>
    /// <returns></returns>
    public static List<Elements> LoadFiberConcreteTable(string _iB = "")
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string query;
                if (_iB == "")
                    query = "select * from FiberConcrete";
                else
                    query = string.Format("select * from FiberConcrete where i_B = '{0}'", _iB);

                IEnumerable<Elements> output = cnn.Query<Elements>(query, new DynamicParameters());
                return output.ToList();
            }
        }
        catch (Exception _e)
        {
            MessageBox.Show(_e.Message);
            return new List<Elements>();
        }
    }

    public static List<FiberConcreteClass> LoadFiberConcreteClass()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<FiberConcreteClass>("select * from FiberConcreteClass", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<FiberConcreteClass>();
        }
    }

    public static List<RFibKor> LoadRFibKn()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<RFibKor>("select * from RFibKn", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<RFibKor>();
        }
    }

    public static List<Fiber_K> LoadFiber_Kor()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Fiber_K>("select * from Fiber_Kor", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<Fiber_K>();
        }
    }


    public static List<Fiber_K> LoadFiber_Kn()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Fiber_K>("select * from Fiber_Kn", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<Fiber_K>();
        }
    }

    public static List<RFibKor> LoadRFibKor()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<RFibKor>("select * from RFibKor", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<RFibKor>();
        }
    }

    /// <summary>
    ///  Данные по испытаниям образца (приложение "б")
    /// </summary>
    /// <param name="_LabId">Образец</param>
    /// <returns></returns>
    public static List<FaF> LoadRChartFaF(string _LabId)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<FaF>($"select * from RChartFaF where LabId = '{_LabId}'", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<FaF>();
        }
    }

    public static int RChartFaFMaxId()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<int>($"select max(Num) MaxNum from RChartFaF ", new DynamicParameters());
                return output.ToList()[0];
            }
        }
        catch
        {
            return 0;
        }
    }



    public static List<FibLab> LoadRFibLab()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<FibLab>("select * from RFibLab", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<FibLab>();
        }
    }

    public static List<Deflection_f_aF> LoadRDeflection(string _Id)
    {
        string query;
        if (string.IsNullOrEmpty(_Id))
        {
            query = string.Format("select * from RDeflection");
        }
        else
        {
            query = string.Format("select * from RDeflection where id == '{0}'", _Id);
        }

        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {                    
                var output = cnn.Query<Deflection_f_aF>(query, new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<Deflection_f_aF>();
        }
    }

    public static List<string> LoadBeamDeflection()
    {
        string query;
        
        query = string.Format("select Id from RDeflection group by Id ");
        
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>(query, new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Местные нагрузки LocalStress = LocalCompression 
    /// </summary>
    /// <returns>Данные и расчет </returns>
    public static List<LocalStress> LoadLocalStress()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<LocalStress>("select * from LocalStress order by id", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<LocalStress>();
        }
    }

    /// <summary>
    /// Местные нагрузки
    /// </summary>
    /// <returns>Данные и расчет </returns>
    public static List<LocalStress> LoadLocalPunch()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<LocalStress>("select * from LocalPunch order by id", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<LocalStress>();
        }
    }

    /// <summary>
    /// Относительные деформации бетона в зависимости от влажности воздуха
    /// </summary>
    /// <returns>Список</returns>
    public static List<EpsilonFromAirHumidity> LoadBetonEpsilonFromAirHumidity()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<EpsilonFromAirHumidity>("select * from EpsilonFromAirHumidity", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<EpsilonFromAirHumidity>();
        }
    }

    /// <summary>
    ///  сохранить расстановку стержней
    /// </summary>
    /// <param name="_ds"></param>
    /// <param name="_BeamSection"></param>
    public static void SaveRods(List<BSRod>  _ds, BeamSection  _BeamSection)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {
                    cnn.Execute(string.Format("delete from BSRod where SectionType = {0}", (int)_BeamSection), null , tr);

                    foreach (BSRod rod in _ds)
                    {
                        rod.SectionType = _BeamSection;
                        int cnt = cnn.Execute("insert into BSRod (CG_X, CG_Y, D, SectionType, Dnom) values (@CG_X, @CG_Y, @D, @SectionType, @Dnom)", rod, tr);
                        
                    }
                    tr.Commit();
                }
            }
        }
        catch
        {
            throw ;
        }
    }

    /// <summary>
    ///  сохранить точки сечения
    /// </summary>
    /// <param name="_ds"></param>
    /// <param name="_SectionNum"></param>
    public static void SaveSection(List<NdmSection> _ds, string _SectionNum)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {
                    cnn.Execute(string.Format("delete from NdmSection where Num = '{0}'", _SectionNum), null, tr);

                    foreach (var sec in _ds)
                    {                            
                        int cnt = cnn.Execute("insert into NdmSection (X, Y, N, Num) values (@X, @Y, @N, @Num)", sec, tr);

                    }
                    tr.Commit();
                }
            }
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Загружается геометрия сечений
    /// </summary>
    /// <returns>Список</returns>
    public static List<InitBeamSectionGeometry> LoadBeamSectionGeometry(BeamSection _SectionType)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<InitBeamSectionGeometry>("select * from InitBeamSection", new DynamicParameters());                                        

                return output.ToList();
            }
        }
        catch
        {
            throw;
        }
    }

    public static void UpdateBeamSectionGeometry(List<InitBeamSectionGeometry> beamSections)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {
                    foreach (InitBeamSectionGeometry bSection in beamSections)
                    {
                        cnn.Execute($"update InitBeamSection set bw = @bw, hw = @hw, bf = @bf, hf = @hf, b1f = @b1f, h1f = @h1f, r1 = @r1, r2 = @r2" +
                            $" where SectionTypeNum = @SectionTypeNum", bSection, tr);
                                                    
                    }
                    tr.Commit();
                }
            }
        }
        catch
        {
            throw;
        }
    }

    public static List<RebarDiameters> LoadRebarDiameters()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<RebarDiameters>("select * from RebarDiameters", new RebarDiameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<RebarDiameters>();
        }
    }

    /// <summary>
    /// Выборка арматуры
    /// </summary>
    /// <param name="_ClassRebar">Класс арматуры</param>
    /// <returns>Список - номинальные диаметры и площади сечения арматуры</returns>
    public static List<RebarDiameters> DiametersOfTypeRebar(string _ClassRebar)
    {
        List<RebarDiameters> rD = new List<RebarDiameters>();
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string query = $"select * from RebarDiameters where TypeRebar = '{_ClassRebar}'";
                var output = cnn.Query<RebarDiameters>(query, new RebarDiameters());
                rD = output.ToList();
                return rD;
            }
        }
        catch
        {
            return new List<RebarDiameters>();
        }
    }

    /// <summary>
    /// Класс фибры
    /// </summary>
    /// <returns></returns>
    public static List<FiberClass> LoadFiberClass()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<FiberClass>("select * from FiberClass", new DynamicParameters());
                return output.ToList();
            }
        }
        catch
        {
            return new List<FiberClass>();
        }
    }


    /// <summary>
    /// Параметры расчета на раскрытие трещины
    /// </summary>        
    public static NdmCrc LoadNdmCrc()
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<NdmCrc>("select * from NDMCrc where id = 1", new DynamicParameters());

                if (output != null /*&& output.Count() > 0*/)
                    return output.ToList()[0];
                else
                    return new NdmCrc() {Id =1, fi1 = 1.4, fi2 = 0.5, fi3 = 0.4, mu_fv = 0.015, kf = 1};                  
            }
        }
        catch
        {
            return new NdmCrc();
        }
    }

    /// <summary>
    /// Сохранить коэффициенты расчета на раскрытие трещины
    /// </summary>        
    public static void SaveNdmCrc(NdmCrc _NdmCrc)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {                                               
                    cnn.Execute("update NDMCrc set fi1 = @fi1, fi2 = @fi2, fi3 = @fi3, mu_fv = @mu_fv, psi_s = @psi_s where Id = @Id", 
                        _NdmCrc, tr);                                                    
                    tr.Commit();
                }
            }
        }
        catch
        {
            throw new Exception ("Не удалось сохранить значения в БД");
        }
    }

    /// <summary>
    /// Сохранить коэффициенты расчета на раскрытие трещины
    /// </summary>        
    public static void SaveStrengthFactors(StrengthFactors _sFactors)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {
                    cnn.Execute("update StrengthFactors set " +
                        " Yft = @Yft, Yb = @Yb, Yb1 = @Yb1, Yb2 = @Yb2, Yb3 = @Yb3, Yb5 = @Yb5 " +
                        " where Id = @Id",
                        _sFactors, tr);
                    tr.Commit();
                }
            }
        }
        catch
        {
            throw new Exception("Не удалось сохранить значения в БД");
        }
    }

    internal static void DeleteRFibLab(string _LabId)
    {
        try
        {
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Open();
                using (var tr = cnn.BeginTransaction())
                {
                    int cnt = cnn.Execute($"DELETE FROM RFibLab WHERE Id = '{_LabId}'");

                    cnt = cnn.Execute($"DELETE FROM RChartFaF WHERE LabId = '{_LabId}'");

                    tr.Commit();
                }
            }
        }
        catch (Exception _e)
        {
            MessageBox.Show(_e.Message);
        };
    }
}



