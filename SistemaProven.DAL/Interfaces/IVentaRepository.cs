using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaProven.Entity;

namespace SistemaProven.DAL.Interfaces
{
    public interface IVentaRepository: IGenericRepository<Venta>
    {
        Task<Venta> Regisstrar(Venta venta);

        Task<List<DetalleVenta>> Reporte(DateTime FechaInicio, DateTime FechaFinal);
    }
}
