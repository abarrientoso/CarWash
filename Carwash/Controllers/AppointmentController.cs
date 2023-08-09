using Carwash.Models;
using Carwash.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Carwash.Controllers
{
    public class AppointmentController : Controller
    {
        
        carwashEntities db = new carwashEntities();
        string urlDomain = "https://localhost:44325/";

        public ActionResult UsersAppointments()
        {
            string userEmail = Session["Email"] as string;
            var user = db.users.FirstOrDefault(u => u.email == userEmail);

            if (user != null)
            {
                int userId = user.user_id;

                // Filtrar las citas asociadas al usuario logueado
                List<appointments> userAppointments = db.appointments.Where(a => a.user_id == userId).ToList();
                List<AppointmentUserViewModel> appointmentList = new List<AppointmentUserViewModel>();
                foreach (var appointments in userAppointments)
                {
                    AppointmentUserViewModel appointmentUser = new AppointmentUserViewModel
                    {
                        appointment_id = appointments.appointment_id,
                        date = appointments.date,
                        email = user.email,
                        phone = user.phone,
                        time = appointments.time,
                        name = user.name
                    };
                    appointmentList.Add(appointmentUser);
                }
                return View(appointmentList);
            }
            return RedirectToAction("Index", "User");
        }

        public ActionResult Index()
        {
            string userEmail = Session["Email"] as string;
            var user = db.users.FirstOrDefault(u => u.email == userEmail);

            if (user != null)
            {
                int userId = user.user_id;

                // Filtrar las citas asociadas al usuario logueado
                List<appointments> userAppointments = db.appointments.Where(a => a.user_id == userId).ToList();

                return View(userAppointments);
            }
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            appointments appointment = db.appointments.SingleOrDefault(u => u.appointment_id == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            ViewBag.AppointmentId = appointment.appointment_id; 
            ViewBag.TimeOptions = GetTimeOptions();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(appointments feedback)
        {
            if (ModelState.IsValid || true)
            {
                string userEmail = Session["Email"] as string;
                var user = db.users.FirstOrDefault(u => u.email == userEmail);

                if (user != null)
                {
                    var existingFeedback = db.appointments.Find(feedback.appointment_id);
                    if (existingFeedback != null)
                    {

                        existingFeedback.date = feedback.date;
                        existingFeedback.time = feedback.time;
    
                        db.Entry(existingFeedback).State = EntityState.Modified;

                        db.SaveChanges();

                        if (Session["Rol"].Equals("Administrador"))
                        {
                            return RedirectToAction("UsersAppointments");
                        } else
                        {
                            return RedirectToAction("Index");
                        }
                        
                    }
                }
                else
                {
                    ViewBag.TimeOptions = GetTimeOptions();
                    return View();
                }
            }
            ViewBag.TimeOptions = GetTimeOptions();
            return View(feedback);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.TimeOptions = GetTimeOptions();
            return View();
        }

        [HttpPost]
        public ActionResult Create(appointments appointment)
        {
            if (ModelState.IsValid || true)
            {
                string userEmail = Session["Email"] as string;
                var user = db.users.FirstOrDefault(u => u.email == userEmail);

                if (user != null)
                {
                    appointment.user_id = user.user_id;
                    db.appointments.Add(appointment);
                    db.SaveChanges();

                    SendEmail(userEmail);
                    Send_Email_Company(user.name, userEmail, "He agendado una cita para el lavado de mi vehiculo.");
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.TimeOptions = GetTimeOptions();
                    return View();
                }
            }

            return View(appointment);

        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
               
                appointments appointment = db.appointments.Find(id);

                if (appointment != null)
                {

                    db.appointments.Remove(appointment);
                    db.SaveChanges();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private SelectList GetTimeOptions()
        {
            List<SelectListItem> timeOptions = new List<SelectListItem>();

            for (int hour = 7; hour <= 17; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    string displayValue = $"{hour:00}:{minute:00}"; 
                    timeOptions.Add(new SelectListItem { Text = displayValue, Value = displayValue });
                }
            }

            return new SelectList(timeOptions, "Value", "Text");
        }

        #region Helpers

        private void SendEmail(string toAddress)
        {
            string fromAddress = "";
            string Contrasena = "";
            string subject = "Cita agendada para su vehiculo";
            string body = "<p>Enhorabuena su cita ha sido agendada con nosotros. Lo esperamos pronto!</p><br>";

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(fromAddress, Contrasena);

            MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);
            message.IsBodyHtml = true;

            client.Send(message);

            client.Dispose();
        }

        private void Send_Email_Company(string nombre , string correoUsuario, string subject)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(correoUsuario);
                mail.To.Add("correo@compania.com");
                mail.Subject = $"Cita para usuario {nombre}";
                mail.Body = $"Correo: {correoUsuario} <br/>Asunto: {subject}";
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

        #endregion
    }
}