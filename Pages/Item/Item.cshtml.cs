using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Profescipta_test.Pages.Order;

namespace Profescipta_test.Pages.Item
{
    public class ItemModel : PageModel
    {
        public List<FormData> listTest { get; set; } = new List<FormData>();
        public OrderInfo orderInfo { get; set; } = new OrderInfo();
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
            var sessionData = HttpContext.Session.GetString("listTest");
            if (!string.IsNullOrEmpty(sessionData))
            {
                listTest = JsonConvert.DeserializeObject<List<FormData>>(sessionData);
            }
            CalculateTotals();
        }

    

        public IActionResult OnPostAdd(FormData formData)
        {
            if (ModelState.IsValid)
            {
             
                var sessionData = HttpContext.Session.GetString("listTest");
                listTest = string.IsNullOrEmpty(sessionData) ? new List<FormData>()
                                                             : JsonConvert.DeserializeObject<List<FormData>>(sessionData);

              
                formData.Id = listTest.Count + 1;

                decimal quantity = decimal.TryParse(formData.tempQuantity, out var q) ? q : 0;
                decimal price = decimal.TryParse(formData.tempPrice, out var p) ? p : 0;

                formData.tempTotal = quantity * price;

                listTest.Add(formData);


                HttpContext.Session.SetString("listTest", JsonConvert.SerializeObject(listTest));
                CalculateTotals();
            }

            return RedirectToPage();
        }

        public IActionResult OnPostSaveToDatabase()
        {
           
            orderInfo.ORDER_NO = Request.Form["salesOrder"];
            orderInfo.ORDER_DATE = Request.Form["orderDate"];
            orderInfo.COM_CUSTOMER_ID = Request.Form["Customer"];
            orderInfo.ADDRESS = Request.Form["Address"];


         
            if (string.IsNullOrEmpty(orderInfo.ORDER_NO) || string.IsNullOrEmpty(orderInfo.ORDER_DATE) ||
        string.IsNullOrEmpty(orderInfo.COM_CUSTOMER_ID) || string.IsNullOrEmpty(orderInfo.ADDRESS))
            {
                errorMessage = "All fields are required" + orderInfo.ORDER_NO + orderInfo.ORDER_DATE + orderInfo.COM_CUSTOMER_ID + orderInfo.ADDRESS;
                return Page();
            }
          
            var sessionData = HttpContext.Session.GetString("listTest");
            if (string.IsNullOrEmpty(sessionData))
            {
                errorMessage = "No items to save";
                return Page();
            }

            listTest = JsonConvert.DeserializeObject<List<FormData>>(sessionData);

           
            string connectionString = "Data Source=ALDI;Initial Catalog=profescipta_test;Integrated Security=True";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    
                    long newOrderId;
                    string insertOrderQuery = @"
            INSERT INTO SO_ORDER (ORDER_NO, ORDER_DATE, COM_CUSTOMER_ID, ADDRESS)
            OUTPUT INSERTED.SO_ORDER_ID
            VALUES (@ORDER_NO, @ORDER_DATE, @COM_CUSTOMER_ID, @ADDRESS);";

                    using (SqlCommand orderCommand = new SqlCommand(insertOrderQuery, connection))
                    {
                        orderCommand.Parameters.AddWithValue("@ORDER_NO", orderInfo.ORDER_NO);
                        orderCommand.Parameters.AddWithValue("@ORDER_DATE", orderInfo.ORDER_DATE);
                        orderCommand.Parameters.AddWithValue("@COM_CUSTOMER_ID", orderInfo.COM_CUSTOMER_ID);
                        orderCommand.Parameters.AddWithValue("@ADDRESS", orderInfo.ADDRESS);
                        newOrderId = Convert.ToInt64(orderCommand.ExecuteScalar());
                    }

                    // Insert order items into database.
                    foreach (var item in listTest)
                    {
                        string insertItemQuery = @"
                INSERT INTO SO_ITEM (SO_ORDER_ID, ITEM_NAME, QUANTITY, PRICE)
                VALUES (@ORDER_ID, @ITEM_NAME, @QUANTITY, @PRICE);";

                        using (SqlCommand itemCommand = new SqlCommand(insertItemQuery, connection))
                        {
                            itemCommand.Parameters.AddWithValue("@ORDER_ID", newOrderId);
                            itemCommand.Parameters.AddWithValue("@ITEM_NAME", item.tempName);
                            itemCommand.Parameters.AddWithValue("@QUANTITY", item.tempQuantity);
                            itemCommand.Parameters.AddWithValue("@PRICE", item.tempPrice);
                            
                            itemCommand.ExecuteNonQuery();
                        }
                    }
                }

            
                HttpContext.Session.Remove("listTest");
                orderInfo = new OrderInfo();
                successMessage = "New Order Added Successfully";
                return RedirectToPage("/Order/Index");
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return Page();
            }
        }
    }

    public class FormData
    {
        public int Id { get; set; }
        public string tempName { get; set; }
        public string tempQuantity { get; set; }
        public string tempPrice { get; set; }
        public decimal tempTotal { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal GrandTotal { get; set; }
    }
}




