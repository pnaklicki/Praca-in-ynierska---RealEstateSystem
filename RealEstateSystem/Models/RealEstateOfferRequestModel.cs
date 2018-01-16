using RealEstateSystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateSystem.Models
{
    public class RealEstateOfferRequestModel
    {
        private int pageNumber;
        private PropertyType propertyType;
        private Province province;
        private AdvertisementType advertisementType;
        private City city;

        public PropertyType PropertyType
        {
            get
            {
                return propertyType;
            }

            private set
            {
                propertyType = value;
            }
        }

        public AdvertisementType AdvertisementType
        {
            get
            {
                return advertisementType;
            }

            private set
            {
                advertisementType = value;
            }
        }

        public int PageNumber
        {
            get
            {
                return pageNumber;
            }

            set
            {
                pageNumber = value;
            }
        }

        public Province Province
        {
            get
            {
                return province;
            }

            set
            {
                province = value;
            }
        }

        public City City
        {
            get
            {
                return city;
            }

            set
            {
                city = value;
            }
        }

        public RealEstateOfferRequestModel(Province a_province,City a_city, PropertyType a_propertyType, AdvertisementType a_advertisementType)
        {
            //Always start from first page
            PageNumber = 1;
            Province = a_province;
            PropertyType = a_propertyType;
            AdvertisementType = a_advertisementType;
            City = a_city;
        }
    }
}