using Dekauto.Students.Service.Students.Service.Domain.Entities;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IGroupsRepository : IRepository<Group>
    {
        Task<Group> GetGroupByNameAsync(string groupName);
    }
}
