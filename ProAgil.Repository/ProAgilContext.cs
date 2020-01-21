using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProAgil.Domain;
using ProAgil.Domain.Identity;

namespace ProAgil.Repository
{
    public class ProAgilContext : IdentityDbContext<User, Role, int, 
                                                    IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
                                                    IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        /*
            Um Data Context é um objeto do tipo System.Data.Linq.DataContext que suporta a atualização e 
            manutenção de um banco de dados para objetos conhecidos do LINQ efetuando o tratamento da c
            onexão do banco de dados, dessa forma ,para acessar as tabelas de um banco de dados elas 
            devem estar mapeadas e disponíveis em um objeto Data Context.
        */
        //Aqui onde serão criados classes e tabelas
        public ProAgilContext(DbContextOptions<ProAgilContext> options) : base (options){}
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Palestrante> Palestrantes { get; set; }
        public DbSet<PalestranteEvento> PalestranteEventos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<RedeSocial> RedeSociais { get; set; }

        /*especificando que a tabela de PalestranteEvento como tem relação de n para n, possui as seguintes chaves
        EventoId e PalestranteId */
        protected override void OnModelCreating(ModelBuilder modelBuilder){

            base.OnModelCreating(modelBuilder);

            // configuração necessária para relacionamento de n para n
            modelBuilder.Entity<UserRole>(UserRole =>{
                UserRole.HasKey(ur => new {ur.UserId, ur.RoleId});

                UserRole.HasOne(ur => ur.Role)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.RoleId)
                        .IsRequired();    
                
                UserRole.HasOne(ur => ur.User)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.UserId)
                        .IsRequired(); 
            });


            modelBuilder.Entity<PalestranteEvento>()
            .HasKey(PE => new {PE.EventoId, PE.PalestranteId});
        }
    }
}