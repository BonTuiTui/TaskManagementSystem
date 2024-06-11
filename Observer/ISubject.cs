using System;
using Microsoft.AspNetCore.Hosting.Server;

namespace TaskManagementSystem.Observer
{
	public interface ISubject
	{
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void Notify(string message);

    }
}

