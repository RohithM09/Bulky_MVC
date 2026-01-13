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
   // [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        { 
            _unitOfWork= unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> CompanyFromDb = _unitOfWork.Company.GetAll().ToList();
           
            return View(CompanyFromDb);
        }
        public IActionResult Upsert(int? id)
        {
           
            
              if (id == null || id == 0)
            {//create
                return View(new Company());
            }
            else
            {//
                Company companyobj = _unitOfWork.Company.Get(i => i.Id == id);
                return View(companyobj);
            }

            
            //ViewBag.CategoryList = CategoryList;
           // ViewData["CategoryList"] = CategoryList;
           
        }
        [HttpPost]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                
                if(obj.Id==0)
                {
                    _unitOfWork.Company.Add(obj);
                }
                else
                {
                    _unitOfWork.Company.Update(obj);
                }

                    _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index", "Company");
            }
            else
            {
               
                return View(obj);
            }
               
        }
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var objFromDb = _unitOfWork.Company.Get(i=>i.Id==id);
            if (objFromDb == null)
            {
                return Json(new { successs = false, message = "Error while deleting" });
            }
            else
            {
               
                _unitOfWork.Company.Remove(objFromDb);
                _unitOfWork.Save();

                return Json(new { success=true,message="Delete successful" });
            }
        }


        #endregion
    }
}
