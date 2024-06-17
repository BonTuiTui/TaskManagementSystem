using System;
using TaskManagementSystem.Services.Observer;

namespace TaskManagementSystem.Interfaces
{
	public interface ISubject
	{
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void Notify(string message);
    }
}