using MouseDrag;

namespace ThinkRing
{
    public static class HaloManager
    {
        public static Halo halo;
        public static Creature creature;


        public static void Update(RainWorldGame game)
        {
            if (halo == null) {
                creature = game?.FirstAlivePlayer?.realizedCreature;

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

            if (creature != game?.FirstAlivePlayer?.realizedCreature || creature.slatedForDeletetion) {
                halo.RemoveFromRoom();
                halo.Destroy();
                halo = null;
                Plugin.Logger.LogDebug("Destroyed halo");
                return;
            }

            if (halo.room != creature.room && creature.room != null) {
                halo.RemoveFromRoom();
                creature.room.AddObject(halo);
            }

            /*
            if (creature == null || creature.slatedForDeletetion || halo.room != creature.room) {
                halo.averageVoice--;
//                Plugin.Logger.LogDebug("halo.averageVoice: " + halo.averageVoice);
                if (halo.averageVoice <= 0f) {
                    halo.RemoveFromRoom();
                    halo.Destroy();
                    halo = null;
//                    Plugin.Logger.LogDebug("Deleted halo");
                }
            } else {
                if (halo.averageVoice < 20f)
                    halo.averageVoice++;
            }*/

            if (MouseDrag.Drag.dragChunk != null)
                halo.connectionPos = MouseDrag.Drag.dragChunk.pos;
        }
    }
}
