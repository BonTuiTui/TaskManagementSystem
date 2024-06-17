using System;
using TaskManagementSystem.Interfaces;

namespace TaskManagementSystem.Services.Observer
{
	public class TaskNotifier: ISubject
	{
		public TaskNotifier()
		{

		}
        private List<IObserver> observers = new List<IObserver>();

        public void Attach(IObserver observer)
        {
            observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void Notify(string message)
        {
            foreach (var observer in observers)
            {
                observer.Update(message).GetAwaiter().GetResult();
            }
        }
    }
}

