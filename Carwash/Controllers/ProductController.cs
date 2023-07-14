using Carwash.Models;
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
    public class ProductController : Controller
    {
        carwashEntities db = new carwashEntities();
        string urlDomain = "https://localhost:44325/";
        public ActionResult Index()
        {
            List<product_stock> products = db.product_stock.ToList(); 
            return View(products);
        }

        public ActionResult GetImage(int productId)
        {
            var product = db.product_stock.Find(productId);
            if (product != null && product.product_img != null)
            {
                return File(product.product_img, "image/png");
            }

            return HttpNotFound();
        }

        public ActionResult Edit(int id)
        {
            product_stock producto =  db.product_stock.SingleOrDefault(u => u.id_stock== id);
            return View(producto);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(product_stock model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    byte[] imageData = new byte[imageFile.ContentLength];
                    using (BinaryReader binaryReader = new BinaryReader(imageFile.InputStream))
                    {
                        imageData = binaryReader.ReadBytes(imageFile.ContentLength);
                    }

                    model.product_img = imageData;
                }

                db.product_stock.Attach(model);
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Create()
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult Create(product_stock model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    byte[] imageData = new byte[imageFile.ContentLength];
                    using (BinaryReader binaryReader = new BinaryReader(imageFile.InputStream))
                    {
                        imageData = binaryReader.ReadBytes(imageFile.ContentLength);
                    }

                    model.product_img = imageData;
                }

                db.product_stock.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");

            }

            return View(model);
            
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                

                product_stock material = db.product_stock.Find(id);

                if (material != null)
                {

                    db.product_stock.Remove(material);
                    db.SaveChanges();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}