using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaProven.BLL.Interfaces;
using System.Security.Cryptography;

namespace SistemaProven.BLL.Implementacion
{
    public class UtilidadesService : IUtilidadesService
    {
        public string GenerarClave()
        {
            string clave = Guid.NewGuid().ToString("N").Substring(0,6);
            return clave;
        }

        public string ConvertirSha256(string texto)
        {
            StringBuilder hashCreate = new StringBuilder();

            using(SHA256 has = SHA256.Create()) 
            {
                Encoding enc = Encoding.UTF8;

                byte[] result = has.ComputeHash(enc.GetBytes(texto));

                foreach (byte b in result) hashCreate.Append(b.ToString("x2"));

                return hashCreate.ToString();
            }
        }
    }
}
