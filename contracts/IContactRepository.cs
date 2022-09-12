using System;
using System.Linq;
using System.Threading;
using Heartland.models;

namespace Heartland.contracts{
    public interface IContactRepository : IDisposable, IBaseRepository<Contact>{ 
        event EventHandler CountAddedEvent;
        void StartListeningForUpdateCount();
    } 
    
}