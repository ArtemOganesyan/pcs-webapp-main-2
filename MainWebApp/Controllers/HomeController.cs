using MainWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MainWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.MasterContent = "Master content from: " + LocalAddress();
            ViewBag.ForumContent = "Forum application url not set";
            ViewBag.FeedbackContent = "Feedback application url not set";
            return View();
        }

        public IActionResult UpdateEndpoints(Endpoints model)
        {
            if (model.FeedbackEndpoint == null || model.ForumEndpoint == null)
            {
                ViewBag.MasterContent = "Master content from: " + LocalAddress();
                ViewBag.ForumContent = "Forum application url not set";
                ViewBag.FeedbackContent = "Feedback application url not set";
                return View("Index");
            }

            if (model.FeedbackEndpoint == "error" || model.ForumEndpoint == "error")
            {
                LogWrite("Hello error from application");
                return View("Index");
            }

            ViewBag.MasterContent = "Master content from: " + LocalAddress();

            string ForumUrl = model.ForumEndpoint;
            string FeedbackUrl = model.FeedbackEndpoint;

            string ForumContent = "";
            using (WebClient client = new WebClient())
            {
                ForumContent = client.DownloadString(ForumUrl + "/Home/Forum");
            }

            LogWrite(ForumUrl);

            string FeedbackContent = "";
            using (WebClient client = new WebClient())
            {
                FeedbackContent = client.DownloadString(FeedbackUrl + "/Home/Feedback");
            }

            LogWrite(FeedbackUrl);

            ViewBag.ForumContent = ForumContent;
            ViewBag.FeedbackContent = FeedbackContent;

            try
            {
                AwsParameterStoreClient AwsClient = new AwsParameterStoreClient(Amazon.RegionEndpoint.USEast2);
                var value = AwsClient.GetValueAsync("TestParameter").Result;
                ViewBag.SsmParameter = value;
            }
            catch (Exception ex)
            {
                LogWrite(ex.Message);
            }

            //string test = GetAwsParameter("TestParameter");


            return View("Index");
        }

        public IActionResult Forum()
        {
            LogWrite("Forum content requested");
            ViewBag.IP = LocalAddress();
            return View();
        }

        public IActionResult Feedback()
        {
            LogWrite("Feedback content requested");
            ViewBag.IP = LocalAddress();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string LocalAddress()
        {
            string localIP = "";
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }

        private void LogWrite(string LogMessage)
        {
            try
            {
                using (StreamWriter writer = System.IO.File.AppendText("/var/log/webapp.log"))
                {
                    writer.WriteLine("Web application message: " + LogMessage);
                }

            }
            catch (Exception)
            {

            }
        }

        /*
        private async Task<string> GetAwsParameter(string ParameterName)
        {
            AwsParameterStoreClient client = new AwsParameterStoreClient(Amazon.RegionEndpoint.USEast2);

            var value = await client.GetValueAsync(ParameterName);

            return value;
        }
        */
    }
}
