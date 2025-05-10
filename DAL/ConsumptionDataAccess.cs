using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using WaterBillManagementSystem.Entities; // Cần using Entities

namespace WaterBillManagementSystem.DAL
{
    public class ConsumptionDataAccess
    {
        public bool InsertConsumptionRecord(ConsumptionDTO record)
        {
            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("usp_insert", cnn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = record.SerialID;
                cmd.Parameters.Add("@months", SqlDbType.Date).Value = record.Month;
                cmd.Parameters.Add("@consumptionamount", SqlDbType.Decimal).Value = record.ConsumptionAmount; // DbType cho numeric
                                                                                                              // Đặt Precision và Scale nếu cần cho Decimal
                cmd.Parameters["@consumptionamount"].Precision = 9;
                cmd.Parameters["@consumptionamount"].Scale = 3;
                cmd.Parameters.Add("@segmentNumber", SqlDbType.Int).Value = record.SegmentNumber;

                try
                {
                    cnn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException ex)
                {
                    // Mã lỗi 2627 là lỗi trùng khóa chính (PK violation)
                    // Mã lỗi 2601 là lỗi trùng khóa duy nhất (Unique constraint violation)
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        Console.WriteLine($"SQL Error inserting consumption: Duplicate record for SerialID={record.SerialID}, Month={record.Month:yyyy-MM}. {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"SQL Error inserting consumption (Duplicate PK/UQ): {ex.ToString()}");
                    }
                    else
                    {
                        Console.WriteLine("SQL Error inserting consumption: " + ex.Message);
                        System.Diagnostics.Debug.WriteLine($"SQL Error inserting consumption: {ex.ToString()}");
                    }
                    return false; // Trả về false khi có lỗi SQL
                }
                catch (Exception ex)
                {
                    Console.WriteLine("General Error inserting consumption: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine($"General Error inserting consumption: {ex.ToString()}");
                    return false;
                }
            }
        }

        public bool UpdateConsumptionPrice(int serialId, DateTime month, decimal calculatedPrice)
        {
            string query = @"SET NOCOUNT OFF;
                       UPDATE dbo.consumption
                       SET price = @Price
                       WHERE SerialID = @SerialID AND months = @Month";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                // Dùng kiểu Money hoặc Decimal phù hợp với cột 'price' trong DB
                cmd.Parameters.Add("@Price", SqlDbType.Money).Value = calculatedPrice;
                cmd.Parameters.Add("@SerialID", SqlDbType.Int).Value = serialId;
                cmd.Parameters.Add("@Month", SqlDbType.Date).Value = month;

                try
                {
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected == 1; // Chỉ thành công nếu cập nhật đúng 1 dòng
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error updating price for {serialId}/{month:yyyy-MM}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error updating price: {ex.ToString()}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error updating price for {serialId}/{month:yyyy-MM}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"General Error updating price: {ex.ToString()}");
                    return false;
                }
            }
        }

        public DataTable GetAllConsumptionRecords()
        {
            DataTable dt = new DataTable();
            string query = @"
             SELECT
                cons.SerialID,
                cons.months,
                cons.consumptionamount,
                cons.segmentNumber,
                cons.price,
                ci.CustomerType 
             FROM
                dbo.consumption cons
             LEFT JOIN
                dbo.CustomerInfo ci ON cons.SerialID = ci.SerialID
             ORDER BY
                cons.SerialID, cons.months";

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
                    Console.WriteLine("SQL Error getting all consumption with customer type: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine($"SQL Error getting all consumption: {ex.ToString()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("General Error getting all consumption: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine($"General Error getting all consumption: {ex.ToString()}");
                }
            }
            return dt; // Trả về DataTable
        }

        public bool DeleteConsumption(int serialId, DateTime month)
        {
            string query = @"SET NOCOUNT OFF; -- Thêm để đảm bảo rowsAffected trả về đúng
                    DELETE FROM dbo.consumption
                    WHERE SerialID = @SerialID AND months = @Month";
            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.Add("@SerialID", SqlDbType.Int).Value = serialId;
                cmd.Parameters.Add("@Month", SqlDbType.Date).Value = month;
                try
                {
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected == 1;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error deleting consumption for {serialId}/{month:yyyy-MM}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error deleting consumption: {ex.ToString()}");
                    return false; // Trả về false khi có lỗi
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error deleting consumption for {serialId}/{month:yyyy-MM}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"General Error deleting consumption: {ex.ToString()}");
                    return false;
                }
            }
        }

        public DataTable GetConsumptionDetailsForUser(int serialId)
        {
            DataTable dt = new DataTable();
            string query = @"
                SELECT c.SerialID, c.UserName, c.Address, s.months, s.consumptionamount, s.segmentNumber, s.price
                FROM CustomerInfo c
                INNER JOIN consumption s ON c.SerialID = s.SerialID
                WHERE c.SerialID = @SerialID";

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
                    Console.WriteLine("SQL Error getting user consumption details: " + ex.Message);
                }
            }
            return dt;
        }

        public DataTable GetConsumptionRecordsBySerialId(int serialId)
        {
            DataTable dt = new DataTable();
            string query = @"
            SELECT
                cons.SerialID,
                cons.months,
                cons.consumptionamount,
                cons.segmentNumber,
                cons.price,
                ci.CustomerType
            FROM
                dbo.consumption cons
            LEFT JOIN
                dbo.CustomerInfo ci ON cons.SerialID = ci.SerialID
            WHERE
                cons.SerialID = @SerialIDToSearch -- Thêm điều kiện WHERE
            ORDER BY
                cons.months";

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
                    Console.WriteLine($"SQL Error getting consumption for SerialID {serialId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error GetConsumptionRecordsBySerialId: {ex.ToString()}");
                }
            }
            return dt;
        }
    }
}
