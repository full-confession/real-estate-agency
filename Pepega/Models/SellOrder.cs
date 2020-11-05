using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pepega.Models
{
    public class SellOrder
    {
        public int SellOrderId { get; set; }

        [DisplayName("Недвижимость")]
        public int PropertyId { get; set; }

        [DisplayName("Менеджер")]
        public int ManagerId { get; set; }


        [DisplayName("Дата создания")]
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; }

        [DisplayName("Цена")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [DisplayName("Номер")]
        public string Number { get; set; }

        [DisplayName("Стоимость услуг")]
        public decimal AgencyCharge { get; set; }

        [DisplayName("Процент агенства")]
        public decimal AgencyPercent { get; set; }


        public Property Property { get; set; }

        public Manager Manager { get; set; }

        public Sold Sold { get; set; }


        public IEnumerable<SellOrderStatus> Statuses { get; set; }
    }


    public class Status
    {
        public int StatusId { get; set; }

        public string Name { get; set; }

        public string Style { get; set; }

    }


    public class SellOrderStatus
    {
        public int SellOrderStatusId { get; set; }

        public int SellOrderId { get; set; }

        public int StatusId { get; set; }

        public DateTime SetDate { get; set; }


        public SellOrder SellOrder { get; set; }

        public Status Status { get; set; }
    }
}
