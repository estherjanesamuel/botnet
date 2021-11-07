﻿using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.OpenAI {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddOpenAIClient(this IServiceCollection services) {
			services.AddTransient<OpenAIClient>();
			return services;
		}
	}
}
