using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OntarioGo.Entity;
using Microsoft.AspNetCore.Authorization;

namespace OntarioGo.Controllers
{
    [Authorize] 
    public class FavoritesController : Controller
    {
        private string connectionString = "Server=ontariodb.mssql.somee.com;Database=ontariodb;User Id=ontariogo_SQLLogin_1;Password=rmf7zitki5;TrustServerCertificate=True;";

       
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId")?.Value;

            if (claim == null)
                throw new Exception("User not authenticated");

            return int.Parse(claim);
        }

      
        
        public IActionResult Add(int propertyId)
        {
            int userId = GetUserId(); 

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM Favorites WHERE UserId=@UserId AND PropertyId=@PropertyId";

                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@UserId", userId);
                checkCmd.Parameters.AddWithValue("@PropertyId", propertyId);

                conn.Open();
                int exists = (int)checkCmd.ExecuteScalar();
                conn.Close();

                if (exists == 0)
                {
                    string insertQuery = @"INSERT INTO Favorites (UserId, PropertyId, CreatedAt)
                                           VALUES (@UserId, @PropertyId, GETDATE())";

                    SqlCommand cmd = new SqlCommand(insertQuery, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Details", "Properties", new { id = propertyId });
        }


        public IActionResult Index()
        {
            List<PropertyEntity> favorites = new List<PropertyEntity>();

            int userId = GetUserId(); 
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT p.*
                FROM Favorites f
                INNER JOIN Properties p ON f.PropertyId = p.PropertyId
                WHERE f.UserId = @UserId
                ORDER BY f.CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    favorites.Add(new PropertyEntity
                    {
                        PropertyId = (int)reader["PropertyId"],
                        Title = reader["Title"]?.ToString(),
                        Description = reader["Description"]?.ToString(),
                        Price = (decimal)reader["Price"],
                        City = reader["City"]?.ToString(),
                        Province = reader["Province"]?.ToString(),
                        Address = reader["Address"]?.ToString(),
                        PropertyType = reader["PropertyType"]?.ToString(),
                        Bedrooms = reader["Bedrooms"] == DBNull.Value ? null : (int?)reader["Bedrooms"],
                        Bathrooms = reader["Bathrooms"] == DBNull.Value ? null : (int?)reader["Bathrooms"],
                        IsAvailable = (bool)reader["IsAvailable"],
                        OwnerId = (int)reader["OwnerId"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
            }

            return View(favorites);
        }

    
     
        public IActionResult Remove(int propertyId)
        {
            int userId = GetUserId();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Favorites WHERE UserId=@UserId AND PropertyId=@PropertyId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@PropertyId", propertyId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}