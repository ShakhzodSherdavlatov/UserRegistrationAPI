using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;

using DbTools.DataBase;
using DbTools.DataBase.Interfaces;

using Microsoft.Extensions.Options;

using UserRegistrationAPI.API.DataContracts;
using UserRegistrationAPI.API.DataContracts.Settings;

namespace UserRegistration.Services
{
    public class UserService : IUserService
    {
        private AppSettings _settings;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private Repository<User> _repository;

        public UserService(IOptions<AppSettings> settings, IMapper mapper, IUnitOfWork uow)
        {
            _settings = settings?.Value;
            _mapper = mapper;
            _uow = uow;

            _repository = _uow.Factory<User>();
        }

        public async Task<User> CreateAsync(User user)
        {
            var res = await _repository.AddAsync(user);
            await _uow.SaveAsync().ConfigureAwait(false);
            return res;
        }

        public async Task<User> UpdateAsync(User user)
        {
            var res = await _repository.UpdateAsync(user);
            await _uow.SaveAsync().ConfigureAwait(false);
            return res;
        }

        public async Task<User> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<User> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<List<User>> GetAllUsers(string searchText = "")
        {
            return (List<User>)await _repository
                .FindAsync(u => string.IsNullOrWhiteSpace(searchText)
                                || (u.sn.Contains(searchText)
                                    || u.givenName.Contains(searchText)
                                    || u.telephoneNumber.Contains(searchText)));
        }
    }
}