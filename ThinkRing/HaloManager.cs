namespace ThinkRing
{
    public static class HaloManager
    {
        public static OracleHalo oHalo;
        public static TempleGuardHalo gHalo;
        public static Options.ActivateTypes activeType = Options.ActivateTypes.Dragging;
        public static Options.ColorTypes colorType = Options.ColorTypes.CharacterDarker;
        public static Options.LightningTypes lightningType = Options.LightningTypes.RustyMachine;
        public static Options.HaloTypes haloType = Options.HaloTypes.Oracle;


        public static void Update(RainWorldGame game)
        {
            Creature creature = game?.FirstAlivePlayer?.realizedCreature;

            if (oHalo == null && gHalo == null) {
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

                if (haloType == Options.HaloTypes.Oracle) {
                    oHalo = new OracleHalo(head);
                    creature.room.AddObject(oHalo);
                }
                if (haloType == Options.HaloTypes.TempleGuard) {
                    gHalo = new TempleGuardHalo(head);
                    creature.room.AddObject(gHalo);
                }
            }

            if (oHalo != null || gHalo != null)
                if (MouseDrag.Drag.dragChunk != null && Options.blink.Value)
                    (creature as Player)?.Blink(30);

            if (oHalo != null) {
                oHalo.connectionPos = MouseDrag.Drag.dragChunk?.pos;
                oHalo.randomBoltPositions = MouseDrag.Drag.dragChunk?.owner == creature;
                oHalo.shortestDistFromHalo = MouseDrag.Drag.dragChunk?.owner == creature;

                if (oHalo.slatedForDeletetion || oHalo.room != creature?.room)
                    oHalo = null;
            }

            if (gHalo != null) {
                gHalo.connectionPos = MouseDrag.Drag.dragChunk?.pos;
                //TODO add lightningbolts?

                if (gHalo.slatedForDeletetion || gHalo.room != creature?.room)
                    gHalo = null;
            }
        }
    }
}
