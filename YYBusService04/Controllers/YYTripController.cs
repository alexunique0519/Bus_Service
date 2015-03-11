using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YYBusService04.Models.ViewModels;
using YYBusService04.Models;
using System.Data.Entity.SqlServer;

namespace YYBusService.Controllers
{
    //This controller is used to manage the data and render view which is associated with trip.
 
    public class YYTripController : Controller
    {
        //define a dbContext in YYTripController 
        private BusServiceContext db = new BusServiceContext();

        //pass a busRouteCode and route a name to index action, then get all the data associated with the routeCode and pass them to the index view
        public ActionResult Index(string busRouteCode, string routeName)
        {
            //if there is query string about busRouteCode and routeName, save them in session variables, and get all the trip info 
            if(busRouteCode != null && routeName != null)
            {
                Session["routeCode"] = busRouteCode;
                Session["routeName"] = routeName;
                
                //using Linq to retrive all the required data associated with the particular busRouteCode and assign the value to 
                //pre-defined viewModel collection
                var TripBusDriverViews = from rs in db.routeSchedules
                                                join atrip in db.trips
                                                    on rs.routeScheduleId equals atrip.routeScheduleId
                                                join adriver in db.drivers
                                                    on atrip.driverId equals adriver.driverId
                                                join abus in db.buses
                                                    on atrip.busId equals abus.busId
                                         where rs.busRouteCode.Equals (busRouteCode)
                                         orderby atrip.tripDate descending, rs.startTime ascending
                                         select new TripBusDriverView
                                         {
                                             tripId = atrip.tripId,
                                             tripDate = SqlFunctions.DateName("dw", atrip.tripDate).Remove(3) + " " + (SqlFunctions.DatePart("dd", atrip.tripDate)+100).ToString().Remove(0, 1)
                                                        + " " + SqlFunctions.DateName("MM", atrip.tripDate).Remove(3) + " " + SqlFunctions.DateName("YYYY", atrip.tripDate),
                                             startTime = rs.startTime,
                                             driverName = adriver.fullName,
                                             busNumber = abus.busNumber,
                                             comments = atrip.comments
                                         };
               
               return View(TripBusDriverViews);

            }
            //if the session data of "busRouteCode" is valid, get all the trip info
            else if (Session["busRouteCode"] != null)
            {
                string sbusRouteCode = Session["busRouteCode"].ToString();
                Session["routeName"] = db.busRoutes.Find(sbusRouteCode).routeName;

                //using Linq to retrive all the required data associated with the particular busRouteCode and assign the value to 
                //pre-defined viewModel collection
                var TripBusDriverViews = from rs in db.routeSchedules
                                         join atrip in db.trips
                                             on rs.routeScheduleId equals atrip.routeScheduleId
                                         join adriver in db.drivers
                                             on atrip.driverId equals adriver.driverId
                                         join abus in db.buses
                                             on atrip.busId equals abus.busId
                                         where rs.busRouteCode.Equals(sbusRouteCode)
                                         orderby atrip.tripDate descending, rs.startTime ascending
                                         select new TripBusDriverView
                                         {
                                             tripId = atrip.tripId,
                                             tripDate = SqlFunctions.DateName("dw", atrip.tripDate).Remove(3) + " " + SqlFunctions.DateName("dd", atrip.tripDate)
                                                        + " " + SqlFunctions.DateName("MM", atrip.tripDate).Remove(3) + " " + SqlFunctions.DateName("YYYY", atrip.tripDate),
                                             startTime = rs.startTime,
                                             driverName = adriver.fullName,
                                             busNumber = abus.busNumber,
                                             comments = atrip.comments
                                         };

                return View(TripBusDriverViews);
            }

            //if busRouteCode can not get from either the querystring or session data, return a message and display
            //it on the top of the index page in YYBusRoute Controller
            TempData["messageType"] = "danger";
            TempData["message"] = "please select a valid bus route in the bus route listing";
            return RedirectToAction("index", "YYBusRoute");
        }

        //retrive routeSchedule accoding to specified routeCode, and also drivers infos as well as bus infos, 
        //and new appropriate object before assigning them to viewbag.
        public void GatherAllDataFortheView(string routeCode)
        {
            var routeSchedules = from rs in db.routeSchedules
                                 where rs.busRouteCode.Equals(routeCode)
                                 orderby rs.startTime
                                 select rs;

            int nCount = routeSchedules.Count();

            //put all the schedules into a viewbag data called "ViewBag.schedulesList"
            ViewBag.routeScheduleId = new SelectList(routeSchedules, "routeScheduleId", "startTime");

            //using linq to retrieve all the driver's infos and put them into the viewbag
            var driverInfos = from adriver in db.drivers
                              orderby adriver.fullName
                              select adriver;

            //put all the drivers into viewbag data called "ViewBag.driversList"
            ViewBag.driversList = new SelectList(driverInfos, "driverId", "fullName");

            var busses = from abus in db.buses
                         where abus.status.Equals("available")
                         orderby abus.busNumber
                         select abus;

            ViewBag.busses = busses.OrderBy(a => a.busNumber).ToList();
        }

        //this action will retrive all the necessary data and render a create view for bus trip.
        public ActionResult Create()
        {
            if(Session["routeCode"] == null)
            {
                TempData["messageType"] = "danger";
                TempData["message"] = "please select a valid bus route in the bus route listing";
                return RedirectToAction("index", "YYBusRoute");
            }

            //get the routeCode which has been stored in session data
            string routeCode = Session["routeCode"].ToString();

            //invoke this funtion to retrive all the data and put it in viewbag.
            GatherAllDataFortheView(routeCode);

            return View();
        }

        //receive the post data and save it into the data base, if the modelstate is unvalid or there is any exception show messages
        //and return to the creat view.
        [HttpPost]
        public ActionResult Create(trip busTrip)
        {
            try
            {
                //because if there is no radiobutton selected, the busId is not null but 0, so busId value should be checked 
                if (busTrip.busId == 0)
                {
                    ModelState.AddModelError("busId", "a bus number should be selected");
                }

                if(ModelState.IsValid)
                {
                    string busRouteCode = db.routeSchedules.Find(busTrip.routeScheduleId).busRouteCode;
                    string RouteName = db.busRoutes.Find(busRouteCode).routeName;

                    db.trips.Add(busTrip);
                    db.SaveChanges();
                    TempData["messageType"] = "success";
                    TempData["message"] = "one new trip record has beed added";
                    return RedirectToAction("Index", new {busRouteCode, RouteName});
                }
                             
            }
            catch (Exception ex)
            {
                //if the database throws any exception, add the innermost exception into the modelstate
                ModelState.AddModelError(string.Empty, "exception thrown: " + ex.GetBaseException());
            }

            string routeCode = Session["routeCode"].ToString();
            GatherAllDataFortheView(routeCode);
            return View(busTrip);

        }
    }
}