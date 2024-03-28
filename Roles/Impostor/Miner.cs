﻿using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;
using TOHE.Modules;

namespace TOHE.Roles.Impostor;

internal class Miner : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 4200;
    private static readonly HashSet<byte> PlayerIds = [];
    public static bool HasEnabled => PlayerIds.Any();
    public override bool IsEnable => HasEnabled;
    public override CustomRoles ThisRoleBase => CustomRoles.Shapeshifter;
    //==================================================================\\

    private static OptionItem MinerSSDuration;
    private static OptionItem MinerSSCD;

    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Miner);
        MinerSSDuration = FloatOptionItem.Create(Id + 2, "ShapeshiftDuration", new(1f, 180f, 1f), 1, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Miner])
            .SetValueFormat(OptionFormat.Seconds);
        MinerSSCD = FloatOptionItem.Create(Id + 3, "ShapeshiftCooldown", new(1f, 180f, 1f), 15f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Miner])
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Init()
    {
        PlayerIds.Clear();
    }
    public override void Add(byte playerId)
    {
        PlayerIds.Add(playerId);
    }

    public override void ApplyGameOptions(IGameOptions opt, byte playerId)
    {
        AURoleOptions.ShapeshifterCooldown = MinerSSCD.GetFloat();
        AURoleOptions.ShapeshifterDuration = MinerSSDuration.GetFloat();
    }
    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.AbilityButton.OverrideText(Translator.GetString("MinerTeleButtonText"));
    }

    public override void OnShapeshift(PlayerControl shapeshifter, PlayerControl target, bool shapeshifting, bool shapeshiftIsHidden)
    {
        if (!shapeshifting && !shapeshiftIsHidden) return;

        if (Main.LastEnteredVent.ContainsKey(shapeshifter.PlayerId))
        {
            var lastVentPosition = Main.LastEnteredVentLocation[shapeshifter.PlayerId];
            Logger.Info($"Miner - {shapeshifter.GetNameWithRole()}:{lastVentPosition}", "MinerTeleport");
            shapeshifter.RpcTeleport(lastVentPosition);
            shapeshifter.RPCPlayCustomSound("Teleport");
        }
    }
}
