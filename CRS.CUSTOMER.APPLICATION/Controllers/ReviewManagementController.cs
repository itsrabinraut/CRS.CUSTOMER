using CRS.CUSTOMER.APPLICATION.Helper;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.ReviewManagement;
using CRS.CUSTOMER.BUSINESS.NotificationManagement;
using CRS.CUSTOMER.BUSINESS.ReviewManagement;
using CRS.CUSTOMER.SHARED;
using CRS.CUSTOMER.SHARED.ReviewManagement;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    public class ReviewManagementController : CustomController
    {
        private readonly IReviewManagementBusiness _reviewBuss;
        private readonly INotificationManagementBusiness _NotificationBuss;
        public ReviewManagementController(IReviewManagementBusiness reviewBuss, INotificationManagementBusiness notificationBuss)
        {
            _reviewBuss = reviewBuss;
            _NotificationBuss = notificationBuss;
        }
        #region
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        [HttpGet, Route("user/account/review")]
        public ActionResult ReviewList()
        {
            TempData["BackFromMenuBar"] = "Review";
            var Response = new List<CustomerReviewedListModel>();
            var AgentId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            var dbResponse = _reviewBuss.GetCustomerReviewedList(AgentId);
            if (dbResponse != null && dbResponse.Count > 0)
            {
                Response = dbResponse.MapObjects<CustomerReviewedListModel>();
                Response.ForEach(x => x.ClubLogo = ImageHelper.ProcessedImage(x.ClubLogo));
                Response.ForEach(x => x.ClubId = x.ClubId.EncryptParameter());
                Response.ForEach(x => x.ClubLocationId = x.ClubLocationId.EncryptParameter());
                Response.ForEach(x => x.ReviewId = x.ReviewId.EncryptParameter());
            }
            return View(Response);
        }
        #endregion

        [HttpGet]
        public ActionResult Review(ReviewReservationRequestModel Request)
        {
            var CustomerId = !string.IsNullOrEmpty(Request.CustomerId) ? Request.CustomerId.DecryptParameter() : Request.CustomerId;
            var ReservationId = !string.IsNullOrEmpty(Request.ReservationId) ? Request.ReservationId.DecryptParameter() : Request.ReservationId;
            CustomerId = string.IsNullOrEmpty(CustomerId) ? Request.CustomerId : CustomerId;
            ReservationId = string.IsNullOrEmpty(ReservationId) ? Request.ReservationId : ReservationId;
            if (string.IsNullOrEmpty(CustomerId) || string.IsNullOrEmpty(ReservationId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            var dbRequest1 = new Common()
            {
                AgentId = !string.IsNullOrEmpty(ApplicationUtilities.GetSessionValue("AgentId").ToString()) ? ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter() : null,
                ActionUser = ApplicationUtilities.GetSessionValue("Username").ToString(),
                ActionIP = ApplicationUtilities.GetIP()
            };
            if (!string.IsNullOrEmpty(Request.NotificationId))
            {
                var notificationId = Request.NotificationId.DecryptParameter();
                var dbResponse1 = _NotificationBuss.ManageSingleNotificationReadStatus(dbRequest1, notificationId);
            }
            var SessionCustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter();
            if (string.IsNullOrEmpty(SessionCustomerId) || SessionCustomerId != CustomerId)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            var dbRequest = Request.MapObject<ReviewReservationRequestCommon>();
            dbRequest.CustomerId = CustomerId;
            dbRequest.ReservationId = ReservationId;
            var dbResponse = _reviewBuss.GetReservationDetails(dbRequest);
            if (dbResponse.Code == ResponseCode.Success)
            {
                var ResponseModel = dbResponse.Data.MapObject<ReviewReservationResponseModel>();
                ResponseModel.ClubId = ResponseModel.ClubId.EncryptParameter();
                ResponseModel.CustomerId = ResponseModel.CustomerId.EncryptParameter();
                ResponseModel.ReservationId = ResponseModel.ReservationId.EncryptParameter();
                ResponseModel.ClubLogo = ImageHelper.ProcessedImage(ResponseModel.ClubLogo);
                return View(ResponseModel);
            }
            AddNotificationMessage(new NotificationModel()
            {
                NotificationType = NotificationMessage.INFORMATION,
                Message = dbResponse.Message ?? "Invalid request",
                Title = NotificationMessage.INFORMATION.ToString()
            });
            return Redirect("/");
        }

        [HttpGet]
        public ActionResult Review2(ReviewClubDetailModel Request, string RequestString = "")
        {
            if ((Request == null || string.IsNullOrEmpty(Request.ClubId)) && !string.IsNullOrEmpty(RequestString))
            {
                Request = JsonConvert.DeserializeObject<ReviewClubDetailModel>(HttpUtility.UrlDecode(RequestString));
            }
            var cId = !string.IsNullOrEmpty(Request.ClubId) ? Request.ClubId.DecryptParameter() : null;
            if (string.IsNullOrEmpty(cId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            var ResponseModel = new ReviewHostListByClubViewModel();
            ResponseModel.ReviewClubDetailModel = Request.MapObject<ReviewClubDetailModel>();
            var dbRequest = new ReviewHostListByClubRequestCommon
            {
                ClubId = cId
            };
            var dbResponse = _reviewBuss.GetHostListByClub(dbRequest);
            if (dbResponse.Code == ResponseCode.Success)
            {
                ResponseModel.ReviewHostListModel = dbResponse.Data?.MapObjects<ReviewHostListByClubResponseModel>();
                foreach (var item in ResponseModel.ReviewHostListModel)
                {
                    item.ClubId = item.ClubId.EncryptParameter();
                    item.HostId = item.HostId.EncryptParameter();
                    item.HostImage = ImageHelper.ProcessedImage(item.HostImage);
                }
                return View(ResponseModel);
            }
            AddNotificationMessage(new NotificationModel()
            {
                NotificationType = NotificationMessage.INFORMATION,
                Message = dbResponse.Message ?? "Invalid request",
                Title = NotificationMessage.INFORMATION.ToString()
            });
            return Redirect("/");
        }

        [HttpGet]
        public ActionResult Review3(string Request)
        {
            var reviewDetail = JsonConvert.DeserializeObject<ReviewClubDetailModel>(HttpUtility.UrlDecode(Request));
            var ResponseModel = new Review3ViewModel();
            ResponseModel.ReviewClubDetailModel = reviewDetail.MapObject<ReviewClubDetailModel>();
            var cId = !string.IsNullOrEmpty(ResponseModel.ReviewClubDetailModel.ClubId) ? ResponseModel.ReviewClubDetailModel.ClubId.DecryptParameter() : null;
            if (string.IsNullOrEmpty(cId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            var culture = ApplicationUtilities.GetSessionValue("culture")?.ToString()?.ToLower();
            culture = string.IsNullOrEmpty(culture) ? "ja" : culture;
            var dbResponse = _reviewBuss.GetReviewRemarkList();
            if (dbResponse == null || dbResponse.Count <= 0)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            foreach (var item in dbResponse)
            {
                ResponseModel.ReviewRemarkListModel.Add(new ReviewRemarkListResponseModel()
                {
                    RemarkId = item.RemarkId?.EncryptParameter(),
                    RemarkLabel = (!string.IsNullOrEmpty(culture) && culture == "en") ? item.RemarkLabelEnglish : item.RemarkLabelJapanese,
                    RemarkType = item.RemarkTypeEnglish,
                    RemarkTypeEnglish = item.RemarkTypeEnglish
                });
            }
            return View(ResponseModel);
        }

        [HttpGet]
        public ActionResult Review4(string Request)
        {
            var reviewDetail = JsonConvert.DeserializeObject<ReviewClubDetailModel>(HttpUtility.UrlDecode(Request));
            var ResponseModel = new Review4ViewModel();
            ResponseModel.ReviewClubDetailModel = reviewDetail.MapObject<ReviewClubDetailModel>();
            var cId = !string.IsNullOrEmpty(ResponseModel.ReviewClubDetailModel.ClubId) ? ResponseModel.ReviewClubDetailModel.ClubId.DecryptParameter() : null;
            if (string.IsNullOrEmpty(cId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            var culture = ApplicationUtilities.GetSessionValue("culture")?.ToString()?.ToLower();
            culture = string.IsNullOrEmpty(culture) ? "ja" : culture;
            var dbQuestionResponse = _reviewBuss.GetReviewDichotomousQuestionList();
            if (dbQuestionResponse == null || dbQuestionResponse.Count <= 0)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            foreach (var item in dbQuestionResponse)
            {
                ResponseModel.ReviewDichotomousQuestionModel.Add(new ReviewDichotomousQuestionModel()
                {
                    RemarkId = item.RemarkId?.EncryptParameter(),
                    RemarkLabel = (!string.IsNullOrEmpty(culture) && culture == "en") ? item.RemarkLabelEnglish : item.RemarkLabelJapanese
                });
            }

            var dbAnswerResponse = _reviewBuss.GetReviewDichotomousAnswerList();
            if (dbAnswerResponse == null || dbAnswerResponse.Count <= 0)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            foreach (var item in dbAnswerResponse)
            {
                ResponseModel.ReviewDichotomousAnswerModel.Add(new ReviewDichotomousAnswerModel()
                {
                    RemarkId = item.RemarkId?.EncryptParameter(),
                    RemarkLabel = (!string.IsNullOrEmpty(culture) && culture == "en") ? item.RemarkLabelEnglish : item.RemarkLabelJapanese,
                    RemarkLabelEnglish = item.RemarkLabelEnglish
                });
            }
            return View(ResponseModel);
        }

        [HttpGet]
        public ActionResult ReviewDetails(string Request)
        {
            var reviewDetail = JsonConvert.DeserializeObject<ReviewClubDetailModel>(HttpUtility.UrlDecode(Request));
            var ResponseModel = new ReviewClubDetailModel();
            ResponseModel = reviewDetail.MapObject<ReviewClubDetailModel>();
            var cId = !string.IsNullOrEmpty(ResponseModel.ClubId) ? ResponseModel.ClubId.DecryptParameter() : null;
            if (string.IsNullOrEmpty(cId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            return View(ResponseModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> ReviewDetails(string Request, string ReviewMVPHostId, string[] ReviewDichotomousQAIdList, string[] ReviewRemarkIdList, string[] ReviewHostIdList, int RatingScale)
        {
            var redirectToUrl = string.Empty;
            var reviewDetail = JsonConvert.DeserializeObject<ReviewClubDetailModel>(HttpUtility.UrlDecode(Request));
            var ErrorResponseModel = new ReviewClubDetailModel();
            ErrorResponseModel = reviewDetail.MapObject<ReviewClubDetailModel>();
            var clubId = !string.IsNullOrEmpty(ErrorResponseModel.ClubId) ? ErrorResponseModel.ClubId.DecryptParameter() : null;
            var reservationId = !string.IsNullOrEmpty(ErrorResponseModel.ReservationId) ? ErrorResponseModel.ReservationId.DecryptParameter() : null;
            var MVPHostId = !string.IsNullOrEmpty(ReviewMVPHostId) ? ReviewMVPHostId.DecryptParameter() : null;
            if (string.IsNullOrEmpty(clubId) || string.IsNullOrEmpty(reservationId) || string.IsNullOrEmpty(MVPHostId) || RatingScale <= 0)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                redirectToUrl = "/";
                return Json(new { redirectToUrl });
            }

            var hostIdList = ReviewHostIdList.Select(x => x.DecryptParameter()).ToList();
            var RemarkIdList = ReviewRemarkIdList.Select(x => x.DecryptParameter()).ToList();
            string[] DichotomousQAIdList = ReviewDichotomousQAIdList;
            for (int i = 0; i < DichotomousQAIdList.Length; i++)
            {
                string[] parts = DichotomousQAIdList[i].Split(':');
                DichotomousQAIdList[i] = $"{parts[0].DecryptParameter()}:{parts[1].DecryptParameter()}";
            }
            var dbRequest = new ManageReviewRequestCommon()
            {
                CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString()?.DecryptParameter(),
                ReservationId = reservationId,
                ClubId = clubId,
                MVPHostId = MVPHostId,
                RatingScale = RatingScale,
                HostList = hostIdList != null ? string.Join(",", hostIdList.ToArray()) : null,
                RemarkList = RemarkIdList != null ? string.Join(",", RemarkIdList.ToArray()) : null,
                DichotomousList = DichotomousQAIdList != null ? string.Join(",", DichotomousQAIdList.ToArray()) : null,
                ActionUser = ApplicationUtilities.GetSessionValue("Username").ToString(),
                ActionIP = ApplicationUtilities.GetIP()
            };
            var dbResponse = _reviewBuss.ManageReview(dbRequest);
            if (dbResponse.Code == ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.SUCCESS,
                    Message = dbResponse.Message ?? "Success",
                    Title = NotificationMessage.SUCCESS.ToString()
                });
                redirectToUrl = "/";
                return Json(new { redirectToUrl });
            }
            else
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Message ?? "Failed",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Json(new { redirectToUrl });
            }
        }
    }
}