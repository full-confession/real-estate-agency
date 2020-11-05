using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pepega.Models;

namespace Pepega.Controllers
{

    public class ClientIndexModel
    {
        public int PageSize { get; set; } = 10;
        public int PageIndex { get; set; } = 1;
        public int PageCount { get; set; }
        public int TotalEntryCount { get; set; }
        public int SelectedEntyCount { get; set; }
        public ClientFilters Filters { get; set; } = new ClientFilters();
        public IEnumerable<Client> Clients { get; set; }
    }

    public class ClientCreateModel
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

        [DisplayName("Номер паспорта")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string PassportNumber { get; set; }

        [DisplayName("Дата рождения")]
        [Required(ErrorMessage = "Обязательное поле")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [DisplayName("Контактный номер")]
        public string PhoneNumber { get; set; }
    }

    public class ClientController : Controller
    {
        private readonly Context context;

        public ClientController(Context context)
        {
            this.context = context;
        }


        public async Task<IActionResult> Index([FromQuery] ClientIndexModel indexModel)
        {
            var query = context.Clients.AsNoTracking().OrderBy(e => e.ClientId).AsQueryable();
            query = query.ApplyFilters(indexModel.Filters);

            var count = await query.CountAsync();
            var result = await query
                .Skip((indexModel.PageIndex - 1) * indexModel.PageSize)
                .Take(indexModel.PageSize)
                .ToListAsync();

            return View(new ClientIndexModel
            {
                Clients = result,
                SelectedEntyCount = count,
                TotalEntryCount = await context.Clients.CountAsync(),
                PageCount = (int)Math.Ceiling(count / (double)indexModel.PageSize),
                Filters = indexModel.Filters,
                PageSize = indexModel.PageSize,
                PageIndex = indexModel.PageIndex
            });
        }

        public async Task<IActionResult> Inspect(int id)
        {
            var client = await context.Clients
                .Where(e => e.ClientId == id)
                .Include(e => e.Sellers)
                .ThenInclude(e => e.SellOrder)
                .Include(e => e.Buyers)
                .ThenInclude(e => e.SellOrder)
                .FirstOrDefaultAsync();



            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }



            return View(client);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] Client formClient)
        {

            if (!ModelState.IsValid)
            {
                return View("Edit", formClient);
            }

            var client = await context.Clients.FindAsync(formClient.ClientId);
            if (client == null)
            {
                return BadRequest();
            }

            client.FirstName = formClient.FirstName;
            client.MiddleName = formClient.MiddleName;
            client.LastName = formClient.LastName;
            client.PassportNumber = formClient.PassportNumber;
            client.PhoneNumber = formClient.PhoneNumber;

            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = client.ClientId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await context.Clients.FindAsync(id);
            if (client == null)
            {
                return BadRequest();
            }

            context.Clients.Remove(client);
            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Create", new ClientCreateModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ClientCreateModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", editModel);
            }

            var clientModel = new Client
            {
                RegistrationDate = DateTime.Now,
                BirthDate = editModel.BirthDate.Value,
                FirstName = editModel.FirstName,
                LastName = editModel.LastName,
                MiddleName = editModel.MiddleName,
                PassportNumber = editModel.PassportNumber,
                PhoneNumber = editModel.PhoneNumber
            };

            var client = await context.Clients.AddAsync(clientModel);
            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = client.Entity.ClientId });
        }
    }
}
