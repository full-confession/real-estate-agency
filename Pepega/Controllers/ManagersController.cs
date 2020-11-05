using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pepega.Models;

namespace Pepega.Controllers
{
    public class TopManager
    {
        public int ManagerId { get; set; }
        public int SoldCount { get; set; }
        public decimal Income { get; set; }
    }

    public class ManagerIndexModel
    {
        public TopManager TopManager { get; set; }

        //ublic ClientFilters Filters { get; set; } = new ClientFilters();
        public IEnumerable<Manager> Managers { get; set; }
    }
    public class ManagersController : Controller
    {
        private readonly Context context;

        public ManagersController(Context context)
        {
            this.context = context;
        }


        public async Task<IActionResult> Index()
        {

            var query = context.Managers.AsNoTracking().OrderBy(e => e.ManagerId).Include(e => e.City).AsQueryable();

            var result = await query
                .AsNoTracking()
                .ToListAsync();

            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            var first = month.AddMonths(-1);
            var last = month.AddDays(-1);

            var incomes = await context.Solds
                .Where(e => e.Date > first && e.Date < last)
                .GroupBy(e => e.SellOrder.ManagerId)
                .Select(g => new TopManager
                {
                    ManagerId = g.Key,
                    Income = g.Sum(e => e.Income),
                    SoldCount = g.Count()
                }).ToListAsync();

            var top = incomes.OrderByDescending(e => e.Income);

            return View(new ManagerIndexModel
            {
                Managers = result,
                TopManager = top.FirstOrDefault()
            });
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await context.Manager.AsNoTracking()
                .Include(e => e.City)
                .Include(e => e.SellOrders)
                .ThenInclude(e => e.Sold)
                .Include(e => e.SellOrders)
                .ThenInclude(e => e.Statuses)
                .ThenInclude(e => e.Status)
                .FirstOrDefaultAsync(m => m.ManagerId == id);

            if (manager == null)
            {
                return NotFound();
            }

            return View(manager);
        }

        // GET: Managers/Create
        public async Task<IActionResult> Create()
        {
            return View(new ManagerCreateModel
            {
                CityList = new SelectList(await context.Cities.AsNoTracking().ToListAsync(), "CityId", "Name")
            });
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ManagerCreateModel manager)
        {
            if (!ModelState.IsValid)
            {
                manager.CityList = new SelectList(await context.Cities.AsNoTracking().ToListAsync(), "CityId", "Name");
                return View(manager);
            }

            var model = new Manager
            {
                JoinDate = DateTime.Now,
                BirthDate = manager.BirthDate.Value,
                CityId = manager.CityId.Value,
                FirstName = manager.FirstName,
                MiddleName = manager.MiddleName,
                LastName = manager.LastName,
                OrderPercent = manager.OrderPercent.Value,
                PassportNumber = manager.PassportNumber,
                PhoneNumber = manager.PhoneNumber
            };

            context.Managers.Add(model);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = model.ManagerId });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await context.Manager.FindAsync(id);
            if (manager == null)
            {
                return NotFound();
            }

            var model = new ManagerCreateModel
            {
                BirthDate = manager.BirthDate,
                CityId = manager.CityId,
                FirstName = manager.FirstName,
                MiddleName = manager.MiddleName,
                LastName = manager.LastName,
                OrderPercent = manager.OrderPercent,
                PassportNumber = manager.PassportNumber,
                PhoneNumber = manager.PhoneNumber,
                CityList = new SelectList(await context.Cities.AsNoTracking().ToListAsync(), "CityId", "Name")
            };

            ViewBag.managerId = id.Value;

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] ManagerCreateModel model)
        {
            var manager = await context.Manager.FindAsync(id);
            if (manager == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.CityList = new SelectList(await context.Cities.AsNoTracking().ToListAsync(), "CityId", "Name");
                return View(model);
            }

            manager.BirthDate = model.BirthDate.Value;
            manager.FirstName = model.FirstName;
            manager.LastName = model.LastName;
            manager.MiddleName = model.MiddleName;
            manager.OrderPercent = model.OrderPercent.Value;
            manager.PassportNumber = model.PassportNumber;
            manager.PhoneNumber = model.PhoneNumber;
            manager.CityId = model.CityId.Value;

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
