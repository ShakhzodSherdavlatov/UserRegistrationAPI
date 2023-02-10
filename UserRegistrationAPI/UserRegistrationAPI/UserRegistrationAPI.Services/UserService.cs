using AutoMapper;
using UserRegistrationAPI.API.Common.Settings;
using UserRegistrationAPI.Services.Model;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using DbTools.DataBase.Interfaces;
using DbTools.DataBase;
using System.Collections.Generic;

namespace UserRegistrationAPI.Services
{
    public class UserService : IUserService
    {
        private AppSettings _settings;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private Repository<User> _usersinfo;

        public UserService(IOptions<AppSettings> settings, IMapper mapper, IUnitOfWork uow)
        {
            _settings = settings?.Value;
            _mapper = mapper;
            _uow = uow;

            _usersinfo = _uow.Factory<User>();
        }

        public async Task<User> CreateAsync(User user)
        {
            var res = await _usersinfo.AddAsync(user);
            await _uow.SaveAsync();
            return res;
        }

        public async Task<User> UpdateAsync(User user)
        {
            var res = await _usersinfo.UpdateAsync(user);
            await _uow.SaveAsync();
            return res;
        }

        public async Task<User> DeleteAsync(int id)
        {
            return  await _usersinfo.DeleteAsync(id);

        }

        public async Task<User> GetAsync(int id)
        {
            return await _usersinfo.GetAsync(id);
        }

        public async Task<List<User>> GetAllUsers(string searchText="")
        {
            return (List<User>)await _usersinfo.FindAsync(u=>string.IsNullOrWhiteSpace(searchText)?true: u.sn.Contains(searchText) || u.givenName.Contains(searchText) || u.telephoneNumber.Contains(searchText));

        }
    }
}
