using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using WaterBillManagementSystem.Entities;

namespace WaterBillManagementSystem.DAL
{
    public class UserDataAccess
    {
        public string GetStoredPasswordHash(string username)
        {
            string storedHash = null;
            // Câu lệnh SQL chỉ lấy password hash dựa trên username
            string query = "SELECT passwords FROM CustomerInfo WHERE UserName = @Username";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                try
                {
                    cnn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        storedHash = result.ToString();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error getting password hash: " + ex.Message);
                }
            }
            return storedHash; // Trả về hash đã lưu
        }

        public bool UserNameExists(string userName)
        {
            string query = "SELECT COUNT(1) FROM dbo.CustomerInfo WHERE UserName = @UserName";
            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = userName;
                try
                {
                    cnn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error checking if UserName exists ({userName}): {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error UserNameExists: {ex.ToString()}");
                    return true;
                }
            }
        }

        public bool SerialIdExists(int serialId)
        {
            string query = "SELECT COUNT(1) FROM dbo.CustomerInfo WHERE SerialID = @SerialID";
            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.Add("@SerialID", SqlDbType.Int).Value = serialId;
                try
                {
                    cnn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error checking if SerialID exists ({serialId}): {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error SerialIdExists: {ex.ToString()}");
                    return true; // An toàn khi có lỗi DB, coi như đã tồn tại
                }
            }
        }

        public bool InsertUser(CustomerDTO newUser, string passwordHash)
        {
            string query = @"SET NOCOUNT OFF;
                           INSERT INTO dbo.CustomerInfo
                               (UserName, passwords, NationalID, SerialID, Address, CustomerType)
                           VALUES
                               (@UserName, @PasswordHash, @NationalID, @SerialID, @Address, @CustomerType)";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = newUser.UserName;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 72).Value = passwordHash;
                cmd.Parameters.Add("@NationalID", SqlDbType.BigInt).Value = newUser.NationalID;
                cmd.Parameters.Add("@SerialID", SqlDbType.Int).Value = newUser.SerialID;
                cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 100).Value = newUser.Address;

                if (newUser.CustomerType.HasValue)
                {
                    cmd.Parameters.Add("@CustomerType", SqlDbType.Int).Value = newUser.CustomerType.Value;
                }
                else
                {
                    cmd.Parameters.Add("@CustomerType", SqlDbType.Int).Value = DBNull.Value;
                }

                try
                {
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected == 1;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error inserting user ({newUser.UserName}): {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error inserting user: {ex.ToString()}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error inserting user ({newUser.UserName}): {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"General Error inserting user: {ex.ToString()}");
                    return false;
                }
            }
        }

        public CustomerDTO GetCustomerBySerialId(int serialId)
        {
            CustomerDTO customer = null;
            string query = "SELECT SerialID, UserName, NationalID, Address, CustomerType FROM dbo.CustomerInfo WHERE SerialID = @SerialID";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.Add("@SerialID", SqlDbType.Int).Value = serialId;
                try
                {
                    cnn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // Nếu tìm thấy khách hàng
                        {
                            customer = new CustomerDTO
                            {
                                SerialID = Convert.ToInt32(reader["SerialID"]),
                                UserName = reader["UserName"].ToString(),
                                NationalID = Convert.ToInt32(reader["NationalID"]),
                                Address = reader["Address"].ToString(),
                                CustomerType = reader["CustomerType"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CustomerType"])
                            };
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error getting customer by SerialID {serialId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error getting customer by SerialID: {ex.ToString()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error getting customer by SerialID {serialId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"General Error getting customer by SerialID: {ex.ToString()}");
                }
            }
            return customer;
        }

        public CustomerDTO GetCustomerByUsername(string username)
        {
            CustomerDTO customer = null;
            string query = "SELECT SerialID, UserName, NationalID, Address FROM CustomerInfo WHERE UserName = @Username";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                try
                {
                    cnn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new CustomerDTO
                            {
                                SerialID = Convert.ToInt32(reader["SerialID"]),
                                UserName = reader["UserName"].ToString(),
                                NationalID = Convert.ToInt32(reader["NationalID"]),
                                Address = reader["Address"].ToString()
                            };
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error getting customer: " + ex.Message);
                }
            }
            return customer;
        }

        public bool UpdatePasswordHash(string username, string newHash)
        {
            string query = @"SET NOCOUNT OFF;
                           UPDATE dbo.CustomerInfo
                           SET passwords = @PasswordHash
                           WHERE UserName = @Username";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                // Thêm parameters rõ ràng để tránh SQL Injection và lỗi kiểu dữ liệu
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 72).Value = newHash;
                cmd.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                try
                {
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // Nếu username không tồn tại, rowsAffected sẽ là 0
                    if (rowsAffected == 1)
                    {
                        System.Diagnostics.Debug.WriteLine($"Password hash updated successfully for user '{username}'.");
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Password hash update failed for user '{username}'. Rows affected: {rowsAffected}. User might not exist.");
                        return false;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error updating password hash for {username}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error updating password hash for {username}: {ex.ToString()}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error updating password hash for {username}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"General Error updating password hash for {username}: {ex.ToString()}");
                    return false;
                }
            }
        }

        public bool UpdateCustomerType(int serialId, int? newCustomerType) 
        {
            string query = @"SET NOCOUNT OFF;
                           UPDATE dbo.CustomerInfo
                           SET CustomerType = @CustomerType
                           WHERE SerialID = @SerialID";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                // Xử lý giá trị NULL cho parameter
                if (newCustomerType.HasValue)
                {
                    cmd.Parameters.Add("@CustomerType", SqlDbType.Int).Value = newCustomerType.Value;
                }
                else
                {
                    cmd.Parameters.Add("@CustomerType", SqlDbType.Int).Value = DBNull.Value; // Gán DBNull nếu là null
                }
                cmd.Parameters.Add("@SerialID", SqlDbType.Int).Value = serialId;

                try
                {
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected == 1; // Thành công nếu đúng 1 dòng được cập nhật
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error updating CustomerType for {serialId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error updating CustomerType: {ex.ToString()}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error updating CustomerType for {serialId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"General Error updating CustomerType: {ex.ToString()}");
                    return false;
                }
            }
        }

        public bool UpdatePasswordHashBySerialId(int serialId, string newHash)
        {
            string query = @"SET NOCOUNT OFF;
                           UPDATE dbo.CustomerInfo
                           SET passwords = @PasswordHash
                           WHERE SerialID = @SerialID";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                // Thêm parameters rõ ràng
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 72).Value = newHash;
                cmd.Parameters.Add("@SerialID", SqlDbType.Int).Value = serialId;

                try
                {
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected == 1;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error updating password hash for SerialID {serialId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error updating password hash for SerialID {serialId}: {ex.ToString()}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error updating password hash for SerialID {serialId}: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"General Error updating password hash for SerialID {serialId}: {ex.ToString()}");
                    return false;
                }
            }
        }
    }
}
