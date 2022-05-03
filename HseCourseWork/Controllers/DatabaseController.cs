using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class DatabaseController : Controller
{
    private readonly IConfiguration _config;
    private List<SelectListItem> _itemsList;

    public DatabaseController(IConfiguration config)
    {
        _config = config;
        _itemsList = new List<SelectListItem>();
    }

    public IActionResult PeriodicTable()
    {
        var data = GetUniqueDataTable();
        SetListItems(data);
        return View("~/Views/Home/PeriodicTable.cshtml", _itemsList);
    }

    private bool CheckContains(StringValues elems, SelectListItem item)
    {
        bool flag;
        flag = true;
        foreach (var elem in elems)
        {
            if (!item.Value.Contains(elem))
            {
                flag = false;
                break;
            }

            string s = item.Text;
            int index = s.IndexOf(elem);
            while (index + elem.Length < item.Text.Length)
            {
                if (s[index + elem.Length].ToString().ToUpper() != s[index + elem.Length].ToString())
                {
                    flag = false;
                    break;
                }

                if (s.Substring(index + elem.Length).Contains(elem))
                {
                    index = s.IndexOf(elem, index + elem.Length);
                }
                else
                {
                    break;
                }
            }
        }

        return flag;
    }
    
    public IActionResult Get()
    {
        DataTable uniqueData = GetUniqueDataTable();
        SetListItems(uniqueData);

        var elems = Request.Form["elem"];
        if (elems.Count == 0)
        {
            return  View("~/Views/Home/ErrorView.cshtml","No elements selected!");
        }

        List<SelectListItem> list = new List<SelectListItem>();
        foreach (var item in _itemsList)
        {
            if (CheckContains(elems, item))
            {
                list.Add(item);
            }
        }

        if (list.Count == 0)
        {
            return View("~/Views/Home/ErrorView.cshtml","No such substances!");
        }

        return View("~/Views/Home/Get.cshtml", list);
    }

    public IActionResult ErrorView(string error)
    {
        return View("~/Views/Home/ErrorView.cshtml", error);
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

    private void SetListItems(DataTable dataTable)
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

            SelectListItem item = new SelectListItem(compound, compound);
            if (list.All(i => i.Text != item.Text))
            {
                list.Add(item);
            }
        }
        _itemsList = list;
    }

    private DataTable RemoveColumns(DataTable dataTable)
    {
        for (int col = dataTable.Columns.Count - 1; col >= 0; col--)
        {
            bool removeColumn = true;
            foreach (DataRow row in dataTable.Rows)
            {
                if (!row.IsNull(col) && row[col].ToString()!="")
                {
                    removeColumn = false;
                    break;
                }
            }

            if (removeColumn) dataTable.Columns.RemoveAt(col);
        }

        return dataTable;
    }

    public IActionResult ShowData()
    {
        string compound = Request.Form["compound"].ToString();
        DataTable errorDataTable = new DataTable();
        errorDataTable.Columns.Add("Error");

        string query = _config.GetSection("CompQuery").Value + "'" + compound + "' " +
                       "OR table2.Соединение = " + "'" + compound + "'";
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
                    uniqueData = RemoveColumns(uniqueData);
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