namespace Shared.Contracts.Models;

public static class HttpMethods
{
    public const string Get = "GET";
    public const string Post = "POST";
    public const string Put = "PUT";
    public const string Patch = "PATCH";
    public const string Delete = "DELETE";
    public const string Head = "HEAD";
    public const string Options = "OPTIONS";

    public static readonly string[] AllMethods = 
    {
        Get, Post, Put, Patch, Delete, Head, Options
    };

    public static bool IsValid(string method)
    {
        return AllMethods.Contains(method?.ToUpperInvariant());
    }
}
