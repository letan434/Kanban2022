using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KanbanApp.BackendServer.Helpers;
using KanbanApp.ViewModels.Contents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KanbanApp.BackendServer.Controllers
{
    public partial class IssuesController
    {        
        #region Attachments

        [HttpGet("{issueId}/attachments")]
        public async Task<IActionResult> GetAttachment(string issueId)
        {


            var query = await _context.Attachments
                .Where(x => x.IssueId == issueId)
                .Select(c => new AttachmentVm()
                {
                    Id = c.Id,
                    LastModifiedDate = c.LastModifiedDate,
                    CreateDate = c.CreateDate,
                    FileName = c.FileName,
                    FilePath = c.FilePath,
                    FileSize = c.FileSize,
                    FileType = c.FileType,
                    IssueId= c.IssueId
                }).ToListAsync();

            return Ok(query);
        }

        [HttpDelete("attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found attachment with id {attachmentId}"));
            await _storageService.DeleteFileAsync(attachment.FileName);
            _context.Attachments.Remove(attachment);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest(new ApiBadRequestResponse($"Delete attachment failed"));
        }

        #endregion Attachments
        [HttpGet("attachments/downloadFile")]
        public async Task<IActionResult> DownloadFile(string file)
        {
            var rootPath = _env.ContentRootPath;
            string directoryPath = string.Format(@"{0}/{1}"
                            , rootPath
                            , string.Format("/{0}/{1}", "wwwroot", "user-attachments"));
            string path = file;
            var filePath = Path.Combine(directoryPath, path);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", path);
        }
    }
}
