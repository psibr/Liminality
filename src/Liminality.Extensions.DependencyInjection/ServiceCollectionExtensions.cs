using Microsoft.Extensions.DependencyInjection;
using System;

namespace Liminality.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddLiminality(this IServiceCollection services)
        {
        }

        public static void AddTypedStateMachine<TStateMachine>(this IServiceCollection services)
        {
        }
    }
}
