using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanbanApp.BackendServer.Data;
using KanbanApp.BackendServer.Helpers;
using KanbanApp.ViewModels.Contents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KanbanApp.BackendServer.Controllers
{
    public class LabelsController : BaseController
    {
        
        private readonly ApplicationDbContext _context;
        public LabelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var label = await _context.Labels.FindAsync(id);
            if (label == null)
                return NotFound(new ApiNotFoundResponse($"Label with id: {id} is not found"));

            var labelVm = new LabelVm()
            {
                Id = label.Id,
                Name = label.Name
            };

            return Ok(labelVm);
        }
    }
}
