(function ($, ss) {

    $(document).ready(function () {
        var cloudZoomAdjustPicture = $('.cloudZoomAdjustPictureOnProductAttributeValueChange');
        var thumbsInCarousel = $('.cloudZoomPictureThumbnailsInCarouselData');

        if (cloudZoomAdjustPicture.attr('data-isIntegratedByWidget') === 'true') {
            var cloudZoomPictureElementSelector = '#sevenspikes-cloud-zoom';

            var addCloudZoomWindowElement = function (cloudZoomWindowPictureElement) {
                var selectorOfTheParentElementOfTheCloudZoomWindow = cloudZoomWindowPictureElement.attr('data-selectorOfTheParentElementOfTheCloudZoomWindow');
                if (selectorOfTheParentElementOfTheCloudZoomWindow == undefined || selectorOfTheParentElementOfTheCloudZoomWindow === "") {
                    return;
                }

                var elementThatWillContainTheZoomWindow = $(selectorOfTheParentElementOfTheCloudZoomWindow);

                if (elementThatWillContainTheZoomWindow != undefined) {
                    var cloudZoomWindowElementId = cloudZoomWindowPictureElement.attr('data-zoomWindowElementId');
                    if (cloudZoomWindowElementId == undefined || cloudZoomWindowElementId === "") {
                        return;
                    }

                    var cloudZoomWindowElement = '<div id=' + cloudZoomWindowElementId + ' style="position: absolute;"></div>';
                    $(elementThatWillContainTheZoomWindow).prepend(cloudZoomWindowElement);
                }
            };

            var sevenSpikesCloudZoom = $(cloudZoomPictureElementSelector);

            var defaultImageContainerSelector = sevenSpikesCloudZoom.attr('data-defaultImageContainerSelector');
            if (defaultImageContainerSelector == undefined || defaultImageContainerSelector === '') {
                return;
            }

            if (!ss.isMobile() && ss.getViewPort().width > 980) {

                var nopPictureElement;
                var sevenSpikesPictureElement;

                $(defaultImageContainerSelector).each(function () {
                    if ($(this).find(cloudZoomPictureElementSelector).length <= 0) {
                        nopPictureElement = $(this);
                    } else {
                        sevenSpikesPictureElement = $(this);
                    }
                });

                if (nopPictureElement != null && sevenSpikesPictureElement != null) {
                    nopPictureElement.replaceWith(sevenSpikesPictureElement);
                    sevenSpikesPictureElement.show();
                }

                addCloudZoomWindowElement(sevenSpikesCloudZoom);

                $.event.trigger({ type: 'nopCloudZoomLoadCompletedEvent' });
            }
            else {
                if (sevenSpikesCloudZoom.length > 0 && $(defaultImageContainerSelector).length > 1) {
                    sevenSpikesCloudZoom.closest('.gallery').remove();
                }
            }
        }

        if (thumbsInCarousel.length > 0) {

            var numberOfVisibleItems = (parseInt(thumbsInCarousel.attr('data-numVisible')) || 4);
            var isVertical = thumbsInCarousel.attr('data-vertical') === 'true';

            $('#carousel').jcarousel({
                vertical: isVertical,
                numVisible: numberOfVisibleItems,
                scroll: 1,
                wrap: 'none',
                size: (parseInt(thumbsInCarousel.attr('data-size')) || 0),
                rtl: thumbsInCarousel.attr('data-rtl') === 'true'
            });

            if (thumbsInCarousel.attr('data-vertical') === 'true') {
                var carouselItemHeight = $('#carousel li').outerHeight(true);
                $('#carousel').parent().css({ 'height': carouselItemHeight * numberOfVisibleItems + 'px' });
            }
            else {
                $('#carousel').css({ 'width': '20000em', 'font-size': '13px' });
            }


            $('.jcarousel-prev, .jcarousel-next').disableTextSelect();
        }

        if ($('.cloudZoomEnableClickToZoom').length > 0) {
            $('.picture #wrap').on('click', function (e) {
                e.preventDefault();

                var imgSources = new Array();

                var imgItem = function (source, title) {
                    this.src = source;
                    this.title = title;
                };

                if ($('.picture-thumbs a.cloud-zoom-gallery').length < 1) {
                    imgSources.push(new imgItem($('.picture a.cloud-zoom').attr('href'), $('.picture a.cloud-zoom').attr('title')));
                }

                $('.picture-thumbs a.cloud-zoom-gallery').each(function () {
                    imgSources.push(new imgItem($(this).attr('href'), $(this).attr('title')));
                });

                var indexOfHighlightedImage = 0;

                for (var i = 0; i < imgSources.length; i++) {
                    if (imgSources[i].src === $('.picture a.cloud-zoom').attr('href')) {
                        indexOfHighlightedImage = i;
                        break;
                    }
                }

                $.magnificPopup.open({
                    items: imgSources,
                    type: 'image',
                    removalDelay: 300,
                    gallery: {
                        enabled: true
                    }
                }, indexOfHighlightedImage);
            });
        }

        if (cloudZoomAdjustPicture.length > 0) {
            var productId = cloudZoomAdjustPicture.attr('data-productId');
            var pictureFullSizePrefix = '_fullsize';
            var pictureAdjustmentTableName = 'productAttributeValueAdjustmentTable_' + productId;

            $('[id^="product_attribute_"]').each(function () {
                var ctrl = $(this);
                
                if (ctrl.closest('.product-variant-line').length > 0) {
                    return;
                }

                ctrl.on('change', function () {
                    var controlId = ctrl.attr('id');
                    var pictureFullSizeUrl = '';

                    if ((ctrl.is(':radio') && ctrl.is(':checked')) || (ctrl.is(':checkbox') && ctrl.is(':checked'))) {
                        pictureFullSizeUrl = window[pictureAdjustmentTableName][controlId + pictureFullSizePrefix];
                    } else if (ctrl.is('select')) {
                        var idx = ctrl.find('option:selected').index();

                        if (idx !== -1) {
                            pictureFullSizeUrl = window[pictureAdjustmentTableName][controlId + pictureFullSizePrefix][idx];
                        }
                    }

                    if (typeof pictureFullSizeUrl == 'string' && pictureFullSizeUrl !== '') {
                        $('.cloud-zoom-gallery[href="' + pictureFullSizeUrl + '"]').click();

                        $.event.trigger({
                            type: 'nopMainProductImageChanged',
                            target: ctrl,
                            pictureDefaultSizeUrl: pictureFullSizeUrl,
                            pictureFullSizeUrl: pictureFullSizeUrl
                        });
                    }
                });
            });
        }
    });

})(jQuery, sevenSpikes);