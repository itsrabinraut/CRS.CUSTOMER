﻿using CRS.CUSTOMER.APPLICATION.Helper;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models;
using CRS.CUSTOMER.APPLICATION.Models.NotificationHelper;
using CRS.CUSTOMER.APPLICATION.Models.NotificationManagement;
using CRS.CUSTOMER.BUSINESS.NotificationManagement;
using CRS.CUSTOMER.SHARED;
using CRS.CUSTOMER.SHARED.NotificationManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class NotificationManagementController : CustomController
    {
        private readonly INotificationManagementBusiness _buss;
        private static AmazonS3Configruation _AmazonS3Configruation = ApplicationUtilities.GetAppDataJsonConfigValue<AmazonConfigruation>("AmazonConfigruation").AmazonS3Configruation;
        private static SignalRConfigruationModel _signalRConfigruation = ApplicationUtilities.GetAppDataJsonConfigValue<SignalRConfigruationModel>("SignalRConfigruation");
        private readonly SignalRStringCipher _stringCipher;
        private readonly NotificationHelper _notificationHelper;

        public NotificationManagementController(INotificationManagementBusiness buss,
           SignalRStringCipher stringCipher,
           NotificationHelper notificationHelper)
        {
            _buss = buss;
            _stringCipher = stringCipher;
            _notificationHelper = notificationHelper;
        }

        [HttpGet]
        public ActionResult ViewAllNotifications()
        {
            var requestCommon = new ManageNotificationCommon()
            {
                ActionUserId = ApplicationUtilities.GetSessionValue("UserId").ToString().DecryptParameter(),
                AgentId = ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter(),
            };
            var dbResponse = _buss.GetAllNotification(requestCommon);

            var responseModel = new List<NotificationDetailModel>();
            responseModel = dbResponse.MapObjects<NotificationDetailModel>();
            ViewBag.ActionPageName = "NavMenu";
            if (responseModel?.FirstOrDefault()?.UnReadNotification != null &&
                    int.TryParse(responseModel.FirstOrDefault().UnReadNotification, out int unReadCount) &&
                    unReadCount > 0)
                ViewBag.PageTitle = $"{Resources.Resource.Notifications} ({responseModel.FirstOrDefault().UnReadNotification})";
            else
                ViewBag.PageTitle = Resources.Resource.Notifications;
            responseModel.ForEach(x =>
            {
                x.NotificationId = x.NotificationId.EncryptParameter();
                x.NotificationURL = (!string.IsNullOrEmpty(x.NotificationURL) && x.NotificationURL.Trim() != "#") ? URLHelper.EncryptQueryParams(x.NotificationURL) : "#";
                x.NotificationImage = ImageHelper.ProcessedImage(x.NotificationImage, false, $"{_AmazonS3Configruation.BaseURL}/{_AmazonS3Configruation.BucketName}/{_AmazonS3Configruation.NotificationNoImageURL.TrimStart('/')}");
                x.CreatedDate = x.CreatedDate;
                ViewBag.CreatedDate = Convert.ToDateTime(x.CreatedDate).ToString("yyyy.MM.dd");
            });
            foreach (var item in responseModel)
            {
                if (!string.IsNullOrWhiteSpace(item.ExtraDetails))
                {
                    string[] dbValues = item.ExtraDetails.Split(new[] { ',' }, 2);

                    if (dbValues.Length > 0)
                    {
                        dbValues[0] = ImageHelper.ProcessedImage(
                            dbValues[0],
                            false,
                            $"{_AmazonS3Configruation.BaseURL}/{_AmazonS3Configruation.BucketName}/{_AmazonS3Configruation.NotificationNoImageURL.TrimStart('/')}"
                        );

                        item.ExtraDetails = string.Join(",", dbValues);
                    }
                }
            }
            return View(responseModel);
        }

        [HttpPost, OverrideActionFilters]
        public JsonResult HasUnReadNotification()
        {
            var CustomerId = ApplicationUtilities.GetSessionValue("AgentId").ToString();
            CustomerId = !string.IsNullOrEmpty(CustomerId) ? CustomerId.DecryptParameter() : null;
            if (!string.IsNullOrEmpty(CustomerId))
            {
                var dbResponse = _buss.HasUnReadNotification(CustomerId);
                if (dbResponse)
                {
                    return Json(new { HasUnReadNotification = true });
                }
            }
            return Json(new { HasUnReadNotification = false });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ManageSingleNotificationReadStatus(string notificationId = "")
        {
            string NotificationId = "";
            if (!string.IsNullOrEmpty(notificationId))
                NotificationId = notificationId.DecryptParameter();
            var dbRequest = new Common()
            {
                AgentId = !string.IsNullOrEmpty(ApplicationUtilities.GetSessionValue("AgentId").ToString()) ? ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter() : null,
                ActionUser = ApplicationUtilities.GetSessionValue("Username").ToString(),
                ActionIP = ApplicationUtilities.GetIP()
            };
            if (!string.IsNullOrEmpty(dbRequest.AgentId) && !string.IsNullOrEmpty(dbRequest.ActionUser) && !string.IsNullOrEmpty(NotificationId))
            {
                var dbResponse = _buss.ManageSingleNotificationReadStatus(dbRequest, NotificationId);
                if (dbRequest != null && dbResponse.Code == ResponseCode.Success)
                {
                    Session["NotificationUnReadCount"] = dbResponse.Extra1;
                    return Json(new { Code = "0", Message = dbResponse.Message ?? "Success", PageTitle = Resources.Resource.Notifications, notificationUnReadCount = dbResponse.Extra1 });
                }
                return Json(new
                {
                    Code = "1",
                    Message = dbResponse.Message ?? "Invalid request"
                });
            }
            return Json(new { Code = "1", Message = "Something went wrong. Please try again later" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult CustomerReservationCancelRemark(string NotificationId, string CustomerRemarks)
        {
            var dbRequest = new Common()
            {
                AgentId = !string.IsNullOrEmpty(ApplicationUtilities.GetSessionValue("AgentId").ToString()) ? ApplicationUtilities.GetSessionValue("AgentId").ToString().DecryptParameter() : null,
                ActionUser = ApplicationUtilities.GetSessionValue("Username").ToString(),
                //NotificationId=NotificationId.DecryptParameter()
            };
            if (!string.IsNullOrEmpty(dbRequest.AgentId) && !string.IsNullOrEmpty(dbRequest.ActionUser))
            {
                var dbResponse = _buss.ManageReservationCancelRemark(dbRequest, NotificationId.DecryptParameter(), CustomerRemarks);
                if (dbResponse != null && dbResponse.Code == ResponseCode.Success) return Json(new { Code = "0", Message = dbResponse.Message ?? "Success", PageTitle = Resources.Resource.Notifications });
                return Json(new { Code = "1", Message = dbResponse.Message ?? "Invalid request" });
            }
            return Json(new { Code = "1", Message = "Something went wrong. Please try again later." });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> ManageNotificationReadStatus(string notificationId)
        {
            const string errorMessage = "Invalid request";

            var decryptedNotificationId = !string.IsNullOrEmpty(notificationId) ? notificationId.DecryptParameter() : null;
            if (!string.IsNullOrEmpty(notificationId) && string.IsNullOrEmpty(decryptedNotificationId))
                return Json(new { Code = "1", Message = errorMessage });

            var sessionAgentId = ApplicationUtilities.GetSessionValue("AgentId")?.ToString();
            var decryptedAgentId = !string.IsNullOrEmpty(sessionAgentId) ? sessionAgentId.DecryptParameter() : null;
            var sessionUserId = ApplicationUtilities.GetSessionValue("UserId")?.ToString();
            var decryptedUserId = !string.IsNullOrEmpty(sessionUserId) ? sessionUserId.DecryptParameter() : null;
            if (string.IsNullOrEmpty(decryptedAgentId))
                return Json(new { Code = "1", Message = errorMessage });

            var helperRequest = new NotificationReadRequestModel
            {
                notificationId = decryptedNotificationId,
                agentId = decryptedAgentId,
                actionUser = decryptedUserId
            };

            var helperResponse = await _notificationHelper.MarkNotificationAsReadHelperAsync(helperRequest);
            if (helperResponse == null || helperResponse.code != "0")
                return Json(new { Code = "1", Message = helperResponse?.message ?? "Something went wrong. Please try again later" });

            var data = helperResponse.data?.MapObject<NotificationReadResponseModel>();
            return Json(new
            {
                Code = "0",
                Message = helperResponse.message ?? "Success",
                PageTitle = Resources.Resource.Notifications,
                id = data?.notificationId
            });
        }
    }
}