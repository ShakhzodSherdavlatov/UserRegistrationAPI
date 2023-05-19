using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserRegistration.Services;
using UserRegistrationAPI.API.DataContracts;

namespace UserRegistration.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService service, IMapper mapper, ILogger<UserController> logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }

        #region GET

        /// <summary>
        /// Returns a user entity according to the provided Id.
        /// </summary>
        /// <remarks>
        /// XML comments included in controllers will be extracted and injected in Swagger/OpenAPI file.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns>
        /// Returns a user entity according to the provided Id.
        /// </returns>
        /// <response code="201">Returns the newly created item.</response>
        /// <response code="204">If the item is null.</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(User))]
        [HttpGet("{id}")]
        public async Task<User> Get(int id)
        {
            _logger.LogDebug($"UserControllers::Get::{id}");

            var data = await _service.GetAsync(id).ConfigureAwait(false);

            if ( data != null )
                return _mapper.Map<User>(data);
            else
                return null;
        }

        [HttpGet]
        public async Task<List<User>> Get(string searchText)
        {
            _logger.LogDebug($"UserControllers::Get::{searchText}");

            var data = await _service.GetAllUsers(searchText);

            if ( data != null )
                return data;
            else
                return null;
        }

        #endregion

        #region POST

        /// <summary>
        /// Creates a user.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="value"></param>
        /// <returns>A newly created user.</returns>
        /// <response code="201">Returns the newly created item.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(User))]
        public async Task<User> CreateUser([FromBody] User value)
        {
            _logger.LogDebug($"UserControllers::Post::");

            if ( value == null )
                throw new ArgumentNullException("value");


            var data = await _service.CreateAsync(value);

            if ( data != null )
                return _mapper.Map<User>(data);
            else
                return null;
        }

        #endregion

        #region PUT

        /// <summary>
        /// Updates an user entity.
        /// </summary>
        /// <remarks>
        /// No remarks.
        /// </remarks>
        /// <param name="parameter"></param>
        /// <returns>
        /// Returns a boolean notifying if the user has been updated properly.
        /// </returns>
        /// <response code="200">Returns a boolean notifying if the user has been updated properly.</response>
        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<User> UpdateUser(User parameter)
        {
            if ( parameter == null )
                throw new ArgumentNullException("parameter");

            return await _service.UpdateAsync(parameter);
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Deletes an user entity.
        /// </summary>
        /// <remarks>
        /// No remarks.
        /// </remarks>
        /// <param name="id">User Id</param>
        /// <returns>
        /// Boolean notifying if the user has been deleted properly.
        /// </returns>
        /// <response code="200">Boolean notifying if the user has been deleted properly.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<User> DeleteDevice(int id)
        {
            return await _service.DeleteAsync(id);
        }

        #endregion

        #region Exceptions

        [HttpGet("exception/{message}")]
        [ProducesErrorResponseType(typeof(Exception))]
        public async Task RaiseException(string message)
        {
            _logger.LogDebug($"UserControllers::RaiseException::{message}");

            throw new Exception(message);
        }

        #endregion
    }
}