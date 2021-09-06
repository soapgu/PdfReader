using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Caliburn.Micro;
using System.Dynamic;
using System.Configuration;
using System.Reflection;


namespace PdfReader
{
    public class AppBootstrapper : BootstrapperBase
    {
        CompositionContainer container;
        

        public AppBootstrapper()
        {
            Initialize();
        }

        /// <summary>
        /// By default, we are configure to use MEF
        /// </summary>
        protected override void Configure()
        {
            var catalog = new AggregateCatalog(
                AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()
                );

            container = new CompositionContainer(catalog);

            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);
            batch.AddExportedValue(catalog);

            container.Compose(batch);
        }



        protected override object GetInstance(Type serviceType, string key)
        {
            return GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return GetInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            dynamic settings = new ExpandoObject();
            //settings.WindowStyle = WindowStyle.ToolWindow;
            //settings.ShowInTaskbar = false;
            //settings.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            settings.ResizeMode = System.Windows.ResizeMode.NoResize;
            settings.SizeToContent = System.Windows.SizeToContent.Manual;

            settings.WindowState = System.Windows.WindowState.Maximized;

            settings.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            //settings.MaxWidth = 512;
            //settings.MaxHeight = 420;

            

            settings.Title = "PDF阅读器";
            DisplayRootViewFor<IShell>(settings);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);

        }

        /// <summary>
        /// 	<para>Returns an instance of the custom implementation for the provided type or contract name.</para>
        /// </summary>
        /// <param name="serviceType">The type of the requested instance.</param>
        /// <param name="key">The contract name of the instance requested. If no contract name is specifed, the type will be used.</param>
        /// <param name="requiredCreationPolicy">Optionally specify whether the returned instance should be a shared, non-shared or any instance.</param>
        /// <returns>The requested instance.</returns>
        public object GetInstance(Type serviceType, string key, CreationPolicy requiredCreationPolicy = CreationPolicy.Any)
        {

            var exports = GetExportsCore(container, serviceType, key, requiredCreationPolicy).ToList();
            if (!exports.Any())
                throw new Exception(string.Format("CouldNotLocateAnyInstancesOfContract",
                                                  serviceType != null ? serviceType.ToString() : key));

            return exports.First().Value;
        }

        public IEnumerable<object> GetInstances(Type serviceType, CreationPolicy requiredCreationPolicy = CreationPolicy.Any)
        {

            IEnumerable<Export> exports = GetExportsCore(container, serviceType, null, requiredCreationPolicy);

            return exports.Select(e => e.Value);
        }


        internal static IEnumerable<Export> GetExportsCore(CompositionContainer container, Type serviceType, string key, CreationPolicy policy, bool includeDefaults = false)
        {
            string contractName = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            string requiredTypeIdentity = serviceType != null
                                              ? AttributedModelServices.GetTypeIdentity(serviceType)
                                              : null;
            var importDef = new ContractBasedImportDefinition(
                contractName,
                requiredTypeIdentity,
                Enumerable.Empty<KeyValuePair<string, Type>>(),
                ImportCardinality.ZeroOrMore,
                false,
                true,
                policy);

            return container.GetExports(importDef).Where(e => includeDefaults || !e.Metadata.ContainsKey("IsDefault"));
        }
    }
}
