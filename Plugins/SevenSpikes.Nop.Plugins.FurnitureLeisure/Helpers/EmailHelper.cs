using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Infrastructure.Constants;
using SevenSpikes.Nop.Plugins.FurnitureLeisure.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SevenSpikes.Nop.Plugins.FurnitureLeisure.Helpers
{
    public class EmailHelper : IEmailHelper
    {
        private const int UNITED_STATES_COUNTRY_ID = 1;

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailSender _emailSender;
        private readonly IStoreContext _storeContext;
        private readonly ICountryService _countryService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IWorkContext _workContext;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ITokenizer _tokenizer;
        private readonly IQueuedEmailService _queuedEmailService;

        public EmailHelper(IMessageTemplateService messageTemplateService,
            IEmailSender emailSender,
            IStoreContext storeContext,
            ICountryService countryService,
            IMessageTokenProvider messageTokenProvider,
            IWorkContext workContext,
            IEmailAccountService emailAccountService,
            ITokenizer tokenizer,
            IQueuedEmailService queuedEmailService)
        {
            _messageTemplateService = messageTemplateService;
            _emailSender = emailSender;
            _storeContext = storeContext;
            _countryService = countryService;
            _messageTokenProvider = messageTokenProvider;
            _workContext = workContext;
            _emailAccountService = emailAccountService;
            _tokenizer = tokenizer;
            _queuedEmailService = queuedEmailService;
        }

        public void SendEmail(CatalogRequestModel model)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateByName(Plugin.CatalogRequestMessageTemplateName, _storeContext.CurrentStore.Id);

            if(messageTemplate == null)
            {
                messageTemplate = new MessageTemplate()
                {
                    Name = Plugin.CatalogRequestMessageTemplateName,
                    BccEmailAddresses = string.Empty,
                    Subject = Plugin.CatalogRequestMessageTemplateSubject,
                    Body = Plugin.CatalogRequestMessageTemplateBody,
                    IsActive = true,
                    LimitedToStores = false
                };

                _messageTemplateService.InsertMessageTemplate(messageTemplate);
            }

            var emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

            var tokens = new List<Token>();

            _messageTokenProvider.AddStoreTokens(tokens, _storeContext.CurrentStore, emailAccount);
            _messageTokenProvider.AddCustomerTokens(tokens, _workContext.CurrentCustomer);

            tokens.AddRange(GetTokens(model));

            var bcc = messageTemplate.GetLocalized(x => x.BccEmailAddresses);
            var subject = messageTemplate.GetLocalized(x => x.Subject);
            var body = messageTemplate.GetLocalized(x => x.Body);

            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            var email = new QueuedEmail()
            {
                Priority = QueuedEmailPriority.High,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = "Sales@FurnitureLeisure.com",
                ToName = "Sales Furniture Leisure",
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id
            };

            _queuedEmailService.InsertQueuedEmail(email);
        }

        private IList<Token> GetTokens(CatalogRequestModel model)
        {
            var usCountry = _countryService.GetCountryById(UNITED_STATES_COUNTRY_ID);

            var state = usCountry.StateProvinces.FirstOrDefault(x => x.Id == model.StateId);
            var stateName = state == null ? string.Empty : state.Name;

            var shouldCreateAccount = model.CreateAccount == 2 ? "true" : "false"; //TODO: Resource
            var shouldRecieveCall = model.ShouldRecieveCall == 2 ? "true" : "false"; //TODO: Resource

            var tokens = new List<Token>()
            {
                new Token("CatalogRequest.FirstName", model.FirstName),
                new Token("CatalogRequest.LastName", model.LastName),
                new Token("CatalogRequest.Email", model.Email),
                new Token("CatalogRequest.CompanyName", model.CompanyName),
                new Token("CatalogRequest.StreetAddress", model.StreetAddress),
                new Token("CatalogRequest.City", model.City),
                new Token("CatalogRequest.State", stateName),
                new Token("CatalogRequest.ZipCode", model.ZipCode),
                new Token("CatalogRequest.PhoneNumber", model.PhoneNumber),
                new Token("CatalogRequest.CreateAccount", shouldCreateAccount),
                new Token("CatalogRequest.ShouldRecieveCall", shouldRecieveCall),
            };

            return tokens;
        }
    }
}
