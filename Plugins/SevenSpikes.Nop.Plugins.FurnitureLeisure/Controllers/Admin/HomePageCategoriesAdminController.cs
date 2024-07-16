using Nop.Admin.Controllers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Seo;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Security;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers.Admin
{
    public class HomePageCategoriesAdminController : BaseAdminController
    {
        private readonly HomePageCategoriesSettings _homePageCategoriesSettings;
        private readonly ICategoryService _categoryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ISettingService _settingService;

        public HomePageCategoriesAdminController(
            HomePageCategoriesSettings homePageCategoriesSettings, 
            ICategoryService categoryService, 
            IGenericAttributeService genericAttributeService, 
            ISettingService settingService)
        {
            _homePageCategoriesSettings = homePageCategoriesSettings;
            _categoryService = categoryService;
            _genericAttributeService = genericAttributeService;
            _settingService = settingService;
        }

        public ActionResult List()
        {
            return View("List");
        }

        [HttpPost]
        [AdminAntiForgery(true)]
        public ActionResult List(DataSourceRequest command)
        {
            IList<int> categories = _homePageCategoriesSettings.Categories;

            var allCategories = _categoryService.GetAllCategories(showHidden: true);

            var categoryModels = new List<HomePageCategoryModel>();

            foreach (var categoryId in categories)
            {
                var category = allCategories.FirstOrDefault(x => x.Id == categoryId);

                if (category == null)
                    continue;

                var categoryDescription = category.GetAttribute<string>("CustomDescription");

                var model = new HomePageCategoryModel
                {
                    Id = category.Id,
                    CategorySeName = category.GetSeName(),
                    CategoryName = category.GetFormattedBreadCrumb(allCategories),
                    CategoryDescription = categoryDescription
                };

                categoryModels.Add(model);
            }

            var gridModel = new DataSourceResult
            {
                Data = categoryModels,
                Total = categoryModels.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult Update(DataSourceRequest command, int id, string categoryDescription)
        {
            if (id == 0)
                return List(command);

            var category = _categoryService.GetCategoryById(id);

            if (category == null)
                return List(command);

            IList<int> categories = _homePageCategoriesSettings.Categories;

            if (categories.Contains(id))
            {
                _genericAttributeService.SaveAttribute(category, "CustomDescription", categoryDescription);
            }

            return List(command);
        }

        [HttpPost]
        public ActionResult Delete(DataSourceRequest command, int id)
        {
            if (id == 0)
                return List(command);

            var category = _categoryService.GetCategoryById(id);

            if (category == null)
                return List(command);

            IList<int> categories = _homePageCategoriesSettings.Categories;

            if (categories.Contains(id))
            {
                _genericAttributeService.SaveAttribute<string>(category, "CustomDescription", null);

                _homePageCategoriesSettings.Categories.Remove(id);

                _settingService.SaveSetting(_homePageCategoriesSettings);
            }

            return List(command);
        }

        [HttpPost]
        public ActionResult Create(DataSourceRequest command, int categoryName, string categoryDescription)
        {
            var id = categoryName;

            var category = _categoryService.GetCategoryById(id);

            if (category == null)
                return List(command);

            IList<int> categories = _homePageCategoriesSettings.Categories;

            if (!categories.Contains(id))
            {
                _genericAttributeService.SaveAttribute(category, "CustomDescription", categoryDescription);

                _homePageCategoriesSettings.Categories.Add(category.Id);

                _settingService.SaveSetting(_homePageCategoriesSettings);
            }

            return List(command);
        }

        public ActionResult GetCategories()
        {
            IList<SelectListItem> models = new List<SelectListItem>();

            var categories = _categoryService.GetAllCategories(showHidden: true);

            foreach (var c in categories)
            {
                models.Add(new SelectListItem
                {
                    Text = c.GetFormattedBreadCrumb(categories),
                    Value = c.Id.ToString()
                });
            }

            return new JsonResult { Data = models, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}