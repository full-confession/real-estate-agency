using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Pepega.Models
{
    public class ClientFilters
    {
        [DisplayName("Код")]
        public int? Id { get; set; }

        [DisplayName("Имя")]
        public string FirstName { get; set; }

        [DisplayName("Фамилия")]
        public string LastName { get; set; }

        [DisplayName("Отчество")]
        public string MiddleName { get; set; }

        [DisplayName("Номер паспорта")]
        public string PassportNumber { get; set; }

        [DisplayName("Дата рождения")]
        [DataType(DataType.Date)]
        public DateTime? BirthDateFrom { get; set; }
        [DataType(DataType.Date)]
        public DateTime? BirthDateTo { get; set; }

        [DisplayName("Дата регистрации")]
        [DataType(DataType.Date)]
        public DateTime? RegistrationDateFrom { get; set; }
        [DataType(DataType.Date)]
        public DateTime? RegistrationDateTo { get; set; }
    }

    public static class ClientFiltersExtensions
    {

        public static IQueryable<Client> ApplyFilters(this IQueryable<Client> query,
            ClientFilters filterModel)
        {
            if (filterModel.Id.HasValue)
            {
                query = query.Where(e => e.ClientId == filterModel.Id);
            }

            if (filterModel.FirstName != null)
            {
                query = query.Where(e => EF.Functions.Like(e.FirstName, $"%{filterModel.FirstName}%"));
            }

            if (filterModel.LastName != null)
            {
                query = query.Where(e => EF.Functions.Like(e.LastName, $"%{filterModel.LastName}%"));
            }

            if (filterModel.MiddleName != null)
            {
                query = query.Where(e => EF.Functions.Like(e.MiddleName, $"%{filterModel.MiddleName}%"));
            }

            if (filterModel.PassportNumber != null)
            {
                query = query.Where(e => EF.Functions.Like(e.PassportNumber, $"%{filterModel.PassportNumber}%"));
            }


            query = query.BirthDateFilter(filterModel);
            query = query.RegistrationDateFilter(filterModel);
            return query;
        }

        public static IQueryable<Client> BirthDateFilter(this IQueryable<Client> query, ClientFilters filterModel)
        {

            var from = filterModel.BirthDateFrom;
            var to = filterModel.BirthDateTo;

            if (!from.HasValue &&
                !to.HasValue)
            {
                return query;
            }

            if (from.HasValue && !to.HasValue)
            {
                return query.Where(e =>
                    e.BirthDate >= from.Value);
            }

            if (!from.HasValue)
            {
                return query.Where(e =>
                    e.BirthDate <= to.Value);
            }


            return query.Where(e =>
                e.BirthDate >= from.Value
                && e.BirthDate <= to.Value);

        }

        public static IQueryable<Client> RegistrationDateFilter(this IQueryable<Client> query, ClientFilters filterModel)
        {

            var from = filterModel.RegistrationDateFrom;
            var to = filterModel.RegistrationDateTo;

            if (!from.HasValue &&
                !to.HasValue)
            {
                return query;
            }

            if (from.HasValue && !to.HasValue)
            {
                return query.Where(e =>
                    e.RegistrationDate >= from.Value);
            }

            if (!from.HasValue)
            {
                return query.Where(e =>
                    e.RegistrationDate <= to.Value);
            }


            return query.Where(e =>
                e.BirthDate >= from.Value
                && e.RegistrationDate <= to.Value);

        }
    }
}
