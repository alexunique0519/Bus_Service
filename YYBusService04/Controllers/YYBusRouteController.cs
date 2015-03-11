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
    public class YYBusRouteController : Controller
    {
        private BusServiceContext db = new BusServiceContext();

        // GET: YYBusRoutes
        public ActionResult Index()
        {
            //pass the busRoutes list to bueRoute Index view
            return View(db.busRoutes.ToList());
        }

        // Find the busRoute Info from the dataContext and then pass it to the detail view
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a busRoute or input a valid routeCode in url before going to next page";
                return RedirectToAction("Index");
            }
            busRoute busRoute = db.busRoutes.Find(id);
            if (busRoute == null)
            {
                return HttpNotFound();
            }
            return View(busRoute);
        }

        // go the the YYBusRoute createview
        public ActionResult Create()
        {
            return View();
        }

        // POST: YYBusRoutes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        //get the post-back data, in this case is the busRoute Object, and save it in database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "busRouteCode,routeName")] busRoute busRoute)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //check whether there is a same busRoute record in database
                    var Route = db.busRoutes.Find(busRoute.busRouteCode);
                    if (Route != null)
                    {
                        TempData["messageType"] = "danger";
                        TempData["message"] = "There is already a same routeId in database";
                        return View();
                    }

                    db.busRoutes.Add(busRoute);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                TempData["messageType"] = "danger";
                TempData["message"] = "exception thrown: " + ex.Message;
            } 
         
            return View(busRoute);
        }

        //Get the busRoute object that need be edit and pass it to editView 
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a busRoute or input a valid routeCode in url before going to next page"; 
                return RedirectToAction("Index");
            }
            busRoute busRoute = db.busRoutes.Find(id);
            if (busRoute == null)
            {
                return HttpNotFound();
            }
            return View(busRoute);
        }

        // POST: YYBusRoutes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        //get the post-back data from edit operation, and update the busRoute info in database
        public ActionResult Edit([Bind(Include = "busRouteCode,routeName")] busRoute busRoute)
        {
            if (ModelState.IsValid)
            {
                db.Entry(busRoute).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(busRoute);
        }

        // GET: YYBusRoutes/Delete/5
        //find the busRoute by id, and then pass the object to deleteView
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a busRoute or input a valid routeCode in url before going to next page";
                return RedirectToAction("Index");
            }
            busRoute busRoute = db.busRoutes.Find(id);
            if (busRoute == null)
            {
                return HttpNotFound();
            }
            return View(busRoute);
        }

        // POST: YYBusRoutes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //get post-pack data, in this case it is the busRouteCode, then use it to find the all the data that should be deleted
        public ActionResult DeleteConfirmed(string id)
        {
            busRoute route = db.busRoutes.Find(id);

            //busRouteCode is the ForeignKey of routeStops table, so we should delete the routeStops record which have the same routeCode
            var routeStops = db.routeStops.Where(r => r.busRouteCode == id);

            foreach (routeStop routeStop in routeStops)
            {
                db.routeStops.Remove(routeStop);
            }

            //busRouteCode is the ForeignKey of routeSchedule table, so we should delete the routeSchedule record which have the same routeCode
            var routeSchedules = db.routeSchedules.Where(r => r.busRouteCode == id);

            foreach (routeSchedule routeSchedule in routeSchedules)
            {
                db.routeSchedules.Remove(routeSchedule);
            }

            db.busRoutes.Remove(route);

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //release  all the data
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
