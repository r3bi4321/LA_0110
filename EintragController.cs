using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;


namespace API_Webjournal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EintragController : ControllerBase
    {
        private readonly ILogger<EintragController> _logger;
        private readonly JournalDbContext _dbContext;

        public EintragController(ILogger<EintragController> logger)
        {
            _logger = logger;
            _dbContext = new JournalDbContext("mongodb://localhost:27017"); 
        }

        //get old entries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JournalEntry>>> GetAll()
        {
            var entries = await _dbContext.JournalEntries.Find(new BsonDocument()).ToListAsync();
            return Ok(entries);
        }

        // get by id 
        [HttpGet("{id}")]
        public async Task<ActionResult<JournalEntry>> GetById(string id)
        {
            var entry = await _dbContext.JournalEntries.Find(e => e.Id == new ObjectId(id)).FirstOrDefaultAsync();
            if (entry == null)
            {
                return NotFound();
            }
            return Ok(entry);
        }

        // post 

        [HttpPost]
        public async Task<ActionResult<JournalEntry>> Create([FromBody] JournalEntry newEntry)
        {
            if (newEntry == null || string.IsNullOrWhiteSpace(newEntry.Titel) || string.IsNullOrWhiteSpace(newEntry.Beitrag))
            {
                return BadRequest("Titel und Beitrag sind erforderlich.");
            }

            newEntry.CreatedAt = DateTime.UtcNow;
            await _dbContext.JournalEntries.InsertOneAsync(newEntry);
            return CreatedAtAction(nameof(GetById), new { id = newEntry.Id.ToString() }, newEntry);
        }


        // update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] JournalEntry updatedEntry)
        {
            if (updatedEntry == null || string.IsNullOrWhiteSpace(updatedEntry.Titel) || string.IsNullOrWhiteSpace(updatedEntry.Beitrag))
            {
                return BadRequest("Titel und Beitrag sind erforderlich.");
            }

            var entry = await _dbContext.JournalEntries.Find(e => e.Id == new ObjectId(id)).FirstOrDefaultAsync();
            if (entry == null)
            {
                return NotFound();
            }

            updatedEntry.Id = entry.Id; 
            updatedEntry.CreatedAt = entry.CreatedAt; 

            await _dbContext.JournalEntries.ReplaceOneAsync(e => e.Id == entry.Id, updatedEntry);
            return NoContent(); 
        }

        // delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _dbContext.JournalEntries.DeleteOneAsync(e => e.Id == new ObjectId(id));
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent(); 
        }
    }


public class JournalDbContext
    {
        private readonly IMongoDatabase _database;

        public JournalDbContext(string connectionString)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("Journal"); 
        }

        public IMongoCollection<JournalEntry> JournalEntries => _database.GetCollection<JournalEntry>("Journaleinträge");
    }

    public class JournalEntry
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Login_ID { get; set; }
        public string Titel { get; set; }
        public string Beitrag { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
