using HtmlAgilityPack;
using RealEstateSystem.Enums;
using RealEstateSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using static RealEstateSystem.Models.RealEstateOfferModel;

namespace RealEstateSystem.RealEstate
{
    public class OfertyDomWebsite : RealEstateWebsite
    {
        private const string pageNumInHtml = "rog3 sort2nsna";
        //XPaths
        private const string offerDescInHtml = ".//div[@id = 'pd']";
        private const string offersInHtml = ".//a[@class = 'anp']";
        private const string offerTitleInHtml = ".//span[@class = 'roboto']";
        private const string offerTypeInHtml = ".//span[@style = 'float: left; color: #333333; font-size: 15px;']";
        private const string offerPriceInHtml = ".//td[@style = 'font-weight: bold; font-size: 16px;']";
        private const string offerAreaInHtml = ".//a[@title = 'powiat']";
        private const string offerCityInHtml = ".//a[@title = 'miejscowość']";
        private const string offerImagesInHtml = ".//div[@class = 'showcase']";
        private const string offerImageInHtml = ".//div[@class = 'showcase-content']";
        private const string offerSingleImageInHtml = ".//img";

        public OfertyDomWebsite() : base("www.oferty-dom.pl", "/szukaj/{" +
            pageNumber +
            "}.html?sort=domyslne&pageLimit=40&propertyType={" +
            propertyType +
            "}&advertisementType={" +
            advertisementType +
            "}&city=&dzielnica=&powiat=" +
            "&province={" +
            province,
            "Oferty dom")
        {
            try
            {
                //Add property types
                PropertyTypeIdMap.Add(Enums.PropertyType.FLAT, "6");
                PropertyTypeIdMap.Add(Enums.PropertyType.LAND, "2");
                PropertyTypeIdMap.Add(Enums.PropertyType.GARAGE, "3");
                PropertyTypeIdMap.Add(Enums.PropertyType.PREMISE, "5");
                PropertyTypeIdMap.Add(Enums.PropertyType.ROOM, "11");
                PropertyTypeIdMap.Add(Enums.PropertyType.ELSE, "10");
                PropertyTypeIdMap.Add(Enums.PropertyType.HOUSE, "1");

                //Add provinces
                ProvinceIdMap.Add(Province.FromString("Dolnośląskie"), "1");
                ProvinceIdMap.Add(Province.FromString("Kujawsko-pomorskie"), "2");
                ProvinceIdMap.Add(Province.FromString("Lubelskie"), "3");
                ProvinceIdMap.Add(Province.FromString("Lubuskie"), "4");
                ProvinceIdMap.Add(Province.FromString("Łódzkie"), "5");
                ProvinceIdMap.Add(Province.FromString("Małopolskie"), "6");
                ProvinceIdMap.Add(Province.FromString("Mazowieckie"), "7");
                ProvinceIdMap.Add(Province.FromString("Opolskie"), "8");
                ProvinceIdMap.Add(Province.FromString("Podkarpackie"), "9");
                ProvinceIdMap.Add(Province.FromString("Podlaskie"), "10");
                ProvinceIdMap.Add(Province.FromString("Pomorskie"), "11");
                ProvinceIdMap.Add(Province.FromString("Śląskie"), "12");
                ProvinceIdMap.Add(Province.FromString("Świętokrzyskie"), "13");
                ProvinceIdMap.Add(Province.FromString("Warmińsko-mazurskie"), "14");
                ProvinceIdMap.Add(Province.FromString("Wielkopolskie"), "15");
                ProvinceIdMap.Add(Province.FromString("Zachodniopomorskie"), "16");

                //Add advertisement types
                AdvertisementTypeIdMap.Add(Enums.AdvertisementType.RENT, "4");
                AdvertisementTypeIdMap.Add(Enums.AdvertisementType.SALE, "1");
                AdvertisementTypeIdMap.Add(Enums.AdvertisementType.ALL, "6");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //Some error in spelling of provinces
            }
        }

        protected override string FormatRequestString(RealEstateOfferRequestModel a_offerModel)
        {
            string formattedRequest = websiteAdressBegin + Adress + RequestLinkSchema;

            try
            {
                formattedRequest = formattedRequest.Replace("{" + pageNumber + "}", a_offerModel.PageNumber.ToString());
                formattedRequest = formattedRequest.Replace("{" + propertyType + "}", PropertyTypeIdMap[a_offerModel.PropertyType]);
                formattedRequest = formattedRequest.Replace("{" + advertisementType + "}", AdvertisementTypeIdMap[a_offerModel.AdvertisementType]);
                formattedRequest = formattedRequest.Replace("{" + province + "}", ProvinceIdMap[a_offerModel.Province]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                formattedRequest = null;
            }
            return formattedRequest;
        }

        private RealEstateOfferModel ParseSingleOfferFromHtml(string webAddress, string a_html)
        {
            RealEstateOfferModel offer = new RealEstateOfferModel(webAddress);
            string currentState = null;
            try
            {

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(a_html);
                //Get offer images
                var offerImagesNode = document.DocumentNode.SelectSingleNode(offerImagesInHtml);
                var offerImagesListNode = offerImagesNode.SelectNodes(offerImageInHtml);
                foreach (var imageNode in offerImagesListNode)
                {
                    string address = "https://oferty-dom.pl" + imageNode.SelectSingleNode(offerSingleImageInHtml).GetAttributeValue("src", null);
                    if (address != null)
                    {
                        offer.ImagesAddresses.Add(address);
                    }
                }
                currentState = "offer images";

                //Get offer title
                var offerTitleNode = document.DocumentNode.SelectSingleNode(offerTitleInHtml);
                offer.Title = offerTitleNode.InnerText.Trim();
                currentState = "offer title node";

                //Get offer description
                var offerDescriptionNode = document.DocumentNode.SelectSingleNode(offerDescInHtml);
                offer.Description = offerDescriptionNode.InnerText.Trim();
                currentState = "offer description node";

                //Get offer price
                var offerPriceNode = document.DocumentNode.SelectSingleNode(offerPriceInHtml);
                offer.Price = ParsePrice(offerPriceNode.InnerText.Trim());
                currentState = "offer price node";

                //Get offer type (advertisement and real estate type)
                var offerTypeNode = document.DocumentNode.SelectSingleNode(offerTypeInHtml);
                string offerType = offerTypeNode.InnerText.ToLower();
                currentState = "offer type node";

                var offerCityNode = document.DocumentNode.SelectSingleNode(offerCityInHtml);
                string offerCity = offerCityNode.InnerText.Trim();
                currentState = "offer city node";

                var offerAreaNode = document.DocumentNode.SelectSingleNode(offerAreaInHtml);
                string offerArea = offerAreaNode.InnerText.Trim().ToLower().Remove(0, 4);
                if (offerArea.Contains("("))
                {
                    offerArea = offerArea.Remove(offerArea.IndexOf("(")).Trim();
                }
                offerArea = Regex.Replace(offerArea, @"\s+", "");
                currentState = "offer area node";

                //Get province
                offer.Province = Province.INVALID;
                foreach (var province in Province.PROVINCES)
                {
                    if (Regex.IsMatch(Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(offerType.Replace("-"," ").ToLower())), "\\b" + Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(province.Name.Replace("-", " ").ToLower())) + "\\b"))
                    {
                        offer.Province = province;
                        break;
                    }
                }

                currentState = "province";

                //Get city
                offer.OfferCity = City.INVALID;
                foreach (var city in City.CITIES)
                {
                    if (Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(offerCity.Replace("-", " ").ToLower())).Equals(Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(city.CityDetails.Nazwa.Replace("-", " ").ToLower()))))
                    {
                                offer.OfferCity = City.GetCity(offer.Province.Name, offerArea, city.CityDetails.Nazwa);
                                break;
                    }
                }
                if (offer.OfferCity == null || offer.OfferCity == City.INVALID)
                {
                    throw new Exception("City " + offerCity + " could not be parsed");
                }
                currentState = "city";

                //Get advertisement type
                if (offerType.Contains("sprzedaż") && offerType.Contains("wynajem"))
                {
                    offer.AdvType = AdvertisementType.ALL;
                }
                else if (offerType.Contains("sprzedaż"))
                {
                    offer.AdvType = AdvertisementType.SALE;
                }
                else if (offerType.Contains("wynajem"))
                {
                    offer.AdvType = AdvertisementType.RENT;
                }
                currentState = "advertisement type";

                //Get property type
                if (offerType.Contains("mieszkanie"))
                {
                    offer.PropType = PropertyType.FLAT;
                }
                else if (offerType.Contains("dom"))
                {
                    offer.PropType = PropertyType.HOUSE;
                }
                else if (offerType.Contains("lokal użytkowy"))
                {
                    offer.PropType = PropertyType.PREMISE;
                }
                else if (offerType.Contains("działka"))
                {
                    offer.PropType = PropertyType.LAND;
                }
                else if (offerType.Contains("pokój"))
                {
                    offer.PropType = PropertyType.ROOM;
                }
                else if (offerType.Contains("garaż"))
                {
                    offer.PropType = PropertyType.GARAGE;
                }
                else
                {
                    offer.PropType = PropertyType.ELSE;
                }
                currentState = "property type";

            }
            catch (Exception ex)
            {
                offer = null;
                Debug.WriteLine(currentState);
                Debug.WriteLine(ex);
            }

            return offer;
        }

        private async Task<bool> ParseOffersFromHtml(string a_html)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(a_html);
                var offersNodes = document.DocumentNode.SelectNodes(offersInHtml);
                foreach (var offerNode in offersNodes)
                {
                    string offerAddress = websiteAdressBegin + Adress + offerNode.GetAttributeValue("href", null);
                    string offerWebsite = ReadWebsite(offerAddress);
                    var offer = ParseSingleOfferFromHtml(offerAddress, offerWebsite);
                    if (offer != null)
                    {
                        bool isDuplicate = false;
                        int currentSize = WebsitesManagement.Instance.RealEstateOffers.Count;
                        for (int i = 0; i < currentSize; i++)
                        {
                            var singleOffer = WebsitesManagement.Instance.RealEstateOffers.ElementAt(i);
                            if (singleOffer.IsDuplicate(offer))
                            {
                                isDuplicate = true;
                                break;
                            }
                        }
                        if (!isDuplicate)
                        {
                            lock (WebsitesManagement.Instance.RealEstateOffers)
                            {
                                WebsitesManagement.Instance.RealEstateOffers.Add(offer);
                            }
                        }
                    }
                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
            return true;
        }

        public async override void GetRealEstateOffers()
        {
            foreach (var propType in Enums.PropertyType.Values)
            {
                foreach (var province in Province.PROVINCES)
                {
                    try
                    {
                        RealEstateOfferRequestModel model = new RealEstateOfferRequestModel(Province.FromString(province.Name), City.ALL,
                            Enums.PropertyType.FromString(propType.Name), Enums.AdvertisementType.ALL);
                        string request = FormatRequestString(model);
                        string offersWebsite = ReadWebsite(request);
                        if (offersWebsite != null)
                        {
                            //Parse offers from first page
                            await ParseOffersFromHtml(offersWebsite);
                            //Get number of pages left to check
                            string offerTotalPages = offersWebsite.Substring(offersWebsite.LastIndexOf("golink('/oferta/search/")
                                + "golink('/oferta/search/".Length);
                            offerTotalPages = offerTotalPages.Remove(offerTotalPages.IndexOf(".html"));
                            int count = Int32.Parse(offerTotalPages);
                            for (int i = 2; i < count; i++)
                            {
                                //Parse offers from rest of pages
                                model.PageNumber++;
                                request = FormatRequestString(model);
                                offersWebsite = ReadWebsite(request);
                                await ParseOffersFromHtml(offersWebsite);
                            }
                        }
                        else
                        {
                            //Invalid html
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        protected override int ParsePrice(string priceText)
        {
            string priceInt = "";
            foreach (var digit in priceText)
            {
                int number;
                if(Int32.TryParse(digit.ToString(), out number))
                {
                    priceInt += digit;
                }
            }
            return Int32.Parse(priceInt);
        }
    }
}