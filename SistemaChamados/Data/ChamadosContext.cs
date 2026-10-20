using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaChamados.Models;

namespace SistemaChamados.Data
{
    public class ChamadosContext : DbContext
    {
        public ChamadosContext(DbContextOptions<ChamadosContext> options) : base(options)
        {
        }

        //Tabelas do banco
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Chamado> Chamados { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar FK do Comentario para Usuario sem cascade
            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);
        }
        }
    }