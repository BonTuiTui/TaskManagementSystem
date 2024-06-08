using TaskManagementSystem.Models;
namespace TaskManagementSystem.ProjectFactory
{
	public interface IProjectFactory
	{
        IProjects createuser(string User_id, string Name, string Description);
    }
}