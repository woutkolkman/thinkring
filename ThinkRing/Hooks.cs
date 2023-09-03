using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ThinkRing
{
    class Hooks
    {
        public static void Apply()
        {
            //initialize options & load sprites
            On.RainWorld.OnModsInit += RainWorldOnModsInitHook;

            //at tickrate
            On.RainWorldGame.Update += RainWorldGameUpdateHook;

            //at framerate
            On.RainWorldGame.RawUpdate += RainWorldGameRawUpdateHook;
        }


        //initialize options & load sprites
        static void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            MachineConnector.SetRegisteredOI(Plugin.GUID, new Options());
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
    }
}
