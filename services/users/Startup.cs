using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace backend
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid id { get; set; }
        public string name { get; set; }
        public string password { get; set; }
    }

    public class UsersContext : DbContext
    {
        public UsersContext(DbContextOptions<UsersContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users_user");
        }
    }

    [ApiController]
    [Route("/")]
    public class UsersController : ControllerBase
    {
        private static List<string> sessions = new List<string>();
        private readonly UsersContext _context;

        public UsersController(UsersContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<User> Create([FromBody] User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        [HttpGet("me")]
        public async Task<User> View()
        {
            string id = Request.Headers["X-User-Id"];
            Guid guid = new Guid();

            try {
                guid = new Guid(id);
            } catch (Exception exception) {
                this.HttpContext.Response.StatusCode = 404;
                return new User();
            }
            
            User user = await _context.Users.FirstOrDefaultAsync(model => model.id == guid);

            if (user == null)
            {
                this.HttpContext.Response.StatusCode = 404;
                return new User();
            }
            return user;
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string dbUser = Environment.GetEnvironmentVariable("DB_USER");
            string dbPass = Environment.GetEnvironmentVariable("DB_PASS");
            string dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            string dbPort = Environment.GetEnvironmentVariable("DB_POSRT");
            string dbName = Environment.GetEnvironmentVariable("DB_NAME");
            
            services.AddDbContext<UsersContext>(options => options.UseNpgsql($"User ID={dbUser};Password={dbPass};Host={dbHost};Port={dbPort};Database={dbName};Pooling=true;"));

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
