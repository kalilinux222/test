(function ($, ss) {
    // "ss" will be alias for the "sevenSpikes" global object

    var megaMenuSkipEventBinding = false;
    var lastScrollTop = 0;
    var appSettings = {
        isEnabled: false
    };


    // Native functions
    (function (errorsCounter) {
        // Compatibility fallback for "teim"
        if (typeof String.prototype.trim !== 'function') {
            String.prototype.trim = function () {
                return this.replace(/^\s+|\s+$/g, '');
            };
        }

        // Compatibility fallback for "startsWith"
        if (typeof String.prototype.startsWith !== 'function') {
            String.prototype.startsWith = function (needle) {
                return this.indexOf(needle) === 0;
            };
        }

        // JS errors logging for test purposes
        window.onerror = function (msg, url, linenumber) {
            if (document.body == null || errorsCounter > 10) {
                return;
            }
            var currentJavascriptError = 'URL: ' + url + '��MESSAGE: ' + msg + '��ROW: ' + linenumber;
            var areThereAnyJsErrors = document.createElement('input');
            areThereAnyJsErrors.setAttribute('class', 'javascriptErrorsElement');
            areThereAnyJsErrors.setAttribute('type', 'hidden');
            areThereAnyJsErrors.setAttribute('value', currentJavascriptError);
            document.body.appendChild(areThereAnyJsErrors);
            errorsCounter++;
            //document.getElementsByClassName('javascriptErrorsElement')[0].getAttribute('value').split('��');
        };

        // Make it on window.load as the jCarousel should be initialized, which is done on document.ready
        window.onload = function () {
            if (sevenSpikes.getViewPort().width <= ss.breakPointWidth) {
                $('.block .jcarousel-container-vertical').hide();
            }

            if (sevenSpikes.isMobile()) {
                $('.mega-menu-responsive .labelForNextPlusButton').on('click', function () {
                    $(this).next('.plus-button').click();
                });
            }
        };
    })(0);

    $.fn.extend({
        // custom select
        simpleSelect: function () {
            return this.each(function () {
                var that = $(this);

                that.wrap('<div class="select-wrap"></div>')
                        .after('<span class="select-box">' + '<span class="select-inner">' + that.find(':selected').text() + '</span>' + '</span>');

                that.change(function () {
                    that.next().children('.select-inner').text(that.find(':selected').text());
                });
            });
        }
    });

    // Functions
    function addMobileClassIfEnabled(shouldAddClassForMobile) {
        var addClassOnMobile = function () {
            if (shouldAddClassForMobile && sevenSpikes.isMobile()) {
                $('.product-grid .item-box').addClass('mobile-box');
            }
        };

        addClassOnMobile();

        $(document).on('nopAjaxFiltersFiltrationCompleteEvent', addClassOnMobile);
    }

    function attachDetachSideBanners(viewport) {
        if (viewport < ss.breakPointWidth) {
            // detach anywhere slider banners from the left or right side and attach them at the end of the master-wrapper-page
            var banners = $('.leftside-3 .slider-wrapper, .rightside-3 .slider-wrapper, .side-2 .slider-wrapper').detach();
            $('.master-wrapper-main').append('<div class="mobile-banners"></div>');
            $('.mobile-banners').append(banners);
        }
        else {
            $('.mobile-banners').detach();
        }
    }

    function toggleSideBlocks(isInitialLoad) {
        $('.block').each(function () {
            if (sevenSpikes.getViewPort().width > ss.breakPointWidth) {
                if (!isInitialLoad) {

                    if (appSettings.isEnabled === false) {
                        $(this).children().eq(1).show();
                        $(this).children().eq(0).children('a.toggleControl').removeClass('closed');
                    }
                    else {
                        $(this).children('.listbox').eq(0).show();
                    }

                    $(this).find('.jcarousel-container-vertical').show();
                }
            }
            else {
                if (appSettings.isEnabled === false) {
                    $(this).children().eq(1).hide();
                    $(this).children().eq(0).children('a.toggleControl').addClass('closed');
                }
                else {
                    $(this).children('.listbox').eq(0).hide();
                }

                $(this).find('.jcarousel-container-vertical').hide();
            }
        });
    }

    function addSideBlocksClickEvents() {
        $('.block .title').not($('.nopAjaxFilters7Spikes .block .title')).off('click').on('click', function () {
            if (sevenSpikes.getViewPort().width <= ss.breakPointWidth) {
                var thisSibling = $(this).siblings();
                thisSibling.slideToggle('slow', function () {
                    thisSibling.css('overflow', '');
                });
            }
        });

        $('.nop-jcarousel.vertical-holder .carousel-title').on('click', function () {
            if (sevenSpikes.getViewPort().width <= ss.breakPointWidth) {
                var thisSibling = $(this).siblings();
                thisSibling.slideToggle('slow', function () {
                    thisSibling.css('overflow', '');
                });
            }
        });
    }

    function sublistIndent(sublistSelector, withCategories, sublistWidth, levelZIndex) {
        var sublistElement = $(sublistSelector);

        if (sevenSpikes.getViewPort().width <= ss.breakPointWidth) {
            sublistElement.css({ 'width': sublistWidth - 7, 'z-index': sublistElement.css('z-index') + levelZIndex });//7px sublist indent
            $('> .sublist > li > ' + withCategories, sublistSelector).css('width', sublistWidth - 57);// 50px - width of .plus-button + 7px indent
        }
        else {
            sublistElement.css({ 'width': '', 'z-index': '' });
            $('> .sublist > li > ' + withCategories, sublistSelector).css('width', '');
        }

        var nextSublist = sublistSelector + '> .sublist > li > .sublist-wrap';

        if ($(nextSublist).length > 0) {
            sublistIndent(nextSublist, withCategories, sublistWidth - 7, levelZIndex + 1);
        }
    }

    function initSublistIndent() {
        if ($('.mega-menu-responsive').length > 0) {
            sublistIndent('.mega-menu-responsive > li > .sublist-wrap', appSettings.selectors.withSubcategories, 320, 1);// 320px - width of responsive top level menu; 1 - initial z-index
        }
        else {
            sublistIndent(appSettings.selectors.headerMenu + ' > ul > li > .sublist-wrap', appSettings.selectors.withSubcategories, 320, 1);
        }
    }

    function hasScrolled(navWrapperReal, navWrapperParent) {
        var realNavWrapper = $(navWrapperReal);
        var navWrapper = realNavWrapper.filter('.stick');

        if (navWrapper.length === 0 || navWrapperParent.length === 0) {
            realNavWrapper.removeClass('nav-up');
            return;
        }

        var st = $(window).scrollTop();

        if (st <= $(navWrapperParent).offset().top) {
            realNavWrapper.removeClass('nav-up');
            return;
        }

        if (st > lastScrollTop) {
            navWrapper.addClass('nav-up');
        }
        else {
            navWrapper.removeClass('nav-up');
        }

        lastScrollTop = st;
    }

    function stickyNav(navWrapper, navWrapperParent) {
        var parent = $(navWrapperParent);
        var windowScrollTop = $(window).scrollTop();

        if (windowScrollTop > 0 && windowScrollTop >= parent.offset().top) {
            parent.css('height', parent.height() + 'px');
            $(navWrapper).addClass('stick');
            $(document).trigger({ type: 'navigationHasSticked' });
        }
        else {
            parent.css('height', '');
            $(navWrapper).removeClass('stick');
            $(document).trigger({ type: 'navigationHasSticked' });
        }
    }

    function windowScrollEvents() {
        if (appSettings.hasStickyNav) {
            stickyNav(appSettings.selectors.navWrapper, appSettings.selectors.navWrapperParent);
        }

        if (appSettings.doesDesktopHeaderMenuStick) {
            stickyNav(appSettings.selectors.headerMenuDesktopStickElement, appSettings.selectors.headerMenuDesktopStickParentElement);
        }

        $(window).on('scroll', function () {
            if (appSettings.hasStickyNav) {
                stickyNav(appSettings.selectors.navWrapper, appSettings.selectors.navWrapperParent);
            }

            if (appSettings.doesDesktopHeaderMenuStick) {
                stickyNav(appSettings.selectors.headerMenuDesktopStickElement, appSettings.selectors.headerMenuDesktopStickParentElement);
            }

            if (appSettings.displayGoToTop) {
                if ($(window).scrollTop() > 100) {// 100px - go to top scroll barier
                    $('#goToTop').show();
                }
                else {
                    $('#goToTop').hide();
                }
            }

            hasScrolled(appSettings.selectors.navWrapper, appSettings.selectors.navWrapperParent);
        });

        if (appSettings.displayGoToTop) {
            $('#goToTop').on('click', function () {
                $('html,body').animate({ scrollTop: 0 }, 400);
            });
        }
    }

    function onWidthBreak(viewport, isInitialLoad) {
        var selectors = appSettings.selectors;
        var isSearchBoxDetachable = appSettings.isSearchBoxDetachable;
        var isHeaderLinksWrapperDetachable = appSettings.isHeaderLinksWrapperDetachable;

        if (viewport <= ss.breakPointWidth) {

            $(selectors.headerMenu).add($(selectors.sublist)).add($(selectors.overlayOffCanvas));
            $(selectors.filtersContainer).detach().insertAfter(selectors.headerMenu);
            if (isSearchBoxDetachable) {
                $(selectors.searchBox).detach().insertAfter(selectors.navWrapperParent);
            }
            if (isHeaderLinksWrapperDetachable) {
                $(selectors.headerLinksWrapper).detach().insertAfter(selectors.headerLinksWrapperMobileInsertAfter);
            }

            $(selectors.shoppingCartLink).off('mouseenter.flyout-cart').off('mouseleave.flyout-cart');
            $('.top-menu > li > .sublist-wrap').css('display', '');

        }
        else {
            $(selectors.headerMenu).css({ 'height': '', 'top': '' });
            $(selectors.sublist).css({ 'height': '', 'top': '' });
            $(selectors.filtersContainer).css('height', '');
            if (!isInitialLoad) {
                $(selectors.filtersContainer).detach().insertBefore("#availableSortOptionsJson");
            }
            if (isHeaderLinksWrapperDetachable) {
                $(selectors.headerLinksWrapper).detach().prependTo(selectors.headerLinksWrapperDesktopPrependTo);
            }
            if (isSearchBoxDetachable) {
                $(selectors.searchBox).detach().insertAfter(selectors.searchBoxBefore);
            }

            $(selectors.shoppingCartLink).on('mouseenter.flyout-cart', function (e) {
                var cart = $(this).children('.flyout-cart');
                if (cart.attr('isSliding') == undefined || cart.attr('isSliding') === 'false') {
                    cart.attr('isSliding', 'true').slideDown(100, function () {
                        $(this).css('overflow', '');
                        $(this).attr('isSliding', 'false');
                    });
                }
                e.preventDefault();
            }).on('mouseleave.flyout-cart', function (e) {
                $(this).children('.flyout-cart').attr('isSliding', 'true').slideUp(100, function () {
                    $(this).css('overflow', '');
                    $(this).attr('isSliding', 'false');
                });
                e.preventDefault();
            });
        }
    }

    function onOverlayClick() {
        var selectors = appSettings.selectors;

        $(selectors.movedElements).removeClass('move-right');
        $(selectors.overlayOffCanvas).fadeOut(function () {
            $(this).removeClass('show');
        });
        $('html, body').removeClass('scrollYRemove');
    }

    function overlayOffCanvasShow(selectors) {
        $(selectors.overlayOffCanvas).addClass('show').fadeIn();
        $('html, body').addClass('scrollYRemove');
        $(document).trigger({ type: "onOverlayOffCanvasShow" });
    }

    function onMenuTitleClick() {
        var selectors = appSettings.selectors;

        $(selectors.headerMenu).addClass('open');
        $(selectors.movedElements).addClass('move-right');
        overlayOffCanvasShow(selectors);
    }

    function addDetachableClickEvents() {
        var selectors = appSettings.selectors;

        if (appSettings.isSearchBoxDetachable) {
            $(selectors.searchBoxOpener).click(function () {
                $(selectors.searchBox).addClass('open');
                overlayOffCanvasShow(selectors);
            });
        }

        $(selectors.headerLinksOpener).on('click', function () {
            $(selectors.headerLinksWrapper).addClass('open');
            overlayOffCanvasShow(selectors);
        });

        // open-close menu
        $(selectors.menuTitle).click(function () {
            onMenuTitleClick();
        });

        $(selectors.closeMenu).click(function () {
            $(selectors.headerMenu).removeClass('open');
            onOverlayClick();

            if (appSettings.isEnabled) {
                // removes the PERFECT SCROLL from the mobile menu while not open
                $('.header-menu, .sublist-wrap').perfectScrollbar('destroy');
            }
        });

        // canvas overlay click
        $(selectors.overlayOffCanvas).click(function () {
            $(selectors.sublist).parent().removeClass('active').animate({ scrollTop: 0 });
            $(selectors.headerMenu).add($(selectors.filtersContainer)).removeClass('open');

            if (appSettings.isSearchBoxDetachable) {
                $(selectors.searchBox).removeClass('open');
            }

            $(selectors.headerLinksWrapper).removeClass('open');

            onOverlayClick();

            if (appSettings.isEnabled) {
                //removes the PERFECT SCROLL from the mobile menu while not open
                $('.header-menu, .sublist-wrap, .nopAjaxFilters7Spikes.open').perfectScrollbar('destroy');
            }
        });

        // close sublist
        $('.sublist').on('click', '.back-button', function () {
            $(this).parent('.sublist').parent('.sublist-wrap').removeClass('active');

            if (appSettings.isEnabled) {
                //add the PERFECT SCROLL to the upper level of the menu
                $(this).parents().eq(4).perfectScrollbar({
                    swipePropagation: false,
                    wheelSpeed: 2,
                    suppressScrollX: true
                });
            }
        });

        // filters open-close
        $(selectors.filtersOpener).click(function () {
            $(selectors.filtersContainer).toggleClass('open');
            $(selectors.movedElements).toggleClass('move-right');
            overlayOffCanvasShow(selectors);

            if (appSettings.isEnabled) {
                $('.nopAjaxFilters7Spikes.open').perfectScrollbar({
                    swipePropagation: false,
                    wheelSpeed: 1,
                    suppressScrollX: true
                });
            }
        });

        // add close button
        $('<div class="close-filters"><span>close</span></div>').insertBefore('.filtersPanel');
        $('.close-filters').click(function () {
            $(selectors.filtersContainer).toggleClass('open');
            $(selectors.movedElements).toggleClass('move-right');
            $(selectors.overlayOffCanvas).fadeOut(function () {
                $(this).removeClass('show');
            });
            $('html, body').removeClass('scrollYRemove');

            if (appSettings.isEnabled) {
                $('.nopAjaxFilters7Spikes.open').perfectScrollbar('destroy');
            }
        });
    }
    
    function isMobileDeviceInternal() {
        var isMobile = {
            Android: function () {
                return navigator.userAgent.match(/Android/i);
            },
            BlackBerry: function () {
                return navigator.userAgent.match(/BlackBerry/i);
            },
            iOS: function () {
                return navigator.userAgent.match(/iPhone|iPad|iPod/i);
            },
            Opera: function () {
                return navigator.userAgent.match(/Opera Mini/i);
            },
            Windows: function () {
                return navigator.userAgent.match(/IEMobile/i);
            },
            any: function () {
                return (this.Android() || this.BlackBerry() || this.iOS() || this.Opera() || this.Windows()) ? true : false;
            }
        };

        return isMobile.any();
    }

    function loadProductBoxImagesOnScroll() {

        var windowObj = $(window);

        var lazyLoadProductBoxImages = function () {
            var currentScrollTop = windowObj.scrollTop() + windowObj.outerHeight() + 100;

            $('img[data-lazyloadsrc]').not('[loadedimage]').each(function () {
                var that = $(this);

                if (that.offset().top < currentScrollTop) {
                    that.attr('src', that.attr('data-lazyloadsrc'));

                    that.attr('loadedimage', 'true');
                }
            });
        };

        $(document).ready(function () {
            lazyLoadProductBoxImages();

            windowObj.on('scroll', lazyLoadProductBoxImages);
        });

        $(document).on('nopAjaxFiltersFiltrationCompleteEvent nopAjaxCartProductAddedToCartEvent nopQuickViewDataShownEvent newProductsAddedToPageEvent', lazyLoadProductBoxImages);
    }

    // Properties
    ss.breakPointWidth = 980; // The last pixel width where the theme is still responsive. It takes the setting from the themes.
    ss.isRtl = $('#isRtlEnabled').val() === 'true';
    ss.isMobileDevice = isMobileDeviceInternal();

    // Methods
    ss.AntiSpam = function (emailName, emailDomain) {
        window.location.href = 'mailto:' + emailName + '@' + emailDomain;
    };

    ss.getAttrValFromDom = function (elementSelector, elementAttribute, defaultValue) {
        var value = $(elementSelector).attr(elementAttribute);

        if (value == undefined || value === '') {
            //if (window.console) {
            //    console.log('"' + elementAttribute + '" was not found.');
            //}

            if (defaultValue != undefined) {
                value = defaultValue;
            } else {
                value = '';
            }
        }

        return value;
    };

    ss.getHiddenValFromDom = function (elementSelector, defaultValue) {
        var value = $(elementSelector).val();

        if (value == undefined || value === '') {
            if (defaultValue != undefined) {
                value = defaultValue;
            } else {
                value = '';
            }
        }

        return value;
    };

    ss.getUrlVar = function (name) {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');

        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];

            if (name != null && hash[0] === name) {
                return hash[1];
            }
        }

        if (name != null) {
            return null;
        }

        return vars;
    };

    ss.addWindowEvent = function (eventName, functionToCall) {
        if (window.addEventListener) window.addEventListener(eventName, functionToCall, false);
        else if (window.attachEvent) window.attachEvent("on" + eventName, functionToCall);
    };

    ss.getViewPort = function () {
        var e = window, a = 'inner';
        if (!('innerWidth' in window)) {
            a = 'client';
            e = document.documentElement || document.body;
        }

        var result = { width: e[a + 'Width'], height: e[a + 'Height'] };

        return result;
    };

    ss.isMobile = isMobileDeviceInternal;

    ss.prepareTopMenu = function (viewport) {
        viewport = viewport || ss.getViewPort().width;

        if (megaMenuSkipEventBinding === false) {
            $('.menu-title').click(function () {
                if ($(this).hasClass('close')) $(this).removeClass('close');
                else $(this).addClass('close');

                $(this).siblings('.top-menu, .mega-menu-responsive').slideToggle('fast', function () {
                    $(this).css('overflow', '');
                });

                if (appSettings.isEnabled) {
                    //adds the PERFECT SCROLL to the whole mobile menu
                    $('.header-menu, .sublist-wrap').perfectScrollbar({
                        swipePropagation: false,
                        wheelSpeed: 1,
                        suppressScrollX: true
                    });
                }
            });

            $('.plus-button').on('click', function (e) {
                var thisPlusButton = $(this);
                e.stopPropagation();

                if (thisPlusButton.hasClass('close')) {
                    thisPlusButton.removeClass('close');
                } else {
                    thisPlusButton.addClass('close');
                }
                var sublist = thisPlusButton.siblings('.sublist-wrap');
                if (sublist.hasClass('active')) {
                    sublist.removeClass('active');
                } else {
                    if (appSettings.isEnabled) {
                        thisPlusButton.parents().eq(2).animate({ scrollTop: 0 }, 180, function () {
                            sublist.addClass('active');
                        });
                    } else {
                        sublist.addClass('active');
                    }
                }

                if (appSettings.isEnabled) {
                    //removes the PERFECT SCROLL when opening inner level and add again when closing it
                    thisPlusButton.parents().eq(2).perfectScrollbar('destroy');
                }
            });

            megaMenuSkipEventBinding = true;
        }

        var menus = '.top-menu li';

        if (viewport > ss.breakPointWidth) {
            $('.sublist-wrap.active').removeClass('active');
            $('.plus-button.close').removeClass('close');

            $(menus).each(function () {
                var mouseEnterTimeout, mouseLeaveTimeout;

                $(this).on('mouseenter', function () {
                    var that = $(this);
                    clearTimeout(mouseLeaveTimeout);
                    mouseEnterTimeout = setTimeout(function () {
                        $('a', that).first().addClass('hover');
                        $('.sublist-wrap', that).first().addClass('active');
                    }, 180);
                }).on('mouseleave', function () {
                    var that = $(this);
                    clearTimeout(mouseEnterTimeout);
                    mouseLeaveTimeout = setTimeout(function () {
                        $('a', that).first().removeClass('hover');
                        $('.sublist-wrap', that).first().removeClass('active');
                    }, 180);
                });
            });
        } else {
            $(menus).off('mouseenter mouseleave');
            $('.sublist-wrap.active').removeClass('active');
            $('.plus-button.close').removeClass('close');
        }
    };

    ss.initResponsiveTheme = function (appSettingsObj) {
        if (appSettingsObj == null) {
            return;
        }

        appSettings = appSettingsObj;

        if (appSettings.selectors != null) {
            if (appSettings.selectors.headerLinksWrapperMobileInsertAfter == null) {
                appSettings.selectors.headerLinksWrapperMobileInsertAfter = '.header';
            }
            if (appSettings.selectors.headerLinksWrapperDesktopPrependTo == null) {
                appSettings.selectors.headerLinksWrapperDesktopPrependTo = '.header';
            }
        }

        // themeBreakpoint, hasSideBanners, shouldAddClassForMobile

        ss.breakPointWidth = appSettings.themeBreakpoint || 980;

        var previousWidth = sevenSpikes.getViewPort().width;

        var checkWidth = function (isInitialLoad) {
            var viewport = sevenSpikes.getViewPort().width;

            if (appSettings.isEnabled) {
                if (isInitialLoad && appSettings.doesDesktopHeaderMenuStick) {
                    $(appSettings.selectors.headerMenu).wrap('<div id="headerMenuParent" class="header-menu-parent"></div>');
                }
            }

            if (isInitialLoad || (viewport > ss.breakPointWidth && previousWidth <= ss.breakPointWidth) || (viewport <= ss.breakPointWidth && previousWidth > ss.breakPointWidth)) {
                ss.prepareTopMenu(viewport);
                addMobileClassIfEnabled(appSettings.shouldAddClassForMobile);
                toggleSideBlocks(isInitialLoad);

                if (appSettings.hasSideBanners) {
                    attachDetachSideBanners(viewport);
                }

                if (appSettings.isEnabled) {
                    onWidthBreak(viewport, isInitialLoad);

                    if (appSettings.doesSublistHasIndent === true) {
                        initSublistIndent();
                    }
                }

                $(document).trigger({ type: 'themeBreakpointPassed7Spikes', isInitialLoad: isInitialLoad, isMobileResolution: viewport <= ss.breakPointWidth });
            }

            previousWidth = viewport;
        };

        checkWidth(true);

        sevenSpikes.addWindowEvent('resize', function () { checkWidth(false); });
        sevenSpikes.addWindowEvent('orientationchange', function () { checkWidth(false); });

        if (appSettings.isEnabled) {
            if (appSettings.hasStickyNav || appSettings.displayGoToTop || appSettings.doesDesktopHeaderMenuStick) {

                if (appSettings.selectors.headerMenuDesktopStickElement == null) {
                    appSettings.selectors.headerMenuDesktopStickElement = appSettings.selectors.headerMenu;
                }
                if (appSettings.selectors.headerMenuDesktopStickParentElement == null) {
                    appSettings.selectors.headerMenuDesktopStickParentElement = '#headerMenuParent';
                }
                windowScrollEvents();
            }

            addDetachableClickEvents();

            if (appSettings.doesScrollAfterFiltration === true) {
                $(document).on('nopAjaxFiltersFiltrationCompleteEvent', function () {
                    if (sevenSpikes.getViewPort().width <= ss.breakPointWidth) {
                        $(appSettings.selectors.overlayOffCanvas).triggerHandler('click');
                        setTimeout(function () { $(appSettings.selectors.overlayOffCanvas).hide(); }, 450);
                    }
                });
            }
        }

        addSideBlocksClickEvents();

        // Remove the filters icon, if no filters available
        if ($('.nopAjaxFilters7Spikes .block').length === 0) {
            $('.responsive-nav-wrapper .filters-button').remove();
        }

        if (appSettings.DisableFootable == null || appSettings.DisableFootable === false) {
            // FOOTABLE.JS
            if (typeof $('body').footable == 'function') {
                $('.wishlist-page .cart, .compare-products-table-mobile, .recurring-payments .data-table, .downloadable-products-page .data-table, .reward-points-history .data-table, .order-summary-content .cart, .order-details-page .data-table, .return-request-page .data-table, .forum-table, .private-messages-page .data-table').footable();
            }

            if ($('.checkout-page').length > 0) {
                $(document).ajaxSuccess(function () {
                    if ($('.order-summary-content .cart').length > 0) {
                        $('.order-summary-content .cart').footable();
                    }
                });
            }
        }
        if (appSettings.lazyLoadImages) {
            loadProductBoxImagesOnScroll();
        }
    };



})(jQuery, window.sevenSpikes = window.sevenSpikes || {});