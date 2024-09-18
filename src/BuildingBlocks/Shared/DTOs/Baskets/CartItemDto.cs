﻿namespace Shared.DTOs.Baskets
{
    public class CartItemDto
    {
        public int Quantity { get; set; }
        public decimal ItemPrice { get; set; }
        public string ItemNo { get; set; }
        public string ItemName { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
