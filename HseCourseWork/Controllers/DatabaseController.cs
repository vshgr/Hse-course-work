using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class DatabaseController : Controller
{
    private readonly IConfiguration _config;

    public DatabaseController(IConfiguration config)
    {
        _config = config;
    }

    private DataTable GetUniqueDataTable()
    {
        DataTable dataTable = new DataTable();
        DataTable uniqueData = new DataTable();
        string connectionString = _config.GetConnectionString("Connection");
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(_config.GetValue<string>("SqlQuery:Query"),
                connection);
            sqlDataAdapter.Fill(dataTable);
            var uniqueRows = dataTable.AsEnumerable().Distinct(DataRowComparer.Default);
            uniqueData = uniqueRows.CopyToDataTable();
        }

        return uniqueData;
    }

    private List<SelectListItem> GetListItems(DataTable dataTable)
    {
        List<SelectListItem> list = new List<SelectListItem>();
        foreach (DataRow row in dataTable.Rows)
        {
            string compound;
            if (!string.IsNullOrEmpty(row["Compound"].ToString()))
            {
                compound = row["Compound"].ToString();
            }
            else
            {
                compound = row["Соединение"].ToString();
            }
            list.Add(new SelectListItem
            {
                Text = compound,
                Value = compound
            });
        }

        return list;
    }
    public IActionResult Get()
    {
        DataTable uniqueData = GetUniqueDataTable();
        List<SelectListItem> list = GetListItems(uniqueData);
        ViewBag.Compounds = list;
        return View("~/Views/Home/Get.cshtml");
    }
    
    public IActionResult ShowData()
    {
        string compound = Request.Form["compound"].ToString();
        DataTable errorDataTable = new DataTable();
        errorDataTable.Columns.Add("Error");
        if (compound == "")
        {
            DataRow row = errorDataTable.NewRow();
            row["Error"] = "Empty request. Please select compound";
            errorDataTable.Rows.Add(row);
            return View("~/Views/Home/ShowData.cshtml", errorDataTable);
        }

        string query = _config.GetSection("CompQuery").Value + "'" + compound + "'";
        string connectionString = _config.GetConnectionString("Connection");
        DataTable dataTable = new DataTable();
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, connection);
                sqlDataAdapter.Fill(dataTable);
                if (dataTable.Rows.Count > 0)
                {
                    var uniqueRows = dataTable.AsEnumerable().Distinct(DataRowComparer.Default);
                    DataTable uniqueData = uniqueRows.CopyToDataTable();
                    return View("~/Views/Home/ShowData.cshtml", uniqueData);
                }

                return View("~/Views/Home/ShowData.cshtml", dataTable);
            }
        }
        catch (Exception ex)
        {
            DataRow row = errorDataTable.NewRow();
            row["Error"] = ex.Message;
            errorDataTable.Rows.Add(row);
            return View("~/Views/Home/ShowData.cshtml", errorDataTable);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}