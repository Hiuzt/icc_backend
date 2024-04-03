using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly AppDbContext _appDbContext;
        private readonly IHostEnvironment _hostEnvironment;
        public UserController(AppDbContext appDbContext, IHostEnvironment hostEnvironment)
        {
            _appDbContext = appDbContext;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                var users = _appDbContext.Users;

                foreach (var user in users)
                {
                    user.ImagePath = String.Format("{0}://{1}{2}/profiles/{3}", HttpContext.Request.Scheme, HttpContext.Request.Host, HttpContext.Request.PathBase, user.ImagePath);

                }

                return Ok(new { success = true, data = new { message = "Sikeresen lekérted az adatokat!", users } });
            }
            catch
            {
                return BadRequest("Szerverhiba");
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        public IActionResult GetOneUser(int id)
        {
            try
            {
                var users = _appDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

                if (users == null)
                {
                    return BadRequest("Nincs ilyen felhasználó!");
                }

                return Ok(new { success = true, data = new { message = "Sikeresen lekérted az adatokat!", users } });
            }
            catch
            {
                return BadRequest("Szerverhiba");
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddUser(Users user)
        {
            user.ImagePath = "Nincs kép feltöltve";

            IFormFile imageFile = user.ImageFile!;
            user.ImagePath = await SaveImage(imageFile);

            _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { success = true, data = new { message = "Sikeresen létrehoztál egy felhasználót" } });
        }

        [NonAction]
        public async Task<string> SaveImage(IFormFile imageSource)
        {

            string imageName = new String(Path.GetFileNameWithoutExtension(imageSource.FileName).Take(10).ToArray()).Replace(" ", "-");
            imageName = imageName + DateTime.Now.ToString("yymmssff") + Path.GetExtension(imageSource.FileName);

            // Image path létrehozása
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Public/Profiles", imageName);

            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await imageSource.CopyToAsync(fileStream);
            }
            return imageName;
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = _appDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

                if (user == null)
                {
                    return BadRequest("Nincs ilyen felhasználó!");
                }

                _appDbContext.Users.Remove(user);
                await _appDbContext.SaveChangesAsync();

                return Ok(new { success = true, data = new { message = "Sikeresen kitörölted a felhasználót!" } });
            }
            catch
            {
                return BadRequest("Szerverhiba");
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, Users user)
        {

            try
            {
                var currentUser = _appDbContext.Users.Where(x => x.Id == id).FirstOrDefault();

                if (currentUser == null)
                {
                    return BadRequest("Nincs ilyen felhasználó!");
                }

                
                currentUser.Name = user.Name;
                currentUser.Email = user.Email;
                currentUser.Password = user.Password;
                currentUser.Role = user.Role;

                if (user.ImageFile != null)
                {
                    IFormFile imageFile = user.ImageFile;
                    currentUser.ImagePath = await SaveImage(imageFile);
                }

                await _appDbContext.SaveChangesAsync();

                return Ok(new { success = true, data = new { message = "Sikeresen szerkesztetted a felhasználót!", currentUser } });


            }
            catch
            {
                return BadRequest("Szerverhiba");
            }
        }

    }
}
