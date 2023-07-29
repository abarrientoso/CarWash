using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Carwash.Models.ViewModel;
using System.Web;
using System.Web.Mvc;
using Carwash.Models;
using System.IO;
using ClosedXML.Excel;

namespace Carwash.Controllers
{
    public class ExportController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ExportUsersExcel()
        {
            using (var db = new carwashEntities())
            {

                var usuarios = db.users.ToList();

                DataTable tabla = new DataTable("Usuarios");
                tabla.Columns.AddRange(new DataColumn[]
                {
            new DataColumn("ID", typeof(int)),
            new DataColumn("Nombre", typeof(string)),
            new DataColumn("Correo", typeof(string))
                });

                foreach (var usuario in usuarios)
                {
                    tabla.Rows.Add(usuario.user_id, usuario.name, usuario.email);
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(tabla);
                    var memoryStream = new MemoryStream();
                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Usuarios.xlsx");
                }
            }
        }

        public ActionResult ExportVendorsExcel()
        {
            using (var db = new carwashEntities())
            {

                var proveedores = db.Vendors.ToList();

                DataTable tabla = new DataTable("Proveedores");
                tabla.Columns.AddRange(new DataColumn[]
                {
            new DataColumn("ID", typeof(int)),
            new DataColumn("Nombre", typeof(string)),
            new DataColumn("Descripcion", typeof(string)),
            new DataColumn("Telefono", typeof(string)),
            new DataColumn("Direccion", typeof(string)),
            new DataColumn("Correo", typeof(string))
                });

                foreach (var proveedor in proveedores)
                {
                    tabla.Rows.Add(proveedor.Id, proveedor.Name, proveedor.Description, proveedor.Phone, proveedor.Address, proveedor.Email);
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(tabla);
                    var memoryStream = new MemoryStream();
                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Proveedores.xlsx");
                }
            }
        }
    }
}