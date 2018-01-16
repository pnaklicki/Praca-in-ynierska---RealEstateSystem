using RealEstateSystem.Models;
using RealEstateSystem.RealEstate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace RealEstateSystem
{
    public class WebsitesManagement
    {
        private static WebsitesManagement instance = null;
        private List<RealEstateWebsite> estateWebsites;
        private List<RealEstateOfferModel> realEstateOffers;
        private Timer timer;

        private WebsitesManagement()
        {
            realEstateOffers = new List<RealEstateOfferModel>();
            estateWebsites = new List<RealEstateWebsite>();
            estateWebsites.Add(new OfertyDomWebsite());
            estateWebsites.Add(new DomiPortaWebsite());
        }

        public static WebsitesManagement Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new WebsitesManagement();
                }
                return instance;
            }
        }

        public List<RealEstateOfferModel> RealEstateOffers
        {
            get
            {
                return realEstateOffers;
            }

            set
            {
                realEstateOffers = value;
            }
        }

        public List<RealEstateOfferModel> GetFilteredOffers(RealEstateOfferRequestModel a_offer)
        {
            List<RealEstateOfferModel> offers = new List<RealEstateOfferModel>();
            lock (RealEstateOffers)
            {
                offers = RealEstateOffers.Where(m => m.IsMatch(a_offer)).ToList();
            }
            return offers;
        }

        /// <summary>
        /// This method starts collecting all offers from all websites,
        /// so it may take a while to complete
        /// </summary>
        public async void GetOffersFromAllWebsites()
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(OnOffersRefreshEvent);
            timer.Interval = 24 * 60 * 60 * 1000; //24 hours interval
            timer.Enabled = true;
            await Task.Delay(2000);

            foreach (var website in estateWebsites)
            {
                   website.GetRealEstateOffers();
            }
        }

        private void OnOffersRefreshEvent(object source, ElapsedEventArgs e)
        {
            //Start real estate refresh process
            GetOffersFromAllWebsites();
        }
    }
}