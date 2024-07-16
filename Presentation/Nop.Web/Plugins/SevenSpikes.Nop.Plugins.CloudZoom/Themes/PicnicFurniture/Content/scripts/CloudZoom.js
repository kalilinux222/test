(function ($, ss) {
    $(document).ready(function () {
        CloudZoom.quickStart();

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

                    var cloudZoomWindowElement = '<div id=' + cloudZoomWindowElementId +
                        ' style="position: absolute;"></div>';
                    $(elementThatWillContainTheZoomWindow).prepend(cloudZoomWindowElement);
                }
            };

            var sevenSpikesCloudZoom = $(cloudZoomPictureElementSelector);

            var defaultImageContainerSelector = sevenSpikesCloudZoom.attr('data-defaultImageContainerSelector');
            if (defaultImageContainerSelector == undefined || defaultImageContainerSelector === '') {
                return;
            }

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

            //Cloudzoom window element (Zoom Position - element) must have size only when there is zoom applied.
            var cloudZoomWindowElementId = sevenSpikesCloudZoom.attr('data-zoomWindowElementId');

            $('#cloudZoomImage').bind('cloudzoom_start_zoom', function () {
                if (cloudZoomWindowElementId) {
                    var zoomWindowWidth = sevenSpikesCloudZoom.attr('data-zoom-window-width');
                    var zoomWindowHeight = sevenSpikesCloudZoom.attr('data-zoom-window-height');

                    $('#' + cloudZoomWindowElementId).width(zoomWindowWidth);
                    $('#' + cloudZoomWindowElementId).height(zoomWindowHeight);
                }

                $('.gallery .ribbon-wrapper .ribbon-position').hide();
            });

            $('#cloudZoomImage').bind('cloudzoom_end_zoom', function () {
                if (cloudZoomWindowElementId) {
                    $('#' + cloudZoomWindowElementId).css("width", "");
                    $('#' + cloudZoomWindowElementId).css("height", "");
                }

                $('.gallery .ribbon-wrapper .ribbon-position').show();
            });
        }

        //Changing the title position when the zoom become inside is necessary, because if the title stays on Top it takes some space
        //and moves the zoom image down. Now we check when the zoom become auto inside and set the position to bottom.
        function changeTitlePositionWhenZoomIsInside(cloudZoomInstance) {
            if (cloudZoomInstance && cloudZoomInstance.options.autoInside > 0) {

                if ($(window).width() < cloudZoomInstance.options.autoInside) {
                    cloudZoomInstance.options.captionPosition = 'bottom';
                }

                $(window).resize(function () {
                    if ($(window).width() <= cloudZoomInstance.options.autoInside) {
                        cloudZoomInstance.options.captionPosition = 'bottom';
                    }
                    else {
                        cloudZoomInstance.options.captionPosition = 'top';
                    }
                });
            }
        }

        $('#cloudZoomImage').bind('cloudzoom_ready', function () {

            $('.sevenspikes-cloudzoom-gallery').removeClass('not-loaded');

            var cloudZoomInstance = $('#cloudZoomImage').data('CloudZoom');

            changeTitlePositionWhenZoomIsInside(cloudZoomInstance);
        });

        if (thumbsInCarousel.length > 0) {

            var numberOfVisibleItems = (parseInt(thumbsInCarousel.attr('data-numVisible')) || 5);
            var numberOfScrollableItems = (parseInt(thumbsInCarousel.attr('data-numScrollable')) || numberOfVisibleItems || 5);
            var isVertical = thumbsInCarousel.attr('data-vertical') === 'true';
            var isRtl = thumbsInCarousel.attr('data-rtl') === 'true';
            var responsiveBreakpointsString = thumbsInCarousel.attr('data-responsive-breakpoints-for-thumbnails');

            // RTL should not be set on the slick carousel plugin when the caoursel is vertical,
            // because the carousel breaks due to a bug.

            var responsiveBreakpointsObj = {};

            var isCarouselRtl = isVertical ? false : isRtl;

            try {

                responsiveBreakpointsObj = JSON.parse(responsiveBreakpointsString);

                for (var i = 0; i < responsiveBreakpointsObj.length; i++) {

                    if (!responsiveBreakpointsObj[i].settings.vertical) {
                        responsiveBreakpointsObj[i].settings.rtl = isRtl;
                    }
                }
            }
            catch (e) {
                console.log('Invalid carousel breakpoints setting value!');
            }

            $('#picture-thumbs-carousel').slick({
                rtl: isCarouselRtl,
                infinite: true,
                vertical: isVertical,
                slidesToShow: numberOfVisibleItems,
                slidesToScroll: numberOfScrollableItems,
                easing: "swing",
                draggable: false,
                responsive: responsiveBreakpointsObj
            });

        }

        if ($('.cloudZoomEnableClickToZoom').length > 0) {
            $('#cloudZoomImage').bind('click',
                    function () {
                        var cloudZoom = $(this).data('CloudZoom');
                        cloudZoom.closeZoom();

                        var imgSources = new Array();

                        var imgItem = function (source, title) {
                            this.src = source;
                            this.title = title;
                        };

                        if ($('.picture-thumbs a.cloudzoom-gallery').length < 1) {
                            imgSources.push(new imgItem($('.picture a.picture-link').attr('data-full-image-url'),
                                $('.picture a.picture-link img').attr('title')));
                        }

                        $('.picture-thumbs a.cloudzoom-gallery').each(function () {

                            // The Slick Slider is cloning the elements in the carousel so we need to filter the cloned ones
                            // when adding the images to the default zoom image sources array. 
                            if(!$(this).parent().hasClass('slick-cloned')) {
                                imgSources.push(new imgItem($(this).attr('data-full-image-url'), $(this).attr('title')));
                            }
                        });

                        var indexOfHighlightedImage = 0;

                        for (var i = 0; i < imgSources.length; i++) {

                            // We need to remove the image extension before comparing to the current image src because the current image
                            // has size in its src (e.g. 0000183_dotted-light-bra_870.jpeg).
                            var imgSourceSliced = imgSources[i].src.slice(0, -5);
                            var currentImageSrc = $(".picture a.picture-link img").attr("src");

                            if (currentImageSrc.indexOf(imgSourceSliced) !== -1) {
                                indexOfHighlightedImage = i;
                                break;
                            }
                        }

                        $.magnificPopup.open({
                            items: imgSources,
                            type: 'image',
                            removalDelay: 300,
                            gallery: {
                                enabled: true,
                                tPrev: thumbsInCarousel.attr('data-magnificpopup-prev'),
                                tNext: thumbsInCarousel.attr('data-magnificpopup-next'),
                                tCounter: thumbsInCarousel.attr('data-magnificpopup-counter')
                            },
                            tClose: thumbsInCarousel.attr('data-magnificpopup-close'),
                            tLoading: thumbsInCarousel.attr('data-magnificpopup-loading')
                        },
                            indexOfHighlightedImage);
                        return false;
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
                        $('.cloudzoom-gallery[data-full-image-url="' + pictureFullSizeUrl + '"]').click();

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

        $(document).on("product_attributes_changed", function (e) {

            if (e.changedData && e.changedData.pictureFullSizeUrl) {
                var pictureFullSizeUrl = e.changedData.pictureFullSizeUrl;

                $('.cloudzoom-gallery[data-full-image-url="' + pictureFullSizeUrl + '"]').click();

                $.event.trigger({
                    type: 'nopMainProductImageChanged',
                    pictureDefaultSizeUrl: pictureFullSizeUrl,
                    pictureFullSizeUrl: pictureFullSizeUrl
                });
            }
        });
    });

})(jQuery, sevenSpikes);