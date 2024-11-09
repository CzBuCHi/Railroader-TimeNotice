using System;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using Game;
using Game.Events;
using JetBrains.Annotations;
using Network.Messages;
using Railloader;
using UI.Builder;
using UI.Common;
using UnityEngine;

namespace TimeNotice;

[UsedImplicitly]
public sealed class TimeNoticePlugin : SingletonPluginBase<TimeNoticePlugin>, IModTabHandler
{
    private const string ModIdentifier = "TimeNotice";

    private static IModdingContext _Context  = null!;
    private static Settings        _Settings = null!;

    public TimeNoticePlugin(IModdingContext context, IUIHelper uiHelper) {
        _Context = context;
        _Settings = _Context.LoadSettingsData<Settings>(ModIdentifier) ?? new Settings();
    }

    public override void OnEnable() {
        Messenger.Default!.Register(this, new Action<TimeMinuteDidChange>(OnTimeMinute));
    }

    public override void OnDisable() {
        Messenger.Default!.Unregister(this);
    }

    private static void OnTimeMinute(TimeMinuteDidChange obj) {
        var now    = TimeWeather.Now;
        var hour   = Mathf.FloorToInt(now.Hours);
        var minute = Mathf.FloorToInt((now.Hours - hour) * 60.0f);

        foreach (var notifyInfo in _Settings.Notifications.Where(notifyInfo => notifyInfo.Hour == hour && notifyInfo.Minute == minute)) {
            ShowNotification(notifyInfo);
        }
    }

    private static void ShowNotification(NotifyInfo notifyInfo) {
        WindowManager.Shared!.Present(new Alert(AlertStyle.Console, notifyInfo.Message, TimeWeather.Now.TotalSeconds));
    }

    private static void SaveSettings() {
        _Context.SaveSettingsData(ModIdentifier, _Settings);
    }

    public void ModTabDidOpen(UIPanelBuilder builder) {
        builder.ButtonStrip(strip => {
            strip.AddButton("Add", AddNew);
            strip.AddExpandingVerticalSpacer();
            strip.AddButton("Save", ModTabDidClose);
        });

        builder.AddSection("Notifications", section => {
            foreach (var notifyInfo in _Settings.Notifications.OrderBy(o => o.Time)) {
                section.AddField("Hour", section.AddSliderQuantized(() => notifyInfo.Hour, () => notifyInfo.Hour.ToString("0"), o => { notifyInfo.Hour = (int)o; }, 1, 0, 23, o => notifyInfo.Hour = (int)o)!);
                section.AddField("Minute", section.AddSliderQuantized(() => notifyInfo.Minute, () => notifyInfo.Minute.ToString("0"), o => { notifyInfo.Minute = (int)o; }, 1, 0, 59, o => notifyInfo.Minute = (int)o)!);
                section.AddField("Message", section.AddInputField(notifyInfo.Message, o => notifyInfo.Message = o)!);
                section.ButtonStrip(strip => {
                    strip.AddButton("Remove", Remove(notifyInfo));
                    strip.AddButton("Show", Show(notifyInfo));
                });
            }
        });

        return;

        void AddNew() {
            _Settings.Notifications.Add(new NotifyInfo { Hour = 12, Minute = 0, Message = "New notification" });
            builder.Rebuild();
        }

        Action Remove(NotifyInfo notifyInfo) => () => {
            _Settings.Notifications.Remove(notifyInfo);
            builder.Rebuild();
        };

        Action Show(NotifyInfo notifyInfo) => () => ShowNotification(notifyInfo);
    }

    public void ModTabDidClose() {
        SaveSettings();
    }
}
