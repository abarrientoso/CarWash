using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Carwash.Models;

namespace Carwash.Controllers
{
    public class UserController : Controller
    {
        carwashEntities db = new carwashEntities();
        string urlDomain = "https://localhost:44325/";
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration(User user)
        {
            db.Users1.Add(user);
            db.SaveChanges();
            return View();
        }

        public ActionResult Login ()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            var lg = db.Users1.Where(a=>a.email.Equals(user.email) && a.password.Equals(user.password)).FirstOrDefault();
            if (lg != null)
            {
                return RedirectToAction("Index", "User");
            } else
            {
                return RedirectToAction("Login", "User");
            }
        }

        public ActionResult ForgotPassword ()
        {
            return View();
        }

        [HttpGet]
        public ActionResult StartRecovery (string token)
        {
            Models.ViewModel.RecoveryPasswordViewModel model = new Models.ViewModel.RecoveryPasswordViewModel();
            model.token = token;
            using (carwashEntities db = new carwashEntities())
            {
                if(model.token == null || model.token.Trim().Equals(""))
                {
                    ViewBag.Error = "El token ha expirado";
                    return View("StartRecovery");
                }
                var oUser = db.Users1.Where(d => d.token_recovery == model.token).FirstOrDefault();
                if(oUser == null)
                {
                    ViewBag.Error = "El token ha expirado";
                    return View("StartRecovery");
                }
            }
            
            return View(model);
        }

        [HttpPost]
        public ActionResult Recovery (Models.ViewModel.RecoveryPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("StartRecovery", model);
                }

                using (Models.carwashEntities db = new Models.carwashEntities())
                {
                    var oUser = db.Users1.Where(d => d.token_recovery == model.token).FirstOrDefault();
                    if(oUser != null)
                    {
                        oUser.password = GetSha256(model.Password);
                        oUser.token_recovery = null;
                        db.Entry(oUser).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            ViewBag.Message = "Contraseña modificada con exito";
            return View("Index");
        }

        [HttpGet]
        public ActionResult PasswordRecovery()
        {
            Models.ViewModel.RecoveryViewModel model = new Models.ViewModel.RecoveryViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult PasswordRecovery (Models.ViewModel.RecoveryViewModel model)
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
                    var oUser = db.Users1.Where(d => d.email == model.Email).FirstOrDefault();
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

        #region HELPERS
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
            string fromAddress = "correo@correo.com";
            string Contrasena = "su contraseña";
            string url = urlDomain + "/User/StartRecovery/?token="+token;
            string subject = "Recuperación de contraseña";
            string body = "<p>Correo para recuperación de contraseña</p><br>"+
                "<a href='"+ url + "'>Click para recuperar</a>";


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
