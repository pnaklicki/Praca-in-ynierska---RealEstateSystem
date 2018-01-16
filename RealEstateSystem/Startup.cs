using Microsoft.Owin;
using Owin;
using RealEstateSystem.Enums;
using RealEstateSystem.Models;
using RealEstateSystem.RealEstate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

[assembly: OwinStartupAttribute(typeof(RealEstateSystem.Startup))]
namespace RealEstateSystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {   
            ConfigureAuth(app);
            new Task(async () =>
            {
                DataManagement.Instance.UpdateAreaData();
                Province.Initialize();
                await Task.Delay(1000);
                WebsitesManagement.Instance.GetOffersFromAllWebsites();
            }).Start();
        }
    }
}
