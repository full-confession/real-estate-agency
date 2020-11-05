using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pepega.Models;

namespace Pepega.Controllers
{
    public class SoldController : Controller
    {
        private readonly Context context;

        public SoldController(Context context)
        {
            this.context = context;
        }


        public class AddModel
        {
            public int SellOrderId { get; set; }

            [Required(ErrorMessage = "Обязательное поле")]
            [DisplayName("Цена")]
            public decimal? FinalPrice { get; set; }

            public SellOrder SellOrder { get; set; }
        }


        public class IndexModel
        {
            [DataType(DataType.Date)]
            public DateTime? From { get; set; }
            [DataType(DataType.Date)]
            public DateTime? To { get; set; }
            public IEnumerable<Sold> Solds { get; set; }
        }


        public async Task<IActionResult> Index([FromQuery] IndexModel fromModel)
        {
            var query = context.Solds.AsNoTracking()
                .OrderBy(e => e.Date).AsQueryable();

            if (fromModel.From.HasValue && fromModel.To.HasValue)
            {
                query = query.Where(e => e.Date >= fromModel.From.Value && e.Date <= fromModel.To.Value);
            }
            else if (fromModel.From.HasValue)
            {
                query = query.Where(e => e.Date >= fromModel.From.Value);
            }
            else if (fromModel.To.HasValue)
            {
                query = query.Where(e => e.Date <= fromModel.To.Value);
            }

            fromModel.Solds = await query
                .Include(e => e.SellOrder).ToListAsync();

            return View(fromModel);
        }


        public class ReportModel
        {
            public decimal Income { get; set; }
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }
            public IEnumerable<Sold> Solds { get; set; }
        }

        public async Task<IActionResult> Report([FromQuery] IndexModel fromModel)
        {
            var query = context.Solds.AsNoTracking()
                .OrderBy(e => e.Date).AsQueryable();

            if (fromModel.From.HasValue && fromModel.To.HasValue)
            {
                query = query.Where(e => e.Date >= fromModel.From.Value && e.Date <= fromModel.To.Value);
            }
            else if (fromModel.From.HasValue)
            {
                query = query.Where(e => e.Date >= fromModel.From.Value);
            }
            else if (fromModel.To.HasValue)
            {
                query = query.Where(e => e.Date <= fromModel.To.Value);
            }

            var model = new ReportModel
            {
                Income = await query.SumAsync(e => e.Income),
                From = fromModel.From,
                To = fromModel.To,
                Solds = await query
                    .Include(e => e.SellOrder).ToListAsync()
            };

            return View(model);
        }

        public class InspectModel
        {
            public Sold Sold { get; set; }

            public int SellOrderId { get; set; }


            [Required(ErrorMessage = "Обязательное поле")]
            [DisplayName("# Клиент")]
            public int? ClientId { get; set; }

            public IEnumerable<Buyer> Buyers { get; set; }
        }

        public async Task<IActionResult> Inspect(int id)
        {
            var entity = await context.Solds.AsNoTracking()
                .Include(e => e.SellOrder)
                .SingleOrDefaultAsync(e => e.SellOrderId == id);

            if (entity == null)
            {
                return NotFound();
            }

            return View(new InspectModel
            {
                Sold = entity,
                Buyers = await context.Buyers.AsNoTracking()
                    .Where(e => e.SellOrderId == id)
                    .OrderBy(e => e.ClientId)
                    .Include(e => e.Client).ToListAsync(),
                SellOrderId = id
            });
        }


        public async Task<IActionResult> Add(int sellOrderId)
        {
            var entity = await context.SellOrders.AsNoTracking()
                .SingleOrDefaultAsync(e => e.SellOrderId == sellOrderId);

            if (entity == null)
            {
                return BadRequest();
            }


            return View(new AddModel
            {
                SellOrderId = sellOrderId,
                SellOrder = entity
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] AddModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = await context.SellOrders.AsNoTracking()
                .Include(e => e.Manager)
                .SingleOrDefaultAsync(e => e.SellOrderId == model.SellOrderId);

            if (entity == null)
            {
                return BadRequest();
            }


            var sold = new Sold
            {
                SellOrderId = entity.SellOrderId,
                Date = DateTime.Now,
                FinalPrice = model.FinalPrice.Value,
                Income = entity.AgencyCharge + model.FinalPrice.Value * entity.AgencyPercent / 100M,
                IncomeManager = model.FinalPrice.Value * entity.Manager.OrderPercent / 100M
            };

            await context.SellOrderStatuses.AddAsync(new SellOrderStatus
            {
                SellOrderId = entity.SellOrderId,
                StatusId = (await context.Statuses.Where(e => e.Name == "Завершен").FirstAsync()).StatusId,
                SetDate = DateTime.Now
            });

            await context.Solds.AddAsync(sold);
            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = model.SellOrderId });
        }



        [HttpPost]
        public async Task<IActionResult> AddBuyer([FromForm] InspectModel model)
        {

            var entity = await context.SellOrders.AsNoTracking()
                .SingleOrDefaultAsync(e => e.SellOrderId == model.SellOrderId);

            if (entity == null)
            {
                return RedirectToAction("Inspect", new { id = model.SellOrderId });
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Inspect", new { id = model.SellOrderId });
            }


            var any = await context.Buyers.Where(e => e.SellOrderId == model.SellOrderId && e.ClientId == model.ClientId.Value)
                .AnyAsync();

            if (any)
            {
                return RedirectToAction("Inspect", new { id = model.SellOrderId });
            }

            await context.Buyers.AddAsync(new Buyer
            {
                ClientId = model.ClientId.Value,
                SellOrderId = model.SellOrderId
            });

            await context.SaveChangesAsync();


            model.Sold = await context.Solds.AsNoTracking()
                .Include(e => e.SellOrder)
                .SingleOrDefaultAsync(e => e.SellOrderId == model.SellOrderId);

            model.Buyers = await context.Buyers.AsNoTracking()
                .Where(e => e.SellOrderId == model.SellOrderId)
                .Include(e => e.Client).ToListAsync();

            return RedirectToAction("Inspect", new { id = model.SellOrderId });
        }
    }
}
