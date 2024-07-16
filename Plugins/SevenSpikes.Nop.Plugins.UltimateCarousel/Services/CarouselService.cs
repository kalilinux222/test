using Nop.Core;
using Nop.Core.Data;
using Nop.Services.Events;
using SevenSpikes.Nop.Framework.Theme;
using SevenSpikes.Nop.Mappings.Domain;
using SevenSpikes.Nop.Plugins.UltimateCarousel.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace SevenSpikes.Nop.Plugins.UltimateCarousel.Services
{
    public class CarouselService : ICarouselService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<UCarousel> _carouselRepository;
        private readonly IRepository<CarouselItem> _carouselItemRepository;
        private readonly IRepository<EntityWidgetMapping> _entityWidgetMappingRepository;
        private readonly IWebHelper _webHelper;

        public CarouselService(IEventPublisher eventPublisher, IRepository<UCarousel> carouselRepository, IRepository<CarouselItem> carouselItemRepository, IRepository<EntityWidgetMapping> entityWidgetMappingRepository, IWebHelper webHelper)
        {
            _eventPublisher = eventPublisher;
            _carouselRepository = carouselRepository;
            _carouselItemRepository = carouselItemRepository;
            _entityWidgetMappingRepository = entityWidgetMappingRepository;
            _webHelper = webHelper;
        }

        /* ======= Carousel ======= */

        public IList<UCarousel> GetAllCarousels()
        {
            var query = from carousel in _carouselRepository.Table
                        orderby carousel.DisplayOrder
                        select carousel;

            return query.ToList();
        }

        public IList<UCarousel> GetVisibleCarouselsByWidgetZone(string widgetZone)
        {
            var query = from carousel in _carouselRepository.Table
                        where carousel.IsEnabled
                        join entityWidgetMapping in _entityWidgetMappingRepository.Table on carousel.Id equals entityWidgetMapping.EntityId
                        where entityWidgetMapping.EntityType == Constants.EntityType && entityWidgetMapping.WidgetZone == widgetZone
                        orderby entityWidgetMapping.DisplayOrder
                        select carousel;

            return query.ToList();
        }

        public UCarousel GetCarouselById(int carouselId)
        {
            var query = from carousel in _carouselRepository.Table
                        where carousel.Id == carouselId
                        orderby carousel.DisplayOrder
                        select carousel;

            return query.FirstOrDefault();
        }

        public void InsertCarousel(UCarousel carousel)
        {
            if (carousel == null)
                throw new ArgumentNullException("carousel");

            _carouselRepository.Insert(carousel);

            _eventPublisher.EntityInserted(carousel);
        }

        public void UpdateCarousel(UCarousel carousel)
        {
            if (carousel == null)
                throw new ArgumentNullException("carousel");

            _carouselRepository.Update(carousel);

            _eventPublisher.EntityUpdated(carousel);
        }

        public void DeleteCarousel(UCarousel carousel)
        {
            if (carousel == null)
                throw new ArgumentNullException("carousel");

            _carouselRepository.Delete(carousel);

            _eventPublisher.EntityDeleted(carousel);
        }

        public IList<SelectListItem> GetAvailableTemplates()
        {
            var themeTemplates = new List<string>();

            var theme = ThemeHelper.GetCurrentDesktopTheme();
            if(Directory.Exists(_webHelper.MapPath(string.Format(Constants.PathToThemeTemplates, theme))))
            {
                themeTemplates = Directory.GetFileSystemEntries(_webHelper.MapPath(string.Format(Constants.PathToThemeTemplates, theme)), "*.cshtml", SearchOption.TopDirectoryOnly).ToList();
            }

            var commonTemplates = Directory.GetFileSystemEntries(_webHelper.MapPath(Constants.PathToTemplates), "*.cshtml", SearchOption.TopDirectoryOnly).ToList();
            
            foreach (var commonTemplate in commonTemplates)
            {
                if (!themeTemplates.Any(x => Path.GetFileName(x) == Path.GetFileName(commonTemplate)))
                {
                    themeTemplates.Add(commonTemplate);
                }
            }

            return themeTemplates.Select(template => new SelectListItem
            {
                Text = Path.GetFileName(template),
                Value = Path.GetFileNameWithoutExtension(template)
            }).ToList();
        }

        /* ======= Carousel Items ======= */

        public IList<CarouselItem> GetAllCarouselItemsForCarousel(int carouselId)
        {
            var query = from i in _carouselItemRepository.Table
                        where i.CarouselId == carouselId
                        orderby i.DisplayOrder
                        select i;

            return query.ToList();
        }

        public IList<CarouselItem> GetAllVisibleCarouselItemsByCarouselId(int carouselId)
        {
            var query = from i in _carouselItemRepository.Table
                        where i.CarouselId == carouselId && i.Visible
                        orderby i.DisplayOrder
                        select i;

            return query.ToList();
        }

        public CarouselItem GetCarouselItemById(int carouselItemId)
        {
            var query = from carousel in _carouselItemRepository.Table
                        where carousel.Id == carouselItemId
                        orderby carousel.DisplayOrder
                        select carousel;

            return query.FirstOrDefault();
        }

        public void InsertCarouselItem(CarouselItem carouselItem)
        {
            if (carouselItem == null)
                throw new ArgumentNullException("CarouselItem");

            _carouselItemRepository.Insert(carouselItem);

            _eventPublisher.EntityInserted(carouselItem);
        }

        public void UpdateCarouselItem(CarouselItem carouselItem)
        {
            if (carouselItem == null)
                throw new ArgumentNullException("CarouselItem");

            _carouselItemRepository.Update(carouselItem);

            _eventPublisher.EntityUpdated(carouselItem);
        }

        public void DeleteCarouselItem(CarouselItem carouselItem)
        {
            if (carouselItem == null)
                throw new ArgumentNullException("CarouselItem");

            _carouselItemRepository.Delete(carouselItem);

            _eventPublisher.EntityDeleted(carouselItem);
        }
    }
}