using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using YYBusService04.Models.ViewModels;
using YYBusService04.Models;

namespace YYBusService04.Controllers
{
    public class YYBusStopController : Controller
    {
        public enum sortingMethod
        {
            sortingByStopNumber=1,
            sortingByLocation,
            sortingByLocationHash
        }

        private BusServiceContext db = new BusServiceContext();

        // this action is for the default page for busStopController, display all the busStops ordered by location
        public ActionResult Index()
        {
            //return View(db.busStops.ToList().OrderBy(b => b.location));
            return View(db.busStops.OrderBy(b => b.location));
        }

        // display details for a particular stop 
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a valid stop first";
                return RedirectToAction("Index");
            }
            //find the busStop Object from the database
            busStop busstop = db.busStops.Find(id);
            if (busstop == null)
            {
                return HttpNotFound();
            }
            return View(busstop);
        }

        //navigate to the view to Create a new stop
        public ActionResult Create()
        {
            return View();
        }

        // POST: /YYBusStop/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //Get the date from the create form, then save the stop data into database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="busStopNumber,location,locationHash,goingDowntown")] busStop busstop)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.busStops.Add(busstop);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
               
            catch (Exception ex)
            {
                //get the baseException and build a error message 
                TempData["messageType"] = "danger";
                TempData["message"] = "exception thrown: " + ex.GetBaseException().ToString();
                
            }
            return Create();
        }

        // navigate to the bus stop edit view for a particular bus stop
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a valid stop first";
                return RedirectToAction("Index");
            }
            busStop busstop = db.busStops.Find(id);
            if (busstop == null)
            {
                return HttpNotFound();
            }
            return View(busstop);
        }

        // POST: /YYBusStop/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        // //Get the date from the Edit form, then update the stop data into database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="busStopNumber,location,locationHash,goingDowntown")] busStop busstop)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(busstop).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                
            }
            catch (Exception ex)
            {
                //get the baseException and build a error message 
                TempData["messageType"] = "danger";
                TempData["message"] = "exception thrown: " + ex.GetBaseException().ToString();
            }

            return View(busstop);
        }

        // navigate to the delete view and pass the selected stop object
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                //if the equals null stay at the index page and display a error message
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a valid stop first";
                return RedirectToAction("Index");
            }
            busStop busstop = db.busStops.Find(id);
            if (busstop == null)
            {
                return HttpNotFound();
            }
            return View(busstop);
        }

        // get the stop date from the delete page and then remove the record from the data base
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            busStop busstop = db.busStops.Find(id);
            db.busStops.Remove(busstop);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //accept the bus stop number, if the number is valid get all the busRoute 
        public ActionResult RouteSelector(int? busStopNumber)
        {
            if(busStopNumber == null || busStopNumber == 0 )
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a valid bus stop";
                return RedirectToAction("Index");
            }
            else if (null == db.busStops.Find(busStopNumber))
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a valid bus stop";
                return RedirectToAction("Index");
            }
            else 
            {
                //find the routes which include the busstop
                var routeStops = from rs in db.routeStops
                                    join br in db.busRoutes
                                        on rs.busRouteCode equals br.busRouteCode
                                    join bs in db.busStops
                                        on rs.busStopNumber equals bs.busStopNumber
                                 where (rs.busStopNumber == busStopNumber)
                                 select new StopRouteView
                                 {
                                     routeStopId = rs.routeStopId,
                                     busStopNumber = (Int32)rs.busStopNumber,
                                     Location = bs.location,
                                     busRouteCode = rs.busRouteCode,
                                     routeName = br.routeName 
                                 };
                //if there is no route using the selected stop, return to index action and display the message
                if(0 == routeStops.Count())
                {
                    string stopName = db.busStops.Find(busStopNumber).location;
                    string message = String.Format("There is no route using the stop - stop number:{0}, stop location: {1}", busStopNumber, stopName);

                    TempData["messageType"] = "danger";
                    TempData["message"] = message;
                    return RedirectToAction("index");
                }
                //if there is only one bus route using this stop, pass the routeStopId to the RouteStopSchedule action in the YYRouteSchedule controller
                else if (1 == routeStops.Count())
                {
                    return RedirectToAction("RouteStopSchedule", "YYRouteSchedule", new { routeStopId = routeStops.Single().routeStopId });
                }
                //if there are multiple routes using this stop, present a drop-down list of the routes for user to select
                else
                {
                    ViewBag.routeStopId = new SelectList(routeStops.OrderBy(a => a.routeName), "routeStopId", "routeName");
                    ViewBag.StopNumber = busStopNumber;
                    ViewBag.location = db.busStops.Find(busStopNumber).location;
                    return View();
                }
            }

        }

        //adding the sorting function to sort the bus stop listing in different ways
        public ActionResult Sorting(int sortingMethodId=0)
        {
            if (sortingMethodId == 0)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "Please select a sorting method first";
                return RedirectToAction("Index");
            }
            switch ((sortingMethod)sortingMethodId)
            {
                case sortingMethod.sortingByStopNumber:
                    return View("Index", db.busStops.OrderBy(b => b.busStopNumber));
                case sortingMethod.sortingByLocation:
                    return View("Index", db.busStops.OrderBy(b => b.location));
                case sortingMethod.sortingByLocationHash:
                    return View("Index", db.busStops.OrderBy(b => b.locationHash));
                default:
                    {
                        TempData["messageType"] = "danger";
                        TempData["message"] = "Please select a valid sorting method first";
                        return RedirectToAction("Index");
                    }
            }

            
        }

        //release the memory when quit
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
