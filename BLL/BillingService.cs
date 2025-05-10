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
    public class BillingService
    {
        private readonly ConsumptionDataAccess _consumptionDataAccess;
        private readonly UserDataAccess _userDataAccess;

        // Bậc 1: 0 - 10 m3
        private const decimal ThresholdTier1 = 10m;
        private const decimal PriceTier1_Policy = 5973m;  // Giá cho hộ chính sách/nghèo/cận nghèo
        private const decimal PriceTier1_Other = 8500m;   // Giá cho hộ dân cư khác

        // Bậc 2: >10 - 20 m3
        private const decimal ThresholdTier2 = 20m;
        private const decimal PriceTier2 = 9900m;

        // --- THÊM BẬC 3 ---
        // Ngưỡng cho bậc 3 (ví dụ: từ trên 20m3 đến 30m3)
        private const decimal ThresholdTier3 = 30m;
        private const decimal PriceTier3 = 16000m;

        // --- THÊM BẬC 4 (Bậc cao nhất) ---
        // Đơn giá cho tất cả lượng nước tiêu thụ trên ngưỡng bậc 3
        private const decimal PriceTier4 = 22000m; 


        public BillingService()
        {
            _consumptionDataAccess = new ConsumptionDataAccess();
            _userDataAccess = new UserDataAccess();
        }

        // CalculateSegmentNumber đã có
        public int CalculateSegmentNumber(decimal consumptionAmount)
        {
            if (consumptionAmount <= 20.00m && consumptionAmount >= 0.00m) return 1;
            return 6;
        }

        public decimal CalculateProgressiveBillPrice(decimal consumptionAmount, int customerType) // Giả sử 1=Policy, khác 1=Other
        {
            decimal totalCost = 0m;
            decimal remainingAmount = consumptionAmount;

            // --- Bậc 1 (0 đến ThresholdTier1) ---
            if (remainingAmount > 0)
            {
                decimal tier1AmountToCalculate = Math.Min(remainingAmount, ThresholdTier1);
                decimal tier1PriceRate = (customerType == 1) ? PriceTier1_Policy : PriceTier1_Other;
                totalCost += tier1AmountToCalculate * tier1PriceRate;
                remainingAmount -= tier1AmountToCalculate;
            }

            // --- Bậc 2 (Từ trên ThresholdTier1 đến ThresholdTier2) ---
            if (remainingAmount > 0)
            {
                // Lượng nước trong bậc 2 là phần chênh lệch giữa ThresholdTier2 và ThresholdTier1
                decimal tier2Capacity = ThresholdTier2 - ThresholdTier1;
                decimal tier2AmountToCalculate = Math.Min(remainingAmount, tier2Capacity);
                totalCost += tier2AmountToCalculate * PriceTier2;
                remainingAmount -= tier2AmountToCalculate;
            }

            // --- Bậc 3 (Từ trên ThresholdTier2 đến ThresholdTier3) ---
            if (remainingAmount > 0)
            {
                // Lượng nước trong bậc 3 là phần chênh lệch giữa ThresholdTier3 và ThresholdTier2
                decimal tier3Capacity = ThresholdTier3 - ThresholdTier2;
                decimal tier3AmountToCalculate = Math.Min(remainingAmount, tier3Capacity);
                totalCost += tier3AmountToCalculate * PriceTier3; // Sử dụng đơn giá bậc 3
                remainingAmount -= tier3AmountToCalculate;
            }

            // --- Bậc 4 (Phần còn lại, trên ThresholdTier3) ---
            if (remainingAmount > 0)
            {
                // Toàn bộ lượng nước còn lại sẽ được tính theo đơn giá bậc 4
                totalCost += remainingAmount * PriceTier4; // Sử dụng đơn giá bậc 4
            }

            return totalCost;
        }

        // AddConsumption đã có
        public bool AddConsumption(int serialId, DateTime month, decimal consumptionAmount)
        {
            CustomerDTO customer = _userDataAccess.GetCustomerBySerialId(serialId);
            if (customer == null)
            {
                Console.WriteLine($"AddConsumption Error: Customer with SerialID {serialId} not found.");
                System.Diagnostics.Debug.WriteLine($"AddConsumption BLL: Customer with SerialID {serialId} not found.");
                return false;
            }
            // Mặc định là "Other" (0) nếu CustomerType là null trong DB
            int customerType = customer.CustomerType ?? 0;

            int segmentNumber = CalculateSegmentNumberBasedOnTiers(consumptionAmount);

            ConsumptionDTO basicRecord = new ConsumptionDTO
            {
                SerialID = serialId,
                Month = month,
                ConsumptionAmount = consumptionAmount,
                SegmentNumber = segmentNumber
            };

            bool insertSuccess = _consumptionDataAccess.InsertConsumptionRecord(basicRecord);
            if (!insertSuccess)
            {
                Console.WriteLine($"AddConsumption Error: Failed to insert basic consumption record for {serialId}/{month:yyyy-MM-dd}.");
                System.Diagnostics.Debug.WriteLine($"AddConsumption BLL: InsertConsumptionRecord failed for {serialId}/{month:yyyy-MM-dd}.");
                return false;
            }

            decimal calculatedPrice = CalculateProgressiveBillPrice(consumptionAmount, customerType);
            System.Diagnostics.Debug.WriteLine($"AddConsumption BLL: Calculated Price for {serialId}/{month:yyyy-MM-dd} with amount {consumptionAmount} and type {customerType} is {calculatedPrice}");

            bool updateSuccess = _consumptionDataAccess.UpdateConsumptionPrice(serialId, month, calculatedPrice);
            if (!updateSuccess)
            {
                Console.WriteLine($"AddConsumption Warning: Failed to update calculated price for {serialId}/{month:yyyy-MM-dd}. Basic record inserted but price is missing/wrong.");
                System.Diagnostics.Debug.WriteLine($"AddConsumption BLL: UpdateConsumptionPrice failed for {serialId}/{month:yyyy-MM-dd}.");
            }

            return true;
        }


        public DataTable GetAllConsumption()
        {
            return _consumptionDataAccess.GetAllConsumptionRecords();
        }

        public bool DeleteConsumptionRecord(int serialId, DateTime month)
        {
            return _consumptionDataAccess.DeleteConsumption(serialId, month);
        }

        // Lấy chi tiết hóa đơn cho user (trả về DataTable cho dễ hiển thị)
        public DataTable GetBillDetailsForUser(int serialId)
        {
            return _consumptionDataAccess.GetConsumptionDetailsForUser(serialId);
        }
        public int CalculateSegmentNumberBasedOnTiers(decimal consumptionAmount)
        {
            if (consumptionAmount < 0) consumptionAmount = 0; // Đảm bảo không âm

            if (consumptionAmount <= ThresholdTier1) // 0 - 10 m3
            {
                return 1;
            }
            else if (consumptionAmount <= ThresholdTier2) // >10 - 20 m3
            {
                return 2;
            }
            else if (consumptionAmount <= ThresholdTier3) // >20 - 30 m3 (theo ví dụ ngưỡng)
            {
                return 3;
            }
            else // >30 m3
            {
                return 4;
            }
        }

        public DataTable GetConsumptionBySerialId(int serialId)
        {
            if (serialId <= 0) return new DataTable();
            return _consumptionDataAccess.GetConsumptionRecordsBySerialId(serialId);
        }
    }
}
