using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using WaterBillManagementSystem.DAL;

namespace WaterBillManagementSystem.DAL
{
    public class AdminDataAccess
    {
        public string GetStoredAdminPasswordHash()
        {
            string storedHash = null;
            string query = "SELECT TOP 1 pass FROM adminpass";
            try
            {
                // Sử dụng DatabaseHelper để lấy kết nối
                using (SqlConnection cnn = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, cnn))
                    {
                        cnn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            storedHash = result.ToString();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetStoredAdminPasswordHash: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Generic Error in GetStoredAdminPasswordHash: {ex.Message}");
            }

            return storedHash;
        }
    }
}
