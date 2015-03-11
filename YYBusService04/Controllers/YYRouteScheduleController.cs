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
    public class YYRouteScheduleController : Controller
    {
        private BusServiceContext db = new BusServiceContext();

        //retrieve all the schedules info and send them to the index view to render the 
        //routeshedule view
        public ActionResult Index()
        {
            var routeschedules = db.routeSchedules.Include(r => r.busRoute);
            return View(routeschedules.ToList());
        }

        //find the routeSchedule object from the datebase and pass the model object to the detail view
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            routeSchedule routeschedule = db.routeSchedules.Find(id);
            if (routeschedule == null)
            {
                return HttpNotFound();
            }
            return View(routeschedule);
        }

        //new a selectList of all the busRoutes and save them in the viewbag
        public ActionResult Create()
        {
            ViewBag.busRouteCode = new SelectList(db.busRoutes, "busRouteCode", "routeName");
            return View();
        }

        
        //get the post data from the post request, if the modelstate is valid save the data into database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="routeScheduleId,busRouteCode,startTime,isWeekDay,comments")] routeSchedule routeschedule)
        {
            if (ModelState.IsValid)
            {
                db.routeSchedules.Add(routeschedule);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.busRouteCode = new SelectList(db.busRoutes, "busRouteCode", "routeName", routeschedule.busRouteCode);
            return View(routeschedule);
        }

        //when the id value is valid, render a edit view to edit the routeSchedule object
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            routeSchedule routeschedule = db.routeSchedules.Find(id);
            if (routeschedule == null)
            {
                return HttpNotFound();
            }
            ViewBag.busRouteCode = new SelectList(db.busRoutes, "busRouteCode", "routeName", routeschedule.busRouteCode);
            return View(routeschedule);
        }

        //get the routeSchedule object from the post data, if the modelstate is valid, save the data into database and 
        //redirect to index page, otherwise go to the creat view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="routeScheduleId,busRouteCode,startTime,isWeekDay,comments")] routeSchedule routeschedule)
        {
            if (ModelState.IsValid)
            {
                db.Entry(routeschedule).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.busRouteCode = new SelectList(db.busRoutes, "busRouteCode", "routeName", routeschedule.busRouteCode);
            return View(routeschedule);
        }

        //is the id value is not null, find an routeSchedule object with the id, and pass it to the delete view.
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            routeSchedule routeschedule = db.routeSchedules.Find(id);
            if (routeschedule == null)
            {
                return HttpNotFound();
            }
            return View(routeschedule);
        }

        //get the id from the post, and find the routeSchedule record in the database, after deleting it return to the index page
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            routeSchedule routeschedule = db.routeSchedules.Find(id);
            db.routeSchedules.Remove(routeschedule);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //is the routeStopId is valid, then find all the data which is necessary for the routeStopSchedule viewModel
        //send them to the view to render the page.
        public ActionResult RouteStopSchedule(int? routeStopId)
        {
            if (routeStopId == null || routeStopId == 0)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a valid route stop to see the schedule";
                return RedirectToAction("Index", "YYBusStop");
            }
            else
            {
                string busRouteCode = db.routeStops.Find(routeStopId).busRouteCode;
                int Number = (int)db.routeStops.Find(routeStopId).busStopNumber;
                int offsetMinutes = (int)db.routeStops.Find(routeStopId).offsetMinutes;

                var routeSchedules = from aRouteSchedule in db.routeSchedules
                                     where (aRouteSchedule.busRouteCode == busRouteCode)
                                     select aRouteSchedule;


                var RouteStopSchedules = from rSchedule in db.routeSchedules
                                         join rStop in db.routeStops
                                             on rSchedule.busRouteCode equals rStop.busRouteCode
                                         join bStop in db.busStops
                                             on rStop.busStopNumber equals bStop.busStopNumber
                                         join bRoute in db.busRoutes
                                             on rSchedule.busRouteCode equals bRoute.busRouteCode
                                         where (rSchedule.busRouteCode == busRouteCode && rStop.routeStopId == routeStopId)
                                         select new RouteStopScheduleView
                                         {
                                            routeScheduleId = rSchedule.routeScheduleId,
                                            busRouteCode = busRouteCode,
                                            routeName = bRoute.routeName,
                                            busStopNumber = (int)rStop.busStopNumber,
                                            location = bStop.location,
                                            //arriveTime = rSchedule.startTime.Add(TimeSpan.FromMinutes((int)rStop.offsetMinutes)),
                                            arriveTime = (TimeSpan)System.Data.Entity.DbFunctions.AddMinutes(rSchedule.startTime, rStop.offsetMinutes),
                                            isWeekday = rSchedule.isWeekDay
                                         };

                int nCount = RouteStopSchedules.Count();

                if(nCount == 0)
                {
                    TempData["messageType"] = "danger";
                    TempData["message"] = "There is no schedule infomation of this bus stop in this route.";
                    return RedirectToAction("RouteSelector", "YYBusStop", new { busStopNumber = Number });
                }

                return View(RouteStopSchedules.OrderBy(a => a.arriveTime));
            }
        }

        //release all the data
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
