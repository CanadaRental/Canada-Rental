using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OntarioGo.Entity;
using Microsoft.AspNetCore.Authorization;

namespace OntarioGo.Controllers
{
    public class PropertiesController : Controller
    {
        private string connectionString = "Server=ontariodb.mssql.somee.com;Database=ontariodb;User Id=ontariogo_SQLLogin_1;Password=rmf7zitki5;TrustServerCertificate=True;";

        
    
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId")?.Value;

            if (claim == null)
                throw new Exception("User not authenticated");

            return int.Parse(claim);
        }

      
        public IActionResult Index()
        {
            List<PropertyEntity> properties = new List<PropertyEntity>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Properties ORDER BY CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    properties.Add(new PropertyEntity
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
                        CreatedAt = (DateTime)reader["CreatedAt"],
                        Images = new List<PropertyImageEntity>()
                    });
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM PropertyImages";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int propertyId = (int)reader["PropertyId"];
                    var property = properties.FirstOrDefault(p => p.PropertyId == propertyId);

                    if (property != null)
                    {
                        byte[] imageBytes = (byte[])reader["ImageData"];
                        string base64 = Convert.ToBase64String(imageBytes);

                        property.Images.Add(new PropertyImageEntity
                        {
                            ImageId = (int)reader["ImageId"],
                            Base64Image = base64
                        });
                    }
                }
            }

            return View(properties);
        }

     
        public IActionResult Details(int id)
        {
            PropertyEntity property = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Properties WHERE PropertyId=@Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    property = new PropertyEntity
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
                        CreatedAt = (DateTime)reader["CreatedAt"],
                        Images = new List<PropertyImageEntity>()
                    };
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM PropertyImages WHERE PropertyId=@Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (property != null)
                    {
                        byte[] imageBytes = (byte[])reader["ImageData"];
                        string base64 = Convert.ToBase64String(imageBytes);

                        property.Images.Add(new PropertyImageEntity
                        {
                            ImageId = (int)reader["ImageId"],
                            Base64Image = base64
                        });
                    }
                }
            }

            return View(property);
        }

        
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(PropertyEntity model, List<IFormFile> images)
        {
            int ownerId = GetUserId();

            int propertyId = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                INSERT INTO Properties 
                (Title, Description, Price, City, Province, Address, PropertyType, Bedrooms, Bathrooms, OwnerId)
                OUTPUT INSERTED.PropertyId
                VALUES 
                (@Title, @Description, @Price, @City, @Province, @Address, @Type, @Bedrooms, @Bathrooms, @OwnerId)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Title", model.Title ?? "");
                cmd.Parameters.AddWithValue("@Description", model.Description ?? "");
                cmd.Parameters.AddWithValue("@Price", model.Price);
                cmd.Parameters.AddWithValue("@City", model.City ?? "");
                cmd.Parameters.AddWithValue("@Province", model.Province ?? "");
                cmd.Parameters.AddWithValue("@Address", model.Address ?? "");
                cmd.Parameters.AddWithValue("@Type", model.PropertyType ?? "");
                cmd.Parameters.AddWithValue("@Bedrooms", (object?)model.Bedrooms ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Bathrooms", (object?)model.Bathrooms ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OwnerId", ownerId);

                conn.Open();
                propertyId = (int)cmd.ExecuteScalar();
            }

            if (images != null && images.Count > 0)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (var image in images)
                    {
                        if (image.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            image.CopyTo(ms);

                            SqlCommand imgCmd = new SqlCommand(@"
                                INSERT INTO PropertyImages (PropertyId, ImageData)
                                VALUES (@PropertyId, @ImageData)", conn);

                            imgCmd.Parameters.AddWithValue("@PropertyId", propertyId);
                            imgCmd.Parameters.AddWithValue("@ImageData", ms.ToArray());

                            imgCmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Edit(int id)
        {
            PropertyEntity property = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Properties WHERE PropertyId=@Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    property = new PropertyEntity
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
                        OwnerId = (int)reader["OwnerId"]
                    };
                }
            }

            return View(property);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string[] queries = new string[]
                    {
                "DELETE FROM PropertyImages WHERE PropertyId=@Id",
                "DELETE FROM Favorites WHERE PropertyId=@Id",
                "DELETE FROM Messages WHERE PropertyId=@Id",
                "DELETE FROM PropertyViews WHERE PropertyId=@Id",
                "DELETE FROM Reviews WHERE PropertyId=@Id"
                    };

                    foreach (var query in queries)
                    {
                        SqlCommand cmd = new SqlCommand(query, conn, transaction);
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }

                    SqlCommand deleteProperty = new SqlCommand(
                        "DELETE FROM Properties WHERE PropertyId=@Id",
                        conn,
                        transaction
                    );

                    deleteProperty.Parameters.AddWithValue("@Id", id);
                    deleteProperty.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return RedirectToAction("Index");
        }
    }
}