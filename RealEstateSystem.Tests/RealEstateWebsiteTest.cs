// <copyright file="RealEstateWebsiteTest.cs">Copyright ©  2016</copyright>
using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RealEstateSystem.RealEstate;

namespace RealEstateSystem.RealEstate.Tests
{
    /// <summary>This class contains parameterized unit tests for RealEstateWebsite</summary>
    [PexClass(typeof(RealEstateWebsite))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class RealEstateWebsiteTest
    {
        /// <summary>Test stub for ParseLink(String[])</summary>
        [PexMethod]
        public string ParseLinkTest(
            [PexAssumeUnderTest]RealEstateWebsite target,
            string[] a_argumentValues
        )
        {
            string result = target.ParseLink(a_argumentValues);
            return result;
            // TODO: add assertions to method RealEstateWebsiteTest.ParseLinkTest(RealEstateWebsite, String[])
        }
    }
}
