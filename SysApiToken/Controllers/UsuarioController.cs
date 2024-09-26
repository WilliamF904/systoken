using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SysApiToken.Auth;
using SysApiToken.Models;



namespace SysApiToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly BdContext _context;


         public UsuarioController(BdContext context)
        {
            _context = context;
            
        }
        private static readonly string _key = "ESFE2024SecretKeyForTokenAuthentication";
        private readonly JwtAuthentication _jwtAuthentication = new JwtAuthentication(_key);


        //{"login":"SysAdmin","password":"Admin2021"}  {"login": "roberto","password": "moran2024"}
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] object pUsuario)// Define el método como una acción asincrónica que devuelve un resultado HTTP. Recibe un objeto `pUsuario` enviado en el cuerpo de la solicitud HTTP.
        {
            var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            string strUsuario = JsonSerializer.Serialize(pUsuario);// Serializa el objeto `pUsuario` a una cadena JSON.
            Usuario usuario = JsonSerializer.Deserialize<Usuario>(strUsuario, option);

            Usuario usuario_auth = await _context.Usuario.Where(x => x.Login == usuario.Login && x.Password == _jwtAuthentication.EncriptarMD5(usuario.Password)).FirstOrDefaultAsync(); //.TOLIST SI SON MUCHOS
            // Consulta la base de datos para encontrar un usuario cuyo login y contraseña (después de encriptar la contraseña ingresada con MD5) coincidan.  `FirstOrDefaultAsync()` obtiene el primer resultado o null si no existe.

            if (usuario_auth != null && usuario_auth.Id > 0 && usuario.Login == usuario_auth.Login)
            {
                // Verificar si el usuario está activo (Estatus == 1) 
                if (usuario_auth.Estatus == 1)
                {  
                    if (usuario_auth.IdRol == 1)   // Verificar el rol del usuario Suponiendo que el rol 1 tiene acceso
                    { 
                        var token = _jwtAuthentication.Authenticate(usuario_auth);// Generar el token JWT para el usuario autenticado
                        return Ok(token); // Devolver el token si el usuario tiene el rol adecuado
                    }
                    else
                    {
                        return Unauthorized(new { message = "Acceso denegado. El usuario no tiene permisos suficientes." });// Si el usuario tiene un rol diferente al rol 1, denegar el acceso

                    }
                }
                else
                {        
                    return Unauthorized(new { message = "Acceso denegado. El usuario está inactivo." });// Si el usuario está inactivo (Estatus != 1)
                }
            }
            else
            {
                return Unauthorized(new { message = "Acceso denegado. Credenciales invalidas." });// Si no se encuentra el usuario o las credenciales son incorrectas, se devuelve una respuesta HTTP 401 (Unauthorized).
            }
        }


        // GET: api/Usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuario()
        {
            return await _context.Usuario.ToListAsync();
        }


        [HttpGet]
        [Route("Lista")]
        public IActionResult Lista()
        {
            List<Usuario> lista = new List<Usuario>();

            try
            {
                lista = _context.Usuario.Include(c => c.IdRolv).ToList();

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new { mensaje = ex.Message, response = lista });
            }
        }


        // GET: api/Usuario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);

            if (usuario == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            return usuario;
        }


      
        // PUT: api/Usuario/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest("El ID en la solicitud no coincide con el ID del usuario.");

            }

            // Encripta la contraseña antes de almacenar el usuario
            usuario.Password = _jwtAuthentication.EncriptarMD5(usuario.Password);
            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound(new { message = $"Usuario con ID {id} no encontrado." });
                }
                else
                {
                    throw;
                }
            }

            return Ok("El usuario ha sido modificado correctamente.");

        }


        // POST: api/Usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // Encripta la contraseña antes de almacenar el usuario
            usuario.Password = _jwtAuthentication.EncriptarMD5(usuario.Password);

            _context.Usuario.Add(usuario);// Añade el usuario a la base de datos
            await _context.SaveChangesAsync();// Guarda los cambios de forma asíncrona

            // Devuelve una respuesta indicando que el recurso se ha creado correctamente
            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }


        // DELETE: api/Usuario/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            _context.Usuario.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok("El usuario ha sido eliminado correctamente.");

        }


        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.Id == id);
        }
    }
}
