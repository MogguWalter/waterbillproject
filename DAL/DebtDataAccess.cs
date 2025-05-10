using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using WaterBillManagementSystem.DAL;
using WaterBillManagementSystem.Entities;

namespace WaterBillManagementSystem.DAL
{
    public class DebtDataAccess
    {
        public bool InsertDebt(DebtDTO debt)
        {
            // Chỉ insert SerialID và months vào bảng Debts
            string query = "INSERT INTO Debts (SerialID, months) VALUES (@SerialID, @Month)";
            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.AddWithValue("@SerialID", debt.SerialID);
                cmd.Parameters.AddWithValue("@Month", debt.Month); // Đảm bảo đúng kiểu
                try
                {
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error inserting debt: " + ex.Message);
                    return false;
                }
            }
        }

        public DataTable GetAllDebtsRaw() // Lấy dữ liệu thô từ bảng Debts
        {
            DataTable dt = new DataTable();
            string query = "SELECT SerialID, months FROM Debts ORDER BY SerialID, months";
            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                try
                {
                    cnn.Open();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error getting all debts: " + ex.Message);
                }
            }
            return dt;
        }

        // Lấy nợ + giá tiền cho User (dùng cho debtsuser)
        public DataTable GetDebtsWithPriceForUser(int serialId)
        {
            DataTable dt = new DataTable();
            // Query JOIN từ code gốc debtsuser.cs
            string query = @"
                 SELECT D.SerialID, D.months, c.price
                 FROM Debts D
                 INNER JOIN consumption c ON D.SerialID = c.SerialID AND D.months = c.months
                 WHERE D.SerialID = @SerialID";
            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.AddWithValue("@SerialID", serialId);
                try
                {
                    cnn.Open();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error getting user debts with price: " + ex.Message);
                }
            }
            return dt;
        }


        public bool DeleteDebt(int serialId, DateTime month)
        {
            // Xóa khỏi bảng Debts
            bool success = false;
            string query = "DELETE FROM Debts WHERE SerialID = @SerialID AND months = @Month";
            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.AddWithValue("@SerialID", serialId);
                cmd.Parameters.AddWithValue("@Month", month);
                try
                {
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    success = rowsAffected > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error deleting debt: " + ex.Message);
                }
            }
            return success;
        }

        public DataTable GetDebtsBySerialId(int serialId)
        {
            DataTable dt = new DataTable();
            string query = @"
        SELECT
            D.SerialID,
            D.months,
            c.price AS AmountDue -- Lấy giá tiền từ bảng consumption làm số tiền nợ
        FROM
            dbo.Debts D
        INNER JOIN -- Hoặc LEFT JOIN nếu muốn hiển thị nợ ngay cả khi consumption bị thiếu (không nên)
            dbo.consumption c ON D.SerialID = c.SerialID AND D.months = c.months
        WHERE
            D.SerialID = @SerialIDToSearch
        ORDER BY
            D.months";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.Add("@SerialIDToSearch", SqlDbType.Int).Value = serialId;
                try
                {
                    cnn.Open();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dt);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error getting debts for SerialID {serialId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error GetDebtsBySerialId: {ex.ToString()}");
                }
            }
            return dt;
        }
    }
}

