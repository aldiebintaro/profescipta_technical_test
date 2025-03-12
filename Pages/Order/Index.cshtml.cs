using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.IO;
using System.Diagnostics.Metrics;


namespace Profescipta_test.Pages.Order
{
    public class IndexModel : PageModel
    {

        public List<OrderInfo> listOrders = new List<OrderInfo>();
        public string SearchKeyword { get; set; }
        public string SearchDate { get; set; }


        public IActionResult OnPostExport()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("No", typeof(string));
            dt.Columns.Add("Sales Order", typeof(string));
            dt.Columns.Add("Order Date", typeof(string));
            dt.Columns.Add("Customer", typeof(string));

            try
            {
                string connectionString = "Data Source=ALDI;Initial Catalog=profescipta_test;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT SO_ORDER_ID, SO.ORDER_NO, SO.ORDER_DATE, CC.CUSTOMER_NAME " +
                                 "FROM SO_ORDER SO JOIN COM_CUSTOMER CC ON SO.COM_CUSTOMER_ID = CC.COM_CUSTOMER_ID " +
                                 "WHERE (SO.ORDER_NO LIKE @Search OR CC.CUSTOMER_NAME LIKE @Search) ";

                    if (!string.IsNullOrEmpty(SearchDate))
                    {
                        sql += "AND CONVERT(date, SO.ORDER_DATE) = @SearchDate ";
                    }

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Search", "%" + (SearchKeyword ?? "") + "%");

                        if (!string.IsNullOrEmpty(SearchDate))
                        {
                            command.Parameters.AddWithValue("@SearchDate", SearchDate);
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dt.Rows.Add(
                                    reader["SO_ORDER_ID"].ToString(),
                                    reader["ORDER_NO"].ToString(),
                                    Convert.ToDateTime(reader["ORDER_DATE"]).ToString("yyyy-MM-dd"),
                                    reader["CUSTOMER_NAME"].ToString()
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error exporting data: " + ex.Message);
                return Page();
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt, "Orders");

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders.xlsx");
                }
            }
        }


        public IActionResult OnPostDelete(string SO_ORDER_ID)
        {
            if (string.IsNullOrEmpty(SO_ORDER_ID))
            {
                return Page();
            }

            try
            {
                string connectionString = "Data Source=ALDI;Initial Catalog=profescipta_test;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    
                    string deleteItemsQuery = "DELETE FROM SO_ITEM WHERE SO_ORDER_ID=@SO_ORDER_ID";
                    using (SqlCommand deleteItemsCmd = new SqlCommand(deleteItemsQuery, connection))
                    {
                        deleteItemsCmd.Parameters.AddWithValue("@SO_ORDER_ID", SO_ORDER_ID);
                        deleteItemsCmd.ExecuteNonQuery();
                    }

                    string deleteOrderQuery = "DELETE FROM SO_ORDER WHERE SO_ORDER_ID=@SO_ORDER_ID";
                    using (SqlCommand deleteOrderCmd = new SqlCommand(deleteOrderQuery, connection))
                    {
                        deleteOrderCmd.Parameters.AddWithValue("@SO_ORDER_ID", SO_ORDER_ID);
                        deleteOrderCmd.ExecuteNonQuery();
                    }
                }

                return RedirectToPage("/Order/Index"); 
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error deleting order: " + ex.Message);
                return Page();
            }
        }

        public void OnGet(string searchKeyword, string searchDate)
        {
            SearchKeyword = searchKeyword;
            SearchDate = searchDate;

            try
            {
                string connectionString = "Data Source=ALDI;Initial Catalog=profescipta_test;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT SO_ORDER_ID, SO.ORDER_NO, SO.ORDER_DATE, CC.CUSTOMER_NAME " +
                                 "FROM SO_ORDER SO JOIN COM_CUSTOMER CC ON SO.COM_CUSTOMER_ID = CC.COM_CUSTOMER_ID " +
                                 "WHERE (SO.ORDER_NO LIKE @Search OR CC.CUSTOMER_NAME LIKE @Search) ";

                    if (!string.IsNullOrEmpty(searchDate))
                    {
                        sql += "AND CONVERT(date, SO.ORDER_DATE) = @SearchDate ";
                    }

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Search", "%" + (searchKeyword ?? "") + "%");

                        if (!string.IsNullOrEmpty(searchDate))
                        {
                            command.Parameters.AddWithValue("@SearchDate", searchDate);
                        }

                        int number = 1;

                        using (SqlDataReader reader = command.ExecuteReader())

                        {
                            while (reader.Read())
                            {
                                OrderInfo info = new OrderInfo
                                {
                                    
                                    SO_ORDER_ID = number.ToString(),
                                    ORDER_NO = reader.GetString(1),
                                    ORDER_DATE = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                    CUSTOMER_NAME = reader.GetString(3)
                                };

                                listOrders.Add(info);
                                number++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
        }
    }

    public class OrderInfo
    {
        public string SO_ORDER_ID { get; set; } 
        public string COM_CUSTOMER_ID { get; set; }
        public string ORDER_NO { get; set; }
        public string ORDER_DATE { get; set; }
        public string CUSTOMER_NAME { get; set; }
        public string ADDRESS { get; set; }
    }

    public class ItemInfo
    {
        public string SO_ORDER_ID { get; set; }
        public string SO_ITEM_ID { get; set; }  
        public string ITEM_NAME { get; set; }
        public string QUANTITY { get; set; }
        public string PRICE { get; set; }
        public string TOTAL { get; set; }

    }


}
