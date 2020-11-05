using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Pepega.Models
{
    public class Manager
    {
        public int ManagerId { get; set; }

        [DisplayName("Имя")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string FirstName { get; set; }

        [DisplayName("Фамилия")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string LastName { get; set; }

        [DisplayName("Отчество")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string MiddleName { get; set; }


        public City City { get; set; }

        [DisplayName("Город")]
        [Required(ErrorMessage = "Обязательное поле")]
        public int CityId { get; set; }

        [DisplayName("Дата рождения")]
        [Required(ErrorMessage = "Обязательное поле")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [DisplayName("Номер паспорта")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string PassportNumber { get; set; }

        [DisplayName("Дата устройства")]
        [Required(ErrorMessage = "Обязательное поле")]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        [DisplayName("Контактный номер")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string PhoneNumber { get; set; }

        [DisplayName("Процент с продаж")]
        [Required(ErrorMessage = "Обязательное поле")]
        public decimal OrderPercent { get; set; }


        public IEnumerable<SellOrder> SellOrders { get; set; }
    }


    public class ManagerCreateModel
    {
        [DisplayName("Имя")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string FirstName { get; set; }

        [DisplayName("Фамилия")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string LastName { get; set; }

        [DisplayName("Отчество")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string MiddleName { get; set; }

        [DisplayName("Город")]
        [Required(ErrorMessage = "Обязательное поле")]
        public int? CityId { get; set; }

        [DisplayName("Дата рождения")]
        [Required(ErrorMessage = "Обязательное поле")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [DisplayName("Номер паспорта")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string PassportNumber { get; set; }

        [DisplayName("Контактный номер")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string PhoneNumber { get; set; }

        [DisplayName("Процент с продаж")]
        [Required(ErrorMessage = "Обязательное поле")]
        public decimal? OrderPercent { get; set; }

        public SelectList CityList { get; set; }
    }
}
