﻿using System;
using System.Configuration;
using System.Web.Mvc;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.ReservationHistoryV2;
using CRS.CUSTOMER.BUSINESS.ReservationManagementV2;
using CRS.CUSTOMER.SHARED;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    public class ReservationHistoryManagementV2Controller : CustomController
    {
        private readonly IReservationManagementV2Business _buss;
        public ReservationHistoryManagementV2Controller(IReservationManagementV2Business buss)
        {
            _buss = buss;
        }
        public ActionResult ReservationHistory()
        {
            var FileLocationPath = "";
            ReservationCommonModel responseInfo = new ReservationCommonModel();
            var customerId = ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter();

            #region "Reserved History"
            var dbReservedInfo = _buss.GetReservedList(customerId);
            responseInfo.GetReservedList = dbReservedInfo.MapObjects<ReservationHistoryV2Model>();
            foreach (var item in responseInfo.GetReservedList)
            {
                item.ClubId = item.ClubId.EncryptParameter();
                item.ReservationId = item.ReservationId.EncryptParameter();
                item.CustomerId = item.CustomerId.EncryptParameter();
                item.InvoiceId = item.InvoiceId.EncryptParameter();
                if (DateTime.TryParse(item.VisitDate, out DateTime date))
                {
                    item.VisitDate = date.ToString("M/d/yyyy");
                }
            }
            #endregion

            #region "Visited History"
            var dbVisitedInfo = _buss.GetVisitedHistoryList(customerId);
            responseInfo.GetVisitedHistoryList = dbVisitedInfo.MapObjects<VisitedHistoryModel>();
            foreach (var visitedItem in responseInfo.GetVisitedHistoryList)
            {
                visitedItem.ClubId = visitedItem.ClubId.EncryptParameter();
                visitedItem.ReservationId = visitedItem.ReservationId.EncryptParameter();
                visitedItem.CustomerId = visitedItem.CustomerId.EncryptParameter();
                visitedItem.InvoiceId = visitedItem.InvoiceId.EncryptParameter();
                if (DateTime.TryParse(visitedItem.VisitDate, out DateTime date))
                {
                    visitedItem.VisitDate = date.ToString("M/d/yyyy");
                }
            }
            #endregion

            #region "Cancelled History"
            var dbCancelledInfo = _buss.GetCancelledHistory(customerId);
            responseInfo.GetCancelledHistoryList = dbCancelledInfo.MapObjects<CancelledHistoryModel>();
            foreach (var cancelItem in responseInfo.GetCancelledHistoryList)
            {
                cancelItem.ClubId = cancelItem.ClubId.EncryptParameter();
                cancelItem.ReservationId = cancelItem.ReservationId.EncryptParameter();
                cancelItem.CustomerId = cancelItem.CustomerId.EncryptParameter();
                cancelItem.InvoiceId = cancelItem.InvoiceId.EncryptParameter();
                if (DateTime.TryParse(cancelItem.VisitDate, out DateTime date))
                {
                    cancelItem.VisitDate = date.ToString("M/d/yyyy");
                }
            }
            #endregion

            #region "All History"
            var dbAllInfo = _buss.GetAllHistoryList(customerId);
            responseInfo.GetAllHistoryList = dbAllInfo.MapObjects<AllHistoryModel>();
            foreach (var allItem in responseInfo.GetAllHistoryList)
            {
                allItem.ClubId = allItem.ClubId.EncryptParameter();
                allItem.ReservationId = allItem.ReservationId.EncryptParameter();
                allItem.CustomerId = allItem.CustomerId.EncryptParameter();
                allItem.InvoiceId = allItem.InvoiceId.EncryptParameter();
                if (DateTime.TryParse(allItem.VisitDate, out DateTime date))
                {
                    allItem.VisitDate = date.ToString("M/d/yyyy");
                }
            }
            #endregion
            if (ConfigurationManager.AppSettings["Phase"] != null && ConfigurationManager.AppSettings["Phase"].ToString().ToUpper() != "DEVELOPMENT") FileLocationPath = ConfigurationManager.AppSettings["ImageVirtualPath"].ToString();
            responseInfo.GetReservedList.ForEach(x => x.ClubLogo = FileLocationPath + x.ClubLogo);
            responseInfo.GetVisitedHistoryList.ForEach(x => x.ClubLogo = FileLocationPath + x.ClubLogo);
            responseInfo.GetCancelledHistoryList.ForEach(x => x.ClubLogo = FileLocationPath + x.ClubLogo);
            responseInfo.GetAllHistoryList.ForEach(x => x.ClubLogo = FileLocationPath + x.ClubLogo);
            return View(responseInfo);
        }
        public ActionResult ViewHistoryDetail(string ReservationId = "")
        {
            string CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter();
            string reservationId = "";
            if (!string.IsNullOrEmpty(ReservationId)) reservationId = ReservationId.DecryptParameter();
            ReservationHistoryDetailModel responseinfo = new ReservationHistoryDetailModel();
            var dbResponseInfo = _buss.GetReservationHistoryDetail(CustomerId, reservationId);
            responseinfo = dbResponseInfo.MapObject<ReservationHistoryDetailModel>();
            responseinfo.HImages = responseinfo.HostImages.Split(',');
            return View(responseinfo);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RescheduleReservation(string Selectedhour = "", string Selectedminute = "00", string ReservationID = "")
        {
            var redirectToUrl = string.Empty;
            if (!string.IsNullOrEmpty(Selectedhour) && !string.IsNullOrEmpty(Selectedminute) && !string.IsNullOrEmpty(ReservationID))
            {
                var commonDBRequest = new Common();
                commonDBRequest.ActionIP = ApplicationUtilities.GetIP();
                commonDBRequest.ActionUser = ApplicationUtilities.GetSessionValue("Username").ToString();
                commonDBRequest.AgentId = ReservationID.DecryptParameter();

                string time = Selectedhour + ":" + Selectedminute;
                var dbResponseInfo = _buss.RescheduleReservation(commonDBRequest, time);
                if (dbResponseInfo != null)
                {
                    if (dbResponseInfo.Code == ResponseCode.Success)
                    {
                        AddNotificationMessage(new NotificationModel()
                        {
                            Message = dbResponseInfo.Message ?? " Your reservation time has been updated",
                            NotificationType = NotificationMessage.SUCCESS,
                            Title = NotificationMessage.SUCCESS.ToString(),
                        });
                        redirectToUrl = Url.Action("ReservationHistory", "ReservationHistoryManagementV2");
                        return Json(new { redirectToUrl });
                    }
                    else
                    {
                        AddNotificationMessage(new NotificationModel()
                        {
                            Message = dbResponseInfo.Message ?? "Failed",
                            NotificationType = NotificationMessage.INFORMATION,
                            Title = NotificationMessage.INFORMATION.ToString(),
                        });
                        return Json(new { redirectToUrl });
                    }
                }
            }
            else
            {
                AddNotificationMessage(new NotificationModel()
                {
                    Message = "Invalid Reservation Details",
                    NotificationType = NotificationMessage.WARNING,
                    Title = NotificationMessage.WARNING.ToString(),
                });
                return RedirectToAction("ReservationHistory", "ReservationHistoryManagementV2");
            }
            return Json(new { redirectToUrl });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CancelReservation(string RID = "")
        {
            string ReservationId = !string.IsNullOrEmpty(RID) ? ReservationId = RID.DecryptParameter() : null;
            if (string.IsNullOrEmpty(ReservationId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    Message = "Invalid Reservation Details",
                    NotificationType = NotificationMessage.WARNING,
                    Title = NotificationMessage.WARNING.ToString(),
                });
                return RedirectToAction("ReservationHistory", "ReservationHistoryManagementV2");
            }
            var commonDBRequest = new Common();
            commonDBRequest.ActionIP = ApplicationUtilities.GetIP();
            commonDBRequest.ActionUser = ApplicationUtilities.GetSessionValue("Username").ToString();
            commonDBRequest.AgentId = ReservationId;
            var dbResponseInfo = _buss.CancelReservation(commonDBRequest);
            dbResponseInfo.Extra1 = "true";
            if (dbResponseInfo != null)
            {
                if (dbResponseInfo.Code == ResponseCode.Success)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        Message = dbResponseInfo.Message ?? "Your reservation has been cancelled",
                        NotificationType = NotificationMessage.SUCCESS,
                        Title = NotificationMessage.SUCCESS.ToString()
                    });
                    return Json(dbResponseInfo.SetMessageInTempData(this));
                }
                else
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        Message = dbResponseInfo.Message ?? "Failed",
                        NotificationType = NotificationMessage.INFORMATION,
                        Title = NotificationMessage.INFORMATION.ToString(),
                    });
                    return Json(JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                AddNotificationMessage(new NotificationModel()
                {
                    Message = "Something went wrong. Please try again later.",
                    NotificationType = NotificationMessage.INFORMATION,
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return Json(JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult RedoReservation(string RID = "")
        {
            string ReservationId = !string.IsNullOrEmpty(RID) ? ReservationId = RID.DecryptParameter() : null;
            if (string.IsNullOrEmpty(ReservationId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    Message="Invalid Reservation details",
                    NotificationType=NotificationMessage.WARNING,
                    Title=NotificationMessage.WARNING.ToString(),
                });
                return RedirectToAction("ReservationHistory", "ReservationHistoryManagementV2");
            }
            var commonDbRequest = new Common();
            commonDbRequest.ActionUser = ApplicationUtilities.GetSessionValue("Username").ToString();
            commonDbRequest.ActionIP = ApplicationUtilities.GetIP();
            commonDbRequest.AgentId = ReservationId;
            var dbResponseInfo = _buss.RedoReservation(commonDbRequest);
            dbResponseInfo.Extra1 = "true";
            if (dbResponseInfo != null)
            {
                if (dbResponseInfo.Code == ResponseCode.Success)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        Message = dbResponseInfo.Message ?? " Your reservation has been redo",
                        NotificationType = NotificationMessage.SUCCESS,
                        Title = NotificationMessage.SUCCESS.ToString(),
                    });
                    return Json(dbResponseInfo.SetMessageInTempData(this));
                }
                else
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        Message = dbResponseInfo.Message ?? "Failed",
                        NotificationType = NotificationMessage.INFORMATION,
                        Title = NotificationMessage.INFORMATION.ToString(),
                    });
                    return Json(JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                AddNotificationMessage(new NotificationModel()
                {
                    Message = "Something went wrong. Please try again later.",
                    NotificationType = NotificationMessage.INFORMATION,
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return Json(JsonRequestBehavior.AllowGet);
            }            
        }
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult DeleteReservation(string RID = "")
        {
            string ReservationId = !string.IsNullOrEmpty(RID) ? ReservationId = RID.DecryptParameter() : null;
            if (string.IsNullOrEmpty(ReservationId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    Message = "Invalid Reservation details",
                    NotificationType = NotificationMessage.WARNING,
                    Title = NotificationMessage.WARNING.ToString(),
                });
                return RedirectToAction("ReservationHistory", "ReservationHistoryManagementV2");
            }
            var commonDbRequest = new Common();
            commonDbRequest.ActionUser = ApplicationUtilities.GetSessionValue("Username").ToString();
            commonDbRequest.ActionIP = ApplicationUtilities.GetIP();
            commonDbRequest.AgentId = ReservationId;
            var dbResponseInfo = _buss.DeleteReservation(commonDbRequest);
            if (dbResponseInfo != null)
            {
                if (dbResponseInfo.Code == ResponseCode.Success)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        Message = dbResponseInfo.Message ?? " Your reservation has been Deleted",
                        NotificationType = NotificationMessage.SUCCESS,
                        Title = NotificationMessage.SUCCESS.ToString(),
                    });
                    return Json(dbResponseInfo.SetMessageInTempData(this));
                }
                else
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        Message = dbResponseInfo.Message ?? "Failed",
                        NotificationType = NotificationMessage.INFORMATION,
                        Title = NotificationMessage.INFORMATION.ToString(),
                    });
                    return Json(JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                AddNotificationMessage(new NotificationModel()
                {
                    Message = "Something went wrong. Please try again later.",
                    NotificationType = NotificationMessage.INFORMATION,
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return Json(JsonRequestBehavior.AllowGet);
            }
        }
    }
}
