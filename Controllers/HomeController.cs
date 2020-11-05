using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BankAccounts.Models;
using System.Linq;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private BankAccountsContext dbContext;
        public HomeController( BankAccountsContext context)
        {
            dbContext = context;
        }
        ///////////////////////////////////////////// GET HOME PAGE /////////////////////////

       [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
        ///////////////////////////////////////////// POST REGISTRATION /////////////////////////

        [HttpPost("registration")]
        public IActionResult Registration(User UserSubmission)
        {
            System.Console.WriteLine("*************************** Enter Registration Function ***************************");
            if (ModelState.IsValid)
            {
                System.Console.WriteLine("*************************** Is Valid ***************************");
                if(dbContext.User.Any(u => u.Email == UserSubmission.Email))
                {
                    System.Console.WriteLine("*************************** Email already in use error ***************************");
                    ModelState.AddModelError("Email","Email already in use!");
                    return View("Index");
                }
                else
                {
                    System.Console.WriteLine("*************************** Success, Hashing password ***************************");
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    UserSubmission.Password = Hasher.HashPassword(UserSubmission, UserSubmission.Password);
                    dbContext.Add(UserSubmission);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetInt32("UserId", UserSubmission.UserId);
                    return RedirectToAction("accountpage");
                }
            }
            System.Console.WriteLine("*************************** ModelState is not valid ***************************");
            ModelState.AddModelError("Email", "Invalid Email/Password");
            return View("Index");
        }

        ///////////////////////////////////////////// GET LOGIN PAGE /////////////////////////

        [HttpGet("loginpage")]
        public IActionResult LoginPage()
        {
            return View();
        }
        ///////////////////////////////////////////// POST LOGIN /////////////////////////

        [HttpPost("login")]
        public IActionResult Login(LoginUser UserSubmission)
        {
            System.Console.WriteLine("*************************** Enter Registration Function ***************************");
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.User.FirstOrDefault(u => u.Email == UserSubmission.Email);
                if(userInDb == null)
                {
                    System.Console.WriteLine("*************************** Email not in Database error ***************************");
                    ModelState.AddModelError("Email", "Email not in Database");
                    return View("loginpage");
                }            
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(UserSubmission, userInDb.Password, UserSubmission.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("Email","not your Email!");
                    return View("loginpage");
                }
                System.Console.WriteLine("*************************** Success, Hashing password ***************************");
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                return RedirectToAction("AccountPage");
            }
                ModelState.AddModelError("Email", "Invalid Email/Password");
                System.Console.WriteLine("*************************** Not Valid ***************************");
                return View("loginpage");
        }

        ///////////////////////////////////////////// GET ACCOUNT PAGE /////////////////////////

        [HttpGet("accountpage")]
        public IActionResult AccountPage()
        {
            if(HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                
                System.Console.WriteLine("*************************** User in session ***************************");
                ViewBag.Transaction= dbContext.Transaction.Where(a =>a.UserId == HttpContext.Session.GetInt32("UserId"));
                User User=dbContext.User.Include(user => user.Transactions).FirstOrDefault( u => u.UserId == HttpContext.Session.GetInt32("UserId"));
                ViewBag.User = User;
                //total amount
                decimal total =0;
                foreach(Transaction t in User.Transactions)
                {
                    total+= t.Amount;
                };
                ViewBag.Total=total;

                return View();
            }
        }
        ///////////////////////////////////////////// POST TRANSACTION /////////////////////////

        [HttpPost("transactions")]
        public IActionResult Transactions(Transaction transaction)
        {
            decimal f = transaction.Amount;
            decimal truncated = (decimal)(Math.Truncate((double)f*100.0) / 100.0);
            transaction.Amount = (decimal)(Math.Round((double)f, 2));

            int? IntVariable = HttpContext.Session.GetInt32("UserID");
            int userID = IntVariable ?? default(int);

            decimal total =0;
            User User=dbContext.User.Include(user => user.Transactions).FirstOrDefault( u => u.UserId == HttpContext.Session.GetInt32("UserId"));
            foreach(Transaction t in User.Transactions)
            {
                total+= t.Amount;
            };
            if(total + transaction.Amount < 0)
            {
                ModelState.AddModelError("Amount", "You're too poor to withdraw that much!");
                //Viewbags only last one refresh need.
                ViewBag.Transaction= dbContext.Transaction.Where(a =>a.UserId == HttpContext.Session.GetInt32("UserId"));
                ViewBag.User = User;
                ViewBag.Total = total;
                return View("AccountPage");
            };
           
            dbContext.Add(transaction);
            dbContext.SaveChanges();
            return RedirectToAction("AccountPage");
        }



    }
}
