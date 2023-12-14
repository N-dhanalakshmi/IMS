using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using IMS.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IMS.Controllers;

[Actions]
    [ExceptionFilter]
    public class PaymentController : Controller
    {
        Details logClass=new Details();
       
        private const string _key = "rzp_test_RbSBYnsK8PwJv3";
        private const string _secret = "CVnPoBKIw5pg4FmxcdiosHik";        

        public ViewResult Registration(string id,string name,string policyname,DateTime date,decimal amount)
        {
            HttpContext.Session.SetString("id",id);
            HttpContext.Session.SetString("name",name);
            HttpContext.Session.SetString("policy",policyname);
            string? datenew=date.ToString("yyyy-MM-dd HH:mm:ss");
            string? datenew1=date.ToString("yyyy-MM-dd HH:mm:ss");
            //HttpContext.Session.SetString("date",datenew);
            HttpContext.Session.SetString("date1",datenew1);
            string? amountnew = amount.ToString();
            HttpContext.Session.SetString("amount",amountnew);
            var model = new RegistrationModel() {
            EmployeeId=id,
            Name=name,
            PolicyName=policyname,
            PolicyDate=date,
            PremiumAmount=amount
            };
            return View(model);
        }

        public ViewResult Payment(RegistrationModel registration)
        {
            
            OrderModel order = new OrderModel()
            {
                OrderAmount = registration.PremiumAmount,
                Currency = "INR",
                Payment_Capture = 1,    // 0 - Manual capture, 1 - Auto capture
                Notes = new Dictionary<string, string>()
                {
                    { "note 1", "first note while creating order" }, { "note 2", "you can add max 15 notes" },
                    { "note for account 1", "this is a linked note for account 1" }, { "note 2 for second transfer", "it's another note for 2nd account" }
                }
            };
            // var orderId = CreateOrder(order);
            var orderId = CreateTransfersViaOrder(order);

            RazorPayOptionsModel razorPayOptions = new RazorPayOptionsModel()
            {
                Key = _key,
                AmountInSubUnits = order.OrderAmountInSubUnits,
                Currency = order.Currency,
                Name =registration.PolicyName,
                EmployeeId = registration.EmployeeId,
                ImageLogUrl = "",
                OrderId = orderId,
                ProfileName = registration.Name,
                ProfileContact = registration.Mobile,
                ProfileEmail = registration.Email,
                Notes = new Dictionary<string, string>()
                {
                    { "note 1", "this is a payment note" }, { "note 2", "here also, you can add max 15 notes" }
                }
            };
            return View(razorPayOptions);
        }

        private string CreateOrder(OrderModel order)
        {
            try
            {
                RazorpayClient client = new RazorpayClient(_key, _secret);
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", order.OrderAmountInSubUnits);
                options.Add("currency", order.Currency);
                options.Add("payment_capture", order.Payment_Capture);
                options.Add("notes", order.Notes);

                Order orderResponse = client.Order.Create(options);
                var orderId = orderResponse.Attributes["id"].ToString();
                return orderId;
            }
            catch (Exception exception)
            {
                logClass.WriteIntoLog(exception.Message);
                return "";
            }
        }

        private string CreateTransfersViaOrder(OrderModel order)
        {
            try
            {
                RazorpayClient client = new RazorpayClient(_key, _secret);
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", order.OrderAmountInSubUnits);
                options.Add("currency", order.Currency);
                options.Add("payment_capture", order.Payment_Capture);
                options.Add("notes", order.Notes);

                List<Dictionary<string, object>> transfers = new List<Dictionary<string, object>>();

                // Tranfer to Account 1
                Dictionary<string, object> transfer = new Dictionary<string, object>();
                transfer.Add("account", "acc_FrZdKIHffMifPl");              // account 1
                transfer.Add("amount", order.OrderAmountInSubUnits / 2);    // 50% amount of the total amount
                transfer.Add("currency", "INR");
                transfer.Add("notes", order.Notes);
                List<string> linkedAccountNotes = new List<string>();
                linkedAccountNotes.Add("note for account 1");
                transfer.Add("linked_account_notes", linkedAccountNotes);
                transfers.Add(transfer);

                // Transfer to Account 2
                transfer = new Dictionary<string, object>();
                transfer.Add("account", "acc_FrZbSTT96Jfp6n");              // account 2
                transfer.Add("amount", order.OrderAmountInSubUnits / 2);    // 50% amount of the total amount
                transfer.Add("currency", "INR");
                transfer.Add("notes", order.Notes);
                linkedAccountNotes = new List<string>();
                linkedAccountNotes.Add("note 2 for second transfer");
                transfer.Add("linked_account_notes", linkedAccountNotes);
                transfers.Add(transfer);

                // Add transfers to options object
                options.Add("transfers", transfers);

                Order orderResponse = client.Order.Create(options);
                var orderId = orderResponse.Attributes["id"].ToString();
                return orderId;
            }
            catch (Exception exception)
            {
                logClass.WriteIntoLog(exception.Message);
                return "";
            }
        }
[HttpPost]
        public IActionResult AfterPayment()
        {

            var paymentStatus = Request.Form["paymentstatus"].ToString();
     
            if (paymentStatus == "Fail")
                {
                    TempData["Dues2"]="Try Again";
                    RegistrationModel registrationModel=new RegistrationModel();
                    registrationModel.EmployeeId=HttpContext.Session.GetString("id");
                    registrationModel.Name=HttpContext.Session.GetString("name");
                    registrationModel.PolicyName=HttpContext.Session.GetString("policy");
                    registrationModel.PolicyDate=Convert.ToDateTime(HttpContext.Session.GetString("date1"));
                    registrationModel.PremiumAmount=Convert.ToDecimal(HttpContext.Session.GetString("amount"));
                    return View("Registration",registrationModel);
                }

           
            else 
            {
                 var orderId = Request.Form["orderid"].ToString();
            var paymentId = Request.Form["paymentid"].ToString();
            var signature = Request.Form["signature"].ToString();

            var validSignature = CompareSignatures(orderId, paymentId, signature);
            validSignature=true;

                string? id=HttpContext.Session.GetString("id");
            string? name=HttpContext.Session.GetString("name");
            string? policy=HttpContext.Session.GetString("policy");
            string? date=HttpContext.Session.GetString("date1");
            DateTime datenew=Convert.ToDateTime(date);
            string? amount=HttpContext.Session.GetString("amount");
            decimal amount1=Convert.ToDecimal(amount);
            DateTime duefrom=Convert.ToDateTime(date).AddYears(1);
            string? duefromstring=duefrom.ToString("dd-MM-yyy");
            DateTime dueto=duefrom.AddMonths(1);
            string? duetostring=dueto.ToString("dd-MM-yyy");
                SqlConnection connection = new SqlConnection();
            connection.ConnectionString=Details.GetConnectionString();
            using (connection){ 
          connection.Open();
          Console.WriteLine(date);
            SqlCommand updatecommand =new SqlCommand($"update Dues set Status='Paid' , DueDateFrom='{duefromstring}' , DueDateTo='{duetostring}' where EmployeeId='{id}' and FORMAT(PolicyDate, 'yyyy-MM-dd HH:mm:ss')='{date}'",connection);
            updatecommand.ExecuteNonQuery();     }
                DataSet dataSet=new DataSet();
                EmployeeFile employee=new EmployeeFile();
                dataSet= employee.GetAllDues(id);
                return RedirectToAction("Dues","Employee",dataSet);
            }
              //  return View("Registration");
            
        }

        private bool CompareSignatures(string orderId, string paymentId, string razorPaySignature)
        {
            var text = orderId + "|" + paymentId;
            var secret = _secret;
            var generatedSignature = CalculateSHA256(text, secret);
            if (generatedSignature == razorPaySignature)
                return true;
            else
                return false;
        }

        private string CalculateSHA256(string text, string secret)
        {
            string result = "";
            var enc = Encoding.Default;
            byte[]
            baText2BeHashed = enc.GetBytes(text),
            baSalt = enc.GetBytes(secret);
            System.Security.Cryptography.HMACSHA256 hasher = new HMACSHA256(baSalt);
            byte[] baHashedText = hasher.ComputeHash(baText2BeHashed);
            result = string.Join("", baHashedText.ToList().Select(b => b.ToString("x2")).ToArray());
            return result;
        }

        public ViewResult Capture()
        {
            return View();
        }

        public IActionResult CapturePayment(string paymentId)
        {
            RazorpayClient client = new RazorpayClient(_key, _secret);
            Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);
            var amount = payment.Attributes["amount"];
            var currency = payment.Attributes["currency"];

            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", amount);
            options.Add("currency", currency);
            Razorpay.Api.Payment paymentCaptured = payment.Capture(options);
            ViewBag.Message = "Payment capatured!";
            string? id=HttpContext.Session.GetString("id");
            string? name=HttpContext.Session.GetString("name");
            string? policy=HttpContext.Session.GetString("policy");
            string? date=HttpContext.Session.GetString("date1");
            DateTime datenew=Convert.ToDateTime(date);
            string? amount1=HttpContext.Session.GetString("amount");
            decimal amount2=Convert.ToDecimal(amount);
            DateTime duefrom=Convert.ToDateTime(date).AddYears(1);
            string? duefromstring=duefrom.ToString("dd-MM-yyy");
            DateTime dueto=duefrom.AddMonths(1);
            string? duetostring=dueto.ToString("dd-MM-yyy");
                TempData["Dues1"] = "Congratulations!! Your payment was successful";
                SqlConnection connection = new SqlConnection();
            connection.ConnectionString=Details.GetConnectionString();
            using (connection){
          connection.Open();
            Console.WriteLine(date);
            SqlCommand updatecommand =new SqlCommand($"update Dues set Status='Paid' , DueDateFrom='{duefromstring}' , DueDateTo='{duetostring}' where EmployeeId='{id}' and FORMAT(PolicyDate, 'yyyy-MM-dd HH:mm:ss')='{date}'",connection);
            updatecommand.ExecuteNonQuery();
            } 
                DataSet dataSet=new DataSet();
                EmployeeFile employee=new EmployeeFile();
                dataSet= employee.GetAllDues(id);
                return RedirectToAction("Dues","Employee",dataSet);
        }
    }

