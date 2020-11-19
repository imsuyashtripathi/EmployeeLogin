using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Employee.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;
using Employee.EmployeeReference;
using WCFdemo;

namespace Employee.Controllers
{
    public class UserController : Controller
    {
        //Registration action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration post action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude ="IsEmailVerified,ActivationCode")]Users user)
        {
            bool status = false;
            string message = "";
            //Model validation
            if (ModelState.IsValid)
            {
                //Email already Exists
                #region
                var isExist = IsEmailExist(user.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExists", "Email already exists");
                    return View(user);
                }
                #endregion 

                //generate Activation Code
                #region Generate Activation Code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                //password hashing
                #region Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion

                //Save data to database
                #region Save to Database
                using (SuyashEntities dc=new SuyashEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //send mail to user
                    SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString());
                    message = "Registration Succesfully done.Account activation link" +
                        "has been sent to  your email id:"+user.EmailID;
                    status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            ViewBag.Status=status;

            return View(user);
        }
        //verify mail

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (suyashEntities dc=new suyashEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false;

                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //Login
        [HttpGet]
        public ActionResult Login()
        {
            EmployeeReference.EmployeeClient empClient = new EmployeeReference.EmployeeClient();
            EmployeeInfo employeeinfo = empClient.GetEmployee();
            ViewBag.Message = employeeinfo.ID+":"+employeeinfo.Name;
            return View();
        }

        //Login post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login,string ReturnUrl="")
        {
            string message = "";
            using (SuyashEntities dc=new SuyashEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == login.EmailID).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RemeberMe ? 525600 : 20;
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RemeberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);
                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }       
                    }
                    else
                    {
                        message = "Invalid credential provided";
                    }
                }
                else
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View();
        }
        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login","User");
        }




        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (suyashEntities dc=new suyashEntities())
            {
                var v = dc.Users.Where(a=>a.EmailID==emailID).FirstOrDefault();
                return v != null;
            }
        }
        [NonAction]
        public void SendVerificationLinkEmail(string emailID,string activationCode)
        {
            var verifyurl = "/user/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsolutePath.Replace(Request.Url.PathAndQuery, verifyurl);

            var fromEmail = new MailAddress("suyashtripathi98@gmail.com","HealthAsyst");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "Imsuyashtripathiofficial";
            string subject = "Your Account is successful created!";
            string body = "<br></br>We are excited to tell you that your HealthAsyst account is" +
                "successfully created.Please click on below link to verify your account" +
                "<br/><br/><a href='" + link + "'>"+link+"</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials=true,
                Credentials=new NetworkCredential(fromEmail.Address,fromEmailPassword)
            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
    }
    
}