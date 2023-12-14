using Microsoft.AspNetCore.Mvc;
using System.Data;
using IMS.Models;
using IMS.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace IMS.Controllers;
[Actions]
[ExceptionFilter]
public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;

    private CookieOptions policycookie;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IFormatProvider? Culture;
    
    public EmployeeController(ApplicationDbContext context, ILogger<HomeController> logger, IWebHostEnvironment hostEnvironment)
    {
        policycookie = new CookieOptions();
        policycookie.Expires = DateTime.Now.AddSeconds(30);
        _context = context;
        _hostEnvironment = hostEnvironment;

    }

    EmployeeFile employee = new EmployeeFile();
    public IActionResult AddPolicy()
    {
        if ((HttpContext.Session.GetString("User2")) != null)
        {
            string? user = HttpContext.Session.GetString("empid");
            TempData["VisitTime"] = Request.Cookies[user];
            Response.Cookies.Append(user, Convert.ToString(DateTime.Now), policycookie);
            return View();
        }
        else
            return RedirectToAction("Login", "Home");
    }

    public IActionResult HealthRegister()
    {
        if ((HttpContext.Session.GetString("User2")) != null)
        {
            TempData["user"] = HttpContext.Session.GetString("User2");
            ViewBag.id = HttpContext.Session.GetString("empid");
            return View();
        }
        else return RedirectToAction("Login", "Home");
    }
    [HttpPost]
    public IActionResult HealthRegister(Policy policy)
    {
        policy.PolicyDate = DateTime.Now;
        _context.Policy.Add(policy);
        _context.SaveChanges();
        employee.InsertIntoPolicyRequests(policy);
        TempData["Register"] = "Policy request sent Successfully, view policies in Policy page.";
        return Redirect("AddPolicy");
    }
    public IActionResult PolicyRequestRemove(string empid, DateTime date)
    {
        
        string? user = HttpContext.Session.GetString("User2");
        // Policy list=(Policy)_context.Policy.Where(record => record.EmployeeId==empid&& record.PolicyDate.ToString("dd-MM-yyyy HH:mm:ss")==date.ToString("dd-MM-yyyy HH:mm:ss"));
        // _context.Policy.Remove(list);

        Details details = new Details();
        employee.RemovePolicyRequests(empid, date);

        IEnumerable<Policy> policies = _context.Policy.Where(option => option.UserName == user).ToList();
        return RedirectToAction("Policy", policies);
    }


    public IActionResult Policy()
    {
        if ((HttpContext.Session.GetString("User2")) != null)
        {
            DateTime time;
            string? user = HttpContext.Session.GetString("User2");
            IEnumerable<Policy> policies = _context.Policy.Where(option => option.UserName == user).ToList();
            foreach (var policy in policies)
            {
                time = policy.PolicyDate;
                ViewBag.timeonehour = time.AddHours(1);
                ViewBag.timenow = DateTime.Now;
                ViewBag.time = DateTime.Now;
            }

            return View(policies);
        }
        else return RedirectToAction("Login", "Home");
    }

    public IActionResult Dues()
    {
        string? EmployeeId;
        DataSet dataSet = new DataSet();
        if ((HttpContext.Session.GetString("User2")) != null)
        {
            string? user = HttpContext.Session.GetString("User2");
            EmployeeId = Convert.ToString(HttpContext.Session.GetString("empid"));
            Details details = new Details();
            dataSet = employee.GetAllDues(EmployeeId);
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                DateTime date = DateTime.Now;

                string? datestring = date.ToString("dd-MM-yyy hh:mm:ss tt");

                string? date2 = Convert.ToString(row["DueDateFrom"]);
                if (datestring == (date2 + " " + "12:00:00 AM"))
                {
                    ViewBag.statusset = "Pay";
                   
            details.SendTwilioSMS("You have Due to pay check with the link http://localhost:5150");
                }
                else
                {

                    ViewBag.statusset = "nil";

                }
            }

            return View(dataSet);
        }
        else return RedirectToAction("Login", "Home");
    }


    public IActionResult Help()
    {
        if ((HttpContext.Session.GetString("User2")) != null)
        {
            ViewBag.username = HttpContext.Session.GetString("User2");
            ViewBag.id = HttpContext.Session.GetString("empid");


            return View();
        }
        else return RedirectToAction("Login","Home");
    }
    [HttpPost]
    public IActionResult Help(Help help)
    {

        string? id = help.EmployeeId;

        _context.Help.Add(help);
        _context.SaveChanges();
        Details details = new Details();
        Response response = new Response();

        response.Username = HttpContext.Session.GetString("User2");
        response.EmployeeId = id;
        response.id = help.id;
        response.Reply = "";
        _context.Response.Add(response);
        _context.SaveChanges();
        ViewBag.username = HttpContext.Session.GetString("User2");

        TempData["Help"] = "Query submitted successfully";
        return View();
    }
    public IActionResult QueryResponse()
    {
        if ((HttpContext.Session.GetString("User2")) != null)
        {

            Details details = new Details();

            return View(employee.GetQueryResponsesofuserid(HttpContext.Session.GetString("User2")));
        }
        else return RedirectToAction("Login", "Home");
    }
    public IActionResult UnsendQueryWithId(string id, int keyid)
    {
        
            Details details = new Details();

            employee.UnsendQuerySent(id, keyid);
            return RedirectToAction("QueryResponse");
        
    }
    public IActionResult ClaimAmount(string empid, string? name, DateTime date, string? type, string? policyname)
    {
        if ((HttpContext.Session.GetString("User2")) != null)
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString=Details.GetConnectionString();
            using (connection)
            {
                connection.Open();
                SqlCommand updatecommand = new SqlCommand($"Select*from Policy where EmployeeId='{empid}' and FORMAT(PolicyDate, 'MM-dd-yyyy HH:mm:ss')='{date}' ", connection);
                SqlDataReader sdr = updatecommand.ExecuteReader();
                while (sdr.Read())
                {
                    ViewBag.dob = Convert.ToDateTime(sdr["DateofBirth"]);
                    ViewBag.policyamount = Convert.ToDecimal(sdr["PolicyAmount"]);
                    ViewBag.policyduration = Convert.ToInt32(sdr["PolicyDuration"]);
                }
            }
            ViewBag.id = empid;
            ViewBag.name = name;
            ViewBag.type = type;
            ViewBag.policyname = policyname;
            ViewBag.policydate = date;
            return View();
        }
        return RedirectToAction("Login","Home");
    }

    [HttpPost]
    public IActionResult ClaimAmount(ClaimNow claim)
    {
Console.WriteLine(claim.PolicyDate);
Console.WriteLine(employee.CheckClaim(claim));
        if (!employee.CheckClaim(claim))
        {
            
            List<MemoryStream> files = new List<MemoryStream>();
            foreach (var file in Request.Form.Files)
            {
                MemoryStream stream1 = new MemoryStream();
                file.CopyTo(stream1);
                files.Add(stream1);
            }
            claim.MedicalProof = files.FirstOrDefault().ToArray();
            claim.Receipt = files.LastOrDefault().ToArray();
            _context.ClaimNow.Add(claim);
            _context.SaveChanges();
            employee.DoAfterClaim(claim);
            TempData["Claim"] = "";
            IEnumerable<Policy> policies = _context.Policy.Where(record => record.EmployeeId == claim.EmployeeId && record.PolicyDate.ToString("yyyy-MM-dd HH:mm") == claim.PolicyDate.ToString("yyyy-MM-dd HH:mm"));
            return RedirectToAction("Policy", policies);
        }
        else
        {
            TempData["Claim"] = "Request already sent";
            IEnumerable<Policy> policies = _context.Policy.Where(record => record.EmployeeId == claim.EmployeeId && record.PolicyDate.ToString("yyyy-MM-dd HH:mm") == claim.PolicyDate.ToString("yyyy-MM-dd HH:mm"));
            return RedirectToAction("Policy", policies);
        }
    }

}
