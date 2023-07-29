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

        private List<SelectListItem> dropdownCreate(int id)
        {
            List<Vendors> lista = db.Vendors.ToList();

            List<SelectListItem> listaCategorias = new List<SelectListItem>();

            if (id == 0)
            {
                for (int i = 0; i < lista.Count; i++)
                {
                    listaCategorias.Add(new SelectListItem { Value = lista[i].Id.ToString(), Text = lista[i].Name.ToString() });
                }
            }
            else
            {
                for (int i = 0; i < lista.Count; i++)
                {
                    if (lista[i].Id == id)
                    {
                        listaCategorias.Add(new SelectListItem { Value = lista[i].Id.ToString(), Text = lista[i].Name.ToString(), Selected = true });
                    }
                    else
                    {
                        listaCategorias.Add(new SelectListItem { Value = lista[i].Id.ToString(), Text = lista[i].Name.ToString() });
                    }

                }
            }
            return listaCategorias;
        }

        public ActionResult GetImage(int productId)
        {
            var product = db.product_stock.Find(productId);
            if (product != null && product.product_image != null)
            {
                return File(product.product_image, "image/png");
            }

            return HttpNotFound();
        }

        public ActionResult Edit(int id)
        {
            product_stock producto = db.product_stock.SingleOrDefault(u => u.id_stock == id);
            List<SelectListItem> lista = dropdownCreate(producto.id_vendor.Value);
            ViewBag.idLocal = lista;

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

                    model.product_image = imageData;
                }
                Vendors vendor = db.Vendors.SingleOrDefault(u => u.Id == model.id_vendor);
                model.vendor_name = vendor.Name;
                db.product_stock.Attach(model);
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.idLocal = dropdownCreate(0);
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

                    model.product_image = imageData;
                }
                Vendors vendor = db.Vendors.SingleOrDefault(u => u.Id == model.id_vendor);
                model.vendor_name = vendor.Name;
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