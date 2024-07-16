using Nop.Core.Domain.Media;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Web.Controllers;
using Nop.Web.Framework.Security.Captcha;
using SevenSpikes.Nop.Core.Helpers;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Domain.Settings;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Helpers;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure.Constants;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Controllers
{
    public class FurnitureLeisureController : BasePublicController
    {
        private const int UNITED_STATES_COUNTRY_ID = 1;

        private readonly IConvertToDictionaryHelper _convertToDictionaryHelper;
        private readonly IPictureService _pictureService;
        private readonly FurnitureLeisureSettings _furnitureLeisureSettings;
        private readonly ICountryService _countryService;
        private readonly IEmailHelper _emailHelper;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerHelper _customerHelper;

        public FurnitureLeisureController(IConvertToDictionaryHelper convertToDictionaryHelper,
            IPictureService pictureService
            , FurnitureLeisureSettings furnitureLeisureSettings,
            ICountryService countryService,
            IEmailHelper emailHelper,
            ILogger logger,
            ILocalizationService localizationService,
            ICustomerHelper customerHelper)
        {
            _convertToDictionaryHelper = convertToDictionaryHelper;
            _pictureService = pictureService;
            _furnitureLeisureSettings = furnitureLeisureSettings;
            _countryService = countryService;
            _emailHelper = emailHelper;
            _logger = logger;
            _localizationService = localizationService;
            _customerHelper = customerHelper;
        }

        public ActionResult GetForWidgetZone(string widgetZone)
        {
            if (_furnitureLeisureSettings.WidgetZone == widgetZone)
            {
                IList<string> carouselImagePairs = _furnitureLeisureSettings.CarouselImagePairs;

                IDictionary<int, int> carouselImageDictionaryFromSemicolonSeparatedPairs = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPair(carouselImagePairs);

                IList<CarouselImageModel> carouselImageModels = carouselImageDictionaryFromSemicolonSeparatedPairs
                    .Select(x =>
                    {
                        Picture picture = _pictureService.GetPictureById(x.Key);

                        if (picture == null)
                        {
                            throw new Exception("Picture cannot be loaded");
                        }

                        var model = new CarouselImageModel
                        {
                            PictureId = x.Key,
                            PictureUrl = _pictureService.GetPictureUrl(picture),
                            DisplayOrder = carouselImageDictionaryFromSemicolonSeparatedPairs[x.Key]
                        };

                        return model;
                    }).ToList();

                return View(Views.Clients, carouselImageModels);
            }
            else if(widgetZone.Equals("home_page_top") || widgetZone.Equals("categorydetails_before_breadcrumb"))
            {
                if (_furnitureLeisureSettings.EnableCatalogRequestBanner)
                {
                    return DisplayCustomProductCatalogsBanner();
                }
            }

            return new EmptyResult();
        }

        [HttpGet]
        public ActionResult CatalogRequest()
        {
            var states = GetStates();

            var model = new CatalogRequestModel()
            {
                AvailableStates = states
            };

            return View(model);
        }

        [HttpPost]
        [CaptchaValidator]
        public ActionResult CatalogRequest(CatalogRequestModel model, bool captchaValid)
        {
            if (!captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            if (!ModelState.IsValid)
            {
                model.AvailableStates = GetStates();

                return View(model);
            }

            var registerAccount = model.CreateAccount == 2;

            try
            {
                if (registerAccount)
                {
                    var result = _customerHelper.CreateFurnitureLeisureCustomer(model);

                    if (result.Success)
                    {
                        _emailHelper.SendEmail(model);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }

                        model.Password = string.Empty;
                        model.ConfirmPassword = string.Empty;

                        model.AvailableStates = GetStates();

                        return View(model);
                    }
                }
                else
                {
                    _emailHelper.SendEmail(model);
                }
            }
            catch(Exception e)
            {
                _logger.Error(e.Message, e);
            }
            

            return RedirectToAction("CatalogRequestSuccess", new { registeredAccount = registerAccount });
        }

        public ActionResult CatalogRequestSuccess(bool registeredAccount)
        {
            var model = new CatalogRequestSucessModel();

            if(registeredAccount)
            {
                model.NotificationText = _localizationService.GetResource("SevenSpikes.Plugins.FurnitureLeisure.CatalogRequest.RegisterCustomer.Success");
            }

            model.RegisteredAccount = registeredAccount;

            return View(model);
        }

        private IList<SelectListItem> GetStates()
        {
            var states = new List<SelectListItem>();

            states.Add(new SelectListItem
            {
                Text = "Select State", //TODO: Resource
                Value = "0"
            });

            var stateSelectListItems = _countryService
                .GetCountryById(UNITED_STATES_COUNTRY_ID)
                .StateProvinces
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
                .ToList();

            states.AddRange(stateSelectListItems);

            return states;
        }

        private ActionResult DisplayCustomProductCatalogsBanner()
        {
            var model = new CatalogRequestBannerModel()
            {
                LeftText = _furnitureLeisureSettings.LeftText,
                Title = _furnitureLeisureSettings.Title
            };

            return View("CustomProductCatalogBanner", model); 
        }
    }
}