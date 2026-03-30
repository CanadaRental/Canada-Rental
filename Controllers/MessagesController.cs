using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OntarioGo.Entity;
using Microsoft.AspNetCore.Authorization;

namespace OntarioGo.Controllers
{
    public class MessagesController : Controller
    {
        private string connectionString = "Server=ontariodb.mssql.somee.com;Database=ontariodb;User Id=ontariogo_SQLLogin_1;Password=rmf7zitki5;TrustServerCertificate=True;";

        
        private int? GetUserId()
        {
            var claim = User.FindFirst("UserId")?.Value;
            return claim != null ? int.Parse(claim) : (int?)null;
        }

        private string GetUserEmail()
        {
            return User.Identity?.Name ?? "";
        }

     
        public IActionResult Send(int propertyId)
        {
            ViewBag.PropertyId = propertyId;
            return View();
        }


        [HttpPost]
        public IActionResult Send(int propertyId, string messageText)
        {
            int? senderId = GetUserId();          
            string senderEmail = GetUserEmail(); 

            messageText = messageText ?? "";     

            int ownerId = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

               
                string getOwnerQuery = "SELECT OwnerId FROM Properties WHERE PropertyId=@PropertyId";
                SqlCommand ownerCmd = new SqlCommand(getOwnerQuery, conn);
                ownerCmd.Parameters.AddWithValue("@PropertyId", propertyId);

                object result = ownerCmd.ExecuteScalar();
                if (result != null)
                {
                    ownerId = (int)result;
                }

                
                string insertMessageQuery = @"
                INSERT INTO Messages 
                (PropertyId, SenderId, SenderEmail, MessageText, SentAt) 
                VALUES (@PropertyId, @SenderId, @SenderEmail, @MessageText, GETDATE())";

                SqlCommand msgCmd = new SqlCommand(insertMessageQuery, conn);

                msgCmd.Parameters.AddWithValue("@PropertyId", propertyId);
                msgCmd.Parameters.AddWithValue("@SenderId", (object?)senderId ?? DBNull.Value);
                msgCmd.Parameters.AddWithValue("@SenderEmail", senderEmail ?? "");
                msgCmd.Parameters.AddWithValue("@MessageText", messageText);

                msgCmd.ExecuteNonQuery();

               
                if (ownerId != 0)
                {
                    string notificationQuery = @"
                    INSERT INTO Notifications (UserId, Message, IsRead, CreatedAt)
                    VALUES (@UserId, @Message, 0, GETDATE())";

                    SqlCommand notifCmd = new SqlCommand(notificationQuery, conn);

                    notifCmd.Parameters.AddWithValue("@UserId", ownerId);
                    notifCmd.Parameters.AddWithValue("@Message", "You received a new message on your property");

                    notifCmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Details", "Properties", new { id = propertyId });
        }

    
        [Authorize] 
        public IActionResult Inbox()
        {
            List<MessageEntity> messages = new List<MessageEntity>();

            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (userIdClaim == null)
                return RedirectToAction("Login", "Auth");

            int ownerId = int.Parse(userIdClaim);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT m.*, p.Title 
                FROM Messages m
                INNER JOIN Properties p ON m.PropertyId = p.PropertyId
                WHERE p.OwnerId = @OwnerId
                ORDER BY m.SentAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OwnerId", ownerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    messages.Add(new MessageEntity
                    {
                        MessageId = (int)reader["MessageId"],
                        PropertyId = (int)reader["PropertyId"],
                        SenderId = reader["SenderId"] == DBNull.Value ? null : (int?)reader["SenderId"],
                        SenderEmail = reader["SenderEmail"]?.ToString(),
                        MessageText = reader["MessageText"]?.ToString(),
                        SentAt = (DateTime)reader["SentAt"]
                    });
                }
            }

            return View(messages);
        }
    }
}