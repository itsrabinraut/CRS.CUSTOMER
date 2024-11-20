using CRS.CUSTOMER.APPLICATION.Models.Dashboard;
using CRS.CUSTOMER.APPLICATION.Models.LocationManagement;
using System.Collections.Generic;

namespace CRS.CUSTOMER.APPLICATION.Models.LocationManagementV2
{
    public class LocationV2ClubHostRequestModel
    {
        public int GroupId { get; set; } = 1;
        public int PageNo { get; set; } = 1;
        public string ClubId { get; set; } = string.Empty;
        public string RenderId { get; set; } = "";
    }

    public class LocationV2ClubHostModel
    {
        public LocationV2ClubHostRequestModel RequestModel { get; set; } = new LocationV2ClubHostRequestModel();
        public List<LocationV2ClubListModel> ClubListModel { get; set; } = new List<LocationV2ClubListModel>();
        public List<LocationV2HostListModel> HostListModel { get; set; } = new List<LocationV2HostListModel>();
        public List<BannersModel> Banners { get; set; } = new List<BannersModel>();
    }
    public class GroupWisePaginationModel
    {
        public int TotalPages { get; set; }
        public string PositionId { get; set; }
    }
    public class LocationV2ClubListModel
    {
        public string ClubId { get; set; }
        public string ClubCode { get; set; }
        public string LocationId { get; set; }
        public string ClubName { get; set; }
        public string ClubNameJapanese { get; set; }
        public string GroupName { get; set; }
        public string FullName { get; set; }
        public string ClubLogo { get; set; }
        public string ClubCoverPhoto { get; set; }
        public string ClubDescription { get; set; }
        public string Tag1 { get; set; }
        public string Tag2 { get; set; }
        public string Tag3 { get; set; }
        public string Tag4 { get; set; }
        public string Tag5 { get; set; }
        public string ClubOpeningTime { get; set; }
        public string ClubClosingTime { get; set; }
        public string ClubTimePeriod { get; set; }
        public string TodaysClubSchedule { get; set; }
        public string IsBookmarked { get; set; }
        public int TotalComment { get; set; }
        public int AverageRating { get; set; }
        public string ClubCategory { get; set; }
        public string LandLine { get; set; }
        public List<string> ClubGalleryImage { get; set; } = new List<string>();
        public List<string> HostGalleryImage { get; set; } = new List<string>();
        public List<ClubWeeklyScheduleModel> ClubWeeklyScheduleList { get; set; } = new List<ClubWeeklyScheduleModel>();
    }

    public class LocationV2HostListModel
    {
        public string LocationId { get; set; }
        public string ClubId { get; set; }
        public string ClubCode { get; set; }
        public string HostId { get; set; }
        public string HostCode { get; set; }
        public string HostName { get; set; }
        public string Occupation { get; set; }
        public string Rank { get; set; }
        public string ClubName { get; set; }
        public string ClubNameJapanese { get; set; }
        public string HostImage { get; set; }
        public string IsBookmarked { get; set; }
        public string ClubLogo { get; set; }
        public string HostPosition { get; set; }
        public string HostNameJapanese { get; set; }
        public string FormattedDOB { get; set; }
        public string HostHeight { get; set; }
        public string HostBirthPlace { get; set; }
        public string HostLoveCount { get; set; }
        public string HostBloodType { get; set; }
        public string HostConstellationGroup { get; set; }
        public string LiquorStrength { get; set; }
        public string HostTitle { get; set; }
    }

    public class LocationV2ClubWeeklyScheduleModel
    {
        public string EnglishDay { get; set; }
        public string Date { get; set; }
        public string JapaneseDate { get; set; }
        public string DateLabel { get; set; }
        public string Schedule { get; set; }
        public string ScheduleLabel { get; set; }
        public string JapaneseDay { get; set; }
        public string DayLabel { get; set; }
        public string DateValue { get; set; }
        public string FullDate { get; set; }
    }
}