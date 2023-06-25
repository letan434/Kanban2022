using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanbanApp.BackendServer.Authorization;
using KanbanApp.BackendServer.Constants;
using KanbanApp.BackendServer.Data;
using KanbanApp.BackendServer.Data.Entities;
using KanbanApp.BackendServer.Helpers;
using KanbanApp.ViewModels;
using KanbanApp.ViewModels.Systems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KanbanApp.BackendServer.Controllers
{
    public class FunctionsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FunctionsController> _logger;

        public FunctionsController(ApplicationDbContext context, ILogger<FunctionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostFunction([FromBody] FunctionCreateRequest request)
        {
            _logger.LogInformation("Begin postFunction API");
            var dbFunction = await _context.Functions.FindAsync(request.Id);
            if (dbFunction != null)
                return BadRequest(new ApiBadRequestResponse($"Function with id {request.Id} is existed."));

            var function = new Function()
            {
                Id = request.Id,
                Name = request.Name,
                ParentId = request.ParentId,
                SortOrder = request.SortOrder,
                Url = request.Url,
                Icon = request.Icon
            };
            _context.Functions.Add(function);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("End PostFunction API - Success");
                return CreatedAtAction(nameof(GetById), new { id = function.Id }, request);
            }
            else
            {
                _logger.LogInformation("End PostFunction API - Failed");
                return BadRequest(new ApiBadRequestResponse("Create function is failed"));
            }
        }

        [HttpGet]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctions()
        {
            var functions = _context.Functions;

            var functionvms = await functions.Select(u => new FunctionVm()
            {
                Id = u.Id,
                Name = u.Name,
                Url = u.Url,
                SortOrder = u.SortOrder,
                ParentId = u.ParentId,
                Icon = u.Icon
            }).ToListAsync();

            return Ok(functionvms);
        }

        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctionsPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.Functions.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter)
                || x.Id.Contains(filter)
                || x.Url.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(u => new FunctionVm()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Url = u.Url,
                    SortOrder = u.SortOrder,
                    ParentId = u.ParentId,
                    Icon = u.Icon
                })
                .ToListAsync();

            var pagination = new Pagination<FunctionVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found function with id {id}"));

            var functionVm = new FunctionVm()
            {
                Id = function.Id,
                Name = function.Name,
                Url = function.Url,
                SortOrder = function.SortOrder,
                ParentId = function.ParentId,
                Icon = function.Icon
            };
            return Ok(functionVm);
        }

        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutFunction(string id, [FromBody] FunctionCreateRequest request)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found function with id {id}"));

            function.Name = request.Name;
            function.ParentId = request.ParentId;
            function.SortOrder = request.SortOrder;
            function.Url = request.Url;
            function.Icon = request.Icon;
            _context.Functions.Update(function);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse("Create function failed"));
        }

        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteFunction(string id)
        {
            var function =  _context.Functions.Where(x => x.Id == id || x.ParentId == id);
            if (function == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found function with id {id}"));

            _context.Functions.RemoveRange(function);
            var commands = _context.CommandInFunctions.Where(x => x.FunctionId == id);
            _context.CommandInFunctions.RemoveRange(commands);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
              
                return Ok();
            }
            return BadRequest(new ApiBadRequestResponse("Delete function failed"));
        }

        [HttpGet("{functionId}/commands")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommantsInFunction(string functionId)
        {
            var query = from a in _context.Commands
                        join cif in _context.CommandInFunctions on a.Id equals cif.CommandId into result1
                        from commandInFunction in result1.DefaultIfEmpty()
                        join f in _context.Functions on commandInFunction.FunctionId equals f.Id into result2
                        from function in result2.DefaultIfEmpty()
                        select new
                        {
                            a.Id,
                            a.Name,
                            commandInFunction.FunctionId
                        };

            query = query.Where(x => x.FunctionId == functionId);

            var data = await query.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        //[HttpGet("{functionId}/commands/not-in-function")]
        //[ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        //public async Task<IActionResult> GetCommantsNotInFunction(string functionId)
        //{
        //    var query = from a in _context.Commands
        //                join cif in _context.CommandInFunctions on a.Id equals cif.CommandId into result1
        //                from commandInFunction in result1.DefaultIfEmpty()
        //                join f in _context.Functions on commandInFunction.FunctionId equals f.Id into result2
        //                from function in result2.DefaultIfEmpty()
        //                select new
        //                {
        //                    a.Id,
        //                    a.Name,
        //                    commandInFunction.FunctionId
        //                };

        //    query = query.Where(x => x.FunctionId != functionId).Distinct();

        //    var data = await query.Select(x => new CommandVm()
        //    {
        //        Id = x.Id,
        //        Name = x.Name
        //    }).ToListAsync();

        //    return Ok(data);
        //}

        [HttpPost("{functionId}/commands")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostCommandToFunction(string functionId, [FromQuery] CommandAssignRequest request)
        {
            //var commandInFunction = await _context.CommandInFunctions.FindAsync(request.CommandId, request.FunctionId);
            //if (commandInFunction != null)
            //    return BadRequest(new ApiBadRequestResponse($"This command has been added to function"));

            //var entity = new CommandInFunction()
            //{
            //    CommandId = request.CommandId,
            //    FunctionId = request.FunctionId
            //};
            foreach (var commandId in request.CommandIds)
            {
                if (await _context.CommandInFunctions.FindAsync(commandId, functionId) != null)
                {
                    return BadRequest(new ApiBadRequestResponse("This command is not existed in function"));
                }
                var entity = new CommandInFunction()
                {
                    CommandId = commandId,
                    FunctionId = functionId
                };

                _context.CommandInFunctions.Add(entity);
            }
            if (request.AddToAllFunctions)
            {
                var ortherFunction = _context.Functions.Where(x => x.Id != functionId);
                foreach (var function in ortherFunction)
                {
                    foreach (var commandId in request.CommandIds)
                    {
                        if (await _context.CommandInFunctions.FindAsync(commandId, functionId) == null)
                        {
                            _context.CommandInFunctions.Add(new CommandInFunction()
                            {
                                CommandId = commandId,
                                FunctionId = function.Id
                            });
                        }
                    }
                }
            }
            var result = await _context.SaveChangesAsync();
            //_context.CommandInFunctions.Add(entity);

            if (result > 0)
            {
                return Ok();
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Add command to function failed"));
            }
        }

        [HttpDelete("{functionId}/commands")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.UPDATE)]
        public async Task<IActionResult> DeleteCommandToFunction(string functionId, [FromQuery] CommandAssignRequest request)
        {
            foreach (var commandId in request.CommandIds)
            {
                var entity = await _context.CommandInFunctions.FindAsync(commandId, functionId);
                if (entity == null) return BadRequest(new ApiBadRequestResponse($"This command is not existed in function"));
                _context.CommandInFunctions.Remove(entity);
            }

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Ok();
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Delete command to function failed"));
            }
        }
        [HttpGet("{functionId}/parents")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctionByParentId(string functionId)
        {
            var functions = _context.Functions.Where(x => x.ParentId == functionId);


            var functionVms = await functions.Select(u => new FunctionVm()
            {
                Id = u.Id,
                Name = u.Name,
                Icon = u.Icon,
                Url = u.Url,
                ParentId = u.ParentId,
                SortOrder = u.SortOrder
            }).ToListAsync();

            return Ok(functionVms);
        }
    }
}