using System.Collections.Generic;

namespace Crashes.Models
{
    public class Role
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; }
        public string Account { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<int> GildIds { get; set; }
        public int ActiveGild { get; set; }
    }
}