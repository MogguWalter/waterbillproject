using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterBillManagementSystem.BLL;
using WaterBillManagementSystem.DAL;

namespace WaterBillManagementSystem.BLL
{
    public class AdminService
    {
        private readonly AdminDataAccess _adminDataAccess;

        public AdminService()
        {
            _adminDataAccess = new AdminDataAccess();
        }

        public bool AuthenticateAdmin(string enteredPassword)
        {
            if (string.IsNullOrEmpty(enteredPassword)) return false;
            string storedHash = _adminDataAccess.GetStoredAdminPasswordHash();
            if (storedHash == null) return false;
            return PasswordHasher.VerifyPassword(enteredPassword, storedHash); // Dùng verify của BCrypt
        }
        
    }
}
