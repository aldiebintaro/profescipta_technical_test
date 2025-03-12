using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Profescipta_test.Pages.Item;
using System.Data.SqlClient;

namespace Profescipta_test.Pages.Order
{
    public class editModel : PageModel
    {
        public List<ItemInfo> listItem = new List<ItemInfo>();
        public List<FormData> listTest { get; set; } = new List<FormData>();
        public OrderInfo orderInfo { get; set; } = new OrderInfo();
        public ItemInfo itemInfo { get; set; } = new ItemInfo();
        public decimal TotalQuantity { get; set; }
        public decimal GrandTotal { get; set; }
        public string errorMessage { get; set; } = "";
        public string successMessage { get; set; } = "";

        public void CalculateTotals()
        {
            TotalQuantity = listTest.Sum(item => decimal.TryParse(item.tempQuantity, out var q) ? q : 0);
            GrandTotal = listTest.Sum(item => item.tempTotal);
        }


        public void OnGet()
        {
            String SO_ORDER_ID = Request.Query["SO_ORDER_ID"];
            try
            {
                String connectionString = "Data Source=ALDI;Initial Catalog=profescipta_test;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT SO_ITEM_ID, SO_ORDER_ID, ITEM_NAME, QUANTITY, PRICE FROM SO_ITEM WHERE SO_ORDER_ID=@SO_ORDER_ID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@SO_ORDER_ID", SO_ORDER_ID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ItemInfo info = new ItemInfo
                                {
                                    SO_ITEM_ID = reader.GetInt64(0).ToString(),
                                    SO_ORDER_ID = reader.GetInt64(1).ToString(),
                                    ITEM_NAME = reader.GetString(2),
                                    QUANTITY = reader.GetInt32(3).ToString(),
                                    PRICE = reader.GetDouble(4).ToString()
                                };

                                int quantity;
                                double price;
                                if (int.TryParse(info.QUANTITY, out quantity) && double.TryParse(info.PRICE, out price))
                                {
                                    info.TOTAL = (quantity * price).ToString("F2"); // Ensure proper decimal formatting
                                }

                                listItem.Add(info);
                                listTest.Add(new FormData
                                {
                                    tempQuantity = info.QUANTITY,
                                    tempTotal = decimal.Parse(info.TOTAL)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

           
            CalculateTotals();
        

            try
            {
                String connectionString = "Data Source=ALDI;Initial Catalog=profescipta_test;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT * FROM SO_ITEM WHERE SO_ORDER_ID=@SO_ORDER_ID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@SO_ORDER_ID", SO_ORDER_ID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                itemInfo.SO_ITEM_ID = reader.GetInt64(0).ToString();
                                itemInfo.SO_ORDER_ID = reader.GetInt64(1).ToString();
                                itemInfo.ITEM_NAME = reader.GetString(2);
                                itemInfo.QUANTITY = reader.GetInt32(3).ToString();
                                itemInfo.PRICE = reader.GetDouble(4).ToString();
                                int quantity = int.Parse(itemInfo.QUANTITY);
                                int price = int.Parse(itemInfo.PRICE);
                                itemInfo.TOTAL = (quantity * price).ToString();          
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
        public void OnPost()
        {
            orderInfo.SO_ORDER_ID = Request.Query["SO_ORDER_ID"];
            orderInfo.ORDER_NO = Request.Form["salesOrder"];
            orderInfo.ORDER_DATE = Request.Form["orderDate"];
            orderInfo.COM_CUSTOMER_ID = Request.Form["Customer"];
            orderInfo.ADDRESS = Request.Form["Address"];

            if (string.IsNullOrEmpty(orderInfo.ORDER_NO) || string.IsNullOrEmpty(orderInfo.ORDER_DATE) ||
            string.IsNullOrEmpty(orderInfo.COM_CUSTOMER_ID) || string.IsNullOrEmpty(orderInfo.ADDRESS))
            {
                errorMessage = "All fields are required";
                return;
            }

            try
            {

                String connectionString = "Data Source=ALDI;Initial Catalog=profescipta_test;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE SO_ORDER " + 
                                "SET ORDER_NO=@ORDER_NO, ORDER_DATE=@ORDER_DATE, COM_CUSTOMER_ID=@COM_CUSTOMER_ID, ADDRESS=@ADDRESS " +
                                "WHERE SO_ORDER_ID=@SO_ORDER_ID";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ORDER_NO", orderInfo.ORDER_NO);
                        command.Parameters.AddWithValue("@ORDER_DATE", orderInfo.ORDER_DATE);
                        command.Parameters.AddWithValue("@COM_CUSTOMER_ID", orderInfo.COM_CUSTOMER_ID);
                        command.Parameters.AddWithValue("@ADDRESS", orderInfo.ADDRESS);
                        command.Parameters.AddWithValue("@SO_ORDER_ID", orderInfo.SO_ORDER_ID);

                        command.ExecuteNonQuery();
                    }
                }


            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Response.Redirect("/Order/Index");
        }
    }

        
}
