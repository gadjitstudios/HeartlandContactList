using Heartland.data;
using Heartland.models;

namespace Heartland.contracts{
    public interface IDbContext{
        Datasource<Contact> Contacts { get; set; }
     } 
}