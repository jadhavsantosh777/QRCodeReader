using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.Mvc;

namespace QRCodeReader.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";       
            return View();
        }       

        [HttpPost]
        public ActionResult Upload()
        {
            string QRCodeText = "";
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Documents/"), fileName);
                    file.SaveAs(path);

                    byte[] fileData;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        file.InputStream.CopyTo(ms);
                        fileData = ms.GetBuffer();
                    }

                    var formData = new MultipartFormDataContent();
                    formData.Add(new StreamContent(new MemoryStream(fileData)), "file", file.FileName);

                    var client = new HttpClient();
                    HttpResponseMessage response = client.PostAsync("http://api.qrserver.com/v1/read-qr-code/", formData).Result;
                    var QRCodeJsonString =  response.Content.ReadAsStringAsync();
                    List<QRCodeResult> result = JsonConvert.DeserializeObject<List<QRCodeResult>>(QRCodeJsonString.Result.ToString());
                    QRCodeText = Convert.ToString(result[0].symbol[0].data);                    
                }
            }
            ViewBag.QRCode = QRCodeText;
            return View("Index");           
        }        
    }

    public class Symbol
    {
        public int seq { get; set; }
        public string data { get; set; }
        public object error { get; set; }
    }

    public class QRCodeResult
    {
        public string type { get; set; }
        public IList<Symbol> symbol { get; set; }
    }
}
