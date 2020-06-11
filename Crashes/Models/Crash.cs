using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Crashes.Models
{
    public class Crash
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        [DisplayName("Цех")]
        public int GildId { get; set; }
        [DisplayName("Оборудование")]
        public int EquipmentId { get; set; }
        [DisplayName("Причина")]
        public string Reason { get; set; }
        [DisplayName("Статус")]
        public int StatusId { get; set; }
        [DisplayName("Начало")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime Start { get; set; }
        [DisplayName("Завершение")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime? Stop { get; set; }

        public int Foundry { get; set; }
        public int Sector { get; set; }
        public int Order { get; set; }

        public Role Role { get; set; }
        public List<Foundry> FoundryRepo { get; set; }
        public List<Gild> GildRepo { get; set; }
        public List<Sector> SectorRepo { get; set; }
        public List<Equipment> EquipmentRepo { get; set; }

        public string Sender { get; set; }
    }
}