using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Crashes.Models;

namespace Crashes.Controllers
{
    public class HomeController : Controller
    {
        const int ClosedCrashesCount = 35;

        public ActionResult Index()
        {
            var account = User.Identity.Name.Split('\\').Last();
            var role = DB.GetRole(account);

            if (role.RoleId == 0)
                return View("NoPermissions", role);

            if (Session["RedirectGildId"] != null)
                role.ActiveGild = (int)Session["RedirectGildId"];

            var crash = GenerateEmptyCrash(role);
            return View(crash);
        }

        public ActionResult CrashGild(Crash crash)
        {
            if (crash.Role == null)
                return RedirectToAction("Index");

            crash.Role.ActiveGild = crash.GildId;
            crash.SectorRepo = DB.GetSectorRepo(crash.GildId);
            crash.EquipmentRepo = DB.GetEquipmentRepo(crash.SectorRepo).Where(x => x.Active).ToList();
            RemoveCrashedEquipmentFromRepo(crash);
            return View("Index", crash);
        }

        private static void RemoveCrashedEquipmentFromRepo(Crash crash)
        {
            var activeCrashes = DB.GetActiveCrashes(crash.GildId);
            if (activeCrashes != null)
            {
                foreach (var ac in activeCrashes)
                    crash.EquipmentRepo.RemoveAll(x => x.Id == ac.EquipmentId);
            }
        }

        public ActionResult ActiveCrashes(int gildId)
        {
            var crashes = DB.GetActiveCrashes(gildId);
            if (crashes != null)
                FillCrashRepos(crashes);

            return PartialView(crashes);
        }

        public ActionResult AllActiveCrashes()
        {
            var crashes = DB.GetActiveCrashes();
            if (crashes != null)
                FillCrashRepos(crashes);

            return PartialView(crashes);
        }

        public ActionResult ClosedCrashes(int gildId)
        {
            var crashes = DB.GetClosedCrashes(gildId, ClosedCrashesCount);
            FillCrashRepos(crashes);

            Session["ClosedCrashesCount"] = ClosedCrashesCount;
            return PartialView(crashes);
        }

        public ActionResult CrashManage(int id, string sender)
        {
            var crash = DB.GetCrashById(id);
            crash.Sender = sender;
            FillCrashRepos(crash);

            return View(crash);
        }

        public ActionResult AddCrash(Crash crash)
        {
            DB.CreateCrash(crash);

            Session["RedirectGildId"] = crash.GildId;
            return RedirectToAction("Index");
        }

        public ActionResult UpdateCrash(Crash crash)
        {
            DB.UpdateCrash(crash);

            if (crash.Sender == "search")
                return RedirectToAction("Search", new { equipment = crash.EquipmentId });
            else
            {
                ModelState.Clear();
                var role = DB.GetRole(User.Identity.Name.Split('\\').Last());
                role.ActiveGild = crash.GildId;
                var outCrash = GenerateEmptyCrash(role);
                outCrash.Role = role;

                return View("Index", outCrash);
            }
        }

        public ActionResult RemoveCrash(int id, int gild, int equipment, string sender)
        {
            DB.MarkCrashDeleted(id);

            if (sender == "search")
                return View("Search", GenerateEmptySearch(equipment));
            else
            {
                var role = DB.GetRole(User.Identity.Name.Split('\\').Last());
                role.ActiveGild = gild;
                var crash = GenerateEmptyCrash(role);
                return View("Index", crash);
            }
        }

        public ActionResult CancelManage(int gild, int equipment, string sender)
        {
            if (sender == "search")
                return View("Search", GenerateEmptySearch(equipment));
            else
            {
                var role = DB.GetRole(User.Identity.Name.Split('\\').Last());
                role.ActiveGild = gild;
                var crash = GenerateEmptyCrash(role);
                return View("Index", crash);
            }
        }

        private static void FillCrashRepos(Crash crash)
        {
            if (crash != null)
            {
                crash.Role = DB.GetRole(crash.Role.WorkerId);

                if (crash.Role != null)
                    crash.GildRepo = DB.GetGildRepo(crash.Role.GildIds);
                else
                {
                    crash.Role = new Role() { WorkerName = "УЗ удалена" };
                    var crashGildId = DB.GetGildIdByEquipmentId(crash.EquipmentId);
                    crash.GildRepo = DB.GetGildRepo(new List<int>() { crashGildId });
                }

                crash.FoundryRepo = DB.GetFoundryRepo();
                crash.SectorRepo = DB.GetSectorRepo(crash.GildId);
                crash.EquipmentRepo = DB.GetEquipmentRepo(crash.SectorRepo);
            }
        }

        private static void FillCrashRepos(List<Crash> crashes)
        {
            if (crashes != null)
                crashes.ForEach(x => FillCrashRepos(x));
        }

        public ActionResult Search(int equipment = 0)
        {
            var role = DB.GetRole(User.Identity.Name.Split('\\').Last());
            var search = new Search
            {
                Equipment = equipment,
                PeriodStart = DateTime.Now.AddDays(-30).Date,
                PeriodStop = DateTime.Now.Date
            };

            if (role.RoleId == 0)
                return View("NoPermissions");

            if (role != null)
            {
                var sectors = new List<Sector>();
                foreach (var gild in role.GildIds)
                    sectors.AddRange(DB.GetSectorRepo(gild));

                search.Role = role;
                search.EquipmentRepo = DB.GetEquipmentRepo(sectors);
            }
            else
                search.EquipmentRepo = DB.GetEquipmentRepo();

            return View(search);
        }

        public ActionResult SearchResults(Search search)
        {
            if (search.PeriodStart >= search.PeriodStop)
                return View("Search", search);

            Session["SearchPeriodStart"] = search.PeriodStart;
            Session["SearchPeriodStop"] = search.PeriodStop;

            var periodCrashRepo = DB.GetReportCrashes(search.PeriodStart, search.PeriodStop);
            var periodCrashes = periodCrashRepo.Where(x => x.EquipmentId == search.Equipment).ToList();

            if (periodCrashes.Count > 0)
                foreach (var crash in periodCrashes) crash.Role = DB.GetRole(crash.AuthorId);

            return PartialView("_SearchResults", periodCrashes);
        }

        private static Crash GenerateEmptyCrash(Role role)
        {
            var moment = DateTime.Now;
            var crash = new Crash()
            {
                Role = role,
                Start = moment,
                Stop = moment,
                FoundryRepo = DB.GetFoundryRepo()
            };

            if (role.ActiveGild != 0)
                crash.GildId = DB.GetGild(role.ActiveGild).Id;

            crash.GildRepo = DB.GetGildRepo(role.GildIds);
            crash.SectorRepo = DB.GetSectorRepo(crash.GildId);
            crash.EquipmentRepo = DB.GetEquipmentRepo(crash.SectorRepo);

            RemoveCrashedEquipmentFromRepo(crash);
            return crash;
        }

        private Search GenerateEmptySearch(int equipment)
        {
            var search = new Search()
            {
                Equipment = equipment,
                PeriodStart = DateTime.Now.AddDays(-30).Date,
                PeriodStop = DateTime.Now,
                Role = DB.GetRole(User.Identity.Name.Split('\\').Last())
            };

            var sectors = new List<Sector>();
            foreach (var gild in search.Role.GildIds)
                sectors.AddRange(DB.GetSectorRepo(gild));

            search.EquipmentRepo = DB.GetEquipmentRepo(sectors);
            return search;
        }
    }
}