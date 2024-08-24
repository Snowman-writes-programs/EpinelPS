﻿using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace ServerSelector.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        CmbServerSelection.SelectedIndex = ServerSwitcher.IsUsingOfficalServer() ? 0 : 1;

        TxtIpAddress.IsEnabled = !ServerSwitcher.IsUsingOfficalServer();
        ChkOffline.IsChecked = ServerSwitcher.IsOffline();

        if (OperatingSystem.IsWindows() && !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            TabPc.Content = new TextBlock()
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Text = "Administrator privileges are required to change servers."
            };
        }

        UpdateIntegrityLabel();
    }

    private void SetGamePathValid(bool isValid)
    {
        if (isValid)
        {
            txtGamePath.BorderBrush = null;
        }
        else
        {
            txtGamePath.BorderBrush = new SolidColorBrush(Color.FromRgb(255,0,0));
        }
    }
    private void SetLauncherPathValid(bool isValid)
    {
        if (isValid)
        {
            txtLauncherPath.BorderBrush = null;
        }
        else
        {
            txtLauncherPath.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }
    }

    private bool ValidatePaths(bool showMessage)
    {
        if (string.IsNullOrEmpty(txtGamePath.Text))
        {
            SetGamePathValid(false);
            if (showMessage)
                ShowWarningMsg("Game path is blank", "Error");
            return false;
        }
        if (string.IsNullOrEmpty(txtLauncherPath.Text))
        {
            SetLauncherPathValid(false);
            if (showMessage)
                ShowWarningMsg("Launcher path is blank", "Error");
            return false;
        }

        if (!Directory.Exists(txtGamePath.Text))
        {
            SetGamePathValid(false);
            if (showMessage)
                ShowWarningMsg("Game path does not exist", "Error");
            return false;
        }

        if (!Directory.Exists(txtLauncherPath.Text))
        {
            SetLauncherPathValid(false);
            if (showMessage)
                ShowWarningMsg("Launcher path does not exist", "Error");
            return false;
        }

        if (!File.Exists(Path.Combine(txtLauncherPath.Text, "nikke_launcher.exe")))
        {
            SetGamePathValid(false);
            if (showMessage)
                ShowWarningMsg("Launcher path is invalid. Make sure that the game executable exists in the launcher folder", "Error");

            return false;
        }

        SetGamePathValid(true);
        SetLauncherPathValid(true);

        return true;
    }

    private async void UpdateIntegrityLabel()
    {
        if (!ValidatePaths(false) || txtGamePath.Text == null || txtLauncherPath.Text == null)
            return;

        SetLoadingScreenVisible(true);
        LblStatus.Text = "Status: " + await ServerSwitcher.CheckIntegrity(txtGamePath.Text, txtLauncherPath.Text);
        SetLoadingScreenVisible(false);
    }

    private void SetLoadingScreenVisible(bool visible)
    {
        if (visible)
        {
            MainUI.IsVisible = false;
            LoadingUI.IsVisible = true;
        }
        else
        {
            LoadingUI.IsVisible = false;
            MainUI.IsVisible = true;
        }
    }

    private async void Save_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!ValidatePaths(true) || txtGamePath.Text == null || txtLauncherPath.Text == null)
            return;

        if (CmbServerSelection.SelectedIndex == 1)
        {
            if (!IPAddress.TryParse(TxtIpAddress.Text, out _))
            {
                ShowWarningMsg("Invalid IP address. The entered IP address should be IPv4 or IPv6.", "Error");
                return;
            }
        }
        if (TxtIpAddress.Text == null) TxtIpAddress.Text = "";

        SetLoadingScreenVisible(true);
        try
        {
            await ServerSwitcher.SaveCfg(CmbServerSelection.SelectedIndex == 0, txtGamePath.Text, txtLauncherPath.Text, TxtIpAddress.Text, ChkOffline.IsChecked ?? false);
        }
        catch (Exception ex)
        {
            ShowWarningMsg("Failed to save configuration: " + ex.ToString(), "Error");
        }
        UpdateIntegrityLabel();
        SetLoadingScreenVisible(false);
    }

    private void CmbServerSelection_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CmbServerSelection != null)
        {
            TxtIpAddress.IsEnabled = CmbServerSelection.SelectedIndex == 1;
            ChkOffline.IsEnabled = CmbServerSelection.SelectedIndex == 1;
            LblIp.IsEnabled = CmbServerSelection.SelectedIndex == 1;
        }
    }

    public static void ShowWarningMsg(string text, string title)
    {
        ContentDialog dlg = new ContentDialog() { Title = title, Content = text, PrimaryButtonText = "OK" };
        dlg.ShowAsync();
    }

    private async void BtnSelectGamePath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null)
        {
            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select game path (with game executable)",
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                txtGamePath.Text = files[0].TryGetLocalPath();

                // validate if the folder has game exe
                if (!string.IsNullOrEmpty(txtGamePath.Text))
                {
                    if (!File.Exists(Path.Combine(txtGamePath.Text, "nikke.exe")))
                    {
                        ShowWarningMsg("Game path is invalid. Make sure that nikke.exe exists in the launcher folder", "Error");
                        return;
                    }
                }
                UpdateIntegrityLabel();
            }
        }
    }
    private async void BtnSelectLauncherPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null)
        {
            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select launcher path",
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                txtLauncherPath.Text = files[0].TryGetLocalPath();

                // validate if the folder has game exe
                if (!string.IsNullOrEmpty(txtLauncherPath.Text))
                {
                    if (!File.Exists(Path.Combine(txtLauncherPath.Text, "nikke_launcher.exe")))
                    {
                        ShowWarningMsg("Launcher path is invalid. Make sure that the game executable exists in the launcher folder", "Error");
                        return;
                    }
                }
                UpdateIntegrityLabel();
            }
        }
    }
    private void GamePath_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        UpdateIntegrityLabel();
    }
    private void LauncherPath_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        UpdateIntegrityLabel();
    }
}
