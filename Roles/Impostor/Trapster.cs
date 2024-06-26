﻿namespace TOHE.Roles.Impostor;

internal class Trapster : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 2600;
    private static readonly HashSet<byte> Playerids = [];
    public static bool HasEnabled => Playerids.Any();
    
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.ImpostorKilling;
    //==================================================================\\

    private static OptionItem TrapsterKillCooldown;
    private static OptionItem TrapConsecutiveBodies;
    private static OptionItem TrapTrapsterBody;
    private static OptionItem TrapConsecutiveTrapsterBodies;

    private static readonly HashSet<byte> BoobyTrapBody = [];
    private static readonly Dictionary<byte, byte> KillerOfBoobyTrapBody = [];

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.ImpostorRoles, CustomRoles.Trapster);
        TrapsterKillCooldown = FloatOptionItem.Create(Id + 2, "KillCooldown", new(2.5f, 180f, 2.5f), 30f, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Trapster])
            .SetValueFormat(OptionFormat.Seconds);
        TrapConsecutiveBodies = BooleanOptionItem.Create(Id + 3, "TrapConsecutiveBodies", true, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Trapster]);
        TrapTrapsterBody = BooleanOptionItem.Create(Id + 4, "TrapTrapsterBody", true, TabGroup.ImpostorRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Trapster]);
        TrapConsecutiveTrapsterBodies = BooleanOptionItem.Create(Id + 5, "TrapConsecutiveBodies", true, TabGroup.ImpostorRoles, false)
            .SetParent(TrapTrapsterBody);
    }

    public override void Init()
    {
        BoobyTrapBody.Clear();
        KillerOfBoobyTrapBody.Clear();
        Playerids.Clear();
    }
    public override void Add(byte playerId)
    {
        Playerids.Clear();
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = TrapsterKillCooldown.GetFloat();

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        BoobyTrapBody.Add(target.PlayerId);
        return true;
    }

    public override bool OnCheckReportDeadBody(PlayerControl reporter, GameData.PlayerInfo deadBody, PlayerControl killer)
    {
        var target  = deadBody?.Object;

        // if trapster dead
        if (target.Is(CustomRoles.Trapster) && TrapTrapsterBody.GetBool() && !reporter.Is(CustomRoles.Pestilence))
        {
            var killerId = target.PlayerId;

            Main.PlayerStates[reporter.PlayerId].deathReason = PlayerState.DeathReason.Trap;
            reporter.RpcMurderPlayer(reporter);
            reporter.SetRealKiller(target);

            RPC.PlaySoundRPC(killerId, Sounds.KillSound);
            
            if (TrapConsecutiveTrapsterBodies.GetBool())
            {
                BoobyTrapBody.Add(reporter.PlayerId);
            }
            
            return false;
        }

        // if reporter try reported trap body
        if (BoobyTrapBody.Contains(target.PlayerId) && reporter.IsAlive()
            && !reporter.Is(CustomRoles.Pestilence) && _Player.RpcCheckAndMurder(target, true))
        {
            var killerId = target.PlayerId;
            
            Main.PlayerStates[reporter.PlayerId].deathReason = PlayerState.DeathReason.Trap;
            reporter.SetRealKiller(target);
            reporter.RpcMurderPlayer(reporter);
            
            RPC.PlaySoundRPC(killerId, Sounds.KillSound);
            if (TrapConsecutiveBodies.GetBool())
            {
                BoobyTrapBody.Add(reporter.PlayerId);
            }

            return false;
        }

        return true;
    }
}
