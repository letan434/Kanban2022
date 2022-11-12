using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using KanbanApp.BackendServer.Authorization;
using KanbanApp.BackendServer.Constants;
using KanbanApp.BackendServer.Data;
using KanbanApp.BackendServer.Data.Entities;
using KanbanApp.BackendServer.Extensions;
using KanbanApp.BackendServer.Helpers;
using KanbanApp.BackendServer.Services;
using KanbanApp.ViewModels;
using KanbanApp.ViewModels.Contents;
using KanbanApp.ViewModels.Systems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KanbanApp.BackendServer.Controllers
{
    public class ProjectsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _file;

        public ProjectsController(ApplicationDbContext context, IStorageService file)
        {
            _context = context;
            _file = file;
        }
        [HttpPost]
        public async Task<IActionResult> PostProject([FromBody] ProjectCreateRequest request)
        {
            var project = CreateProjectEntity(request);
            project.OwnerUserId = User.GetUserId();
            project.CreateDate = DateTime.Now;
            _context.Projects.Add(project);
            
            
            await _context.SaveChangesAsync();
            string StatusId =  Guid.NewGuid().ToString();
            var statusListInit = new List<Status>()
            {
                new Status()
                {
                    Id = "BACKLOG" + StatusId,
                    Name = "Backlog",
                    Description = "btn-secondary",
                    ProjectId = project.Id,
                    ListPosition = 1
                },
                new Status()
                {
                    Id = "SELECTED" +  StatusId,
                    Name = "Selected for Development",
                    ProjectId = project.Id,
                    Description = "btn-secondary",
                    ListPosition = 2
                },
                new Status()
                {
                    Id = "IN_PROGRESS" + StatusId,
                    Name = "In progress",
                    Description = "btn-primary",
                    ProjectId = project.Id,
                    ListPosition = 3
                },
                new Status()
                {
                    Id = "DONE" + StatusId,
                    Name = "Done",
                    Description = "btn-success",
                    ProjectId = project.Id,
                    ListPosition = 4
                }

            };
            foreach (var statusItem in statusListInit)
            {
                _context.Statuses.Add(statusItem);
            }
            var userInProject = new UserInProject()
            {
                UserId = project.OwnerUserId,
                ProjectId = project.Id
            };
            _context.UserInProjects.Add(userInProject);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new
                {
                    id = project.Id
                });
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Create Project failed"));
            }
        }
        [HttpPut("{projectId}")]
        public async Task<IActionResult> PutProject(string projectId, [FromBody] ProjectVm request)
        {
            var project = await _context.Projects.FindAsync(request.Id);
            project.CategoryId = request.CategoryId;
            project.Description = request.Description;
            project.Name = request.Name;
            project.AvatarUrl = request.AvatarUrl;
            _context.Projects.Update(project);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Update Project failed"));
            }
        }
        [HttpPost("{projectId}/client")]
        public async Task<IActionResult> PostProjectByClient(string projectId, [FromForm] ProjectCreateRequest request)
        {
            var project = await _context.Projects.FindAsync(request.Id);
            if (project == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found project with id: {projectId}"));
            project.CategoryId = request.CategoryId;
            project.Description = request.Description;
            project.Name = request.Name;
           
            if (request.AvatarUrl != null)
            {
                if (project.AvatarUrl != null)
                {
                    await _file.DeleteFileAsync(project.AvatarUrl);
                }
                var originalFileName = ContentDispositionHeaderValue.Parse(request.AvatarUrl.ContentDisposition).FileName.Trim('"');
                var fileName = $"{originalFileName.Substring(0, originalFileName.LastIndexOf('.'))}{Path.GetExtension(originalFileName)}";
                await _file.SaveFileAsync(request.AvatarUrl.OpenReadStream(), fileName);
                project.AvatarUrl = _file.GetFileUrl(fileName);
            }
            _context.Projects.Update(project);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Update Project failed"));
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found project base with id: {id}"));
            var listStatuses = await _context.Statuses.Where(s => s.ProjectId == id).OrderBy(x=>x.ListPosition).Select(x=>
            new StatusVm() {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ProjectId = x.ProjectId
            }).ToListAsync();
            var listIssues = new List<IssueQuickVm>();
            foreach(StatusVm status in listStatuses)
            {
                var issueItem = await _context.Issues.Where(x => x.StatusId == status.Id)
                    .Select(x => new IssueQuickVm()
                    {
                        Id = x.Id,
                        Description = x.Description,
                        Title = x.Title,
                        Sample = x.Sample.ToString(),
                        ListPosition = x.ListPosition,
                        Priority = x.Priority,
                        ReporterId = x.ReporterId,
                        Status = status,
                        ProjectId = project.Id,
                        CreateDate =x.CreateDate,
                        LastModifiedDate = x.LastModifiedDate,
                        Estimate = x.Estimate,
                        TimeRemaining = x.TimeRemaining,
                        TimeSpent = x.TimeSpent,
                        UserIds = _context.UserInIssues.Where(k=> k.IssueId == x.Id && k.ProjectId == project.Id).Select(x=> x.UserId).ToArray(),
                        Comments = _context.Comments.Where(c=>c.IssueId == x.Id).Select(cm=> new CommentVm()
                        {
                            Id = cm.Id,
                            CreateDate = cm.CreateDate,
                            LastModifiedDate = cm.LastModifiedDate,
                            Body = cm.Body,
                            IssueId = cm.IssueId,
                            UserId = cm.UserId,
                            User = _context.Users.Where(us=>us.Id == cm.UserId).Select(use=> new UserVmFE()
                            {
                                Id = use.Id,
                                CreateDate = use.CreateDate.ToString(),
                                UserName = use.FirstName + ' ' + use.LastName,
                                AvatarUrl = use.AvatarUrl,
                                UserNameMain = use.UserName
                            }).FirstOrDefault()
                        }).OrderByDescending(x=>x.CreateDate).ToList(),
                        Attachments = _context.Attachments.Where(a=>a.IssueId == x.Id).Select(x=> new AttachmentVm() { 
                            Id = x.Id,
                            CreateDate = x.CreateDate,
                            LastModifiedDate = x.LastModifiedDate,
                            FileName = x.FileName,
                            FilePath = x.FilePath,
                            FileSize = x.FileSize,
                            FileType = x.FileType,
                            IssueId = x.IssueId,
                        }).ToList()

                    }).ToListAsync();
                listIssues.AddRange(issueItem);
            }
            var query1 = from p in _context.Projects
                         join uip in _context.UserInProjects on p.Id equals uip.ProjectId
                         join u in _context.Users on uip.UserId equals u.Id
                         where p.Id == project.Id
                         select new {u};

            //var IssueInUsers = listIssues.Select(x => x.Id).ToArray();

            var listUsers = await query1.Select(x => new UserVmFE()
            {
                Id = x.u.Id,
                Email = x.u.Email,
                UserName = x.u.FirstName + ' ' +  x.u.LastName,
                AvatarUrl = x.u.AvatarUrl,
                CreateDate = x.u.CreateDate.ToString(),
                LastModifiedDate = x.u.LastModifiedDate.ToString(),
                IssueIds = _context.UserInIssues.Where(k => k.UserId == x.u.Id).Select(h => h.IssueId).ToArray(),
                UserNameMain = x.u.UserName

            }).ToListAsync();

            var projectVm = new ProjectVm()
            {
                Id = project.Id,
                Description = project.Description,
                CreateDate = project.CreateDate,
                CategoryId = project.CategoryId,
                LastModifiedDate = project.LastModifiedDate,
                Issues = listIssues,
                Users = listUsers,
                OwnerUserId = project.OwnerUserId,
                Statuses = listStatuses,
                Name = project.Name,
                AvatarUrl = project.AvatarUrl

            };

            return Ok(projectVm);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProjects()
        {
            var projects = _context.Projects;

            var projectQuickVms = await projects.Select(u => new ProjectQuickVm()
            {
                Id = u.Id,
                CategoryId = u.CategoryId,
                CreateDate = u.CreateDate,
                LastModifiedDate = u.LastModifiedDate,
                Name = u.Name,
                Description = u.Description
            }).ToListAsync();

            return Ok(projectQuickVms);
        }
        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.CONTENT_PROJECT, CommandCode.VIEW)]
        public async Task<IActionResult> GetProjectsPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.Projects.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter)
                || x.Description.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).ToListAsync();

            var data = items.Select(c => CreateProjectVm(c)).ToList();

            var pagination = new Pagination<ProjectVm>
            {
                Items = data,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        [HttpGet("{projectId}/{userId}")]
        public async Task<IActionResult> GetUserofProject(string projectId, string userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            var user =  await _context.Users.FindAsync(userId);
            var IssueIds = await _context.UserInIssues.Where(x => x.UserId == user.Id && x.ProjectId == projectId).Select(u => u.IssueId).ToArrayAsync();

            var userVmFe = new UserVmFE()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.FirstName + ' ' + user.LastName,
                CreateDate = user.CreateDate.ToString(),
                LastModifiedDate = user.LastModifiedDate.ToString(),
                AvatarUrl = user.AvatarUrl,
                IssueIds = IssueIds,
                UserNameMain = user.UserName
            };
            return Ok(userVmFe);
        }
        [HttpGet("{projectId}/users")]
        public async Task<IActionResult> GetUserByProjectId(string projectId)
        {
            var query = from p in _context.Projects
                        join uip in _context.UserInProjects on p.Id equals uip.ProjectId
                        join u in _context.Users on uip.UserId equals u.Id
                        where p.Id == projectId
                        select new { u.Id, u.UserName, u.FirstName, u.LastName, u.Dob, u.Email, u.PhoneNumber};


            var userVm =await query.Select(u => new UserVm()
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Dob = u.Dob,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber
            }).ToListAsync();
            return Ok(userVm);
        }
        [HttpDelete("{projectId}")]
        [ClaimRequirement(FunctionCode.CONTENT_PROJECT, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteProject(string projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return BadRequest(new ApiBadRequestResponse($"can not find project with id: {projectId}"));
            _context.Projects.Remove(project);
            var userInProjectIds = _context.UserInProjects.Where(u => u.ProjectId == projectId);
            _context.UserInProjects.RemoveRange(userInProjectIds);
            var userInIssue = _context.UserInIssues.Where(x => x.ProjectId == projectId);
            _context.UserInIssues.RemoveRange(userInIssue);
            var statuses = _context.Statuses.Where(x => x.ProjectId == projectId);
            _context.Statuses.RemoveRange(statuses);
            var result = await _context.SaveChangesAsync();
            if(result > 0)
            {
                var projectVm = new ProjectVm()
                {
                    Id = project.Id,
                    Description = project.Description,
                    CategoryId = project.CategoryId,
                    Name = project.Name
                };
                return Ok(projectVm);
            }
            return BadRequest(new ApiBadRequestResponse("Delete project failed"));
        }
        [HttpPut("{projectId}/users")]
        public async Task<IActionResult> AddUserToProject(string projectId, [FromBody] AddUserToProjectRequest request)
        {
            if (request.UserIds?.Length == 0)
            {
                return BadRequest(new ApiBadRequestResponse("Users name can not emty"));
            }
            var project = await _context.Projects.FindAsync(projectId);
            if(User.GetUserId() == project.OwnerUserId)
            {
                foreach(var userId in request.UserIds)
                {
                    if (await _context.UserInProjects.FindAsync(userId, projectId) == null)
                    {
                        var userInProject = new UserInProject()
                        {
                            UserId = userId,
                            ProjectId = projectId
                        };
                        _context.UserInProjects.Add(userInProject);
                    }
                }
                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    return NoContent();
                }
                else return BadRequest(new ApiBadRequestResponse("Add user to project faild"));
            }
            return BadRequest(new ApiBadRequestResponse($"user not role add user to project"));
        }
        [HttpDelete("{projectId}/users")]
        //[ClaimRequirement(FunctionCode.CONTENT_PROJECT, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteUserToProject(string projectId, [FromQuery] AddUserToProjectRequest request)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (User.GetUserId() == project.OwnerUserId)
            {
                foreach (var userId in request.UserIds)
                {
                    if (userId == project.OwnerUserId) return BadRequest(new ApiBadRequestResponse("not delete admin of project"));
                    var entity = await _context.UserInProjects.FindAsync(userId, projectId);
                    if (entity == null) return BadRequest(new ApiBadRequestResponse($"This user is not existed in project"));
                    _context.UserInProjects.Remove(entity);
                    var entity1 = _context.UserInIssues.Where(x=>x.UserId == userId).ToList();
                    if (entity == null) return BadRequest(new ApiBadRequestResponse($"This user is not existed in project"));
                    foreach (var item in entity1) {
                        var entity2 = _context.Comments.Where(x => x.UserId == userId && x.IssueId == item.IssueId).ToList();
                        foreach(var comment in entity2)
                        {
                            _context.Comments.Remove(comment);
                        }
                        _context.UserInIssues.Remove(item); }
                    
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
            return BadRequest(new ApiBadRequestResponse($"user not role add user to project"));
        }
        [HttpPost("{projectId}/status")]
        public async Task<IActionResult> PostStatusToProject(string projectId, [FromBody] AddNewStatusToProjectRequest request)
        {
            var statusEntity = new Status()
            {
                Id = request.Name + projectId,
                Name = request.Name,
                Description = request.Description,
                ProjectId = projectId
            };
            var dem = _context.Statuses.Where(x => x.ProjectId == projectId).Count();
            statusEntity.ListPosition = dem + 1;
            _context.Statuses.Add(statusEntity);
            var result = await _context.SaveChangesAsync();
            if(result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"cannot add status to project with id: {projectId}"));
        }        
        [HttpGet("{projectId}/status")]
        public async Task<IActionResult> GetStatusesToProject(string projectId)
        {
            var statusVms = await _context.Statuses.Where(x => x.ProjectId == projectId).Select(u => new StatusVm()
            {
                Id = u.Id,
                Name = u.Name,
                ProjectId = u.ProjectId,
                Description = u.Description
            }).ToListAsync();
            return Ok(statusVms);
        }
        [HttpPut("{projectId}/status/{statusId}")]
        public async Task<IActionResult> PutStatusToProject(string projectId, string statusId, [FromBody] AddNewStatusToProjectRequest request)
        {
            var status = await _context.Statuses.FindAsync(statusId);
            if (status == null) return BadRequest(new ApiBadRequestResponse($"can not find status with id: {statusId}"));
            status.Name = request.Name;
            status.Description = request.Description;
            _context.Statuses.Update(status);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"cannot put status with id: {projectId}"));
        }
        [HttpGet("statusdashboard")]
        public async Task<IActionResult> GetDataDashboardToProject()
        {
            var projects = await _context.Projects.OrderBy(x=>x.CreateDate).ToListAsync();
            if (projects == null) return BadRequest(new ApiBadRequestResponse($"can not find project"));

            var query1 = from sta in _context.Statuses
                         join iss in _context.Issues on sta.Id equals iss.StatusId
                         select new  { iss.Id, iss.Title, sta.Name, sta.ProjectId };
            
            List<InfoProject> listResult = new List<InfoProject>();
            List<InfoProjectFull> listResultBase = new List<InfoProjectFull>();

            listResultBase = await query1.Select(x => new InfoProjectFull()
            {
                IssueId = x.Id,
                ProjectId = x.ProjectId,
                IssueName = x.Title,
                StatusName = x.Name
            }).ToListAsync();

            listResult = projects.Select(x => new InfoProject()
            {
                Id = x.Id,
                TaskDone = 0,
                TaskBackLog = 0,
                TaskProgress = 0,
                Name = x.Name
            }).ToList();

            listResultBase.ForEach(x => {
                if(x.StatusName == "Done") listResult.Find(p => p.Id == x.ProjectId).TaskDone += 1;
                else
                {
                    if (x.StatusName == "Backlog") listResult.Find(p => p.Id == x.ProjectId).TaskBackLog += 1;
                    else listResult.Find(p => p.Id == x.ProjectId).TaskProgress += 1;
                }
                

            });
            var uri = "https://vietjack.com/de-kiem-tra-lop-9/images/de-thi-giua-ki-1-toan-lop-9-co-dap-an-2021-25923.png";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    using (Stream stream = await client.GetStreamAsync(uri))
                    {                         
                        byte[] buffer = new byte[16384];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            while (true)
                            {
                                int num = await stream.ReadAsync(buffer, 0, buffer.Length);
                                int read;
                                if ((read = num) > 0)
                                    ms.Write(buffer, 0, read);
                                else
                                    break;
                            }
                            var imageData = "data:image/png;base64," +  Convert.ToBase64String(ms.ToArray());
                        }
                        buffer = (byte[])null;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            if (listResult.Count > 0)
            {
                return Ok(listResult);

            }
            return BadRequest(new ApiBadRequestResponse($"cannot put status with id"));
        }
        [HttpGet("newItem")]
        public IActionResult GetNewItemDashboardOfProject()
        {
            DateTime datenow = DateTime.Now;
            DateTime dateBeforeMonth;
            if (datenow.Month == 1)
            {
                dateBeforeMonth = new DateTime(datenow.Year - 1, 12, datenow.Day);
            }
            else
            {
                dateBeforeMonth = new DateTime(datenow.Year, datenow.Month -1, datenow.Day);

            }
            int projectCount = _context.Projects.Where(x => x.CreateDate >= dateBeforeMonth).Count();
            int commentCount = _context.Comments.Where(x => x.CreateDate >= dateBeforeMonth).Count();

            int userCount = _context.Users.Where(x => x.CreateDate >= dateBeforeMonth).Count();
            int issueCount = _context.Issues.Where(x => x.CreateDate >= dateBeforeMonth).Count();
            var result = new CountNewDashboard(projectCount, issueCount, commentCount, userCount);
            return Ok(result);
        }
        #region Private method
        private static Project CreateProjectEntity(ProjectCreateRequest request)
        {
            return new Project()
            {
                Id = Guid.NewGuid().ToString(),
                CategoryId = request.CategoryId,
                Description = request.Description,
                Name = request.Name,
            };
        }
        private static ProjectVm CreateProjectVm(Project project)
        {
            return new ProjectVm()
            {
                Id = project.Id,
                Name = project.Name,
                CategoryId = project.CategoryId,
                Description = project.Description
            };
        }
        private static UserVm CreateUserVm(User user)
        {
            return new UserVm()
            {
                Id = user.Id,
                Dob = user.Dob,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };
        }
        #endregion private

    }
}
