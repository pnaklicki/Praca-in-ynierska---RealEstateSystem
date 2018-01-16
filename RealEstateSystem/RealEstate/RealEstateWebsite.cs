using RealEstateSystem.Enums;
using RealEstateSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstateSystem.RealEstate
{
    public abstract class RealEstateWebsite
    {

        //Constant values of variables in websites requests
        protected const string pageNumber="pageNum";
        protected const string province = "province";
        protected const string propertyType = "propType";
        protected const string advertisementType = "advType";
        protected const string websiteAdressBegin = "http://";

        private string adress;
        private string name;
        private string requestLinkSchema;

        private WebClient client;
        private Dictionary<PropertyType, string> propertyTypeIdMap;
        private Dictionary<Province, string> provinceIdMap;
        private Dictionary<AdvertisementType, string> advertisementTypeIdMap;

        public Dictionary<PropertyType, string> PropertyTypeIdMap
        {
            get
            {
                return propertyTypeIdMap;
            }
            private set
            {
                propertyTypeIdMap = value;
            }
        }

        public Dictionary<Province, string> ProvinceIdMap
        {
            get
            {
                return provinceIdMap;
            }
            private set
            {
                provinceIdMap = value;
            }
        }

        public Dictionary<AdvertisementType, string> AdvertisementTypeIdMap
        {
            get
            {
                return advertisementTypeIdMap;
            }
            private set
            {
                advertisementTypeIdMap = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            private set
            {
                name = value;
            }
        }

        public string Adress
        {
            get
            {
                return adress;
            }
            private set
            {
                adress = value;
            }
        }

        public string RequestLinkSchema
        {
            get
            {
                return requestLinkSchema;
            }
            set
            {
                requestLinkSchema = value;
            }
        }

        public RealEstateWebsite(string a_adress, string a_requestLinkSchema, string a_name)
        {
            client = new WebClient();
            client.Encoding = Encoding.UTF8;
            Adress = a_adress;
            Name = a_name;
            requestLinkSchema = a_requestLinkSchema;
            PropertyTypeIdMap = new Dictionary<PropertyType, string>();
            AdvertisementTypeIdMap = new Dictionary<AdvertisementType, string>();
            ProvinceIdMap = new Dictionary<Province, string>();
        }

        protected abstract int ParsePrice(string priceText);

        protected string ReadWebsite(string a_adress)
        {
            string website = "";

            try
            {
                website = client.DownloadString(a_adress);
            }
            catch (Exception ex)
            {
                website = null;
                Debug.WriteLine(ex);
            }
            return website;
        }

        protected abstract string FormatRequestString(RealEstateOfferRequestModel a_offerModel);

        public abstract void GetRealEstateOffers();
    }
}
