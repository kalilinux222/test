using Nop.Admin.Controllers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Security;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers.Admin
{
    public class PopularCategoriesAdminController : BaseAdminController
    {
        private readonly PopularCategoriesSettings _popularCategoriesSettings;
        private readonly ICategoryService _categoryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ISettingService _settingService;

        public PopularCategoriesAdminController(
            PopularCategoriesSettings popularCategoriesSettings,
            ICategoryService categoryService,
            IGenericAttributeService genericAttributeService,
            ISettingService settingService)
        {
            _popularCategoriesSettings = popularCategoriesSettings;
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
            IList<int> categories = _popularCategoriesSettings.Categories;

            var allCategories = _categoryService.GetAllCategories(showHidden: true);

            var categoryModels = new List<PopularCategoryModel>();

            foreach (var categoryId in categories)
            {
                var category = allCategories.FirstOrDefault(x => x.Id == categoryId);

                if (category == null)
                    continue;

                var popularCategoryIds = category.GetAttribute<List<string>>("PopularCategoryIds");
                var popularCategories = allCategories.Where(x => popularCategoryIds.Contains(x.Id.ToString()));
                var popularCategoryNames = popularCategories.Select(x => x.Name);

                var model = new PopularCategoryModel
                {
                    Id = category.Id,
                    CategoryName = category.GetFormattedBreadCrumb(allCategories),
                    PopularCategories = string.Join(", ", popularCategoryNames),
                    PopularCategoryIds = popularCategoryIds
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
        public ActionResult Update(DataSourceRequest command, int id, List<string> popularCategoryIds)
        {
            if (id == 0)
                return List(command);

            var category = _categoryService.GetCategoryById(id);

            if (category == null)
                return List(command);

            IList<int> categories = _popularCategoriesSettings.Categories;

            if (categories.Contains(id))
            {
                _genericAttributeService.SaveAttribute(category, "PopularCategoryIds", popularCategoryIds);
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

            IList<int> categories = _popularCategoriesSettings.Categories;

            if (categories.Contains(id))
            {
                _genericAttributeService.SaveAttribute<string>(category, "PopularCategoryIds", null);

                _popularCategoriesSettings.Categories.Remove(id);

                _settingService.SaveSetting(_popularCategoriesSettings);
            }

            return List(command);
        }

        [HttpPost]
        public ActionResult Create(DataSourceRequest command, int categoryName, List<string> popularCategoryIds)
        {
            var id = categoryName;

            var category = _categoryService.GetCategoryById(id);

            if (category == null)
                return List(command);

            IList<int> categories = _popularCategoriesSettings.Categories;

            if (!categories.Contains(id))
            {
                _genericAttributeService.SaveAttribute(category, "PopularCategoryIds", popularCategoryIds);

                _popularCategoriesSettings.Categories.Add(category.Id);

                _settingService.SaveSetting(_popularCategoriesSettings);
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

        public ActionResult GetPopularCategories(string popularCategoryIds)
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