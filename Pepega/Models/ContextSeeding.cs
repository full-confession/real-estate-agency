using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Pepega.Models
{
    public static class ContextSeeding
    {


        private static List<string> cities = new List<string>
        {
            "г. Новосибирск", "г. Томск", "г. Красноярск"
        };

        private static List<string> districts = new List<string>
        {
            "Советский р-он", "Кировский р-он", "Ленинский р-он", "Октябрьский р-он", "Центральный р-он"
        };

        private static List<string> streets = new List<string>
        {
            "пр-т Ленина", "пр-т Ленина", "ул. Каменская", "ул. Гоголя", "ул. Горького"
        };

        private static List<string> names = new List<string>
        {
            "Карим", "Эрик", "Макар", "Тарас", "Егор", "Нестор", "Остап", "Руслан", "Святослав", "Святослав"
        };

        private static List<string> surnames = new List<string>
        {
            "Носков", "Кононов", "Яровой", "Фёдоров", "Ефремов", "Рожков", "Доронин", "Соловьёв", "Павлив", "Гурьев"
        };

        private static List<string> patronymics = new List<string>
        {
            "Борисович", "Михайлович", "Станиславович", "Викторович", "Сергеевич", "Фёдорович", "Платонович", "Станиславович", "Сергеевич", "Максимович"
        };

        private static List<int> passportSeries = new List<int>
        {
            1111, 2222, 3333, 4444, 5555
        };

        private static List<int> phoneCodes = new List<int>
        {
            111, 222, 333, 444, 555
        };


        public static void Seed(Context context)
        {
            const int NUM_PROPERTY = 1000;
            const int NUM_ORDERS = 500;
            const int NUM_CLIENTS = 750;
            const int NUM_MANAGERS_PER_CITY = 5;

            context.Database.EnsureCreated();

            if (context.Properties.Any())
            {
                return;
            }

            var propertyTypes = new List<PropertyType>
            {
                context.PropertyTypes.Add(new PropertyType {Name = "Квартира"}).Entity,
                context.PropertyTypes.Add(new PropertyType {Name = "Дом"}).Entity,
                context.PropertyTypes.Add(new PropertyType {Name = "Участок"}).Entity
            };

            var rand = new Random();

            foreach (var cityName in cities)
            {
                var city = context.Cities.Add(new City { Name = cityName }).Entity;

                foreach (var districtName in districts)
                {
                    var district = context.Districts.Add(new District { Name = districtName, City = city }).Entity;

                    foreach (var streetName in streets)
                    {
                        var street = context.Streets.Add(new Street { Name = streetName, District = district }).Entity;
                    }
                }


                for (var i = 0; i < NUM_MANAGERS_PER_CITY; ++i)
                {
                    var manager = new Manager
                    {
                        City = city,
                        BirthDate = new DateTime(1980, 1, 1),
                        FirstName = names.GetRandom(rand),
                        LastName = surnames.GetRandom(rand),
                        JoinDate = new DateTime(2000, 1, 1),
                        MiddleName = patronymics.GetRandom(rand),
                        OrderPercent = 1.5M,
                        PassportNumber = $"{passportSeries.GetRandom(rand)}-{rand.Next(0, 1000000):D6}",
                        PhoneNumber = $"7{phoneCodes.GetRandom(rand)}{rand.Next(0, 10000000):D7}"
                    };

                    context.Managers.Add(manager);
                }
            }

            context.SaveChanges();





            var steets = context.Streets.AsNoTracking().ToList();

            var dateA = new DateTime(2016, 1, 1);
            var dateB = new DateTime(2018, 1, 1);
            var dateDelta = dateB - dateA;
            for (var i = 0; i < NUM_PROPERTY; ++i)
            {
                var property = context.Properties.Add(new Property
                {
                    PropertyType = propertyTypes.GetRandom(rand),
                    CreationDate = dateA + new TimeSpan(rand.Next(dateDelta.Days), 0, 0, 0),
                    Street = steets.GetRandom(rand),
                    HouseNumber = rand.Next(1, 100),
                    Number = $"{rand.Next(1, 100):D2}:{rand.Next(1, 100):D2}:{rand.Next(1, 1000000):D6}:{rand.Next(1000, 100000)}"
                    //Building = rand.Next(1, 10) > 8 ? rand.Next(1, 5).ToString() : null,
                    //Housing = rand.Next(1, 10) > 8 ? rand.Next(1, 8).ToString() : null,
                    //ApartmentNumber = 
                    //City = cities.GetRandom(rand),
                    //Disctrict = districts.GetRandom(rand),
                    //Street = streets.GetRandom(rand),
                    //Building = rand.Next(1, 100).ToString()
                }).Entity;



                if (property.PropertyType.Name == "Квартира" || property.PropertyType.Name == "Дом")
                {
                    property.Building = rand.Next(1, 10) > 8 ? new int?(rand.Next(1, 5)) : null;
                    property.Housing = rand.Next(1, 10) > 8 ? new int?(rand.Next(1, 8)) : null;

                    if (property.PropertyType.Name == "Квартира")
                    {
                        property.ApartmentNumber = rand.Next(1, 100);
                    }

                    var buildingDesc = context.BuildingDescriptions.Add(new BuildingDescription
                    {
                        Property = property,
                        BuildYear = rand.Next(1990, 2018),
                        NumberOfFloors = rand.Next(1, 30),
                        Parking = rand.Next(0, 5) == 0,
                        Elevator = rand.Next(0, 2) == 0
                    }).Entity;

                    var apartmentDesc = context.ApartmentDescriptions.Add(new ApartmentDescription
                    {
                        Property = property,
                        Area = rand.Next(50, 200),
                        NumberOfRooms = rand.Next(1, 8),
                        Balcony = rand.Next(0, 6) > 2
                    }).Entity;

                    apartmentDesc.Floor = buildingDesc.NumberOfFloors.HasValue ?
                        rand.Next(1, buildingDesc.NumberOfFloors.Value + 1) :
                        rand.Next(1, 30);
                }

                if (property.PropertyType.Name == "Дом" || property.PropertyType.Name == "Участок")
                {
                    var areaDesciption = context.AreaDescriptions.Add(new AreaDescription
                    {
                        Property = property,
                        Area = rand.Next(200, 400),
                        Electricity = rand.Next(0, 10) < 8,
                        FertileSoil = rand.Next(0, 10) < 6,
                        Water = rand.Next(0, 100) < 95
                    });
                }
            }

            context.SaveChanges();



            var birthA = new DateTime(1900, 1, 1);
            var birthDelta = new DateTime(2000, 1, 1) - birthA;
            for (var i = 0; i < NUM_CLIENTS; ++i)
            {
                var client = context.Clients.Add(new Client
                {
                    FirstName = names.GetRandom(rand),
                    LastName = surnames.GetRandom(rand),
                    MiddleName = patronymics.GetRandom(rand),
                    BirthDate = birthA + new TimeSpan(rand.Next(birthDelta.Days), 0, 0, 0, 0),
                    RegistrationDate = dateA + new TimeSpan(rand.Next(dateDelta.Days), 0, 0, 0),
                    PassportNumber = $"{passportSeries.GetRandom(rand)}-{rand.Next(0, 1000000):D6}",
                    PhoneNumber = $"7{phoneCodes.GetRandom(rand)}{rand.Next(0, 10000000):D7}"
                });
            }

            context.SaveChanges();

            var createdStatus = context.Statuses.Add(new Status
            {
                Name = "Создан",
                Style = "info"
            }).Entity;

            var pendingStatus = context.Statuses.Add(new Status
            {
                Name = "На проверке",
                Style = "warning"
            }).Entity;

            var activeStatus = context.Statuses.Add(new Status
            {
                Name = "Активен",
                Style = "success"
            }).Entity;

            var finishedStatus = context.Statuses.Add(new Status
            {
                Name = "Завершен",
                Style = "secondary"
            }).Entity;

            var canceledStatus = context.Statuses.Add(new Status
            {
                Name = "Прекращен",
                Style = "danger"
            }).Entity;



            var propertyIds = Enumerable.Range(1, NUM_PROPERTY).OrderBy(r => rand.Next()).Take(NUM_ORDERS).ToList();
            //var clientsIds = Enumerable.Range(1, 100).OrderBy(r => rand.Next()).Take(50).ToList();

            dateA = new DateTime(2018, 1, 1);
            dateB = new DateTime(2018, 2, 1);
            dateDelta = dateB - dateA;

            for (var i = 0; i < propertyIds.Count; ++i)
            {

                var property = context.Properties
                    .Find(propertyIds[i]);

                context.Entry(property).Reference(e => e.Street).Load();
                context.Entry(property.Street).Reference(e => e.District).Load();

                var cityId = property.Street.District.CityId;

                var managers = context.Managers.AsNoTracking().Where(e => e.CityId == cityId).ToList();

                var order = new SellOrder
                {
                    PropertyId = propertyIds[i],
                    ManagerId = managers.GetRandom(rand).ManagerId,
                    CreationDate = dateA + new TimeSpan(rand.Next(dateDelta.Days), 0, 0, 0),
                    AgencyCharge = rand.Next(10, 30),
                    AgencyPercent = rand.Next(4, 8),
                    Price = rand.Next(0, 10000)
                };

                var letters = new List<char> { 'А', 'Б', 'В', 'Н' };

                order.Number = $"{letters.GetRandom(rand)}{order.CreationDate.Year}-{letters.GetRandom(rand)}{order.CreationDate.Month}{order.CreationDate.Day}-{rand.Next(1000, 10000)}";

                var sellOrder = context.SellOrders.Add(order).Entity;

                context.SaveChanges();


                var created = context.SellOrderStatuses.Add(new SellOrderStatus
                {
                    SellOrder = sellOrder,
                    Status = createdStatus,
                    SetDate = sellOrder.CreationDate
                }).Entity;


                var pending = context.SellOrderStatuses.Add(new SellOrderStatus
                {
                    SellOrder = sellOrder,
                    Status = pendingStatus,
                    SetDate = created.SetDate + new TimeSpan(0, rand.Next(12), 0, 0)
                }).Entity;



                var numberOfSellers = rand.Next(1, 5);

                for (int j = 0; j < numberOfSellers; ++j)
                {

                    var ownership = new Ownership
                    {
                        IssueDate = dateA - new TimeSpan(rand.Next(100), 0, 0, 0),
                        Number = $"{rand.Next(1000, 10000)}-{rand.Next(100000, 1000000)}"
                    };

                    var seller = new Seller
                    {
                        ClientId = rand.Next(1, NUM_CLIENTS),
                        SellOrder = sellOrder,
                        Ownership = ownership
                    };

                    context.Sellers.Add(seller);
                }

                context.SaveChanges();

                var endStatus = rand.Next(0, 100);


                if (endStatus < 90)
                {
                    var active = context.SellOrderStatuses.Add(new SellOrderStatus
                    {
                        SellOrder = sellOrder,
                        Status = activeStatus,
                        SetDate = pending.SetDate + new TimeSpan(rand.Next(5, 60), 0, 0)
                    }).Entity;

                    if (endStatus > 70)
                    {


                        var endDate = DateTime.Now;
                        var delta = (endDate - active.SetDate.AddDays(3)).Days;

                        if (delta >= 3)
                        {

                            var finished = context.SellOrderStatuses.Add(new SellOrderStatus
                            {
                                SellOrder = sellOrder,
                                Status = finishedStatus,
                                SetDate = active.SetDate + new TimeSpan(rand.Next(3, delta), 0, 0, 0)
                            }).Entity;



                            var sold = new Sold
                            {
                                Date = finished.SetDate,
                                FinalPrice = order.Price * (rand.Next(80, 120) / 100M),
                                SellOrder = sellOrder
                            };


                            context.Entry(sellOrder).Reference(e => e.Manager).Load();

                            sold.Income = sellOrder.AgencyCharge + sold.FinalPrice * sellOrder.AgencyPercent / 100M;
                            sold.IncomeManager = sold.FinalPrice * sellOrder.Manager.OrderPercent / 100M;

                            context.Solds.Add(sold);

                            var numberOfBuyers = rand.Next(1, 5);

                            for (var j = 0; j < numberOfBuyers; ++j)
                            {
                                var buyer = new Buyer
                                {
                                    ClientId = rand.Next(1, NUM_CLIENTS),
                                    SellOrder = sellOrder
                                };

                                context.Buyers.Add(buyer);
                            }
                        }

                    }
                    else if (endStatus > 65)
                    {
                        context.SellOrderStatuses.Add(new SellOrderStatus
                        {
                            SellOrder = sellOrder,
                            Status = canceledStatus,
                            SetDate = active.SetDate + new TimeSpan(rand.Next(10, 120), 0, 0)
                        });
                    }
                }
                else
                {
                    context.SellOrderStatuses.Add(new SellOrderStatus
                    {
                        SellOrder = sellOrder,
                        Status = canceledStatus,
                        SetDate = created.SetDate + new TimeSpan(rand.Next(10, 120), 0, 0)
                    });
                }

                context.SaveChanges();
            }

            context.SaveChanges();
        }


        public static T GetRandom<T>(this List<T> list, Random random)
        {
            return list[random.Next(list.Count)];
        }
    }
}
