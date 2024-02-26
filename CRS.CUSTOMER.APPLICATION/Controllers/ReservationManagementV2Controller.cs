﻿using CRS.CUSTOMER.APPLICATION.Helper;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.ReservationHistory;
using CRS.CUSTOMER.APPLICATION.Models.ReservationManagementV2;
using CRS.CUSTOMER.APPLICATION.Models.ReviewManagement;
using CRS.CUSTOMER.BUSINESS.ReservationManagementV2;
using CRS.CUSTOMER.SHARED;
using CRS.CUSTOMER.SHARED.ReservationManagementV2;
using Newtonsoft.Json;
using Syncfusion.CompoundFile.XlsIO.Native;
using Syncfusion.XlsIO.Implementation.PivotAnalysis;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    public class ReservationManagementV2Controller : CustomController
    {
        private readonly IReservationManagementV2Business _buss;
        public ReservationManagementV2Controller(IReservationManagementV2Business buss)
        {
            _buss = buss;
        }
        [HttpGet]
        public JsonResult InitiateClubReservationProcess(string ClubId, string SelectedDate = "", string SelectedHost = "")
        {
            var ResponseModel = new InitiateClubReservationCommonModel();
            var culture = Request.Cookies["culture"]?.Value ?? "ja";
            var cId = !string.IsNullOrEmpty(ClubId) ? ClubId.DecryptParameter() : null;
            var responseData = new Dictionary<string, object> { { "Code", 1 }, { "Message", "Invalid Details" }, { "PartialView", "" }, { "UnreservableDates", "" } };
            var dbResponse = _buss.InitiateClubReservationProcess(cId, SelectedDate);
            if (dbResponse.Code == ResponseCode.Success)
            {
                ResponseModel = dbResponse.MapObject<InitiateClubReservationCommonModel>();
                ResponseModel.ClubId = ClubId;
                ResponseModel.SelectedHost = SelectedHost;
                var partialViewString = RenderHelper.RenderPartialViewToString(this, "_ReservationPopup", ResponseModel);
                responseData["Code"] = 0;
                responseData["Message"] = "Success";
                responseData["PartialView"] = partialViewString;
                var unReservableDateList = ResponseModel.ClubReservationScheduleModel
                                             .Where(item => !string.IsNullOrEmpty(item.Schedule) && item.Schedule.Trim().ToUpper() != "RESERVABLE")
                                             .Select(item => item.Date)
                                             .ToList();

                responseData["UnreservableDates"] = Newtonsoft.Json.JsonConvert.SerializeObject(unReservableDateList);

            }
            else responseData["Message"] = dbResponse.Message ?? responseData["Message"];
            return Json(responseData, JsonRequestBehavior.AllowGet);
        }

        #region Plan Management
        [HttpGet]
        public ActionResult Plan(string ClubId, string Date, string Time, string NoOfPeople, string SelectedHost = "")
        {
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
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
            //check if the customer can proceed with the reservation process
            var dbResponse = _buss.IsReservationProcessValid(cId, customerId, Date, Time, NoOfPeople);
            if (dbResponse.Item1 != ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Item2 ?? "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
            //check if the customer can proceed with the reservation process
            //check Verify club and get club details
            var dbResponse2 = _buss.VerifyAndGetClubBasicDetail(cId);
            if (dbResponse.Item1 != ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Item2 ?? "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
            //check Verify club and get club details
            var FileLocationPath = string.Empty;
            if (ConfigurationManager.AppSettings["Phase"] != null && ConfigurationManager.AppSettings["Phase"].ToString().ToUpper() != "DEVELOPMENT") FileLocationPath = ConfigurationManager.AppSettings["ImageVirtualPath"].ToString();
            var ResponseModel = new PlanViewModel();
            ResponseModel.ClubDetailModel = dbResponse2.Item3.MapObject<ClubBasicDetailModel>();
            ResponseModel.ClubDetailModel.ClubId = ResponseModel.ClubDetailModel.ClubId.EncryptParameter();
            //Plan
            var dbResponse3 = _buss.GetPlans(cId, customerId);
            if (dbResponse3.Item1 == ResponseCode.Failed)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Item2 ?? "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
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
            return View(ResponseModel);
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
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
            var ResponseModel = new HostViewV2Model();
            ResponseModel.ClubDetailModel = ClubDetail.MapObject<ClubBasicDetailModel>();
            var dbResponse = _buss.GetHostList(cId);
            if (dbResponse.Count > 0)
            {
                ResponseModel.HostListModel = dbResponse.MapObjects<HostListV2Model>();
                ResponseModel.HostListModel.ForEach(x => { x.ClubId = x.ClubId.EncryptParameter(); x.HostId = x.HostId.EncryptParameter(); });
            }
            return View(ResponseModel);
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
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
            string FileLocationPath = string.Empty;
            if (ConfigurationManager.AppSettings["Phase"] != null && ConfigurationManager.AppSettings["Phase"].ToString().ToUpper() != "DEVELOPMENT") FileLocationPath = ConfigurationManager.AppSettings["ImageVirtualPath"].ToString();
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
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
            var dbResponse = _buss.GetSelectedHostDetail(cId, HostIdLists);
            if (dbResponse != null && dbResponse.Count > 0)
            {
                ResponseModel.HostListModel = dbResponse.MapObjects<HostListV2Model>();
                ResponseModel.HostListModel.ForEach(x => x.HostImage = FileLocationPath + x.HostImage);
            }
            return View(ResponseModel);
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
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
            #region check if the customer can proceed with the reservation process
            var dbResponse = _buss.IsReservationProcessValid(cId, customerId, VisitDate, VisitTime, NoOfPeople);
            if (dbResponse.Item1 != ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Item2 ?? "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
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
                    Message = dbResponse.Item2 ?? "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
            var ResponseModel = new BillingViewModel();
            ResponseModel = dbResponse2.Item3.MapObject<BillingViewModel>();
            ViewBag.PaymentMethodList = DDLHelper.LoadDropdownList("PAYMENTMETHODLIST") as Dictionary<string, string>;
            return View(ResponseModel);
        }
        #endregion

        #region Reservation Confirmation
        [HttpGet]
        public ActionResult ReservationConfirmation(ReservationConfirmationRequestModel Model, string[] HostIdList)
        {
            var clubId = !string.IsNullOrEmpty(Model.ClubId) ? Model.ClubId.DecryptParameter() : null;
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
                    Message = "Invalid request",
                    Title = NotificationMessage.ERROR.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
            var hId = HostIdList.Select(x => x.DecryptParameter()).ToList();
            var dbRequest = new ReservationConfirmationRequestCommon
            {
                ClubId = clubId,
                CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter(),
                PlanId = planId,
                VisitDate = Model.VisitDate,
                VisitTime = Model.VisitTime,
                NoOfPeople = Model.NoOfPeople,
                HostIdList = hId != null ? string.Join(",", hId.ToArray()) : null,
                ActionUser = ApplicationUtilities.GetSessionValue("Username").ToString(),
                ActionIP = ApplicationUtilities.GetIP()
            };
            var dbResponse = _buss.ReservationConfirmation(dbRequest);
            if (dbResponse.Code == ResponseCode.Success)
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.SUCCESS,
                    Message = dbResponse.Message ?? "Success",
                    Title = NotificationMessage.SUCCESS.ToString()
                });
                return RedirectToAction("Success", "ReservationManagementV2");
            }
            else
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.ERROR,
                    Message = dbResponse.Message ?? "Invalid request",
                    Title = NotificationMessage.ERROR.ToString()
                });
                return RedirectToAction("Index", "Dashboard");
            }
        }
        #endregion

        #region Reservation Success
        public ActionResult Success()
        {
            return View();
        }
        #endregion
    }
}