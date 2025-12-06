using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{

	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;

		public BrandController(DataContext context)
		{
			_dataContext = context;
		}

		public async Task<IActionResult> Index(string slug = "")
		{
			BrandModel brand = _dataContext.Brands.Where(c => c.Slug == slug).FirstOrDefault();

			if (brand == null)
			{
				return RedirectToAction("Index");
			}
			
			var productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);
			ViewBag.Slug = slug;
			return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
		}
	}
}
