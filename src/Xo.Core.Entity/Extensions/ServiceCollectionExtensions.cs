using Xo.Core.Entity.Mappers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Xo.Core.Entity.Extensions;

public static partial class ServiceCollectionExtensions
{
	public static IServiceCollection AddMapper(this IServiceCollection @this)
	{
		@this.TryAddSingleton(new MapperConfiguration(config => config.AddProfile<StateProfile>()).CreateMapper());
		return @this;
	}

	public static IServiceCollection AddDatabase(this IServiceCollection @this,
		IConfiguration config) => @this.AddDatabaseConfig(config);

	private static IServiceCollection AddDatabaseConfig(this IServiceCollection @this,
		IConfiguration config)
	{
		var sqlLiteConnectionString = config
			.GetConnectionString("SqlLite");
		var postgresConnectionString = config
			.GetConnectionString("Postgres");
		var migrationsAssembly = typeof(CoreDbCtx).GetTypeInfo().Assembly.GetName().Name;

		return @this
			.AddDbContext<CoreDbCtx>(config =>
			{
				if (!string.IsNullOrWhiteSpace(sqlLiteConnectionString))
				{
					config.UseSqlite(sqlLiteConnectionString);
				}
				else
				{
					config.UseNpgsql(postgresConnectionString);
				}
			}, ServiceLifetime.Transient);
	}
}