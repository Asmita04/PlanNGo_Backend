using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanNGo_Backend.Model;
using PlanNGo_Backend.Service;
using System.Security.Claims;

using WebAppApi13.Data;

namespace PlanNGo_Backend.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ClientService _clientService = new ClientService();
        public ClientsController(ApplicationDbContext context, ClientService clientService)
        {
            _clientService = clientService;
            _context = context;
        }

        // GET: api/Clients
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.Clients.ToListAsync();
        }

        // GET: api/Clients/5
        [Authorize(Roles = "admin,client")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        // PUT: api/Clients/5
        [Authorize(Roles ="client")]
        [HttpPut("ById")]
        public async Task<IActionResult> UpdateClientById(Client client)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId != client.UserId)
            {
                return BadRequest();
            }

            if(await _clientService.UpdateClientById(client)) {     
                return NoContent();
            }
            else
            {
                return NotFound();
            }


        }

        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClient", new { id = client.ClientId }, client);
        }

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

       
    }
}
