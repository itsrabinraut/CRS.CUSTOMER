using CRS.CUSTOMER.APPLICATION.Helper;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.DashboardV2;
using CRS.CUSTOMER.APPLICATION.Models.Search;
using CRS.CUSTOMER.APPLICATION.Models.SearchFilterManagement;
using CRS.CUSTOMER.BUSINESS.CommonManagement;
using CRS.CUSTOMER.BUSINESS.Dashboard;
using CRS.CUSTOMER.BUSINESS.DashboardV2;
using CRS.CUSTOMER.BUSINESS.Search;
using CRS.CUSTOMER.BUSINESS.SearchFilterManagement;
using CRS.CUSTOMER.SHARED.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    public class SearchController : CustomController
    {
        private readonly ISearchBusiness _searchBusiness;
        private readonly IDashboardBusiness _oldDashboardBusiness;
        private readonly IDashboardV2Business _dashboardBusiness;
        private readonly ICommonManagementBusiness _commonBusiness;
        private readonly ISearchFilterManagementBusiness _searchBusinessOld;
        public SearchController(ISearchBusiness searchBusiness, IDashboardBusiness oldDashboardBusiness, IDashboardV2Business dashboardBusiness, ICommonManagementBusiness commonBusiness, ISearchFilterManagementBusiness searchBusinessOld) =>
            (_searchBusiness, _oldDashboardBusiness, _dashboardBusiness, _commonBusiness, _searchBusinessOld) = (searchBusiness, oldDashboardBusiness, dashboardBusiness, commonBusiness, searchBusinessOld);
        [HttpGet]
        public ActionResult ClubSearchResult(string LocationId, string SearchFilter, string ClubCategory, string Price, string Shift, string Time, string ClubAvailability, bool NewClub = false)
        {
            ViewBag.ActionPageName = "SearchFilter";
            var lId = !string.IsNullOrEmpty(LocationId) ? LocationId.DecryptParameter() : null;
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var Response = new ClubSearchResultModel();
            var clubRecommendationDBResponse = _searchBusinessOld.GetRecommendedClub(lId);
            Response.RecommendedClubModel = clubRecommendationDBResponse.MapObjects<ClubRecommendationListModel>();
            Response.RecommendedClubModel.ForEach(x =>
            {
                x.LocationId = x.LocationId.EncryptParameter();
                x.ClubId = x.ClubId.EncryptParameter();
                x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo);
            });
            if (NewClub)
            {
                var dbResponse = _searchBusiness.GetNewClub(lId, CustomerId, null);
                Response.FilteredClubModel = dbResponse.MapObjects<SearchFilterClubDetailModel>();
            }
            else
            {
                var dbRequest = new ClubPreferenceFilterRequest
                {
                    LocationId = lId,
                    SearchFilter = SearchFilter,
                    ClubCategory = !string.IsNullOrEmpty(ClubCategory) ? string.Join(",", ClubCategory.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    Price = !string.IsNullOrEmpty(Price) ? string.Join(",", Price.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    Shift = string.IsNullOrEmpty(Shift) ? string.Empty : Shift.DecryptParameter(),
                    Time = !string.IsNullOrEmpty(Time) ? string.Join(",", Time.Split(',').Select(x => x.DecryptParameter())).Trim(',') : null,
                    ClubAvailability = !string.IsNullOrEmpty(ClubAvailability) ? string.Join(",", ClubAvailability.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    CustomerId = CustomerId
                };
                var dbResponse = _searchBusiness.ClubPreferenceFilter(dbRequest);
                Response.FilteredClubModel = dbResponse.MapObjects<SearchFilterClubDetailModel>();
            }
            Response.FilteredClubModel.ForEach(x =>
            {
                x.ClubId = x.ClubId.EncryptParameter();
                x.ClubLocationId = x.ClubLocationId.EncryptParameter();
                x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo);
                x.HostGalleryImage = x.HostGalleryImage.Select(y => ImageHelper.ProcessedImage(y)).ToList();
            });

            ViewBag.LocationId = LocationId;
            ViewBag.LocationLabel = DDLHelper.GetValueForKey(DDLHelper.LoadDropdownList("1", lId), LocationId);
            return View(Response);
        }

        [HttpGet]
        public ActionResult DateTimeFilter(string LocationId, string Date, string Time, string NoOfPeople, string ResultType = "", string FilteredTime = "")
        {
            ViewBag.ActionPageName = "SearchFilter";
            var lId = !string.IsNullOrEmpty(LocationId) ? LocationId.DecryptParameter() : null;
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var Response = new ClubSearchResultModel();
            var clubRecommendationDBResponse = _searchBusinessOld.GetRecommendedClub(lId);
            Response.RecommendedClubModel = clubRecommendationDBResponse.MapObjects<ClubRecommendationListModel>();
            Response.RecommendedClubModel.ForEach(x =>
            {
                x.LocationId = x.LocationId.EncryptParameter();
                x.ClubId = x.ClubId.EncryptParameter();
                x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo);
            });
            var filterDate = DateTime.Today.ToString("yyyy/MM/dd");
            if (DateTime.TryParse(Date, out DateTime date))
            {
                filterDate = Date;
            }
            var dbRequest = new ClubDateTimeAndOtherFilterRequest
            {
                LocationId = lId,
                Date = filterDate,
                Time = string.IsNullOrEmpty(Time) ? string.Empty : Time.DecryptParameter(),
                NoOfPeople = string.IsNullOrEmpty(NoOfPeople) ? string.Empty : NoOfPeople.DecryptParameter(),
                CustomerId = CustomerId,
                ResultType = ResultType?.DecryptParameter() ?? string.Empty,
                FilteredTime = FilteredTime.Trim()

            };
            var dbResponse = _searchBusiness.ClubFilterViewDateTimeAndOthers(dbRequest);
            Response.FilteredClubModel = dbResponse.MapObjects<SearchFilterClubDetailModel>();
            Response.FilteredClubModel.ForEach(x =>
            {
                x.ClubId = x.ClubId.EncryptParameter();
                x.ClubLocationId = x.ClubLocationId.EncryptParameter();
                x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo);
                x.HostGalleryImage = x.HostGalleryImage.Select(y => ImageHelper.ProcessedImage(y)).ToList();
            });
            ViewBag.LocationId = LocationId;
            ViewBag.LocationLabel = DDLHelper.GetValueForKey(DDLHelper.LoadDropdownList("1", lId), LocationId);
            ViewBag.Date = filterDate;
            ViewBag.Time = string.IsNullOrEmpty(Time) ? string.Empty : Time.DecryptParameter();
            ViewBag.NoOfPeople = string.IsNullOrEmpty(NoOfPeople) ? string.Empty : NoOfPeople.DecryptParameter();
            return View("ClubSearchResult", Response);
        }

        [HttpGet]
        public ActionResult HostSearchResult(string LocationId, string SearchFilter, string Height, string Age, string BloodType, string ConstellationGroup, string Occupation, bool NewHost = false)
        {
            ViewBag.ActionPageName = "SearchFilter";
            var Response = new HostSearchResultModel();
            Response.HostRecommendationModel = new List<DashboardV2HostDetailModel>();
            var lId = !string.IsNullOrEmpty(LocationId) ? LocationId.DecryptParameter() : null;
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var hostRecommendationDBResponse = _oldDashboardBusiness.GetRecommendedHost(lId, CustomerId);
            var HostRecommendationModel = new List<DashboardV2HostDetailModel>();
            foreach (var item in hostRecommendationDBResponse)
            {
                Response.HostRecommendationModel.Add(new DashboardV2HostDetailModel
                {
                    ClubId = item.ClubId.EncryptParameter(),
                    ClubNameEnglish = item.ClubNameEnglish,
                    ClubNameJapanese = item.ClubNameJapanese,
                    ClubLogo = ImageHelper.ProcessedImage(item.ClubLogo),
                    HostId = item.HostId.EncryptParameter(),
                    HostNameEnglish = item.HostName,
                    HostNameJapanese = item.HostNameJapanese,
                    HostLogo = ImageHelper.ProcessedImage(item.HostImage),
                    ClubLocationId = item.LocationId.EncryptParameter(),
                    IsBookmarked = item.IsBookmarked
                });
            }
            if (NewHost)
            {
                var dbHostResponse = _dashboardBusiness.GetNewHost(lId, CustomerId,null);
                Response.FilteredHostModel = dbHostResponse.MapObjects<DashboardV2HostDetailModel>();
            }
            else
            {
                var dbRequest = new HostPreferenceFilterRequest
                {
                    LocationId = lId,
                    SearchFilter = SearchFilter,
                    Height = !string.IsNullOrEmpty(Height) ? string.Join(",", Height.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    Age = !string.IsNullOrEmpty(Age) ? string.Join(",", Age.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    BloodType = !string.IsNullOrEmpty(BloodType) ? string.Join(",", BloodType.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    ConstellationGroup = !string.IsNullOrEmpty(ConstellationGroup) ? string.Join(",", ConstellationGroup.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    Occupation = (string.IsNullOrEmpty(Occupation) || Occupation.Trim() == "0") ? string.Empty : Occupation.DecryptParameter(),
                    CustomerId = CustomerId
                };
                var dbHostResponse = _searchBusiness.HostPreferenceFilter(dbRequest);
                Response.FilteredHostModel = dbHostResponse.MapObjects<DashboardV2HostDetailModel>();
            }
            Response.FilteredHostModel.ForEach(x =>
            {
                x.ClubId = x.ClubId.EncryptParameter();
                x.HostId = x.HostId.EncryptParameter();
                x.ClubLocationId = x.ClubLocationId.EncryptParameter();
                x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo);
                x.HostLogo = ImageHelper.ProcessedImage(x.HostLogo);
            });
            ViewBag.LocationId = LocationId;
            ViewBag.LocationLabel = DDLHelper.GetValueForKey(DDLHelper.LoadDropdownList("1", lId), LocationId);
            return View(Response);
        }

        #region Club/Host Search filter V2 
        [HttpGet]
        public ActionResult Index(string LocationId)
        {
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var lId = !string.IsNullOrEmpty(LocationId) ? LocationId.DecryptParameter() : string.Empty;
            var Response = new PreferenceFilterModel();
            Response.LocationModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("1"));
            Response.AgeModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("7"));
            Response.ConstellationModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("8"));
            Response.BloodTypeModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("5"));
            Response.ClubAvailabilityModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("4"));
            Response.ClubCategoryModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("2"));
            Response.HeightModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("6"));
            Response.PlanPriceModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("3"));
            Response.OccupationModel = DDLHelper.ConvertDictionaryToList(DDLHelper.LoadDropdownList("9"), true);
            var HostConstellationGroup = _commonBusiness.GetDDL("023");
            HostConstellationGroup.ForEach(x => x.Value = x.Value.EncryptParameter());
            ViewBag.HostConstellationGroup = HostConstellationGroup;
            var dbClubResponse = _dashboardBusiness.GetNewClub(lId, CustomerId, "1");
            Response.ClubModel = dbClubResponse.MapObjects<DashboardV2ClubDetailModel>();
            Response.ClubModel.ForEach(x => { x.ClubId = x.ClubId.EncryptParameter(); x.ClubLocationId = x.ClubLocationId.EncryptParameter(); x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo); });
            var dbHostResponse = _dashboardBusiness.GetNewHost(lId, CustomerId,null, "1");
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
                    //CurrentUrl = CurrentUrl + "/LocationManagement/ClubDetail_V2";
                    if (!CurrentUrl.Contains("/LocationManagement/ClubDetail_V2"))
                        CurrentUrl += "/LocationManagement/ClubDetail_V2";
                    var parameters = new List<string>();
                    if (!string.IsNullOrEmpty(item.LocationId))
                        parameters.Add($"LocationId={item.LocationId.EncryptParameter()}");

                    if (!string.IsNullOrEmpty(item.ClubId))
                        parameters.Add($"ClubId={item.ClubId.EncryptParameter()}");

                    string queryString = string.Join("&", parameters);

                    StringBuilder urlBuilder = new StringBuilder(CurrentUrl);
                    urlBuilder.Append(CurrentUrl.Contains("?") ? "&" : "?");
                    urlBuilder.Append(queryString);

                    mappedItem.URL = urlBuilder.ToString();
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
            ViewBag.ClubDetailMapData = mappedData;
            ViewBag.LocationId = LocationId;
            return View(Response);
        }

        [HttpGet]
        public ActionResult ClubFilter(ClubSearchFilterRequestModel Request, bool NewClub = false)
        {
            var lId = !string.IsNullOrEmpty(Request.LocationId) ? Request.LocationId.DecryptParameter() : null;
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var Response = new ClubSearchResultModel();
            var clubRecommendationDBResponse = _searchBusinessOld.GetRecommendedClub(lId);
            Response.RecommendedClubModel = clubRecommendationDBResponse.MapObjects<ClubRecommendationListModel>();
            Response.RecommendedClubModel.ForEach(x =>
            {
                x.LocationId = x.LocationId.EncryptParameter();
                x.ClubId = x.ClubId.EncryptParameter();
                x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo);
            });
            if (NewClub)
            {
                var dbResponse = _searchBusiness.GetNewClub(lId, CustomerId,null);
                Response.FilteredClubModel = dbResponse.MapObjects<SearchFilterClubDetailModel>();
            }
            else
            {
                var dbRequest = new ClubPreferenceFilterRequest
                {
                    LocationId = lId,
                    SearchFilter = Request.SearchFilter,
                    ClubCategory = !string.IsNullOrEmpty(Request.ClubCategory) ? string.Join(",", Request.ClubCategory.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    Price = !string.IsNullOrEmpty(Request.Price) ? string.Join(",", Request.Price.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    Shift = string.IsNullOrEmpty(Request.Shift) ? string.Empty : Request.Shift.DecryptParameter(),
                    Time = !string.IsNullOrEmpty(Request.Time) ? string.Join(",", Request.Time.Split(',').Select(x => x.DecryptParameter()).Where(x => x != null)).Trim(',')
                            : string.Empty,
                    ClubAvailability = !string.IsNullOrEmpty(Request.ClubAvailability) ? string.Join(",", Request.ClubAvailability.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    CustomerId = CustomerId
                };
                var dbResponse = _searchBusiness.ClubPreferenceFilter(dbRequest);
                Response.FilteredClubModel = dbResponse.MapObjects<SearchFilterClubDetailModel>();
            }
            Response.FilteredClubModel.ForEach(x =>
            {
                x.ClubId = x.ClubId.EncryptParameter();
                x.ClubLocationId = x.ClubLocationId.EncryptParameter();
                x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo);
                x.HostGalleryImage = x.HostGalleryImage.Select(y => ImageHelper.ProcessedImage(y)).ToList();
            });
            return View(Response);
        }

        [HttpGet]
        public ActionResult HostFilter(HostSearchFilterRequestModel Request, bool NewHost = false, int StartIndex = 0, int PageSize = 12)
        {
            var Response = new HostSearchResultModel();
            Response.HostRecommendationModel = new List<DashboardV2HostDetailModel>();
            var lId = !string.IsNullOrEmpty(Request.LocationId) ? Request.LocationId.DecryptParameter() : null;
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var hostRecommendationDBResponse = _searchBusinessOld.GetRecommendedHost(lId, CustomerId);
            var HostRecommendationModel = new List<DashboardV2HostDetailModel>();
            foreach (var item in hostRecommendationDBResponse)
            {
                Response.HostRecommendationModel.Add(new DashboardV2HostDetailModel
                {
                    ClubId = item.ClubId.EncryptParameter(),
                    ClubNameEnglish = item.ClubNameEnglish,
                    ClubNameJapanese = item.ClubNameJapanese,
                    ClubLogo = ImageHelper.ProcessedImage(item.ClubLogo),
                    HostId = item.HostId.EncryptParameter(),
                    HostNameEnglish = item.HostName,
                    HostNameJapanese = item.HostNameJapanese,
                    HostLogo = ImageHelper.ProcessedImage(item.HostImage),
                    ClubLocationId = item.LocationId.EncryptParameter(),
                    IsBookmarked = item.IsBookmarked
                });
            }
            if (NewHost)
            {
                var dbHostResponse = _dashboardBusiness.GetNewHost(lId, CustomerId, null);
                Response.FilteredHostModel = dbHostResponse.MapObjects<DashboardV2HostDetailModel>();
            }
            else
            {

                var dbRequest = new HostPreferenceFilterRequest
                {
                    LocationId = lId,
                    SearchFilter = Request.SearchFilter,
                    Height = !string.IsNullOrEmpty(Request.Height) ? string.Join(",", Request.Height.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    Age = !string.IsNullOrEmpty(Request.Age) ? string.Join(",", Request.Age.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    BloodType = !string.IsNullOrEmpty(Request.BloodType) ? string.Join(",", Request.BloodType.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    ConstellationGroup = !string.IsNullOrEmpty(Request.ConstellationGroup) ? string.Join(",", Request.ConstellationGroup.Split(',').Select(x => x.DecryptParameter())).Trim(',') : string.Empty,
                    Occupation = (string.IsNullOrEmpty(Request.Occupation) || Request.Occupation.Trim() == "0") ? string.Empty : Request.Occupation.DecryptParameter(),
                    CustomerId = CustomerId,
                    Type = "1",
                    Skip = StartIndex,
                    Take = PageSize,
                };
                var dbHostResponse = _searchBusiness.HostPreferenceFilter(dbRequest);
                Response.FilteredHostModel = dbHostResponse.MapObjects<DashboardV2HostDetailModel>();
            }
            Response.FilteredHostModel.ForEach(x =>
            {
                x.ClubId = x.ClubId.EncryptParameter();
                x.HostId = x.HostId.EncryptParameter();
                x.ClubLocationId = x.ClubLocationId.EncryptParameter();
                x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo);
                x.HostLogo = ImageHelper.ProcessedImage(x.HostLogo);
            });
            ViewBag.LocationId = Request.LocationId;
            Response.RequestModel = Request.MapObject<HostSearchFilterRequestModel>();
            ViewBag.StartIndex = StartIndex;
            ViewBag.TotalRecords = (Response.FilteredHostModel.Count > 0 && !string.IsNullOrEmpty(Response.FilteredHostModel.FirstOrDefault().TotalRecords)) ? Convert.ToInt32(Response.FilteredHostModel.FirstOrDefault().TotalRecords) : 0;
            return View(Response);
        }
        #endregion
    }
}