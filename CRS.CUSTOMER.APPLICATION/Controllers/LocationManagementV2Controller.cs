using CRS.CUSTOMER.APPLICATION.Helper;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.Dashboard;
using CRS.CUSTOMER.APPLICATION.Models.LocationManagement;
using CRS.CUSTOMER.APPLICATION.Models.LocationManagementV2;
using CRS.CUSTOMER.BUSINESS.CommonManagement;
using CRS.CUSTOMER.BUSINESS.Dashboard;
using CRS.CUSTOMER.BUSINESS.LocationManagement;
using CRS.CUSTOMER.BUSINESS.RecommendedClubHost;
using CRS.CUSTOMER.SHARED;
using CRS.CUSTOMER.SHARED.RecommendedClubHost;
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
        private readonly Dictionary<string, string> _locationJapaneseLabelHelper = ApplicationUtilities.MapJsonDataToDictionaryViaKeyName("URLManagementConfigruation", "LocationJapaneseLabel");
        private readonly IDashboardBusiness _dashboardBuss;
        private readonly IRecommendedClubHostBusiness _recommendedClubHostBuss;
        private readonly ILocationManagementBusiness _business;
        private readonly ICommonManagementBusiness _commonBusiness;

        public LocationManagementV2Controller(IDashboardBusiness dashboardBuss, IRecommendedClubHostBusiness recommendedClubHostBuss, ILocationManagementBusiness business, ICommonManagementBusiness commonBusiness)
        {
            _dashboardBuss = dashboardBuss;
            _recommendedClubHostBuss = recommendedClubHostBuss;
            _business = business;
            _commonBusiness = commonBusiness;
        }

        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        [HttpGet, Route("area/{prefectures}/{area}")]
        public ActionResult Index(string prefectures, string area, LocationV2ClubHostRequestModel request)
        {
            var response = new LocationV2ClubHostModel();
            var culture = Request.Cookies["culture"]?.Value;
            culture = string.IsNullOrEmpty(culture) ? "ja" : culture;
            var PrefecturesArea = $"/{prefectures}/{area}";
            var LocationJapaneseLabel = ApplicationUtilities.GetKeyValueFromDictionary(_locationJapaneseLabelHelper, PrefecturesArea);
            ViewBag.LocationJapaneseLabel = string.IsNullOrEmpty(LocationJapaneseLabel) ? PrefecturesArea : LocationJapaneseLabel;
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
            var groupWiseData = new List<GroupWisePaginationModelCommon>();
            if (!string.IsNullOrEmpty(locationId))
            {
                groupWiseData = _recommendedClubHostBuss.GetTotalRecommendedPageCount(locationId);
            }
            if (groupWiseData != null && groupWiseData.Count() > 0)
            {
                if (!string.IsNullOrEmpty(recommendedClubDBRequest.PositionId))
                {
                    recommendedClubDBRequest.PositionId = groupWiseData[(Convert.ToInt32(recommendedClubDBRequest.PositionId) - 1)].PositionId;
                }
            }

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
                if (groupWiseData != null && groupWiseData.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(recommendedHostDBRequest.PositionId))
                    {
                        recommendedHostDBRequest.PositionId = groupWiseData[(Convert.ToInt32(recommendedHostDBRequest.PositionId) - 1)].PositionId;
                    }
                }

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
            //var groupWiseData = _recommendedClubHostBuss.GetTotalRecommendedPageCount(locationId);
            if (groupWiseData != null && groupWiseData.Count() > 0)
            {
                ViewBag.TotalGroupCount = groupWiseData[0].TotalPages;
            }
            else
            {
                ViewBag.TotalGroupCount = 0;
            }

            response.RequestModel = request.MapObject<LocationV2ClubHostRequestModel>();
            ViewBag.RenderValue = !string.IsNullOrEmpty(request.RenderId) ? request.RenderId : null;
            var metaTagDBResponse = _commonBusiness.GetMetaTagInfo("1", locationId);
            ViewBag.MetaClubCount = metaTagDBResponse.Item1;
            ViewBag.MetaHostCount = metaTagDBResponse.Item2;
            return View(response);
        }

        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        [HttpGet, Route("area/{prefectures}/{area}/hostclub/{ClubId}/{target?}")]
        public ActionResult ClubDetail(string prefectures, string area, string ClubId, string target = "", string ScheduleFilterDate = null)
        {
            var PrefecturesArea = $"/{prefectures}/{area}";
            ViewBag.LocationId = PrefecturesArea;
            ViewBag.ClubId = ClubId;
            var LocationJapaneseLabel = ApplicationUtilities.GetKeyValueFromDictionary(_locationJapaneseLabelHelper, PrefecturesArea);
            ViewBag.LocationJapaneseLabel = string.IsNullOrEmpty(LocationJapaneseLabel) ? PrefecturesArea : LocationJapaneseLabel;
            var culture = Request.Cookies["culture"]?.Value;
            culture = string.IsNullOrEmpty(culture) ? "ja" : culture;
            string agentId = ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter();
            if (!string.IsNullOrEmpty(target) && (ApplicationUtilities.IsAlphanumeric(target) && !ApplicationUtilities.IsPureString(target)))
            {
                var dbResponse = _business.ViewHostDetailsV2(target, agentId);
                if (dbResponse != null)
                {
                    var ResponseModel = dbResponse.MapObject<ViewHostDetailModelV2>();
                    ResponseModel.ClubId = ResponseModel.ClubId.EncryptParameter();
                    ResponseModel.HostId = ResponseModel.HostId.EncryptParameter();
                    ResponseModel.LocationId = ResponseModel.LocationId.EncryptParameter();
                    ResponseModel.ClubLogo = ImageHelper.ProcessedImage(ResponseModel.ClubLogo);
                    if (!string.IsNullOrEmpty(ResponseModel.HostInstagramLink) && ResponseModel.HostInstagramLink != "#")
                    {
                        if (!ResponseModel.HostInstagramLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) ResponseModel.HostInstagramLink = "https://" + ResponseModel.HostInstagramLink;
                    }
                    if (!string.IsNullOrEmpty(ResponseModel.HostTwitterLink) && ResponseModel.HostTwitterLink != "#")
                    {
                        if (!ResponseModel.HostTwitterLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) ResponseModel.HostTwitterLink = "https://" + ResponseModel.HostTwitterLink;
                    }
                    if (!string.IsNullOrEmpty(ResponseModel.HostTiktokLink) && ResponseModel.HostTiktokLink != "#")
                    {
                        if (!ResponseModel.HostTiktokLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) ResponseModel.HostTiktokLink = "https://" + ResponseModel.HostTiktokLink;
                    }
                    if (!string.IsNullOrEmpty(ResponseModel.HostLine) && ResponseModel.HostLine != "#")
                    {
                        if (!ResponseModel.HostLine.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) ResponseModel.HostLine = "https://" + ResponseModel.HostLine;
                    }
                    if (ResponseModel.HostGalleryImageList != null && ResponseModel.HostGalleryImageList.Count > 0)
                    {
                        for (int i = 0; i < ResponseModel.HostGalleryImageList.Count; i++) ResponseModel.HostGalleryImageList[i] = ImageHelper.ProcessedImage(ResponseModel.HostGalleryImageList[i]);
                    }
                    ResponseModel.HostIdentityDetailsModel.ForEach(x => x.Label = (!string.IsNullOrEmpty(culture) && culture == "en") ? x.LabelEnglish : x.LabelJapanese);
                    ViewBag.ClubName = !string.IsNullOrEmpty(ResponseModel.ClubNameJapanese) ? ResponseModel.ClubNameJapanese : ResponseModel.ClubNameEnglish;
                    ViewBag.HostName = !string.IsNullOrEmpty(ResponseModel.HostNameJapanese) ? ResponseModel.HostNameJapanese : ResponseModel.HostNameEnglish;
                    return View("HostDetail", ResponseModel);
                }
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Something went wrong.",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            else
            {
                var locationId = ApplicationUtilities.GetKeyValueFromDictionary(_locationHelper, PrefecturesArea);
                string sFD = null;
                if (ScheduleFilterDate != null)
                {
                    DateTime date = DateTime.ParseExact(ScheduleFilterDate, "yyyy年 M月", null);
                    sFD = date.ToString("yyyy/MM");
                }
                //var cId = !string.IsNullOrEmpty(ClubId) ? ClubId.DecryptParameter() : null;
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
                var responseModel = new Models.LocationManagementV2.ClubDetailModel();
                var clubDetailResp = _business.GetClubDetailById("", agentId, ClubId);
                if (clubDetailResp.Code.Trim() != "0")
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = !string.IsNullOrEmpty(clubDetailResp.Message) ? clubDetailResp.Message : NotificationMessage.ERROR.ToString(),
                        Title = NotificationMessage.INFORMATION.ToString()
                    });
                    return Redirect("/");
                }
                responseModel = clubDetailResp.MapObject<Models.LocationManagementV2.ClubDetailModel>();
                responseModel.ClubId = responseModel.ClubId.EncryptParameter();
                responseModel.LocationId = responseModel.LocationId.EncryptParameter();
                if (!string.IsNullOrEmpty(responseModel.ClubCoverPhoto))
                    responseModel.ClubCoverPhoto = ImageHelper.ProcessedImage(responseModel.ClubCoverPhoto);
                responseModel.ClubLogo = ImageHelper.ProcessedImage(responseModel.ClubLogo);
                if (!string.IsNullOrEmpty(responseModel.LocationURL) && responseModel.LocationURL != "#")
                {
                    if (!responseModel.LocationURL.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) responseModel.LocationURL = "https://" + responseModel.LocationURL;
                }
                responseModel.ClubWeeklyScheduleList.ForEach(x => x.DayLabel = (!string.IsNullOrEmpty(culture) && culture == "en") ? x.EnglishDay : x.JapaneseDay);
                var cId = clubDetailResp.ClubId;
                ViewBag.target = target;
                var dbBasicInfoResponse = _business.GetClubBasicInformation(cId);
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
                if (!string.IsNullOrEmpty(responseModel.GetClubBasicInformation.LocationURL) && responseModel.GetClubBasicInformation.LocationURL != "#")
                {
                    if (!responseModel.GetClubBasicInformation.LocationURL.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) responseModel.GetClubBasicInformation.LocationURL = "https://" + responseModel.GetClubBasicInformation.LocationURL;
                }
                var ClubName = !string.IsNullOrEmpty(responseModel.ClubNameJp) ? responseModel.ClubNameJp : responseModel.ClubNameEng;
                #region TAB
                #region TAB 2
                if (!string.IsNullOrEmpty(target) && target.Trim() == "host")
                {
                    ViewBag.target = "host";
                    var dbHostList = _business.GetHostList(locationId, cId);
                    responseModel.HostListModels = dbHostList.MapObjects<LocationV2HostListModel>();
                    foreach (var item in responseModel.HostListModels)
                    {
                        item.ClubId = item.ClubId.EncryptParameter();
                        item.HostId = item.HostId.EncryptParameter();
                        item.LocationId = item.LocationId.EncryptParameter();
                        item.HostImage = ImageHelper.ProcessedImage(item.HostImage);
                    }
                    var dbTopHostList = _business.GetHostList(locationId, cId, agentId, "trhl");
                    responseModel.TopHostListModels = dbTopHostList.MapObjects<LocationV2HostListModel>();
                    foreach (var item in responseModel.TopHostListModels)
                    {
                        item.ClubId = item.ClubId.EncryptParameter();
                        item.HostId = item.HostId.EncryptParameter();
                        item.LocationId = item.LocationId.EncryptParameter();
                        item.HostImage = ImageHelper.ProcessedImage(item.HostImage);
                    }
                    PopulateMetaTagInfo("host", ClubName, locationId, PrefecturesArea, responseModel.ClubId.DecryptParameter(), responseModel.ClubDescription);
                }
                #endregion
                #region TAB 3
                else if (!string.IsNullOrEmpty(target) && target.Trim() == "review")
                {
                    ViewBag.target = "review";
                    var reviewDBResponse = _business.GetClubReviewAndRatings(cId);
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
                                item.CustomerImage = "https://s3.ap-northeast-1.amazonaws.com/dev-assets.hoslog.jp/ADMIN/no-image.jpeg";
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
                    PopulateMetaTagInfo("review", ClubName, locationId, PrefecturesArea, responseModel.ClubId.DecryptParameter(), responseModel.ClubDescription);
                }
                #endregion
                #region TAB 4
                else if (!string.IsNullOrEmpty(target) && target.Trim() == "gallery")
                {
                    ViewBag.target = "gallery";
                    var clubGalleryImageDBResponse = _business.GetClubGalleryImage(responseModel.ClubId.DecryptParameter(), "A");
                    if (clubGalleryImageDBResponse != null && clubGalleryImageDBResponse.Count > 0)
                    {
                        responseModel.ClubGalleryImageList = clubGalleryImageDBResponse.Select(x => ImageHelper.ProcessedImage(x)).ToList();
                    }
                    else responseModel.ClubGalleryImageList = new List<string>();
                    PopulateMetaTagInfo("gallery", ClubName, locationId, PrefecturesArea, responseModel.ClubId.DecryptParameter(), responseModel.ClubDescription);
                }
                #endregion
                #region TAB 5
                else if (!string.IsNullOrEmpty(target) && target.Trim() == "schedule")
                {
                    ViewBag.target = "schedule";
                    responseModel.GetScheduleDDL = GetScheduleList();
                    var dbScheduleResponse = _business.GetAllScheduleTabList(cId, sFD);
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
                    PopulateMetaTagInfo("schedule", ClubName, locationId, PrefecturesArea, responseModel.ClubId.DecryptParameter(), responseModel.ClubDescription);
                }
                #endregion
                #region TAB 6
                else if (!string.IsNullOrEmpty(target) && target.Trim() == "notice")
                {
                    ViewBag.target = "notice";
                    var dbAllNoticeResponse = _business.GetAllNoticeTabList(cId);
                    responseModel.GetAllNoticeTabList = dbAllNoticeResponse.MapObjects<Models.LocationManagementV2.AllNoticeModel>();
                    foreach (var allNotice_item in responseModel.GetAllNoticeTabList)
                    {
                        // Parse the date string using the specified format and culture
                        DateTime date = DateTime.ParseExact(allNotice_item.EventDate, "yyyy年MM月dd日", CultureInfo.InvariantCulture);
                        // Get the day name
                        allNotice_item.DayName = date.ToString("ddd");
                    }
                    PopulateMetaTagInfo("notice", ClubName, locationId, PrefecturesArea, responseModel.ClubId.DecryptParameter(), responseModel.ClubDescription);
                }
                #endregion
                #region TAB 1
                else
                {
                    // var dbTopHostList = _business.GetHostList(locationId, cId, agentId, "trhl");
                    ViewBag.target = "";
                    //responseModel.TopHostListModels = dbTopHostList.MapObjects<LocationV2HostListModel>();
                    //foreach (var item in responseModel.TopHostListModels)
                    //{
                    //    item.ClubId = item.ClubId.EncryptParameter();
                    //    item.HostId = item.HostId.EncryptParameter();
                    //    item.LocationId = item.LocationId.EncryptParameter();
                    //    item.HostImage = ImageHelper.ProcessedImage(item.HostImage);
                    //}

                    var dbNoticeResponseInfo = _business.GetNoticeByClubId(cId);
                    responseModel.GetNoticeByClubId = dbNoticeResponseInfo.MapObjects<Models.LocationManagementV2.NoticeModel>();
                    foreach (var notice_item in responseModel.GetNoticeByClubId)
                    {
                        DateTime date = DateTime.ParseExact(notice_item.EventDate, "yyyy年MM月dd日", CultureInfo.InvariantCulture);
                        notice_item.Day = date.ToString("ddd");
                    }

                    var dbPlanDetailRes = _business.GetPlanDetail(cId);
                    responseModel.GetPlanDetailList = dbPlanDetailRes.MapObjects<Models.LocationManagementV2.PlanDetailModel>();
                    var groupedResults = responseModel.GetPlanDetailList
                    .GroupBy(planDetail => planDetail.PlanName)
                    .Where(group => group.Any(detail => detail.Label == "Status")) // Ensure the group contains a "Status" row
    .Select(group => new
    {
        PlanName = group.Key,
        GetPlanGroupDetail = group.ToList()
    })
                    .ToList();
                    ViewBag.PlanGroup = groupedResults.MapObjects<Models.LocationManagement.PlanGroup>();
                    ViewBag.PlanGroup1 = groupedResults.MapObjects<Models.LocationManagement.PlanGroup>();
                    PopulateMetaTagInfo("", ClubName, locationId, PrefecturesArea, responseModel.ClubId.DecryptParameter(), responseModel.ClubDescription);
                }
                #endregion
                #endregion
                ViewBag.ActionPageName = "ClubHostDetailNavMenu";
                ViewBag.BackButtonURL = "/";
                ViewBag.FileLocationPath = "";
                ViewBag.SFilterDate = ScheduleFilterDate;
                ViewBag.ClubName = !string.IsNullOrEmpty(responseModel.ClubNameJp) ? responseModel.ClubNameJp : responseModel.ClubNameEng;
                return View(responseModel);
            }
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

        private void PopulateMetaTagInfo(string type, string clubName, string locationId, string prefecturesArea, string clubId, string clubDescription = "")
        {
            string[] PrefecturesAreaPart = prefecturesArea.Split('/');
            string metaTagDescription;

            switch (type.Trim())
            {
                case "host":
                    var hostMetaTagDBResponse = _commonBusiness.GetMetaTagInfo("2", locationId, clubId);
                    metaTagDescription = $"{clubName}のホスト・スタッフ一覧です。業界初！ホストクラブの初回来店がネット予約出来る「ホスログ」にて、{clubName}のホスト・スタッフ{hostMetaTagDBResponse.Item1}件を掲載中です☆";
                    break;
                case "schedule":
                    metaTagDescription = $"{clubName}のスケジュール一覧です。業界初！ホストクラブの初回来店がネット予約出来る「ホスログ」にて、{clubName}のイベント・店休日など最新情報をチェック☆";
                    break;
                case "notice":
                    metaTagDescription = $"{clubName}のお知らせ一覧です。業界初！ホストクラブの初回来店がネット予約出来る「ホスログ」にて、{clubName}のお知らせを最速をチェック☆";
                    break;
                case "review":
                    var reviewMetaTagDBResponse = _commonBusiness.GetMetaTagInfo("3", locationId, clubId);
                    metaTagDescription = $"{clubName}のレビュー（口コミ）一覧です。業界初！ホストクラブの初回来店がネット予約出来る「ホスログ」にて、{clubName}のレビュー（口コミ）{reviewMetaTagDBResponse.Item1}件を掲載中です☆";
                    break;
                case "gallery":
                    var galleryMetaTagDBResponse = _commonBusiness.GetMetaTagInfo("4", locationId, clubId);
                    metaTagDescription = $"{clubName}の写真一覧です。業界初！ホストクラブの初回来店がネット予約出来る「ホスログ」にて、[club]([ruby])の写真{galleryMetaTagDBResponse.Item1}件を掲載中です☆";
                    break;
                default:
                    metaTagDescription = $"{clubName}‖{PrefecturesAreaPart[2]}({PrefecturesAreaPart[1]})の店舗情報はホスログでチェック☆{clubDescription}";
                    break;
            }
            ViewBag.MetaTagDescription = metaTagDescription;
        }
    }
}