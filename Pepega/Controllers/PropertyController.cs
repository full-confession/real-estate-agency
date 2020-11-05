using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pepega.Models;

namespace Pepega.Controllers
{

    public class PaginationModel
    {
        public int PageCount { get; set; }

        public int TotalEntries { get; set; }

        public int Selected { get; set; }
    }

    public class IndexModel
    {
        public IEnumerable<Property> Properties { get; set; }
        public PaginationModel Pagination { get; set; }
        public PropertyFilterModel FilterModel { get; set; }
        public SelectList PropertyTypeList { get; set; }
        public SelectList BoolTypeList { get; set; }

        public SelectList CityList { get; set; }
        public SelectList DistrictList { get; set; }
        public SelectList StreetList { get; set; }
    }

    public class PropertyInspectModel
    {
        public Property Property { get; set; }
    }

    public class PropertyEditModel
    {
        public Property Property { get; set; }
        public SelectList PropertyTypeList { get; set; }
        public SelectList CityList { get; set; }
        public SelectList DistrictList { get; set; }
        public SelectList StreetList { get; set; }
    }


    public class PropertyController : Controller
    {

        private readonly Context context;

        public PropertyController(Context context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index([FromQuery] PropertyFilterModel filterModel)
        {
            var query = context.Properties.AsNoTracking().OrderBy(e => e.PropertyId).AsQueryable();
            query = query.ApplyFilters(filterModel, context);

            var count = await query.CountAsync();
            var result = await query
                .Skip((filterModel.PageIndex - 1) * filterModel.PageSize)
                .Take(filterModel.PageSize)
                .Include(e => e.BuildingDescription)
                .Include(e => e.ApartmentDescription)
                .Include(e => e.AreaDescription)
                .Include(e => e.PropertyType)
                .Include(e => e.Street).ThenInclude(e => e.District).ThenInclude(e => e.City)
                .ToListAsync();


            var model = new IndexModel
            {
                Properties = result.AsEnumerable(),
                FilterModel = filterModel,
                Pagination = new PaginationModel
                {
                    PageCount = (int)Math.Ceiling(count / (double)filterModel.PageSize),
                    Selected = count,
                    TotalEntries = await context.Properties.CountAsync()
                },
                PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name"),
                BoolTypeList = new SelectList(new[]
                {
                    new SelectListItem {Value = "true", Text = "Есть"},
                    new SelectListItem {Value = "false", Text = "Нет"},
                }, "Value", "Text"),
                CityList = new SelectList(
                    await context.Cities.AsNoTracking().ToListAsync(),
                    "CityId", "Name")
            };

            if (filterModel.CityId.HasValue)
            {
                model.DistrictList = new SelectList(
                    await context.Districts.AsNoTracking().Where(e => e.CityId == filterModel.CityId.Value).ToListAsync(),
                    "DistrictId", "Name");

                if (filterModel.DistrictId.HasValue)
                {
                    model.StreetList = new SelectList(
                        await context.Streets.AsNoTracking().Where(e => e.DistrictId == filterModel.DistrictId.Value).ToListAsync(),
                        "StreetId", "Name");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Inspect(int id)
        {
            var propery = await context.Properties.AsNoTracking()
                .Include(e => e.PropertyType)
                .Include(e => e.ApartmentDescription)
                .Include(e => e.BuildingDescription)
                .Include(e => e.AreaDescription)
                .Include(e => e.Street).ThenInclude(e => e.District).ThenInclude(e => e.City)
                .Include(e => e.SellOrders)
                .SingleOrDefaultAsync(e => e.PropertyId == id);

            if (propery == null)
            {
                return NotFound();
            }

            return View(new PropertyInspectModel
            {
                Property = propery
            });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var property = await context.Properties.AsNoTracking()
                .Include(e => e.PropertyType)
                .Include(e => e.Street).ThenInclude(e => e.District).ThenInclude(e => e.City)
                .SingleOrDefaultAsync(e => e.PropertyId == id);

            if (property == null)
            {
                return NotFound();
            }

            var model = new PropertyEditModel();
            await FillEditModel(model, property);

            return View(model);
        }

        private async Task FillEditModel(PropertyEditModel model, Property property)
        {
            model.Property = property;
            model.PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name");
            model.CityList = new SelectList(await context.Cities.AsNoTracking().ToListAsync(),
                "CityId", "Name", property.Street.District.CityId);
            model.DistrictList = new SelectList(
                await context.Districts.AsNoTracking().Where(e => e.CityId == property.Street.District.CityId)
                    .ToListAsync(),
                "DistrictId", "Name", property.Street.DistrictId);
            model.StreetList = new SelectList(
                await context.Streets.AsNoTracking().Where(e => e.DistrictId == property.Street.DistrictId)
                    .ToListAsync(),
                "StreetId", "Name", property.StreetId);
        }

        public async Task<IActionResult> EditAreaDescription(int id)
        {
            var areaDescription = await context.AreaDescriptions.AsNoTracking()
                .SingleOrDefaultAsync(e => e.PropertyId == id);

            if (areaDescription == null)
            {
                return BadRequest();
            }


            return View(areaDescription);
        }

        [HttpPost]
        public async Task<IActionResult> EditAreaDescription([FromForm] AreaDescription model)
        {
            var areaDescription = await context.AreaDescriptions
                .SingleOrDefaultAsync(e => e.PropertyId == model.PropertyId);

            if (areaDescription == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View("EditAreaDescription", model);
            }

            areaDescription.Area = model.Area;
            areaDescription.Electricity = model.Electricity;
            areaDescription.FertileSoil = model.FertileSoil;
            areaDescription.Water = model.Water;

            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = areaDescription.PropertyId });
        }

        public async Task<IActionResult> DeleteAreaDescription(int id)
        {
            var areaDescription = await context.AreaDescriptions
                .SingleOrDefaultAsync(e => e.PropertyId == id);

            if (areaDescription == null)
            {
                return BadRequest();
            }

            context.AreaDescriptions.Remove(areaDescription);
            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = id });
        }

        public IActionResult AddAreaDescription(int id)
        {
            return View(new AreaDescription { PropertyId = id });
        }

        [HttpPost]
        public async Task<IActionResult> AddAreaDescription([FromForm] AreaDescription model)
        {
            var property = await context.Properties.AnyAsync(e => e.PropertyId == model.PropertyId);
            if (!property || await context.AreaDescriptions.SingleOrDefaultAsync(e => e.PropertyId == model.PropertyId) != null)
            {
                return BadRequest();
            }

            await context.AreaDescriptions.AddAsync(model);
            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = model.PropertyId });
        }


        public async Task<IActionResult> EditBuildingDescription(int id)
        {
            var buildingDescription = await context.BuildingDescriptions.AsNoTracking()
                .SingleOrDefaultAsync(e => e.PropertyId == id);

            if (buildingDescription == null)
            {
                return BadRequest();
            }


            return View(buildingDescription);
        }

        [HttpPost]
        public async Task<IActionResult> EditBuildingDescription([FromForm] BuildingDescription model)
        {
            var buildingDescription = await context.BuildingDescriptions
                .SingleOrDefaultAsync(e => e.PropertyId == model.PropertyId);

            if (buildingDescription == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View("EditBuildingDescription", model);
            }

            buildingDescription.BuildYear = model.BuildYear;
            buildingDescription.Elevator = model.Elevator;
            buildingDescription.NumberOfFloors = model.NumberOfFloors;
            buildingDescription.Parking = model.Parking;

            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = buildingDescription.PropertyId });
        }

        public async Task<IActionResult> DeleteBuildingDescription(int id)
        {
            var buildingDescription = await context.BuildingDescriptions
                .SingleOrDefaultAsync(e => e.PropertyId == id);

            if (buildingDescription == null)
            {
                return BadRequest();
            }

            context.BuildingDescriptions.Remove(buildingDescription);
            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = id });
        }

        public IActionResult AddBuildingDescription(int id)
        {
            return View(new BuildingDescription { PropertyId = id });
        }

        [HttpPost]
        public async Task<IActionResult> AddBuildingDescription([FromForm] BuildingDescription model)
        {
            var property = await context.Properties.AnyAsync(e => e.PropertyId == model.PropertyId);
            if (!property || await context.BuildingDescriptions.SingleOrDefaultAsync(e => e.PropertyId == model.PropertyId) != null)
            {
                return BadRequest();
            }

            await context.BuildingDescriptions.AddAsync(model);
            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = model.PropertyId });
        }



        public async Task<IActionResult> EditApartmentDescription(int id)
        {
            var apartmentDescription = await context.ApartmentDescriptions.AsNoTracking()
                .SingleOrDefaultAsync(e => e.PropertyId == id);

            if (apartmentDescription == null)
            {
                return BadRequest();
            }

            return View(apartmentDescription);
        }

        [HttpPost]
        public async Task<IActionResult> EditApartmentDescription([FromForm] ApartmentDescription model)
        {
            var apartmentDescription = await context.ApartmentDescriptions
                .SingleOrDefaultAsync(e => e.PropertyId == model.PropertyId);

            if (apartmentDescription == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View("EditApartmentDescription", model);
            }

            apartmentDescription.Area = model.Area;
            apartmentDescription.Balcony = model.Balcony;
            apartmentDescription.Floor = model.Floor;
            apartmentDescription.NumberOfRooms = model.NumberOfRooms;

            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = apartmentDescription.PropertyId });
        }

        public async Task<IActionResult> DeleteApartmentDescription(int id)
        {
            var apartmentDescription = await context.ApartmentDescriptions
                .SingleOrDefaultAsync(e => e.PropertyId == id);

            if (apartmentDescription == null)
            {
                return BadRequest();
            }

            context.ApartmentDescriptions.Remove(apartmentDescription);
            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = id });
        }

        public IActionResult AddApartmentDescription(int id)
        {
            return View(new ApartmentDescription { PropertyId = id });
        }

        [HttpPost]
        public async Task<IActionResult> AddApartmentDescription([FromForm] ApartmentDescription model)
        {
            var property = await context.Properties.AnyAsync(e => e.PropertyId == model.PropertyId);
            if (!property || await context.ApartmentDescriptions.SingleOrDefaultAsync(e => e.PropertyId == model.PropertyId) != null)
            {
                return BadRequest();
            }

            await context.ApartmentDescriptions.AddAsync(model);
            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = model.PropertyId });
        }



        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] PropertyEditModel editModel)
        {
            var property = await context.Properties
                .Include(e => e.Street).ThenInclude(e => e.District).ThenInclude(e => e.City)
                .SingleOrDefaultAsync(e => e.PropertyId == editModel.Property.PropertyId);

            if (property == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                //editModel.PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name");

                await FillEditModel(editModel, property);
                return View("Edit", editModel);
            }


            property.StreetId = editModel.Property.StreetId;
            property.HouseNumber = editModel.Property.HouseNumber;
            property.Housing = editModel.Property.Housing;
            property.Building = editModel.Property.Building;
            property.PropertyTypeId = editModel.Property.PropertyTypeId;
            property.ApartmentNumber = editModel.Property.ApartmentNumber;


            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = editModel.Property.PropertyId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var property = await context.Properties
                .SingleOrDefaultAsync(e => e.PropertyId == id);

            if (property == null)
            {
                return BadRequest();
            }

            context.Properties.Remove(property);
            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Create()
        {
            return View(new PropertyEditModel
            {
                Property = new Property(),
                PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name"),
                CityList = new SelectList(await context.Cities.AsNoTracking().ToListAsync(), "CityId", "Name")
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PropertyEditModel editModel)
        {
            if (!ModelState.IsValid)
            {
                editModel.PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name");
                editModel.CityList =
                    new SelectList(await context.Cities.AsNoTracking().ToListAsync(), "CityId", "Name");


                return View("Create", editModel);
            }


            editModel.Property.CreationDate = DateTime.Now;

            await context.Properties.AddAsync(editModel.Property);
            await context.SaveChangesAsync();

            return RedirectToAction("Inspect", new { id = editModel.Property.PropertyId });
        }

        public IActionResult CreateAddAreaDescription([FromQuery] PropertyEditModel editModel)
        {
            ModelState.Clear();
            editModel.Property.AreaDescription = new AreaDescription();
            editModel.PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name");
            return View("Create", editModel);
        }

        public IActionResult CreateRemoveAreaDescription([FromQuery] PropertyEditModel editModel)
        {
            ModelState.Clear();
            editModel.Property.AreaDescription = null;
            editModel.PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name");

            return View("Create", editModel);
        }

        public IActionResult CreateAddBuildingDescription([FromQuery] PropertyEditModel editModel)
        {
            ModelState.Clear();
            editModel.Property.BuildingDescription = new BuildingDescription();
            editModel.PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name");
            return View("Create", editModel);
        }

        public IActionResult CreateRemoveBuildingDescription([FromQuery] PropertyEditModel editModel)
        {
            ModelState.Clear();
            editModel.Property.BuildingDescription = null;
            editModel.PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name");

            return View("Create", editModel);
        }

        public IActionResult CreateAddApartmentDescription([FromQuery] PropertyEditModel editModel)
        {
            ModelState.Clear();
            editModel.Property.ApartmentDescription = new ApartmentDescription();
            editModel.PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name");

            return View("Create", editModel);
        }

        public IActionResult CreateRemoveApartmentDescription([FromQuery] PropertyEditModel editModel)
        {
            ModelState.Clear();
            editModel.Property.ApartmentDescription = null;
            editModel.PropertyTypeList = new SelectList(context.PropertyTypes.ToList(), "PropertyTypeId", "Name");

            return View("Create", editModel);
        }


        [HttpGet]
        public async Task<IActionResult> Districts([FromQuery] int cityId)
        {
            return Json(await context.Districts.Where(e => e.CityId == cityId).AsNoTracking().ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Streets([FromQuery] int districtId)
        {
            return Json(await context.Streets.Where(e => e.DistrictId == districtId).AsNoTracking().ToListAsync());
        }
    }
}
