using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Crashes.Models
{
    public class Search
    {
        [DisplayName("Оборудование")]
        public int Equipment { get; set; }
        [DisplayName("Начало периода")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime PeriodStart { get; set; }
        [DisplayName("Окончание периода")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime PeriodStop { get; set; }

        public Role Role { get; set; }
        public List<Equipment> EquipmentRepo { get; set; }
    }
}