using MouseDrag;

namespace ThinkRing
{
    public static class HaloManager
    {
        public static Halo halo;


        public static void Update(RainWorldGame game)
        {
            Creature creature = game?.FirstAlivePlayer?.realizedCreature;

            if (halo == null) {
                if (!(creature is Player) || creature.room == null || creature.slatedForDeletetion)
                    return;

                var head = ((creature as Player).graphicsModule as PlayerGraphics)?.head;
                if (head == null)
                    return;

                halo = new Halo(head);
                creature.room.AddObject(halo);
                Plugin.Logger.LogDebug("Created new halo");
                return;
            }
            if (halo.slatedForDeletetion || creature?.room != halo.owner?.owner?.owner?.room) {
                halo = null;
                Plugin.Logger.LogDebug("Halo is destroyed");
            }

            if (MouseDrag.Drag.dragChunk != null)
                halo.connectionPos = MouseDrag.Drag.dragChunk.pos;
        }
    }
}
