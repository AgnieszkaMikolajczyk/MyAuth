using MyAuth.Enums;
using MyAuth.Models.EDM;
using MyAuth.Models.UserManagementModels;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MyAuth.Controllers
{
    public class UserManagementController : Controller
    {
        private const int numberOfIterations = 10000;
        private const int saltLength = 16;
        private const int hashLength = 20;

        [AllowAnonymous]
        public ActionResult SignIn(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(SignInViewModel model, string whereToUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (MyAuthContext ctx = new MyAuthContext())
            {
                var user = (from u in ctx.Users
                            join r in ctx.Roles on u.RoleID equals r.ID
                            where u.Email == model.Email
                            select new UserProfile
                            {
                                Email = u.Email,
                                Name = u.Name,
                                Role = r.Name,
                            }).FirstOrDefault();

                if (user != null && IsPasswordValid(model.Email, model.Password))
                {
                    var authTicket = new FormsAuthenticationTicket(1, user.Name, DateTime.Now, DateTime.Now.AddMinutes(20), model.RememberMe, user.Role);
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    HttpContext.Response.Cookies.Add(authCookie);
                    return RedirectToLocal(whereToUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
                }
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private bool IsPasswordValid(string email, string passwordInput)
        {
            using (MyAuthContext ctx = new MyAuthContext())
            {
                //Fetch the stored hashed password value from the database
                string savedPasswordHashString = (from u in ctx.Users
                                            where u.Email == email
                                            select u.Password.ToString()).FirstOrDefault();
                
                byte[] savedPasswordHashBytes = Convert.FromBase64String(savedPasswordHashString);

                //Retrieve salt
                byte[] saltBytes = new byte[saltLength];
                Array.Copy(savedPasswordHashBytes, 0, saltBytes, 0, saltLength);

                //Salt the password input with the salt from the password saved in the database
                var pbkdf2 = new Rfc2898DeriveBytes(passwordInput, saltBytes, numberOfIterations);
                
                //Hash the salted input
                byte[] passwordInputHashBytes = pbkdf2.GetBytes(hashLength);

                //Compare the obtained hashed value to the hashed password from the database
                for (int i = 0; i < hashLength; i++)
                {
                    if (savedPasswordHashBytes[i + saltLength] != passwordInputHashBytes[i])
                        return false;
                }
                return true;
            }
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegistrationViewModel model)
        {
            //invalid input data
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (MyAuthContext ctx = new MyAuthContext())
            {
                //user already exists
                var user = (from u in ctx.Users
                            where u.Email == model.Email
                            select new UserProfile { Email = u.Email }).FirstOrDefault();

                if (user != null)
                {
                    ModelState.AddModelError("", "User with this email address already exists");
                    return View(model);
                }

                //create new user
                Users newUser = new Users()
                {
                    Email = model.Email,
                    Name = model.Name,
                    RoleID = (int)RoleEnum.Sales,
                    Password = HashPassword(model.Password)
                };
                ctx.Users.Add(newUser);
                ctx.SaveChanges();
            }

            ViewBag.Message = "You have been successfully registered!";
            return View();
        }
        private string HashPassword(string password)
        {
            //Generate a long random salt using a CSPRNG
            byte[] saltBytes;
            new RNGCryptoServiceProvider().GetBytes(saltBytes = new byte[saltLength]);

            //Salt the password
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, numberOfIterations);

            //Hash password and salt together
            byte[] hashBytes = pbkdf2.GetBytes(hashLength);

            //Concatenate the hashed value and the salt in one array
            byte[] saltAndHashBytes = new byte[saltLength + hashLength];
            Array.Copy(saltBytes, 0, saltAndHashBytes, 0, saltLength);
            Array.Copy(hashBytes, 0, saltAndHashBytes, saltLength, hashLength);

            //Convert the concatenated hashed value (password + salt) and salt to string
            string savedPasswordHash = Convert.ToBase64String(saltAndHashBytes);
            return savedPasswordHash;

        }

       
    }
}