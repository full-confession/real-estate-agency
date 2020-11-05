using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Pepega.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Pepega.Controllers
{

    public class SellOrderIndexModel
    {
        public IEnumerable<SellOrder> SellOrders { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageCount { get; set; }
    }

    public class SellOrderCreateModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        [DisplayName("# Недвижимости")]
        public int? PropertyId { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DisplayName("Цена")]
        [DataType(DataType.Currency)]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DisplayName("Стоимость услуг")]
        public decimal? AgencyCharge { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [DisplayName("Процент агенства")]
        public decimal? AgencyPercent { get; set; }
    }

    public class SellOrderInspectModel
    {
        public SelectList StatusList { get; set; }
        public SellOrder SellOrder { get; set; }
        public IEnumerable<Seller> Sellers { get; set; }
        public IEnumerable<SellOrderStatus> Statuses { get; set; }
    }

    public class SellOrderController : Controller
    {

        private readonly Context context;
        public SellOrderController(Context context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {

            var query = context.SellOrders
                .OrderBy(e => e.SellOrderId).AsQueryable();

            var count = await query.CountAsync();
            var result = await query
                .Skip((pageIndex - 1) * 50)
                .Take(50)
                .Include(e => e.Statuses)
                .ThenInclude(e => e.Status)
                .ToListAsync();


            var model = new SellOrderIndexModel
            {
                SellOrders = result,
                PageCount = (int)Math.Ceiling(count / (double)50),
                PageIndex = pageIndex
            };

            return View("Index", model);
        }

        public IActionResult Create()
        {
            return View("Create", new SellOrderCreateModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] SellOrderCreateModel createModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", createModel);
            }

            if (!await context.Properties.AnyAsync(e => e.PropertyId == createModel.PropertyId.Value))
            {
                ModelState.AddModelError("PropertyId", "Несуществующая недвижимость");
                return View("Create", createModel);
            }

            var model = new SellOrder
            {
                PropertyId = createModel.PropertyId.Value,
                CreationDate = DateTime.Now,
                Price = createModel.Price.Value,
                AgencyCharge = createModel.AgencyCharge.Value,
                AgencyPercent = createModel.AgencyPercent.Value
            };

            var rand = new Random();
            var letters = new List<char> { 'А', 'Б', 'В', 'Н' };

            model.Number = $"{letters.GetRandom(rand)}{model.CreationDate.Year}-{letters.GetRandom(rand)}{model.CreationDate.Month}{model.CreationDate.Day}-{rand.Next(1000, 10000)}";

            var sellOrder = (await context.SellOrders.AddAsync(model)).Entity;

            var status = await context.Statuses.FindAsync(1);

            await context.SellOrderStatuses.AddAsync(new SellOrderStatus
            {
                SellOrder = sellOrder,
                Status = status,
                SetDate = DateTime.Now
            });

            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = model.SellOrderId });
        }

        public async Task<IActionResult> Inspect(int id)
        {
            var entity = await context.SellOrders.AsNoTracking()
                .Include(e => e.Property)
                .ThenInclude(e => e.Street).ThenInclude(e => e.District).ThenInclude(e => e.City)
                .Include(e => e.Manager)
                .SingleOrDefaultAsync(e => e.SellOrderId == id);

            if (entity == null)
            {
                return NotFound();
            }

            var model = new SellOrderInspectModel
            {
                SellOrder = entity,
                Statuses = await context.SellOrderStatuses
                    .AsNoTracking()
                    .Include(e => e.Status)
                    .Where(e => e.SellOrderId == entity.SellOrderId)
                    .OrderByDescending(e => e.SetDate)
                    .ToListAsync(),

                Sellers = await context.Sellers.AsNoTracking()
                    .Where(e => e.SellOrderId == entity.SellOrderId)
                    .OrderBy(e => e.ClientId)
                    .Include(e => e.Client)
                    .Include(e => e.Ownership)
                    .ToListAsync(),

                StatusList = new SelectList(await context.Statuses
                    .AsNoTracking()
                    .ToListAsync(), "StatusId", "Name")
            };

            return View("Inspect", model);
        }


        public class AddSellerModel
        {
            public int SellOrderId { get; set; }


            [Required(ErrorMessage = "Обязательное поле")]
            [DisplayName("Клиент")]
            public int? ClientId { get; set; }


            [Required(AllowEmptyStrings = false, ErrorMessage = "Обязательное поле")]
            [DisplayName("Номер свидетельства")]
            public string OwnershipNumber { get; set; }


            [DataType(DataType.Date)]
            [Required(ErrorMessage = "Обязательное поле")]
            [DisplayName("Дата выдачи")]
            public DateTime? OwnershipIssueDate { get; set; }
        }

        public async Task<IActionResult> AddSeller(int sellOrderId)
        {
            var entity = await context.SellOrders.AsNoTracking()
                .SingleOrDefaultAsync(e => e.SellOrderId == sellOrderId);

            if (entity == null)
            {
                return BadRequest();
            }

            return View(new AddSellerModel { SellOrderId = sellOrderId });
        }

        [HttpPost]
        public async Task<IActionResult> AddSeller([FromForm] AddSellerModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = await context.SellOrders.AsNoTracking()
                .SingleOrDefaultAsync(e => e.SellOrderId == model.SellOrderId);

            if (entity == null)
            {
                return BadRequest();
            }

            var ownership = (await context.Ownerships.AddAsync(new Ownership
            {
                Number = model.OwnershipNumber,
                IssueDate = model.OwnershipIssueDate.Value
            })).Entity;

            var client = await context.Clients.FindAsync(model.ClientId.Value);

            if (client == null)
            {
                ModelState.AddModelError("ClientId", "Несуществующий клиент");
                return View(model);
            }

            await context.Sellers.AddAsync(new Seller
            {
                SellOrderId = model.SellOrderId,
                ClientId = model.ClientId.Value,
                Ownership = ownership
            });

            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = model.SellOrderId });
        }


        public async Task<IActionResult> LockSellers(int sellOrderId)
        {
            var entity = await context.SellOrders.AsNoTracking()
                .SingleOrDefaultAsync(e => e.SellOrderId == sellOrderId);

            if (entity == null)
            {
                return BadRequest();
            }

            var anySellers = await context.Sellers.AsNoTracking().AnyAsync(e => e.SellOrderId == sellOrderId);

            if (!anySellers)
            {
                return BadRequest();
            }


            await context.SellOrderStatuses.AddAsync(new SellOrderStatus
            {
                SellOrderId = entity.SellOrderId,
                StatusId = (await context.Statuses.Where(e => e.Name == "На проверке").FirstAsync()).StatusId,
                SetDate = DateTime.Now
            });

            await context.SaveChangesAsync();


            return RedirectToAction("Inspect", new { id = sellOrderId });
        }

        public async Task<IActionResult> Verify(int sellOrderId)
        {
            var entity = await context.SellOrders.AsNoTracking()
                .SingleOrDefaultAsync(e => e.SellOrderId == sellOrderId);


            if (entity == null)
            {
                return BadRequest();
            }

            await context.SellOrderStatuses.AddAsync(new SellOrderStatus
            {
                SellOrderId = entity.SellOrderId,
                StatusId = (await context.Statuses.Where(e => e.Name == "Активен").FirstAsync()).StatusId,
                SetDate = DateTime.Now
            });

            await context.SaveChangesAsync();


            return RedirectToAction("Inspect", new { id = sellOrderId });
        }

        public async Task<IActionResult> Reject(int sellOrderId)
        {
            var entity = await context.SellOrders.AsNoTracking()
                .SingleOrDefaultAsync(e => e.SellOrderId == sellOrderId);


            if (entity == null)
            {
                return BadRequest();
            }

            await context.SellOrderStatuses.AddAsync(new SellOrderStatus
            {
                SellOrderId = entity.SellOrderId,
                StatusId = (await context.Statuses.Where(e => e.Name == "Прекращен").FirstAsync()).StatusId,
                SetDate = DateTime.Now
            });

            await context.SaveChangesAsync();


            return RedirectToAction("Inspect", new { id = sellOrderId });
        }
        public class NewStatusModel
        {
            public int? SellOrderId { get; set; }

            public int? StatusId { get; set; }
        }

        public async Task<IActionResult> SetNewStatus([FromForm] NewStatusModel model)
        {
            if (!model.SellOrderId.HasValue)
            {
                return RedirectToAction("Index");
            }

            if (model.StatusId.HasValue)
            {
                await context.SellOrderStatuses.AddAsync(new SellOrderStatus
                {
                    SellOrderId = model.SellOrderId.Value,
                    StatusId = model.StatusId.Value,
                    SetDate = DateTime.Now
                });
                await context.SaveChangesAsync();
            }

            return RedirectToAction("Inspect", new { id = model.SellOrderId });
        }

        public class ReportModel
        {
            public DateTime? From { get; set; }

            public DateTime? To { get; set; }

            public IEnumerable<SellOrderStatus> Statuses { get; set; }
        }

        public async Task<ViewResult> Report([FromQuery] ReportModel model)
        {
            var query = context.SellOrderStatuses.AsNoTracking()
                .Include(e => e.Status)
                .Include(e => e.SellOrder)
                .AsQueryable();

            if (model.From.HasValue && model.To.HasValue)
            {
                query = query.Where(e => e.SetDate >= model.From && e.SetDate <= model.To);
            }
            else if (model.From.HasValue && !model.To.HasValue)
            {
                query = query.Where(e => e.SetDate >= model.From);
            }
            else if (!model.From.HasValue && model.To.HasValue)
            {
                query = query.Where(e => e.SetDate <= model.To);
            }

            model.Statuses = await query.OrderBy(e => e.SetDate).ToListAsync();
            return View("Report", model);
        }
    }
}
