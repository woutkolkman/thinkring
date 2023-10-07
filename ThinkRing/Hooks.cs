using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.Runtime.CompilerServices;
using RWCustom;

namespace ThinkRing
{
    class Hooks
    {
        public static void Apply()
        {
            //initialize options & load sprites
            On.RainWorld.OnModsInit += RainWorldOnModsInitHook;

            //preset correct options
            On.RainWorld.PostModsInit += RainWorldPostModsInitHook;

            //at tickrate
            On.RainWorldGame.Update += RainWorldGameUpdateHook;

            //at framerate
            On.RainWorldGame.RawUpdate += RainWorldGameRawUpdateHook;

            //at new game
            On.RainWorldGame.ctor += RainWorldGameCtorHook;

            //hook for kill effect
            IDetour detourKillCreature = new Hook(
                typeof(MouseDrag.Health).GetMethod("KillCreature", BindingFlags.Static | BindingFlags.Public),
                typeof(Hooks).GetMethod("MouseDragHealth_KillCreature_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
            );

            //hook for revive effect
            IDetour detourReviveCreature = new Hook(
                typeof(MouseDrag.Health).GetMethod("ReviveCreature", BindingFlags.Static | BindingFlags.Public),
                typeof(Hooks).GetMethod("MouseDragHealth_ReviveCreature_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
            );
        }


        //initialize options & load sprites
        static void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            MachineConnector.SetRegisteredOI(Plugin.GUID, new Options());

            try {
                Futile.atlasManager.LoadImage("sprites" + System.IO.Path.DirectorySeparatorChar + "thinkring_karma");
            } catch (Exception ex) {
                Plugin.Logger.LogError("RainWorldOnModsInitHook exception: " + ex.ToString());
            }
            Plugin.Logger.LogDebug("RainWorldOnModsInitHook, sprites loaded");
        }


        //preset correct options
        static void RainWorldPostModsInitHook(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);

            //if mod runs for the first time, change Mouse Drag options
            if (Options.hasRanBefore.Value)
                return;
            Options.hasRanBefore.Value = true;
            MachineConnector.SaveConfig(MachineConnector.GetRegisteredOI(Plugin.GUID));
            //TODO, if config is reset, this option will also be reset, and Mouse Drag option will be reset next game restart

            Plugin.Logger.LogDebug("RainWorldOnModsInitHook, changing Mouse Drag options");
            try {
                MouseDrag.Options.velocityDrag.Value = true;
                MachineConnector.SaveConfig(MachineConnector.GetRegisteredOI(MouseDrag.Plugin.GUID));
            } catch (Exception ex) {
                Plugin.Logger.LogError("RainWorldOnModsInitHook exception: " + ex.ToString());
            }
        }


        //at tickrate
        static void RainWorldGameUpdateHook(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);
            HaloManager.Update(self);
        }


        //at framerate
        static void RainWorldGameRawUpdateHook(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);
        }


        //at new game
        static void RainWorldGameCtorHook(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);

            foreach (Options.ActivateTypes val in Enum.GetValues(typeof(Options.ActivateTypes)))
                if (String.Equals(Options.activateType.Value, val.ToString()))
                    HaloManager.activeType = val;
            foreach (Options.ColorTypes val in Enum.GetValues(typeof(Options.ColorTypes)))
                if (String.Equals(Options.colorType.Value, val.ToString()))
                    HaloManager.colorType = val;
            foreach (Options.LightningTypes val in Enum.GetValues(typeof(Options.LightningTypes)))
                if (String.Equals(Options.lightningType.Value, val.ToString()))
                    HaloManager.lightningType = val;
            foreach (Options.HaloTypes val in Enum.GetValues(typeof(Options.HaloTypes)))
                if (String.Equals(Options.haloType.Value, val.ToString()))
                    HaloManager.haloType = val;
            Plugin.Logger.LogDebug("RainWorldGameCtorHook, activeType: " + HaloManager.activeType.ToString() + 
                ", colorType: " + HaloManager.colorType.ToString() + 
                ", lightningType: " + HaloManager.lightningType.ToString() + 
                ", haloType: " + HaloManager.haloType.ToString());
        }


        //hook for kill effect
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MouseDragHealth_KillCreature_RuntimeDetour(Action<RainWorldGame, PhysicalObject> orig, RainWorldGame game, PhysicalObject obj)
        {
            bool shouldPop = obj is PhysicalObject;
            bool isAlive = (obj as Creature)?.State?.alive ?? false;
            orig(game, obj);
            if (Options.saintPop.Value != true)
                return;

            if (!shouldPop || obj?.room == null)
                return;
            UnityEngine.Vector2 pos = (obj as Creature)?.mainBodyChunk.pos ?? obj.firstChunk.pos;

            if (isAlive) {
                obj.room.PlaySound(SoundID.Firecracker_Bang, pos, 1f, 0.75f + UnityEngine.Random.value);
                obj.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pos, 1f, 0.5f + UnityEngine.Random.value * 0.5f);
            } else {
                obj.room.PlaySound(SoundID.Snail_Pop, pos, 1f, 1.5f + UnityEngine.Random.value);
            }
            for (int n = 0; n < 20; n++)
                obj.room.AddObject(new Spark(pos, Custom.RNV() * UnityEngine.Random.value * 40f, new UnityEngine.Color(1f, 1f, 1f), null, 30, 120));
        }


        //hook for revive effect
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MouseDragHealth_ReviveCreature_RuntimeDetour(Action<PhysicalObject> orig, PhysicalObject obj)
        {
            bool shouldPop = obj is PhysicalObject;
            bool isAlive = (obj as Creature)?.State?.alive ?? false;
            orig(obj);
            if (Options.saintPop.Value != true)
                return;

            if (!shouldPop || obj?.room == null)
                return;
            UnityEngine.Vector2 pos = (obj as Creature)?.mainBodyChunk.pos ?? obj.firstChunk.pos;

            if (!isAlive && obj is Creature) {
                obj.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pos, 1f, 1f);
                obj.room.AddObject(new ShockWave(pos, 100f, 0.07f, 6, false));
                for (int i = 0; i < 10; i++)
                    obj.room.AddObject(new WaterDrip(pos, Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Mathf.Lerp(4f, 21f, UnityEngine.Random.value), false));
            } else {
                obj.room.PlaySound(SoundID.HUD_Pause_Game, pos, 1f, 0.5f);
            }
        }
    }
}
