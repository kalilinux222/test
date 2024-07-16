/*
** nopCommerce one page checkout
*/

$(document).ready(function () {

    var pageElement = $('body');

    if (pageElement.length > 0) {
        pageElement.prepend('<div class="avalaraAddressValidationWindow"></div>');

        var avalaraAddressValidationPopup = $(".avalaraAddressValidationWindow").kendoWindow({
            draggable: false,
            resizable: false,
            modal: true,
            actions: ["Close"],
            animation: false,
            visible: false
        }).data("kendoWindow");

        //TODO: Change the class!
        avalaraAddressValidationPopup.wrapper.addClass("ajaxCart").addClass("confirm-address-popup");

        $(document).on("click", ".k-overlay", function () {
            avalaraAddressValidationPopup.close();
        });
    }
});

var Checkout = {
    loadWaiting: false,
    failureUrl: false,

    init: function (failureUrl) {
        this.loadWaiting = false;
        this.failureUrl = failureUrl;

        Accordion.disallowAccessToNextSections = true;
    },

    ajaxFailure: function () {
        location.href = Checkout.failureUrl;
    },
    
    _disableEnableAll: function (element, isDisabled) {
        var descendants = element.find('*');
        $(descendants).each(function() {
            if (isDisabled) {
                $(this).attr('disabled', 'disabled');
            } else {
                $(this).removeAttr('disabled');
            }
        });

        if (isDisabled) {
                element.attr('disabled', 'disabled');
            } else {
                element.removeAttr('disabled');
            }
    },

    setLoadWaiting: function (step, keepDisabled) {
        if (step) {
            if (this.loadWaiting) {
                this.setLoadWaiting(false);
            }
            var container = $('#' + step + '-buttons-container');
            container.addClass('disabled');
            container.css('opacity', '.5');
            this._disableEnableAll(container, true);
            $('#' + step + '-please-wait').show();
        } else {
            if (this.loadWaiting) {
                var container = $('#' + this.loadWaiting + '-buttons-container');
                var isDisabled = (keepDisabled ? true : false);
                if (!isDisabled) {
                    container.removeClass('disabled');
                    container.css('opacity', '1');
                }
                this._disableEnableAll(container, isDisabled);
                $('#' + this.loadWaiting + '-please-wait').hide();
            }
        }
        this.loadWaiting = step;
    },

    gotoSection: function (section) {
        debugger
        section = $('#opc-' + section);
        section.addClass('allow');
        Accordion.openSection(section);
    },

    back: function () {
        if (this.loadWaiting) return;
        Accordion.openPrevSection(true, true);

        //scroll to the top of the page when going
        $("html, body").animate({ scrollTop: 0 }, "slow");
    },

    setStepResponse: function (response) {

        if (response.update_element_in_section) {

            var billingAddress = '';
            var selectedPaymentMethodInfo = '';

            var hasBillingAddressField = response.update_element_in_section.html.indexOf('has-billing-address');

            if (hasBillingAddressField !== -1) {

                billingAddress = $('#checkout-billing-load').detach();
                selectedPaymentMethodInfo = $('input[name=paymentmethodcheck]:checked').attr('id');
            }

            $(response.update_element_in_section.name).html(response.update_element_in_section.html);

            if (hasBillingAddressField !== -1) {

                billingAddress.appendTo('.paymentmethodcheck-info .has-billing-address');
                $('#' + selectedPaymentMethodInfo).click();
            }
        }

        if (response.update_section) {
            $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
        }

        if (response.allow_sections) {
            response.allow_sections.each(function (e) {
                $('#opc-' + e).addClass('allow');
            });
        }
        
        //TODO move it to a new method
        if ($("#billing-address-select").length > 0) {
            Billing.newAddress(!$('#billing-address-select').val());
        }
        if ($("#shipping-address-select").length > 0) {
            Shipping.newAddress(!$('#shipping-address-select').val());
        }

        if (!response.skip_scroll) {
            var errorFields = $('#checkout-steps').find('.input-validation-error');

            if (errorFields.length > 0) {
                var firstErrorField = errorFields.first();
                var firstErrorFieldLabel = firstErrorField.siblings('label');

                if (firstErrorFieldLabel.length > 0) {

                    //scroll to the first error field
                    try {
                        firstErrorFieldLabel[0].scrollIntoView({
                            behavior: "smooth",
                            block: "center"
                        });
                    } catch (error) {
                        //fallback to prevent browser crashing
                        firstErrorFieldLabel[0].scrollIntoView(false);
                    }
                }
            } else {

                //scroll to the top of the page when going
                $("html, body").animate({ scrollTop: 0 }, "slow");
            }
        }

        if (response.goto_section) {
            Checkout.gotoSection(response.goto_section);
            return true;
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        return false;
    }
};

var Billing = {
    form: false,
    saveUrl: false,
    disableBillingAddressCheckoutStep: false,

    init: function (form, saveUrl, disableBillingAddressCheckoutStep) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.disableBillingAddressCheckoutStep = disableBillingAddressCheckoutStep;
    },

    newAddress: function (isNew) {
        //if (isNew) {
        //    this.resetSelectedAddress();
        //    $('#billing-new-address-form').show();
        //} else {
        //    $('#billing-new-address-form').hide();
        //}
    },

    resetSelectedAddress: function () {
        var selectElement = $('#billing-address-select');
        if (selectElement) {
            selectElement.val('');
        }
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('billing');
        
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        //ensure that response.wrong_billing_address is set
        //if not set, "true" is the default value
        if (typeof response.wrong_billing_address == 'undefined') {
            response.wrong_billing_address = false;
        }
        if (Billing.disableBillingAddressCheckoutStep) {
            if (response.wrong_billing_address) {
                Accordion.showSection('#opc-billing');
            } else {
                Accordion.hideSection('#opc-billing');
            }
        }


        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};

var AvalaraTax = {
    addressValidationUrl: false,
    init: function(validationUrl) {
        this.addressValidationUrl = validationUrl;
    },
    useValidatedAddress: function(url, addressId, isNewAddress, address) {
        var postData = {
            addressId: addressId,
            isNewAddress: isNewAddress
        };
        addAntiForgeryToken(postData);

        $.ajax({
            cache: false,
            type: 'POST',
            url: url,
            data: postData,
            dataType: 'json',
            success: function () {

                $.event.trigger({ type: "updateShippingAddressFields", isNewAddress: isNewAddress, address: address });
            }
        });
    }
}

var Shipping = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    newAddress: function (isNew) {
        //if (isNew) {
        //    this.resetSelectedAddress();
        //    $('#shipping-new-address-form').show();
        //} else {
        //    $('#shipping-new-address-form').hide();
        //}
    },

    togglePickUpInStore: function (pickupInStoreInput) {
        if (pickupInStoreInput.checked) {
            $('#shipping-addresses-form').hide();
        }
        else {
            $('#shipping-addresses-form').show();
        }
    },

    resetSelectedAddress: function () {
        var selectElement = $('#shipping-address-select');
        if (selectElement) {
            selectElement.val('');
        }
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('shipping');
        
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        } else {
            if (response.goto_section && AvalaraTax.addressValidationUrl) {
                $.ajax({
                    cache: false,
                    url: AvalaraTax.addressValidationUrl,
                    type: 'get',
                    success: function (data) {

                        if (data) {
                            $(".avalaraAddressValidationWindow").html(data);

                            var window = $(".avalaraAddressValidationWindow").data("kendoWindow");
                            setTimeout(function(){
                                    window.center();
                                    window.open();
                            }, 500);
                        }
                    }
                });
            }
        }

        Checkout.setStepResponse(response);
    }
};

var ShippingOptions = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('shipping-options');

        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};

var ShippingMethod = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    validate: function() {
        var methods = document.getElementsByName('shippingoption');
        if (methods.length==0) {
            alert('Your order cannot be completed at this time as there is no shipping methods available for it. Please make necessary changes in your shipping address.');
            return false;
        }

        for (var i = 0; i< methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify shipping method.');
        return false;
    },
    
    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        if (this.validate()) {
            Checkout.setLoadWaiting('shipping-method');
        
            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};

var PaymentMethod = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    validate: function () {
        var methods = document.getElementsByName('paymentmethod');
        if (methods.length == 0) {
            alert('Your order cannot be completed at this time as there is no payment methods available for it.');
            return false;
        }
        
        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify payment method.');
        return false;
    },
    
    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        if (this.validate()) {
            Checkout.setLoadWaiting('payment-method');
            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $(this.form + ", input[name=paymentmethod]:checked, input[name=paymentmethodcheck]:checked").serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};

var PaymentInfo = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        Checkout.setLoadWaiting('payment-info');
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};

var ConfirmOrder = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (saveUrl, successUrl) {
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    save: function () {
        if (Checkout.loadWaiting != false) return;
        
        //terms of service
        var termOfServiceOk = true;
        if ($('#termsofservice').length > 0) {
            //terms of service element exists
            if (!$('#termsofservice').is(':checked')) {
                $("#terms-of-service-warning-box").dialog();
                termOfServiceOk = false;
            } else {
                termOfServiceOk = true;
            }
        }
        if (termOfServiceOk) {
            Checkout.setLoadWaiting('confirm-order');

            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $('#opc-confirm_order .checkout-attributes *').serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        } else {
            return false;
        }
    },
    
    resetLoadWaiting: function (transport) {
        Checkout.setLoadWaiting(false, ConfirmOrder.isSuccess);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }
        
        if (response.redirect) {
            ConfirmOrder.isSuccess = true;
            location.href = response.redirect;
            return;
        }
        if (response.success) {
            ConfirmOrder.isSuccess = true;
            window.location = ConfirmOrder.successUrl;
        }

        Checkout.setStepResponse(response);
    }
};