﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using IotaWebApp.Data;
using IotaWebApp.Models;
using System.Threading.Tasks;  // For Task and async/await

namespace IotaWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebsiteCMSDbContext _context;

        public HomeController(WebsiteCMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var contents = await _context.WebsiteContents.ToListAsync(); 
            return View(contents);
        }
    }
}
