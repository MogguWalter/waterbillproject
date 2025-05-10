using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaterBillManagementSystem.BLL
{
    public static class PasswordHasher
    {
        // Hash mật khẩu bằng BCrypt
        public static string HashPassword(string password)
        {
            // WorkFactor (độ phức tạp) có thể điều chỉnh, 11-12 là phổ biến
            return BCrypt.Net.BCrypt.HashPassword(password, 11);
        }

        // Kiểm tra mật khẩu nhập vào với hash đã lưu
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            try
            {
                // Hàm Verify của BCrypt sẽ tự so sánh
                return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                // Lỗi nếu storedHash không phải là định dạng BCrypt hợp lệ
                Console.WriteLine("Error verifying password (invalid hash format?): " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("General error verifying password: " + ex.Message);
                return false;
            }
        }
    }
}
