using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using IMS.Models;
using IMS.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using PagedList;
using PagedList.Mvc;
namespace IMS.Controllers;

[Actions]
[ExceptionFilter]
public class AdminController : Controller
{
    Uri baseAddress = new Uri("http://localhost:5212");
    HttpClient client;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;
    private IClaimPolicyDI _IClaimPolicy;
    
    public AdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, IClaimPolicyDI IClaimPolicy)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
        _IClaimPolicy = IClaimPolicy; ;
        client = new HttpClient();
        client.BaseAddress = baseAddress;
    }
    AdminFile admin = new AdminFile();
    public IActionResult AddEmployee()
    {

        if ((HttpContext.Session.GetString("User1")) != null)
        { return View(); }
        else
        {
            return RedirectToAction("Login", "Home");
        }
    }
    [HttpPost]
    public IActionResult AddEmployee(LoginDetails login)
    {
        if (ModelState.IsValid)
        {
            admin.CreateEmployee(login);
            TempData["AddEmployee"] = "Employee Added Successfully";
            return RedirectToAction("Details", "Home");
        }
        return View("AddEmployee");
    }

    public IActionResult AddNewAdmin()
    {
        if ((HttpContext.Session.GetString("User1")) != null)
        {
            return View();
        }
        return RedirectToAction("Login", "Home");
    }
    [HttpPost]
    public IActionResult AddNewAdmin(Login login)
    {
        if (ModelState.IsValid)
        {
            string data = JsonConvert.SerializeObject(login);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = client.PostAsync("/Crud", content).Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                admin.InsertAdminNewLogin(login);
                TempData["CreateEmployee"] = "New Admin details added";
                return RedirectToAction("Details", "Home");
            }
        }
        return View();
    }

    public IActionResult EmployeeList()
    {
        if ((HttpContext.Session.GetString("User1")) != null)
        {
            IEnumerable<LoginDetails> data = admin.GetEmployeeList();
            return View(data);
        }
        else return RedirectToAction("Login", "Home");
    }

    public IActionResult EmployeeUpdate(string userid)
    {
        if ((HttpContext.Session.GetString("User1")) != null)
        {
            ViewBag.user = userid;
            return View();
        }
        else return RedirectToAction("Login", "Home");

    }
    [HttpPost]
    public IActionResult EmployeeUpdate(Login login)
    {

        admin.UpdateEmployeeList(login);
        TempData["EmployeeUpdate"] = "Employee details updated successfully";
        IEnumerable<LoginDetails> data = admin.GetEmployeeList();
        return View("EmployeeList", data);

    }
    public IActionResult EmployeeRemove(string userid, string empid)
    {
        if (HttpContext.Session.GetString("User1") != null)
        {
            ViewBag.user = userid;
            ViewBag.id = empid;
            return View();
        }
        else return RedirectToAction("Login", "Home");

    }
    [HttpPost]
    public IActionResult EmployeeRemove(Login login)
    {
        admin.RemoveEmployeeList(login.Username);
        TempData["EmployeeRemove"] = "Employee removed from list successfully";
        IEnumerable<LoginDetails> data = admin.GetEmployeeList();
        return View("EmployeeList", data);

    }

    public IActionResult PolicyRequests()
    {
        if (HttpContext.Session.GetString("User1") != null)
        {
            ViewBag.timenow = DateTime.Now;
            return View(admin.GetPolicyRequests());
        }
        else return RedirectToAction("Login", "Home");
    }
    public IActionResult SelectPolicyRequest(string? status){
        DataSet dataSet=admin.SelectPolicyRequests(status);
        if(dataSet.Tables[0].Rows.Count > 0)
        return RedirectToAction("PolicyRequests",admin.SelectPolicyRequests(status));
        else return  RedirectToAction("PolicyRequests",admin.GetPolicyRequests());
    }

    public IActionResult PolicyAccept(string id, string name, string type, string policyname, DateTime date, decimal amount, int duration)
    {
       string? text="Your policy request has been accepted. See details in http://localhost:5150";
      Details details=new();
      Thread thread=new(()=>admin.AcceptPolicyRequests(id, name, type, policyname, date, amount, duration));
      Thread threadUser=new(()=>details.SendTwilioSMS(text));
       thread.Start();
       thread.Join();
       threadUser.Start();
       threadUser.Join();
        return RedirectToAction("PolicyRequests");
    }
    public IActionResult PolicyDeny(string id, DateTime date)
    {

        admin.DenyPolicyRequests(id, date);
        string? user=admin.GetUserName(id);
        if(!string.IsNullOrEmpty(user)){
            Details details=new Details();
            details.SendTwilioSMS("Your policy request has been Denied.Please contact Admin for more details. neela.dhanalakshmi1810@gmail.com");
             }
        return RedirectToAction("PolicyRequests");
    }

    public IActionResult DuesAll()
    {

        if ((HttpContext.Session.GetString("User1")) != null)
        {
            IEnumerable<Dues> dues = _context.Dues.ToList();
            return View(dues);
        }
        return RedirectToAction("Login", "Home");
    }

    public IActionResult ViewClaimRequests(int page)
    {
        if ((HttpContext.Session.GetString("User1")) != null)
        {
            //int pageCount;
            List<ClaimNow> list = _IClaimPolicy.ReadClaimRequest();
            // if(page!=null)
            // pageCount=page;
            // else 
            // pageCount=1;.ToPagedList(pageCount,5)
            return View(list);

        }
        return RedirectToAction("Login", "Home");
    }


    public IActionResult Requests()
    {
        if ((HttpContext.Session.GetString("User1")) != null)
        {
            DataSet dataSet = admin.GetRequestsOfUser();
            return View(dataSet);
        }
        else return RedirectToAction("Login", "Home");
    }

    public IActionResult ReplyQueryWithId(string id, string query, string reply, string user, int keyid)
    {
        if ((HttpContext.Session.GetString("User1")) != null)
        {

            TempData["id"] = keyid;
            ViewBag.id6 = id;
            ViewBag.query = query;
            ViewBag.reply = reply;
            ViewBag.user = user;
            return View();
        }
        else return RedirectToAction("Login", "Home");
    }
    [HttpPost]
    public IActionResult ReplyQueryWithId(Response response)
    {
        string? id = response.EmployeeId;
        int keyid = Convert.ToInt32(TempData["id"]);
        admin.ReplyAllQuery(response, keyid);

        return RedirectToAction("Requests", admin.GetRequestsOfUser());
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
