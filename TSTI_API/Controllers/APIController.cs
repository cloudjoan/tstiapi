﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TSTI_API.Models;

namespace TSTI_API.Controllers
{
    public class APIController : Controller
    {
        TESTEntities testDB = new TESTEntities();

        static string API_KEY = "6xdTlREsMbFd0dBT28jhb5W3BNukgLOos";

        // GET: API
        [HttpGet]
        public ActionResult Index()
        {
            return Json("Hello World!", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveData(TB_MVC_CUST bean)
        {

            testDB.TB_MVC_CUST.Add(bean);
            var result = testDB.SaveChanges();

            if (result == 1)
            {
                return Json("Finsih");
            }
            else
            {
                return Json("Fail");
            }

            //if (Request.Headers["X-MBX-APIKEY"] == API_KEY)
            //{
            //    testDB.TB_MVC_CUST.Add(bean);
            //    testDB.SaveChanges();

            //    return Json(bean);
            //}
            //else
            //{
            //    return Json("Fail!");
            //}

        }

        [HttpPost]
        public ActionResult SaveDatas(List<TB_MVC_CUST> beans)
        {

            //testDB.TB_MVC_CUST.Add(beans);
            var result = testDB.SaveChanges();

            if(result > 0)
            {
                return Json("Finsih");
            }
            else
            {
                return Json("Fail");
            }
            
        }

        [HttpPost]
        public ActionResult TestGetFormData(FormCollection data)
        {
            return Json(string.Format("{0} Email:{1}", data["userNames"] ?? "逢金", data["email"] ?? "沒有"));
        }

    }
}