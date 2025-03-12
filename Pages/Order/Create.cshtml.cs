using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace Profescipta_test.Pages.Order
{
    public class createModel : PageModel
    {
        public OrderInfo orderInfo { get; set; } = new OrderInfo(); // Ensure it's always initialized
        public string errorMessage { get; set; } = "";
        public string successMessage { get; set; } = "";

        public void OnGet()
        {
            // Ensure orderInfo is not null
            if (orderInfo == null)
            {
                orderInfo = new OrderInfo();
            }
        }

        public void OnPost()
        {
            System.Diagnostics.Debug.WriteLine($"ORDER_NO: {orderInfo.ORDER_NO}, ORDER_DATE: {orderInfo.ORDER_DATE}, CUSTOMER_ID: {orderInfo.COM_CUSTOMER_ID}, ADDRESS: {orderInfo.ADDRESS}");
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
                using (SqlConnection connection  = new SqlConnection(connectionString)) 
                {
                    connection.Open();
                    String sql = "INSERT INTO SO_ORDER" +
                        "(ORDER_NO, ORDER_DATE, COM_CUSTOMER_ID, ADDRESS) VALUES" +
                        "(@ORDER_NO, @ORDER_DATE, @COM_CUSTOMER_ID, @ADDRESS);";

                    using (SqlCommand command = new SqlCommand(sql, connection)) 
                    {
                    command.Parameters.AddWithValue("@ORDER_NO", orderInfo.ORDER_NO);
                        command.Parameters.AddWithValue("@ORDER_DATE", orderInfo.ORDER_DATE);
                        command.Parameters.AddWithValue("@COM_CUSTOMER_ID", orderInfo.COM_CUSTOMER_ID);
                        command.Parameters.AddWithValue("@ADDRESS", orderInfo.ADDRESS);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) 
            { 
                errorMessage = ex.Message;
                return;
            }

            orderInfo = new OrderInfo();
            successMessage = "New Order Added Successfully";

            Response.Redirect("/Order/Index");
        }
    }

  
}
