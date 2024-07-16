/*
* Copyright 2016 Seven Spikes Ltd. All rights reserved. (http://www.nop-templates.com)
* http://www.nop-templates.com/t/licensinginfo
*/

(function ($) {
    // Tab id attribute name
    var tabIdAttr = 'data-itemId';
    var tabStrip;

    var reorderMethodsQueue = function () {
        var queueInternal = [];
        var isSaveToPopInternal = true;

        return {
            isSaveToPop: function (isSave) {
                if (isSave === undefined) {
                    // Getter
                    return isSaveToPopInternal;
                }

                isSaveToPopInternal = isSave;

                this.next();
            },
            push: function (item) {
                queueInternal.push(item);

                this.next();
            },
            next: function () {
                if (isSaveToPopInternal) {
                    var nextCall = queueInternal.shift();

                    if (typeof nextCall === 'function') {
                        nextCall();
                    }
                }
            }
        };
    };

    var reorderQueue = new reorderMethodsQueue();

    var customListProductsReorderQueue = new reorderMethodsQueue();

    var initializeTabStrip = function () {
        tabStrip = $("#tabstrip").kendoTabStrip({
            animation: {
                open: {
                    effects: "fadeIn"
                }
            }
        }).data("kendoTabStrip");
        
        if (tabStrip.tabGroup.children().length > 0) {
            tabStrip.activateTab('li:first');
        }

        tabStrip.disable($('#add-new-item-from-tabstrip-button'));

        $("#tabstrip ul.k-tabstrip-items").kendoSortable({
            filter: "li.k-item:not(#add-new-item-from-tabstrip-button)",
            axis: "x",
            container: "ul.k-tabstrip-items",
            hint: function (element) {
                return $("<div id='sortable-tabs-hint' class='k-widget k-header k-tabstrip'><ul class='k-tabstrip-items k-reset'><li class='k-item k-state-active k-tab-on-top'>" + element.html() + "</li></ul></div>");
            },
            start: function (e) {
                tabStrip.activateTab(e.item);
            },
            change: function (e) {
                var reference = tabStrip.tabGroup.children().eq(e.newIndex);

                if (e.oldIndex < e.newIndex) {
                    tabStrip.insertAfter(e.item, reference);
                } else {
                    tabStrip.insertBefore(e.item, reference);
                }

                // Save the new order
                reorderQueue.push(function () {
                    var ids = [];

                    $('#tabstrip li.k-item[' + tabIdAttr + ']').each(function () {
                        ids.push($(this).attr(tabIdAttr));
                    });

                    var sendData = addAntiForgeryToken();
                    sendData.groupId = parseInt($('#Id').val());
                    sendData.ids = ids;

                    reorderQueue.isSaveToPop(false);

                    $.ajax({
                        url: $('.visualized-tabs-wrapper').attr('data-reorderGroupItemsUrl'),
                        type: 'POST',
                        data: sendData
                    }).fail(function (data) {
                        // TODO: We should handle the case when an error have occured.
                        // TODO: Think of way to revert the sorting...
                    }).always(function () {
                        reorderQueue.isSaveToPop(true);
                    });
                });
            }
        });
    };

    var findProductsForItem = function (itemId) {
        var contentTabId = $('#tabstrip li[' + tabIdAttr + '="' + itemId + '"]').attr('aria-controls');
        var tabContent = $('#' + contentTabId);

        tabContent.find('.found-products-grid .loader').show();

        var data = addAntiForgeryToken();

        data.id = itemId;

        $.ajax({
            url: $('.visualized-tabs-wrapper').attr('data-getProductsForSourceTypeUrl'),
            type: 'POST',
            data: data
        }).done(function (data) {
            tabContent.find('.found-products-grid .products-grid').html(data);

            if (tabContent.find('.itemSourceType').val() === '70') {
                $('.visualized-tabs-wrapper').trigger('customListProductsLoaded', { tabContent: tabContent });
            }
        }).always(function () {
            tabContent.find('.found-products-grid .loader').hide();

            if (tabContent.find('.itemSourceType').val() !== '70') {
                $('.products-vary-by-acl-message').removeClass('hidden');
            }
        });
    };

    var addProductsToCustomList = function (that) {
        var tabContent = that.closest('.tab-content');
        var form = that.closest('form');
        var submitUrl = form.attr('action');
        var submitType = form.attr('method');
        var itemId = parseInt(tabContent.find('.group-item-id').val()) || 0;

        if (isNaN(itemId) || itemId === 0) {
            return;
        }

        var sendData = form.serialize();
        sendData += '&save=save'; // we add this because the method has [FormValueRequired("save")]

        $.ajax({
            url: submitUrl,
            type: submitType,
            data: sendData
        }).done(function () {
            // products grid refresh - do we really need this?!
            var grid = form.find('.homepage-products-mappings-grid').data('kendoGrid');
            grid.dataSource.read();

            // Update the found products grid
            findProductsForItem(itemId);
        });
    };

    var removeProductFromCustomList = function (that) {
        var tabContent = that.closest('.tab-content');

        // simulate click on the product REMOVE button.
        tabContent.find('.found-products-grid .item-box[data-productId="' + that.val() + '"] .remove-button').click();
    };

    $(document).ready(function () {

        initializeTabStrip();

        // Add new item
        $('.add-new-group-item-button, #add-new-item-from-tabstrip-button').on('click', function (e) {
            e.preventDefault();
            
            var sendData = addAntiForgeryToken();
            sendData.groupId = parseInt($('#Id').val());

            $.ajax({
                url: $('.visualized-tabs-wrapper').attr('data-createGroupItemUrl'),
                type: 'POST',
                data: sendData
            }).done(function (data) {
                var dataObject = $(data);
                var itemIdInput = dataObject.find('.group-item-id');
                var newItemId = itemIdInput.val();

                if (newItemId == undefined) {
                    return;
                }

                $('.add-new-group-item-button').addClass('hidden');
                $('.no-product-items-available').addClass('hidden');

                var addNewTabAnchor = $('#add-new-item-from-tabstrip-button');
                var addNewTabContent = $('.add-new-item-tabstrip-content');

                // Add the new content element to the contents wrapper.
                $('#tabstrip').show().find(addNewTabContent).before(dataObject);

                tabStrip.insertBefore({
                    text: itemIdInput.attr('data-defaultTitle')
                }, addNewTabAnchor);

                var newTab = addNewTabAnchor.prev();
                newTab.attr(tabIdAttr, newItemId);
                tabStrip.activateTab(newTab);

                if (tabStrip.tabGroup.children().length > 2) {
                    $('.drag-and-drop-items-info').removeClass('hidden');
                }
            });
        });

        // Item source changed
        $('.visualized-tabs-wrapper').on('change', '.itemSourceType', function () {
            var that = $(this);
            var tabContent = that.closest('.tab-content');
            var selectedValue = parseInt(that.val());
            var data = addAntiForgeryToken();
            var itemId = parseInt(tabContent.find('.group-item-id').val()) || 0;
            var tabAnchor = $('#tabstrip .tabs-anchors li[aria-controls="' + tabContent.attr('id') + '"]');

            data.itemId = itemId;
            data.sourceType = selectedValue;

            tabContent.find('.custom-item-content, .found-products-grid .products-grid, .custom-list-add-products').empty();

            if (selectedValue === 0) {
                var defaultItemTitle = $('.visualized-tabs-wrapper').attr('data-defaultItemTitle') || 'New Item';
                tabAnchor.find('.k-link').text(defaultItemTitle);
                return;
            }

            $.ajax({
                url: $('.visualized-tabs-wrapper').attr('data-sourceTypeChangeUrl'),
                type: 'POST',
                data: data
            }).done(function (data) {
                var customContent = tabContent.find('.custom-item-content');
                customContent.html(data);

                tabContent.find('.custom-list-add-products').empty();

                tabAnchor.find('.k-link').text(customContent.find('.group-item-title[name="Title"]').val());

                if (selectedValue === 10 || selectedValue === 20 || selectedValue === 30 || selectedValue === 35 || selectedValue === 70) {
                    // Featured products, Bestsellers, New Products, Marked As New or Custom List
                    findProductsForItem(itemId);
                }
            });
        });

        // Item remove
        $('.visualized-tabs-wrapper').on('click', '.remove-item', function (e) {
            e.preventDefault();

            var that = $(this);
            var itemId = parseInt(that.attr(tabIdAttr));

            if (isNaN(itemId) || itemId === 0) {
                return;
            }

            var confirmation = confirm("Are you sure ?");

            if (confirmation === false) {
                return;
            }

            var sendData = addAntiForgeryToken();
            sendData.id = itemId;

            $.ajax({
                url: $('.visualized-tabs-wrapper').attr('data-deleteGroupItemUrl'),
                type: 'POST',
                data: sendData
            }).done(function () {
                var tabToRemove = $('#tabstrip li').filter('[' + tabIdAttr + '="' + itemId + '"]');

                tabStrip.remove(tabToRemove);

                if (tabStrip.tabGroup.children().length === 1) {
                    $('#tabstrip').hide();
                    $('.add-new-group-item-button').removeClass('hidden');
                    $('.no-product-items-available').removeClass('hidden');
                    $('.products-vary-by-acl-message').addClass('hidden');
                }
                else if (tabToRemove.is('.k-state-active')) {
                    tabStrip.activateTab('li:first');
                }

                if (tabStrip.tabGroup.children().length <= 2) {
                    $('.drag-and-drop-items-info').addClass('hidden');
                }
            });
        });

        // Item title change
        $('.visualized-tabs-wrapper').on('change', '.group-item-title', function () {
            var that = $(this);
            var thatText = that.val();
            var tabContent = that.closest('.tab-content');
            var tabAnchor = $('#tabstrip .tabs-anchors li[aria-controls="' + tabContent.attr('id') + '"]');
            var allTitleInputs = tabContent.find('.group-item-title');
            var allLanguageInputs = tabContent.find('input[name$="LanguageId"]');
            var data = addAntiForgeryToken();

            data.Id = parseInt(tabAnchor.attr(tabIdAttr)) || 0;
            data.Title = allTitleInputs.filter('[name="Title"]').val();

            allTitleInputs.each(function () {
                var currentTextInput = $(this);
                data[currentTextInput.attr('name')] = currentTextInput.val();
            });

            allLanguageInputs.each(function () {
                var currentTextInput = $(this);
                data[currentTextInput.attr('name')] = currentTextInput.val();
            });

            if (thatText !== '') {
                tabAnchor.find('.k-link').text(thatText);
            }

            $.ajax({
                url: $('.visualized-tabs-wrapper').attr('data-titleChangeUrl'),
                type: 'POST',
                data: data
            }).done(function (data) {
                allTitleInputs.filter('[name="Title"]').val(data.title);
                tabAnchor.find('.k-link').text(data.title);
            });
        });

        // Item active change
        $('.visualized-tabs-wrapper').on('change', '.group-item-active-input', function () {
            var that = $(this);
            var data = addAntiForgeryToken();

            data.Id = parseInt(that.attr(tabIdAttr)) || 0;
            data.Active = that.is(':checked');

            $.ajax({
                url: $('.visualized-tabs-wrapper').attr('data-activeChangeUrl'),
                type: 'POST',
                data: data
            });
        });

        // Item EntityId/SortMethod changed
        $('.visualized-tabs-wrapper').on('change', '.itemEntityId, .itemSortMethod', function () {
            var tabContent = $(this).closest('.tab-content');
            var tabAnchor = $('#tabstrip .tabs-anchors li[aria-controls="' + tabContent.attr('id') + '"]');
            var itemId = parseInt(tabContent.find('.group-item-id').val()) || 0;
            var entityId = parseInt(tabContent.find('.itemEntityId').val()) || 0;
            var sortMethod = parseInt(tabContent.find('.itemSortMethod').val()) || 0;

            if (entityId === 0) {
                tabContent.find('.found-products-grid .products-grid').empty();
                return;
            }

            var data = addAntiForgeryToken();
            data.Id = itemId;
            data.EntityId = entityId;
            data.SortMethod = sortMethod;

            $.ajax({
                url: $('.visualized-tabs-wrapper').attr('data-entityOrSortMethodChangeUrl'),
                type: 'POST',
                data: data
            }).done(function (data) {
                tabAnchor.find('.k-link').text(data.title);

                findProductsForItem(itemId);
            });
        });

        // Custom Item add/remove
        $('.visualized-tabs-wrapper').on('click', '.custom-list-add-products .homepage-products-mappings-grid input[type="checkbox"]', function () {
            var that = $(this);

            if (that.is(':checked')) {
                addProductsToCustomList(that);
            } else {
                removeProductFromCustomList(that);
            }
        });

        // Product remove button
        $('.visualized-tabs-wrapper').on('click', '.found-products-grid .item-box .remove-button', function () {
            var that = $(this);
            var tabContent = that.closest('.tab-content');
            var itemId = parseInt(tabContent.find('.group-item-id').val()) || 0;
            var itemBox = that.closest('.item-box');
            var productId = parseInt(itemBox.attr('data-productId')) || 0;

            if (isNaN(itemId) || itemId === 0 || isNaN(productId) || productId === 0) {
                return;
            }

            var confirmation = confirm('Are you sure ?');

            if (confirmation === false) {
                return;
            }

            var sendData = addAntiForgeryToken();
            sendData.id = itemId;
            sendData.productId = productId;

            $.ajax({
                url: $('.visualized-tabs-wrapper').attr('data-deleteProductFromCustomListUrl'),
                type: 'POST',
                data: sendData
            }).done(function () {
                itemBox.remove();

                // products grid refresh
                var grid = tabContent.find('.homepage-products-mappings-grid').data('kendoGrid');
                grid.dataSource.read();

                // Update the found products grid, because we can have more products than the setting
                findProductsForItem(itemId);
            });
        });

        $('.visualized-tabs-wrapper').on('customListProductsLoaded', function (e, data) {
            var tabContent = data.tabContent;

            if (tabContent == undefined || tabContent.length === 0) {
                return;
            }

            if (tabContent.find('.itemSourceType').val() === '70') {
                // Custom List

                var customProductsGrid = tabContent.find('.found-products-grid .products-grid');

                if (customProductsGrid.hasClass('sortable-grid')) {
                    // Ensure that there is only one initialization of sortable and removable
                    customProductsGrid.kendoSortable('destroy');
                }

                customProductsGrid.addClass('sortable-grid');

                var customProductsItemBoxes = customProductsGrid.children('.item-box');
                customProductsItemBoxes.addClass('sortable removable');

                // add remove buttons
                customProductsItemBoxes.prepend('<div class="remove-button">remove</div>');

                tabContent.find('.drag-and-drop-products-info').toggleClass('hidden', customProductsItemBoxes.length < 2);

                // initialize sortable
                customProductsGrid.kendoSortable({
                    filter: '.item-box.sortable',
                    container: customProductsGrid,
                    hint: function (element) {
                        return element.clone().addClass('hint');
                    },
                    placeholder: function () {
                        return '<div class="item-box sortable placeholder"><span>Drag and drop your product here</span></div>';
                    },
                    change: function () {
                        // Save the new order
                        customListProductsReorderQueue.push(function () {
                            var ids = [];

                            tabContent.find('.products-grid.sortable-grid .item-box.sortable').each(function () {
                                ids.push($(this).attr('data-productId'));
                            });

                            var sendData = addAntiForgeryToken();
                            sendData.itemId = parseInt(tabContent.find('.group-item-id').val()) || 0;
                            sendData.productIds = ids;

                            customListProductsReorderQueue.isSaveToPop(false);

                            $.ajax({
                                url: $('.visualized-tabs-wrapper').attr('data-reorderProductsInCustomListUrl'),
                                type: 'POST',
                                data: sendData
                            }).fail(function () {
                                // TODO: We should handle the case if an error occured.
                                // TODO: Think of way to revert the sorting...
                            }).always(function () {
                                customListProductsReorderQueue.isSaveToPop(true);
                            });
                        });
                    }
                });
            }
        });

        $('#Store').on('change', function () {
            var newValue = $(this).val();
            $('.searchStoreId').each(function () {
                var that = $(this);

                that.val(newValue);

                addProductsToCustomList(that);
            });
        });

        $('.products-vary-by-acl-message').each(function() {
            var that = $(this);

            if (that.closest('.tab-content').find('.products-grid .item-box').length > 0) {
                that.removeClass('hidden');
            }
        });
    });
})(jQuery);