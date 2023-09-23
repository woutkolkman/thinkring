using System;

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
        }


        //initialize options & load sprites
        static void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            MachineConnector.SetRegisteredOI(Plugin.GUID, new Options());
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
            MouseDrag.Options.velocityDrag.Value = true;
            MachineConnector.SaveConfig(MachineConnector.GetRegisteredOI(MouseDrag.Plugin.GUID));
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
    }
}
