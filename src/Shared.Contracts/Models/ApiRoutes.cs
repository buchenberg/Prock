namespace Shared.Contracts.Models;

public static class ApiRoutes
{
    public const string BaseUrl = "/prock/api";
    
    public static class MockRoutes
    {
        public const string Base = $"{BaseUrl}/mock-routes";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string Update = $"{Base}/{{id}}";
        public const string Delete = $"{Base}/{{id}}";
        public const string Enable = $"{Base}/{{id}}/enable";
        public const string Disable = $"{Base}/{{id}}/disable";
    }
    
    public static class Config
    {
        public const string Base = $"{BaseUrl}/config";
        public const string Get = Base;
        public const string UpdateUpstreamUrl = $"{Base}/upstream-url";
    }
    
    public static class OpenApi
    {
        public const string Base = $"{BaseUrl}/openapi";
        public const string GetAll = $"{Base}/documents";
        public const string GetById = $"{Base}/documents/{{id}}";
        public const string Create = $"{Base}/documents";
        public const string Update = $"{Base}/documents/{{id}}";
        public const string Delete = $"{Base}/documents/{{id}}";
        public const string GenerateMocks = $"{Base}/documents/{{id}}/generate-mocks";
    }
    
    public static class SignalR
    {
        public const string Hub = $"{BaseUrl}/signalr";
    }
}
