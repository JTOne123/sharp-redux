﻿using Autofac;
using Todo.Engine;
using Sharp.Redux.Visualizer;
using System.Windows;

namespace Todo.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IoC.Init();
            // connect visualizer
            var sourceDispatcher = IoCRegistrar.Container.Resolve<ITodoReduxDispatcher>();
            Bootstrapper.Init(sourceDispatcher);
            ReduxVisualizer.Init(
                sourceDispatcher,
                new string[] { "Todo.Engine.Actions" });
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            IoCRegistrar.Container.Dispose();
            base.OnExit(e);
        }
    }
}
