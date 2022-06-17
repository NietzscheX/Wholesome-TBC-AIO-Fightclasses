﻿using robotManager.Helpful;
using robotManager.Products;
using System;
using System.Net;
using System.Text;
using System.Threading;
using WholesomeTBCAIO;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;

public static class AutoUpdater
{
    public static void CheckUpdate(string mainVersion)
    {
        if (wManager.Information.Version.Contains("1.7.2"))
        {
            Logger.Log($"AIO couldn't load (v {wManager.Information.Version})");
            Products.ProductStop();
            return;
        }

        Version currentVersion = new Version(mainVersion);

        DateTime dateBegin = new DateTime(2020, 1, 1);
        DateTime currentDate = DateTime.Now;

        long elapsedTicks = currentDate.Ticks - dateBegin.Ticks;
        elapsedTicks /= 10000000;

        double timeSinceLastUpdate = elapsedTicks - AIOTBCSettings.CurrentSetting.LastUpdateDate;

        // If last update try was < 30 seconds ago, we exit to avoid looping
        if (timeSinceLastUpdate < 30)
        {
            Logger.Log($"Last update attempts was {timeSinceLastUpdate} seconds ago. Exiting updater.");
            return;
        }

        try
        {
            AIOTBCSettings.CurrentSetting.LastUpdateDate = elapsedTicks;
            AIOTBCSettings.CurrentSetting.Save();

            string onlineDllLink = "https://github.com/Wholesome-wRobot/Wholesome-TBC-AIO-Fightclasses/raw/master/AIO/Compiled/Wholesome_TBC_AIO_Fightclasses.dll";
            string onlineVersionLink = "https://raw.githubusercontent.com/Wholesome-wRobot/Wholesome-TBC-AIO-Fightclasses/master/AIO/Compiled/Auto_Version.txt";

            var onlineVersionTxt = new WebClient { Encoding = Encoding.UTF8 }.DownloadString(onlineVersionLink);
            Version onlineVersion = new Version(onlineVersionTxt);

            if (onlineVersion.CompareTo(currentVersion) <= 0)
            {
                Logger.Log($"Your version is up to date ({currentVersion} / {onlineVersion})");
                return;
            }

            // File check
            string currentFile = Others.GetCurrentDirectory + @"\FightClass\" + wManager.wManagerSetting.CurrentSetting.CustomClass;
            var onlineFileContent = new WebClient { Encoding = Encoding.UTF8 }.DownloadData(onlineDllLink);
            if (onlineFileContent != null && onlineFileContent.Length > 0)
            {
                Logger.Log($"Updating your version {currentVersion} to online Version {onlineVersion}");
                System.IO.File.WriteAllBytes(currentFile, onlineFileContent); // replace user file by online file
                Thread.Sleep(1000);
                new Thread(CustomClass.ResetCustomClass).Start();
            }
        }
        catch (Exception e)
        {
            Logging.WriteError("Auto update: " + e);
        }
    }
}
