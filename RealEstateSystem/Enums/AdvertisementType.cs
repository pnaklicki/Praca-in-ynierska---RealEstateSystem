using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateSystem.Enums
{
    public class AdvertisementType
    {
        public static readonly AdvertisementType ALL = new AdvertisementType("all");
        public static readonly AdvertisementType SALE = new AdvertisementType("sale");
        public static readonly AdvertisementType RENT = new AdvertisementType("rent");
        public static readonly AdvertisementType INVALID = new AdvertisementType("invalid");

        public static IEnumerable<AdvertisementType> Values
        {
            get
            {
                yield return SALE;
                yield return ALL;
                yield return RENT;
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

        private AdvertisementType(string advName)
        {
            name = advName;
        }

        public static AdvertisementType FromString(string advType)
        {
            advType = advType.ToLower();
            if (advType == "all")
            {
                return ALL;
            }
            foreach (AdvertisementType type in Values)
            {
                if (type.name.ToLower() == advType)
                {
                    return type;
                }
            }
            return INVALID;
        }
    }
}