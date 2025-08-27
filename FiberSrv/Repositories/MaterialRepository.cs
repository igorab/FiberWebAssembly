using BSFiberCore.Models.BL.Lib;
using Dapper;
using System.Data;
using System.Data.SQLite;

namespace FiberSrv.Repositories
{
    public class MaterialRepository
    {
        private readonly string _connectionString;
        private string LoadConnectionString() => _connectionString;
        public MaterialRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// справочник диаметров арматуры
        /// </summary>
        /// <returns></returns>
        public async Task<List<RebarDiameters>> LoadRebarDiameters()
        {
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    await cnn.OpenAsync();
                    var output = await cnn.QueryAsync<RebarDiameters>(@$"select * from RebarDiameters", new RebarDiameters());
                    return output.ToList();
                }
            }
            catch
            {
                return new List<RebarDiameters>();
            }
        }

        /// <summary>
        /// Данные по фибробетону из БД
        /// </summary>
        /// <returns></returns>
        public async  Task< List<Elements>> LoadFiberConcreteTable(string _iB = "")
        {
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    await cnn.OpenAsync();
                    string query;
                    if (_iB == "")
                        query = "select * from FiberConcrete";
                    else
                        query = string.Format("select * from FiberConcrete where i_B = '{0}'", _iB);

                    IEnumerable<Elements> output = await cnn.QueryAsync<Elements>(query, new DynamicParameters());
                    return output.ToList();
                }
            }
            catch (Exception _e)
            {
                MessageBox.Show(_e.Message);
                return new List<Elements>();
            }
        }


        /// <summary>
        /// Тип арматуры
        /// </summary>
        /// <returns></returns>
        public async Task<List<Rebar>> LoadRebar()
        {
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    await cnn.OpenAsync();
                    var output = await cnn.QueryAsync<Rebar>("select * from Rebar", new DynamicParameters());
                    return output.ToList();
                }
            }
            catch
            {
                return new List<Rebar>();
            }
        }


        /// <summary>
        /// Выборка Таблицы 2 выборка классов фибробетона по остаточной прочности на растяжение
        /// </summary>
        /// <returns></returns>
        public async Task<List<BSFiberBeton>> LoadBSFiberBeton()
        {
            List<BSFiberBeton> bt = new List<BSFiberBeton>();
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    await cnn.OpenAsync();
                    string query = "select * from BSFiberBeton";
                    var output = await cnn.QueryAsync<BSFiberBeton>(query, new DynamicParameters());
                    bt = output.ToList();
                }
            }
            catch { }
            return bt;
        }

        /// <summary>
        /// Классы бетона по сопротивлению на растяжение Bft 
        /// </summary>
        /// <returns>Список</returns>
        public async Task<List<FiberBft>> LoadFiberBft()
        {
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    await cnn.OpenAsync();
                    var output = await cnn.QueryAsync<FiberBft>("select * from FiberBft order by Rfbtn", new DynamicParameters());
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
        public  async Task<List<Beton>> LoadBetonData(int _betonTypeId)
        {
            try
            {
                using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    await cnn.OpenAsync();
                    IEnumerable<Beton> output = await cnn.QueryAsync<Beton>($"select * from Beton where BetonType = {_betonTypeId}", new DynamicParameters());
                    List<Beton> res = output.ToList();
                    return res;
                }
            }
            catch (Exception _e)
            {
                return new List<Beton>();
            }
        }


    }
}
