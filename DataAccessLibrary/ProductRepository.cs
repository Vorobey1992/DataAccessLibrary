using ADO.NET_Fundamentals.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.NET_Fundamentals.Data
{
    public class ProductRepository : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _connection;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
        }

        // CRUD operations for Product
        public int CreateProduct(Product product)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            string insertProductQuery = @"
                INSERT INTO Products (Name, Description, Weight, Height, Width, Length)
                VALUES (@Name, @Description, @Weight, @Height, @Width, @Length);
                SELECT SCOPE_IDENTITY();
                                                        ";

            using SqlCommand command = new(insertProductQuery, connection);
            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Description", product.Description);
            command.Parameters.AddWithValue("@Weight", product.Weight);
            command.Parameters.AddWithValue("@Height", product.Height);
            command.Parameters.AddWithValue("@Width", product.Width);
            command.Parameters.AddWithValue("@Length", product.Length);

            // Выполняем команду и получаем Id с помощью ExecuteScalar
            int newProductId = Convert.ToInt32(command.ExecuteScalar());
            return newProductId;

        }

        public void UpdateProduct(int productId, Product updatedProduct)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            string updateProductQuery = @"
                    UPDATE Products
                    SET Name = @Name, Description = @Description, Weight = @Weight, Height = @Height, Width = @Width, Length = @Length
                    WHERE Id = @Id
                                     ";

            using SqlCommand command = new(updateProductQuery, connection);
            command.Parameters.AddWithValue("@Id", productId); // Используем переданный Id
            command.Parameters.AddWithValue("@Name", updatedProduct.Name);
            command.Parameters.AddWithValue("@Description", updatedProduct.Description);
            command.Parameters.AddWithValue("@Weight", updatedProduct.Weight);
            command.Parameters.AddWithValue("@Height", updatedProduct.Height);
            command.Parameters.AddWithValue("@Width", updatedProduct.Width);
            command.Parameters.AddWithValue("@Length", updatedProduct.Length);

            command.ExecuteNonQuery();
        }


        public void DeleteProduct(int productId)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            string deleteProductQuery = @"
                    DELETE FROM Products
                    WHERE Id = @Id
                ";

            using SqlCommand command = new(deleteProductQuery, connection);
            command.Parameters.AddWithValue("@Id", productId);
            command.ExecuteNonQuery();
        }

        public List<Product> GetAllProducts()
        {
            List<Product> products = new();

            using (SqlConnection connection = new(_connectionString))
            {
                connection.Open();

                string getAllProductsQuery = @"
                    SELECT Id, Name, Description, Weight, Height, Width, Length
                    FROM Products
                ";

                using SqlCommand command = new(getAllProductsQuery, connection);
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Product product = new()
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"].ToString(),
                        Weight = Convert.ToDouble(reader["Weight"]),
                        Height = Convert.ToDouble(reader["Height"]),
                        Width = Convert.ToDouble(reader["Width"]),
                        Length = Convert.ToDouble(reader["Length"])
                    };
                    products.Add(product);
                }
            }

            return products;
        }
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
