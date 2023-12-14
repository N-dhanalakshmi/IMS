using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace IMS.Models
{
    public interface IClaimPolicyDI
    {
        List<ClaimNow> ReadClaimRequest();

    }
    public class ClaimClass : IClaimPolicyDI
    {



        public List<ClaimNow> ReadClaimRequest()
        {
            List<ClaimNow> claim=new List<ClaimNow>();
            SqlConnection connection = new SqlConnection
            {
                ConnectionString = Details.GetConnectionString()
            };
            using (connection)
            {
                connection.Open();
                SqlCommand selectcommand = new SqlCommand($"Select * from ClaimNow ", connection);
                SqlDataReader sdr = selectcommand.ExecuteReader();
                while (sdr.Read())
                {
                    ClaimNow claimNow = new ClaimNow();
                    claimNow.EmployeeId = Convert.ToString(sdr["EmployeeId"]);
                    claimNow.Name=Convert.ToString(sdr["Name"]);
                    claimNow.PolicyName=Convert.ToString(sdr["PolicyName"]);
                    claimNow.PolicyAmount = Convert.ToDecimal(sdr["PolicyAmount"]);
                    claimNow.PolicyDuration = Convert.ToInt32(sdr["PolicyDuration"]);
                    claimNow.Reason=Convert.ToString(sdr["Reason"]);
                    claimNow.MedicalProof=Encoding.ASCII.GetBytes(Convert.ToString(sdr["MedicalProof"]));
                    claimNow.Receipt=Encoding.ASCII.GetBytes(Convert.ToString(sdr["Receipt"]));
                    claim.Add(claimNow);
                }
            
            }
            return claim;

        }

    }

    public abstract class LogClass{
        public abstract void WriteIntoLog(string? logText);
        public abstract void SendMail(string? from,string? recipient,string? subject,string? body); 
    }
}