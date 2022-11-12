using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using KanbanApp.BackendServer.Authorization;
using KanbanApp.BackendServer.Constants;
using KanbanApp.BackendServer.Data;
using KanbanApp.BackendServer.Data.Entities;
using KanbanApp.BackendServer.Helpers;
using KanbanApp.BackendServer.Services;
using KanbanApp.ViewModels;
using KanbanApp.ViewModels.Contents;
using KanbanApp.ViewModels.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KanbanApp.BackendServer.Controllers
{
    public class UsersController : BaseController
    {
        
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _file;
        public UsersController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context, IStorageService file)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _file = file;
        }
        [HttpPost]
        [ApiValidationFilter]
        public async Task<IActionResult> PostUser(UserCreateRequest request)
        {
            var user = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                Dob = DateTime.Parse(request.Dob),
                UserName = request.UserName,
                LastName = request.LastName,
                FirstName = request.FirstName,
                PhoneNumber = request.PhoneNumber,
                CreateDate = DateTime.Now,
                AvatarUrl = request.AvatarUrl
            };
            if(user.AvatarUrl == null)
            {
                user.AvatarUrl = "/user-attachments/Xả Ảnh Anime Đẹp, Hiếm _.jpg";
            }
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse(result));
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users;

            var uservms = await users.Select(u => new UserVm()
            {
                Id = u.Id,
                UserName = u.UserName,              
                FirstName = u.FirstName,
                LastName = u.LastName
            }).ToListAsync();

            return Ok(uservms);
        }
        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetUsersPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _userManager.Users;
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Email.Contains(filter)
                || x.UserName.Contains(filter)
                || x.PhoneNumber.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserVm()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Dob = u.Dob,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName                   
                })
                .ToListAsync();

            var pagination = new Pagination<UserVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

            var userVm = new UserVm()
            {
                Id = user.Id,
                UserName = user.FirstName + user.LastName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Dob = user.Dob,
                PhoneNumber = user.PhoneNumber
            };
            return Ok(userVm);
        }
        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        public async Task<IActionResult> PutUser(string id, [FromBody] UserCreateRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Dob = DateTime.Parse(request.Dob);
            user.LastModifiedDate = DateTime.Now;
            user.PhoneNumber = request.PhoneNumber;
            user.AvatarUrl = request.AvatarUrl;
            
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }
        //Request URL: https://localhost:5000/api/users/{id}/uploadAvatar
        [HttpPost("{id}/uploadAvatar")]
        public async Task<IActionResult> PutUserFE(string id, [FromForm] UserUpdateRequest request)
         {   
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.LastModifiedDate = DateTime.Now;
           
            if (request.AvatarUrl != null)
            {
                if (user.AvatarUrl != null)
                {
                    await _file.DeleteFileAsync(user.AvatarUrl);
                }
                var originalFileName = ContentDispositionHeaderValue.Parse(request.AvatarUrl.ContentDisposition).FileName.Trim('"');
                var fileName = $"{originalFileName.Substring(0, originalFileName.LastIndexOf('.'))}{Path.GetExtension(originalFileName)}";
                await _file.SaveFileAsync(request.AvatarUrl.OpenReadStream(), fileName);
                user.AvatarUrl = _file.GetFileUrl(fileName);
            }
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }
        //[HttpPut("{id}/avatar")]
        //[ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        //public async Task<IActionResult> PutUserAvatar(string id, string request)
        //{
        //    var user = await _userManager.FindByIdAsync(id);
        //    if (user == null)
        //        return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

        //    user.AvatarUrl = request;

        //    var result = await _userManager.UpdateAsync(user);

        //    if (result.Succeeded)
        //    {
        //        return NoContent();
        //    }
        //    return BadRequest(new ApiBadRequestResponse(result));
        //}
        [HttpPut("{id}/change-password")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutUserPassword(string id, [FromBody] UserPasswordChangeRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {id}"));

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }
        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                var uservm = new UserVm()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Dob = user.Dob,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                return Ok(uservm);
            }
            return BadRequest(new ApiBadRequestResponse(result));
        }
        [HttpGet("{userId}/menu")]
        public async Task<IActionResult> GetMenuByUserPermission(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);
            var query = from f in _context.Functions
                        join p in _context.Permissions
                            on f.Id equals p.FunctionId
                        join r in _roleManager.Roles on p.RoleId equals r.Id
                        join a in _context.Commands
                            on p.CommandId equals a.Id
                        where roles.Contains(r.Name) && a.Id == "VIEW"
                        select new FunctionVm
                        {
                            Id = f.Id,
                            Name = f.Name,
                            Url = f.Url,
                            ParentId = f.ParentId,
                            SortOrder = f.SortOrder,
                            Icon = f.Icon
                        };
            var data = await query.Distinct()
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.SortOrder)
                .ToListAsync();
            return Ok(data);
        }
        [HttpGet("{userId}/roles")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
            }
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);

        }
        [HttpPost("{userId}/roles")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.UPDATE)]
        public async Task<IActionResult> PostRolesToUserUser(string userId, [FromBody] RoleAssignRequest request)
        {
            if (request.RoleNames?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse("Role name can not emty"));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
            }
            var result = await _userManager.AddToRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            return BadRequest(new ApiBadRequestResponse(result));
        }
        [HttpDelete("{userId}/roles")]
        [ClaimRequirement(FunctionCode.SYSTEM_USER, CommandCode.VIEW)]
        public async Task<IActionResult> RemoveRoleFromUser(string userId, [FromQuery] RoleAssignRequest request)
        {
            if (request.RoleNames?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse("Role name can not emty"));
            }
            if (request.RoleNames?.Length == 1 && request.RoleNames[0] == SystemConstants.Roles.Admin)
            {
                return BadRequest(new ApiBadRequestResponse($"can not remove {SystemConstants.Roles.Admin} role"));
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found user with id: {userId}"));
            var result = await _userManager.RemoveFromRolesAsync(user, request.RoleNames);
            if (result.Succeeded)
                return Ok();

            return BadRequest(new ApiBadRequestResponse(result));
        }
        [HttpGet("{userId}/projects")]
        public async Task<IActionResult> GetAllProjectByUserId(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var query = from u in _context.Users
                        join uip in _context.UserInProjects on u.Id equals uip.UserId
                        join p in _context.Projects on uip.ProjectId equals p.Id
                        where u.Id == userId
                        select new { p };
            var listprojects = await query.Select(u => new ProjectQuickVm()
            {
                Id = u.p.Id,
                CategoryId = u.p.CategoryId,
                Description = u.p.Description,
                CreateDate = u.p.CreateDate,
                LastModifiedDate = u.p.LastModifiedDate,
                Name = u.p.Name
            }).ToListAsync();
            return Ok(listprojects);
        }
    }
}
