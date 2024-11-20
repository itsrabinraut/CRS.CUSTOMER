
using System.Collections.Generic;

namespace CRS.CUSTOMER.SHARED.LocationManagement
{
    public class LocationClubListCommon
    {
        public string ClubId { get; set; }
        public string LocationId { get; set; }
        public string ClubName { get; set; }
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
        public string TodaysClubSchedule { get; set; }
        public string ClubTimePeriod { get; set; }
        public string ClubNameJapanese { get; set; }
        public string IsBookmarked { get; set; }
        public List<string> ClubGalleryImage { get; set; }
        public List<ClubWeeklyScheduleCommon> ClubWeeklyScheduleList { get; set; } = new List<ClubWeeklyScheduleCommon>();
    }

    public class LocationHostListCommon
    {
        public string LocationId { get; set; }
        public string ClubId { get; set; }
        public string HostId { get; set; }
        public string HostName { get; set; }
        public string HostNameJapanese { get; set; }
        public string Occupation { get; set; }
        public string Rank { get; set; }
        public string ClubName { get; set; }
        public string HostImage { get; set; }
        public string IsBookmarked { get; set; }
        public string HostPosition { get; set; }
        public string HostCode { get; set; }
        public string ClubCode { get; set; }
        public string LocationURL { get; set; }
        public string HostTitle { get; set; }
    }

    public class ClubWeeklyScheduleCommon
    {
        public string EnglishDay { get; set; }
        public string Date { get; set; }
        public string DateValue { get; set; }
        public string JapaneseDate { get; set; }
        public string Schedule { get; set; }
        public string ScheduleLabel { get; set; }
        public string JapaneseDay { get; set; }
        public string FullDate { get; set; }
    }
}
