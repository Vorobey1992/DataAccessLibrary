using ADO.NET_Fundamentals.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO.NET_Fundamentals.Data
{
    public class OrderRepository : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _connection;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
        }

        // CRUD operations for Order
        public int CreateOrder(Order order)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            string insertOrderQuery = @"
                    INSERT INTO Orders (Status, CreatedDate, UpdatedDate, ProductId)
                    VALUES (@Status, @CreatedDate, @UpdatedDate, @ProductId)
                    SELECT SCOPE_IDENTITY();
                ";

            using SqlCommand command = new(insertOrderQuery, connection);
            command.Parameters.AddWithValue("@Status", order.Status);
            command.Parameters.AddWithValue("@CreatedDate", order.CreatedDate);
            command.Parameters.AddWithValue("@UpdatedDate", order.UpdatedDate);
            command.Parameters.AddWithValue("@ProductId", order.ProductId);

            // Выполняем команду и получаем Id с помощью ExecuteScalar
            int newOrderId = Convert.ToInt32(command.ExecuteScalar());
            return newOrderId;
        }

        public void UpdateOrder(int orderId, Order order)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            string updateOrderQuery = @"
                    UPDATE Orders
                    SET Status = @Status, CreatedDate = @CreatedDate, UpdatedDate = @UpdatedDate, ProductId = @ProductId
                    WHERE Id = @Id
                ";

            using SqlCommand command = new(updateOrderQuery, connection);
            command.Parameters.AddWithValue("@Id", orderId);
            command.Parameters.AddWithValue("@Status", order.Status);
            command.Parameters.AddWithValue("@CreatedDate", order.CreatedDate);
            command.Parameters.AddWithValue("@UpdatedDate", order.UpdatedDate);
            command.Parameters.AddWithValue("@ProductId", order.ProductId);

            command.ExecuteNonQuery();
        }

        public void DeleteOrder(int orderId)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            string deleteOrderQuery = @"
                    DELETE FROM Orders
                    WHERE Id = @Id
                ";

            using SqlCommand command = new(deleteOrderQuery, connection);
            command.Parameters.AddWithValue("@Id", orderId);
            command.ExecuteNonQuery();
        }

        public List<Order> GetOrdersByFilter(DateTime startDate, DateTime endDate, OrderStatus status, int productId)
        {
            List<Order> orders = new();

            using (SqlConnection connection = new(_connectionString))
            {
                connection.Open();

                string getOrdersByFilterQuery = @"
                    SELECT Id, Status, CreatedDate, UpdatedDate, ProductId
                    FROM Orders
                    WHERE CreatedDate >= @StartDate AND CreatedDate <= @EndDate AND Status = @Status AND ProductId = @ProductId
                ";

                using SqlCommand command = new(getOrdersByFilterQuery, connection);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@ProductId", productId);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Order order = new()
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Status = Enum.Parse<OrderStatus>(reader["Status"].ToString()), // Преобразуем строку в перечисление
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                        UpdatedDate = Convert.ToDateTime(reader["UpdatedDate"]),
                        ProductId = Convert.ToInt32(reader["ProductId"])
                    };
                    orders.Add(order);
                }
            }

            return orders;
        }

        public void BulkDeleteOrdersByFilter(DateTime startDate, DateTime endDate, OrderStatus status, int productId)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            SqlTransaction transaction = connection.BeginTransaction(); // Начало транзакции

            try
            {
                string bulkDeleteOrdersQuery = @"
            DELETE FROM Orders
            WHERE CreatedDate >= @StartDate AND CreatedDate <= @EndDate AND Status = @Status AND ProductId = @ProductId
        ";

                using SqlCommand command = new(bulkDeleteOrdersQuery, connection);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@ProductId", productId);

                // Устанавливаем транзакцию для команды
                command.Transaction = transaction;

                command.ExecuteNonQuery();

                // Если все операции успешно выполнены, фиксируем транзакцию
                transaction.Commit();
            }
            catch (Exception)
            {
                // В случае ошибки, выполняем роллбек, чтобы отменить все изменения
                transaction.Rollback();
                throw; // Передаем ошибку дальше
            }
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
