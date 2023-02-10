using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRegistration.Services.Model;

namespace UserRegistration.Services
{
    public interface IUserService
    {
        public Task<User> CreateAsync(User user);
        public Task<User> UpdateAsync(User user);
        public Task<User> DeleteAsync(int id);
        public Task<User> GetAsync(int id);
        public Task<List<User>> GetAllUsers(string searchText);
    }
}
