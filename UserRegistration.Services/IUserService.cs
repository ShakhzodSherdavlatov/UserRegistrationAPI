using System.Collections.Generic;
using System.Threading.Tasks;

using UserRegistrationAPI.API.DataContracts;

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
