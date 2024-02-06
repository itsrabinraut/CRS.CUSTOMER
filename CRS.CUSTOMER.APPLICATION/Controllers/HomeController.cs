﻿using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.Home;
using CRS.CUSTOMER.BUSINESS.Home;
using CRS.CUSTOMER.SHARED;
using CRS.CUSTOMER.SHARED.Home;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    [OverrideActionFilters]
    public class HomeController : CustomController
    {
        private readonly IHomeBusiness _buss;
        public HomeController(IHomeBusiness buss) => _buss = buss;
        #region Landing Page
        [HttpGet]
        public ActionResult HomePage()
        {
            var Username = ApplicationUtilities.GetSessionValue("Username").ToString();
            if (!string.IsNullOrEmpty(Username))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            var culture = Request.Cookies["culture"]?.Value;
            culture = string.IsNullOrEmpty(culture) ? "ja" : culture;
            ViewBag.Language = culture;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult ChangeLanguage(string lang)
        {
            try
            {
                new LanguageMang().SetLanguage(lang);
                HttpCookie HasLandingSession = new HttpCookie("HasLandingSession", "True")
                {
                    Expires = DateTime.Now.AddDays(1)
                };
                HttpContext.Response.Cookies.Add(HasLandingSession);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }
        #endregion

        #region Register Management
        [HttpGet]
        public ActionResult Register(string ReferCode = "")
        {
            var Username = ApplicationUtilities.GetSessionValue("Username").ToString();
            if (!string.IsNullOrEmpty(Username))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            var Response = new RegistrationHoldModel();
            if (!string.IsNullOrEmpty(ReferCode))
            {
                ReferralModelCommon referCommon = new ReferralModelCommon();
                referCommon.ReferCode = ReferCode;
                referCommon.ActionIP = ApplicationUtilities.GetIP();
                var dbReferralRes = _buss.ValidateReferralCode(referCommon);
                if (dbReferralRes != null && dbReferralRes.Code == "0")
                {
                    ViewBag.ReferCode = ReferCode;
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.SUCCESS,
                        Message = dbReferralRes.Message,
                        Title = NotificationMessage.SUCCESS.ToString(),
                    });
                    return View(Response);
                }
                else
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = dbReferralRes.Message,
                        Title = NotificationMessage.INFORMATION.ToString(),
                    });
                    return View(Response);
                }
            }
            return View(Response);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Register(RegistrationHoldModel Model, string ReferCode = "")
        {
            ViewBag.ReferCode = ReferCode;
            if (ModelState.IsValid)
            {
                RegistrationHoldCommon Common = Model.MapObject<RegistrationHoldCommon>();
                Common.ActionIP = ApplicationUtilities.GetIP();
                Common.ActionUser = Common.MobileNumber;
                var dbResponse = _buss.RegistrationHold(Common);
                if (dbResponse.Code == 0)
                {
                    var otpModel = new RegistrationModel()
                    {
                        AgentId = dbResponse.Extra1.DefaultEncryptParameter(),
                        MobileNumber = Model.MobileNumber
                    };
                    TempData["ReferCode"] = ReferCode;
                    //Session["exptime"] = DateTime.Parse(dbResponse.Extra2.ToString());
                    Session["exptime"] = DateTime.Parse(DateTime.Now.AddMinutes(2).ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                    return View("VerifyOTP", otpModel);
                }
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Message ?? "Failed",
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return View(Model);
            }
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key }).ToList();
            return View(Model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult VerifyOTP(RegistrationModel Model)
        {
            var ReferCode = string.Empty;
            if (TempData.ContainsKey("ReferCode")) ReferCode = TempData["ReferCode"] as string;
            if (ModelState.IsValid)
            {
                RegistrationCommon Common = Model.MapObject<RegistrationCommon>();
                Common.VerificationCode = string.Concat(Model.OTP1, Model.OTP2, Model.OTP3, Model.OTP4, Model.OTP5, Model.OTP6);
                Common.AgentId = Common.AgentId.DefaultDecryptParameter();
                Common.ActionIP = ApplicationUtilities.GetIP();
                Common.ActionUser = Common.MobileNumber;
                Common.ReferCode = ReferCode;
                var dbResponse = _buss.Register(Common);
                if (dbResponse.Code == 0)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.SUCCESS,
                        Message = dbResponse.Message ?? "SUCCESS",
                        Title = NotificationMessage.SUCCESS.ToString(),
                    });
                    return RedirectToAction("SetRegistrationPassword", "Home", new { AgentId = dbResponse.Extra1.DefaultEncryptParameter(), UserId = dbResponse.Extra2.DefaultEncryptParameter(), MobileNumber = Model.MobileNumber });
                }
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Message ?? "Failed",
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return View(Model);
            }
            var errorMessages = ModelState.Where(x => x.Value.Errors.Count > 0)
                                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                                    .ToList();

            var notificationModels = errorMessages.Select(errorMessage => new NotificationModel
            {
                NotificationType = NotificationMessage.ERROR,
                Message = errorMessage,
                Title = NotificationMessage.ERROR.ToString(),
            }).ToArray();
            AddNotificationMessage(notificationModels);
            return View(Model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ResendOTP(string AgentId, string MobileNumber)
        {
            var dbRequest = new ResendRegistrationOTPCommon()
            {
                AgentId = AgentId?.DefaultDecryptParameter(),
                MobileNumber = MobileNumber
            };
            var message = string.Empty;
            if (string.IsNullOrEmpty(dbRequest.AgentId) || string.IsNullOrEmpty(dbRequest.MobileNumber))
            {
                //AddNotificationMessage(new NotificationModel()
                //{
                //    NotificationType = NotificationMessage.INFORMATION,
                //    Message = "Invalid detail",
                //    Title = NotificationMessage.INFORMATION.ToString(),
                //});
                //return Json("Invalid detail", JsonRequestBehavior.AllowGet);
                message = "Invalid detail";
                return Json(new { code = "1", message }, JsonRequestBehavior.AllowGet);
            }
            CommonDbResponse dbResponse = _buss.ResendRegistrationOTP(dbRequest);
            if (dbResponse != null && dbResponse.Code == ResponseCode.Success)
            {
                //Session["exptime"] = !string.IsNullOrEmpty(dbResponse.Extra1) ? DateTime.Parse(dbResponse.Extra1.ToString()) : DateTime.Parse(DateTime.Now.AddMinutes(2).ToString());
                //AddNotificationMessage(new NotificationModel()
                //{
                //    NotificationType = NotificationMessage.SUCCESS,
                //    Message = dbResponse.Message ?? "SUCCESS",
                //    Title = NotificationMessage.SUCCESS.ToString(),
                //});
                message = dbResponse.Message ?? "SUCCESS";
                var ExpTime = DateTime.Parse(DateTime.Now.AddMinutes(2).ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                //var ExpTime = !string.IsNullOrEmpty(dbResponse.Extra2) ? DateTime.Parse(dbResponse.Extra2.ToString()).ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Parse(DateTime.Now.AddMinutes(2).ToString()).ToString("yyyy-MM-dd HH:mm:ss"); ;
                return Json(new { code = "0", message, ExpTime }, JsonRequestBehavior.AllowGet);
            }
            message = dbResponse.Message ?? "Failed";
            return Json(new { code = "1", message }, JsonRequestBehavior.AllowGet);
            //else
            //{
            //    AddNotificationMessage(new NotificationModel()
            //    {
            //        NotificationType = NotificationMessage.INFORMATION,
            //        Message = dbResponse.Message ?? "Failed",
            //        Title = NotificationMessage.INFORMATION.ToString(),
            //    });
            //}
            //return Json(dbResponse.Message, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SetRegistrationPassword(string AgentId, string UserId, string MobileNumber)
        {
            var response = new SetRegistrationPasswordModel()
            {
                AgentId = AgentId,
                UserId = UserId,
                MobileNumber = MobileNumber
            };
            return View(response);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SetRegistrationPassword(SetRegistrationPasswordModel Model)
        {
            if (ModelState.IsValid)
            {
                if (Model.Password != Model.ConfirmPassword)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = "Password and confirm password must match",
                        Title = NotificationMessage.INFORMATION.ToString()
                    });
                    return View(Model);
                }
                SetRegistrationPasswordCommon Common = Model.MapObject<SetRegistrationPasswordCommon>();
                Common.AgentId = Common.AgentId.DefaultDecryptParameter();
                Common.UserId = Common.UserId.DefaultDecryptParameter();
                Common.ActionIP = ApplicationUtilities.GetIP();
                Common.ActionUser = Model.MobileNumber;
                var dbResponse = _buss.SetRegistrationPassword(Common);
                if (dbResponse.Code == 0)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.SUCCESS,
                        Message = dbResponse.Message ?? "Success",
                        Title = NotificationMessage.SUCCESS.ToString(),
                    });
                    return RedirectToAction("Index", "Home");
                }
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Message ?? "Failed",
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return View(Model);
            }
            var errorMessages = ModelState.Where(x => x.Value.Errors.Count > 0)
                                   .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                                   .ToList();
            //var notificationModels = errorMessages.Select(errorMessage => new NotificationModel
            //{
            //    NotificationType = NotificationMessage.ERROR,
            //    Message = errorMessage,
            //    Title = NotificationMessage.ERROR.ToString(),
            //}).ToArray();
            //AddNotificationMessage(notificationModels);
            AddNotificationMessage(new NotificationModel()
            {
                NotificationType = NotificationMessage.INFORMATION,
                Message = "Please fill all required fields",
                Title = NotificationMessage.INFORMATION.ToString(),
            });
            return View(Model);
        }
        #endregion

        #region Login Management
        public ActionResult Index()
        {
            var Username = ApplicationUtilities.GetSessionValue("Username").ToString();
            if (string.IsNullOrEmpty(Username))
            {
                ViewBag.CallJavaScriptFunction = TempData["CallJavaScriptFunction"] ?? "False";
                var HasLandingSession = Request.Cookies["HasLandingSession"]?.Value;
                if (!string.IsNullOrEmpty(HasLandingSession) && HasLandingSession.Trim() == "True")
                {
                    this.ClearSessionData(false);
                    HttpCookie LandingSession = new HttpCookie("HasLandingSession", "True")
                    {
                        Expires = DateTime.Now.AddDays(1)
                    };
                    HttpContext.Response.Cookies.Add(LandingSession);
                }
                else this.ClearSessionData();
                LoginRequestModel Response = new LoginRequestModel();
                HttpCookie cookie = Request.Cookies["CRS-CUSTOMER-LOGINID"];
                if (cookie != null) Response.LoginId = cookie.Value.DefaultDecryptParameter() ?? null;
                return View(Response);
            }
            else return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Index(LoginRequestModel Model, bool RememberMe = false)
        {
            if (ModelState.IsValid)
            {
                var loginResponse = Login(Model);
                if (loginResponse.Item3 && !string.IsNullOrEmpty(Model.LoginId) && RememberMe)
                {
                    HttpContext.Response.Cookies.Add(new HttpCookie("CRS-CUSTOMER-LOGINID", Model.LoginId.DefaultEncryptParameter())
                    {
                        Expires = DateTime.Now.AddMonths(1)
                    });
                }
                else
                {
                    var LoginId = string.Empty;
                    HttpCookie cookie = Request.Cookies["CRS-CUSTOMER-LOGINID"];
                    if (cookie != null) LoginId = cookie.Value.DefaultDecryptParameter() ?? null;
                    HttpContext.Response.Cookies.Add(new HttpCookie("CRS-CUSTOMER-LOGINID", "")
                    {
                        Expires = DateTime.Now.AddMonths(-1)
                    });
                }
                return RedirectToAction(loginResponse.Item1, loginResponse.Item2);
            }
            else
            {
                var errorMessages = ModelState.Where(x => x.Value.Errors.Count > 0)
                                    //.SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                                    .SelectMany(x => x.Value.Errors.Select(e => $"{e.ErrorMessage}"))
                                    .ToList();

                var notificationModels = errorMessages.Select(errorMessage => new NotificationModel
                {
                    NotificationType = NotificationMessage.ERROR,
                    Message = errorMessage,
                    Title = NotificationMessage.ERROR.ToString(),
                }).ToArray();

                AddNotificationMessage(notificationModels);
                return View(Model);
            }
        }

        public Tuple<string, string, bool> Login(LoginRequestModel Model)
        {
            LoginRequestCommon commonRequest = Model.MapObject<LoginRequestCommon>();
            commonRequest.SessionId = Session.SessionID;
            var dbResponse = _buss.Login(commonRequest);
            try
            {
                var FileLocationPath = "";
                if (ConfigurationManager.AppSettings["Phase"] != null && ConfigurationManager.AppSettings["Phase"].ToString().ToUpper() != "DEVELOPMENT") FileLocationPath = ConfigurationManager.AppSettings["ImageVirtualPath"].ToString();
                if (dbResponse.Code == 0)
                {
                    var response = dbResponse.Data.MapObject<LoginResponseModel>();
                    Session["SessionGuid"] = commonRequest.SessionId;
                    Session["UserId"] = response.UserId.EncryptParameter();
                    Session["AgentId"] = response.AgentId.EncryptParameter();
                    Session["Username"] = response.NickName;
                    Session["EmailAddress"] = response.EmailAddress;
                    var ProfileImage = !string.IsNullOrEmpty(response.ProfileImage) ? FileLocationPath + response.ProfileImage : "/Content/assets/images/customer/demo-image.jpeg";
                    Session["ProfileImage"] = ProfileImage;
                    Session["CreatedOn"] = response.ActionDate;
                    Session["SystemLinkModel"] = response.SystemLink;
                    return new Tuple<string, string, bool>("Index", "Dashboard", true);
                }
                this.AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Message,
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return new Tuple<string, string, bool>("Index", "Home", false);
            }
            catch (Exception)
            {
                this.AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Something went wrong",
                    Title = NotificationMessage.INFORMATION.ToString(),
                });

                return new Tuple<string, string, bool>("Index", "Home", false);
            }
        }

        [OverrideActionFilters]
        public ActionResult LogOff()
        {
            TempData["CallJavaScriptFunction"] = "True";
            Session["AgentId"] = null;
            Session["Username"] = null;
            Session["HasLandingSession"] = null;
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Forgot Password
        public ActionResult ForgotPassword()
        {
            Session.Clear();
            ForgotPasswordModel model = new ForgotPasswordModel();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordModel model, string timet)
        {
            DateTime starttime;
            try
            {
                starttime = DateTime.ParseExact(timet, "yyyy-MM-dd HH:mm:ss:fff", null);
            }
            catch (Exception ex)
            {

                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Something went Wrong" + ex,
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return View();
            }

            if (ModelState.IsValid)
            {
                ForgotPasswordCommon fpc = new ForgotPasswordCommon();
                fpc = model.MapObject<ForgotPasswordCommon>();
                fpc.CreatedPlatform = "CUSTOMER";
                CommonDbResponse dbresp = _buss.ForgotPassword(fpc);
                if (dbresp.Code == 0)
                {
                    var otpModel = new RegistrationModel()
                    {
                        AgentId = dbresp.Extra1.EncryptParameter(),
                        MobileNumber = model.MobileNo
                    };
                    Session["exptime"] = DateTime.Parse(DateTime.Now.AddMinutes(2).ToString()).ToString("yyyy-MM-dd HH:mm:ss");//DateTime.Parse(dbresp.Extra2.ToString());
                    //Session["exptime"] = DateTime.Parse(starttime.ToString());
                    return View("ForgotPasswordOTP", otpModel);
                }
                else
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = dbresp.Message,
                        Title = NotificationMessage.INFORMATION.ToString(),
                    });
                    return View();
                }
            }
            return View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult VerifyforgotpasswordOTP(RegistrationModel Model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(Model.OTP1) || string.IsNullOrEmpty(Model.OTP2) || string.IsNullOrEmpty(Model.OTP3) || string.IsNullOrEmpty(Model.OTP4) || string.IsNullOrEmpty(Model.OTP5) || string.IsNullOrEmpty(Model.OTP5))
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = "OTP code missing",
                        Title = NotificationMessage.INFORMATION.ToString(),
                    });
                    return View("ForgotPasswordOTP", Model);
                }
                RegistrationCommon Common = Model.MapObject<RegistrationCommon>();
                Common.VerificationCode = string.Concat(Model.OTP1, Model.OTP2, Model.OTP3, Model.OTP4, Model.OTP5, Model.OTP6);
                Common.AgentId = Common.AgentId.DecryptParameter();
                Common.ActionIP = ApplicationUtilities.GetIP();
                Common.ActionUser = Common.MobileNumber;
                var dbResponse = _buss.ForgotPasswordOTP(Common);

                if (dbResponse.Code == 0)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.SUCCESS,
                        Message = dbResponse.Message ?? "Success",
                        Title = NotificationMessage.SUCCESS.ToString(),
                    });
                    return RedirectToAction("SetNewPassword", "Home", new { AgentId = dbResponse.Extra1.EncryptParameter(), MobileNumber = Model.MobileNumber.EncryptParameter(), UserID = dbResponse.Extra3.EncryptParameter() });
                }
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Message ?? "Failed",
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return View("ForgotPasswordOTP", Model);
            }
            var errorMessages = ModelState.Where(x => x.Value.Errors.Count > 0)
                                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                                    .ToList();

            var notificationModels = errorMessages.Select(errorMessage => new NotificationModel
            {
                NotificationType = NotificationMessage.ERROR,
                Message = errorMessage,
                Title = NotificationMessage.ERROR.ToString(),
            }).ToArray();
            AddNotificationMessage(notificationModels);
            return RedirectToAction("ForgotPassword", "Home");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ResendFPOTP(string AgentId, string MobileNumber)
        {
            var dbRequest = new ResendRegistrationOTPCommon()
            {
                AgentId = AgentId?.DefaultDecryptParameter(),
                MobileNumber = MobileNumber
            };
            var message = string.Empty;
            if (string.IsNullOrEmpty(dbRequest.AgentId) || string.IsNullOrEmpty(dbRequest.MobileNumber))
            {
                //AddNotificationMessage(new NotificationModel()
                //{
                //    NotificationType = NotificationMessage.INFORMATION,
                //    Message = "Invalid detail",
                //    Title = NotificationMessage.INFORMATION.ToString(),
                //});
                //return Json("Invalid detail", JsonRequestBehavior.AllowGet);
                message = "Invalid detail";
                return Json(new { code = "1", message }, JsonRequestBehavior.AllowGet);
            }
            CommonDbResponse dbResponse = _buss.ResendForgotPasswordOTP(dbRequest);
            if (dbResponse != null && dbResponse.Code == ResponseCode.Success)
            {
                //Session["exptime"] = !string.IsNullOrEmpty(dbResponse.Extra2) ? DateTime.Parse(dbResponse.Extra2.ToString()) : DateTime.Parse(DateTime.Now.AddMinutes(2).ToString());
                //AddNotificationMessage(new NotificationModel()
                //{
                //    NotificationType = NotificationMessage.SUCCESS,
                //    Message = dbResponse.Message ?? "SUCCESS",
                //    Title = NotificationMessage.SUCCESS.ToString(),
                //});
                message = dbResponse.Message ?? "SUCCESS";
                //var ExpTime = !string.IsNullOrEmpty(dbResponse.Extra2) ? DateTime.Parse(dbResponse.Extra2.ToString()).ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Parse(DateTime.Now.AddMinutes(2).ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                var ExpTime = DateTime.Parse(DateTime.Now.AddMinutes(2).ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                return Json(new { code = "0", message, ExpTime }, JsonRequestBehavior.AllowGet);
            }
            //else
            //{
            //    AddNotificationMessage(new NotificationModel()
            //    {
            //        NotificationType = NotificationMessage.INFORMATION,
            //        Message = dbResponse.Message ?? "Failed",
            //        Title = NotificationMessage.INFORMATION.ToString(),
            //    });
            //}
            message = dbResponse.Message ?? "Failed";
            return Json(new { code = "1", message }, JsonRequestBehavior.AllowGet);
            //return Json(dbResponse.Message, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Set new Password
        [HttpGet]
        public ActionResult SetNewPassword(string AgentId, string MobileNumber, string UserID)
        {
            var aId = !string.IsNullOrEmpty(AgentId) ? AgentId.DecryptParameter() : null;
            var mn = !string.IsNullOrEmpty(MobileNumber) ? MobileNumber.DecryptParameter() : null;
            var uId = !string.IsNullOrEmpty(UserID) ? UserID.DecryptParameter() : null;
            if (string.IsNullOrEmpty(aId) || string.IsNullOrEmpty(mn) || string.IsNullOrEmpty(uId))
            {
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Invalid request",
                    Title = NotificationMessage.INFORMATION.ToString()
                });
                return RedirectToAction("Index", "Home");
            }
            var response = new SetRegistrationPasswordModel()
            {
                AgentId = AgentId,
                UserId = UserID,
                MobileNumber = MobileNumber
            };
            return View(response);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SetNewPassword(SetRegistrationPasswordModel Model)
        {
            if (ModelState.IsValid)
            {
                var aId = !string.IsNullOrEmpty(Model.AgentId) ? Model.AgentId.DecryptParameter() : null;
                var mn = !string.IsNullOrEmpty(Model.MobileNumber) ? Model.MobileNumber.DecryptParameter() : null;
                var uId = !string.IsNullOrEmpty(Model.UserId) ? Model.UserId.DecryptParameter() : null;
                if (string.IsNullOrEmpty(aId) || string.IsNullOrEmpty(mn) || string.IsNullOrEmpty(uId))
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = "Invalid request",
                        Title = NotificationMessage.INFORMATION.ToString()
                    });
                    return RedirectToAction("Index", "Home");
                }
                if (Model.Password != Model.ConfirmPassword)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = "Password and confirm password must match",
                        Title = NotificationMessage.INFORMATION.ToString()
                    });
                    return View(Model);
                }
                if (Model.Password == null || Model.ConfirmPassword == null)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = "Password is required",
                        Title = NotificationMessage.INFORMATION.ToString()
                    });
                    return View(Model);
                }
                SetRegistrationPasswordCommon Common = Model.MapObject<SetRegistrationPasswordCommon>();
                Common.AgentId = Common.AgentId.DecryptParameter();
                Common.UserId = Common.UserId.DecryptParameter();
                Common.ActionIP = ApplicationUtilities.GetIP();
                Common.ActionUser = Model.MobileNumber;
                var dbResponse = _buss.SetNewPassword(Common);
                if (dbResponse.Code == 0)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.SUCCESS,
                        Message = dbResponse.Message ?? "Success",
                        Title = NotificationMessage.SUCCESS.ToString(),
                    });
                    return RedirectToAction("Index", "Home");
                }
                AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Message ?? "Failed",
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return View(Model);
            }
            else
            {
                var errorMessages = ModelState.Where(x => x.Value.Errors.Count > 0)
                                   .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                                   .ToList();
                var notificationModels = errorMessages.Select(errorMessage => new NotificationModel
                {
                    NotificationType = NotificationMessage.ERROR,
                    Message = errorMessage,
                    Title = NotificationMessage.ERROR.ToString(),
                }).ToArray();
                AddNotificationMessage(notificationModels);
                return View(Model);
            }
        }
        #endregion
    }
}