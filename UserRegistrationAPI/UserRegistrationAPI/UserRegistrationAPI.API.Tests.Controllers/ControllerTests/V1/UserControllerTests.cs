using AutoMapper;
using UserRegistrationAPI.API.Controllers.V1;
using UserRegistrationAPI.API.DataContracts.Requests;
using UserRegistrationAPI.API.DataContracts;
using UserRegistrationAPI.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace UserRegistrationAPI.API.Tests.Controllers.ControllerTests.V1
{
    [TestClass]
    public class UserControllerTests : TestBase
    {
        //NOTE: should be replaced by an interface
        UserController _controller;

        public UserControllerTests() : base()
        {
            var businessService = _serviceProvider.GetRequiredService<IUserService>();
            var mapper = _serviceProvider.GetRequiredService<IMapper>();
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<UserController>();

            _controller = new UserController(businessService, mapper, logger);
        }

        [TestMethod]
        public async Task CreateUser_Nominal_OK()
        {
            //Simple test
            var user = await _controller.CreateUser(new UserCreationRequest
            {
                User = new User { ID = 1, givenName = "Firstname 1", sn = "Lastname 1", telephone="+998911322351" },
                Date = DateTime.Now
            });

            Assert.IsNotNull(user);
        }


    }
}
