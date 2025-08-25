using Dapper;
using System.Data.SQLite;
using BSFiberCore.Models.BL.Lib;

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
