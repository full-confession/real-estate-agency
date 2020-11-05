using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Pepega.Models
{
    public class City
    {
        public int CityId { get; set; }

        public string Name { get; set; }


        public List<District> Districts { get; set; }
    }

    public class District
    {
        public int DistrictId { get; set; }

        public int CityId { get; set; }

        public string Name { get; set; }

        [DisplayName("Город")]
        public City City { get; set; }

        public List<Street> Streets { get; set; }
    }

    public class Street
    {
        public int StreetId { get; set; }
        public int DistrictId { get; set; }

        public string Name { get; set; }

        [DisplayName("Район")]
        public District District { get; set; }
    }
}
