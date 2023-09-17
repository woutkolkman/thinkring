namespace ThinkRing
{
    public static class HaloManager
    {
        public static Halo halo;
        public static Options.ActivateTypes activeType = Options.ActivateTypes.Dragging;
        public static Options.ColorTypes colorType = Options.ColorTypes.Static;
        public static Options.LightningTypes lightningType = Options.LightningTypes.RustyMachine;


        public static void Update(RainWorldGame game)
        {
            Creature creature = game?.FirstAlivePlayer?.realizedCreature;

            if (halo == null) {
                if (!(creature is Player) || creature.room == null || creature.slatedForDeletetion)
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

                halo = new Halo(head);
                creature.room.AddObject(halo);
            }

            if (MouseDrag.Drag.dragChunk != null) {
                halo.connectionPos = MouseDrag.Drag.dragChunk.pos;
                if (Options.blink.Value)
                    (creature as Player)?.Blink(30);
            }
            halo.randomBoltPositions = MouseDrag.Drag.dragChunk?.owner == creature;
            halo.shortestDistFromHalo = MouseDrag.Drag.dragChunk?.owner == creature;

            if (halo.slatedForDeletetion || halo.room != creature?.room) {
                halo = null;
            }
        }
    }
}
