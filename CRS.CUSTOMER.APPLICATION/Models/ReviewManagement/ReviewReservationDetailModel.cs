namespace CRS.CUSTOMER.APPLICATION.Models.ReviewManagement
{
    public class ReviewClubDetailModel
    {
        public string ReservationId { get; set; }
        public string ClubId { get; set; }
        public string ClubLogo { get; set; }
        public string ClubNameEnglish { get; set; }
        public string ClubNameJapanese { get; set; }
        public string ClubLocationName { get; set; }
    }
    public class ReviewReservationRequestModel
    {
        public string CustomerId { get; set; }
        public string ReservationId { get; set; }
        public string NotificationId { get; set; }
    }
    public class ReviewReservationResponseModel
    {
        public string CustomerId { get; set; }
        public string ReservationId { get; set; }
        public string ReservationDate { get; set; }
        public string ReservationTime { get; set; }
        public string Price { get; set; }
        public string NoOfPeople { get; set; }
        public string UsedPoint { get; set; }
        public string TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string ClubId { get; set; }
        public string ClubLogo { get; set; }
        public string ClubNameEnglish { get; set; }
        public string ClubNameJapanese { get; set; }
        public string ClubLocationName { get; set; }
    }
}