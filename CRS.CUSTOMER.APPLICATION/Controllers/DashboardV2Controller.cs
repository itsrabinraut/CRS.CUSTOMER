using CRS.CUSTOMER.APPLICATION.Helper;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.CommonModel;
using CRS.CUSTOMER.APPLICATION.Models.Dashboard;
using CRS.CUSTOMER.APPLICATION.Models.DashboardV2;
using CRS.CUSTOMER.APPLICATION.Models.Home;
using CRS.CUSTOMER.APPLICATION.Models.LocationManagement;
using CRS.CUSTOMER.APPLICATION.Models.Search;
using CRS.CUSTOMER.APPLICATION.Models.SearchFilterManagement;
using CRS.CUSTOMER.BUSINESS.CommonManagement;
using CRS.CUSTOMER.BUSINESS.Dashboard;
using CRS.CUSTOMER.BUSINESS.DashboardV2;
using CRS.CUSTOMER.BUSINESS.RecommendedClubHost;
using CRS.CUSTOMER.BUSINESS.Search;
using CRS.CUSTOMER.SHARED.RecommendedClubHost;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class DashboardV2Controller : CustomController
    {
        private readonly Dictionary<string, string> _locationHelper = ApplicationUtilities.MapJsonDataToDictionaryViaKeyName("URLManagementConfigruation", "Location");
        private readonly IDashboardBusiness _oldDashboardBusiness;
        private readonly IRecommendedClubHostBusiness _recommendedClubHostBuss;
        private readonly IDashboardV2Business _dashboardBusiness;
        private readonly ICommonManagementBusiness _commonBusiness;
        private readonly ISearchBusiness _searchBusiness;

        public DashboardV2Controller(IDashboardBusiness oldDashboardBusiness, IRecommendedClubHostBusiness recommendedClubHostBuss, IDashboardV2Business dashboardBusiness, ICommonManagementBusiness commonBusiness, ISearchBusiness searchBusiness)
          => (_oldDashboardBusiness, _recommendedClubHostBuss, _dashboardBusiness, _commonBusiness, _searchBusiness) = (oldDashboardBusiness, recommendedClubHostBuss, dashboardBusiness, commonBusiness, searchBusiness);

        #region DASHBOARD
        [HttpGet, Route("")]
        public ActionResult Index()
        {
            Session["ReservationPage1Model"] = null;
            Session["ReservationPage2Model"] = null;
            Session["ReservationPage3Model"] = null;
            Session["ReservationPage4Model"] = null;
            TempData["BackFromMenuBar"] = "null";
            var ResponseModel = new DashboardModel();
            if (Session["SystemLinkModel"] == null)
            {
                var systemLinkModel = new List<SystemLinkModel>();
                systemLinkModel = _commonBusiness.GetSystemLink().MapObjects<SystemLinkModel>();
                Session["SystemLinkModel"] = systemLinkModel;
            }
            var bannerServiceResp = _oldDashboardBusiness.GetBanners();
            if (bannerServiceResp != null && bannerServiceResp.Count > 0)
            {
                ResponseModel.Banners = bannerServiceResp.MapObjects<BannersModel>();
                ResponseModel.Banners.ForEach(x => x.BannerId = x.BannerId?.EncryptParameter());
                ResponseModel.Banners.ForEach(x => x.BannerImage = ImageHelper.ProcessedImage(x.BannerImage));
            }
            var locationServiceResp = _oldDashboardBusiness.GetLocationList();
            if (locationServiceResp != null && locationServiceResp.Count > 0)
            {
                ResponseModel.Location = locationServiceResp.MapObjects<LocationListModel>();
                ResponseModel.Location.ForEach(x => x.LocationID = x.LocationID?.EncryptParameter());
                ResponseModel.Location.ForEach(x => x.LocationImage = ImageHelper.ProcessedImage(x.LocationImage));
            }
            var planListDBResponse = _oldDashboardBusiness.GetPlansList();
            if (planListDBResponse != null && planListDBResponse.Count > 0)
            {
                ResponseModel.PlanModel = planListDBResponse.MapObjects<DashboardPlanModel>();
                ResponseModel.PlanModel.ForEach(x => x.PlanId = x.PlanId?.EncryptParameter());
                ResponseModel.PlanModel.ForEach(x => x.PlanImage = ImageHelper.ProcessedImage(x.PlanImage));
            }
            var recommendedHostListDBResponse = _oldDashboardBusiness.GetRecommendedHost();
            if (recommendedHostListDBResponse.Count > 0)
            {
                ResponseModel.RecommendedHostModel = recommendedHostListDBResponse.MapObjects<DashboardRecommendedHostModel>();
                foreach (var item in ResponseModel.RecommendedHostModel)
                {
                    item.LocationId = item.LocationId?.EncryptParameter();
                    item.ClubId = item.ClubId?.EncryptParameter();
                    item.HostId = item.HostId?.EncryptParameter();
                    item.HostImage = ImageHelper.ProcessedImage(item.HostImage);
                    item.ClubLogo = ImageHelper.ProcessedImage(item.ClubLogo);
                }
            }
            var recommendedClubListDBResponse = _oldDashboardBusiness.GetRecommendedClub();
            if (recommendedClubListDBResponse.Count > 0)
            {
                ResponseModel.RecommendedClubModel = recommendedClubListDBResponse.MapObjects<DashboardRecommendedClubModel>();
                foreach (var item in ResponseModel.RecommendedClubModel)
                {
                    item.LocationId = item.LocationId?.EncryptParameter();
                    item.ClubId = item.ClubId?.EncryptParameter();
                    item.ClubLogo = ImageHelper.ProcessedImage(item.ClubLogo);
                }
            }
            List<RecommendationLocationModel> locationsList = new List<RecommendationLocationModel>();
            foreach (var item in ResponseModel.Location)
            {
                if (!string.IsNullOrEmpty(item.LocationStatus) && item.LocationStatus.Trim().ToLower() == "A")
                    locationsList.Add(new RecommendationLocationModel { name = item.LocationName, lat = item.Latitude, lng = item.Longitude, id = item.LocationURl });
            }

            ViewBag.JsonLocation = JsonConvert.SerializeObject(locationsList);
            ViewBag.ActionPageName = "Dashboard";

            var metaTagDBResponse = _commonBusiness.GetMetaTagInfo("1");
            ViewBag.MetaClubCount = metaTagDBResponse.Item1;
            ViewBag.MetaHostCount = metaTagDBResponse.Item2;
            var jsonRecommendedClubAndHost = GetRecommendedClubAndHost("1");
            var data = jsonRecommendedClubAndHost.Data as RecommendedClubAndHostModel;
            if (data != null && (data.RecommendedClubModel.Count > 0 || data.RecommendedHostModel.Count > 0))
            {
                ViewBag.IsAvailableRecommended = true;
            }
            else
            {
                ViewBag.IsAvailableRecommended = false;
            }
            //var advertisementimage = _commonBusiness.GetAdvertisement();
            //advertisementimage = advertisementimage
            //    .Select(item => ImageHelper.ProcessedImage(item))
            //    .ToList();
            //Session["AdvertisementImage"] = advertisementimage;
            return View(ResponseModel);
        }

        [HttpGet]
        public JsonResult GetRecommendedClubAndHost(string LocationId)
        {
            //var lId = !string.IsNullOrEmpty(LocationId) ? LocationId.DecryptParameter() : null;
            var lId = ApplicationUtilities.GetKeyValueFromDictionary(_locationHelper, LocationId);
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var Response = new RecommendedClubAndHostModel();
            var clubRecommendationDBResponse = _oldDashboardBusiness.GetRecommendedClub(lId);
            if (clubRecommendationDBResponse.Count > 0)
            {
                Response.RecommendedClubModel = clubRecommendationDBResponse.MapObjects<ClubRecommendationListModel>();
                Response.RecommendedClubModel.ForEach(x => x.ClubId = x.ClubId.EncryptParameter());
                Response.RecommendedClubModel.ForEach(x => x.LocationId = x.LocationId.EncryptParameter());
                Response.RecommendedClubModel.ForEach(x => x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo));
            }
            var hostRecommendationDBResponse = _oldDashboardBusiness.GetRecommendedHost(lId, CustomerId);
            if (hostRecommendationDBResponse.Count > 0)
            {
                Response.RecommendedHostModel = hostRecommendationDBResponse.MapObjects<HostRecommendationListModel>();
                Response.RecommendedHostModel.ForEach(x => x.ClubId = x.ClubId.EncryptParameter());
                Response.RecommendedHostModel.ForEach(x => x.HostId = x.HostId.EncryptParameter());
                Response.RecommendedHostModel.ForEach(x => x.LocationId = x.LocationId.EncryptParameter());
                Response.RecommendedHostModel.ForEach(x => x.HostImage = ImageHelper.ProcessedImage(x.HostImage));
                Response.RecommendedHostModel.ForEach(x => x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo));
            }
            return Json(Response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMainPageClubHost(string LocationId)
        {
            var responseData = new Dictionary<string, object> { { "Code", 1 }, { "Message", "Invalid Details" }, { "PartialView", "" } };
            var lId = ApplicationUtilities.GetKeyValueFromDictionary(_locationHelper, LocationId);
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var Response = new LocationClubHostModel();
            #region Recommended Club
            var recommendedClubDBRequest = new RecommendedClubRequestCommon()
            {
                LocationId = lId,
                CustomerId = CustomerId,
                PageType = "1"
            };
            var dbClubResponse = _recommendedClubHostBuss.GetRecommendedClub(recommendedClubDBRequest);
            Response.ClubListModel = dbClubResponse.MapObjects<LocationClubListModel>();
            foreach (var item in Response.ClubListModel)
            {
                item.ClubId = item.ClubId.EncryptParameter();
                item.LocationId = item.LocationId.EncryptParameter();
                item.ClubLogo = ImageHelper.ProcessedImage(item.ClubLogo);
                item.ClubCoverPhoto = ImageHelper.ProcessedImage(item.ClubCoverPhoto);
                item.HostGalleryImage = item.HostGalleryImage.Select(x => ImageHelper.ProcessedImage(x)).ToList();
            }
            #endregion
            #region Recommended Host
            if (Response.ClubListModel != null && Response.ClubListModel.Count > 0)
            {
                var recommendedHostDBRequest = new RecommendedHostRequestCommon()
                {
                    LocationId = lId,
                    CustomerId = CustomerId,
                    PageType = "1"
                };
                var dbHostResponse = _recommendedClubHostBuss.GetRecommendedHost(recommendedHostDBRequest);
                Response.HostListModel = dbHostResponse.MapObjects<LocationHostListModel>();
                foreach (var item in Response.HostListModel)
                {
                    item.ClubId = item.ClubId.EncryptParameter();
                    item.LocationId = item.LocationId.EncryptParameter();
                    item.HostId = item.HostId.EncryptParameter();
                    item.HostImage = ImageHelper.ProcessedImage(item.HostImage);
                    item.ClubLogo = ImageHelper.ProcessedImage(item.ClubLogo);
                }
            }
            #endregion
            if (Response.ClubListModel != null && Response.ClubListModel.Count > 0)
            {
                var partialViewString = RenderHelper.RenderPartialViewToString(this, "_MainPageClubHost", Response);
                responseData["Code"] = 0;
                responseData["Message"] = "Success";
                responseData["PartialView"] = partialViewString;
            }
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult InitiateDateTimeFilterPopup()
        {
            var Response = new LocationClubHostModel();
            var culture = Request.Cookies["culture"]?.Value ?? "ja";
            var responseData = new Dictionary<string, object> { { "Code", 1 }, { "Message", "Invalid Details" }, { "PartialView", "" } };
            var partialViewString = RenderHelper.RenderPartialViewToString(this, "_DateTimeFilterPopUp", Response);
            responseData["Code"] = 0;
            responseData["Message"] = "Success";
            responseData["PartialView"] = partialViewString;
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Filter
        [HttpGet]
        public JsonResult GetLocationFilterPopUp()
        {
            var responseData = new Dictionary<string, object> { { "Code", 1 }, { "Message", "Invalid Details" }, { "PartialView", "" } };
            var Response = new List<StaticDataModel>();
            Response = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("10"));
            if (Response.Count > 0)
            {
                responseData["Code"] = 0;
                responseData["Message"] = "Success";
                responseData["PartialView"] = RenderHelper.RenderPartialViewToString(this, "_LocationFilterPopUp", Response);
            }
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPreferenceFilterPopUp(string LocationId)
        {
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var responseData = new Dictionary<string, object> { { "Code", 1 }, { "Message", "Invalid Details" }, { "PartialView", "" }, { "ClubDetailMapData", "" } };
            //var lId = !string.IsNullOrEmpty(LocationId) ? LocationId.DecryptParameter() : string.Empty;
            var lId = ApplicationUtilities.GetKeyValueFromDictionary(_locationHelper, LocationId);
            var Response = new PreferenceFilterModel();
            Response.AgeModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("7"));
            Response.ConstellationModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("8"));
            Response.BloodTypeModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("5"));
            Response.ClubAvailabilityModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("4"));
            Response.ClubCategoryModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("2"));
            Response.HeightModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("6"));
            Response.PlanPriceModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("3"));
            Response.OccupationModel = DDLHelper.ConvertDictionaryToListJapanese(DDLHelper.LoadDropdownList("9"), true);
            var HostConstellationGroup = _commonBusiness.GetDDL("023");
            HostConstellationGroup.ForEach(x => x.Value = x.Value.EncryptParameter());
            ViewBag.HostConstellationGroup = HostConstellationGroup;
            var dbClubResponse = _dashboardBusiness.GetNewClub(lId, CustomerId, "1");
            Response.ClubModel = dbClubResponse.MapObjects<DashboardV2ClubDetailModel>();
            Response.ClubModel.ForEach(x => { x.ClubId = x.ClubId.EncryptParameter(); x.ClubLocationId = x.ClubLocationId.EncryptParameter(); x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo); });
            var dbHostResponse = _dashboardBusiness.GetNewHost(lId, CustomerId, null, "1");
            Response.HostModel = dbHostResponse.MapObjects<DashboardV2HostDetailModel>();
            Response.HostModel.ForEach(x => { x.ClubId = x.ClubId.EncryptParameter(); x.HostId = x.HostId.EncryptParameter(); x.ClubLocationId = x.ClubLocationId.EncryptParameter(); x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo); x.HostLogo = ImageHelper.ProcessedImage(x.HostLogo); });

            var ClubDetailMapModel = new List<ClubMapDetailModel>();
            var ClubDetailDbResponse = _searchBusiness.GetClubMapDetail(lId);
            ClubDetailMapModel = ClubDetailDbResponse.MapObjects<ClubMapDetailModel>();
            string CurrentUrl = ApplicationUtilities.GetAddressFromUrl(System.Web.HttpContext.Current.Request.Url.AbsoluteUri);
            List<dynamic> mappedData = new List<dynamic>();
            foreach (var item in ClubDetailMapModel)
            {
                dynamic mappedItem = new System.Dynamic.ExpandoObject();
                if (!string.IsNullOrEmpty(CurrentUrl))
                {
                    CurrentUrl = $"/area{item.LocationURL}/hostclub/{item.ClubCode}/host/";
                    mappedItem.URL = CurrentUrl.ToString();
                }
                if (float.TryParse(item.Latitude, out float latitude) && float.TryParse(item.Longitude, out float longitude))
                {
                    mappedItem.lat = latitude;
                    mappedItem.lng = longitude;
                }
                else continue;
                mappedItem.clubNameEnglish = item.ClubNameEnglish;
                mappedItem.clubNameJapanese = item.ClubNameJapanese;
                mappedItem.ratingScale = item.RatingScale;
                mappedItem.clubLogo = ImageHelper.ProcessedImage(item.ClubLogo);

                mappedData.Add(mappedItem);
            }
            responseData["Code"] = 0;
            responseData["Message"] = "Success";
            responseData["PartialView"] = RenderHelper.RenderPartialViewToString(this, "_PreferenceFilterPopUp", Response);
            responseData["ClubDetailMapData"] = mappedData;
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }

        #region Preference
        [HttpGet]
        public ActionResult Preference(string LocationId, string Type)
        {
            var TypeValue = !string.IsNullOrEmpty(Type) ? Type.DecryptParameter() : string.Empty;
            ViewBag.ActionPageName = "SearchFilter";
            ViewBag.TypeValue = TypeValue;
            ViewBag.LocationId = LocationId;
            //var lId = !string.IsNullOrEmpty(LocationId) ? LocationId.DecryptParameter() : LocationId;
            var lId = ApplicationUtilities.GetKeyValueFromDictionary(_locationHelper, LocationId);
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var Response = new List<ClubAvailabilityDetailModel>();
            var dbResponse = _dashboardBusiness.GetAvailabilityClub(lId, CustomerId, TypeValue, null);
            if (dbResponse != null && dbResponse.Count > 0)
            {
                Response = dbResponse.MapObjects<ClubAvailabilityDetailModel>();
                Response.ForEach(x =>
                {
                    x.ClubId = x.ClubId.EncryptParameter();
                    x.ClubLocationId = x.ClubLocationId.EncryptParameter();
                    x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo);
                    x.HostGalleryImage = x.HostGalleryImage.Select(y => ImageHelper.ProcessedImage(y)).ToList();
                });
            }
            return View(Response);
        }
        #endregion
        #endregion
    }
}