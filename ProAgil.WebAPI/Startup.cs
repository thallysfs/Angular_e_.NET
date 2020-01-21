using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProAgil.Domain.Identity;
using ProAgil.Repository;

namespace ProAgil.WebAPI
{
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
    
           services.AddDbContext<ProAgilContext>(
               x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
            );

            IdentityBuilder builder = services.AddIdentityCore<User>(options =>
            {   /*
                    Removendo validações padrões da senha. Na orde:
                    nessa primeira opção eu desativo a obrigatoriedade de ter caractere especial em password
                    desativo a validação de Alpha numérico
                    remover a validação de minúsculas na senha
                    remover a validação de maiúsculas
                    Defino a quantidade máxima de caracteres para 4 na senha
            
                */
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
            });

            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            builder.AddEntityFrameworkStores<ProAgilContext>();
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();

            // configurações do jwt
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>{
                        options.TokenValidationParameters = new TokenValidationParameters{
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                                // AppSettings:Token está criado no arquivo 'appsettings.Development.json'
                                .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                            ValidateIssuer = false,
                            ValidateAudience = false    

                        };
                    });


            services.AddMvc(optoins =>{
                // sempre que eu chamar o controller ele terá que respeitar essa configuração abaixo
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                optoins.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            // nesse ponto ele evitar loops infinitos da relação dos meus Roles e meu usuário
            .AddJsonOptions(opt => opt.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddScoped<IProAgilRepository, ProAgilRepository>();
            //configurando para a aplicação trabalhar com automapper
            services.AddAutoMapper();
            //permissão de configuração de CORS.
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // essa linha vai informar a aplicação que o usuário tem que ser autenticado.
            app.UseAuthentication();

            //app.UseHttpsRedirection();
            //parametrização das permissões de CORs.
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseStaticFiles(new StaticFileOptions(){
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
                RequestPath = new PathString("/Resources")
            });
            app.UseMvc();
        }
    }
}
