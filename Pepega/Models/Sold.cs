using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Pepega.Models
{
    public class Sold
    {
        public int SellOrderId { get; set; }

        [DisplayName("Дата")]
        public DateTime Date { get; set; }

        [DisplayName("Цена недвижимости")]

        public decimal FinalPrice { get; set; }

        [DisplayName("Прибыль агенства")]
        public decimal Income { get; set; }

        [DisplayName("Прибыль менеджера")]
        public decimal IncomeManager { get; set; }

        public SellOrder SellOrder { get; set; }
    }

    public class Buyer
    {
        public int BuyerId { get; set; }
        public int SellOrderId { get; set; }
        public int ClientId { get; set; }

        public SellOrder SellOrder { get; set; }
        public Client Client { get; set; }
    }
}
