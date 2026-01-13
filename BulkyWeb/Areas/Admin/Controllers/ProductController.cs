using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection.Metadata.Ecma335;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment) 
        {
             _webHostEnvironment = webHostEnvironment;
            _unitOfWork= unitOfWork;
        }
        public IActionResult Index()
        {
            List<Product> ProductFromDb = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
           
            return View(ProductFromDb);
        }
        public IActionResult Upsert(int? id)
        {
           
            ProductVM productVM = new ProductVM();
            productVM.Product = new Product();
            productVM.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
              if (id == null || id == 0)
            {//create
                return View(productVM);
            }
            else
            {//
                productVM.Product = _unitOfWork.Product.Get(i => i.Id == id);
                return View(productVM);
            }

            
            //ViewBag.CategoryList = CategoryList;
           // ViewData["CategoryList"] = CategoryList;
           
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\Product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        //delete old image
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl=@"\images\Product\"+fileName;
                }
                if(obj.Product.Id==0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                }

                    _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index", "Product");
            }
            else
            {
               
              
                obj.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });

                return View(obj);
            }
               
        }
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var objFromDb = _unitOfWork.Product.Get(i=>i.Id==id);
            if (objFromDb == null)
            {
                return Json(new { successs = false, message = "Error while deleting" });
            }
            else
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                _unitOfWork.Product.Remove(objFromDb);
                _unitOfWork.Save();

                return Json(new { success=true,message="Delete successful" });
            }
        }


        #endregion
    }
}
