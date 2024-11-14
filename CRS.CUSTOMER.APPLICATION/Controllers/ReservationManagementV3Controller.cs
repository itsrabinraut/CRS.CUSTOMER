using CRS.CUSTOMER.APPLICATION.Helper;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.ReservationManagementV2;
using CRS.CUSTOMER.APPLICATION.Models.ReservationManagementV3;
using CRS.CUSTOMER.BUSINESS.ReservationManagementV2;
using CRS.CUSTOMER.SHARED;
using CRS.CUSTOMER.SHARED.ReservationManagementV2;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    public class ReservationManagementV3Controller : CustomController
    {
        private readonly IReservationManagementV2Business _buss;
        public ReservationManagementV3Controller(IReservationManagementV2Business buss)
        {
            _buss = buss;
        }

        [HttpGet]
        public JsonResult InitiateClubReservationProcess(string ClubId, string SelectedDate = "", string SelectedHost = "")
        {
            var ResponseModel = new InitiateClubReservationCommonModel();
            var culture = Request.Cookies["culture"]?.Value ?? "ja";
            var cId = !string.IsNullOrEmpty(ClubId) ? ClubId.DecryptParameter() : null;
            var responseData = new Dictionary<string, object> { { "Code", 1 }, { "Message", "Invalid Details" }, { "PartialView", "" }, { "UnreservableDates", "" }, { "SelectedDate", "" } };
            var dbResponse = _buss.InitiateClubReservationProcess(cId, SelectedDate);
            if (dbResponse.Code == ResponseCode.Success || dbResponse.Code == ResponseCode.Exception)
            {
                if (dbResponse.Code == ResponseCode.Exception)
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = dbResponse.Message,
                        Title = NotificationMessage.INFORMATION.ToString()
                    });
                ResponseModel = dbResponse.MapObject<InitiateClubReservationCommonModel>();
                ResponseModel.ClubId = ClubId;
                ResponseModel.SelectedHost = SelectedHost;
                var partialViewString = RenderHelper.RenderPartialViewToString(this, "_ReservationPopup", ResponseModel);
                responseData["Code"] = 0;
                responseData["Message"] = "Success";
                responseData["PartialView"] = partialViewString;
                var dayOff = ResponseModel.ClubReservationScheduleModel
                                .Where(item => !string.IsNullOrEmpty(item.Schedule) && item.Schedule.Trim().ToUpper() == "DAYOFF")
                                .Select(item => item.Date)
                                .ToList();

                responseData["Dayoff"] = Newtonsoft.Json.JsonConvert.SerializeObject(dayOff);

                var timeIntervalBySelectedDate = ResponseModel.ClubReservableTimeModel;
                responseData["TimeIntervalBySelectedDate"] = Newtonsoft.Json.JsonConvert.SerializeObject(timeIntervalBySelectedDate);

                var reservedTimeSlot = ResponseModel.ReservedTimeSlotModel;
                responseData["ReservedTimeSlot"] = Newtonsoft.Json.JsonConvert.SerializeObject(reservedTimeSlot);

                if (!string.IsNullOrEmpty(SelectedDate))
                    responseData["SelectedDate"] = SelectedDate;
            }
            else responseData["Message"] = dbResponse.Message ?? responseData["Message"];
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Route("reservation")]
        //[OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Index(string resvpg)
        {
            var ReservationPage1Model = new ReservationManagementCommonModel();
            var ReservationPage2Model = new ReservationManagementCommonModel();
            var ReservationPage3Model = new ReservationManagementCommonModel();
            var ReservationPage4Model = new ReservationManagementCommonModel();
            ReservationPage1Model = Session["ReservationPage1Model"] as ReservationManagementCommonModel;
            ReservationPage2Model = Session["ReservationPage2Model"] as ReservationManagementCommonModel;
            ReservationPage3Model = Session["ReservationPage3Model"] as ReservationManagementCommonModel;
            ReservationPage4Model = Session["ReservationPage4Model"] as ReservationManagementCommonModel;
            if (string.IsNullOrEmpty(resvpg))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            else if (resvpg.Trim() == "01" && ReservationPage1Model.ClubDetailModel != null && ReservationPage1Model.PlanDetailModel != null)
            {
                var response = new PlanViewModel
                {
                    ClubDetailModel = ReservationPage1Model.ClubDetailModel.MapObject<ClubBasicDetailModel>(),
                    PlanDetailModel = ReservationPage1Model.PlanDetailModel.MapObjects<PlanDetailModel>()
                };
                ViewBag.SelectedHost = ReservationPage1Model.SelectedHost;
                ViewBag.VisitDate = ReservationPage1Model.VisitDate;
                ViewBag.VisitTime = ReservationPage1Model.VisitTime;
                ViewBag.NoOfPeople = ReservationPage1Model.NoOfPeople;
                return View("Plan", response);
            }
            else if (resvpg.Trim() == "02" && ReservationPage2Model.ClubDetailModel != null && ReservationPage2Model.HostListModel != null)
            {
                var response = new HostViewV2Model
                {
                    ClubDetailModel = ReservationPage2Model.ClubDetailModel.MapObject<ClubBasicDetailModel>(),
                    HostListModel = ReservationPage2Model.HostListModel.MapObjects<HostListV2Model>()
                };
                return View("Host", response);
            }
            else if (resvpg.Trim() == "03" && ReservationPage3Model.ClubDetailModel != null && ReservationPage3Model.HostListModel != null)
            {
                var response = new ConfirmationViewModel
                {
                    ClubDetailModel = ReservationPage3Model.ClubDetailModel.MapObject<ClubBasicDetailModel>(),
                    HostListModel = ReservationPage3Model.HostListModel.MapObjects<HostListV2Model>()
                };
                return View("Confirmation", response);
            }
            else if (resvpg.Trim() == "04" && ReservationPage4Model.BillingViewModel != null)
            {
                ViewBag.PaymentMethodList = DDLHelper.LoadDropdownList("PAYMENTMETHODLIST") as Dictionary<string, string>;
                var response = new BillingViewModel();
                response = ReservationPage4Model.BillingViewModel.MapObject<BillingViewModel>();
                return View("Billing", response);
            }
            AddNotificationMessage(new NotificationModel()
            {
                NotificationType = NotificationMessage.INFORMATION,
                Message = "Something went wrong. Please try again later",
                Title = NotificationMessage.INFORMATION.ToString()
            });
            return Redirect("/");
        }

        #region Plan Management
        [HttpGet]
        public ActionResult Plan(string ClubId, string Date, string Time, string NoOfPeople, string SelectedHost = "")
        {
            var returnURL = !string.IsNullOrEmpty(Request.UrlReferrer.ToString()) ? Request.UrlReferrer.ToString() : "/";
            var cId = !string.IsNullOrEmpty(ClubId) ? ClubId.DecryptParameter() : null;
            string customerId = ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter();
            if (string.IsNullOrEmpty(cId) ||
                string.IsNullOrEmpty(Date) ||
                string.IsNullOrEmpty(Time) ||
                string.IsNullOrEmpty(NoOfPeople))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect(returnURL);
            }
            //check if the customer can proceed with the reservation process
            var dbResponse = _buss.IsReservationProcessValid(cId, customerId, Date, Time, NoOfPeople);
            if (dbResponse.Item1 != ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Item2 ?? "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect(returnURL);
            }
            //check if the customer can proceed with the reservation process
            //check Verify club and get club details
            var dbResponse2 = _buss.VerifyAndGetClubBasicDetail(cId);
            if (dbResponse.Item1 != ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Item2 ?? "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect(returnURL);
            }
            //check Verify club and get club details
            var FileLocationPath = string.Empty;
            if (ConfigurationManager.AppSettings["Phase"] != null && ConfigurationManager.AppSettings["Phase"].ToString().ToUpper() != "DEVELOPMENT") FileLocationPath = ConfigurationManager.AppSettings["ImageVirtualPath"].ToString();
            var ResponseModel = new PlanViewModel();
            ResponseModel.ClubDetailModel = dbResponse2.Item3.MapObject<ClubBasicDetailModel>();
            ResponseModel.ClubDetailModel.ClubId = ResponseModel.ClubDetailModel.ClubId.EncryptParameter();
            ResponseModel.ClubDetailModel.ClubLogo = ImageHelper.ProcessedImage(ResponseModel.ClubDetailModel.ClubLogo);
            //Plan
            var dbResponse3 = _buss.GetPlans(cId, customerId, Date, Time);
            if (dbResponse3.Item1 == ResponseCode.Failed || dbResponse3.Item1 == ResponseCode.Exception)
            {

                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse3.Item2 ?? "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect(returnURL);
            }
            else if (dbResponse3.Item1 == ResponseCode.Success)
            {
                ResponseModel.PlanDetailModel = dbResponse3.Item3.MapObjects<PlanDetailModel>();
                ResponseModel.PlanDetailModel.ForEach(x => x.PlanId = x.PlanId.EncryptParameter());
            }
            //Plan
            ViewBag.SelectedHost = SelectedHost;
            ViewBag.VisitDate = Date;
            ViewBag.VisitTime = Time;
            ViewBag.NoOfPeople = NoOfPeople;
            var ReservationManagementCommonModel = new ReservationManagementCommonModel
            {
                ClubDetailModel = ResponseModel.ClubDetailModel.MapObject<ClubBasicDetailModel>(),
                PlanDetailModel = ResponseModel.PlanDetailModel.MapObjects<PlanDetailModel>(),
                SelectedHost = SelectedHost,
                VisitDate = Date,
                VisitTime = Time,
                NoOfPeople = NoOfPeople
            };
            Session["ReservationPage1Model"] = ReservationManagementCommonModel;
            string redirectURL = $"/reservation?resvpg=01";
            return Redirect(redirectURL);
        }
        #endregion

        #region Host Management
        [HttpGet]
        public ActionResult Host(string ClubId, string ClubDetailModel)
        {
            var ClubDetail = JsonConvert.DeserializeObject<ClubBasicDetailModel>(HttpUtility.UrlDecode(ClubDetailModel));
            var cId = !string.IsNullOrEmpty(ClubId) ? ClubId.DecryptParameter() : null;
            if (string.IsNullOrEmpty(cId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            var ResponseModel = new HostViewV2Model();
            ResponseModel.ClubDetailModel = ClubDetail.MapObject<ClubBasicDetailModel>();
            var dbResponse = _buss.GetHostList(cId);
            if (dbResponse.Count > 0)
            {
                ResponseModel.HostListModel = dbResponse.MapObjects<HostListV2Model>();
                ResponseModel.HostListModel.ForEach(x =>
                {
                    x.ClubId = x.ClubId.EncryptParameter();
                    x.HostId = x.HostId.EncryptParameter();
                    x.HostImage = ImageHelper.ProcessedImage(x.HostImage);
                });
            }
            var ReservationManagementCommonModel = new ReservationManagementCommonModel
            {
                ClubDetailModel = ResponseModel.ClubDetailModel.MapObject<ClubBasicDetailModel>(),
                HostListModel = ResponseModel.HostListModel.MapObjects<HostListV2Model>()
            };
            Session["ReservationPage2Model"] = ReservationManagementCommonModel;
            string redirectURL = $"/reservation?resvpg=02";
            return Redirect(redirectURL);
        }
        #endregion

        #region Reservation confirmation
        [HttpGet]
        public ActionResult Confirmation(string ClubId, string HostIdList, string ClubDetailModel)
        {
            var ClubDetail = JsonConvert.DeserializeObject<ClubBasicDetailModel>(HttpUtility.UrlDecode(ClubDetailModel));
            var cId = !string.IsNullOrEmpty(ClubId) ? ClubId.DecryptParameter() : null;
            if (string.IsNullOrEmpty(cId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            var ResponseModel = new ConfirmationViewModel();
            ResponseModel.ClubDetailModel = ClubDetail.MapObject<ClubBasicDetailModel>();
            var HostIdListSplit = HostIdList.Split(',');
            var HostIdListArray = HostIdListSplit.Select(x => x.DecryptParameter()).ToArray();
            var HostIdLists = HostIdListArray != null ? string.Join(",", HostIdListArray.ToArray()) : null;
            if (string.IsNullOrEmpty(HostIdLists))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            if (HostIdLists != null && HostIdLists.Trim() == "0")
            {
                ResponseModel.HostListModel = new List<HostListV2Model>
                {
                    new HostListV2Model
                    {
                        HostId = ApplicationUtilities.EncryptParameter("0").ToString(),
                        ClubId =ClubId
                    }
                };
            }
            else
            {
                var dbResponse = _buss.GetSelectedHostDetail(cId, HostIdLists);
                if (dbResponse != null && dbResponse.Count > 0)
                {
                    ResponseModel.HostListModel = dbResponse.MapObjects<HostListV2Model>();
                    ResponseModel.HostListModel.ForEach(x =>
                    {
                        x.HostId = x.HostId.EncryptParameter();
                        x.ClubId = x.ClubId.EncryptParameter();
                        x.HostImage = ImageHelper.ProcessedImage(x.HostImage);
                    });
                }
            }
            var ReservationManagementCommonModel = new ReservationManagementCommonModel
            {
                ClubDetailModel = ResponseModel.ClubDetailModel.MapObject<ClubBasicDetailModel>(),
                HostListModel = ResponseModel.HostListModel.MapObjects<HostListV2Model>()
            };
            Session["ReservationPage3Model"] = ReservationManagementCommonModel;
            string redirectURL = $"/reservation?resvpg=03";
            return Redirect(redirectURL);
        }
        #endregion

        #region
        [HttpGet]
        public ActionResult Billing(string ClubId, string PlanId, string VisitDate, string VisitTime, string NoOfPeople)
        {
            var cId = !string.IsNullOrEmpty(ClubId) ? ClubId.DecryptParameter() : null;
            var pId = !string.IsNullOrEmpty(PlanId) ? PlanId.DecryptParameter() : null;
            string customerId = ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter();
            if (string.IsNullOrEmpty(cId) ||
                string.IsNullOrEmpty(pId) ||
                string.IsNullOrEmpty(VisitDate) ||
                string.IsNullOrEmpty(VisitTime) ||
                string.IsNullOrEmpty(NoOfPeople))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            #region check if the customer can proceed with the reservation process
            var dbResponse = _buss.IsReservationProcessValid(cId, customerId, VisitDate, VisitTime, NoOfPeople);
            if (dbResponse.Item1 != ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Item2 ?? "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            #endregion
            #region get customer reservation billing details
            var dbRequest = new BillingRequestModel
            {
                CustomerId = customerId,
                ClubId = cId,
                PlanId = pId,
                NoOfPeople = NoOfPeople
            };
            #endregion
            var dbResponse2 = _buss.ReservationBillingDetail(dbRequest);
            if (dbResponse2.Item1 != ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse2.Item2 ?? "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            var ResponseModel = new BillingViewModel();
            ResponseModel = dbResponse2.Item3.MapObject<BillingViewModel>();
            ViewBag.PaymentMethodList = DDLHelper.LoadDropdownList("PAYMENTMETHODLIST") as Dictionary<string, string>;
            var ReservationManagementCommonModel = new ReservationManagementCommonModel
            {
                BillingViewModel = ResponseModel.MapObject<BillingViewModel>()
            };
            Session["ReservationPage4Model"] = ReservationManagementCommonModel;
            string redirectURL = $"/reservation?resvpg=04";
            return Redirect(redirectURL);
        }
        #endregion

        #region Reservation Confirmation
        [HttpGet]
        public async Task<ActionResult> ReservationConfirmation(ReservationConfirmationRequestModel Model, string HostIdList)
        {
            var clubId = !string.IsNullOrEmpty(Model.ClubId) ? Model.ClubId.DecryptParameter() : null;
            var agentId = ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter();
            var planId = !string.IsNullOrEmpty(Model.PlanId) ? Model.PlanId.DecryptParameter() : null;
            if (string.IsNullOrEmpty(clubId) ||
                string.IsNullOrEmpty(planId) ||
                string.IsNullOrEmpty(Model.VisitDate) ||
                string.IsNullOrEmpty(Model.VisitTime) ||
                string.IsNullOrEmpty(Model.PaymentType) ||
                string.IsNullOrEmpty(Model.NoOfPeople) &&
                HostIdList.Count() > 0)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.ERROR,
                    Message = "こちらの操作は出来ません",
                    Title = NotificationMessage.ERROR.ToString()
                });
                return Redirect("/");
            }
            var HostIdListSplit = HostIdList.Split(',');
            var HostIdListArray = HostIdListSplit.Select(x => x.DecryptParameter()).ToArray();
            var HostIdLists = HostIdListArray != null ? string.Join(",", HostIdListArray.ToArray()) : null;
            var dbRequest = new ReservationConfirmationRequestCommon
            {
                ClubId = clubId,
                CustomerId = agentId,
                PlanId = planId,
                VisitDate = Model.VisitDate,
                VisitTime = Model.VisitTime,
                NoOfPeople = Model.NoOfPeople,
                HostIdList = HostIdLists,
                PaymentType = (string.IsNullOrEmpty(Model.PaymentType) || Model.PaymentType.Trim() == "0") ? "0" : Model.PaymentType.DecryptParameter(),
                ActionUser = ApplicationUtilities.GetSessionValue("UserId")?.ToString()?.DecryptParameter(),
                ActionIP = ApplicationUtilities.GetIP()
            };
            var dbValidationResponse = _buss.IsReservationProcessValid(dbRequest.ClubId, dbRequest.CustomerId, dbRequest.VisitDate, dbRequest.VisitTime, dbRequest.NoOfPeople);
            if (dbValidationResponse.Item1 != ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbValidationResponse.Item2 ?? "こちらの操作は出来ません",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return Redirect("/");
            }
            var dbResponse = _buss.ReservationConfirmation(dbRequest);
            if (dbResponse.Code == ResponseCode.Success)
            {
                Session["ReservationPage1Model"] = null;
                Session["ReservationPage2Model"] = null;
                Session["ReservationPage3Model"] = null;
                Session["ReservationPage4Model"] = null;
                return Redirect("/reservation/complete");
            }
            else
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.ERROR,
                    Message = dbResponse.Message ?? "こちらの操作は出来ません",
                    Title = NotificationMessage.ERROR.ToString()
                });
                return Redirect("/");
            }
        }
        #endregion

        #region Reservation Success
        [HttpGet, Route("reservation/complete")]
        public ActionResult Success()
        {
            Session["ReservationPage1Model"] = null;
            Session["ReservationPage2Model"] = null;
            Session["ReservationPage3Model"] = null;
            Session["ReservationPage4Model"] = null;
            return View();
        }
        #endregion
    }
}