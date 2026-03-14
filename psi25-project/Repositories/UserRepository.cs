using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;

namespace psi25_project.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly GeoHuntContext _context;

        public UserRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(Guid id)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
