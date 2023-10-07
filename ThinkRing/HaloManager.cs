namespace ThinkRing
{
    public static class HaloManager
    {
        public static BaseHalo halo;
        public static Options.ActivateTypes activeType = Options.ActivateTypes.Dragging;
        public static Options.ColorTypes colorType = Options.ColorTypes.CharacterDarker;
        public static Options.LightningTypes lightningType = Options.LightningTypes.RustyMachine;
        public static Options.HaloTypes haloType = Options.HaloTypes.Oracle;


        public static void Update(RainWorldGame game)
        {
            Creature creature = game?.FirstAlivePlayer?.realizedCreature;

            if (halo == null) {
                if (!(creature is Player) || creature.room == null || 
                    creature.slatedForDeletetion || creature.State?.alive == false)
                    return;

                //don't create new halo if not dragging
                if (activeType == Options.ActivateTypes.Dragging && MouseDrag.Drag.dragChunk == null)
                    return;

                //don't create new halo if mousedrag is not active
                if (activeType == Options.ActivateTypes.ToolsActive && !MouseDrag.State.activated)
                    return;

                var head = ((creature as Player).graphicsModule as PlayerGraphics)?.head;
                if (head == null)
                    return;

                if (haloType == Options.HaloTypes.Oracle)
                    halo = new OracleHalo(head);
                if (haloType == Options.HaloTypes.TempleGuard)
                    halo = new TempleGuardHalo(head);
                if (haloType == Options.HaloTypes.Ghost)
                    halo = new GhostHalo(head);
                if (haloType == Options.HaloTypes.Karma)
                    halo = new KarmaHalo(head);
                if (haloType == Options.HaloTypes.None)
                    halo = new NoneHalo(head); //allows lighning shards
                if (halo != null)
                    creature.room.AddObject(halo);
            }

            if (halo != null) {
                if (MouseDrag.Drag.dragChunk != null && Options.blink.Value)
                    (creature as Player)?.Blink(30);

                halo.connectionPos = MouseDrag.Drag.dragChunk?.pos;
                if (!Options.boltsHitYourself.Value) {
                    halo.randomBoltPositions = MouseDrag.Drag.dragChunk?.owner == creature;
                    halo.shortestDistFromHalo = MouseDrag.Drag.dragChunk?.owner == creature;
                }

                if (halo.slatedForDeletetion || halo.room != creature?.room)
                    halo = null;
            }
        }
    }
}
