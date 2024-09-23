﻿using AutoMapper;
using Ordering.Application.Common.Mappings;
using Ordering.Domain.Entities;
using Shared.Enums.Order;

namespace Ordering.Application.Common.Models;

public class OrderDto : IMapForm<Order>
{
    public long Id { get; set; }
    public string DocumentNo { get; set; }
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string ShippingAddress { get; set; }
    public OrderStatus Status { get; set; }        
    public string InvoiceAddress { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Order, OrderDto>().ReverseMap();
    }
}
