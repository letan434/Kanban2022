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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KanbanApp.BackendServer.Controllers
{
    public partial class IssuesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ISequenceService _sequenceService;
        private readonly IStorageService _storageService;
        private readonly ILogger<IssuesController> _logger;
        private readonly IHostingEnvironment _env;
        public IssuesController(ApplicationDbContext context,
            ISequenceService sequenceService,
            IStorageService storageService, ILogger<IssuesController> logger, IHostingEnvironment env)
        {
            _context = context;
            _sequenceService = sequenceService;
            _storageService = storageService;
            _logger = logger;
            _env = env;
        }
        [HttpPost]
        public async Task<IActionResult> PostIssue([FromBody] IssueCreateRequest request)
        {
            var issue = new Issue()
            {
                Description = request.Description,
                Title = request.Title,
                CreateDate = DateTime.Now,
                Priority = request.Priority,
                ReporterId = request.ReporterId,
                Id = request.Id,
                StatusId = request.Status.Id
            };
            var count = _context.Issues.Where(x => x.StatusId == request.Status.Id).ToList().Count();
            issue.ListPosition = count + 1;
            switch (request.Sample)
            {
                case "TASK":
                    issue.Sample = Sample.TASK;
                    break;
                case "BUG":
                    issue.Sample = Sample.BUG;
                    break;
                case "STORY":
                    issue.Sample = Sample.STORY;
                    break;
                default:
                    issue.Sample = Sample.TASK;
                    break;
            }
            _context.Issues.Add(issue);
            foreach (var userId in request.UserIds)
            {
                var check = _context.UserInIssues.FirstOrDefault(x => x.UserId == userId && x.IssueId == request.Id && x.ProjectId == request.ProjectId);
                if (check == null)
                {
                    var userInIssue = new UserInIssue()
                    {
                        IssueId = request.Id,
                        UserId = userId,
                        ProjectId = request.ProjectId
                    };
                    _context.UserInIssues.Add(userInIssue);
                }
            }
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Create Issue failed"));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIssue(string id, [FromBody] IssueUpdateRequest request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            issue.Title = request.Title;
            issue.StatusId = request.Status.Id;
            issue.Description = request.Description;
            issue.ListPosition = request.ListPosition;
            issue.Priority = request.Priority;
            switch (request.Sample)
            {
                case "TASK":
                    issue.Sample = Sample.TASK;
                    break;
                case "BUG":
                    issue.Sample = Sample.BUG;
                    break;
                case "STORY":
                    issue.Sample = Sample.STORY;
                    break;
                default:
                    issue.Sample = Sample.TASK;
                    break;
            }
            issue.ReporterId = request.ReporterId;
            var Lists = _context.UserInIssues.Where(x => x.IssueId == request.Id);
            _context.UserInIssues.RemoveRange(Lists);
            await _context.SaveChangesAsync();
            foreach (var userId in request.UserIds)
            {
                var check = _context.UserInIssues.FirstOrDefault(x => x.UserId == userId && x.IssueId == request.Id && x.ProjectId == request.ProjectId);
                if (check == null)
                {
                    var userInIssue = new UserInIssue()
                    {
                        IssueId = request.Id,
                        UserId = userId,
                        ProjectId = request.ProjectId
                    };
                    _context.UserInIssues.Add(userInIssue);
                }
            }
            issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);
            //Process attachment
            //if (request.Attachments != null && request.Attachments.Count > 0)
            //{
            //    foreach (var attachment in request.Attachments)
            //    {
            //        var attachmentEntity = await SaveFile(issue.Id, attachment);
            //        _context.Attachments.Add(attachmentEntity);
            //    }
            //}
            if (request.Labels?.Length > 0)
            {
                issue.Labels = string.Join(',', request.Labels);
            }

            _context.Issues.Update(issue);
            if (request.Labels?.Length > 0)
            {
                await ProcessLabel(request, issue);
            }
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Ok(true);
            }
            return Ok(false);

        }
        [HttpPut("{id}/date-expiration")]
        public async Task<IActionResult> PutIssueWithDateExpiration(string id, [FromBody] IssueUpdateRequest request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));

            issue.StartDate = request.StartDate != "" && request.StartDate != null ? DateTime.Parse(request.StartDate) : (DateTime?)null;
            issue.EndDate = request.EndDate != "" && request.EndDate != null ? DateTime.Parse(request.EndDate) : (DateTime?)null;
            issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);

            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Ok();
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }
        [HttpPut("{id}/status")]
        public async Task<IActionResult> PutIssueWithStatus(string id, [FromBody] IssueUpdateRequest request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            issue.StatusId = request.Status.Id;
            issue.ListPosition = request.ListPosition;
            issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);
            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Ok(issue);
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }
        [HttpPost("statuses")]
        public async Task<IActionResult> PutListIssue([FromBody] IssueUpdateRequest[] requests)
        {
            foreach (var request in requests)
            {
                var issue = await _context.Issues.FindAsync(request.Id);
                issue.StatusId = request.Status.Id;
                issue.ListPosition = request.ListPosition;
                if (request.LastModifiedDate != null)
                {
                    issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);
                }
                _context.Issues.Update(issue);
            }
            ActivityLog log = new ActivityLog()
            {
                CreateDate = DateTime.Now,
                Action = "update",
                Content = "statuses",
                EntityId = Guid.NewGuid().ToString(),
                EntityName = "statuses",
                UserId = User.GetUserId()
            };
            _context.ActivityLogs.Add(log);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(true);
            }
            else return BadRequest(new ApiBadRequestResponse($"Update issue failed"));


        }
        [HttpPut("{id}/title")]
        public async Task<IActionResult> PutIssueWithTitle(string id, [FromBody] IssueUpdateRequest request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            issue.Title = request.Title;
            issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);
            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }
        [HttpPut("{id}/type")]
        public async Task<IActionResult> PutIssueWithType(string id, [FromBody] IssueUpdateRequest request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            switch (request.Sample)
            {
                case "TASK":
                    issue.Sample = Sample.TASK;
                    break;
                case "BUG":
                    issue.Sample = Sample.BUG;
                    break;
                case "STORY":
                    issue.Sample = Sample.STORY;
                    break;
                default:
                    issue.Sample = Sample.TASK;
                    break;
            }
            issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);
            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }
        [HttpPut("{id}/description")]
        public async Task<IActionResult> PutIssueWithDescription(string id, [FromBody] IssueUpdateRequest request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            issue.Description = request.Description;
            issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);
            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }

        [HttpPut("{id}/reporter")]
        public async Task<IActionResult> PutIssueWithReporter(string id, [FromBody] IssueUpdateRequest request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            issue.ReporterId = request.ReporterId;
            issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);
            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }
        [HttpPut("{id}/priority")]
        public async Task<IActionResult> PutIssueWithPriority(string id, [FromBody] IssueUpdateRequest request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            issue.Priority = request.Priority;
            issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);
            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }
        [HttpPut("{id}/assignees")]
        public async Task<IActionResult> PutIssueWithAssignees(string id, [FromBody] IssueUpdateRequest request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            var Lists = _context.UserInIssues.Where(x => x.IssueId == request.Id);
            _context.UserInIssues.RemoveRange(Lists);
            await _context.SaveChangesAsync();
            foreach (var userId in request.UserIds)
            {
                var check = _context.UserInIssues.FirstOrDefault(x => x.UserId == userId && x.IssueId == request.Id && x.ProjectId == request.ProjectId);
                if (check == null)
                {
                    var userInIssue = new UserInIssue()
                    {
                        IssueId = request.Id,
                        UserId = userId,
                        ProjectId = request.ProjectId
                    };
                    _context.UserInIssues.Add(userInIssue);
                }
            }
            issue.LastModifiedDate = DateTime.Parse(request.LastModifiedDate);
            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }
        [HttpPut("{id}/comment")]
        public async Task<IActionResult> PutIssueWithComment(string id, [FromBody] CommentVm request)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            var comment = new Comment()
            {
                Id = request.Id,
                IssueId = request.IssueId,
                CreateDate = request.CreateDate,
                LastModifiedDate = request.LastModifiedDate,
                Body = request.Body,
                UserId = request.UserId
            };
            _context.Comments.Add(comment);
            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id: {id}"));
            var status = await _context.Statuses.FindAsync(issue.StatusId);
            var userids = await _context.UserInIssues.Where(x => x.IssueId == id && x.ProjectId == status.ProjectId).Select(
                x => x.UserId).ToArrayAsync();
            var issueVm = new IssueQuickVm()
            {
                Id = issue.Id,
                Description = issue.Description,
                Sample = issue.Sample.ToString(),
                Status = new StatusVm()
                {
                    Id = status.Id,
                    Name = status.Name,
                    Description = status.Description
                ,
                    ProjectId = status.ProjectId
                },
                ListPosition = issue.ListPosition,
                Priority = issue.Priority,
                Title = issue.Title,
                CreateDate = issue.CreateDate,
                LastModifiedDate = issue.LastModifiedDate,
                Estimate = issue.Estimate,
                ProjectId = status.ProjectId,
                ReporterId = issue.ReporterId,
                TimeRemaining = issue.TimeRemaining,
                TimeSpent = issue.TimeSpent,
                UserIds = userids
            };

            return Ok(issueVm);
        }
        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> GetIssuesPaging(string filter, string statusId, int pageIndex, int pageSize)
        {
            var query = from i in _context.Issues
                        join s in _context.Statuses on i.StatusId equals s.Id
                        join p in _context.Projects on s.ProjectId equals p.Id
                        join u in _context.Users on i.ReporterId equals u.Id
                        select new { i, s, p, u };
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.i.Id.Contains(filter));
            }
            if (!string.IsNullOrWhiteSpace(statusId))
            {
                query = query.Where(x => x.i.StatusId == statusId);
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
               .Take(pageSize)
               .Select(u => new IssueVmAdmin()
               {
                   Id = u.i.Id,
                   Description = u.i.Description,
                   Title = u.i.Title,
                   Sample = u.i.Sample.ToString(),
                   UserName = u.u.UserName,
                   ProjectName = u.p.Name,
                   ReporterId = u.i.ReporterId,
                   StatusName = u.s.Name
               }).ToListAsync();
            var pagination = new Pagination<IssueVmAdmin>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Items = items,
                TotalRecords = totalRecords
            };
            return Ok(pagination);
        }
        [HttpPut("{id}/style")]
        public async Task<IActionResult> PutIssueType(int id, string IssueType)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {id}"));
            switch (IssueType)
            {
                case "TASK":
                    issue.Sample = Sample.TASK;
                    break;
                case "BUG":
                    issue.Sample = Sample.BUG;
                    break;
                case "STORY":
                    issue.Sample = Sample.STORY;
                    break;
                default:
                    break;
            }
            issue.LastModifiedDate = DateTime.Now;
            _context.Issues.Update(issue);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIssue(string id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
            {
                return NotFound(new ApiNotFoundResponse($"Cannot find Issue with id: {id}"));
            }
            var userId = User.GetUserId();
            var role = _context.UserRoles.FirstOrDefault(x => x.UserId == userId);
            if (User.GetUserId() == issue.ReporterId || role.RoleId == "Admin")
            {
                _context.Issues.Remove(issue);
            }
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var issueVm = new IssueQuickVm()
                {
                    Id = issue.Id,
                    Description = issue.Description,
                    ListPosition = issue.ListPosition,
                    Priority = issue.Priority,
                    Sample = issue.Sample.ToString()
                };
                return Ok(issueVm);
            }
            return BadRequest(new ApiBadRequestResponse($"delete issue with id: {id} failed"));
        }
        [HttpGet]
        public async Task<IActionResult> GetIssues()
        {
            var issues = _context.Issues;
            var issueVms = await issues.Select(u => new IssueVm()
            {
                StatusId = u.StatusId,
                Id = u.Id,
                Description = u.Description,
                ListPosition = u.ListPosition,
                Priority = u.Priority,
                Sample = u.Sample.ToString(),
            }).ToListAsync();
            return Ok(issueVms);
        }
        [HttpGet("projects/{projectId}")]
        public async Task<IActionResult> GetIssuesByProjectId(string projectId)
        {
            var projectVm = await _context.Projects.FindAsync(projectId);
            var statuesByProjectId = await _context.Statuses.Where(x => x.ProjectId == projectId).Select(u => new StatusVm()
            {
                Id = u.Id,
                Name = u.Name,
                ProjectId = u.ProjectId,
                Description = u.Description
            }).ToListAsync();
            var listIssues = new List<IssueVm>();
            foreach (var status in statuesByProjectId)
            {
                var issueItems = await _context.Issues.Where(x => x.StatusId == status.Id).Select(u => new IssueVm()
                {
                    Id = u.Id,
                    Description = u.Description,
                    ListPosition = u.ListPosition,
                    Priority = u.Priority,
                    Sample = u.Sample.ToString(),
                    Title = u.Title,
                }).ToListAsync();
                listIssues.AddRange(issueItems);
            }
            return Ok(listIssues);
        }
        [HttpPost("attachments")]
        public async Task<IActionResult> addattachments([FromForm] IssueAttachmentsRequest request)
        {
            var issue = await _context.Issues.FindAsync(request.Id);
            if (issue == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found issue base with id {request.Id}"));
            if (request.File == null)
            {
                return BadRequest(new ApiBadRequestResponse($"Update issue failed"));
            }
            var attachment = await SaveFile(request.Id, request.File);
            _context.Attachments.Add(attachment);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(attachment);
            }
            return BadRequest(new ApiBadRequestResponse($"Update issue failed"));

        }
        #region private method
        private async Task<Attachment> SaveFile(string issueId, IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{originalFileName.Substring(0, originalFileName.LastIndexOf('.'))}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            var attachmentEntity = new Attachment()
            {
                FileName = fileName,
                FilePath = _storageService.GetFileUrl(fileName),
                FileSize = file.Length,
                FileType = Path.GetExtension(fileName),
                IssueId = issueId
            };
            return attachmentEntity;
        }
        private async Task ProcessLabel(IssueUpdateRequest request, Issue issue)
        {
            foreach (var labelText in request.Labels)
            {
                if (labelText == null) continue;
                var labelId = TextHelper.ToUnsignString(labelText.ToString());
                var existingLabel = await _context.Labels.FindAsync(labelId);
                if (existingLabel == null)
                {
                    var labelEntity = new Label()
                    {
                        Id = labelId,
                        Name = labelText.ToString()
                    };
                    _context.Labels.Add(labelEntity);
                }
                if (await _context.LabelInIssues.FindAsync(labelId, issue.Id) == null)
                {
                    _context.LabelInIssues.Add(new LabelInIssue()
                    {
                        IssueId = issue.Id,
                        LabelId = labelId
                    });
                }
            }
        }
        #endregion
    }
}