using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanbanApp.BackendServer.Data;
using KanbanApp.BackendServer.Data.Entities;
using KanbanApp.BackendServer.Helpers;
using KanbanApp.BackendServer.Services;
using KanbanApp.ViewModels.Contents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KanbanApp.BackendServer.Controllers
{
    public class StatusesConTroller : BaseController
    {
        private readonly ApplicationDbContext _context;
        public StatusesConTroller(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostStatus([FromBody] StatusCreateRequest request)
        {
            
            var status1 = new Status()
            {
                Id = request.Name + request.ProjectId,
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
            };
            var dem = _context.Statuses.Where(x => x.ProjectId == request.ProjectId).Count();
            status1.ListPosition = dem + 1;
            _context.Statuses.Add(status1);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new { id = status1.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Create Status failed"));
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var status = await _context.Statuses.FindAsync(id);
            if (status == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found Status base with id: {id}"));
            var statusVm = new StatusVm()
            {
                Id = status.Id,
                Name = status.Name,
                Description = status.Description,
                ProjectId = status.ProjectId
            };
            return Ok(statusVm);
        }
        [HttpGet]
        public async Task<IActionResult> GetStatuses()
        {
            var statuses = _context.Statuses;

            var StatusVms = await statuses.Select(u => new StatusVm()
            {
                Id = u.Id,
                ProjectId = u.ProjectId,
                Name = u.Name,
                Description = u.Description
            }).ToListAsync();

            return Ok(StatusVms);
        }
        [HttpGet("{statusId}/project")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatusInProject(string statusId)
        {
            var status = await _context.Statuses.FindAsync(statusId);

            var project = await _context.Projects.FindAsync(status.ProjectId);
            var projectVm = new ProjectVm()
            {
                Id = project.Id,
                Description = project.Description,
                CategoryId = project.CategoryId,
                Name = project.Name
            };

            return Ok(projectVm);
        }
        [HttpPut("{id}")]        
        [ApiValidationFilter]
        public async Task<IActionResult> PutStatus(string id, [FromBody] StatusCreateRequest statusVm)
        {

            var status = await _context.Statuses.FindAsync(id);
            if (status == null)
                return NotFound(new ApiNotFoundResponse($"Cannot find status with id: {id}"));

            status.Name = statusVm.Name;
            status.Description = statusVm.Description;
            status.ProjectId = statusVm.ProjectId;
            _context.Statuses.Update(status);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse("Status cannot put action"));
        }
        [HttpDelete("{statusId}")]
        public async Task<IActionResult> RemoveStatusById(string statusId)
        {
            var statusEntity = await _context.Statuses.FindAsync(statusId);
            _context.Statuses.Remove(statusEntity);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"cannot remove status to project with id: {statusId}"));
        }
    }
}
