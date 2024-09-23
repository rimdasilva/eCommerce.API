﻿using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Basket.API.Service;
using Basket.API.Service.Interfaces;
using Contracts.Common.Interfaces;
using Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs.ScheduledJob;
using ILogger = Serilog.ILogger;

namespace Basket.API.Repositories;

public class BasketRepository(IDistributedCache redisCacheService, ISerializeService serializeService, ILogger logger,
    BackgroundJobHttpService backgroundJobHttpService, IEmailTemplateService emailTemplateService) : IBasketRepository
{
    private readonly IDistributedCache _redisCacheService = redisCacheService;
    private readonly ISerializeService _serializeService = serializeService;
    private readonly ILogger _logger = logger;
    private readonly BackgroundJobHttpService _backgroundJobHttpService = backgroundJobHttpService;
    private readonly IEmailTemplateService _emailTemplateService = emailTemplateService;

    public async Task<bool> DeleteBasketFromUserName(string username)
    {

        await DeleteReminderCheckoutOrder(username);

        _logger.Information($"BEGIN: DeleteBasketFromUserName {username}");

        try
        {
            await _redisCacheService.RemoveAsync(username);

            _logger.Information($"END: DeleteBasketFromUserName {username}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"DeleteBasketFromUserName {ex.Message}");
            throw;
        }
    }

    public async Task<Cart> GetBasketByUserName(string username)
    {
        _logger.Information($"BEGIN: GetBasketByUserName {username}");

        var basket = await _redisCacheService.GetStringAsync(username);

        _logger.Information($"END: GetBasketByUserName {username}");


        return string.IsNullOrEmpty(basket) ? null : _serializeService.Deserialize<Cart>(basket);
    }

    public async Task<Cart> UpdateBasket(Cart cart, DistributedCacheEntryOptions options = null)
    {   
        await DeleteReminderCheckoutOrder(cart.Username);

        _logger.Information($"BEGIN: UpdateBasket {cart.Username}");

        if (options != null)
            await _redisCacheService.SetStringAsync(cart.Username,
                    _serializeService.Serialize(cart), options);
        else
            await _redisCacheService.SetStringAsync(cart.Username,
                    _serializeService.Serialize(cart));

        _logger.Information($"END: UpdateBasket {cart.Username}");

        try
        {
            await TriggerSendEmailReminderCheckoutOrder(cart);
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
        }

        return await GetBasketByUserName(cart.Username);
    }

    private async Task DeleteReminderCheckoutOrder(string username)
    {
        var cart = await GetBasketByUserName(username);
        if (cart == null || string.IsNullOrEmpty(cart.JobId)) return;

        var jobId = cart.JobId;
        var uri = $"{_backgroundJobHttpService.ScheduledJobUrl}/delete/jobId/{jobId}";
        await _backgroundJobHttpService.Client.DeleteAsync(uri);
        _logger.Information($"DeleteReminderCheckoutOrder: Deleted Job Id: {jobId}");
    }

    public async Task<bool> TriggerSendEmailReminderCheckoutOrder(Cart cart)
    {
        var emailTemplate = _emailTemplateService.GenerateReminderCheckoutOrderEmail(cart.Username);

        var model = new ReminderCheckoutOrderDto(cart.EmailAddress, "Reminder checkout", emailTemplate,
            DateTimeOffset.UtcNow./*AddDays(1).AddHours(8).*/AddMinutes(3));

        var uri = $"{_backgroundJobHttpService.ScheduledJobUrl}/send-mail-reminder-checkout-order";
        var response = await _backgroundJobHttpService.Client.PostAsJson(uri, model);

        if (response.EnsureSuccessStatusCode().IsSuccessStatusCode)
        {
            var jobId = await response.ReadContentAs<string>();
            if (!string.IsNullOrEmpty(jobId))
            {
                cart.JobId = jobId;
                await _redisCacheService.SetStringAsync(cart.Username, 
                    _serializeService.Serialize(cart));
            }
        }

        return true;
    }
}
