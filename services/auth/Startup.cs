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

    public class Token
    {
        public Guid id;
        public string code;

        public Token(Guid userId = new Guid(), string userToken = null)
        {
            id = userId;
            code = userToken != null ? userToken : generateToken();
        }

        private string generateToken()
        {   
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 32).Select(s => s[random.Next(s.Length)]).ToArray());
        }
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
    public class AuthController : ControllerBase
    {
        private static Dictionary<string, Token> sessions = new Dictionary<string, Token>();
        private readonly UsersContext _context;

        public AuthController(UsersContext context)
        {
            _context = context;
        }

        [HttpGet("auth")]
        public async Task<string> Auth()
        {
            string token = Request.Headers["X-Token"];

            if (token == null || !sessions.ContainsKey(token))
            {
                this.HttpContext.Response.StatusCode = 404;
                return "";
            }

            this.HttpContext.Response.Headers["X-User-Id"] = sessions[token].id.ToString();

            return "";
        }

        [HttpPost("signin")]
        public async Task<string> SignIn([FromBody] User data)
        {
            var user = await _context.Users.FirstOrDefaultAsync(model => model.name == data.name && model.password == data.password);

            if (user == null)
            {
                this.HttpContext.Response.StatusCode = 404;
                return "";
            }

            Token token = new Token(user.id);
            sessions[token.code] = token;

            return token.code;
        }

        [HttpGet("signout")]
        public async Task<string> SignOut()
        {
            string token = Request.Headers["X-Token"];
            if (!sessions.ContainsKey(token))
            {
                this.HttpContext.Response.StatusCode = 404;
                return "";
            }
            sessions.Remove(token);
            return "signout";
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
