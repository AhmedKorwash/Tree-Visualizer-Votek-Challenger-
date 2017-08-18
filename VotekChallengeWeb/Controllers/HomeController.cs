using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VotekChallengeWeb.Models;
using Newtonsoft.Json;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;

namespace VotekChallengeWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Tree()
        {
            List<TreeModels> model = new List<TreeModels>();
            return View(model);
        }

        [HttpGet]
        public ActionResult Sample()
        {
            Random r = new Random();

            string data = System.IO.File.ReadAllText(Directory.EnumerateFiles(Server.MapPath("~/Content/json")).ToArray()[r.Next(0, Directory.EnumerateFiles(Server.MapPath("~/Content/json")).ToArray().Length)]);
            ViewBag.HiddenObject = data;
            ModelState.Clear();
            return View(GetModelData(JObject.Parse(data)));
        }

        [HttpPost]
        public void SaveTree(string image, string model)
        {
            //Estaplish Filename using Random string
            string filename = Guid.NewGuid().ToString();
            // Select the Directory [Folder]
            var dir = Server.MapPath("~\\jsonUpload");
            // Path for Json File
            var file = Path.Combine(dir, filename + ".json");
            
            FileStream fs = new FileStream(file,FileMode.CreateNew , FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(model);
            sw.Flush();
            sw.Close();
            fs.Close();

            // Path for Image [ScreenShot]
            file = Path.Combine(dir, filename + ".img");
             fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
             sw = new StreamWriter(fs);
             sw.Write(image);
            sw.Flush();
            sw.Close();
            fs.Close();
            //Config File
            file = Path.Combine(dir, "Serial" + ".treevis");
            fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
            sw = new StreamWriter(fs);
            fs.Seek(0, SeekOrigin.End);
            sw.WriteLine(filename);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        [HttpPost]
        public void DownloadFile(string id)
        {
            try
            {
                var dir = Server.MapPath("~\\jsonUpload");
                var file = Path.Combine(dir, id + ".json");

                Response.ContentType = "text/plain;charset=utf-8";
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + id + ".json");
                Response.TransmitFile(Server.MapPath("~\\jsonUpload\\" + id + ".json"));
                Response.End();

                
            }
            catch
            {
                RedirectToAction("Error");
            }  
        }

        [HttpGet]
        public ActionResult PreviousSample()
        {
            var dir  = Server.MapPath("~\\jsonUpload");
            var file = Path.Combine(dir, "Serial" + ".treevis");

            FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            Dictionary<string, Dictionary<string, string>> ListItems = new Dictionary<string, Dictionary<string, string>>();
            var st = sr.ReadLine();
            while (st != null)
            {
                ListItems.Add(st,new Dictionary<string,string>());
                ListItems[st].Add("Json", Path.Combine(dir, st + ".json"));
                ListItems[st].Add("Image", Path.Combine(dir, st + ".img"));
                st = sr.ReadLine();
            }

            return View(ListItems);
        }

        [HttpGet]
        public ActionResult ViewTree(string id)
        {
            try
            {
                var dir = Server.MapPath("~\\jsonUpload");
                var file = Path.Combine(dir, id + ".json");

                ViewBag.HiddenObject = System.IO.File.ReadAllText(file);
                ModelState.Clear();
                return View(GetModelData(JObject.Parse(System.IO.File.ReadAllText(file))));
            }
            catch
            {
                return View("Error");
            }  
        }

        [HttpPost]
        public ActionResult Tree(HttpPostedFileBase selectFiles)
        {
            try
            {
                StreamReader sr = new StreamReader(selectFiles.InputStream);
                string data = sr.ReadToEnd();
                ViewBag.HiddenObject = data;
                ModelState.Clear();
                return View(GetModelData(JObject.Parse(data)));
            }
            catch
            {
                return View("Error");
            }  
        }

        List<TreeModels> GetModelData(JObject obj)
        {
            dynamic json = obj;
            int Count = json.tree_nodes.Count;
            // Create List Of All Nodes in JSon file
            List<TreeModels> allNodes = new List<TreeModels>();
            for (int i = 0; i < Count; i++)
            {
                var mod = new TreeModels()
                {
                    id = json.tree_nodes[i].id,
                    Next_Id = json.tree_nodes[i].next_node_id,
                    Previous_Id = json.tree_nodes[i].previous_sibling_id,
                    Title = json.tree_nodes[i].title
                };
                allNodes.Add(mod);
            }

            // Create the base Node
            List<TreeModels> baseNode = new List<TreeModels>();
            baseNode.Add(new TreeModels()
            {
                Title = json.tree_title
            });

            baseNode.AddRange(allNodes.Where(s => s.Previous_Id == ""));

            // Create Childs Nodes for the Base Nodes and attach with it
            foreach (var item in baseNode)
            {
                item.Childs.AddRange(allNodes.Where(s => s.Previous_Id == item.id));
            }

            //Create the Level 3 of Childs Node
            foreach (var item in baseNode)
            {
                foreach (var child in item.Childs)
                {
                    child.Childs.AddRange(allNodes.Where(s => s.Previous_Id == child.id));
                }
            }

            //Create the Level 4 of Childs Node
            foreach (var item in baseNode)
            {
                foreach (var child in item.Childs)
                {
                    foreach (var _4th in child.Childs)
                    {
                        _4th.Childs.AddRange(allNodes.Where(s => s.Previous_Id == _4th.id));
                    }
                }
            }

            return baseNode;
        }
    }
}