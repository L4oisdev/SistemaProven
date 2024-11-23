using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaProven.Entity;

namespace SistemaProven.BLL.Interfaces
{
    public interface IMenuService
    {
        Task<List<Menu>> ObtenerMenus(int idUsuario);
    }
}
