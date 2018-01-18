using HtmlAgilityPack;
using RealEstateSystem.Models;
using RealEstateSystem.RealEstate;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace RealEstateSystem
{
    public static class DataManagement
    {
        private const string citiesListWebsiteAddress = "http://www.staypoland.com/wszystkie-polskie-miasta_1.htm";
        private const string provincesListWebsiteAddress = "http://www.staypoland.com/wojewodztwa.htm";

        private static List<TerytUslugaWs1.MiejscowoscPelna> cities;
        private static List<TerytUslugaWs1.JednostkaTerytorialna> provinces;

        public static void Initialize()
        {
            cities = new List<TerytUslugaWs1.MiejscowoscPelna>();
            provinces = new List<TerytUslugaWs1.JednostkaTerytorialna>();
            ApplicationDbContext context = new ApplicationDbContext();
            //cities
            try
            {//If there is exception it means table exists
                context.Database.ExecuteSqlCommand("CREATE TABLE [dbo].[Cities] (" +
                              "[Id]          INT          IDENTITY(1, 1) NOT NULL," +
                              "[GmiRodzaj]   VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[GmiSymbol]   VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[Gmina]       VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[Mz]          VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[NMSK]        VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[NMST]        VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[Nazwa]       VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[PowSymbol]   VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[Powiat]      VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[RM]          VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[RMNazwa]     VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[SymBM]       VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[Symbol]      VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[SymbolPodst] VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[SymbolStat]  VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[WojSymbol]   VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "[Wojewodztwo] VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                              "PRIMARY KEY CLUSTERED([Id] ASC));");
            }
            catch (Exception)
            {             
            }
            //Provinces
            try
            {
                context.Database.ExecuteSqlCommand("CREATE TABLE [dbo].[Provinces] (" +
                                "[Id]        INT          IDENTITY(1, 1) NOT NULL," +
                                "[NAZWA]     VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                                "[NAZWA_DOD] VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                                "[WOJ]       VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL," +
                                "PRIMARY KEY CLUSTERED([Id] ASC));");
            }
            catch (Exception ex)
            {             
            }
            //Updates
            try
            {
                context.Database.ExecuteSqlCommand("CREATE TABLE [dbo].[Updates] (" +
                                "[Id]   INT      IDENTITY(1, 1) NOT NULL," +
                                "[Date] DATETIME NOT NULL," +
                                "PRIMARY KEY CLUSTERED([Id] ASC));");
            }
            catch (Exception)
            {   
            }

        }

        public static HashSet<TerytUslugaWs1.MiejscowoscPelna> getCities()
        {
            return new HashSet<TerytUslugaWs1.MiejscowoscPelna>(cities);
        }

        public static List<TerytUslugaWs1.JednostkaTerytorialna> getProvinces()
        {
            return provinces;
        }

        private static string ReadWebsite(string address)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            string website = "";
            try
            {
                website = client.DownloadString(address);
                website = System.Net.WebUtility.HtmlDecode(website);
            }
            catch (Exception ex)
            {
                website = null;
                Debug.WriteLine(ex);
            }
            return website;
        }

        private static bool ShouldDatabaseBeUpdated()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            try
            {
                DateTime lastUpdate = context.Database.SqlQuery<DateTime>("SELECT Date FROM dbo.Updates").Last();
                context.Dispose();
                if (DateTime.Now.Subtract(lastUpdate).TotalDays > 365)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static void UpdateAreaData()
        {
            if (ShouldDatabaseBeUpdated())
            {
                ResetDatabase();
                TerytWs1Client c = new TerytWs1Client();
                try
                {
                    
                    var proxy = c.ChannelFactory;
                    proxy.Credentials.UserName.UserName = "TestPubliczny";
                    proxy.Credentials.UserName.Password = "1234abcd";
                    var channel = proxy.CreateChannel();
                    var isLogged = channel.CzyZalogowany();
                    if (isLogged)
                    {
                        var woj = c.PobierzListeWojewodztw(DateTime.Now);
                        foreach (var w in woj)
                        {
                            provinces.Add(w);
                            AddProvinceToDatabase(w);
                            var pow = c.PobierzListePowiatow(w.WOJ, DateTime.Now);
                            foreach (var p in pow)
                            {
                                var gmi = c.PobierzListeGmin(w.WOJ, p.POW, DateTime.Now);
                                foreach (var g in gmi)
                                {
                                    var we = new TerytUslugaWs1.MiejscowoscPelna();

                                    var mia = c.PobierzListeMiejscowosciWGminieZSymbolem(w.WOJ, p.POW, g.GMI, g.RODZ, DateTime.Now)
                                        .Where(m => m.RMNazwa == "miasto" || m.RMNazwa == "wieś");
                                    foreach (var m in mia)
                                    {
                                        cities.Add(m);
                                        AddCityToDatabase(m);
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
                c.Close();
            }
            else
            {
                try
                {
                    ApplicationDbContext context = new ApplicationDbContext();
                    HashSet<TerytUslugaWs1.MiejscowoscPelna> citiesList = new HashSet<TerytUslugaWs1.MiejscowoscPelna>(context.Database.SqlQuery<TerytUslugaWs1.MiejscowoscPelna>("SELECT GmiRodzaj, GmiSymbol, Gmina, Mz, NMSK, NMST, Nazwa, PowSymbol, Powiat, " +
                        "RM, RMNazwa, SymBM, Symbol, SymbolPodst, SymbolStat, WojSymbol, Wojewodztwo FROM dbo.Cities").ToList());
                    HashSet<TerytUslugaWs1.JednostkaTerytorialna> provincesList = new HashSet<TerytUslugaWs1.JednostkaTerytorialna>(context.Database.SqlQuery<TerytUslugaWs1.JednostkaTerytorialna>("SELECT NAZWA, NAZWA_DOD, WOJ FROM dbo.Provinces").ToList());
                    foreach (var city in citiesList)
                    {
                        cities.Add(city);
                    }
                    provinces.AddRange(provincesList);
                    context.Dispose();
                } catch(Exception ex)
                {
                    Debug.WriteLine(ex);

                }
            }
        }

        private static void AddProvinceToDatabase(TerytUslugaWs1.JednostkaTerytorialna province)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            context.Database.ExecuteSqlCommand("INSERT INTO Provinces VALUES (@NAZWA, @NAZWA_DOD, @WOJ)",
                new SqlParameter("@NAZWA", (province.NAZWA.First().ToString().ToUpper() + province.NAZWA.Substring(1).ToLower()).Trim()), new SqlParameter("@NAZWA_DOD", province.NAZWA_DOD.Trim()),
                new SqlParameter("@WOJ", province.WOJ.Trim()));
            context.Dispose();
        }

        private static void ResetDatabase()
        {
            
            ApplicationDbContext context = new ApplicationDbContext();
            context.Database.ExecuteSqlCommand("DELETE FROM Cities");
            context.Database.ExecuteSqlCommand("DELETE FROM Provinces");
            context.Database.ExecuteSqlCommand("INSERT INTO Updates (Date) VALUES (@Date)", new SqlParameter("@Date", DateTime.Now));
            context.Dispose();
        }

        private static void AddCityToDatabase(TerytUslugaWs1.MiejscowoscPelna city)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            context.Database.ExecuteSqlCommand("INSERT INTO Cities " +
                "VALUES (@GmiRodzaj, @GmiSymbol, @Gmina, @Mz, @NMSK, @NMST, @Nazwa, @PowSymbol, @Powiat, " +
                "@RM, @RMNazwa, @SymBM, @Symbol, @SymbolPodst, @SymbolStat, @WojSymbol, @Wojewodztwo)", 
                new SqlParameter("@GmiRodzaj", city.GmiRodzaj.Trim()), new SqlParameter("@GmiSymbol", city.GmiRodzaj.Trim()),
                new SqlParameter("@Gmina", city.GmiRodzaj.Trim()), new SqlParameter("@Mz", city.Mz.Trim()), 
                new SqlParameter("@NMSK", city.NMSK.Trim()), new SqlParameter("@NMST", city.NMST.Trim()),
                new SqlParameter("@Nazwa", city.Nazwa.Trim().First().ToString() + city.Nazwa.Trim().Replace("&#243;","ó").Substring(1).ToLower()), new SqlParameter("@PowSymbol", city.PowSymbol.Trim()), 
                new SqlParameter("@Powiat", city.Powiat.Trim()), new SqlParameter("@RM", city.RM.Trim()),
                new SqlParameter("@RMNazwa", city.RMNazwa.Trim()), new SqlParameter("@SymBM", city.SymBM.Trim()), 
                new SqlParameter("@Symbol", city.Symbol.Trim()), new SqlParameter("@SymbolPodst", city.SymbolPodst.Trim()), 
                new SqlParameter("@SymbolStat", city.SymbolStat.Trim()), new SqlParameter("@WojSymbol", city.WojSymbol.Trim()),
                new SqlParameter("@Wojewodztwo", city.Wojewodztwo.Trim()));
            context.Dispose();
        }
    }
}