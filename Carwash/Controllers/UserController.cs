using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Carwash.Models;
using Carwash.Models.ViewModel;

namespace Carwash.Controllers
{
    public class UserController : Controller
    {
        carwashEntities db = new carwashEntities();
        string urlDomain = "https://localhost:44325/";

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration(users user, string confirmPassword)
        {

            try
            {
                if (db.users.Any(u => u.email == user.email))
                {
                    ModelState.AddModelError("email", "Este email ya está registrado.");
                    return View(user);
                }
                if (user.password != confirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
                    return View(user);
                }

                string passwordPattern = @"^(?=.*[!@#$%^&*(),.?"":{}|<>])(?=.*[A-Z])(?=.*\d).+$";
                if (!Regex.IsMatch(user.password, passwordPattern))
                {
                    ModelState.AddModelError("password", "La contraseña debe contener al menos un carácter especial, una mayúscula y un número.");
                    return View(user);
                }


                string activationToken = GenerateActivationToken(user);
                string hashedPassword = HashPassword(user.password.Trim().Replace(" ", ""));
                user.email = user.email.Trim().Replace(" ", "");
                user.isActive = false;
                user.password = hashedPassword;
                user.role_id = 3;
                user.activation_token = activationToken;
                db.users.Add(user);
                db.SaveChanges();

                TempData["ActivationEmail"] = user.email;
                SendActivationMail(user.email, activationToken);

                return RedirectToAction("ActivationSent");
            }
            catch (DbEntityValidationException ex)
            {

                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                return View(user);
            }


        }

        public ActionResult ActivationSent()
        {
            string activationEmail = TempData["ActivationEmail"] as string;
            ViewBag.ActivationEmail = activationEmail;
            return View();
        }

        [HttpGet]
        public ActionResult ActivateAccount(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivateAccount(ActivateAccountViewModel model, string token)
        {

            ActivateAccountViewModel tokenModel = new ActivateAccountViewModel();
            tokenModel.Token = token;

            if (ModelState.IsValid)
            {
                var user = db.users.FirstOrDefault(u => u.activation_token == token);


                if (user == null)
                {
                    ModelState.AddModelError("", "Esta cuenta ya se encuentra activa o el token ha expirado");
                    return View(model);
                }

                var comparePass = VerifyPassword(model.Password, user.password.Trim().Replace(" ", ""));

                if ((user.email.Trim() == model.Email) && comparePass)
                {
                    user.isActive = true;
                    user.activation_token = null;
                    db.SaveChanges();

                    FormsAuthentication.SetAuthCookie(model.Email, false);

                    return RedirectToAction("Index", "User");
                }
                else
                {
                    ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                }
            }

            ViewBag.Token = token;
            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(users user)
        {
            if (ModelState.IsValid)
            {
                var loginUser = db.users.SingleOrDefault(u => u.email.Trim() == user.email);

                if (loginUser != null)
                {
                    bool isActive = loginUser.isActive;

                    if (!isActive)
                    {
                        ModelState.AddModelError("", "Esta cuenta no se encuentra activa. Regístrese o revise su correo para activarla.");
                        return View(user);
                    }

                    // Verificar la contraseña utilizando una función hash segura
                    bool isPasswordValid = VerifyPassword(user.password, loginUser.password);

                    if (isPasswordValid)
                    {
                        Session["LoggedInUser"] = loginUser;
                        Session["Email"] = loginUser.email;
                        if (loginUser.role_id == 1)
                        {
                            Session["Rol"] = "Administrador";
                            NotificationsList();
                        } 
                        else
                        {
                            Session["Rol"] = "Cliente";
                        }
                        return RedirectToAction("Index", "User");
                    }
                } 
                else
                {
                    ModelState.AddModelError("", "El usuario no existe.");
                    return View();
                }

                ModelState.AddModelError("", "Credenciales incorrectas.");
                return View(user);
            }

            return View(user);
        }

        [HttpPost]
        public JsonResult NotificationsList()
        {
            string rol = Session["Rol"] as string;

            if (rol == "Administrador")
            {
                List<notifications> notifications = db.notifications.ToList();
                return Json(notifications);
            }

            return Json(new { errorMessage = "No tiene permisos de administrador." });
        }

        [HttpPost]
        public JsonResult DeleteNotification(int notification_id)
        {
            try
            {
                var notification = db.notifications.Find(notification_id);
                if (notification != null)
                {
                    db.notifications.Remove(notification);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Notificación no encontrada." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar la notificación." });
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "User");
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public ActionResult StartRecovery(string token)
        {
            Models.ViewModel.RecoveryPasswordViewModel model = new Models.ViewModel.RecoveryPasswordViewModel();
            model.token = token;
            using (carwashEntities db = new carwashEntities())
            {
                if (model.token == null || model.token.Trim().Equals(""))
                {
                    ViewBag.Error = "El token ha expirado";
                    return View("StartRecovery");
                }
                var oUser = db.users.Where(d => d.token_recovery == model.token).FirstOrDefault();
                if (oUser == null)
                {
                    ViewBag.Error = "El token ha expirado";
                    return View("StartRecovery");
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Recovery(Models.ViewModel.RecoveryPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("StartRecovery", model);
                }

                using (Models.carwashEntities db = new Models.carwashEntities())
                {
                    var oUser = db.users.Where(d => d.token_recovery == model.token).FirstOrDefault();
                    string passwordPattern = @"^(?=.*[!@#$%^&*(),.?"":{}|<>])(?=.*[A-Z])(?=.*\d).+$";
                    if (!Regex.IsMatch(model.Password, passwordPattern))
                    {
                        ModelState.AddModelError("", "La contraseña debe contener al menos un carácter especial, una mayúscula y un número.");
                        return View("StartRecovery", model);
                    }
                    if (oUser != null)
                    {
                        oUser.password = HashPassword(model.Password.Trim().Replace(" ", ""));
                        oUser.token_recovery = null;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            ViewBag.Message = "Contraseña modificada con exito";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult PasswordRecovery()
        {
            Models.ViewModel.RecoveryViewModel model = new Models.ViewModel.RecoveryViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult PasswordRecovery(Models.ViewModel.RecoveryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("ForgotPassword", model);
                }

                string token = GetSha256(Guid.NewGuid().ToString());

                using (carwashEntities db = new carwashEntities())
                {
                    var oUser = db.users.Where(d => d.email == model.Email).FirstOrDefault();
                    if (oUser != null)
                    {
                        oUser.token_recovery = token;
                        db.Entry(oUser).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        // Enviar mail
                        SendEmail(oUser.email, token);
                    }
                }

                return View();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        
        public ActionResult UsersList()
        {
            string rol = Session["Rol"] as string;

            if (rol.Equals("Administrador"))
            {
                List<users> users = db.users.ToList(); // Obtener todos los usuarios de la tabla

                return View(users);
            }
            else {
                return RedirectToAction("Login", "User");
            }
        }

        public ActionResult UserEdit()
        {
            return View();
        }

        public ActionResult UserEdition()
        {
            var loggedInUser = Session["LoggedInUser"] as users;
            return View(loggedInUser);
        }

        [HttpPost]
        public ActionResult UpdateProfile(users updatedUser)
        {
            var loggedInUser = Session["LoggedInUser"] as users;

            if (ModelState.IsValid)
            {
                using (var db = new carwashEntities())
                {
                    var userToUpdate = db.users.Find(loggedInUser.user_id);

                    if (userToUpdate != null)
                    {
                        userToUpdate.phone = updatedUser.phone;
                        userToUpdate.name = updatedUser.name;

                        db.Entry(userToUpdate).State = EntityState.Modified;
                        db.SaveChanges();

                        Session["LoggedInUser"] = userToUpdate;
                        return RedirectToAction("UserEdit", "User");
                    }
                }
            }

            return View("UserEdition", updatedUser);
        }

        public ActionResult UpdateAll(string[] lista)
        {
            string rol = Session["Rol"] as string;

            if (rol.Equals("Administrador"))
            {
                var users = db.users.ToList(); // Obtener todos los usuarios de la base de datos
                int contador = 0;
                foreach (var user in users)
                {

                    user.role_id = int.Parse(lista[contador]);
                    contador++;

                    db.users.Attach(user);


                    db.Entry(user).State = EntityState.Modified;

                    db.SaveChanges();
                }


                return RedirectToAction("UsersList");
            }
            else {
                return RedirectToAction("Login", "User");
            }
                
        }

   
        public ActionResult Delete(int id)
        {
            string rol = Session["Rol"] as string;

            if (rol.Equals("Administrador"))
            {
                var lg = db.users.Where(a => a.user_id.Equals(id)).FirstOrDefault();

                db.users.Remove(lg);
                db.SaveChanges();
                return RedirectToAction("UsersList");
            }
            else {
                return RedirectToAction("Login", "User");
            }

                
        }

        #region HELPERS
        private string GenerateActivationToken(users user)
        {
            string token = Guid.NewGuid().ToString();
            return token;
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {

            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
            byte[] enteredHash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != enteredHash[i])
                    return false;
            }

            return true;
        }

        private string HashPassword(string password)
        {

            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);


            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);


            byte[] hash = pbkdf2.GetBytes(20);


            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);


            string hashedPassword = Convert.ToBase64String(hashBytes);

            return hashedPassword;
        }
        private string GetSha256(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

        private void SendEmail(string toAddress, string token)
        {
            string fromAddress = "";
            string Contrasena = "";
            string url = urlDomain + "/User/StartRecovery/?token=" + token;
            string subject = "Recuperación de contraseña";
            string body = "<p>Correo para recuperación de contraseña</p><br>" +
                "<a href='" + url + "'>Click para recuperar</a>";


            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(fromAddress, Contrasena);

            MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);
            message.IsBodyHtml = true;

            client.Send(message);

            client.Dispose();
        }

        private void SendActivationMail(string toAddress, string activationToken)
        {
            string fromAddress = "";
            string Contrasena = "";
            string url = urlDomain + "/User/ActivateAccount/?token=" + activationToken;
            string subject = "Activación de cuenta - Carwash";
            string body = "<p>Hola! Gracias por registrarte en nuestro sitio web, para activar tu cuenta por favor ingresa a este link:</p><br>" +
                "<a href='" + url + "'>Click aquí para activar su cuenta</a>";


            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(fromAddress, Contrasena);

            MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);
            message.IsBodyHtml = true;

            client.Send(message);

            client.Dispose();
        }
        #endregion

    }
}
