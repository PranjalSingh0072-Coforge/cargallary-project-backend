using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperApi.Models;


namespace DapperApi.Repositories
{
    public class ProductRepository
    {
        private readonly string _connectionString;



#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ProductRepository(IConfiguration configuration)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        {

#pragma warning disable CS8601 // Possible null reference assignment.
            _connectionString = configuration.GetConnectionString("DefaultConnection");
#pragma warning restore CS8601 // Possible null reference assignment.

        }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ProductRepository(string? _connectionString)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        {

#pragma warning disable CS8601 // Possible null reference assignment.
            this._connectionString = _connectionString;
#pragma warning restore CS8601 // Possible null reference assignment.

        }
        

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Product>("SELECT id,  Name, Price,  ImageUrl, Specifications, CreatedDate, UpdateDate ,Quantity FROM Products");
        }

      public async Task<Product> GetByIdAsync(int id)
{
    using (var connection = new SqlConnection(_connectionString))
    {
        #pragma warning disable CS8603 // Possible null reference return.
        return await connection.QuerySingleOrDefaultAsync<Product>(
            "SELECT  Name, Price, ImageUrl, Specifications, CreatedDate, UpdateDate, Quantity FROM Products WHERE Id = @Id", 
            new { Id = id }
        );
        #pragma warning restore CS8603 // Possible null reference return.
    }
}

        // public async Task<int> AddAsync(Product product)
        // {
        //     using (var connection = new SqlConnection(_connectionString))
        //     {
        //         var sql = "INSERT INTO Products (  id, Name, Price, ImageUrl, Specifications, CreatedDate, UpdateDate,Quantity,) VALUES (@Name, @Price, @ImageUrl, @Specifications, @CreatedDate, @UpdateDate, @Quantity); SELECT CAST(SCOPE_IDENTITY() as int)";
        //         return await connection.ExecuteScalarAsync<int>(sql, product);
        //     }
        // }
         public async Task<int> AddAsync(Product product)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var sql = "INSERT INTO Products (Name, Price, ImageUrl, Specifications, CreatedDate, UpdateDate, Quantity) VALUES (@Name, @Price, @ImageUrl, @Specifications, @CreatedDate, @UpdateDate, @Quantity); SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, product);
        }
    }

        public async Task<int> UpdateAsync(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "UPDATE Products SET Name = @Name, Price = @Price,  ImageUrl = @ImageUrl, Specifications = @Specifications, UpdateDate = @UpdateDate, Quantity=@Quantity, WHERE Id = @Id";
                return await connection.ExecuteAsync(sql, product);
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "DELETE FROM Products WHERE Id = @Id";
                return await connection.ExecuteAsync(sql, new { Id = id });
            }
        }

        public async Task SaveData(List<Product> products)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                foreach (var product in products)
                {
                    var query = "INSERT INTO Products (Name, Price, ImageUrl, Specifications, CreatedDate, UpdateDate, Quantity) VALUES (@Name, @Price, @ImageUrl, @Specifications, @CreatedDate, @UpdateDate, @Quantity)";
                    await connection.ExecuteAsync(query, product);
                }
            }
        }
    }
}