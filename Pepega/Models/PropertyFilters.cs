using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Remotion.Linq.Clauses;

namespace Pepega.Models
{
    public class PropertyFilterModel
    {
        public int PageSize { get; set; } = 10;

        public int PageIndex { get; set; } = 1;


        [DisplayName("Город")]
        public int? CityId { get; set; }

        [DisplayName("Район")]
        public int? DistrictId { get; set; }

        [DisplayName("Улица")]
        public int? StreetId { get; set; }

        [DisplayName("Дом")]
        public int? HouseId { get; set; }


        [DisplayName("Тип")]
        public int? PropertyTypeId { get; set; }


        public BuildingFilterModel BuildingFilter { get; set; } = new BuildingFilterModel();
        public ApartmentFilterModel ApartmentFilter { get; set; } = new ApartmentFilterModel();
        public AreaFilterModel AreaFilter { get; set; } = new AreaFilterModel();
    }

    public class BuildingFilterModel
    {
        [DisplayName("Число этажей")]
        public int? NumberOfFloorsFrom { get; set; }
        public int? NumberOfFloorsTo { get; set; }
        public bool NumberOfFloorsApplied { get; set; }

        [DisplayName("Год постройки")]
        public int? BuildYearFrom { get; set; }
        public int? BuildYearTo { get; set; }
        public bool BuildYearApplied { get; set; }

        [DisplayName("Парковка")]
        public bool? Parking { get; set; }
        public bool ParkingApplied { get; set; }

        [DisplayName("Лифт")]
        public bool? Elevator { get; set; }
        public bool ElevatorApplied { get; set; }
    }

    public class ApartmentFilterModel
    {
        [DisplayName("Число комнат")]
        public int? NumberOfRoomsFrom { get; set; }
        public int? NumberOfRoomsTo { get; set; }
        public bool NumberOfRoomsApplied { get; set; }

        [DisplayName("Общая площадь")]
        public int? AreaFrom { get; set; }
        public int? AreaTo { get; set; }
        public bool AreaApplied { get; set; }


        [DisplayName("Балкон")]
        public bool? Balcony { get; set; }
        public bool BalconyApplied { get; set; }

        [DisplayName("Этаж")]
        public int? FloorFrom { get; set; }
        public int? FloorTo { get; set; }
        public bool FloorApplied { get; set; }
    }

    public class AreaFilterModel
    {
        [DisplayName("Площадь")]
        public int? AreaFrom { get; set; }
        public int? AreaTo { get; set; }
        public bool AreaApplied { get; set; }


        [DisplayName("Электричество")]
        public bool? Electricity { get; set; }
        public bool ElectricityApplied { get; set; }

        [DisplayName("Водопровод")]
        public bool? Water { get; set; }
        public bool WaterApplied { get; set; }

        [DisplayName("Плодородная земля")]
        public bool? FertileSoil { get; set; }
        public bool FertileSoilApplied { get; set; }
    }

    public static class PropertyFilters
    {

        public static IQueryable<Property> ApplyFilters(this IQueryable<Property> query,
            PropertyFilterModel filterModel, Context context)
        {
            if (filterModel.PropertyTypeId.HasValue)
            {
                query = query.Where(e => e.PropertyTypeId == filterModel.PropertyTypeId);
            }

            if (filterModel.StreetId.HasValue)
            {
                query = query.Where(e => e.StreetId == filterModel.StreetId);

                if (filterModel.HouseId != null)
                {
                    query = query.Where(e => e.HouseNumber == filterModel.HouseId.Value);
                }
            }
            else if (filterModel.DistrictId.HasValue)
            {
                query = query.Where(e =>
                    context.Streets.Where(
                        ee => ee.DistrictId == filterModel.DistrictId.Value)
                        .Any(eee => eee.StreetId == e.StreetId));
            }
            else if (filterModel.CityId.HasValue)
            {
                query = query.Where(e =>
                    context.Streets.Where(
                        ee => context.Districts.Where(eee => eee.CityId == filterModel.CityId.Value).Any(eee => eee.DistrictId == ee.DistrictId))
                        .Any(eee => eee.StreetId == e.StreetId));
            }


            if (filterModel.ApartmentFilter.Balcony.HasValue)
            {
                filterModel.ApartmentFilter.BalconyApplied = true;
                query = query.Where(e => e.ApartmentDescription.Balcony == filterModel.ApartmentFilter.Balcony.Value);
            }

            if (filterModel.BuildingFilter.Parking.HasValue)
            {
                filterModel.BuildingFilter.ParkingApplied = true;
                query = query.Where(e => e.BuildingDescription.Parking == filterModel.BuildingFilter.Parking.Value);
            }

            if (filterModel.BuildingFilter.Elevator.HasValue)
            {
                filterModel.BuildingFilter.ElevatorApplied = true;
                query = query.Where(e => e.BuildingDescription.Elevator == filterModel.BuildingFilter.Elevator.Value);
            }

            if (filterModel.AreaFilter.Electricity.HasValue)
            {
                filterModel.AreaFilter.ElectricityApplied = true;
                query = query.Where(e => e.AreaDescription.Electricity == filterModel.AreaFilter.Electricity.Value);
            }

            if (filterModel.AreaFilter.Water.HasValue)
            {
                filterModel.AreaFilter.WaterApplied = true;
                query = query.Where(e => e.AreaDescription.Water == filterModel.AreaFilter.Water.Value);
            }

            if (filterModel.AreaFilter.FertileSoil.HasValue)
            {
                filterModel.AreaFilter.FertileSoilApplied = true;
                query = query.Where(e => e.AreaDescription.FertileSoil == filterModel.AreaFilter.FertileSoil.Value);
            }


            query = query.BuildYearFilter(filterModel);
            query = query.NumberOfFloorsFilter(filterModel);

            query = query.AreaFilter(filterModel);
            query = query.NumberOfRoomsFilter(filterModel);

            query = query.FloorFilter(filterModel);
            query = query.AreaAreaFilter(filterModel);

            return query;
        }

        public static IQueryable<Property> NumberOfFloorsFilter(this IQueryable<Property> query, PropertyFilterModel filterModel)
        {

            var from = filterModel.BuildingFilter.NumberOfFloorsFrom;
            var to = filterModel.BuildingFilter.NumberOfFloorsTo;

            if (!from.HasValue &&
                !to.HasValue)
            {
                return query;
            }

            filterModel.BuildingFilter.NumberOfFloorsApplied = true;

            if (from.HasValue && !to.HasValue)
            {
                return query.Where(e =>
                    e.BuildingDescription.NumberOfFloors >= from.Value);
            }

            if (!from.HasValue)
            {
                return query.Where(e =>
                    e.BuildingDescription.NumberOfFloors <= to.Value);
            }


            return query.Where(e =>
                e.BuildingDescription.NumberOfFloors >= from.Value
                && e.BuildingDescription.NumberOfFloors <= to.Value);

        }

        public static IQueryable<Property> BuildYearFilter(this IQueryable<Property> query, PropertyFilterModel filterModel)
        {

            var from = filterModel.BuildingFilter.BuildYearFrom;
            var to = filterModel.BuildingFilter.BuildYearTo;

            if (!from.HasValue &&
                !to.HasValue)
            {
                return query;
            }

            filterModel.BuildingFilter.BuildYearApplied = true;

            if (from.HasValue && !to.HasValue)
            {
                return query.Where(e =>
                    e.BuildingDescription.BuildYear >= from.Value);
            }

            if (!from.HasValue)
            {
                return query.Where(e =>
                    e.BuildingDescription.BuildYear <= to.Value);
            }


            return query.Where(e =>
                e.BuildingDescription.BuildYear >= from.Value
                && e.BuildingDescription.BuildYear <= to.Value);

        }

        public static IQueryable<Property> AreaFilter(this IQueryable<Property> query, PropertyFilterModel filterModel)
        {

            var from = filterModel.ApartmentFilter.AreaFrom;
            var to = filterModel.ApartmentFilter.AreaTo;

            if (!from.HasValue &&
                !to.HasValue)
            {
                return query;
            }

            filterModel.ApartmentFilter.AreaApplied = true;

            if (from.HasValue && !to.HasValue)
            {
                return query.Where(e =>
                    e.ApartmentDescription.Area >= from.Value);
            }

            if (!from.HasValue)
            {
                return query.Where(e =>
                    e.ApartmentDescription.Area <= to.Value);
            }


            return query.Where(e =>
                e.ApartmentDescription.Area >= from.Value
                && e.ApartmentDescription.Area <= to.Value);
        }

        public static IQueryable<Property> AreaAreaFilter(this IQueryable<Property> query, PropertyFilterModel filterModel)
        {

            var from = filterModel.AreaFilter.AreaFrom;
            var to = filterModel.AreaFilter.AreaTo;

            if (!from.HasValue &&
                !to.HasValue)
            {
                return query;
            }

            filterModel.AreaFilter.AreaApplied = true;

            if (from.HasValue && !to.HasValue)
            {
                return query.Where(e =>
                    e.AreaDescription.Area >= from.Value);
            }

            if (!from.HasValue)
            {
                return query.Where(e =>
                    e.AreaDescription.Area <= to.Value);
            }


            return query.Where(e =>
                e.AreaDescription.Area >= from.Value
                && e.AreaDescription.Area <= to.Value);
        }

        public static IQueryable<Property> NumberOfRoomsFilter(this IQueryable<Property> query, PropertyFilterModel filterModel)
        {

            var from = filterModel.ApartmentFilter.NumberOfRoomsFrom;
            var to = filterModel.ApartmentFilter.NumberOfRoomsTo;

            if (!from.HasValue &&
                !to.HasValue)
            {
                return query;
            }

            filterModel.ApartmentFilter.NumberOfRoomsApplied = true;

            if (from.HasValue && !to.HasValue)
            {
                return query.Where(e =>
                    e.ApartmentDescription.NumberOfRooms >= from.Value);
            }

            if (!from.HasValue)
            {
                return query.Where(e =>
                    e.ApartmentDescription.NumberOfRooms <= to.Value);
            }


            return query.Where(e =>
                e.ApartmentDescription.NumberOfRooms >= from.Value
                && e.ApartmentDescription.NumberOfRooms <= to.Value);
        }

        public static IQueryable<Property> FloorFilter(this IQueryable<Property> query, PropertyFilterModel filterModel)
        {

            var from = filterModel.ApartmentFilter.FloorFrom;
            var to = filterModel.ApartmentFilter.FloorTo;

            if (!from.HasValue &&
                !to.HasValue)
            {
                return query;
            }

            filterModel.ApartmentFilter.FloorApplied = true;

            if (from.HasValue && !to.HasValue)
            {
                return query.Where(e =>
                    e.ApartmentDescription.Floor >= from.Value);
            }

            if (!from.HasValue)
            {
                return query.Where(e =>
                    e.ApartmentDescription.Floor <= to.Value);
            }


            return query.Where(e =>
                e.ApartmentDescription.Floor >= from.Value
                && e.ApartmentDescription.Floor <= to.Value);
        }
    }
}
