using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Heartland.contracts;
using Heartland.data;
using Heartland.models;
using Heartland.util;

namespace Heartland.repositories{
    public class ContactRepository : IContactRepository
    {
        public event EventHandler CountAddedEvent;
        private IDbContext _database;
        private EventThread thread;
        private EventWaitHandle UpdateCountHandle;
        private EventWaitHandle ResetUpdateCountHandle;
        System.Timers.Timer _updatSetTimer;
        System.Timers.Timer _updatResetTimer;
        public ContactRepository(IDbContext database)
        {
            _database = database;
                string  EVENT_NAME = "HeartlandResetUpdateCountEvent";
                EventWaitHandle.TryOpenExisting(EVENT_NAME, out ResetUpdateCountHandle);
                if(ResetUpdateCountHandle == null)
                    ResetUpdateCountHandle = new EventWaitHandle(false, EventResetMode.ManualReset, EVENT_NAME);

                EVENT_NAME = "HeartlandUpdateCountEvent";
                EventWaitHandle.TryOpenExisting(EVENT_NAME, out UpdateCountHandle);
                if(UpdateCountHandle == null)
                    UpdateCountHandle = new EventWaitHandle(false, EventResetMode.ManualReset, EVENT_NAME);

                thread = new EventThread();
        }

        ///<summary>
        /// Adds a contact to the DbContext
        ///</summary>
        public async void Add(Contact t)
        {
            await _database.Contacts.Add(t);
            UpdateCountHandle.Set();
            _updatSetTimer = new System.Timers.Timer(1000);
            _updatSetTimer.Elapsed += ResetHandles;
            _updatSetTimer.Start();
        }

        ///<summary>
        /// Resets the EventWaitHandles
        ///</summary>
        private void ResetHandles(Object sender, ElapsedEventArgs args){
            UpdateCountHandle.Reset();
            ResetUpdateCountHandle.Set();
            _updatResetTimer = new System.Timers.Timer(1000);
            _updatResetTimer.Elapsed += ((Object sender, ElapsedEventArgs args)=> ResetUpdateCountHandle.Reset());
            _updatResetTimer.Start();
        }

        ///<summary>
        /// Gets a contact count from the DbContext
        ///</summary>
        public Task<int> GetCount()
        {
            return _database.Contacts.GetCount();
        }

        ///<summary>
        /// Start the EventWaitListeningThread
        ///</summary>
        public void StartListeningForUpdateCount(){
            if(thread != null)
                thread.StartListening(UpdateCountHandle, ResetUpdateCountHandle, CountAddedEvent);
        }

        ///<summary>
        /// Stop the EventWaitListeningThread
        ///</summary>
        public void StopListeningForUpdateCount(){
            if(thread != null)
                thread.StopListening();
        }

        ///<summary>
        /// Dipose of resources
        ///</summary>
        public void Dispose()
        {
            thread.StopListening();
        }
    }
}