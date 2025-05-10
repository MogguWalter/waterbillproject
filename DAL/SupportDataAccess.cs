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
    public class SupportDataAccess
    {
        public bool InsertSupportTicket(SupportTicketDTO ticket)
        {
            string query = "INSERT INTO dbo.techsupport (description) VALUES (@Description)";

            using (SqlConnection cnn = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, cnn))
            {
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 900).Value = ticket.Description;

                try
                {
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected == 1;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error inserting support ticket: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"SQL Error inserting support ticket: {ex.ToString()}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Error inserting support ticket: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"General Error inserting support ticket: {ex.ToString()}");
                    return false;
                }
            }
        }
    }
}
