using api.plugin.models;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;

namespace api.plugin.services;

public interface IAnnouncerService
{
    public void AnnounceToServer(string message);
    public void AnnounceToServerLocalized(IStringLocalizer localizer, string local, params object[] args);
    public void AnnounceToGang(Gang gang, string message);
    public void AnnounceToGangLocalized(Gang gang, IStringLocalizer localizer, string local, params object[] args);
}