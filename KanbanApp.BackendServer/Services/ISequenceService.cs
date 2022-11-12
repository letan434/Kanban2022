using System;
using System.Threading.Tasks;

namespace KanbanApp.BackendServer.Services
{
    public interface ISequenceService
    {
        Task<int> GetIssueNewId();
    }
}
