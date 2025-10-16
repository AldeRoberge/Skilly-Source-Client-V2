using RotMG.Common;
using RotMG.Game.Entities;
using RotMG.Networking;
using RotMG.Utils;
using System;
using System.Linq;

namespace RotMG.Game.Logic.Behaviors
{
    public class Grenade(
        float range = 8,
        int damage = 100,
        float radius = 5,
        float? fixedAngle = null,
        int cooldown = 0,
        int cooldownOffset = 0,
        int cooldownVariance = 0,
        ConditionEffectIndex effect = ConditionEffectIndex.Nothing,
        int effectDuration = 0,
        uint color = 0xFFFF0000)
        : Behavior
    {
        public readonly float                 Range            = range;
        public readonly float                 Radius           = radius;
        public readonly int                   Damage           = damage;
        public readonly float?                FixedAngle       = fixedAngle * MathUtils.ToRadians;
        public readonly int                   Cooldown         = cooldown;
        public readonly int                   CooldownOffset   = cooldownOffset;
        public readonly int                   CooldownVariance = cooldownVariance;
        public readonly ConditionEffectDesc[] Effects          =
        [
            new ConditionEffectDesc(effect, effectDuration)
        ];
        public readonly uint                  Color = color;

        public override void Enter(Entity host)
        {
            host.StateCooldown[Id] = CooldownOffset;
        }

        public override bool Tick(Entity host)
        {
            host.StateCooldown[Id] -= Settings.MillisecondsPerTick;
            if (host.StateCooldown[Id] <= 0)
            {
                if (host.HasConditionEffect(ConditionEffectIndex.Stunned))
                    return false;

                var target = host.GetNearestPlayer(Range);
                if (target != null || FixedAngle != null)
                {
                    Vector2 p;
                    if (FixedAngle != null)
                        p = new Vector2(
                            Range * MathF.Cos(FixedAngle.Value) + host.Position.X,
                            Range * MathF.Sin(FixedAngle.Value) + host.Position.Y);
                    else
                        p = new Vector2(
                            target.Position.X,
                            target.Position.Y
                            );


                    var ack = new AoeAck
                    {
                        Damage = Damage,
                        Radius = Radius,
                        Effects = Effects,
                        Position = p,
                        Hitter = host.Desc.DisplayId,
                        Time = Manager.TotalTime + 1500
                    };

                    var eff = GameServer.ShowEffect(ShowEffectIndex.Throw, host.Id, Color, p);
                    var aoe = GameServer.Aoe(p, Radius, Damage, Effects[0].Effect, Color);
                    var players = host.Parent.PlayerChunks.HitTest(host.Position, Player.SightRadius)
                        .Where(e => e is Player j && j.Entities.Contains(host)).ToArray();

                    foreach (var en in players)
                        (en as Player).Client.Send(eff);

                    Manager.AddTimedAction(1500, () => 
                    {
                        foreach (var en in players)
                            if (en.Parent != null)
                            {
                                (en as Player).AwaitAoe(ack);
                                (en as Player).Client.Send(aoe);
                            }
                    });
                }

                host.StateCooldown[Id] = Cooldown;
                if (CooldownVariance != 0)
                    host.StateCooldown[Id] += MathUtils.NextIntSnap(-CooldownVariance, CooldownVariance, Settings.MillisecondsPerTick);
                return true;
            }
            return false;
        }
    }
}
