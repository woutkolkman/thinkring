namespace ThinkRing
{
    public static class HaloManager
    {
        public static Halo halo;
        public static Options.ActivateTypes activeType = Options.ActivateTypes.Dragging;


        public static void Update(RainWorldGame game)
        {
            Creature creature = game?.FirstAlivePlayer?.realizedCreature;

            if (halo == null) {
                if (!(creature is Player) || creature.room == null || creature.slatedForDeletetion)
                    return;

                //don't create new halo if not dragging
                if (activeType == Options.ActivateTypes.Dragging && MouseDrag.Drag.dragChunk == null)
                    return;

                var head = ((creature as Player).graphicsModule as PlayerGraphics)?.head;
                if (head == null)
                    return;

                halo = new Halo(head);
                creature.room.AddObject(halo);
                Plugin.Logger.LogDebug("Created new halo");
            }

            if (MouseDrag.Drag.dragChunk != null && halo != null)
                halo.connectionPos = MouseDrag.Drag.dragChunk.pos;

            if (halo == null)
                return;

            if (halo.slatedForDeletetion || halo.room != creature?.room) {
                halo = null;
                Plugin.Logger.LogDebug("Halo is destroyed");
                return;
            }
        }
    }
}
