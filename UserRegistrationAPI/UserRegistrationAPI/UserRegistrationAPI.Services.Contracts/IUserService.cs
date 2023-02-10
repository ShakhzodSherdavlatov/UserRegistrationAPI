using UserRegistrationAPI.Services.Model;
using System.Threading.Tasks;

namespace UserRegistrationAPI.Services.Contracts
{
    public interface IUserService
    {
        Task<User> CreateAsync(User user);

        Task<User> UpdateAsync(User user);

        Task<bool> DeleteAsync(int id);

        Task<User> GetAsync(int id);
    }
}
