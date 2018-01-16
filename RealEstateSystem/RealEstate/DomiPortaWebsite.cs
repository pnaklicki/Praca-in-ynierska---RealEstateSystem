using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealEstateSystem.Models;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Diagnostics;
using static RealEstateSystem.Models.RealEstateOfferModel;
using RealEstateSystem.Enums;
using System.Threading;

namespace RealEstateSystem.RealEstate
{
    public class DomiPortaWebsite : RealEstateWebsite
    {
        private string offersIdInHtml = ".//div[@class = 'detail-card__photo WynikiListaZdjecie ']";
        private string offerTitleInHtml = ".//span[@class = 'details__type--localization']";
        private string offerDescInHtml = ".//div[@class = 'details-description__body']";
        private string offerTypeInHtml = ".//title";
        private string offerLocationInHtml = ".//span[@class = 'detail-feature__value detail-feature--full-localization']";
        private string offerImagesInHtml = ".//meta[@property = 'og:image']";
        private string offerPriceInHtml = ".//span[@itemprop = 'price']";

        public DomiPortaWebsite() : base("www.domiporta.pl", "/" +
            propertyType
            +"/"+
            advertisementType
            + "?Localization=" +
            province
            +"&PageNumber="+
            pageNumber, "DomiPorta")
        {
            //Add property types
            PropertyTypeIdMap.Add(PropertyType.ALL, "nieruchomosci");
            PropertyTypeIdMap.Add(PropertyType.FLAT, "mieszkanie");
            PropertyTypeIdMap.Add(PropertyType.GARAGE, "garaz");
            PropertyTypeIdMap.Add(PropertyType.HOUSE, "dom");
            PropertyTypeIdMap.Add(PropertyType.LAND, "dzialke");
            PropertyTypeIdMap.Add(PropertyType.PREMISE, "lokal-uzytkowy");
            PropertyTypeIdMap.Add(PropertyType.ROOM, "pokoj");

            //Add provinces
            foreach (Province type in Province.PROVINCES)
            {
                ProvinceIdMap.Add(type, type.Name);
            }

            //Add advertisement types
            AdvertisementTypeIdMap.Add(AdvertisementType.ALL, "wszystkie");
            AdvertisementTypeIdMap.Add(AdvertisementType.SALE, "sprzedam");
            AdvertisementTypeIdMap.Add(AdvertisementType.RENT, "wynajme");
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
                var offerImagesNode = document.DocumentNode.SelectNodes(offerImagesInHtml);
                foreach (var imageNode in offerImagesNode)
                {
                    string address = imageNode.GetAttributeValue("content", null);
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
                offer.Price = ParsePrice(offerPriceNode.GetAttributeValue("content","0"));
                currentState = "offer price node";

                //Get offer type(advertisement and real estate type)
                var offerTypeNode = document.DocumentNode.SelectSingleNode(offerTypeInHtml);

                var locationNodes = document.DocumentNode.SelectSingleNode(offerLocationInHtml).SelectNodes(".//a");
                currentState = "offer location node";

                string offerProvince = locationNodes[0].InnerText;
                offer.Province = Province.FromString(offerProvince);
                currentState = "province";

                string offerCity = locationNodes[1].InnerText;
                offer.OfferCity = City.FromString(offerCity);
                if (offer.OfferCity == null || offer.OfferCity == City.INVALID)
                {
                    throw new Exception("City " + offerCity + " could not be parsed");
                }
                currentState = "city";


                string offerType = System.Net.WebUtility.HtmlDecode(offerTypeNode.InnerText.ToLower());
                currentState = "offer type node";

                //Get advertisement type
                if (offerType.Contains("sprzedam"))
                {
                    offer.AdvType = AdvertisementType.SALE;
                }
                else if (offerType.Contains("wynajmę"))
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
                    offer.PropType = PropertyType.INVALID;
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
                var offersIdentyfiers = document.DocumentNode.SelectNodes(offersIdInHtml);
                foreach (var offerIdentyfier in offersIdentyfiers)
                {
                    var offerNode = offerIdentyfier.ParentNode;
                    string offerAddress = offerNode.GetAttributeValue("href", null);
                    string offerWebsite = ReadWebsite(offerAddress);
                    var offer = ParseSingleOfferFromHtml(offerAddress,offerWebsite);
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
            foreach (var propType in PropertyType.Values)
            {
                if (propType != PropertyType.ELSE && propType != PropertyType.INVALID)
                {
                    foreach (var prov in Province.PROVINCES)
                    {
                        RealEstateOfferRequestModel offerModel = new RealEstateOfferRequestModel(prov, City.ALL, propType,
                            AdvertisementType.ALL);
                        string request = FormatRequestString(offerModel);
                        string offersWebsite = ReadWebsite(request);
                        if (offersWebsite != null)
                        {
                            await ParseOffersFromHtml(offersWebsite);
                            string offerTotalPages = offersWebsite.Substring(offersWebsite.LastIndexOf("<input type=\"text\" value=\"1\" min=\"1\" max=\"")
                                    + "<input type=\"text\" value=\"1\" min=\"1\" max=\"".Length);
                            offerTotalPages = offerTotalPages.Remove(offerTotalPages.IndexOf('"'));
                            int count = Int32.Parse(offerTotalPages);
                            for (int i = 2; i < count; i++)
                            {
                                offerModel.PageNumber++;
                                request = FormatRequestString(offerModel);
                                offersWebsite = ReadWebsite(request);
                                await ParseOffersFromHtml(offersWebsite);
                            }
                        }

                    }
                }
            }
        }

        protected override string FormatRequestString(RealEstateOfferRequestModel a_offerModel)
        {
            string formattedRequest = websiteAdressBegin + Adress + RequestLinkSchema;

            formattedRequest = formattedRequest.Replace(propertyType, PropertyTypeIdMap[a_offerModel.PropertyType]);
            formattedRequest = formattedRequest.Replace(advertisementType, AdvertisementTypeIdMap[a_offerModel.AdvertisementType]);
            formattedRequest = formattedRequest.Replace(province, ProvinceIdMap[a_offerModel.Province]);
            formattedRequest = formattedRequest.Replace(pageNumber, a_offerModel.PageNumber.ToString());

            return formattedRequest;
        }

        protected override int ParsePrice(string priceText)
        {
            string priceInt = "";
            foreach (var digit in priceText)
            {
                if(digit == '.')
                {
                    break;
                }
                int number;
                if (Int32.TryParse(digit.ToString(), out number))
                {
                    priceInt += digit;
                }
            }
            return Int32.Parse(priceInt);
        }
    }
}