using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterBillManagementSystem.BLL;
using WaterBillManagementSystem.DAL;
using WaterBillManagementSystem.Entities;

namespace WaterBillManagementSystem.BLL
{
    public class UserService
    {
        private readonly UserDataAccess _userDataAccess;

        public UserService()
        {
            _userDataAccess = new UserDataAccess();
        }

        public bool AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return false;
            string storedHash = _userDataAccess.GetStoredPasswordHash(username);
            if (storedHash == null) return false;
            return PasswordHasher.VerifyPassword(password, storedHash); // Dùng verify của BCrypt
        }

        public bool RegisterUser(CustomerDTO newUser, string password)
        {
            if (newUser == null ||
                string.IsNullOrWhiteSpace(newUser.UserName) ||
                string.IsNullOrWhiteSpace(password) ||
                newUser.SerialID <= 0)
            {
                System.Diagnostics.Debug.WriteLine("RegisterUser BLL Error: Invalid input data.");
                return false;
            }

            if (_userDataAccess.UserNameExists(newUser.UserName))
            {
                System.Diagnostics.Debug.WriteLine($"RegisterUser BLL Info: UserName '{newUser.UserName}' already exists.");
                return false; // Username đã tồn tại
            }

            if (_userDataAccess.SerialIdExists(newUser.SerialID))
            {
                System.Diagnostics.Debug.WriteLine($"RegisterUser BLL Info: SerialID '{newUser.SerialID}' already exists.");
                return false; // SerialID đã tồn tại
            }

            try
            {
                // Hash mật khẩu
                string passwordHash = PasswordHasher.HashPassword(password);

                // Gọi DAL để lưu user với passwordHash
                return _userDataAccess.InsertUser(newUser, passwordHash);
            }
            catch (Exception ex) // Bắt các lỗi không mong muốn khác
            {
                Console.WriteLine($"Error during user registration for '{newUser.UserName}': {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"RegisterUser BLL Exception: {ex.ToString()}");
                return false;
            }
        }

        public CustomerDTO GetUserDetails(string username)
        {
            return _userDataAccess.GetCustomerByUsername(username);
        }

        public bool ResetPassword(string username, string newPlainPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(newPlainPassword))
            {
                System.Diagnostics.Debug.WriteLine("ResetPassword Error: Username or new password is empty.");
                return false;
            }

            try
            {
                //Hash mật khẩu mới người dùng vừa nhập
                string newPasswordHash = PasswordHasher.HashPassword(newPlainPassword);

                //Gọi DAL để cập nhật hash mới vào database
                bool success = _userDataAccess.UpdatePasswordHash(username, newPasswordHash);

                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine($"ResetPassword Info: UpdatePasswordHash returned false for user '{username}'. User might not exist or DB update failed.");
                }

                return success;
            }
            catch (Exception ex) // Bắt lỗi nếu có gì đó sai trong quá trình hash hoặc gọi DAL
            {
                Console.WriteLine($"Error resetting password for user {username}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error resetting password for user {username}: {ex.ToString()}");
                return false;
            }
        }

        public bool ResetPasswordBySerialId(int serialId, string newPlainPassword)
        {
            if (serialId <= 0 || string.IsNullOrEmpty(newPlainPassword))
            {
                System.Diagnostics.Debug.WriteLine("ResetPassword Error: SerialID or new password is empty.");
                return false;
            }

            try
            {
                //Hash mật khẩu mới
                string newPasswordHash = PasswordHasher.HashPassword(newPlainPassword);

                //Gọi DAL để cập nhật hash mới vào database bằng SerialID
                return _userDataAccess.UpdatePasswordHashBySerialId(serialId, newPasswordHash); // Gọi hàm DAL mới
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password for user SerialID {serialId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error resetting password for SerialID {serialId}: {ex.ToString()}");
                return false;
            }
        }

        public CustomerDTO GetCustomerDetailsBySerialId(int serialId)
        {
            if (serialId <= 0) return null; // Validation cơ bản
            try
            {
                return _userDataAccess.GetCustomerBySerialId(serialId);
            }
            catch (Exception ex) // Bắt lỗi nếu DAL ném ra
            {
                Console.WriteLine($"Error in BLL calling GetCustomerBySerialId for {serialId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error in BLL GetCustomerDetailsBySerialId: {ex.ToString()}");
                return null;
            }
        }

        public bool UpdateCustomerType(int serialId, int? newCustomerType)
        {
            if (serialId <= 0) 
                return false;
            try
            {
                return _userDataAccess.UpdateCustomerType(serialId, newCustomerType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BLL updating CustomerType for {serialId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error in BLL UpdateCustomerType: {ex.ToString()}");
                return false;
            }
        }

    }
}
