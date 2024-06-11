using System;
namespace TaskManagementSystem.Observer
{
    public interface IObserver
    {
         Task Update(string message);
    }

}
