﻿using Frontend_PI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace Frontend_PI.Controllers
{
    public class ProductController : Controller
    {
        List<Product> productList = new List<Product>();

        HttpClient httpClient;
        string baseAddress;
        public static float sumPriceProduct = 0;
        public ProductController()
        {
        sumPriceProduct = 0;
        baseAddress = "http://localhost:8081/SpringMVC/servlet/";
        httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(baseAddress);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
        }
        public Boolean checkIfexistElement(int id, List<CommandeDetails> commandeDetailsSessionLst)
        {
            List<CommandeDetails> commandeDetailsSession = commandeDetailsSessionLst;
            if (commandeDetailsSession != null && commandeDetailsSession.Count != 0)
                foreach (var cmdS in commandeDetailsSession)
                {
                    if (cmdS.idProduct == id)
                        return true;
                }
            return false;
        }

        public Boolean checkIfexistElement(int id)
        {
            List<CommandeDetails> commandeDetailsSession = (List<CommandeDetails>)Session["commandeDetailsList"];
            if(commandeDetailsSession != null && commandeDetailsSession.Count != 0)
            foreach(var cmdS in commandeDetailsSession)
            {
                    if (cmdS.idProduct == id)
                        return true;
            }
            return false;
        }

        [HttpPost]
        public ActionResult Remplissage(int id,int qty)
        {

            List<Product> products = (List<Product>)Session["productList"];
            if (products == null)
                products = new List<Product>();
            List<CommandeDetails> commandeDetailsList = (List<CommandeDetails>)Session["commandeDetailsList"];
            if (commandeDetailsList == null)
                commandeDetailsList = new List<CommandeDetails>();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage responseMessage = httpClient.GetAsync(baseAddress+ "findProduct/" + id).Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                Product product = responseMessage.Content.ReadAsAsync<Product>().Result;
                products.Add(product);
                Session["productList"] = products;

                List<CommandeDetails> commandeDetailsSession = (List<CommandeDetails>)Session["commandeDetailsList"];
                
                    CommandeDetails commandeDetails = new CommandeDetails();
                    commandeDetails.idProduct = id;
                    commandeDetails.qte = qty;
                if (!this.checkIfexistElement(id))
                {
                    commandeDetailsList.Add(commandeDetails);
                    Session["commandeDetailsList"] = commandeDetailsList;
                    string message = "SUCCESS";
                    return Json(new { Message = message, JsonRequestBehavior.AllowGet });
                }

            }
            string message1 = "Failed";
            return Json(new { Message = message1, JsonRequestBehavior.AllowGet });
           
        }

        public static String show(int id,String type, int qte)
        {
            String productName = "";
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8081");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage responseMessage = httpClient.GetAsync("SpringMVC/servlet/findProduct/" + id).Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                Product product = responseMessage.Content.ReadAsAsync<Product>().Result;
                if (type.Equals("price"))
                {
                    sumPriceProduct = sumPriceProduct + (product.price * qte);
                    return (product.price * qte).ToString();
                }
                productName = product.title ;
            }
            return productName;
        }

        public void updateQtePanier(String idProduct)
        {
            List<CommandeDetails> commandeDetails = (List<CommandeDetails>)Session["commandeDetailsList"];
            for (int i = 0; i <= commandeDetails.Count - 1; i++)
            {
                if (commandeDetails[i].idProduct == Convert.ToInt32(idProduct))
                {
                    commandeDetails.Remove(commandeDetails[i]);
                    if (commandeDetails.Count == 0)
                        break;
                }
            }
        }

        public ActionResult AddRating(int ratingVal, int idProduct)
        {
            FeedBack feed = new FeedBack();
            feed.idProduct = idProduct;
            feed.rate = ratingVal;

            var APIResponse = httpClient.PostAsJsonAsync<FeedBack>(baseAddress + "addFeedback/",
               feed).ContinueWith(postTask => postTask.Result.EnsureSuccessStatusCode());

            return RedirectToAction("Index", "Product");
        }

        public ActionResult deleteCommandDetail(int id)
        {
            List<CommandeDetails> commandeDetails = (List<CommandeDetails>)Session["commandeDetailsList"];
            for(int i=0; i<= commandeDetails.Count - 1 ; i ++)
            {
                if (commandeDetails[i].idProduct == id)
                {
                    commandeDetails.Remove(commandeDetails[i]);
                    if (commandeDetails.Count == 0)
                        break;
                }
            }
            return View("CommandeDetails", commandeDetails);

        }

        public ActionResult updateQte(int id,int qty)
        {
            List<CommandeDetails> commandeDetails = (List<CommandeDetails>)Session["commandeDetailsList"];
            foreach(var cmd in commandeDetails)
            {
                if (cmd.idProduct == id)
                    cmd.qte = qty;
            }
            return View("CommandeDetails", commandeDetails);
        }

        public ActionResult CommandeDetails()
        {
            List<Product> products = (List<Product>)Session["productList"];
            List<CommandeDetails>  commandeDetails = (List<CommandeDetails>)Session["commandeDetailsList"];

            return View(commandeDetails);
        }

        public float sumProductPrice()
        {
            float price = 0;

            return price;
        }

            // GET: Product
            public ActionResult Index()
        {
            HttpResponseMessage responseMessage = httpClient.GetAsync(baseAddress+ "findAllProduct").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                ViewBag.result = responseMessage.Content.ReadAsAsync<IEnumerable<Models.Product>>().Result;
                return View(ViewBag.result);
            }
            return View();

        }

        // GET: Product/Details/5
        public ActionResult Details(int id)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage responseMessage = httpClient.GetAsync(baseAddress + "findProduct/" + id).Result;
            if (responseMessage.IsSuccessStatusCode)
            {
                ViewBag.result = responseMessage.Content.ReadAsAsync<Product>().Result;
                return View(ViewBag.result);
            }
            return View();

        }

        // GET: Product/Create
        public ActionResult Create()
        {
           
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage responseMessage = httpClient.GetAsync(baseAddress+"findAllCategory").Result;
            if (responseMessage.IsSuccessStatusCode)
            {
               var  categories = responseMessage.Content.ReadAsAsync<IEnumerable<Models.Category>>().Result;
                ViewBag.mycategories = new SelectList(categories,"id","name");

            }
            return View();

        }

        // POST: Product/Create
        [HttpPost]
        public ActionResult Create(Product product)
        {
            try
            {

                var APIResponse = httpClient.PostAsJsonAsync<Product>(baseAddress + "addProduct/",
                product).ContinueWith(postTask => postTask.Result.EnsureSuccessStatusCode());

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Product/Edit/5
        public ActionResult Edit(int id)
        {
            HttpResponseMessage responseMessage = httpClient.GetAsync(baseAddress + "findProduct/" + id).Result;

            if (responseMessage.IsSuccessStatusCode)
            {
                ViewBag.result = responseMessage.Content.ReadAsAsync<Product>().Result;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage responseMessage1 = httpClient.GetAsync(baseAddress + "findAllCategory").Result;
                if (responseMessage1.IsSuccessStatusCode)
                {
                    var categories = responseMessage1.Content.ReadAsAsync<IEnumerable<Models.Category>>().Result;
                    ViewBag.mycategories = new SelectList(categories, "id", "name");

                }

                return View(ViewBag.result);
            }

            return View();
        }

        // POST: Product/Edit/5
        [HttpPost]
        public ActionResult Edit( Product product)
        {
            try
            {
                var APIResponse = httpClient.PostAsJsonAsync<Product>(baseAddress + "updateProduct/",
               product).ContinueWith(postTask => postTask.Result.EnsureSuccessStatusCode());

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Product/Delete/5
        public ActionResult Delete(int id)
        {
            var APIResponse = httpClient.DeleteAsync(baseAddress + "/deleteProduct/" + id).Result;
            if (APIResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // POST: Product/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, Product product)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}