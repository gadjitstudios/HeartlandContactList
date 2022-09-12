using System.Threading.Tasks;
using Heartland.contracts;
using Heartland.models;
using Heartland.repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Heartland
{
    class Program
    {
        static async Task Main()
        {
            var services = ConfigureServices();    
            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetService<ContactsListApp>().Run();
        } 
        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();    
            services.AddScoped<IContactRepository, ContactRepository>();    
            services.AddSingleton<IDbContext, DbContext>();
            services.AddTransient<ContactsListApp>();    
            return services;
        }
    }
}
