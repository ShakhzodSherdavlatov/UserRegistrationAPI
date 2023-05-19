using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using ErrorsLibrary.Errors;
using ErrorsLibrary.ErrorService;
using MicroServicesCommonTools.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Minio.Exceptions;
using Newtonsoft.Json;
using UserRegistration.API.Utils;
using UserRegistrationAPI.API.DataContracts.Settings;
using Error = ErrorsLibrary.Errors.Error;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace UserRegistration.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILog _logger;
        private readonly AppSettings _settings;
        private readonly Regex regex = new Regex("<\\w*>");

        public ExceptionMiddleware(RequestDelegate next, ILog logger, AppSettings settings = default)
        {
            _logger = logger;
            _next = next;
            _settings = settings ?? new AppSettings();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            #region ExceptionTypeHandlers

            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                //await _actionLogsService.Create(context.User.GetUserParameters().Tin, null, "Ошибка",
                //    ActionType.Error, ex.Entries.FirstOrDefault().Entity.ToString(),
                //    null, $"{ex.Message}");

                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (SqlException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (ArgumentNullException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (AlreadyExistsException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (NullArgumentException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (InvalidFormatException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (IncorrectCredentialException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (DoesNotExistException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (Error avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (GeneralException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (NullReferenceException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (DivideByZeroException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (FileNotFoundException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (FormatException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (IndexOutOfRangeException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (KeyNotFoundException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (NotSupportedException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (OverflowException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (OutOfMemoryException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (StackOverflowException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (TimeoutException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (ObjectNotFoundException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (JsonReaderException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (JsonWriterException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (JsonSerializationException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (JsonException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (AutoMapperMappingException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (System.Text.Json.JsonException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (AggregateException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (AmbiguousMatchException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (NotImplementedException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (ApiException avEx)
            {
                await HandleExceptionAsync(context, avEx).ConfigureAwait(false);
            }
            catch (IOException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (ApplicationException ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }

            #endregion

            finally
            {
                if ( !context.Request.Path.Value.Contains("swagger.json") &&
                     !context.Request.Path.Value.Contains("index.html") )
                    _logger.Information($"{LogHandlerBuilder(context)}");
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception,
            AppSettings settings = default)
        {
            if ( settings is null )
                settings = _settings;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var log = new StringBuilder();
            log.AppendLine($"{DateTime.UtcNow.ToLongDateString()}{DateTime.UtcNow.ToLongTimeString()} :");
            log.AppendLine($"|{"Error",-8}| {exception.Message}");
            log.AppendLine($"|{"Source",-8}| {settings?.ServiceName} - {exception.Source}");
            log.AppendLine($"|{"Object",-8}| {exception?.TargetSite?.DeclaringType?.Name}");
            log.AppendLine(
                $"|{"Method",-8}| {context.Request?.Method,-4}{context.Request?.Path.Value}|{exception?.TargetSite?.Name}");
            log.AppendLine($"|{"Query",-8}| {context.Request.QueryString}");


            _logger.Error($"{log}");

            var error = exception?.GetType().GetTypeInfo().GetDeclaredField("ErrorInfo")?.GetValue(exception);
            await context.Response.WriteAsync(JsonSerializer.Serialize(error ?? new ErrorModel
            {
                Code = context.Response.StatusCode,
                Description = $"{exception?.Message} {exception?.InnerException}",
                Name = "Internal Server Error",
                Params = new[]
                {
                    $"Source: {_settings.ServiceName} - {exception?.Source}",
                    $"Object: {exception?.TargetSite?.DeclaringType?.Name}",
                    $"Method: {context.Request?.Method,-4}{context.Request?.Path.Value}|{exception?.TargetSite?.Name}",
                    $"Query: {context.Request.QueryString}"
                }
            })).ConfigureAwait(false);
        }

        private StringBuilder LogHandlerBuilder(HttpContext context)
        {
            var log = new StringBuilder();
            log.AppendLine($"{DateTime.UtcNow.ToLongDateString()}{DateTime.UtcNow.ToLongTimeString()} :");
            log.AppendLine($"|{"Response",-8}| {context.Response?.StatusCode} ");
            log.AppendLine($"|{"TIN",-8}| {context.User.GetUserParameters().Tin}");
            //log.AppendLine($"|{"Headers",-8}| \n{string.Join(Environment.NewLine, context.Request.Headers.ToList())}");
            log.AppendLine($"|{"IP",-8}| {context.Connection.RemoteIpAddress}: {context.Connection.RemotePort,-6}");
            log.AppendLine($"|{"Query",-8}| {context.Request.QueryString}");
            log.AppendLine($"|{"Body",-8}| {context.Response.Body}");

            return log;
        }

        private StringBuilder ExceptionHandlerBuilder(HttpContext context, Exception exception, string message,
            string objName, AppSettings settings = default, StringBuilder log = default)
        {
            log ??= LogHandlerBuilder(context);

            log.AppendLine($"{new string('-', 15)} Start Error {new string('-', 15)}");
            log.AppendLine($"|{"Message",-8}| {message}");
            log.AppendLine($"|{"Source",-8}| {settings?.ServiceName} - {exception.Source}");
            log.AppendLine($"|{"Object",-8}| {objName}");
            log.AppendLine($"|{"Method",-8}| {context.Request?.Method,-3} {context.Request?.Path.Value}" +
                           $"| {exception?.TargetSite?.Name}");
            log.AppendLine($"{new string('-', 15)} End Error {new string('-', 15)}");

            return log;
        }
    }
}