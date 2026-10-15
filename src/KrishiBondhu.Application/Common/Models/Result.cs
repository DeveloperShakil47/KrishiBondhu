using System;
using System.Collections.Generic;
using System.Linq;

namespace KrishiBondhu.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; protected set; }
    public bool IsSuccess => Succeeded;
    public string? Message { get; protected set; }
    public List<string> Errors { get; protected set; } = new();
    public string? ErrorMessage => Errors.FirstOrDefault() ?? Message;

    public static Result Success(string? message = null) => new() { Succeeded = true, Message = message };
    public static Result Failure(string error) => new() { Succeeded = false, Errors = new List<string> { error } };
    public static Result Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors.ToList() };
}

public class Result<T> : Result
{
    public T? Data { get; private set; }
    public T? Value => Data;

    public static Result<T> Success(T data, string? message = null) => new() { Succeeded = true, Data = data, Message = message };
    public new static Result<T> Failure(string error) => new() { Succeeded = false, Errors = new List<string> { error } };
    public new static Result<T> Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors.ToList() };
}

public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
