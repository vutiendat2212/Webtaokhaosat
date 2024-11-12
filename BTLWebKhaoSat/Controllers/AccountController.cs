using BTLWebKhaoSat.Models;
using Microsoft.AspNetCore.Mvc;
using BTLWebKhaoSat.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

public class AccountController : Controller
{
    private readonly SurveydbContext _context;

    public AccountController(SurveydbContext context)
    {
        _context = context;
    }

    // GET: Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    public IActionResult Register(UserRegisterViewMode model)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra xem tên đăng nhập đã tồn tại chưa
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password),
                RoleId = 2 ,// Default role is User
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        return View(model);
    }

    // GET: Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var user = _context.Users.SingleOrDefault(u => u.Username == username);

        if (user != null && VerifyPassword(password, user.PasswordHash))
        {
            HttpContext.Session.SetString("UserID", user.UserId.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("RoleId", user.RoleId.ToString());

            if (user.RoleId == 1)
                return RedirectToAction("Index", "Admin");
            else
                return RedirectToAction("Index", "Survey");
        }

        ModelState.AddModelError("", "Invalid username or password");
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
}
