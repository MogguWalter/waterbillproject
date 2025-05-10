using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace WaterBillManagementSystem.DAL
{
    public static class DatabaseHelper
    {
        private static string _cachedConnectionString = null;
        private static bool _isConnectionStringInitialized = false;

        public static string ConnectionString
        {
            get
            {
                if (!_isConnectionStringInitialized)
                {
                    try
                    {
                        ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["WaterBillDb"];
                        if (settings != null)
                        {
                            _cachedConnectionString = settings.ConnectionString;
                            System.Diagnostics.Debug.WriteLine($"DEBUG (DatabaseHelper.ConnectionString GET): Successfully read 'WaterBillDb'. Value: '{_cachedConnectionString}'");
                        }
                        else
                        {
                            _cachedConnectionString = null;
                            System.Diagnostics.Debug.WriteLine("DEBUG (DatabaseHelper.ConnectionString GET): ConnectionString with name 'WaterBillDb' NOT FOUND.");
                        }
                    }
                    catch (ConfigurationErrorsException configEx)
                    {
                        _cachedConnectionString = null;
                        System.Diagnostics.Debug.WriteLine($"DEBUG (DatabaseHelper.ConnectionString GET): CONFIGURATION ERROR reading ConnectionStrings: {configEx.ToString()}");
                    }
                    catch (Exception ex)
                    {
                        _cachedConnectionString = null;
                        System.Diagnostics.Debug.WriteLine($"DEBUG (DatabaseHelper.ConnectionString GET): GENERAL ERROR reading ConnectionStrings: {ex.ToString()}");
                    }
                    _isConnectionStringInitialized = true;
                }
                return _cachedConnectionString;
            }
        }

        public static SqlConnection GetConnection()
        {
            string currentConnectionString = ConnectionString;

            System.Diagnostics.Debug.WriteLine($"DEBUG (DatabaseHelper.GetConnection): Attempting to use ConnectionString: '{currentConnectionString}'");

            if (string.IsNullOrEmpty(currentConnectionString))
            {
                System.Diagnostics.Debug.WriteLine("FATAL ERROR (DatabaseHelper.GetConnection): Final ConnectionString is NULL or EMPTY before creating SqlConnection!");
                throw new InvalidOperationException("DatabaseHelper: The ConnectionString property evaluated to null or empty after attempting to read from configuration. Check App.config, the key 'WaterBillDb', and any configuration errors in the Output window.");
            }
            return new SqlConnection(currentConnectionString);
        }
    }
}

