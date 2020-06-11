using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Crashes.Models
{
    public class Spec
    {
        public int Id { get; set; }
        public int SectorId { get; set; }
        public string Name { get; set; }
        public int ReportOrder { get; set; }
    }
}