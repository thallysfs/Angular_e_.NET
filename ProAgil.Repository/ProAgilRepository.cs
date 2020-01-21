using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProAgil.Domain;

namespace ProAgil.Repository
{
    // os : IProAgilRepository esta referenciando a interface, logo tenho que implementar todos os métodos existentes nessa interface
    public class ProAgilRepository : IProAgilRepository
    {
        private readonly ProAgilContext _context;
        //adicionando o contexto que será injetado
        public ProAgilRepository(ProAgilContext context)
        {
            _context = context;
            //recurso para não travar minhas consultas no banco
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        }


        //GERAIS

        //trabalhar usando o T como uma classe permite ser mais genérico nata hora de usar qualuqer método
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Update<T>(T entity) where T : class
        {
             _context.Update(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
             _context.Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            //se o retorno for maior do que 0 ele retornará verdadeiro
            return (await _context.SaveChangesAsync()) > 0;
        }


    //EVENTOS
        // =false no parâmetro torna esse parâmerto opcional
        public async Task<Evento[]> GetAllEventoAsync(bool includePalestrantes = false)
        {
            IQueryable<Evento> query = _context.Eventos
            .Include(c => c.Lotes)
            .Include(c => c.RedesSociais);

            //se o meu include palestrantes for verdadeiro eu os trago na query
            if(includePalestrantes){
                //no primeiro include eu trarei somente a relação de palestrante e evento, por isso usei o then include para pegar somente o palestrante
                query = query
                .Include(p =>p.PalestrantesEventos)
                .ThenInclude(p => p.Palestrante);
            }

            query = query.OrderBy( c => c.Id);

            return await query.ToArrayAsync();


        }

        public async Task<Evento[]> GetAllEventoAsyncByTema(string tema, bool includePalestrantes)
        {
            IQueryable<Evento> query = _context.Eventos
            .Include(c => c.Lotes)
            .Include(c => c.RedesSociais);

            //se o meu include palestrantes for verdadeiro eu os trago na query
            if(includePalestrantes){
                //no primeiro include eu trarei somente a relação de palestrante e evento, por isso usei o then include para pegar somente o palestrante
                query = query
                .Include(p =>p.PalestrantesEventos)
                .ThenInclude(p => p.Palestrante);
            }

            query = query.OrderByDescending( c => c.DataEvento)
                    .Where(c => c.Tema.ToLower().Contains(tema.ToLower()));

            return await query.ToArrayAsync();
        }

        public async Task<Evento> GetEventoAsyncById(int EventoId, bool includePalestrantes)
        {
            IQueryable<Evento> query = _context.Eventos
            .Include(c => c.Lotes)
            .Include(c => c.RedesSociais);

            //se o meu include palestrantes for verdadeiro eu os trago na query
            if(includePalestrantes){
                //no primeiro include eu trarei somente a relação de palestrante e evento, por isso usei o then include para pegar somente o palestrante
                query = query
                .Include(p =>p.PalestrantesEventos)
                .ThenInclude(p => p.Palestrante);
            }

            query = query.OrderBy( c => c.Id)
                    .Where(c => c.Id == EventoId);

            return await query.FirstOrDefaultAsync();
        }

        //PALESTRANTES
        public async Task<Palestrante> GetPalestranteAsync(int PalestranteId, bool includeEventos = false)
        {
            //Crio uma query, faço uma consulta dos palestrantes incluindo as redes sociais
            IQueryable<Palestrante> query = _context.Palestrantes
            .Include(c => c.RedesSociais);

            //se o meu include palestrantes for verdadeiro eu os trago na query
            if(includeEventos){
                //no primeiro include eu trarei somente a relação de palestrante e evento, por isso usei o then include para pegar somente o palestrante
                query = query
                .Include(pe =>pe.PalestrantesEventos)
                .ThenInclude(e => e.Evento);
            }

            query = query.OrderBy( p => p.Nome)
            .Where(p => p.Id == PalestranteId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Palestrante[]> GetAllPalestrantesAsyncByName(string name, bool includeEventos = false)
        {
            //Crio uma query, faço uma consulta dos palestrantes incluindo as redes sociais
            IQueryable<Palestrante> query = _context.Palestrantes
            .Include(c => c.RedesSociais);

            //se o meu include palestrantes for verdadeiro eu os trago na query
            if(includeEventos){
                //no primeiro include eu trarei somente a relação de palestrante e evento, por isso usei o then include para pegar somente o palestrante
                query = query
                .Include(pe =>pe.PalestrantesEventos)
                .ThenInclude(e => e.Evento);
            }

            query = query.Where(p => p.Nome.ToLower().Contains(name.ToLower()));

            return await query.ToArrayAsync();
        }


    }
}