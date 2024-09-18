﻿using MongoDB.Driver;
using Shared.SeedWork;

namespace Infrastructure.Extensions;

public static class MongoCollectionExtensions
{
    public static Task<PagedList<TDestination>> PaginatedListAsync<TDestination>(
        this IMongoCollection<TDestination> collection,
        FilterDefinition<TDestination> filter,
        int pageIndex, int pageSize) where TDestination : class
        => PagedList<TDestination>.ToPagedList(collection, filter, pageIndex, pageSize);
}
