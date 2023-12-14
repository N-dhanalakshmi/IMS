using System.Data;
using System.Data.SqlClient;

namespace IMS.Models
{
    public class EmployeeFile
    {
        Details details=new Details();
        public void InsertIntoPolicyRequests(Policy policy)
        {
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Details.GetConnectionString();
                using (connection)
                {

                    connection.Open();
                    SqlCommand command1 = new SqlCommand($"insert into PolicyRequests values('{policy.EmployeeId}','{policy.Name}','{policy.Type}','{policy.PolicyName}',CAST('{policy.PolicyDate}' AS DateTime2(7)),'{policy.PolicyAmount}','{policy.PolicyDuration}','Requested')", connection);
                    command1.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }
        }
        public void RemovePolicyRequests(string empid, DateTime time)
        {
            try
            {
                Console.WriteLine(time);
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Details.GetConnectionString();
                using (connection)
                {
                  connection.Open();
                    SqlCommand deletecommand1 = new SqlCommand("DeletePolicy", connection);
                    deletecommand1.CommandType = CommandType.StoredProcedure;
                    deletecommand1.Parameters.AddWithValue("@EmployeeId", empid);
                    deletecommand1.Parameters.AddWithValue("@Date", time);
                    deletecommand1.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlException)
            {
                details.WriteIntoLog(sqlException.Message);
            }


        }
        public void AddDuesFromPolicyAccept(string? id, string? name, string? type, string? policyname, DateTime date, decimal amount, int duration)
        {
           
            decimal dueamount = amount / (2 * 10 * duration);
            DateTime duestartdatedt = date;
            string? duestartdate = duestartdatedt.ToString("dd-MM-yyyy");
            DateTime dueenddatedt = date.AddMonths(1);
            string? dueenddate = dueenddatedt.ToString("dd-MM-yyyy");
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {

                connection.Open();
                SqlCommand insertcommand1 = new SqlCommand($"insert into Dues values('{id}','{name}','{policyname}','{date}','{dueamount}','{duestartdate}','{dueenddate}','UnPaid') ", connection);
                insertcommand1.ExecuteNonQuery();
            }
        }



        public DataSet GetAllDues(string empid)
        {

            DataSet dataSet = new DataSet();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {

                connection.Open();

                SqlCommand selectcommand = new SqlCommand("SelectDues", connection);
                selectcommand.CommandType = CommandType.StoredProcedure;
                selectcommand.Parameters.AddWithValue("@EmployeeId", empid);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectcommand);
                sqlDataAdapter.Fill(dataSet);
            }
            return dataSet;
        }

        public bool CheckClaim(ClaimNow claim)
        {
            int count = 0;
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {
                connection.Open();
                SqlCommand select = new SqlCommand("ClaimCount", connection);
                select.CommandType = CommandType.StoredProcedure;
                select.Parameters.AddWithValue("@EmployeeId", claim.EmployeeId);
                select.Parameters.AddWithValue("@Date", claim.PolicyDate);
                count = Convert.ToInt32(select.ExecuteScalar());
            }
            if (count >= 1)
                return true;
            else return false;
        }
        public void DoAfterClaim(ClaimNow claim)
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {
                connection.Open();
                SqlCommand updateCommand = new SqlCommand("UpdateClaim", connection);
                updateCommand.CommandType = CommandType.StoredProcedure;
                updateCommand.Parameters.AddWithValue("@EmployeeId", claim.EmployeeId);
                updateCommand.Parameters.AddWithValue("@Date", claim.PolicyDate);
                updateCommand.ExecuteNonQuery();
            }
        }

        public DataSet GetQueryResponsesofuserid(string user)
        {
            DataSet dataSet1 = new DataSet();
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {
                connection.Open();
                SqlCommand selectcommand = new SqlCommand($"Select Help.id,Help.Request,Help.SentDate,Help.EmployeeId,Response.Username,Response.Reply,Response.ReplyDate from Help , Response where Help.Username='{user}' AND Response.Username='{user}' and Help.id=Response.id", connection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectcommand);
                sqlDataAdapter.Fill(dataSet1);
            }
            return dataSet1;
        }

        public void InsertIntoResponseFromHelp(string user, string id, int helpid)
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {
                connection.Open();
               
                SqlCommand insertcommand = new SqlCommand($"insert into Response values('{user}','{id}','','',{helpid})", connection);
                insertcommand.ExecuteNonQuery();
            }

        }


        public void UnsendQuerySent(string id, int keyid)
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = Details.GetConnectionString();
            using (connection)
            {

                connection.Open();
               
                SqlCommand deletecommand1 = new SqlCommand($"delete  from Help  where EmployeeId='{id}' and id={keyid}", connection);
                deletecommand1.ExecuteNonQuery();
                SqlCommand deletecommand2 = new SqlCommand($"delete  from Response  where EmployeeId='{id}' and id={keyid}", connection);
                deletecommand2.ExecuteNonQuery();
            }
        }


    }
}