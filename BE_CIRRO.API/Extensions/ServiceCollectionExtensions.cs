using BE_CIRRO.Core.Repositories;
using BE_CIRRO.Core.Services;
using BE_CIRRO.Domain.IRepositories;

namespace BE_CIRRO.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Đăng ký các dịch vụ từ BE_CIRRO.Core
        //User
        services.AddScoped<UserService>();
        services.AddScoped<IUserRepository, UserRepository>();

        //Folder
        services.AddScoped<FolderService>();
        services.AddScoped<IFolderRepository, FolderRepository>();


        return services;
    }
}
