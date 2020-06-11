
namespace Crashes.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public int SectorId { get; set; }
        public int SpecId { get; set; }
        public string Name { get; set; }
        public string Fullname { get; set; }
        public bool Active { get; set; }
    }
}