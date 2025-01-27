﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.API.Entities;
using Product.API.Repositories.Interfaces;
using Shared.DTOs.Product;
using System.ComponentModel.DataAnnotations;

namespace Product.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(IProductRepository repository, IMapper mapper) : ControllerBase
{
    private readonly IProductRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    #region CRUD

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _repository.GetProducts();
        var result = _mapper.Map<IEnumerable<ProductDto>>(products);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetProduct([Required] long id)
    {
        var product = await _repository.GetProduct(id);
        if (product == null) return NotFound();

        var result = _mapper.Map<ProductDto>(product);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
    {
        var productEnity = await _repository.GetProductByNo(productDto.No);
        if(productEnity != null) return BadRequest($"Product No: {productDto.No} is existed.");

        var product = _mapper.Map<CatalogProduct>(productDto);
        await _repository.CreateProduct(product);
        await _repository.SaveChangesAsync();

        var result = _mapper.Map<ProductDto>(product);
        return Ok(result);
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<IActionResult> UpdateProduct([Required]long id, [FromBody] UpdateProductDto productDto)
    {
        var product = await _repository.GetProduct(id);
        if (product == null) return NotFound();

        var updateProduct = _mapper.Map(productDto, product);

        await _repository.UpdateAsync(updateProduct);
        await _repository.SaveChangesAsync();

        var result = _mapper.Map<ProductDto>(product);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteProduct([Required] long id)
    {
        var product = await _repository.GetProduct(id);
        if (product == null) return NotFound();

        await _repository.DeleteAsync(product);
        await _repository.SaveChangesAsync();

        return NoContent();
    }
    #endregion

    #region Additional Resources
    [HttpGet("get-product-by-no/{productNo}")]
    public async Task<IActionResult> GetProductByNo([Required] string productNo)
    {
        var product = await _repository.GetProductByNo(productNo);
        if (product == null) return NotFound();

        var result = _mapper.Map<ProductDto>(product);
        return Ok(result);
    }
    #endregion
}
