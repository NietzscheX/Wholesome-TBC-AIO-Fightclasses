﻿using System;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Hunter
{
    public class BeastMasteryParty : Hunter
    {
        protected override void BuffRotation()
        {
            // Aspect of the Cheetah
            if (!Me.IsMounted
                && !Me.HaveBuff("Aspect of the Cheetah")
                && MovementManager.InMoveTo
                && Me.ManaPercentage > 60
                && settings.UseAspectOfTheCheetah
                && cast.Normal(AspectCheetah))
                return;

            // PARTY Drink
            ToolBox.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold);
        }
        protected override void Pull()
        {
            // Hunter's Mark
            if (ObjectManager.Pet.IsValid
                && !HuntersMark.TargetHaveBuff
                && cast.Normal(HuntersMark))
                return;

            // Serpent Sting
            if (!ObjectManager.Target.HaveBuff("Serpent Sting")
                && ObjectManager.Target.GetDistance < 34f
                && ObjectManager.Target.GetDistance > 13f
                && cast.Normal(SerpentSting))
                return;
        }
        protected override void CombatRotation()
        {
            double lastAutoInMilliseconds = (DateTime.Now - _lastAuto).TotalMilliseconds;

            WoWUnit Target = ObjectManager.Target;

            if (Target.GetDistance < 10f
                && !cast.IsBackingUp)
                ToolBox.CheckAutoAttack(Attack);

            if (Target.GetDistance > 10f
                && !cast.IsBackingUp)
                ReenableAutoshot();

            if (Target.GetDistance < 13f
                && !settings.BackupFromMelee)
                _canOnlyMelee = true;

            // Mend Pet
            if (ObjectManager.Pet.IsAlive
                && ObjectManager.Pet.HealthPercent <= 50
                && !ObjectManager.Pet.HaveBuff("Mend Pet")
                && cast.Normal(MendPet))
                return;

            // Aspect of the viper
            if (!Me.HaveBuff("Aspect of the Viper") 
                && Me.ManaPercentage < 30
                && cast.Normal(AspectViper))
                return;

            // Aspect of the Hawk
            if (!Me.HaveBuff("Aspect of the Hawk")
                && (Me.ManaPercentage > 90 || Me.HaveBuff("Aspect of the Cheetah"))
                || !Me.HaveBuff("Aspect of the Hawk")
                && !Me.HaveBuff("Aspect of the Cheetah")
                && !Me.HaveBuff("Aspect of the Viper"))
                if (cast.Normal(AspectHawk))
                    return;

            // Aspect of the Monkey
            if (!Me.HaveBuff("Aspect of the Monkey")
                && !AspectHawk.KnownSpell
                && cast.Normal(AspectMonkey))
                return;

            // Disengage
            if (settings.UseDisengage
                && Target.Target == Me.Guid
                && Target.GetDistance < 10
                && cast.Normal(Disengage))
                return;

            // Bestial Wrath
            if (Target.GetDistance < 34f
                && Target.HealthPercent < 100
                && Me.ManaPercentage > 10
                && cast.Normal(BestialWrath))
                return;

            // Rapid Fire
            if (Target.GetDistance < 34f
                && Target.HealthPercent < 100
                && cast.Normal(RapidFire))
                return;

            // Kill Command
            if (ObjectManager.Pet.IsAlive
                && cast.Normal(KillCommand))
                return;

            // Raptor Strike
            if (settings.UseRaptorStrike
                && Target.GetDistance < 6f
                && !RaptorStrikeOn()
                && cast.Normal(RaptorStrike))
                return;

            // Mongoose Bite
            if (Target.GetDistance < 6f
                && cast.Normal(MongooseBite))
                return;

            // Feign Death
            if (Me.HealthPercent < 20
                || (ObjectManager.GetNumberAttackPlayer() > 1 && ObjectManager.GetUnitAttackPlayer().Where(u => u.Target == Me.Guid).Count() > 0)
                && cast.Normal(FeignDeath))
                {
                    Thread.Sleep(500);
                    Move.Backward(Move.MoveAction.PressKey, 100);
                    return;
                }

            // Wing Clip
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.UseConcussiveShot
                && WingClip.IsDistanceGood
                && Target.HealthPercent < 20
                && !Target.HaveBuff("Wing Clip")
                && cast.Normal(WingClip))
                return;

            // Hunter's Mark
            if (ObjectManager.Pet.IsValid
                && !HuntersMark.TargetHaveBuff
                && Target.GetDistance > 13f
                && Target.IsAlive
                && cast.Normal(HuntersMark))
                return;

            // Steady Shot
            if (lastAutoInMilliseconds > 100
                && lastAutoInMilliseconds < 500
                && Me.ManaPercentage > 20
                && SteadyShot.IsDistanceGood
                && cast.Normal(SteadyShot))
                return;

            // Serpent Sting
            if (!Target.HaveBuff("Serpent Sting")
                && Target.GetDistance < 34f
                && Target.HealthPercent >= 60
                && Me.ManaPercentage > 50u
                && !SteadyShot.KnownSpell
                && Target.GetDistance > 13f
                && cast.Normal(SerpentSting))
                return;

            // Intimidation interrupt
            if (Target.GetDistance < 34f
                && ToolBox.TargetIsCasting()
                && settings.IntimidationInterrupt
                && cast.Normal(Intimidation))
                return;

            // Arcane Shot
            if (Target.GetDistance < 34f
                && Target.HealthPercent >= 30
                && Me.ManaPercentage > 80
                && !SteadyShot.KnownSpell
                && cast.Normal(ArcaneShot))
                return;
        }
    }
}
