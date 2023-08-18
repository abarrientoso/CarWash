using Carwash.Models;
using Carwash.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Carwash.Controllers
{
    public class BinnacleController : Controller
    {
        carwashEntities db = new carwashEntities();
        string urlDomain = "https://localhost:44325/";
        public ActionResult Index(int id)
        {

            List<Binnacle> binnacle = new List<Binnacle>();
            if (id == 1)
            {
                //binnacle = db.Binnacle.ToList();
                binnacle = db.Binnacle.Where(item => item.type == "Gasto").ToList();
            }
            else {
                binnacle = db.Binnacle.Where(item => item.type == "Ingreso").ToList();
            }
            return View(binnacle);
        }

     
        public ActionResult Edit(int id)
        {
            Binnacle binnacle=  db.Binnacle.SingleOrDefault(u => u.idBinnacle== id);
            return View(binnacle);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Binnacle binnacle)
        {
            if (ModelState.IsValid)
            {

                db.Binnacle.Attach(binnacle);
                db.Entry(binnacle).State = EntityState.Modified;
                db.SaveChanges();
                if (binnacle.type.Equals("Ingreso"))
                    return RedirectToAction("Index", new { id = 2 });
                else
                    return RedirectToAction("Index", new { id = 1 });

            }

            return View(binnacle);
        }

        public ActionResult Create()
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult Create(Binnacle binnacle)
        {
            if (ModelState.IsValid|| true)
            {             
                

                db.Binnacle.Add(binnacle);
                db.SaveChanges();
                if (binnacle.type.Equals("Ingreso"))
                    return RedirectToAction("Index", new { id = 2 });
                else
                    return RedirectToAction("Index", new { id = 1 });


            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
               
                Binnacle binnacle= db.Binnacle.Find(id);
                string tipo = binnacle.type;

                if (binnacle != null)
                {

                    db.Binnacle.Remove(binnacle);
                    db.SaveChanges();
                }
                if (tipo.Equals("Ingreso"))
                    return RedirectToAction("Index", new { id = 2 });
                else
                    return RedirectToAction("Index", new { id = 1 });
                
            }
            catch
            {
                return View();
            }
        }
    }
}