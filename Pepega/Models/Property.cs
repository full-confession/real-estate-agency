using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace Pepega.Models
{

    public class PropertyType
    {
        public int PropertyTypeId { get; set; }

        public string Name { get; set; }

        public List<Property> Properties { get; set; }
    }

    public class Property
    {
        [DisplayName("#")]
        public int PropertyId { get; set; }

        [DisplayName("Тип")]
        public int PropertyTypeId { get; set; }

        [DisplayName("Дата добавления")]
        public DateTime CreationDate { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DisplayName("Кадастровый номер")]
        public string Number { get; set; }


        [Required(ErrorMessage = "Обязательное поле")]
        public int? StreetId { get; set; }

        [DisplayName("Улица")]
        public Street Street { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DisplayName("Дом")]
        public int HouseNumber { get; set; }

        [DisplayName("Строение")]
        public int? Building { get; set; }

        [DisplayName("Корпус")]
        public int? Housing { get; set; }

        [DisplayName("Квартира")]
        public int? ApartmentNumber { get; set; }


        public PropertyType PropertyType { get; set; }

        public BuildingDescription BuildingDescription { get; set; }

        public ApartmentDescription ApartmentDescription { get; set; }

        public AreaDescription AreaDescription { get; set; }


        public List<SellOrder> SellOrders { get; set; }
    }


    public class BuildingDescription
    {
        public int PropertyId { get; set; }

        [DisplayName("Число этажей")]
        public int? NumberOfFloors { get; set; }

        [DisplayName("Год постройки")]
        public int? BuildYear { get; set; }

        [DisplayName("Парковка")]
        public bool? Parking { get; set; }

        [DisplayName("Лифт")]
        public bool? Elevator { get; set; }

        public Property Property { get; set; }
    }

    public class ApartmentDescription
    {
        public int PropertyId { get; set; }

        [DisplayName("Число комнат")]
        public int? NumberOfRooms { get; set; }

        [DisplayName("Общая площадь")]
        public int? Area { get; set; }

        [DisplayName("Балкон")]
        public bool? Balcony { get; set; }

        [DisplayName("Этаж")]
        public int? Floor { get; set; }

        public Property Property { get; set; }
    }


    public class AreaDescription
    {
        public int PropertyId { get; set; }

        [DisplayName("Площадь")]
        public int? Area { get; set; }

        [DisplayName("Плодородная земля")]
        public bool? FertileSoil { get; set; }

        [DisplayName("Электричество")]
        public bool? Electricity { get; set; }

        [DisplayName("Водопровод")]
        public bool? Water { get; set; }

        public Property Property { get; set; }
    }

}
