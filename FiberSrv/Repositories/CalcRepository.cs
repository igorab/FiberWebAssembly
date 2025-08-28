using BSFiberCore.Models.BL.Lib;
using Dapper;
using FiberSrv.Data;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.SQLite;

namespace FiberSrv.Repositories;

public class CalcRepository
{
    private readonly string _connectionString;

    public CalcRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Метод для проверки соединения с базой данных
    public async Task<bool> CheckDatabaseConnectionAsync()
    {
        try
        {
            using (SqliteConnection dbConnection = new SqliteConnection(_connectionString))
            {
                await dbConnection.OpenAsync();
                // Выполняем простой запрос, чтобы проверить соединение
                var result = await dbConnection.QuerySingleOrDefaultAsync<int>("SELECT 1");
                return result == 1;
            }
        }
        catch
        {
            return false; // Если произошла ошибка, возвращаем false
        }
    }

    public async Task<IEnumerable<CalcParameters>> GetCalcParametersAsync()
    {
        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            var parameters = await dbConnection.QueryAsync<CalcParameters>("SELECT * FROM CalcParameters");
            return parameters;
        }
    }

    public async Task AddCalcParameterAsync(CalcParameters parameter)
    {
        using (IDbConnection dbConnection = new SqliteConnection(_connectionString))
        {
            dbConnection.Open();
            var sqlQuery = "INSERT INTO CalcParameters (Length, Width, Name) VALUES (@Length, @Width, @Name)";
            await dbConnection.ExecuteAsync(sqlQuery, parameter);
        }
    }    
}
