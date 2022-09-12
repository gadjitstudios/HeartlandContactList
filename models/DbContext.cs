using Heartland.contracts;
using Heartland.data;

namespace Heartland.models{
    public class DbContext : IDbContext{
        public Datasource<Contact> Contacts { get; set; }
        
        public DbContext()
        {
            Contacts = new Datasource<Contact>();
        }
    }
}