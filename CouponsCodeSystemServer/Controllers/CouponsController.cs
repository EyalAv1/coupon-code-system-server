// using Microsoft.AspNetCore.Components;
using CouponsCodeSystemServer.Data;
using CouponsCodeSystemServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouponsCodeSystemServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        public CouponsController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost("AddCoupon")]
        public async Task<IActionResult> AddCoupon([FromBody] Coupon coupon)
        {
            if (await CouponCodeExistsAsync(coupon.Code))
            {
                return BadRequest(new { message = "Coupon already exist" });
            }
            _appDbContext.Coupons.Add(coupon);
            await _appDbContext.SaveChangesAsync();

            return Ok(coupon);
        }

        private async Task<bool> CouponCodeExistsAsync(string code)
        {
            return await _appDbContext.Coupons.AnyAsync(c => c.Code == code);
        }

        [HttpGet("AllCoupons")]
        public async Task<IActionResult> GetAllCoupons()
        {
            var coupons = await _appDbContext.Coupons.ToArrayAsync();

            return Ok(coupons);
        }

        [HttpPost("CouponByCode")]
        public async Task<IActionResult> GetCouponByName(string code)
        {
            var coupon = await _appDbContext.Coupons.FirstOrDefaultAsync((c) => c.Code == code);
            if (coupon == null)
            {
                return BadRequest(new { message = "Coupon Not Exist" });
            }
            else
            {
                return Ok(coupon);
            }
        }

        [HttpPost("CouponsByUser")]
        public async Task<IActionResult> GetCouponsByUser(int userId)
        {
            var userCoupons = await _appDbContext.Coupons.Where((c) => c.UserId == userId).ToArrayAsync();
            return Ok(userCoupons);
        }

        [HttpDelete("DeleteCoupon")]
        public async Task<IActionResult> DeleteCoupon(int couponId)
        {
            var coupon = await _appDbContext.Coupons.FirstOrDefaultAsync((c) => c.Id == couponId);
            if (coupon == null)
            {
                return BadRequest(new { message = "Coupon not found" });
            }
            _appDbContext.Coupons.Remove(coupon);
            await _appDbContext.SaveChangesAsync();
            return Ok(coupon);
        }

        [HttpPut("updateCoupon/{Id}")]
        public async Task<IActionResult> UpdateCouponInfo(Coupon updatedCoupon)
        {
            // Fetch the entity by its ID
            var coupon = await _appDbContext.Coupons.FirstOrDefaultAsync((c) => c.Code == updatedCoupon.Code);

            if (coupon == null)
            {
                return NotFound();  // Return a 404 if the entity doesn't exist
            }

            // Modify specific properties only
            coupon.Description = updatedCoupon.Description;
            coupon.DiscountAmount = updatedCoupon.DiscountAmount;
            coupon.UsageLimit = updatedCoupon.UsageLimit;
            coupon.AllowDoublePromotion = updatedCoupon.AllowDoublePromotion;
            coupon.ExpirationDate = updatedCoupon.ExpirationDate;

            // Save changes to the database
            await _appDbContext.SaveChangesAsync();

            return Ok(coupon);  // Return the updated entity or a success response
        }

        [HttpPatch("updateUsageCount")]
        public async Task<IActionResult> UpdateCouponUsageCount(Coupon updatedCoupon)
        {
            // Fetch the entity by its ID
            var coupon = await _appDbContext.Coupons.FirstOrDefaultAsync((c) => c.Code == updatedCoupon.Code);

            if (coupon == null)
            {
                return NotFound();  // Return a 404 if the entity doesn't exist
            }

            if (updatedCoupon.UsageLimit < updatedCoupon.UsageCount)
            {
                return BadRequest(new { nessage = "Coupon Usage Is Over" });
            }

            // Modify specific properties only
            coupon.UsageCount = updatedCoupon.UsageCount;

            // Save changes to the database
            await _appDbContext.SaveChangesAsync();

            return Ok(coupon);  // Return the updated entity or a success response
        }

        [HttpPost("CouponsByDates")]
        public async Task<IActionResult> getCouponsByDate(DateTime startDate, DateTime endDate)
        {
            var coupons = await _appDbContext.Coupons.Where((c) => c.CreatedDate >= startDate && c.CreatedDate <= endDate).ToArrayAsync();
            if (coupons == null)
            {
                return BadRequest("Coupons not found with these dates");
            }
            return Ok(coupons);
        }
    }
}
