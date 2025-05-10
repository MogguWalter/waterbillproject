using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterBillManagementSystem.DAL;
using WaterBillManagementSystem.Entities;

namespace WaterBillManagementSystem.BLL
{
    public class SupportService
    {
        private readonly SupportDataAccess _supportDataAccess;

        public SupportService()
        {
            _supportDataAccess = new SupportDataAccess();
        }

        public bool SubmitTicket(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return false;
            }
            SupportTicketDTO ticket = new SupportTicketDTO { Description = description };
            return _supportDataAccess.InsertSupportTicket(ticket);
        }
    }
}
