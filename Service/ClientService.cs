using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanNGo_Backend.Model;
using WebAppApi13.Data;

namespace PlanNGo_Backend.Service
{
    public class ClientService
    {
        private readonly ApplicationDbContext _context;

        public async Task<bool> UpdateClientById(Client client)
        {
            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return ClientExists(client.ClientId);
            }
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }
    }

}
