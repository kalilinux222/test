(function ($, ss) {

    var THEME_BREAKPOINT = 1200;

    function removeInlineStyle(element) {
        element.removeAttr("style");
    }

    function mobileNavigationClickCallback (e) {
        var nextDropdown = $(this).next();
        $('#header-links-opener .header-selectors-wrapper, #account-links .header-links-wrapper, .responsive-nav-wrapper .search-wrap .search-box, .flyout-cart').not(nextDropdown).slideUp();
        e.stopPropagation();
        nextDropdown.slideToggle('slow');
    }

    function handleSearchBox() {

        $(document).on('themeBreakpointPassed7Spikes', function (e) {

            if (e.isInitialLoad && !e.isMobileResolution) {
                return;
            }
            var searchBox = $('.store-search-box');
            removeInlineStyle(searchBox);

            if (e.isMobileResolution) {
                // mobile
                searchBox.detach().appendTo('.responsive-nav-wrapper .search-wrap');
            }
			else {
                // desktop
                searchBox.detach().insertAfter('.header-logo');
            }
        });

        $('.responsive-nav-wrapper .search-wrap span').on('click', mobileNavigationClickCallback);
    }

    function handleHeaderSelectors() {

        $(document).on('themeBreakpointPassed7Spikes', function (e) {

            var headerSelectorsWrapper = $('.header-selectors-wrapper');
            removeInlineStyle(headerSelectorsWrapper);

            if (e.isMobileResolution) {
                // mobile
                headerSelectorsWrapper.detach().insertAfter('.responsive-nav-wrapper .personal-button span');
            }
            else if (e.isInitialLoad == false) {
                // desktop
                headerSelectorsWrapper.detach().appendTo('.header-links-selectors-wrapper');
            }
        });

        $('#header-links-opener span').on('click', mobileNavigationClickCallback);
    }

    function handleHeaderAccountLinks() {

        $(document).on('themeBreakpointPassed7Spikes', function (e) {

            var headerLinksWrapper = $('.header-links-wrapper');
            removeInlineStyle(headerLinksWrapper);

            headerLinksWrapper.detach().insertAfter('.responsive-nav-wrapper .account-links span');
        });

        $('#account-links span').on('click', mobileNavigationClickCallback)
    }

    function handleShoppingCart() {

        $(document).on('themeBreakpointPassed7Spikes', function (e) {

            var cartWrapper = $('.cart-wrapper');
            removeInlineStyle($('.flyout-cart'));

            //if (e.isMobileResolution) {
                // mobile
                cartWrapper.detach().appendTo('.responsive-nav-wrapper');
            //}
        });
    }

    function handleNewsletterSubscribeDetach() {

        $(document).on('themeBreakpointPassed7Spikes', function (e) {
            if (e.isMobileResolution) {
                // mobile
                $('.newsletter-popup-overlay .newsletter').insertAfter('.newsletter-box-description');
            }
            else if (e.isInitialLoad == false) {
                // desktop
                $('.footer-1 .newsletter').insertAfter('.newsletter-popup-description');
            }
        });

    }

    function handleFooterChanges() {

        $(document).on('themeBreakpointPassed7Spikes', function (e) {
            if (e.isMobileResolution) {
                // mobile
                $('.footer .social-sharing').detach().insertAfter('.footer .footer-about-us');
            }
            else if (e.isInitialLoad == false) {
                // desktop
                $('.footer .social-sharing').detach().insertAfter('.footer .footer-block.first .footer-menu');
            }
        });

        $(document).on('themeBreakpointPassed7Spikes', function (e) {
            if (e.isMobileResolution) {
                // mobile
                $('.footer-2 .newsletter').detach().insertAfter('.footer-2 .footer-block.last .footer-menu');
            }
            else if (e.isInitialLoad == false) {
                // desktop
                $('.footer-2 .newsletter').detach().insertAfter('.footer-2 .footer-block.first .footer-about-us');
            }
        });
    }

    function handleFooterBlocksCollapse() {
        $(".footer-block .title").click(function (e) {
            if (ss.getViewPort().width <= THEME_BREAKPOINT) {
                $(this).siblings(".footer-collapse").slideToggle("slow");
            }
            else {
                e.preventdefault();
                $(this).siblings(".footer-collapse").show();
            }
        });
    }

    function handleOneColumnFiltersCollapse() {
        $(".uptown-one-column-ajax-filters-wrapper .filtersTitlePanel").click(function (e) {
            if (ss.getViewPort().width > THEME_BREAKPOINT) {
                $(this).siblings(".filtersPanel").slideToggle("slow");
            }
        });
    }

    function incrementQuantityValue(event) {
        event.preventDefault();
        event.stopPropagation();

        var input = $(this).siblings('.qty-input, .productQuantityTextBox').first();

        var value = parseInt(input.val());
        if (isNaN(value)) {
            input.val(1);
            return;
        }

        value++;
        input.val(value);
    }

    function decrementQuantityValue(event) {
        event.preventDefault();
        event.stopPropagation();

        var input = $(this).siblings('.qty-input, .productQuantityTextBox').first();

        var value = parseInt(input.val());

        if (isNaN(value)) {
            input.val(1);
            return;
        }

        if (value <= 1) {
            return;
        }

        value--;
        input.val(value);
    }

    function handleRemovePrevNextTitle() {
        $('.previous-product a, .next-product a').removeAttr("title");
    }

    function handleFlyoutCartScrolling(isInitialLoad) {
        if (isInitialLoad) {
            $('.cart-wrapper .flyout-cart').css({ 'opacity': '0', 'display': 'block' });
        }

        $('.mini-shopping-cart .items').perfectScrollbar({
            swipePropagation: false,
            wheelSpeed: 1,
            suppressScrollX: true
        });

        if (isInitialLoad) {
            $('.cart-wrapper .flyout-cart').css({ 'display': '', 'opacity': '' });
        }
    }

    function handleFlyoutCartScroll() {
        handleFlyoutCartScrolling(true);

        $(window).on('resize orientationchange', function () {
            setTimeout(function () {
                handleFlyoutCartScrolling(true);
            }, 200);
        });

        $('.header-cart-search-wrapper').on('mouseenter', '.cart-wrapper', function () {
            if (ss.getViewPort().width > THEME_BREAKPOINT) {
                setTimeout(handleFlyoutCartScrolling, 200);
            }
        });

        $('.responsive-nav-wrapper').on('click', '.ico-cart', function () {
            setTimeout(handleFlyoutCartScrolling, 800);
        });
    }

    function handleRemoveItemFromFlyoutCart() {
        $('body').on('click', '.mini-shopping-cart-item-close', function (e) {
            e.preventDefault();

            var flyoutShoppingCartPanelSelector = '#flyout-cart';
            var flyoutShoppingCart = $(flyoutShoppingCartPanelSelector);
            var productId = parseInt($(this).closest('.item').attr('data-productId'));

            if (isNaN(productId) || productId === 0) {
                return;
            }

            $.ajax({
                cache: false,
                type: 'POST',
                url: flyoutShoppingCart.attr('data-removeitemfromflyoutcarturl'),
                data: {
                    'id': productId
                }
            }).done(function (data) {
                if (data.success) {
                    $.ajax({
                        cache: false,
                        type: 'GET',
                        url: flyoutShoppingCart.attr('data-updateFlyoutCartUrl')
                    }).done(function (data) {
                        var newFlyoutShoppingCart = $(data).filter(flyoutShoppingCartPanelSelector);
                        flyoutShoppingCart.replaceWith(newFlyoutShoppingCart);

                        $('#flyout-cart').trigger('mouseenter');
                    });
                }
            });
        });
    }

    function handleClearCartButton() {
        $('.order-summary-content .clear-cart-button').on('click', function (e) {
            e.preventDefault();

            $('.cart [name="removefromcart"]').attr('checked', 'checked');

            $('.order-summary-content .update-cart-button').click();
        });
    }
    
    function initializePlusAndMinusQuantityButtonsClick() {
        $("body").on("click", ".add-to-cart-qty-wrapper .plus", incrementQuantityValue).on("click", ".add-to-cart-qty-wrapper .minus", decrementQuantityValue);
    }

    function handleCustomSelectors() {

        $('select').not('.filtersGroupPanel select').each(function () {

            var customSelect = $(this);
                if (ss.getViewPort().width > THEME_BREAKPOINT ) {
                
                    customSelect.wrap('<div class="custom-select" />');
                    $('<div class="custom-select-text" />').prependTo(customSelect.parent('.custom-select'));
                    customSelect.siblings('.custom-select-text').text(customSelect.children('option:selected').text());
                
                    customSelect.change(function () {
                        $(this).siblings('.custom-select-text').text($(this).children('option:selected').text());
                    });
                }

        });
    }
	
	function equalizeSubcategoryLists() {
		var maxHeight = 0;
		var setMaxHeight = function(){	
			$(".sub-category-sublist").each(function() {
				if ($(this).height() > maxHeight) {
					maxHeight = $(this).height();
				}
			});
			$(".sub-category-sublist").height(maxHeight);
		};
		setMaxHeight();
		$(window).on('resize', setMaxHeight);
	}

    $(document).ready(function () {

        var responsiveAppSettings = {
            themeBreakpoint: THEME_BREAKPOINT,
            isEnabled: true,
            // currently we do not use the built in functionality fot attaching and detaching, because we want to avoid the overlays canvas being displayed
            isSearchBoxDetachable: false,
            isHeaderLinksWrapperDetachable: false,
            doesDesktopHeaderMenuStick: true,
            doesScrollAfterFiltration: true,
            doesSublistHasIndent: true,
            displayGoToTop: true,
            hasStickyNav: true,
            lazyLoadImages: true,
            selectors: {
                menuTitle: ".menu-title",
                headerMenu: ".header-menu",
                closeMenu: ".close-menu",
                //movedElements: ".admin-header-links, .header-upper, .breadcrumb, .header-logo, .responsive-nav-wrapper, .slider-wrapper, .master-column-wrapper, .footer",
                sublist: ".header-menu .sublist",
                overlayOffCanvas: ".overlayOffCanvas",
                withSubcategories: ".with-subcategories",
                filtersContainer: ".nopAjaxFilters7Spikes",
                filtersOpener: ".filters-button span",
                searchBoxOpener: ".search-wrap > span",
                // currently we do not use the built in functionality fot attaching and detaching, because we want to avoid the overlays canvas being displayed
                searchBox: ".search-box",
                navWrapper: ".responsive-nav-wrapper",
                navWrapperParent: ".responsive-nav-wrapper-parent",
                headerMenuDesktopStickElement: ".header",
                headerMenuDesktopStickParentElement: ".master-wrapper-page",
                headerLinksOpener: "",
                headerLinksWrapper: ".header-links-selectors-wrapper",
                headerLinksWrapperMobileInsertAfter: ".header",
                headerLinksWrapperDesktopPrependTo: ".header-upper-centering",
                shoppingCartLink: "",
                overlayEffectDelay: 300
            }
        };

        handleSearchBox();
        handleHeaderSelectors();
        handleHeaderAccountLinks();
        handleShoppingCart();
        handleNewsletterSubscribeDetach();
        handleFooterChanges();

        ss.initResponsiveTheme(responsiveAppSettings);

        handleRemovePrevNextTitle();
        handleFooterBlocksCollapse();
        handleOneColumnFiltersCollapse();
        handleRemoveItemFromFlyoutCart();
        handleClearCartButton();
        initializePlusAndMinusQuantityButtonsClick();
        handleFlyoutCartScroll();
        handleCustomSelectors();

		equalizeSubcategoryLists();

        $('.newsletter-subscribe-block-opener').on('click', function(e) {
            e.preventDefault();

            var newsletterEmail = $('#newsletter-subscribe-block');

            if (newsletterEmail.is(':visible') && newsletterEmail.find('#newsletter-email').val() !== '') {
                $('#newsletter-subscribe-button').click();
                return;
            }

            newsletterEmail.slideToggle();
        });

        $('body').on('click', function (e) {
            if ($(e.target).parents(".responsive-nav-wrapper").length == 0) {
                $('#header-links-opener .header-selectors-wrapper, #account-links .header-links-wrapper, .responsive-nav-wrapper .search-wrap .search-box, .flyout-cart').slideUp();
            }
        });

        $(document).on("nopQuickViewDataShownEvent", function () {
            handleCustomSelectors();
        });

        
        $('.accordion-tab-title').on('click', function () {
            $(this).siblings('.accordion-tab-content').slideToggle().parent().toggleClass('active');
        });

        if ($('.shopping-cart-page .shipping-results').length > 0) {
            $('.shopping-cart-page .accordion-tab.estimate-shipping .accordion-tab-title').trigger('click');
        }
    });

    $(window).load(function () {
        $('.loader-overlay').hide();
    });

})(jQuery, sevenSpikes);