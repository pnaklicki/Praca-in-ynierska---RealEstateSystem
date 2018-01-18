using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace RealEstateSystem.Enums
{
    public class City
    {
        public static readonly City ALL = new City("all");
        public static readonly City INVALID = new City("invalid");

        public static readonly List<City> CITIES;

        static City()
        {
            CITIES = new List<City>();
            foreach(TerytUslugaWs1.MiejscowoscPelna city in DataManagement.getCities())
            {
                CITIES.Add(new City(city));
            }
        }

        private string name;
        private TerytUslugaWs1.MiejscowoscPelna cityDetails;
        public string Name
        {
            get
            {
                return name;
            }
        }

        public TerytUslugaWs1.MiejscowoscPelna CityDetails
        {
            get
            {
                return cityDetails;
            }
        }
        private City(string advName)
        {
            name = advName;
        }

        private City(TerytUslugaWs1.MiejscowoscPelna city)
        {
            cityDetails = city;
        }

        public static City GetCity(string province, string area, string city)
        {
            City result = null;
            province = ConvertToUniversal(province).Trim();
            area = ConvertToUniversal(area).Trim();
            city = ConvertToUniversal(city).Trim();
            try
            {
                var filteredCities = CITIES.Where(m =>
                   ConvertToUniversal(m.cityDetails.Nazwa.ToLower()).Equals(city.ToLower())).ToList();
                if (filteredCities.Count > 1)
                {
                    filteredCities = filteredCities.Where(m =>
                        ConvertToUniversal(m.cityDetails.Wojewodztwo.ToLower()).Equals(province.ToLower())).ToList();
                    if (filteredCities.Count > 1)
                    {
                        return filteredCities.First();
                    }
                }
                result = filteredCities.Single();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return result;
        }

        private static string ConvertToUniversal(string text)
        {
            string result = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(text));
            return result;
        }

        public static City FromString(string city)
        {
            
            city = city.ToLower();
            if (city == "all")
            {
                return ALL;
            }
            foreach (City type in CITIES)
            {
                if (ConvertToUniversal(type.cityDetails.Nazwa.Replace("-", " ").ToLower()) == ConvertToUniversal(city.Replace("-", " ")))
                {
                    return type;
                }
            }
            return INVALID;
        }
    }
}