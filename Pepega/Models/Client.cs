using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Pepega.Models
{
    public class Client
    {
        public int ClientId { get; set; }

        [DisplayName("Дата регистрации")]
        public DateTime RegistrationDate { get; set; }

        [DisplayName("Имя")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string FirstName { get; set; }

        [DisplayName("Фамилия")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string LastName { get; set; }

        [DisplayName("Отчество")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string MiddleName { get; set; }

        [DisplayName("Номер паспорта")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string PassportNumber { get; set; }

        [DisplayName("Дата рождения")]
        [Required(ErrorMessage = "Обязательное поле")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [DisplayName("Контактный номер")]
        public string PhoneNumber { get; set; }


        //public List<SellOrder> SellOrders { get; set; }

        public List<Seller> Sellers { get; set; }
        public List<Buyer> Buyers { get; set; }
    }


    public class Seller
    {
        public int SellerId { get; set; }
        public int ClientId { get; set; }
        public int SellOrderId { get; set; }

        public SellOrder SellOrder { get; set; }
        public Client Client { get; set; }
        public Ownership Ownership { get; set; }

    }

    public class Ownership
    {
        public int SellerId { get; set; }
        public string Number { get; set; }
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; }
    }
}
