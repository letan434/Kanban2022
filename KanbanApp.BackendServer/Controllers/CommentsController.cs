using System;
using System.Threading.Tasks;
using KanbanApp.BackendServer.Authorization;
using KanbanApp.BackendServer.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using KanbanApp.ViewModels.Contents;
using KanbanApp.ViewModels;
using KanbanApp.BackendServer.Helpers;
using KanbanApp.BackendServer.Data.Entities;
using KanbanApp.BackendServer.Extensions;

namespace KanbanApp.BackendServer.Controllers
{
    public partial class IssuesController
    {
        #region Comments

        [HttpGet("{issueId}/comments/filter")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommentsPaging(string issueId, string filter, int pageIndex, int pageSize)
        {
            var query = from c in _context.Comments
                        join u in _context.Users
                           on c.UserId equals u.Id
                        select new { c, u };
            if (issueId!=null)
            {
                query = query.Where(x => x.c.IssueId == issueId);
            }
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.c.Body.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentVm()
                {
                    Id = c.c.Id,
                    Body = c.c.Body,
                    CreateDate = c.c.CreateDate,
                    IssueId = c.c.IssueId,
                    LastModifiedDate = c.c.LastModifiedDate,
                    UserId = c.c.UserId,
                })
                .ToListAsync();

            var pagination = new Pagination<CommentVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        [HttpGet("comments/{commentId}")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommentDetail(string commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found comment with id: {commentId}"));
            var user = await _context.Users.FindAsync(comment.UserId);
            var commentVm = new CommentVm()
            {
                Id = comment.Id,
                Body = comment.Body,
                CreateDate = comment.CreateDate,
                IssueId = comment.IssueId,
                LastModifiedDate = comment.LastModifiedDate,
                UserId = comment.UserId,

            };

            return Ok(commentVm);
        }
        [HttpPost("{issueId}/comments")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostComment(string issueId, [FromBody] CommentCreateRequest request)
        {
            var comment = new Comment()
            {
                Body = request.Body,
                IssueId = request.IssueId,
                UserId = User.GetUserId(),
                CreateDate = DateTime.Now
        };
            _context.Comments.Add(comment);

            var issue = await _context.Issues.FindAsync(issueId);
            if (issue == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found issue base with id: {issue}"));

            issue.NumberOfComments = issue.NumberOfComments.GetValueOrDefault(0) + 1;
            _context.Issues.Update(issue);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetCommentDetail), new { id = issueId, commentId = comment.Id }, request);
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse("Create comment failed"));
            }
        }
        [HttpPut("{issueId}/comments/{commentId}")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.UPDATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PutComment(string commentId, [FromBody] CommentCreateRequest request)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found comment with id: {commentId}"));
            if (comment.UserId != User.Identity.Name)
                return Forbid();
            comment.Body = request.Body;
            _context.Comments.Update(comment);

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update comment failed"));
        }
        [HttpDelete("{issueId}/comments/{commentId}")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteComment(string issueId, string commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found the comment with id: {commentId}"));

            _context.Comments.Remove(comment);

            var issue = await _context.Issues.FindAsync(issueId);
            if (issue == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found issue base with id: {issueId}"));

            issue.NumberOfComments = issue.NumberOfComments.GetValueOrDefault(0) - 1;
            _context.Issues.Update(issue);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var commentVm = new CommentVm()
                {
                    Id = comment.Id,
                    Body = comment.Body,
                    CreateDate = comment.CreateDate,
                    IssueId = comment.IssueId,
                    LastModifiedDate = comment.LastModifiedDate,
                    UserId = comment.UserId
                };
                return Ok(commentVm);
            }
            return BadRequest(new ApiBadRequestResponse($"Delete comment failed"));
        }
        #endregion
    }
}
