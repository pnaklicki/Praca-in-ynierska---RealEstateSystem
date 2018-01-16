using RealEstateSystem.Enums;
using RealEstateSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static RealEstateSystem.Models.RealEstateOfferModel;
using MoreLinq;
using PagedList;

namespace RealEstateSystem.Controllers
{
    public class MainController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.cities = DataManagement.Instance.getCities();
            ViewBag.provinces = DataManagement.Instance.getProvinces();
            return View();
        }

        [HttpPost]
        public ActionResult ReadCity(string keyword)
        {
            StringBuilder cityList = new StringBuilder();
            if (keyword.Length >= 2)
            {
                var matchingCities = new HashSet<TerytUslugaWs1.MiejscowoscPelna>(DataManagement.Instance.getCities().Where(m => m.Nazwa.ToLower().StartsWith(keyword.ToLower())).ToList());
                cityList.AppendLine("<ul>");
                matchingCities = new HashSet<TerytUslugaWs1.MiejscowoscPelna>(matchingCities.DistinctBy(m=>m.Nazwa).ToList());
                foreach (var city in matchingCities)
                {
                    cityList.AppendLine("<li onClick=\"selectCountry('" + city.Nazwa + "')\">" + city.Nazwa + "</li>");
                }
                cityList.AppendLine("</ul>");
            }
            return Content(cityList.ToString());
        }

        [HttpGet]
        public ActionResult Details(string offerId, string city = "all", string propType = "all", string province = "all", string advType = "all", int page = 1)
        {
            List<RealEstateOfferModel> copyList = WebsitesManagement.Instance.RealEstateOffers.ToList();
            ViewBag.cityFilter = city;
            ViewBag.propertyFilter = propType;
            ViewBag.provinceFilter = province;
            ViewBag.advertisementFilter = advType;
            ViewBag.Page = page;
            var offer = copyList.Single(m => m.Id.Equals(Guid.Parse(offerId)));
            return View(offer);
        }

        [HttpGet]
        public ActionResult Search(string city = "all", string propType = "all", string province = "all", string advType = "all", int page = 1)
        {
            List<RealEstateOfferModel> list = null;
            try
            {
                if(city == "")
                {
                    city = "all";
                }
                RealEstateOfferRequestModel requestModel = new RealEstateOfferRequestModel(Province.FromString(province), City.FromString(city), PropertyType.FromString(propType), AdvertisementType.FromString(advType));
                list = WebsitesManagement.Instance.GetFilteredOffers(requestModel);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            ViewBag.cityFilter = city;
            ViewBag.propertyFilter = propType;
            ViewBag.provinceFilter = province;
            ViewBag.advertisementFilter = advType;
            ViewBag.cities = DataManagement.Instance.getCities();
            ViewBag.provinces = DataManagement.Instance.getProvinces();

            return View(list.ToPagedList(page,10));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}