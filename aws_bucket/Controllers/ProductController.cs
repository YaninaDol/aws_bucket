using Microsoft.AspNetCore.Mvc;
using aws_bucket.Data;
using System.Security.Claims;
using aws_bucket.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace aws_bucket.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/[controller]/")]
    public class ProductController : ControllerBase
    {
        private readonly DbContextClass _context;
        public ProductController(DbContextClass context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("OrdersList")]
        public async Task<ActionResult<IEnumerable<Order>>> OrdersList()
        {
           
            return _context.Orders.ToList();
           
        }

        [HttpGet]
        [Route("ListUsers")]
        public async Task<ActionResult<IEnumerable<UserInfo>>> ListUsers()
        {

            return _context.UserInfos.ToList();

        }



        [HttpPost]
        [Route("buyPackage")]
        public async Task<IActionResult> buyPackage( UserInfo userInfo)
        {
            _context.UserInfos.Where(x => x.Login == userInfo.Login).FirstOrDefault().Package = userInfo.Package;
          _context.Add(new Order() { Login=userInfo.Login, Date=DateTime.Now,Package=userInfo.Package});
            _context.SaveChanges();

            return Ok("Accepted!");
        }
    }
}
