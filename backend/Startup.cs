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
            // Notre cl?? secr??te pour les jetons sur le back-end     
            var key = Encoding.ASCII.GetBytes("my-super-secret-key");
            // On pr??cise qu'on veut travaille avec JWT tant pour l'authentification      
            // que pour la v??rification de l'authentification     
            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x => {
                    // On exige des requ??tes s??curis??es avec HTTPS             
                    x.RequireHttpsMetadata = true;
                    x.SaveToken = true;
                    // On pr??cise comment un jeton re??u doit ??tre valid??             
                    x.TokenValidationParameters = new TokenValidationParameters {
                        // On v??rifie qu'il a bien ??t?? sign?? avec la cl?? d??finie ci-dessus                 
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        // On ne v??rifie pas l'identit?? de l'??metteur du jeton                 
                        ValidateIssuer = false,
                        // On ne v??rifie pas non plus l'identit?? du destinataire du jeton                
                        ValidateAudience = false,
                        // Par contre, on v??rifie la validit?? temporelle du jeton                 
                        ValidateLifetime = true,
                        // On pr??cise qu'on n'applique aucune tol??rance de validit?? temporelle                 
                        ClockSkew = TimeSpan.Zero  //the default for this setting is 5 minutes             
                    };
                    // On peut d??finir des ??v??nements li??s ?? l'utilisation des jetons             
                    x.Events = new JwtBearerEvents {
                        // Si l'authentification du jeton est rejet??e ...                 
                        OnAuthenticationFailed = context => {
                            // ... parce que le jeton est expir?? ...                     
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException)) {
                                // ... on ajoute un header ?? destination du front-end indiquant cette expiration                         
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
                    // Utilisez cette ligne si vous voulez que VS lance le front-end angular quand vous d??marrez l'app          
                    //spa.UseAngularCliServer(npmScript: "start");                  
                    // Utilisez cette ligne si le front-end angular est ex??cut?? en dehors de VS (ou dans une autre instance de VS)            
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
