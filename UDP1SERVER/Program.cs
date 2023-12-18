using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.Data;

namespace UDP1SERVER
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpClient server = new UdpClient(9050);
            string connString = "Data Source=DESKTOP-SQL;Initial Catalog=CarsDB;Integrated Security=True";
            IPEndPoint remoteIp = null;
            while (true)
            {
                byte[] bytes = server.Receive(ref remoteIp);
                string request = Encoding.UTF8.GetString(bytes);
                string[] parts = request.Split(':');
                string response;
                switch (parts[0])
                {
                    case "1":
                        response = GetCarById(parts[1], connString);
                        break;
                    case "2":
                        response = GetCarsByPrice(parts[1], parts[2], connString);
                        break;
                    case "3":
                        response = GetCarsByBrandModel(parts[1], parts[2], connString);
                        break;
                    case "4":
                        response = SetCarUnavailable(parts[1], connString);
                        break;
                    default:
                        response = "Wrong request";
                        break;
                }
                byte[] sendBytes = Encoding.UTF8.GetBytes(response);
                server.Send(sendBytes, sendBytes.Length, remoteIp);
            }
        }

        static string GetCarById(string id, string connString)
        {
            string sql = "SELECT * FROM Cars WHERE Id = @Id";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    conn.Open();
                    string result = "";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                result += $"Id: {reader["Id"]}\nModel: {reader["Model"]}\nPrice: {reader["Price"]}";
                            }
                        }
                        else
                        {
                            result = "Car not found";
                        }
                    }
                    return result;
                }
            }
        }
        static string GetCarsByPrice(string minPrice, string maxPrice, string connString)
        {
            string sql = "SELECT * FROM Cars WHERE Price BETWEEN @Min AND @Max";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Min", SqlDbType.Int).Value = minPrice;
                    cmd.Parameters.Add("@Max", SqlDbType.Int).Value = maxPrice;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        string result = "";
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                result += $"Id: {reader["Id"]} \n" +
                                          $"Model: {reader["Model"]} \n" +
                                          $"Price: {reader["Price"]} \n\n";
                            }
                        }
                        else
                        {
                            result = "No cars in this price range";
                        }
                        return result;
                    }
                }
            }
        }
        static string GetCarsByBrandModel(string brand, string model, string connString)
        {
            string sql = "SELECT * FROM Cars WHERE Brand = @Brand AND Model = @Model";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Brand", SqlDbType.NVarChar).Value = brand;
                    cmd.Parameters.Add("@Model", SqlDbType.NVarChar).Value = model;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        string result = "";
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                result += $"Id: {reader["Id"]} \n" +
                                          $"Brand: {reader["Brand"]} \n" +
                                          $"Model: {reader["Model"]} \n" +
                                          $"Price: {reader["Price"]} \n\n";
                            }
                        }
                        else
                        {
                            result = "No cars with this brand and model";
                        }
                        return result;
                    }
                }
            }
        }

        static string SetCarUnavailable(string id, string connString)
        {
            string sql = "UPDATE Cars SET IsAvailable = 0 WHERE Id = @Id";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    conn.Open();
                    int rowsUpdated = cmd.ExecuteNonQuery();
                    return $"{rowsUpdated} cars set unavailable";
                }
            }
        }
    }
}