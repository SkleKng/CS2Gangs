using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using plugin.utils;

namespace plugin.extensions;

public static class PlayerExtensions
{
    public static bool CanTarget(this CCSPlayerController controller, CCSPlayerController target)
    {
        if (target.IsBot) return true;
        return AdminManager.CanPlayerTarget(controller, target);
    }

    public static void PrintLocalizedChat(this CCSPlayerController controller, IStringLocalizer localizer, string local,
        params object[] args)
    {
        if (controller == null || !controller.IsReal()) return;
        string message = localizer[local, args];
        message = message.Replace("%prefix%", localizer["prefix"]);
        message = StringUtils.ReplaceChatColors(message);
        controller.PrintToChat(message);
    }

    public static void PrintLocalizedConsole(this CCSPlayerController controller, IStringLocalizer localizer,
        string local, params object[] args)
    {
        if (controller == null || !controller.IsReal()) return;
        string message = localizer[local, args];
        message = message.Replace("%prefix%", localizer["prefix"]);
        message = StringUtils.ReplaceChatColors(message);
        controller.PrintToConsole(message);
    }

    public static void PrintLocalizedCenter(this CCSPlayerController player, IStringLocalizer localizer, string local,
        params object[] args)
    {
        if (player == null || !player.IsReal()) return;
        string message = localizer[local, args];
        message = message.Replace("%prefix%", localizer["prefix"]);
        message = StringUtils.ReplaceChatColors(message);
        player.PrintToCenter(message);
    }

    public static void SetHp(this CCSPlayerController controller, int health = 100)
    {
        if (health <= 0 || !controller.PawnIsAlive || controller.PlayerPawn.Value == null) return;

        controller.Health = health;
        controller.PlayerPawn.Value.Health = health;

        if (health > 100)
        {
            controller.MaxHealth = health;
            controller.PlayerPawn.Value.MaxHealth = health;
        }

        var weaponServices = controller.PlayerPawn.Value!.WeaponServices;
        if (weaponServices == null) return;

        controller.GiveNamedItem("weapon_healthshot");

        foreach (var weapon in weaponServices.MyWeapons)
            if (weapon != null && weapon.IsValid && weapon.Value!.DesignerName == "weapon_healthshot")
            {
                weapon.Value.Remove();
                break;
            }
    }

    public static void Bury(this CBasePlayerPawn pawn, float depth = 10f)
    {
        var newPos = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y,
            pawn.AbsOrigin!.Z - depth);

        pawn.Teleport(newPos, pawn.AbsRotation!, pawn.AbsVelocity);
    }

    public static void Unbury(this CBasePlayerPawn pawn, float depth = 15f)
    {
        var newPos = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y,
            pawn.AbsOrigin!.Z + depth);

        pawn.Teleport(newPos, pawn.AbsRotation!, pawn.AbsVelocity);
    }

    public static void Freeze(this CBasePlayerPawn pawn)
    {
        pawn.MoveType = MoveType_t.MOVETYPE_OBSOLETE;
    }

    public static void Unfreeze(this CBasePlayerPawn pawn)
    {
        pawn.MoveType = MoveType_t.MOVETYPE_WALK;
    }

    public static void ToggleNoclip(this CBasePlayerPawn pawn)
    {
        if (pawn.MoveType == MoveType_t.MOVETYPE_NOCLIP)
            SetNoclip(pawn, false);
        else
            SetNoclip(pawn, true);
    }

    public static void SetNoclip(this CBasePlayerPawn pawn, bool enabled)
    {
        if (enabled)
        {
            pawn.MoveType = MoveType_t.MOVETYPE_NOCLIP;
            Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 8); // noclip
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }
        else
        {
            pawn.MoveType = MoveType_t.MOVETYPE_WALK;
            Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 2); // walk
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }
    }


    public static void SetNoclip(this CBasePlayerPawn pawn, int toggle)
    {
        switch (toggle)
        {
            case 1:
                SetNoclip(pawn, true);
                break;
            case -1:
                SetNoclip(pawn, false);
                break;
            default:
                ToggleNoclip(pawn);
                break;
        }
    }

    public static void SetGod(this CBasePlayerPawn pawn, int toggle)
    {
        switch (toggle)
        {
            case 1:
                pawn.TakesDamage = false;
                break;
            case -1:
                pawn.TakesDamage = true;
                break;
            case 0:
                pawn.TakesDamage = !pawn.TakesDamage;
                break;
        }
    }

    public static bool IsVIP(this CCSPlayerController player, CS2GangsConfig config)
    {
        if (!player.IsReal())
            return false;
        return AdminManager.PlayerInGroup(player, config.VIPTier1Group!) ||
               AdminManager.PlayerInGroup(player, config.VIPTier2Group!) ||
               AdminManager.PlayerInGroup(player, config.VIPTier3Group!) ||
               AdminManager.PlayerInGroup(player, config.VIPTier4Group!) ||
               AdminManager.PlayerHasPermissions(player, config.DebugPermission!);
    }

    public static int GetVIPTier(this CCSPlayerController player, CS2GangsConfig config)
    {
        if (!player.IsReal())
            return 0;
        if (AdminManager.PlayerHasPermissions(player, config.DebugPermission!))
            return 4;
        if (AdminManager.PlayerInGroup(player, config.VIPTier4Group!))
            return 4;
        if (AdminManager.PlayerInGroup(player, config.VIPTier3Group!))
            return 3;
        if (AdminManager.PlayerInGroup(player, config.VIPTier2Group!))
            return 2;
        if (AdminManager.PlayerInGroup(player, config.VIPTier1Group!))
            return 1;
        return 0;
    }
    public static bool IsReal(this CCSPlayerController player)
    {
        //  Do nothing else before this:
        //  Verifies the handle points to an entity within the global entity list.
        if (!player.IsValid)
            return false;

        if (player.Connected != PlayerConnectedState.PlayerConnected)
            return false;

        if (player.IsBot || player.IsHLTV)
            return false;

        return true;
    }
}