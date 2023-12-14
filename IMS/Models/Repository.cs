using System.Data.SqlClient;
using System.Data;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using OfficeOpenXml;
using System.Net.Mail;
using System.Net;
using Vonage;
using Vonage.Request;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace IMS.Models
{

    public class Details : LogClass
    {
      //  IConfiguration builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
           private static IConfiguration _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
        public static string GetConnectionString()
        {
               
            return _configuration.GetConnectionString("DefaultConnection");

        }
        public override void WriteIntoLog(string? logText){
            try{
            
            string? path = _configuration.GetSection("LogPath").Value;

            StreamWriter streamWriter;
            if(!File.Exists(path))
            {
                streamWriter=File.CreateText(path);
            }
            else { 
                streamWriter=File.AppendText(path);
            }

            streamWriter.WriteLine(logText);
            streamWriter.Flush();
            streamWriter.Close();}
            catch(Exception exception){
                  Console.WriteLine(exception.Message);
            }
            
        }
        public  override void SendMail(string? from,string? recipient, string? subject, string? body)
        {
            MailMessage mailMessage = new MailMessage
            {
            From=new MailAddress(from),
            Subject=subject,
            Body=body,
            IsBodyHtml=true
            };
            mailMessage.To.Add(new MailAddress(recipient));
           SmtpClient smtpClient = new SmtpClient
            {
                Host = "SURYANEELAKANDAN" ,
                Port=587,
                EnableSsl = true
            };
            smtpClient.UseDefaultCredentials=true;
            
            smtpClient.Send(mailMessage);
        }

                public void SendTwilioSMS(string text)
        { 
            
            string authSID = _configuration.GetSection("Twilio").GetSection("SID").Value;
             string authToken = _configuration.GetSection("Twilio").GetSection("Token").Value;
           TwilioClient.Init(authSID,authToken);
           var message=MessageResource.Create(
                body:text,
                from:new Twilio.Types.PhoneNumber(_configuration.GetSection("Twilio").GetSection("Number").Value),
                to:new Twilio.Types.PhoneNumber("+919791878870")
            );
        } 

        public void SendSMS(string text)
        { 
            var credentials= Credentials.FromApiKeyAndSecret(
             _configuration.GetSection("Vonage").GetSection("Key").Value,
             _configuration.GetSection("Vonage").GetSection("Value").Value
            );
            var vonageClient = new VonageClient(credentials);
            vonageClient.SmsClient.SendAnSms(new Vonage.Messaging.SendSmsRequest(){
                To=_configuration.GetSection("Vonage").GetSection("To").Value,
                From=_configuration.GetSection("Vonage").GetSection("From").Value,
                Text=text
            });
        } 

        public string? LoginPage(Login login)
        {
            string? result = "";
            int countAdmin, countEmployee;
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = GetConnectionString();
                using (connection)
                {
                    connection.Open();
                    SqlCommand sqlCommand = new SqlCommand("GetLoginCount", connection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@Username", login.Username);
                    sqlCommand.Parameters.AddWithValue("@Password", login.Password);
                    countAdmin = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    SqlCommand sqlCommandGet = new SqlCommand("GetUserCount", connection);
                    sqlCommandGet.CommandType = CommandType.StoredProcedure;
                    sqlCommandGet.Parameters.AddWithValue("@Username", login.Username);
                    sqlCommandGet.Parameters.AddWithValue("@Password", login.Password);
                    countEmployee = Convert.ToInt32(sqlCommandGet.ExecuteScalar());

                    if (countAdmin == 1)
                        result = "Admin";
                    else if (countEmployee == 1)
                        result = "Employee";
                    else result = "";
                }
            }

            catch (SqlException sqlException)
            {
                WriteIntoLog(sqlException.Message);
            }

            return result;
        }

        public bool UpdatePassword(ChangePassword change)
        {
            bool result = false;
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = GetConnectionString();
                using (connection)
                {
                    connection.Open();
                    SqlCommand sqlCommandGet = new SqlCommand("GetSingleAdminCount", connection);
                    sqlCommandGet.CommandType = CommandType.StoredProcedure;
                    sqlCommandGet.Parameters.AddWithValue("@Username", change.Username);
                    int count1 = Convert.ToInt32(sqlCommandGet.ExecuteScalar());

                    SqlCommand sqlCommand = new SqlCommand("GetOneUserCount", connection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@Username", change.Username);
                    int count2 = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    if (count1 == 1)
                    {
                        SqlCommand updateCommand = new SqlCommand("UpdateSingleAdmin", connection);
                        updateCommand.CommandType = CommandType.StoredProcedure;
                        updateCommand.Parameters.AddWithValue("@Username", change.Username);
                        updateCommand.Parameters.AddWithValue("@OldPassword", change.OldPassword);
                        updateCommand.Parameters.AddWithValue("@NewPassword", change.NewPassword);
                        updateCommand.ExecuteNonQuery();
                        result=true;
                    }
                    else if (count2 == 1)
                    {
                        SqlCommand updateCommand = new SqlCommand("UpdateSingleUser", connection);
                        updateCommand.CommandType = CommandType.StoredProcedure;
                        updateCommand.Parameters.AddWithValue("@Username", change.Username);
                        updateCommand.Parameters.AddWithValue("@OldPassword", change.OldPassword);
                        updateCommand.Parameters.AddWithValue("@NewPassword", change.NewPassword);
                        updateCommand.ExecuteNonQuery();
                        result = true;
                    }
                }
            }
            catch (SqlException sqlException)
            {
                WriteIntoLog(sqlException.Message);
            }
            return result;
        }

        public DataSet GetProfile(string user)
        {

            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = GetConnectionString();
                using (connection)
                {
                    connection.Open();
                    SqlCommand selectcommand = new SqlCommand("GetProfile", connection);
                    selectcommand.CommandType = CommandType.StoredProcedure;
                    selectcommand.Parameters.AddWithValue("@Username", user);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectcommand);
                    sqlDataAdapter.Fill(dataSet);
                }

            }
            catch (SqlException sqlException)
            {
                WriteIntoLog(sqlException.Message);
            }
            return dataSet;

        }
        public string UpdateProfile(ProfileDetail profile)
        {
            string result = "";

            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = GetConnectionString();
                using (connection)
                {
                    connection.Open();
                    SqlCommand sqlCommandGet = new SqlCommand("GetSingleAdminCount", connection);
                    sqlCommandGet.CommandType = CommandType.StoredProcedure;
                    sqlCommandGet.Parameters.AddWithValue("@Username", profile.UserName);
                    int count1 = Convert.ToInt32(sqlCommandGet.ExecuteScalar());

                    SqlCommand sqlCommandGetUser = new SqlCommand("GetOneUserCount", connection);
                    sqlCommandGetUser.CommandType = CommandType.StoredProcedure;
                    sqlCommandGetUser.Parameters.AddWithValue("@Username", profile.UserName);
                    int count2 = Convert.ToInt32(sqlCommandGetUser.ExecuteScalar());


                    SqlCommand updateCommand = new SqlCommand("UpdateProfile", connection);
                    updateCommand.CommandType = CommandType.StoredProcedure;
                    updateCommand.Parameters.AddWithValue("@Username", profile.UserName);
                    updateCommand.Parameters.AddWithValue("@Name", profile.Name);
                    updateCommand.Parameters.AddWithValue("@EmployeeId", profile.EmployeeId);
                    updateCommand.Parameters.AddWithValue("@Role", profile.Role);
                    updateCommand.Parameters.AddWithValue("@Age", profile.Age);
                    updateCommand.Parameters.AddWithValue("@Address", profile.Address);
                    updateCommand.Parameters.AddWithValue("@Phone", profile.Phone);
                    updateCommand.Parameters.AddWithValue("@ImageUrl", profile.ImageUrl);
                    updateCommand.ExecuteNonQuery();
                    if (count1 == 1)
                    { result = "Admin"; }
                    else if (count2 == 1)
                    { result = "Employee"; }

                }
            }

            catch (SqlException sqlException)
            {
                WriteIntoLog(sqlException.Message);
            }



            return result;
        }

        public List<PdfModel> GeneratePdf()
        {
            List<PdfModel> pdfformat = new List<PdfModel>();
            DataSet dataSet = new DataSet();
            LoginDetails login = new LoginDetails();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = GetConnectionString();
            using (connection)
            {
                connection.Open();
                SqlCommand selectcommand = new SqlCommand("GetAllUser", connection);
                selectcommand.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectcommand);
                sqlDataAdapter.Fill(dataSet);
            }

            var document = new PdfDocument();
            string htmlcontent = "<div style='width:100%; text-align:center;font-family: 'Times New Roman', Times, serif;'>";
            htmlcontent += "<h2> Insurance Management System</h2>";
            htmlcontent += "<h2>" + DateTime.Now + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Employee List </h2>";
            htmlcontent += "<h2> Total Count : " + dataSet.Tables[0].Rows.Count + "</h2>";
            htmlcontent += "<table style ='width:100%; border: 1px solid #000'>";
            htmlcontent += "<thead style='font-weight:bold;'>";
            htmlcontent += "<tr >";
            htmlcontent += "<th style='height:30px;' ><ins> <b>Employee Id</b></ins></th>";
            htmlcontent += "<th style='height:30px;' ><ins> <b>Name</b></ins></th>";
            htmlcontent += "<th style='height:30px;' ><ins><b>User Name</b> </ins></th>";
            htmlcontent += "<th style='height:30px;' ><ins> <b>Password</b></ins></th>";
            htmlcontent += "</tr >";
            htmlcontent += "</thead >";
            htmlcontent += "<hr>";
            htmlcontent += "<tbody >";
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                htmlcontent += "<tr>";
                htmlcontent += "<td style='height:20px;'>" + row["EmployeeId"] + "</td>";
                htmlcontent += "<td style='height:20px;'>" + row["Name"] + "</td>";
                htmlcontent += "<td style='height:20px;'>" + row["UserName"] + "</td>";
                htmlcontent += "<td style='height:20px;'>" + row["Password"] + "</td>";
                htmlcontent += "</tr>";
            };
            htmlcontent += "</tbody>";
            htmlcontent += "</table>";
            htmlcontent += "<h2>Thank You</h2>";
            htmlcontent += "</div>";

            PdfGenerator.AddPdfPages(document, htmlcontent, PageSize.A4);

            byte[]? response = null;
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms);
                response = ms.ToArray();
            }
            DateTime date = DateTime.Now;
            string? dateonly = date.ToString("d-M-yyyy");
            string Filename = "EmployeeList_" + dateonly + ".pdf";
            PdfModel pdfModel = new PdfModel();
            pdfModel.responsebyte = response;
            pdfModel.Pdfformat = "application/pdf";
            pdfModel.filename = Filename;
            pdfformat.Add(pdfModel);
            return pdfformat;
        }
        public List<ExcelModel> GenerateExcel()
        {
            DataSet dataSet1 = new DataSet();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = GetConnectionString();
            using (connection)
            {
                connection.Open();
                SqlCommand selectcommand = new SqlCommand("GetAllUser", connection);
                selectcommand.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectcommand);
                sqlDataAdapter.Fill(dataSet1);
            }
            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("EmployeeList");
                worksheet.Cells.LoadFromDataTable(dataSet1.Tables[0], true);
                package.Save();
            }
            stream.Position = 0;
            string excelname = $"EmployeeList_{DateTime.Now.ToString("dd-MM-yyyy")}.xlsx";
            List<ExcelModel> excelformat = new List<ExcelModel>();
            ExcelModel excelModel = new ExcelModel();
            byte[]? response = stream.ToArray();
            excelModel.responsebyte = response;
            excelModel.Excelformat = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excelModel.filename = excelname;
            excelformat.Add(excelModel);
            return excelformat;
        }

    }

}
