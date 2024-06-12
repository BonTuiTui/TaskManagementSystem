using System;
namespace TaskManagementSystem.Interfaces
{
    public interface IObserver
    {
         Task Update(string message);
    }

}
