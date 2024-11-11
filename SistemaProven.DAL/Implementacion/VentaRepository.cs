using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SistemaProven.DAL.DBContext;
using SistemaProven.DAL.Interfaces;
using SistemaProven.Entity;

namespace SistemaProven.DAL.Implementacion
{
    public class VentaRepository: GenericRepository<Venta>, IVentaRepository
    {
        private readonly BaseVentasContext _dbContext;

        public VentaRepository(BaseVentasContext dbContext): base(dbContext)
        {
                _dbContext = dbContext;
        }

        public async Task<Venta> Registrar(Venta venta)
        {
            Venta ventaGenerada = new Venta();

            using(var trasaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach(DetalleVenta dv in venta.DetalleVenta)
                    {
                        Producto productoEncontrado = _dbContext.Productos.Where(p => p.IdProducto == dv.IdProducto).First();

                        productoEncontrado.Stock = productoEncontrado.Stock - dv.Cantidad;
                        _dbContext.Productos.Update(productoEncontrado);  
                    }
                    await _dbContext.SaveChangesAsync();

                    NumeroCorrelativo correlativo = _dbContext.NumeroCorrelativos.Where(n => n.Gestion == "Venta").First();

                    correlativo.UltimoNumero = correlativo.UltimoNumero + 1;
                    correlativo.FechaActualizacion = DateTime.Now;

                    _dbContext.NumeroCorrelativos.Update(correlativo);
                    await _dbContext.SaveChangesAsync();

                    string ceros = string.Concat(Enumerable.Repeat("0", correlativo.CantidadDigitos.Value));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - correlativo.CantidadDigitos.Value, correlativo.CantidadDigitos.Value);

                    venta.NumeroVenta = numeroVenta;

                    await _dbContext.AddAsync(venta);
                    await _dbContext.SaveChangesAsync();

                    ventaGenerada = venta;

                    trasaction.Commit();

                }
                catch (Exception ex)
                {
                    trasaction.Rollback();
                    throw ex;

                }
                return ventaGenerada;
            }
        }

        public async Task<List<DetalleVenta>> Reporte(DateTime FechaInicio, DateTime FechaFinal)
        {
            List<DetalleVenta> listaResumen = await _dbContext.DetalleVenta
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(u => u.IdUsuarioNavigation)
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(tdv => tdv.IdTipoDocumentoVentaNavigation)
                .Where(dv => dv.IdVentaNavigation.FechaRegistro.Value.Date >= FechaInicio.Date && 
                    dv.IdVentaNavigation.FechaRegistro.Value.Date <= FechaFinal.Date).ToListAsync();

            return listaResumen;
        }
    }
}
