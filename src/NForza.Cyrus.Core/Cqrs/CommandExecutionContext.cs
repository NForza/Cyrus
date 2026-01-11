using System;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs
{
    /// <summary>
    /// Provides context for command execution including the command and service provider.
    /// </summary>
    /// <typeparam name="TCommand">The type of command being executed.</typeparam>
    public class CommandExecutionContext : IDisposable 
    {
        private readonly IServiceScope serviceScope;

        /// <summary>
        /// Gets the command to be executed.
        /// </summary>
        public object CommandContract { get; }
        public object? Command { get; set; }

        /// <summary>
        /// Gets the service provider for resolving dependencies.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutionContext{TCommand}"/> class.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="serviceScope">The service scope for dependency resolution.</param>
        public CommandExecutionContext(object commandContract, IServiceScope serviceScope)
        {
            CommandContract = commandContract ?? throw new ArgumentNullException(nameof(commandContract));
            this.serviceScope = serviceScope;
            Services = serviceScope.ServiceProvider;
        }

        /// <summary>
        /// Disposes the service scope if one was created.
        /// </summary>
        public void Dispose()
        {
            serviceScope?.Dispose();
        }
    }
}