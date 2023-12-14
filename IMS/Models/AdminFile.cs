using System.Data;
using System.Data.SqlClient;

namespace IMS.Models
{
    public class AdminFile 
    {
        Details details=new Details();
        public void CreateEmployee(LoginDetails user)
        {
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Details.GetConnectionString();
                using (connection)
                {

                    connection.Open();
                    SqlCommand sqlCommandLogin = new SqlCommand($"insert into LoginData values('{user.EmployeeId}','{user.Name}','{user.Username}','{user.Password}')", connection);
                    sqlCommandLogin.ExecuteNonQuery();
                    SqlCommand sqlCommandAdmin = new SqlCommand($"insert into ProfileDetails values('{user.Name}','Nil',0,'Nil','Nil','{user.Username}','https://i.pinimg.com/originals/df/5f/5b/df5f5b1b174a2b4b6026cc6c8f9395c1.jpg','{user.EmployeeId}')", connection);
                    sqlCommandAdmin.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }

        }
        public void InsertAdminNewLogin(Login login)
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {

                connection.Open();
                SqlCommand command1 = new SqlCommand($"insert into NewAdmin values('{login.Username}','{login.Password}')", connection);
                command1.ExecuteNonQuery();
                SqlCommand command2 = new SqlCommand($"insert into ProfileDetails values('Unavailable','Nil',0,'Nil','Nil','{login.Username}','https://i.pinimg.com/originals/df/5f/5b/df5f5b1b174a2b4b6026cc6c8f9395c1.jpg','000000')", connection);
                command2.ExecuteNonQuery();
            }
        }

        public List<LoginDetails> GetEmployeeList()
        {
            List<LoginDetails> profilelist = new List<LoginDetails>();
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Details.GetConnectionString();
                using (connection)
                {
                    connection.Open();
                    SqlCommand command1 = new SqlCommand("select EmployeeId , Name , UserName from ProfileDetails", connection);
                    SqlDataReader sdr = command1.ExecuteReader();
                    while (sdr.Read())
                    {
                        LoginDetails profile = new LoginDetails
                        {
                            EmployeeId = Convert.ToString(sdr["EmployeeId"]),
                            Name = Convert.ToString(sdr["Name"]),
                            Username = Convert.ToString(sdr["UserName"])
                        };
                        profilelist.Add(profile);
                    }
                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }
            return profilelist;
        }

        public void UpdateEmployeeList(Login login)
        {
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Details.GetConnectionString();
                using (connection)
                {

                    connection.Open();
                    SqlCommand updatecommand1 = new SqlCommand($"UPDATE LoginData SET Password='{login.Password}' WHERE UserName='{login.Username}'", connection);
                    updatecommand1.ExecuteNonQuery();
                    SqlCommand updatecommand2 = new SqlCommand($"UPDATE NewAdmin SET Password='{login.Password}' WHERE UserName='{login.Username}'", connection);
                    updatecommand2.ExecuteNonQuery();

                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }

        }
        public void RemoveEmployeeList(string user)
        {

            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Details.GetConnectionString();
                using (connection)
                {
                    connection.Open();

                    SqlCommand deleteCommand = new SqlCommand("DeleteUserDetails", connection);
                    deleteCommand.CommandType = CommandType.StoredProcedure;
                    deleteCommand.Parameters.AddWithValue("@Username",user);
                    deleteCommand.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }
        }
        public string? GetUserName(string id){
            string? user=null;
            try
            {
                SqlConnection connection = new SqlConnection
                {
                    ConnectionString = Details.GetConnectionString()
                };
                using (connection)
                {
                    connection.Open();
                    SqlCommand command1 = new SqlCommand($"select Phone from ProfileDetails where EmployeeId='{id}'", connection);
                    SqlDataReader sdr = command1.ExecuteReader();
                    while (sdr.Read())
                    {
                        user=sdr["Phone"].ToString();
                    }
                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }
            return user;
        }

        public DataSet GetPolicyRequests()
        {
            DataSet dataSet1 = new DataSet();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {
                connection.Open();
                SqlCommand selectCommand = new SqlCommand("GetPolicyRequests", connection);
                selectCommand.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                sqlDataAdapter.Fill(dataSet1);
            }
            return dataSet1;
        }
        public DataSet SelectPolicyRequests(string status)
        {
            DataSet dataSet = new DataSet();
            try{ 
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {
                connection.Open();
                SqlCommand selectCommand = new SqlCommand("SelectPolicyRequests", connection);
                selectCommand.CommandType = CommandType.StoredProcedure;
                selectCommand.Parameters.AddWithValue("@Status",status);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                sqlDataAdapter.Fill(dataSet);
            }
           }

            catch (SqlException sqlException){
                details.WriteIntoLog(sqlException.Message);
            }
            return dataSet;
            
        }

        public void AcceptPolicyRequests(string id, string name, string type, string policyname, DateTime date, decimal amount, int duration)
        {
            decimal dueamount = amount / (2 * 10 * duration);
            DateTime duestartdatedt = date;
            string? duestartdate = duestartdatedt.ToString("dd-MM-yyyy");
            DateTime dueenddatedt = date.AddMonths(1);
            string? dueenddate = dueenddatedt.ToString("dd-MM-yyyy");
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Details.GetConnectionString();
                using (connection)
                {
                    connection.Open();
                    SqlCommand updateCommand = new SqlCommand("UpdateAcceptedRequests", connection);
                updateCommand.CommandType = CommandType.StoredProcedure;
                updateCommand.Parameters.AddWithValue("@EmployeeId", id);
                updateCommand.Parameters.AddWithValue("@Date", date);
                updateCommand.Parameters.AddWithValue("@Name",name);
                updateCommand.Parameters.AddWithValue("@PolicyName",policyname);
                updateCommand.Parameters.AddWithValue("@PremiumAmount",dueamount);
                updateCommand.Parameters.AddWithValue("@DueDateFrom",duestartdate);
                updateCommand.Parameters.AddWithValue("@DueDateTo",dueenddate);
                updateCommand.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }

        }

        public void DenyPolicyRequests(string id, DateTime date)
        {
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Details.GetConnectionString();
                using (connection)
                {

                    connection.Open();
                   SqlCommand updateCommand = new SqlCommand("UpdateDeniedRequests", connection);
                updateCommand.CommandType = CommandType.StoredProcedure;
                updateCommand.Parameters.AddWithValue("@EmployeeId", id);
                updateCommand.Parameters.AddWithValue("@Date", date);
                updateCommand.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }

        }

        public DataSet GetRequestsOfUser()
        {
            DataSet dataSet = new DataSet();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {
                connection.Open();
                SqlCommand selectcommand = new SqlCommand($"Select Help.id,Help.Request,Help.SentDate,Help.EmployeeId,Response.Username,Response.Reply,Response.ReplyDate from Help , Response where Response.id=Help.id", connection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectcommand);
                sqlDataAdapter.Fill(dataSet);
            }
            return dataSet;
        }

        public void ReplyAllQuery(Response response, int keyid)
        {
            DateTime replytime = DateTime.Now;
            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Details.GetConnectionString();
                using (connection)
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand($"update Response set Reply='{response.Reply}',ReplyDate=CAST(GetDate() AS DateTime2(7)) where Id='{keyid}'", connection);
                    command.ExecuteNonQuery();

                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }

        }
    }
}