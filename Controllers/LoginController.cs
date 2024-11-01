using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;


namespace API_Webjournal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; } 
        public string Login_ID { get; set; } 
        public string Name { get; set; }      
        public string Passwort { get; set; }  
    }
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly UserDbContext _dbContext;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
            _dbContext = new UserDbContext("mongodb://localhost:27017"); 
        }

        //Login
        [HttpPost("login", Name = "LoginUser")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
     
            var user = await _dbContext.Users.Find(u => u.Name == loginRequest.Username).FirstOrDefaultAsync();
            if (user != null && user.Passwort == loginRequest.Password)
            {
                return Ok("Login successful");
            }

            return Unauthorized();
        }

        //Create User
        [HttpPost("create", Name = "CreateUser")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest createUserRequest)
        {
            var existingUser = await _dbContext.Users.Find(u => u.Name == createUserRequest.Username).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return Conflict("User already exists");
            }

            var newUser = new User
            {
                Login_ID = ObjectId.GenerateNewId().ToString(), 
                Name = createUserRequest.Username,
                Passwort = createUserRequest.Password
            };

            await _dbContext.Users.InsertOneAsync(newUser);
            return CreatedAtAction(nameof(Login), new { username = newUser.Name }, "User created successfully");
        }

        //Delete Account
        [HttpDelete("delete/{username}", Name = "DeleteUser")]
        public async Task<IActionResult> Delete(string username)
        {
            var result = await _dbContext.Users.DeleteOneAsync(u => u.Name == username);
            if (result.DeletedCount == 0)
            {
                return NotFound("User not found");
            }

            return Ok("User deleted successfully");
        }

        public class UserDbContext
        {
            private readonly IMongoDatabase _database;

            public UserDbContext(string connectionString)
            {
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase("Journal"); 
            }

            public IMongoCollection<User> Users => _database.GetCollection<User>("Login"); 
        }


    }
}

