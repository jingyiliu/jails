﻿using System;
using System.Security.Policy;

namespace Jails.Isolators.AppDomain
{
    public class AppDomainIsolator : IIsolator
    {
        private readonly AppDomainOptions _options;

        public AppDomainIsolator(AppDomainOptions options = null)
        {
            _options = options ?? AppDomainOptions.GetDefault();
        }

        public AppDomainOptions Options
        {
            get { return _options; }
        }

        IHost IIsolator.Build(IEnvironment environment)
        {
            var domainName = _options.DomainName ?? Guid.NewGuid().ToString();
            var strongName = typeof (Jail).Assembly.Evidence.GetHostEvidence<StrongName>();

            var domain = System.AppDomain.CreateDomain(domainName, null, _options.Setup, _options.Permissions, strongName);
            
            var host = (AppDomainHost) Activator.CreateInstanceFrom(domain,
                typeof (AppDomainHost).Assembly.CodeBase,
                typeof (AppDomainHost).FullName)
                .Unwrap();

            host.AddAssembly(typeof (Jail).Assembly.GetName());

            foreach (var assemblyName in environment.GetAssemblyNames())
            {
                host.AddAssembly(assemblyName);
            }

            return host;
        }
    }
}
