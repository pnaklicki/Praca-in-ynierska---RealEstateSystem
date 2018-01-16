using Microsoft.Test.VisualVerification;
using RealEstateSystem.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Web;

namespace RealEstateSystem.Models
{
    public class RealEstateOfferModel
    {
        private static WebClient client = new WebClient();
        private int price;
        private string title;
        private string description;
        private AdvertisementType advType;
        private PropertyType propType;
        private City city;
        private Province province;
        private List<string> imagesAddresses;
        private String offerAddress;
        private Guid id;

        public RealEstateOfferModel(string address)
        {
            id = Guid.NewGuid();
            offerAddress = address;
            imagesAddresses = new List<string>();
        }

        public Guid Id
        {
            get
            {
                return id;
            }
        }

        public string OfferAddress
        {
            get
            {
                return offerAddress;
            }
        }

        public List<string> ImagesAddresses
        {
            get
            {
                return imagesAddresses;
            }
            set
            {
                imagesAddresses = value;
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

        public int Price
        {
            get
            {
                return price;
            }

            set
            {
                price = value;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        public AdvertisementType AdvType
        {
            get
            {
                return advType;
            }

            set
            {
                advType = value;
            }
        }

        public PropertyType PropType
        {
            get
            {
                return propType;
            }

            set
            {
                propType = value;
            }
        }

        public City OfferCity
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

        private bool AreImagesSame(Bitmap firstImage, Bitmap secondImage)
        {
            Snapshot actual = Snapshot.FromBitmap(firstImage);
            Snapshot expected = Snapshot.FromBitmap(secondImage);
            Snapshot difference = actual.CompareTo(expected);
            SnapshotVerifier v = new SnapshotColorVerifier(Color.Black, new ColorDifference());
            return v.Verify(difference) == VerificationResult.Pass;
        }

        public Bitmap GetBitmapFromUrl(string url)
        {
            Bitmap bitmap = null;
            try
            {
                Stream stream = client.OpenRead(url);
                bitmap = new Bitmap(stream);
                stream.Flush();
                stream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return bitmap;
        }

        private bool DoOffersHaveDuplicatedImages(RealEstateOfferModel offer)
        {
            try
            {
                foreach (var imageFirst in offer.ImagesAddresses)
                {
                    foreach (var imageSecond in imagesAddresses)
                    {
                        Bitmap firstImage = GetBitmapFromUrl(imageFirst);
                        Bitmap secondImage = new Bitmap(GetBitmapFromUrl(imageSecond), firstImage.Width, firstImage.Height);
                        if (AreImagesSame(firstImage, secondImage))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return true;
            }
            return false;
        }

        private bool DoOffersHaveSameLocation(RealEstateOfferModel offer)
        {
            try
            {
                if (offer.OfferCity.CityDetails.Wojewodztwo == OfferCity.CityDetails.Wojewodztwo)
                {
                    if (offer.OfferCity.CityDetails.GmiSymbol == OfferCity.CityDetails.GmiSymbol)
                    {
                        if (offer.OfferCity.CityDetails.Nazwa == OfferCity.CityDetails.Nazwa)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }

        public bool IsDuplicate(RealEstateOfferModel offer)
        {
            try
            {
                if (offer.PropType == PropType)
                {
                    if (offer.AdvType == AdvType)
                    {
                        //Do other stuff
                        if (DoOffersHaveSameLocation(offer))
                        {
                            if (offer.Price == Price)
                            {
                                //Duplicated offers may have same descriptions
                                if (offer.Title == Title)
                                {
                                    return true;
                                }
                                return DoOffersHaveDuplicatedImages(offer);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return false;
        }


        /// <summary>
        /// Method which checks if offer matches given offer
        /// </summary>
        /// <param name="offer">Offer to match</param>
        /// <returns>True if offers match</returns>
        public bool IsMatch(RealEstateOfferRequestModel offer)
        {
            try
            {
                if (this.AdvType == AdvertisementType.INVALID || offer.AdvertisementType == AdvertisementType.INVALID ||
                    this.PropType == PropertyType.INVALID || offer.PropertyType == PropertyType.INVALID || offer.City == City.INVALID)
                {
                    return false;
                }
                else
                {
                    if (this.advType == offer.AdvertisementType || offer.AdvertisementType == AdvertisementType.ALL)
                    {
                        if (this.PropType == offer.PropertyType || offer.PropertyType == PropertyType.ALL)
                        {
                            if (this.Province == offer.Province || offer.Province == Province.ALL)
                            {
                                if (offer.City == City.ALL || this.city.CityDetails.Nazwa == offer.City.CityDetails.Nazwa)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return false;
        }
    }
}