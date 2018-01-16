using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateSystem.Enums
{
    public class PropertyType
    {
        public static readonly PropertyType ALL = new PropertyType("all");
        public static readonly PropertyType HOUSE = new PropertyType("house");
        public static readonly PropertyType LAND = new PropertyType("land");
        public static readonly PropertyType GARAGE = new PropertyType("garage");
        public static readonly PropertyType PREMISE = new PropertyType("premise");
        public static readonly PropertyType FLAT = new PropertyType("flat");
        public static readonly PropertyType ROOM = new PropertyType("room");
        public static readonly PropertyType ELSE = new PropertyType("else");
        public static readonly PropertyType INVALID = new PropertyType("invalid");

        public static IEnumerable<PropertyType> Values
        {
            get
            {
                yield return HOUSE;
                yield return LAND;
                yield return GARAGE;
                yield return PREMISE;
                yield return FLAT;
                yield return ROOM;
                yield return ELSE;
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

        private PropertyType(string propName)
        {
            name = propName;
        }

        public static PropertyType FromString(string propType)
        {
            propType = propType.ToLower();
            if (propType == "all")
            {
                return ALL;
            }
            foreach (PropertyType type in Values)
            {
                if(type.name.ToLower() == propType)
                {
                    return type;
                }
            }
            return INVALID;
        }
    }
}