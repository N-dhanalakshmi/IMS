using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using IMS.Models;
using IMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace IMS.Controllers;
[Actions]
[ExceptionFilter]
public class HomeController : Controller
{
    HttpClient client;
    private CookieOptions tokenCookie;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _hostEnvironment;
    private IConfiguration _configuration;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IWebHostEnvironment hostEnvironment,IConfiguration configuration)
    {
        _configuration=configuration;
        _context = context;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        tokenCookie = new CookieOptions();
        tokenCookie.Expires = DateTime.Now.AddSeconds(30);
        client = new HttpClient();
    }

    private string GenerateToken(Login login)
    {
        var securitykey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]));
        var credentials=new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
        var token=new JwtSecurityToken(_configuration["Jwt:Issuer"],_configuration["Jwt:Audience"],null,
        expires:DateTime.Now.AddSeconds(30),
        signingCredentials:credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool validateToken(string token)
      {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
        SecurityToken validatedToken;
        var claim = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        return claim.Identity.IsAuthenticated;
      }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Login()
    {
        
        return View();
    }
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login(Login login)
    {
        TempData["username"]=login.Username;
        if (ModelState.IsValid)
        { 
            
            Details details = new Details();
            string? loggedUser = details.LoginPage(login);
            if (loggedUser == "Admin")
            {
                TempData["Login"] = "Logged in Successfully";
                HttpContext.Session.SetString("User1", login.Username);
                HttpContext.Session.SetString("User", login.Username);
                Response.Cookies.Append("tokenCookie", GenerateToken(login), tokenCookie);
                _logger.LogInformation("Login was attempted");
                _logger.LogTrace("Employee with Id " + login.Username + " is logged in");
                return Redirect("Details");
            }
            else if (loggedUser == "Employee")
            {
                TempData["Login"] = "Logged in Successfully";
                HttpContext.Session.SetString("User2", login.Username);
                HttpContext.Session.SetString("User", login.Username);
                return Redirect("InsuranceDetails");
            }

        }
                TempData["Login"] = "Invalid Username or Password";

                return View();
    }

    public IActionResult ChangePassword()
    {
        return View();
    }
    [HttpPost]

    public IActionResult ChangePassword(ChangePassword newPassword)
    {
        if (ModelState.IsValid)
        {
            Details details = new Details();

            if (details.UpdatePassword(newPassword))
            {
                TempData["ChangePass"] = "Password Changed Successfully";
                return Redirect("Login");
            }
            else return View();
        }

        return View();
    }

    public IActionResult Details()
    {
        string? token=Request.Cookies["tokenCookie"];
        try{
            if(string.IsNullOrEmpty(token)||!validateToken(token)){
            throw new InvalidLoginException("Invalid Entry");
        }}
        catch(InvalidLoginException invalidlogin)
        {
        Details details=new Details();
        details.WriteIntoLog(invalidlogin.ToString());
        }
        if (HttpContext.Session.GetString("User1") != null)
        {

            Details details = new Details();
            return View(details.GetProfile(HttpContext.Session.GetString("User1")));
        }
        else
        {
            return Redirect("Login");
        }
    }
    public IActionResult InsuranceDetails()
    {
        if ((HttpContext.Session.GetString("User2")) != null)
        {
            Details details = new Details();
            DataSet dataSet = details.GetProfile(HttpContext.Session.GetString("User2"));
            HttpContext.Session.SetString("empid", dataSet.Tables[0].Rows[0].Field<string>("EmployeeId"));
            return View(dataSet);
        }
        else
        {
            return Redirect("Login");
        }
    }

    public IActionResult UpdateProfile()
    {
        if ((HttpContext.Session.GetString("User")) != null)
        {
            Details details = new Details();
            DataSet dataSet = details.GetProfile(HttpContext.Session.GetString("User"));
            ViewBag.Id = dataSet.Tables[0].Rows[0].Field<string>("EmployeeId");
            ViewBag.name = dataSet.Tables[0].Rows[0].Field<string>("Name");
            ViewBag.user = dataSet.Tables[0].Rows[0].Field<string>("UserName");
            return View();
        }
        return Redirect("Login");

    }
    [HttpPost]

    public IActionResult UpdateProfile(ProfileDetail profile, IFormFile file)
    {
        if (ModelState.IsValid)
        { 
            string? rootpath = _hostEnvironment.WebRootPath;
            if (file != null) 
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(rootpath, @"Images\ProfileImages");
                var extension = Path.GetExtension(file.FileName);
                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                { file.CopyTo(fileStreams); }
                profile.ImageUrl = @"\Images\ProfileImages\" + fileName + extension;
            }

            Details details = new Details();



            DataSet profiles = details.GetProfile(HttpContext.Session.GetString("User"));
            if (details.UpdateProfile(profile) == "Admin")
            {
                TempData["Profile"] = "Profile updated Successfully";

                return RedirectToAction("Details", profiles);

            }
            else if (details.UpdateProfile(profile) == "Employee")
            {
                TempData["Profile"] = "Profile updated Successfully";
                return RedirectToAction("InsuranceDetails", profiles);
            }

        }
        return View();

    }
    public IActionResult Cancel()
    {
        Details details = new Details();

        string? user = Convert.ToString(TempData["profile"]);
        if (user == "Admin")
        {
            return View("Details", details.GetProfile(HttpContext.Session.GetString("User")));
        }
        else
            return View("InsuranceDetails", details.GetProfile(HttpContext.Session.GetString("User")));

    }


    public IActionResult Logout()
    {

        HttpContext.Session.Remove("User1");
        HttpContext.Session.Remove("User2");
        HttpContext.Session.Remove("User");
        TempData["Login"] = "Logged out successfully";
        return Redirect("Login");
    }


    public IActionResult GeneratePDF()
    {
        string? file = "";
        byte[]? response = { 0, 0 };
        Details details = new Details();
        IEnumerable<PdfModel> pdfdownload = details.GeneratePdf();
        foreach (var data in pdfdownload)
        {
            file = data.filename;
            response = data.responsebyte;
        }
        return File(response, "application/pdf", file);
    }


    public IActionResult GenerateExcel()
    {
        string? file = "";
        byte[]? response = { 0, 0 };
        Details details = new Details();
        IEnumerable<ExcelModel> exceldownload = details.GenerateExcel();
        foreach (var data in exceldownload)
        {
            file = data.filename;
            response = data.responsebyte;
        }
        return File(response, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file);
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


}
public class InvalidLoginException : Exception
{
    public InvalidLoginException(String message) : base(message)
    {

    }
}