using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using YYBusService04.Models;

namespace YYBusService04.Controllers
{
    public class YYRouteStopController : Controller
    {
        private BusServiceContext db = new BusServiceContext();

        // pass the routeCode and name to index action then get all the stops associated with routeCode, and pass them to indexView
        public ActionResult Index(string id, string name)
        {
            if (id != null)
            {
                var routeStops = db.routeStops.Where(r => r.busRouteCode == id)
                                .OrderBy(r => r.offsetMinutes);
                ViewBag.routeCode = id;
                ViewBag.routeName = name;
                Session["RouteCode"] = id;
                return View(routeStops.ToList());
            }
            else if (Request.QueryString["id"] != null)
            {
                Session["RouteCode"] = Request.QueryString["id"];
                var routeStops = db.routeStops.Where(r => r.busRouteCode == id)
                                   .OrderBy(r => r.offsetMinutes);                
                return View(routeStops.ToList());
            }
            else if (Session["RouteCode"] != null)
            {
                string routeCode = Session["RouteCode"].ToString();
                ViewBag.routeCode = routeCode;
                var busRoute = db.busRoutes.Find(routeCode);
                ViewBag.routeName = busRoute.routeName;
                var routeStops = db.routeStops.Where(r => r.busRouteCode == routeCode)
                                .OrderBy(r => r.offsetMinutes);
                return View(routeStops.ToList());
            }
            else if (Request.Cookies["RouteCode"] != null)
            {
                Session["RouteCode"] = Request.Cookies["RouteCode"];
                string routeCode = Session["RouteCode"].ToString();
                var routeStops = db.routeStops.Where(r => r.busRouteCode == routeCode)
                                 .OrderBy(r => r.offsetMinutes);
                return View(routeStops.ToList());
            }
            else
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "please select a valid route code";
                return RedirectToAction("Index", "YYBusRoute");
            }

        }

        // Find the routeStop object info with the routeStopId and pass it to detailView 
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a routeStop or input a valid routeStopId in url before going to next page";
                return RedirectToAction("Index");
            }
            routeStop routeStop = db.routeStops.Find(id);
            if (routeStop == null)
            {
                return HttpNotFound();
            }
            return View(routeStop);
        }

        // get the data which is necessary for create a new routeStop and pass them to the creatView
        public ActionResult Create()
        {
            string routeCode = Session["RouteCode"].ToString();
            ViewBag.routeCode = routeCode;
            var busRoute = db.busRoutes.Find(routeCode);
            ViewBag.routeName = busRoute.routeName;

            ViewBag.busStopNumber = new SelectList(db.busStops, "busStopNumber", "location");
            return View();
        }

        // POST: YYRouteStops/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        // get the post-back data from the sumbit operation, which is routeStop object and save the data in database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "routeStopId,busRouteCode,busStopNumber,offsetMinutes")] routeStop routeStop)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    routeStop.busRouteCode = Session["RouteCode"].ToString();
                    var busStop = db.busStops.Find(routeStop.busStopNumber);
                    var busRoute = db.busRoutes.Find(routeStop.busRouteCode);
                    routeStop.busStop = busStop;
                    routeStop.busRoute = busRoute;

                    db.routeStops.Add(routeStop);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                ViewBag.busRouteCode = new SelectList(db.busRoutes, "busRouteCode", "routeName", routeStop.busRouteCode);
                ViewBag.busStopNumber = new SelectList(db.busStops, "busStopNumber", "location", routeStop.busStopNumber);
                return View(routeStop);
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                TempData["messageType"] = "danger";
                TempData["message"] = "exception thrown: " + ex.Message;

                ViewBag.busRouteCode = new SelectList(db.busRoutes, "busRouteCode", "routeName", routeStop.busRouteCode);
                ViewBag.busStopNumber = new SelectList(db.busStops, "busStopNumber", "location", routeStop.busStopNumber);
                return View(routeStop);
            }
                    
        }

        // find the object by the id and pass to the edit view.
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a routeStop or input a valid routeStopId in url before going to next page";
                return RedirectToAction("Index");
            }

            routeStop routeStop = db.routeStops.Find(id);
            Session["busRoute"] = routeStop.busRoute;
            Session["busStop"] = routeStop.busStop;

            if (routeStop == null)
            {
                return HttpNotFound();
            }
            //ViewBag.busRouteCode = new SelectList(db.busRoutes, "busRouteCode", "routeName", routeStop.busRouteCode);
            ViewBag.busStopNumber = new SelectList(db.busStops, "busStopNumber", "location", routeStop.busStopNumber);

            return View(routeStop);
        }

        // POST: YYRouteStops/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        // get the routeStop object and save it to the database, it there is any exeption display a message to notify the user 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "routeStopId,busRouteCode,busStopNumber,offsetMinutes")] routeStop routeStop)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(routeStop).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.busRouteCode = new SelectList(db.busRoutes, "busRouteCode", "routeName", routeStop.busRouteCode);
                ViewBag.busStopNumber = new SelectList(db.busStops, "busStopNumber", "location", routeStop.busStopNumber);
                return View(routeStop);
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                TempData["messageType"] = "danger";
                TempData["message"] = "exception thrown: " + ex.Message;

                ViewBag.busRouteCode = new SelectList(db.busRoutes, "busRouteCode", "routeName", routeStop.busRouteCode);
                ViewBag.busStopNumber = new SelectList(db.busStops, "busStopNumber", "location", routeStop.busStopNumber);
                return View(routeStop);   
                
            }
        }

        // select the routeStop object by id, then pass it to deleteView
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a routeStop or input a valid routeStopId in url before going to next page";
                return RedirectToAction("Index");
            }
            routeStop routeStop = db.routeStops.Find(id);
            if (routeStop == null)
            {
                return HttpNotFound();
            }
            return View(routeStop);
        }

        // get the post-back id and find the data in database, delete it and then navigate to the indexView.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            routeStop routeStop = db.routeStops.Find(id);
            db.routeStops.Remove(routeStop);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //release all the data.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
