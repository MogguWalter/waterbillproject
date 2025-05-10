using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using WaterBillManagementSystem.DAL;
using WaterBillManagementSystem.Entities;

namespace WaterBillManagementSystem.BLL
{
    public class DebtService
    {
        private readonly DebtDataAccess _debtDataAccess;

        public DebtService()
        {
            _debtDataAccess = new DebtDataAccess();
        }

        public bool AddDebt(int serialId, DateTime month)
        {
            DebtDTO newDebt = new DebtDTO { SerialID = serialId, Month = month };
            return _debtDataAccess.InsertDebt(newDebt);
        }

        // Lấy danh sách nợ
        public DataTable GetAllDebts()
        {
            return _debtDataAccess.GetAllDebtsRaw();
        }

        // Lấy nợ + giá cho user (trả về DataTable cho dễ hiển thị)
        public DataTable GetUserDebtsWithPrice(int serialId)
        {
            return _debtDataAccess.GetDebtsWithPriceForUser(serialId);
        }


        public bool DeleteDebt(int serialId, DateTime month)
        {
            return _debtDataAccess.DeleteDebt(serialId, month);
        }

        public DataTable GetDebtsBySerialId(int serialId)
        {
            if (serialId <= 0)
            {
                return new DataTable();
            }
            return _debtDataAccess.GetDebtsBySerialId(serialId);
        }
    }
}
