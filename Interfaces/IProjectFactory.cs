using TaskManagementSystem.Services.ProjectFactory;

namespace TaskManagementSystem.Interfaces
{
	public interface IProjectFactory
	{
        IProjects createuser(string User_id, string Name, string Description);
    }
}