using CRS.CUSTOMER.APPLICATION.Helper;
using CRS.CUSTOMER.APPLICATION.Library;
using CRS.CUSTOMER.APPLICATION.Models.Home;
using CRS.CUSTOMER.BUSINESS.CommonManagement;
using CRS.CUSTOMER.BUSINESS.Home;
using CRS.CUSTOMER.SHARED;
using CRS.CUSTOMER.SHARED.Home;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CRS.CUSTOMER.APPLICATION.Controllers
{
    [OverrideActionFilters]
    public class HomeController : CustomController
    {
        private readonly IHomeBusiness _buss;
        private readonly ICommonManagementBusiness _commonBusiness;
        public HomeController(IHomeBusiness buss, ICommonManagementBusiness commonBusiness)
        {
            _buss = buss;
            _commonBusiness = commonBusiness;
        }
        #region Landing Page
        //[OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        //[HttpGet]
        //public ActionResult HomePage()
        //{
        //    var Username = ApplicationUtilities.GetSessionValue("Username").ToString();
        //    if (!string.IsNullOrEmpty(Username))
        //        return Redirect("/");
        //    var culture = Request.Cookies["culture"]?.Value;
        //    culture = string.IsNullOrEmpty(culture) ? "ja" : culture;
        //    ViewBag.Language = culture;
        //    return View();
        //}

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
        [HttpGet, Route("user/register")]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Register(string ReferCode = "", string Type = "")
        {

            var Username = ApplicationUtilities.GetSessionValue("Username").ToString();
            //var advertisementimage = _commonBusiness.GetAdvertisement();
            //advertisementimage = advertisementimage
            //     .Select(item => ImageHelper.ProcessedImage(item))
            //     .ToList();
            //Session["AdvertisementImage"] = advertisementimage;
            if (!string.IsNullOrEmpty(ReferCode) || !string.IsNullOrEmpty(Type))
            {
                ViewBag.ReferCode = ReferCode;
                ViewBag.Type = Type;
            }
            if (!string.IsNullOrEmpty(Username))
                return Redirect("/");
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
                    //AddNotificationMessage(new NotificationModel()
                    //{
                    //    NotificationType = NotificationMessage.SUCCESS,
                    //    Message = dbReferralRes.Message,
                    //    Title = NotificationMessage.SUCCESS.ToString(),
                    //});
                    return View(Response);
                }
                else
                {

                    //AddNotificationMessage(new NotificationModel()
                    //{
                    //    NotificationType = NotificationMessage.INFORMATION,
                    //    Message = dbReferralRes.Message,
                    //    Title = NotificationMessage.INFORMATION.ToString(),
                    //});
                    return Redirect("/user/register");
                }
            }
            return View(Response);
        }

        [HttpPost, Route("user/register"), ValidateAntiForgeryToken]
        public ActionResult Register(RegistrationHoldModel Model, string ReferCode = "", string Type = "")
        {
            ViewBag.ReferCode = ReferCode;
            ViewBag.Type = Type;
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
                        MobileNumber = Model.MobileNumber,
                        NickName = Model.NickName,
                        ProcessId = dbResponse.Extra3
                    };
                    TempData["ReferCode"] = ReferCode;
                    TempData["Type"] = Type;
                    Session["exptime"] = DateTime.Parse(dbResponse.Extra2.ToString());
                    //Session["exptime"] = DateTime.Parse(DateTime.UtcNow.AddMinutes(10).ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.INFORMATION,
                        Message = dbResponse.Message ?? "認証コードが送信されました",
                        Title = NotificationMessage.INFORMATION.ToString(),
                    });
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
        public async Task<ActionResult> VerifyOTP(RegistrationModel Model)
        {
            var ReferCode = string.Empty;
            var Type = string.Empty;
            if (TempData.ContainsKey("ReferCode")) ReferCode = TempData["ReferCode"] as string;
            if (TempData.ContainsKey("Type")) Type = TempData["Type"] as string;
            if (ModelState.IsValid)
            {
                RegistrationCommon Common = Model.MapObject<RegistrationCommon>();
                Common.VerificationCode = Model.OTPCode;
                Common.AgentId = Common.AgentId.DefaultDecryptParameter();
                Common.ActionIP = ApplicationUtilities.GetIP();
                Common.ActionUser = Common.MobileNumber;
                if (!string.IsNullOrEmpty(ReferCode))
                    Common.ReferCode = ReferCode;
                if (!string.IsNullOrEmpty(Type))
                    Common.Type = Type;
                var dbResponse = _buss.Register(Common);
                if (dbResponse.Code == 0)
                {
                    AddNotificationMessage(new NotificationModel()
                    {
                        NotificationType = NotificationMessage.SUCCESS,
                        Message = dbResponse.Message ?? "SUCCESS",
                        Title = NotificationMessage.SUCCESS.ToString(),
                    });
                    return RedirectToAction("SetRegistrationPassword", "Home", new { AgentId = dbResponse.Extra1.DefaultEncryptParameter(), UserId = dbResponse.Extra2.DefaultEncryptParameter(), MobileNumber = Model.MobileNumber, NickName = Model.NickName, ProcessId = Model.ProcessId });
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
                //var ExpTime = DateTime.Parse(DateTime.Now.AddMinutes(10).ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                var ExpTime = !string.IsNullOrEmpty(dbResponse.Extra2) ? DateTime.Parse(dbResponse.Extra2.ToString()).ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Parse(DateTime.Now.AddMinutes(10).ToString()).ToString("yyyy-MM-dd HH:mm:ss"); ;
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
        public ActionResult SetRegistrationPassword(string AgentId, string UserId, string MobileNumber, string NickName, string ProcessId)
        {
            var response = new SetRegistrationPasswordModel()
            {
                AgentId = AgentId,
                UserId = UserId,
                MobileNumber = MobileNumber,
                NickName = NickName,
                ProcessId = ProcessId
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
                Common.AgentId = Common.AgentId.DefaultDecryptParameter();
                Common.UserId = Common.UserId.DefaultDecryptParameter();
                Common.ActionIP = ApplicationUtilities.GetIP();
                Common.ActionUser = Model.MobileNumber;
                ViewBag.NickName = Model.NickName;
                ViewBag.NickName1 = Model.NickName.EncryptParameter();
                var dbResponse = _buss.SetRegistrationPassword(Common);
                if (dbResponse.Code == 0)
                {
                    return Redirect("/user/remind/complete?nickname=" + ViewBag.NickName1);
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
                return View(Model);
            }
        }

        [HttpGet, Route("user/register/complete"), OverrideActionFilters]
        public ActionResult RegisterSuccessView()
        {
            return View();
        }
        #endregion

        #region Login Management
        [HttpGet, Route("login")]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Index(string ReturnURL = "")
        {
            ViewBag.ReturnURL = (!string.IsNullOrEmpty(ReturnURL) && Url.IsLocalUrl(ReturnURL)) ? ReturnURL : string.Empty;
            var Username = ApplicationUtilities.GetSessionValue("Username").ToString();
            string phaseValue = ConfigurationManager.AppSettings["phase"];

            if (string.IsNullOrEmpty(Username))
            {

                //ViewBag.CallJavaScriptFunction = TempData["CallJavaScriptFunction"] ?? "False";
                ViewBag.CallJavaScriptFunction = TempData["CallJavaScriptFunction"] ?? "True";
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


                if (phaseValue.ToUpper() == "DEVELOPMENT")
                {
                    Response.affiliateURL = "http://43.207.72.221:93/";
                    Response.clubURL = "http://43.207.72.221:92/";
                }
                else
                {
                    Response.affiliateURL = "https://affiliate.hoslog.jp";
                    Response.clubURL = "https://club.hoslog.jp";
                }
                HttpCookie cookie = Request.Cookies["CRS-CUSTOMER-LOGINID"];
                if (cookie != null) Response.LoginId = cookie.Value.DefaultDecryptParameter() ?? null;

                return View(Response);
            }
            else return Redirect("/");
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Route("login")]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Index(LoginRequestModel Model, bool RememberMe = false, string ReturnURL = "")
        {
            ViewBag.ReturnURL = (!string.IsNullOrEmpty(ReturnURL) && Url.IsLocalUrl(ReturnURL)) ? ReturnURL : string.Empty;
            if (ModelState.IsValid)
            {
                var loginResponse = Login(Model);
                if (loginResponse.Item2 && !string.IsNullOrEmpty(Model.LoginId) && RememberMe)
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
                string IsPasswordForceful = Convert.ToString(Session["IsPasswordForceful"]);
                if (!string.IsNullOrEmpty(IsPasswordForceful) && IsPasswordForceful == "Y")
                    return RedirectToAction("SetNewPasswordV2", "Home", new { AgentId = Session["AgentId"], MobileNumber = Session["MobileNumber"], UserID = Session["UserId"], NickName = Session["Username"], ReturnUrl = ReturnURL });

                if (loginResponse.Item2)
                {
                    if (!string.IsNullOrEmpty(ReturnURL) && Url.IsLocalUrl(ReturnURL))
                    {
                        return Redirect(ReturnURL);
                    }
                    return Redirect(loginResponse.Item1);
                }
                //return Redirect(loginResponse.Item1,new { ReturnURL });
                return Redirect(loginResponse.Item1 + "?ReturnURL=" + Uri.EscapeDataString(ReturnURL));

            }
            else
            {
                var errorMessages = ModelState.Where(x => x.Value.Errors.Count > 0)
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

        public Tuple<string, bool> Login(LoginRequestModel Model)
        {
            LoginRequestCommon commonRequest = Model.MapObject<LoginRequestCommon>();
            commonRequest.SessionId = Session.SessionID;
            commonRequest.ActionIP = ApplicationUtilities.GetIP();
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
                    Session["ProfileImage"] = ImageHelper.ProcessedImage(response.ProfileImage);
                    Session["CreatedOn"] = response.ActionDate;
                    Session["SystemLinkModel"] = response.SystemLink;
                    Session["IsPasswordForceful"] = response.IsPasswordForceful;
                    Session["MobileNumber"] = response.MobileNumber.EncryptParameter();
                    Session["Amount"] = response.Amount;
                    Session["CustomerCode"] = response.CustomerCode;
                    Session["NotificationUnReadCount"] = response.NotificationUnReadCount;
                    return new Tuple<string, bool>("/", true);
                }
                this.AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = dbResponse.Message,
                    Title = NotificationMessage.INFORMATION.ToString(),
                });
                return new Tuple<string, bool>("login", false);
            }
            catch (Exception)
            {
                this.AddNotificationMessage(new NotificationModel()
                {
                    NotificationType = NotificationMessage.INFORMATION,
                    Message = "Something went wrong",
                    Title = NotificationMessage.INFORMATION.ToString(),
                });

                return new Tuple<string, bool>("login", false);
            }
        }

        [OverrideActionFilters]
        public ActionResult LogOff()
        {
            TempData["CallJavaScriptFunction"] = "True";
            Session["AgentId"] = null;
            Session["Username"] = null;
            Session["HasLandingSession"] = null;
            return Redirect("/login");
        }
        #endregion

        #region Forgot Password
        [HttpGet, Route("user/remind")]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult ForgotPassword()
        {
            this.ClearSessionData();
            ForgotPasswordModel model = new ForgotPasswordModel();
            return View(model);
        }
        [HttpPost, Route("user/remind")]
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
                        MobileNumber = model.MobileNo,
                        NickName = dbresp.Extra3,
                        ProcessId = dbresp.Extra4
                    };
                    Session["exptime"] = DateTime.Parse(dbresp.Extra2.ToString());
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
                if (string.IsNullOrEmpty(Model.OTPCode))
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
                Common.VerificationCode = Model.OTPCode;
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
                    return RedirectToAction("SetNewPasswordV2", "Home", new { AgentId = dbResponse.Extra1.EncryptParameter(), MobileNumber = Model.MobileNumber.EncryptParameter(), UserID = dbResponse.Extra3.EncryptParameter(), NickName = Model.NickName, ProcessId = Model.ProcessId });
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
                message = "Invalid detail";
                return Json(new { code = "1", message }, JsonRequestBehavior.AllowGet);
            }
            CommonDbResponse dbResponse = _buss.ResendForgotPasswordOTP(dbRequest);
            if (dbResponse != null && dbResponse.Code == ResponseCode.Success)
            {
                message = dbResponse.Message ?? "SUCCESS";
                var ExpTime = DateTime.Parse(dbResponse.Extra2.ToString());
                return Json(new { code = "0", message, ExpTime }, JsonRequestBehavior.AllowGet);
            }
            message = dbResponse.Message ?? "Failed";
            return Json(new { code = "1", message }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Set new Password
        [HttpGet]
        public ActionResult SetNewPasswordV2(string AgentId, string MobileNumber, string UserID, string NickName, string ReturnUrl = null, string ProcessId = null)
        {
            TempData["returnURLAfterReset"] = null;
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
                return RedirectToAction("login");
            }
            var response = new SetRegistrationPasswordModel()
            {
                AgentId = AgentId,
                UserId = UserID,
                MobileNumber = MobileNumber,
                NickName = NickName,
                ProcessId = ProcessId
            };
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                TempData["returnURLAfterReset"] = ReturnUrl;
            }
            return View(response);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SetNewPasswordV2(SetRegistrationPasswordModel Model)
        {
            if (ModelState.IsValid)
            {
                var aId = !string.IsNullOrEmpty(Model.AgentId) ? Model.AgentId.DecryptParameter() : null;
                var mn = !string.IsNullOrEmpty(Model.MobileNumber) ? Model.MobileNumber.DecryptParameter() : null;
                var uId = !string.IsNullOrEmpty(Model.UserId) ? Model.UserId.DecryptParameter() : null;
                if (string.IsNullOrEmpty(aId) || string.IsNullOrEmpty(mn) || string.IsNullOrEmpty(uId))
                {
                    TempData["ForgetPWErrorMessage"] = "Invalid request";
                    return RedirectToAction("login");
                }
                if (Model.Password != Model.ConfirmPassword)
                {
                    TempData["ForgetPWErrorMessage"] = "Password and confirm password must match";
                    return View(Model);
                }
                if (Model.Password == null || Model.ConfirmPassword == null)
                {
                    TempData["ForgetPWErrorMessage"] = "Password is required";
                    return View(Model);
                }
                SetRegistrationPasswordCommon Common = Model.MapObject<SetRegistrationPasswordCommon>();
                Common.AgentId = Common.AgentId.DecryptParameter();
                Common.UserId = Common.UserId.DecryptParameter();
                Common.ActionIP = ApplicationUtilities.GetIP();
                Common.ActionUser = Model.MobileNumber;
                ViewBag.NickName = Model.NickName;
                ViewBag.NickName1 = Model.NickName.EncryptParameter();
                if (!string.IsNullOrEmpty(Convert.ToString(TempData["returnURLAfterReset"])))
                {

                    Model.IsPasswordForceful = "N";
                }
                Common.IsPasswordForceful = Model.IsPasswordForceful;
                var dbResponse = _buss.SetNewPassword(Common);
                if (dbResponse.Code == 0)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(Model.IsPasswordForceful)))
                    {
                        var returnurl = TempData["returnURLAfterReset"];
                        TempData["returnURLAfterReset"] = null;
                        this.ClearSessionData();
                        return RedirectToAction("Index", "Home", new { ReturnURL = returnurl });
                    }
                    //return Redirect("/user/remind/complete?nickname ='" + ViewBag.NickName + "'");
                    return Redirect("/user/remind/complete?nickname=" + ViewBag.NickName1);
                }
                TempData["ForgetPWErrorMessage"] = dbResponse.Message ?? "Failed";
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

        [HttpGet, Route("user/remind/complete"), OverrideActionFilters]
        public ActionResult ForgotPasswordSuccessView(string nickname)
        {
            TempData["CallJavaScriptFunction"] = "True";
            Session["AgentId"] = null;
            Session["Username"] = null;
            Session["HasLandingSession"] = null;
            ViewBag.NickName = !string.IsNullOrEmpty(nickname) ? nickname.DecryptParameter() : "";
            return View();
        }
        #endregion
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Error505()
        {

            return View();
        }
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Error404()
        {

            return View();
        }
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Maintenance()
        {

            return View();
        }
        public ActionResult GetGoogleMapData(string libraries, string callback)
        {
            string googleApiKey = ConfigurationManager.AppSettings["GoogleMapKey"];
            string queryParams = $"libraries={libraries}&callback={callback}";
            string url = "";
            if (!string.IsNullOrEmpty(libraries) && !string.IsNullOrEmpty(callback))
                url = $"https://maps.googleapis.com/maps/api/js?key={googleApiKey}&{queryParams}";
            else
                url = $"https://maps.googleapis.com/maps/api/js?key={googleApiKey}";

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;
                var data = response.Content.ReadAsStringAsync().Result;
                return Content(data, "application/javascript");
            }
        }

    }
}