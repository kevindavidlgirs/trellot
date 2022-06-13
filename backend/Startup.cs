using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using prid_2021_g06.Models;
using prid_2021_g06.Service;

namespace prid_2021_g06 {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            //services.AddDbContext<g06Context>(opt => opt.UseInMemoryDatabase("g06"));
            // services.AddDbContext<g06Context>(opt => { 
            //     opt.UseLazyLoadingProxies(); 
            //     opt.UseSqlServer(Configuration.GetConnectionString("g06-mssql")); 
            // });

            services.AddDbContext<g06Context>(opt => {
                opt.UseLazyLoadingProxies();
                opt.UseMySql(Configuration.GetConnectionString("g06-mysql"));
            });

            services.AddControllers().AddNewtonsoftJson(opt => {
                /*   
                ReferenceLoopHandling.Ignore: Json.NET will ignore objects in reference loops and not serialize them. 
                The first time an object is encountered it will be serialized as usual but if the object is  
                encountered as a child object of itself the serializer will skip serializing it. 
                See: https://stackoverflow.com/a/14205542 and https://stackoverflow.com/a/58007282 
                */
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.AddMvc()
            .AddNewtonsoftJson(opt => {
                /*  
                ReferenceLoopHandling.Ignore: Json.NET will ignore objects in reference loops and not serialize them.
                The first time an object is encountered it will be serialized as usual but if the object is 
                encountered as a child object of itself the serializer will skip serializing it.
                See: https://stackoverflow.com/a/14205542 and https://stackoverflow.com/a/58007282
                */
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            //services.AddControllers().AddNewtonsoftJson(); 

            // In production, the Angular files will be served from this directory            
            //services.AddSpaStaticFiles(configuration => {configuration.RootPath = "ClientApp/dist";}); 
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "../frontend/dist"; });

            //------------------------------     
            // configure jwt authentication     
            //------------------------------      
            // Notre clé secrète pour les jetons sur le back-end     
            var key = Encoding.ASCII.GetBytes("my-super-secret-key");
            // On précise qu'on veut travaille avec JWT tant pour l'authentification      
            // que pour la vérification de l'authentification     
            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x => {
                    // On exige des requêtes sécurisées avec HTTPS             
                    x.RequireHttpsMetadata = true;
                    x.SaveToken = true;
                    // On précise comment un jeton reçu doit être validé             
                    x.TokenValidationParameters = new TokenValidationParameters {
                        // On vérifie qu'il a bien été signé avec la clé définie ci-dessus                 
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        // On ne vérifie pas l'identité de l'émetteur du jeton                 
                        ValidateIssuer = false,
                        // On ne vérifie pas non plus l'identité du destinataire du jeton                
                        ValidateAudience = false,
                        // Par contre, on vérifie la validité temporelle du jeton                 
                        ValidateLifetime = true,
                        // On précise qu'on n'applique aucune tolérance de validité temporelle                 
                        ClockSkew = TimeSpan.Zero  //the default for this setting is 5 minutes             
                    };
                    // On peut définir des événements liés à l'utilisation des jetons             
                    x.Events = new JwtBearerEvents {
                        // Si l'authentification du jeton est rejetée ...                 
                        OnAuthenticationFailed = context => {
                            // ... parce que le jeton est expiré ...                     
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException)) {
                                // ... on ajoute un header à destination du front-end indiquant cette expiration                         
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
                services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            // app.UseSignalR(routes => 
            //     routes.MapHub<GeneralHub>("/user")
            // );
            
            app.UseAuthentication();
            app.UseRouting();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapHub<GeneralHub>("/user");
            });

            app.UseSpa(spa => {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,                
                // see https://go.microsoft.com/fwlink/?linkid=864501                  
                spa.Options.SourcePath = "ClientApp";
                if (env.IsDevelopment()) {
                    // Utilisez cette ligne si vous voulez que VS lance le front-end angular quand vous démarrez l'app          
                    //spa.UseAngularCliServer(npmScript: "start");                  
                    // Utilisez cette ligne si le front-end angular est exécuté en dehors de VS (ou dans une autre instance de VS)            
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
