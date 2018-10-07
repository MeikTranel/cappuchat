﻿using Chat.Client.Framework;
using Chat.Client.Presenters;
using Chat.Client.Signalhelpers.Contracts;
using Chat.Client.SignalHelpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using Chat.DataAccess;
using Chat.Shared.Models;
using MahApps.Metro;
using Microsoft.SqlServer.Server;
using WPFNotification.Model;
using WPFNotification.Services;

namespace Chat.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private SignalHubConnectionHelper _hubConnectionHelper;
        private ViewProvider _viewProvider;
        private CappuMainPresenter _cappuMainPresenter;

        private async void AppOnStartup(object sender, StartupEventArgs e)
        {
            ThemeManager.AddAccent("CustomTheme", new Uri("pack://application:,,,/Chat.Client;component/Styles/CustomTheme.xaml"));

            // get the current app style (theme and accent) from the application
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);

            // now change app style to the custom accent and current theme
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent("CustomTheme"),
                theme.Item1);


            _viewProvider = new ViewProvider();

            Current.DispatcherUnhandledException += ApplicationCurrentOnDispatcherUnhandledException;
            Current.Exit += ApplicationCurrentOnExit;

            var testMessage =
                new SimpleMessage(new SimpleUser("mgg"), new SimpleUser("mtr"), "Das ist eine Test message!");
            testMessage.IsLocalMessage = true;
            testMessage.MessageSentDateTime = DateTime.Now;

            DataAccess.DataAccess.InitializeDatabase();
            new ChatRepository().InsertMessage(testMessage);

            _hubConnectionHelper = new SignalHubConnectionHelper("http://localhost:1232/signalr/hubs");


            ISignalHelperFacade signalHelperFacade = new SignalHelperFacade
            {
                ChatSignalHelper = new ChatSignalHelper(_hubConnectionHelper.CreateHubProxy("ChatHub")),
                LoginSignalHelper = new LoginSignalHelper(_hubConnectionHelper.CreateHubProxy("LoginHub")),
                RegisterSignalHelper = new RegisterSignalHelper(_hubConnectionHelper.CreateHubProxy("RegisterHub"))
            };

            await _hubConnectionHelper.Start();

            _cappuMainPresenter = new CappuMainPresenter(signalHelperFacade, _viewProvider)
            {
                LoginPresenter = { ConnectedToServer = _hubConnectionHelper.Connected }
            };

            _cappuMainPresenter.LoginPresenter.StartConnection += LoginPresenterOnStartConnection;

            _viewProvider.Show(_cappuMainPresenter);
        }

        private void ApplicationCurrentOnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(e.Exception.Message))
            {
                Exception innerException = e.Exception.InnerException;

                while (innerException != null)
                {
                    message = message + string.Join(Environment.NewLine, innerException.Message);
                    innerException = innerException.InnerException;
                }
            }
            else
                message = e.Exception.Message;

            _viewProvider.ShowMessage(Texts.Texts.Error, message);
        }

        private void ApplicationCurrentOnExit(object sender, ExitEventArgs e)
        {
            _hubConnectionHelper.Stop();
        }

        private Task<bool> LoginPresenterOnStartConnection()
        {
            return _hubConnectionHelper.Start();
        }
    }
}
