using System;
using System.Collections.Generic;

namespace KanbanApp.ViewModels.Systems
{
    public class UpdatePermissionRequest
    {
        public List<PermissionVm> Permissions { get; set; } = new List<PermissionVm>();

    }
}
