using ClosedXML.Excel;
using Crashes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Crashes.Controllers
{
    public class ReportController : Controller
    {
        const string ReportFilePath = "~/ReportFile/report.xlsx";

        public ActionResult Index()
        {
            var account = User.Identity.Name.Split('\\').Last();
            var roleId = DB.GetRole(account).RoleId;

            if (roleId == 0)
                return View("NoPermissions");

            var report = new Report()
            {
                RoleId = roleId,
                PeriodStart = DateTime.Now.Date,
                PeriodStop = DateTime.Now
            };

            return View(report);
        }

        public ActionResult GetReport(Report report)
        {
            if (report.PeriodStart == new DateTime())
                report.PeriodStart = new DateTime(DateTime.Now.Year - 1, 1, 1);
            if (report.PeriodStop == new DateTime())
                report.PeriodStop = DateTime.Now;

            report.Crashes = DB.GetReportCrashes(report.PeriodStart, report.PeriodStop);
            GenerateReport(report);

            return RedirectToAction("DownloadReport");
        }

        private void GenerateReport(Report report)
        {
            var foundryRepo = DB.GetFoundryRepo();
            var gildRepo = DB.GetGildRepo();
            var sectorRepo = DB.GetSectorRepo();
            var specRepo = DB.GetSpecRepo();
            var equipmentRepo = DB.GetEquipmentRepo();

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Журнал простоев");
            ws.SheetView.FreezeRows(2);
            ws.Style.Font.FontSize = 9;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Column(1).Width = 35;
            ws.Column(3).Width = 15;
            ws.Column(4).Width = 15;
            ws.Column(5).Width = 13;
            ws.Column(6).Width = 100;
            ws.Range("A2:F2").Style.Fill.BackgroundColor = XLColor.LightSkyBlue;
            ws.Row(2).Height = 30;
            ws.Cell("A2").Value = "уч.";
            ws.Cell("B2").Value = "# п/п";
            ws.Cell("C2").Value = "Время н.п.";
            ws.Cell("D2").Value = "Время о.п.";
            ws.Cell("E2").Value = "Длительность п.";
            ws.Cell("F2").Value = "Причина простоя";

            ws.Range("A1:F1").Row(1).Merge().Value =
                "Отчет о простоях с " + report.PeriodStart.ToString() + " по " + report.PeriodStop.ToString();

            int row = 3;

            foundryRepo = foundryRepo.OrderBy(x => x.ReportOrder).ToList();
            foreach (var foundry in foundryRepo)
            {
                var currGilds = gildRepo.Where(x => x.FoundryId == foundry.Id).ToList();
                currGilds = currGilds.OrderBy(x => x.ReportOrder).ToList();

                foreach (var gild in currGilds)
                {
                    var currSectors = new List<Sector>(sectorRepo.Where(x => x.GildId == gild.Id).OrderBy(x => x.ReportOrder));
                    foreach (var sector in currSectors)
                    {
                        var currSpecs = new List<Spec>(specRepo.Where(x => x.SectorId == sector.Id).OrderBy(x => x.ReportOrder));
                        var currEquipment = new List<Equipment>(equipmentRepo.Where(x => x.SectorId == sector.Id));

                        foreach (var spec in currSpecs)
                        {
                            if (equipmentRepo.Where(x => x.SpecId == spec.Id && x.Active).Count() > 0)
                            {
                                ws.Range("A" + row + ":F" + row).Row(1).Merge().Value = foundry.Name + "/" + gild.Name.ToUpper();
                                ws.Cell("A" + row).Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                                row++;
                                ws.Range("A" + row + ":F" + row).Row(1).Merge().Value = sector.Name.ToUpper();
                                ws.Cell("A" + row).Style.Fill.BackgroundColor = XLColor.YellowGreen;
                                row++;

                                if (spec.Name != string.Empty)
                                {
                                    ws.Range("A" + row + ":F" + row).Row(1).Merge().Value = spec.Name.ToUpper();
                                    ws.Cell("A" + row).Style.Fill.BackgroundColor = XLColor.LightSalmon;
                                    row++;
                                }

                                var equipment = currEquipment.Where(x => x.SectorId == sector.Id && x.SpecId == spec.Id).ToList();
                                if (equipment.Count() > 0)
                                {
                                    foreach (var eq in equipment)
                                    {
                                        if (eq.Active)
                                        {
                                            var equipmentCrashes = report.Crashes.Where(x => x.EquipmentId == eq.Id).ToList();
                                            if (equipmentCrashes.Count() > 0)
                                            {
                                                int crashNum = 1;
                                                var totalDuration = new TimeSpan();

                                                foreach (var crash in equipmentCrashes)
                                                {
                                                    ws.Cell("A" + row).Value = eq.Name;
                                                    ws.Cell("B" + row).Value = crashNum++;
                                                    ws.Cell("C" + row).Style.NumberFormat.Format = "dd.MM.yyyy HH:mm";
                                                    ws.Cell("C" + row).Value = crash.Start;
                                                    ws.Cell("D" + row).Style.NumberFormat.Format = "dd.MM.yyyy HH:mm";
                                                    if (crash.StatusId != 1) ws.Cell("D" + row).Value = crash.Stop;
                                                    else crash.Stop = report.PeriodStop;

                                                    var crashDuration = (DateTime)crash.Stop - crash.Start;
                                                    totalDuration += crashDuration;

                                                    ws.Cell("E" + row).Value = ((int)crashDuration.TotalHours).ToString() + " ч. " + crashDuration.Minutes.ToString() + " мин.";
                                                    ws.Cell("F" + row).Value = crash.Reason;
                                                    row++;
                                                }

                                                ws.Range("A" + row + ":D" + row).Row(1).Merge().Value = eq.Fullname + " (общий простой)";
                                                ws.Cell("E" + row).Value = ((int)totalDuration.TotalHours).ToString() + " ч. " + totalDuration.Minutes.ToString() + " мин.";

                                                ws.Range("A" + row + ":E" + row).Style.Fill.BackgroundColor = XLColor.LightGray;
                                                row++;
                                            }
                                            else
                                            {
                                                ws.Cell("A" + row).Value = eq.Name;
                                                ws.Cell("B" + row).Value = 1;
                                                ws.Cell("E" + row).Value = "'00:00";
                                                row++;

                                                ws.Range("A" + row + ":D" + row).Row(1).Merge().Value = eq.Fullname + " (общий простой)";
                                                ws.Cell("E" + row).Value = "'00:00";
                                                ws.Range("A" + row + ":E" + row).Style.Fill.BackgroundColor = XLColor.LightGray;
                                                row++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using (HostingEnvironment.Impersonate())
            {
                wb.SaveAs(Server.MapPath(ReportFilePath));
            }
        }

        public FileResult DownloadReport()
        {
            var name = Path.GetFileName(Server.MapPath(ReportFilePath));
            var doc = System.IO.File.ReadAllBytes(Server.MapPath(ReportFilePath));

            return File(doc, "application/vnd.ms-excel", name);
        }
    }
}