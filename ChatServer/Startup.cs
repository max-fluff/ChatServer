using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
 
namespace ChatServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }
 
        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseDeveloperExceptionPage();
 
            app.UseDefaultFiles();
            app.UseStaticFiles();
 
            app.UseRouting();
 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/webchat");
            });
        }
    }
}