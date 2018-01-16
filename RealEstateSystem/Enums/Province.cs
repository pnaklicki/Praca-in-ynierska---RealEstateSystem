using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RealEstateSystem.Enums
{
    public class Province
    {
        public static readonly Province ALL = new Province("all");
        public static readonly Province INVALID = new Province("invalid");

        public static readonly List<Province> PROVINCES = new List<Province>();

        public static void Initialize()
        {
            foreach (TerytUslugaWs1.JednostkaTerytorialna province in DataManagement.Instance.getProvinces())
            {
                PROVINCES.Add(new Province(province.NAZWA.First().ToString().ToUpper() + province.NAZWA.Substring(1).ToLower()));
            }
        }

        private string name;

        public string Name
        {
            get
            {
                return name;
            }
        }

        private Province(string provName)
        {
            name = provName;
        }

        public static Province FromString(string provType)
        {
            provType = provType.ToLower();
            if (provType == "all")
            {
                return ALL;
            }
            foreach (Province type in PROVINCES)
            {
                if(Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(provType.Replace("-", " "))).Equals(Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(type.name.Replace("-", " ").ToLower()))))
                {
                    return type;
                }
            }
            return INVALID;
        }
    }
}