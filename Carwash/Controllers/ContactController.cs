using Carwash.Models.ViewModel;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace Carwash.Controllers
{
    public class ContactController : Controller
    {
        public ActionResult Contact_Form()
        {
            return View();
        }

        public ActionResult Contact_Confirmation()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Contact(ContactViewModel model)
        {

            if (ModelState.IsValid)
            {
                //using (var db = new carwashEntities())
                //{
                //    db.Contactos.Add(new Contacto
                //    {
                //        Nombre = model.name,
                //        CorreoUsuario = model.email,
                //        Mensaje = model.message
                //    });

                //    db.SaveChanges();
                Send_Email_User(model.Email, model.Name);
                Send_Email_Company(model.Name, model.Email, model.Subject, model.Message);
                return RedirectToAction("Contact_Confirmation");
            }
            return View("Contact_Form", model);
        }

        private void Send_Email_User(string correoUsuario, string nombre)
        {

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("correo@compania.com");
                mail.To.Add(correoUsuario);
                mail.Subject = "Gracias por contactarnos";
                mail.Body = $"Hola {nombre}, Gracias por contactarnos. Hemos recibido tu mensaje y nos pondremos en contacto contigo pronto.";
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("correo@compania", "password");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

        private void Send_Email_Company(string nombre, string correoUsuario, string subject, string mensaje)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(correoUsuario);
                mail.To.Add("correo@compania.com");
                mail.Subject = $"Mensaje de contacto de usuario {nombre}";
                mail.Body = $"Nombre: {nombre}<br/>Correo: {correoUsuario} <br/>Asunto: {subject}  <br/>Mensaje: {mensaje} ";
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("correo@compania", "password");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

    }
}