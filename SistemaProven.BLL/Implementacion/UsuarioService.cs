﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.Net;
using SistemaProven.BLL.Interfaces;
using SistemaProven.DAL.Interfaces;
using SistemaProven.Entity;

namespace SistemaProven.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly IFireBaseService _fireBaseService;
        private readonly ICorreoService _CorreoService;
        private readonly IUtilidadesService _UtildadesService;

        public UsuarioService(
            IGenericRepository<Usuario> repositorio,
            IFireBaseService fireBaseService,
            ICorreoService CorreoService,
            IUtilidadesService UtildadesService
            )
        {
            _repositorio = repositorio;
            _fireBaseService = fireBaseService;
            _CorreoService = CorreoService;
            _UtildadesService = UtildadesService;
        }

        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repositorio.Consultar();
            return query.Include(r => r.IdRolNavigation).ToList();
        }

        public async Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "")
        {
            Usuario UsuarioExixte = await _repositorio.Obtener(u => u.Correo == entidad.Correo);

            if (UsuarioExixte != null) throw new TaskCanceledException("El correo ya existe");

            try
            {
                string claveGenerada = _UtildadesService.GenerarClave();
                entidad.Clave = _UtildadesService.ConvertirSha256(claveGenerada);
                entidad.NombreFoto = NombreFoto;

                if (Foto != null)
                {
                    string urlFoto = await _fireBaseService.SubirStorage(Foto, "carpeta_usuario", NombreFoto);
                    entidad.UrlFoto = urlFoto;
                }

                Usuario usuarioCreado = await _repositorio.Crear(entidad);

                if (usuarioCreado.IdUsuario == 0) throw new TaskCanceledException("No se pudo crear el usurio");

                if(UrlPlantillaCorreo != "") {
                    UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[correo]", usuarioCreado.Correo).Replace("[clave]", claveGenerada);
                    string htmlCorreo = "";

                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(UrlPlantillaCorreo);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if(response.StatusCode == HttpStatusCode.OK)
                    {
                        using(Stream dataStream = response.GetResponseStream())
                        {
                            StreamReader readerStream = null;

                            if(response.CharacterSet == null) readerStream = new StreamReader(dataStream);

                            else readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));

                            htmlCorreo = readerStream.ReadToEnd();
                            response.Close();
                            readerStream.Close();


                        }
                    }

                    if (htmlCorreo != "") await _CorreoService.EnviarCorreo(usuarioCreado.Correo, "Cuenta Creada", htmlCorreo);
                }

                IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == usuarioCreado.IdUsuario);
                usuarioCreado = query.Include(r  => r.IdRolNavigation).First();
                return usuarioCreado;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
        public async Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {
            Usuario UsuarioExixte = await _repositorio.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);

            if (UsuarioExixte != null) throw new TaskCanceledException("El correo ya existe");

            try
            {
                IQueryable<Usuario> queryUsuario = await _repositorio.Consultar(u => u.IdUsuario == entidad.IdUsuario);
                Usuario usuario_editar = queryUsuario.First();

                usuario_editar.Nombre = entidad.Nombre;
                usuario_editar.Correo = entidad.Correo;
                usuario_editar.Telefono = entidad.Telefono;
                usuario_editar.IdRol = entidad.IdRol;

                if(usuario_editar.NombreFoto == "") usuario_editar.NombreFoto = entidad.NombreFoto;

                if(Foto!= null)
                {
                    string urlFoto = await _fireBaseService.SubirStorage(Foto, "carpeta_usuario", usuario_editar.NombreFoto);
                    usuario_editar.UrlFoto = urlFoto;
                }

                bool respuesta = await _repositorio.Editar(usuario_editar);
                if(respuesta) throw new TaskCanceledException("No se pudo modificar el usuario");

                Usuario usuario_editado = queryUsuario.Include(r => r.IdRolNavigation).First();

                return usuario_editado;

            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int IdUsuario)
        {
            try
            { 
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);

                if (usuario_encontrado == null) throw new TaskCanceledException("El usuario no existe");

                string nombreFoto = usuario_encontrado.NombreFoto;
                bool repuesta = await _repositorio.Eliminar(usuario_encontrado);

                if (repuesta) await _fireBaseService.EliminarStorage("carpeta_usuario", nombreFoto);

                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<Usuario> ObtenerPorCredenciales(string correo, string clave)
        {
            string claveEncriptada = _UtildadesService.ConvertirSha256(clave);

            Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo.Equals(correo)
            && u.Clave.Equals(claveEncriptada));

            return usuario_encontrado;
        }

        public async Task<Usuario> ObtenerPorId(int IdUsuario)
        {
            IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == IdUsuario);

            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();
            return resultado;
        }

        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == entidad.IdUsuario);

                if(usuario_encontrado == null) throw new TaskCanceledException("El usuario no existe");

                usuario_encontrado.Correo = entidad.Correo;
                usuario_encontrado.Telefono = entidad.Telefono;

                bool repuesta = await _repositorio.Editar(usuario_encontrado);

                return repuesta;
            }
            catch
            {
                throw;

            }
        }
        public async Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string ClaveNueva)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);

                if (usuario_encontrado == null) throw new TaskCanceledException("El usuario no existe");

                if(usuario_encontrado.Clave != _UtildadesService.ConvertirSha256(ClaveActual)) throw new TaskCanceledException("La contraseña ingresada como actual no es correcta");

                usuario_encontrado.Clave = _UtildadesService.ConvertirSha256(ClaveNueva);

                bool repuesrta = await _repositorio.Editar(usuario_encontrado);

                return repuesrta;

            }
            catch (Exception ex) 
            {
                throw;
            }
        }

        public async Task<bool> RestablecerClave(string correo, string UrlPlantillaCorreo)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo == correo);

                if (usuario_encontrado == null) throw new TaskCanceledException("No encontramos ningun usuario asociado al correo");

                string claveGenerada = _UtildadesService.GenerarClave();
                usuario_encontrado.Clave = _UtildadesService.ConvertirSha256(claveGenerada);

                UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", claveGenerada);
                string htmlCorreo = "";

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(UrlPlantillaCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader readerStream = null;

                        if (response.CharacterSet == null) 
                            readerStream = new StreamReader(dataStream);
                        else 
                            readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));

                        htmlCorreo = readerStream.ReadToEnd();
                        response.Close();
                        readerStream.Close();


                    }
                }

                bool correoEnviado = false;

                if (htmlCorreo != "") 
                    correoEnviado = await _CorreoService.EnviarCorreo(correo, "Contraseña Restablecida", htmlCorreo);

                if(!correoEnviado) 
                    throw new TaskCanceledException("A ocurrido un error. Inténtalo de nuevo más tarde");

                bool repuesta = await _repositorio.Editar(usuario_encontrado);

                return repuesta;
            }
            catch
            {
                throw;
            }
        }
    }
}
