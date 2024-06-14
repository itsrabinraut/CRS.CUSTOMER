﻿using CRS.CUSTOMER.APPLICATION.Helper;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.Dashboard;
using CRS.CUSTOMER.APPLICATION.Models.LocationManagement;
using CRS.CUSTOMER.APPLICATION.Models.LocationManagementV2;
using CRS.CUSTOMER.BUSINESS.Dashboard;
using CRS.CUSTOMER.BUSINESS.LocationManagement;
using CRS.CUSTOMER.BUSINESS.RecommendedClubHost;
using CRS.CUSTOMER.SHARED;
using CRS.CUSTOMER.SHARED.RecommendedClubHost;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    public class LocationManagementV2Controller : CustomController
    {
        private readonly Dictionary<string, string> _locationHelper = ApplicationUtilities.MapJsonDataToDictionaryViaKeyName("URLManagementConfigruation", "Location");
        private readonly IDashboardBusiness _dashboardBuss;
        private readonly IRecommendedClubHostBusiness _recommendedClubHostBuss;
        private readonly ILocationManagementBusiness _business;

        public LocationManagementV2Controller(IDashboardBusiness dashboardBuss, IRecommendedClubHostBusiness recommendedClubHostBuss, ILocationManagementBusiness business)
        {
            _dashboardBuss = dashboardBuss;
            _recommendedClubHostBuss = recommendedClubHostBuss;
            _business = business;
        }

        [HttpGet, Route("area/{prefectures}/{area}")]
        public ActionResult Index(string prefectures, string area, LocationV2ClubHostRequestModel request)
        {
            var response = new LocationV2ClubHostModel();
            var culture = Request.Cookies["culture"]?.Value;
            culture = string.IsNullOrEmpty(culture) ? "ja" : culture;
            var PrefecturesArea = $"/{prefectures}/{area}";
            ViewBag.ActionPageName = "Dashboard";
            var locationId = ApplicationUtilities.GetKeyValueFromDictionary(_locationHelper, PrefecturesArea);
            var agentId = ApplicationUtilities.GetSessionValue("AgentId")?.ToString()?.DecryptParameter();
            var bannerServiceResp = _dashboardBuss.GetBanners();
            if (bannerServiceResp != null && bannerServiceResp.Count > 0)
            {
                response.Banners = bannerServiceResp.MapObjects<BannersModel>();
                response.Banners.ForEach(x =>
                {
                    x.BannerId = x.BannerId?.EncryptParameter();
                    x.BannerImage = ImageHelper.ProcessedImage(x.BannerImage);
                });
            }
            var recommendedClubDBRequest = new RecommendedClubRequestCommon()
            {
                PositionId = request.GroupId.ToString(),
                LocationId = locationId,
                CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter()
            };
            var dbClubResponse = _recommendedClubHostBuss.GetRecommendedClub(recommendedClubDBRequest);
            response.ClubListModel = dbClubResponse.MapObjects<LocationV2ClubListModel>();
            foreach (var item in response.ClubListModel)
            {
                item.ClubId = item.ClubId.EncryptParameter();
                item.LocationId = item.LocationId.EncryptParameter();
                item.ClubLogo = ImageHelper.ProcessedImage(item.ClubLogo);
                item.ClubCoverPhoto = ImageHelper.ProcessedImage(item.ClubCoverPhoto);
                item.HostGalleryImage = item.HostGalleryImage.Select(x => ImageHelper.ProcessedImage(x)).ToList();
            }
            if (response.ClubListModel != null && response.ClubListModel.Count > 0)
            {
                var recommendedHostDBRequest = new RecommendedHostRequestCommon()
                {
                    PositionId = request.GroupId.ToString(),
                    LocationId = locationId,
                    CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter()
                };
                var dbHostResponse = _recommendedClubHostBuss.GetRecommendedHost(recommendedHostDBRequest);
                response.HostListModel = dbHostResponse.MapObjects<LocationV2HostListModel>();
                foreach (var item in response.HostListModel)
                {
                    item.ClubId = item.ClubId.EncryptParameter();
                    item.LocationId = item.LocationId.EncryptParameter();
                    item.HostId = item.HostId.EncryptParameter();
                    item.HostImage = ImageHelper.ProcessedImage(item.HostImage);
                    item.ClubLogo = ImageHelper.ProcessedImage(item.ClubLogo);
                }
                request.ClubId = recommendedHostDBRequest.ClubId?.EncryptParameter();
            }
            ViewBag.LocationId = PrefecturesArea;
            var getTotalPage = _recommendedClubHostBuss.GetTotalRecommendedPageCount();
            ViewBag.TotalGroupCount = getTotalPage >= 0 ? getTotalPage : 0;
            response.RequestModel = request.MapObject<LocationV2ClubHostRequestModel>();
            ViewBag.RenderValue = !string.IsNullOrEmpty(request.RenderId) ? request.RenderId : null;
            return View(response);
        }

        [HttpGet, Route("area/{prefectures}/{area}/hostclub/{ClubId}")]
        public ActionResult ClubDetail(string prefectures, string area, string ClubId, string ScheduleFilterDate = null)
        {
            var culture = Request.Cookies["culture"]?.Value;
            culture = string.IsNullOrEmpty(culture) ? "ja" : culture;
            var PrefecturesArea = $"/{prefectures}/{area}";
            var locationId = ApplicationUtilities.GetKeyValueFromDictionary(_locationHelper, PrefecturesArea);
            string sFD = null;
            if (ScheduleFilterDate != null)
            {
                DateTime date = DateTime.ParseExact(ScheduleFilterDate, "yyyy年 M月", null);
                sFD = date.ToString("yyyy/MM");
            }
            if (string.IsNullOrEmpty(ClubId) || string.IsNullOrEmpty(locationId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.WARNING,
                    Message = "Invalid Details",
                    Title = NotificationMessage.WARNING.ToString()
                });
                return Redirect("/");
            }
            string agentId = ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter();
            var clubDetailResp = _business.GetClubDetailById(ClubId, agentId);
            var responseModel = clubDetailResp.MapObject<Models.LocationManagementV2.ClubDetailModel>();
            responseModel.ClubId = responseModel.ClubId.EncryptParameter();
            responseModel.LocationId = responseModel.LocationId.EncryptParameter();
            var dbHostList = _business.GetHostList(locationId, ClubId);
            responseModel.HostListModels = dbHostList.MapObjects<LocationV2HostListModel>();
            foreach (var item in responseModel.HostListModels)
            {
                item.ClubId = item.ClubId.EncryptParameter();
                item.HostId = item.HostId.EncryptParameter();
                item.LocationId = item.LocationId.EncryptParameter();
                item.HostImage = ImageHelper.ProcessedImage(item.HostImage);
            }
            var dbTopHostList = _business.GetHostList(locationId, ClubId, agentId, "trhl");
            responseModel.TopHostListModels = dbTopHostList.MapObjects<LocationV2HostListModel>();
            foreach (var item in responseModel.TopHostListModels)
            {
                item.ClubId = item.ClubId.EncryptParameter();
                item.HostId = item.HostId.EncryptParameter();
                item.LocationId = item.LocationId.EncryptParameter();
                item.HostImage = ImageHelper.ProcessedImage(item.HostImage);
            }
            var clubGalleryImageDBResponse = _business.GetClubGalleryImage(responseModel.ClubId.DecryptParameter(), "A");
            if (clubGalleryImageDBResponse != null && clubGalleryImageDBResponse.Count > 0)
            {
                responseModel.ClubGalleryImageList = clubGalleryImageDBResponse;
                responseModel.ClubGalleryImageList.ForEach(x => x = ImageHelper.ProcessedImage(x));
            }
            else responseModel.ClubGalleryImageList = new List<string>();
            responseModel.ClubCoverPhoto = ImageHelper.ProcessedImage(responseModel.ClubCoverPhoto);
            responseModel.ClubLogo = ImageHelper.ProcessedImage(responseModel.ClubLogo);
            responseModel.ClubWeeklyScheduleList.ForEach(x => x.DayLabel = (!string.IsNullOrEmpty(culture) && culture == "en") ? x.EnglishDay : x.JapaneseDay);
            var reviewDBResponse = _business.GetClubReviewAndRatings(ClubId);
            if (reviewDBResponse != null && reviewDBResponse.Count > 0)
            {
                responseModel.ClubReviewsModel = reviewDBResponse.MapObjects<GetClubReviewsModel>();
                foreach (var item in responseModel.ClubReviewsModel)
                {
                    if (!string.IsNullOrEmpty(item.CustomerImage))
                    {
                        item.CustomerImage = ImageHelper.ProcessedImage(item.CustomerImage);
                    }
                    else
                    {
                        item.CustomerImage = "";
                    }
                }
                foreach (var item in responseModel.ClubReviewsModel)
                {
                    item.GetClubReviewRemarkList.ForEach(x => x.Remark = (!string.IsNullOrEmpty(culture) && culture == "en") ? x.EnglishRemark : x.JapaneseRemark);
                }
                foreach (var item in responseModel.ClubReviewsModel)
                {
                    foreach (var item_sec in item.GetClubReviewHostList)
                    {
                        if (!string.IsNullOrEmpty(item_sec.HostImage))
                        {
                            item_sec.HostImage = ImageHelper.ProcessedImage(item_sec.HostImage);
                        }
                        else
                        {
                            item_sec.HostImage = "";
                        }
                    }
                }
            }
            var dbNoticeResponseInfo = _business.GetNoticeByClubId(ClubId);
            responseModel.GetNoticeByClubId = dbNoticeResponseInfo.MapObjects<Models.LocationManagementV2.NoticeModel>();
            foreach (var notice_item in responseModel.GetNoticeByClubId)
            {
                DateTime date = DateTime.ParseExact(notice_item.EventDate, "yyyy年MM月dd日", CultureInfo.InvariantCulture);
                notice_item.Day = date.ToString("ddd");
            }
            var dbBasicInfoResponse = _business.GetClubBasicInformation(ClubId);
            responseModel.GetClubBasicInformation = dbBasicInfoResponse.MapObject<Models.LocationManagementV2.ClubBasicInformationModel>();
            if (!string.IsNullOrEmpty(responseModel.GetClubBasicInformation.InstagramLink) && responseModel.GetClubBasicInformation.InstagramLink != "#")
            {
                if (!responseModel.GetClubBasicInformation.InstagramLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) responseModel.GetClubBasicInformation.InstagramLink = "https://" + responseModel.GetClubBasicInformation.InstagramLink;
            }
            if (!string.IsNullOrEmpty(responseModel.GetClubBasicInformation.TwitterLink) && responseModel.GetClubBasicInformation.TwitterLink != "#")
            {
                if (!responseModel.GetClubBasicInformation.TwitterLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) responseModel.GetClubBasicInformation.TwitterLink = "https://" + responseModel.GetClubBasicInformation.TwitterLink;
            }
            if (!string.IsNullOrEmpty(responseModel.GetClubBasicInformation.TiktokLink) && responseModel.GetClubBasicInformation.TiktokLink != "#")
            {
                if (!responseModel.GetClubBasicInformation.TiktokLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) responseModel.GetClubBasicInformation.TiktokLink = "https://" + responseModel.GetClubBasicInformation.TiktokLink;
            }
            if (!string.IsNullOrEmpty(responseModel.GetClubBasicInformation.LineNumber) && responseModel.GetClubBasicInformation.LineNumber != "#")
            {
                if (!responseModel.GetClubBasicInformation.LineNumber.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) responseModel.GetClubBasicInformation.LineNumber = "https://" + responseModel.GetClubBasicInformation.LineNumber;
            }
            if (!string.IsNullOrEmpty(responseModel.GetClubBasicInformation.WebsiteLink) && responseModel.GetClubBasicInformation.WebsiteLink != "#")
            {
                if (!responseModel.GetClubBasicInformation.WebsiteLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) responseModel.GetClubBasicInformation.WebsiteLink = "https://" + responseModel.GetClubBasicInformation.WebsiteLink;
            }
            var dbAllNoticeResponse = _business.GetAllNoticeTabList(ClubId);
            responseModel.GetAllNoticeTabList = dbAllNoticeResponse.MapObjects<Models.LocationManagementV2.AllNoticeModel>();
            foreach (var allNotice_item in responseModel.GetAllNoticeTabList)
            {
                // Parse the date string using the specified format and culture
                DateTime date = DateTime.ParseExact(allNotice_item.EventDate, "yyyy年MM月dd日", CultureInfo.InvariantCulture);
                // Get the day name
                allNotice_item.DayName = date.ToString("ddd");

            }
            var dbScheduleResponse = _business.GetAllScheduleTabList(ClubId, sFD);
            responseModel.GetAllScheduleTabList = dbScheduleResponse.MapObjects<Models.LocationManagementV2.AllScheduleModel>();
            foreach (var item_schedule in responseModel.GetAllScheduleTabList)
            {
                DateTime date = DateTime.ParseExact(item_schedule.ScheduleDate, "yyyy年MM月dd日", null);
                string formattedDayOfWeek = date.ToString("dd");

                // Get the day name (e.g., "Sunday")
                string dayName = date.ToString("ddd");
                item_schedule.Day = formattedDayOfWeek;
                item_schedule.DayName = dayName;
                if (!string.IsNullOrEmpty(item_schedule.ScheduleImage))
                {
                    item_schedule.ScheduleImage = ImageHelper.ProcessedImage(item_schedule.ScheduleImage, false);
                }
                else
                {
                    item_schedule.ScheduleImage = "";
                }
            }
            responseModel.GetScheduleDDL = GetScheduleList();
            var dbPlanDetailRes = _business.GetPlanDetail(ClubId);
            responseModel.GetPlanDetailList = dbPlanDetailRes.MapObjects<Models.LocationManagementV2.PlanDetailModel>();
            var groupedResults = responseModel.GetPlanDetailList
            .GroupBy(planDetail => planDetail.PlanName)
            .Select(group => new
            {
                PlanName = group.Key,
                GetPlanGroupDetail = group.ToList()
            })
            .ToList();
            ViewBag.PlanGroup = groupedResults.MapObjects<Models.LocationManagementV2.PlanGroup>();
            ViewBag.PlanGroup1 = groupedResults.MapObjects<Models.LocationManagementV2.PlanGroup>();
            ViewBag.ActionPageName = "ClubHostDetailNavMenu";
            ViewBag.FileLocationPath = "";
            ViewBag.SFilterDate = ScheduleFilterDate;
            ViewBag.ClubId = ClubId;
            ViewBag.LocationId = PrefecturesArea;
            return View(responseModel);
        }

        private List<Models.LocationManagementV2.ScheduleDDLModel> GetScheduleList()
        {
            List<Models.LocationManagementV2.ScheduleDDLModel> scheduleList = new List<Models.LocationManagementV2.ScheduleDDLModel>();

            DateTime currentDate = DateTime.Now;
            DateTime endDate = currentDate.AddMonths(3);

            while (currentDate <= endDate)
            {
                scheduleList.Add(new Models.LocationManagementV2.ScheduleDDLModel
                {
                    Value = currentDate.ToString("yyyy年 M月", CultureInfo.InvariantCulture),
                    Text = currentDate.ToString("yyyy年 M月", CultureInfo.InvariantCulture)
                });

                currentDate = currentDate.AddMonths(1);
            }

            return scheduleList;
        }
    }
}