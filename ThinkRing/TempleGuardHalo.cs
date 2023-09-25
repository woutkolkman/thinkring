using UnityEngine;
using RWCustom;
using System.Collections.Generic;
using System;

namespace ThinkRing
{
    //basically a tweaked copy from the game (TempleGuardGraphics.Halo)
    public class TempleGuardHalo : BaseHalo
    {
        public int[][] glyphs = new int[4][];
        public float[,] lines = new float[40, 4];
        public float[,] smallCircles = new float[10, 5];
        public int circles = 7;
        public int firstSwapperSprite;
        public int firstLineSprite;
        public int firstSmallCircleSprite;
        public bool[][] dirtyGlyphs;
        public float[][,] glyphPositions;
        public GlyphSwapper[] swappers = new GlyphSwapper[3];
        public float[,] rotation;
        public float[,] rad;
        public float savDisruption;
        public float activity;
        public float slowRingsActive = 1f; //changed to 1f to gradually build halo roughly after flashAmount reached 0f
        public float lastSlowRingsActive;
        public int ringsActive = 2;
        public bool firstUpdate = true;
        public bool deactivated = false;
        public List<EntityID> reactedToCritters = new List<EntityID>();

        //added to original
        public float telekinesis = 0.3f;
        public float lastTelekin = 0.3f; //TODO track telekinesis
        public float stress = 0.5f;
        public float flashAmount = 1f; //start value when created
        public float haloLerpSpeed = 1f; //changed from 0.1f
        public float distAboveHead = 110f; //changed from 200f


        public TempleGuardHalo(GenericBodyPart owner) : base(owner)
        {
            this.firstSprite = 0;
            this.rad = new float[2, 3];
            this.rad[0, 0] = 0f;
            this.rad[0, 1] = 0f;
            this.rad[0, 2] = 0f;
            this.rad[1, 0] = 1f;
            this.rad[1, 1] = 1f;
            this.rad[1, 2] = 1f;
            this.dirtyGlyphs = new bool[this.glyphs.Length][];
            this.glyphPositions = new float[this.glyphs.Length][,];
            for (int i = 0; i < this.glyphs.Length; i++)
            {
                this.glyphs[i] = new int[(int)(this.CircumferenceAtCircle((float)(i * 2), 1f, 0f) / 15f)];
                this.dirtyGlyphs[i] = new bool[this.glyphs[i].Length];
                this.glyphPositions[i] = new float[this.glyphs[i].Length, 3];
                for (int j = 0; j < this.glyphs[i].Length; j++)
                    this.glyphs[i][j] = ((UnityEngine.Random.value < 0.033333335f) ? -1 : UnityEngine.Random.Range(0, 7));
            }
            this.rotation = new float[this.circles, 2];
            for (int k = 0; k < this.rotation.GetLength(0); k++)
            {
                this.rotation[k, 0] = UnityEngine.Random.value;
                this.rotation[k, 1] = this.rotation[k, 0];
            }
            this.totalSprites = this.circles;
            for (int l = 0; l < this.glyphs.Length; l++)
                this.totalSprites += this.glyphs[l].Length;
            this.firstSwapperSprite = firstSprite + this.totalSprites;
            for (int m = 0; m < this.swappers.Length; m++)
                this.swappers[m] = new GlyphSwapper(this);
            this.totalSprites += this.swappers.Length * 3;
            this.firstLineSprite = this.totalSprites;
            for (int n = 0; n < this.lines.GetLength(0); n++)
            {
                this.lines[n, 0] = UnityEngine.Random.value;
                this.lines[n, 1] = this.lines[n, 0];
                this.lines[n, 2] = (float)UnityEngine.Random.Range(0, 3);
                this.lines[n, 3] = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
            }
            this.totalSprites += this.lines.GetLength(0);
            this.firstSmallCircleSprite = this.totalSprites;
            for (int num = 0; num < this.smallCircles.GetLength(0); num++)
            {
                this.smallCircles[num, 0] = UnityEngine.Random.value;
                this.smallCircles[num, 1] = this.smallCircles[num, 0];
                this.smallCircles[num, 2] = (float)UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 6));
                this.smallCircles[num, 3] = (float)UnityEngine.Random.Range((int)this.smallCircles[num, 2] + 1, 7);
                this.smallCircles[num, 4] = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
            }
            this.totalSprites += this.smallCircles.GetLength(0);
        }


        public override void Update(bool eu)
        {
            //destroy and return if owner is deleted or moves to another room
            if (owner?.owner?.owner?.slatedForDeletetion != false || 
                this.room != owner.owner?.owner?.room || 
                (HaloManager.activeType == Options.ActivateTypes.Dragging && connectionPos == null) || //remove halo when not dragging
                (HaloManager.activeType == Options.ActivateTypes.ToolsActive && !MouseDrag.State.activated)) //remove halo when mousedrag is not active
            {
                ringsActive = 2;
                if (slowRingsActive <= 2f && flashAmount < 1f)
                    flashAmount += 1f / 10f;
                if (flashAmount >= 1f) {
                    this.Destroy();
                    this.deactivated = true;
                    this.room?.RemoveObject(this);
                    this.RemoveFromRoom();
                    return;
                }
            } else {
                ringsActive = Options.maxRings.Value; //final amount of rings active
                if (flashAmount > 0f)
                    flashAmount -= 1f / 10f;
            }
            flashAmount = Mathf.Clamp(flashAmount, 0f, 1f); //keep flashAmount a value from 0f to 1f

            this.lastPos = this.pos;
            Vector2 vector = (owner.owner.owner as Creature).mainBodyChunk.pos;
            if (Options.templeGuardHaloOffset.Value) {
                Vector2 headDir = new Vector2(0f, -1f);
                //Vector2 headDir = Custom.DirVec(owner.pos, (owner.owner.owner as Creature).mainBodyChunk.pos);
                vector = (owner.owner.owner as Creature).mainBodyChunk.pos - headDir * Mathf.Lerp(distAboveHead, this.RadAtCircle(2f + this.slowRingsActive * 2f, 1f, 0f), 0.5f);
                //Vector2 vector = this.owner.guard.mainBodyChunk.pos - this.owner.guard.StoneDir * Mathf.Lerp(200f, this.RadAtCircle(2f + this.slowRingsActive * 2f, 1f, 0f), 0.5f);
            }
            this.pos += Vector2.ClampMagnitude(vector - this.pos, 10f);
            this.pos = Vector2.Lerp(this.pos, vector, haloLerpSpeed);
            if (this.firstUpdate) {
                this.pos = vector;
                this.lastPos = this.pos;
                this.firstUpdate = false;
            }
            radius = RadAtCircle(ringsActive, 0f, 0f); //"radius" is unused for TempleGuardHalo

            for (int j = 0; j < this.swappers.Length; j++) //moved before base.Update so swappers can read connectionPos
                this.swappers[j].Update();

            base.Update(eu); //lightning bolts and color cycle

            if ((owner.owner.owner as Creature).dead)
                this.deactivated = true;

            if (this.activity > stress) {
                this.activity = Mathf.Max(stress - 0.0033333334f, stress);
            } else {
                this.activity = stress;
            }
            /*if (UnityEngine.Random.value < 0.01f)
                this.ringsActive = Custom.IntClamp((int)Mathf.Lerp(2f, 9f, Mathf.Pow(stress, 0.5f)), 2, 4);*/
            this.lastSlowRingsActive = this.slowRingsActive;
            if (this.slowRingsActive < (float)this.ringsActive) {
                this.slowRingsActive = Mathf.Min((float)this.ringsActive, this.slowRingsActive + 0.1f);
            } else {
                this.slowRingsActive = Mathf.Max((float)this.ringsActive, this.slowRingsActive - 0.05f);
            }
            this.savDisruption = Mathf.InverseLerp(10f, 150f, Vector2.Distance(this.pos, vector));
            for (int i = 0; i < this.rotation.GetLength(0); i++)
            {
                this.rotation[i, 1] = this.rotation[i, 0];
                this.rotation[i, 0] += 0.2f / Mathf.Max(1f, this.CircumferenceAtCircle((float)i, 1f, this.savDisruption)) 
                    * ((i % 2 == 0) ? -1f : 1f) * Mathf.Lerp(this.Speed, 3f, telekinesis);
            }
            for (int k = 0; k < this.lines.GetLength(0); k++)
            {
                this.lines[k, 1] = this.lines[k, 0];
                this.lines[k, 0] += 0.008333334f * this.lines[k, 3] * this.Speed;
            }
            for (int l = 0; l < this.smallCircles.GetLength(0); l++)
            {
                this.smallCircles[l, 1] = this.smallCircles[l, 0];
                this.smallCircles[l, 0] += 0.004166667f * this.smallCircles[l, 4] * this.Speed;
            }
            for (int m = 0; m < this.glyphs.Length; m++)
            {
                for (int n = 0; n < this.glyphs[m].Length; n++)
                {
                    this.glyphPositions[m][n, 1] = this.glyphPositions[m][n, 0];
                    if (UnityEngine.Random.value < this.Speed / 160f)
                    {
                        if (UnityEngine.Random.value < 0.033333335f && this.glyphPositions[m][n, 0] == 0f && this.glyphs[m][n] > -1)
                        {
                            if (m == this.glyphs.Length - 1) {
                                this.glyphPositions[m][n, 0] = -1f;
                            } else if (m == this.glyphs.Length - 2 && this.ringsActive == 4) {
                                this.glyphPositions[m][n, 0] = -3f;
                            }
                        }
                        else
                        {
                            this.glyphPositions[m][n, 0] = ((UnityEngine.Random.value < 0.05f) ? 1f : 0f);
                        }
                    }
                    if (this.glyphPositions[m][n, 0] == 1f && this.glyphs[m][n] == -1)
                    {
                        this.glyphs[m][n] = UnityEngine.Random.Range(0, 7);
                        this.dirtyGlyphs[m][n] = true;
                    }
                    if (this.glyphPositions[m][n, 2] > 0f && this.glyphs[m][n] > -1)
                    {
                        this.glyphPositions[m][n, 2] -= 0.05f;
                        this.glyphs[m][n] = UnityEngine.Random.Range(0, 7);
                        this.dirtyGlyphs[m][n] = true;
                    }
                }
            }
            for (int num = 0; num < this.smallCircles.GetLength(0); num++)
            {
                if (UnityEngine.Random.value < this.Speed / 120f && this.smallCircles[num, 3] < (float)(this.ringsActive * 2))
                {
                    float num2 = this.RadAtCircle(this.smallCircles[num, 2] - 0.5f, 1f, this.savDisruption);
                    float num3 = this.RadAtCircle(this.smallCircles[num, 3] - 0.5f, 1f, this.savDisruption);
                    Vector2 p = Custom.DegToVec(this.smallCircles[num, 0] * 360f) * Mathf.Lerp(num2, num3, 0.5f);
                    for (int num4 = 0; num4 < this.glyphs.Length; num4++)
                        for (int num5 = 0; num5 < this.glyphs[num4].Length; num5++)
                            if (Custom.DistLess(p, this.GlyphPos(num4, num5, 1f), (num3 - num2) / 2f))
                                this.glyphPositions[num4][num5, 2] = 1f;
                }
            }
            int num6 = 0;
            for (int num7 = 0; num7 < this.glyphs[0].Length; num7++)
                if (this.glyphPositions[0][num7, 0] == 1f)
                    num6++;
            if (num6 > 1)
                for (int num8 = 0; num8 < this.glyphs[0].Length; num8++)
                    this.glyphPositions[0][num8, 0] = 0f;
            for (int num9 = 0; num9 < 2; num9++)
            {
                this.rad[num9, 1] = this.rad[num9, 0];
                if (this.rad[num9, 0] < this.rad[num9, 2]) {
                    this.rad[num9, 0] = Mathf.Min(this.rad[num9, 2], this.rad[num9, 0] + ((num9 == 0) ? 0.15f : 0.0035714286f));
                } else {
                    this.rad[num9, 0] = Mathf.Max(this.rad[num9, 2], this.rad[num9, 0] - ((num9 == 0) ? 0.15f : 0.0035714286f));
                }
                this.rad[num9, 0] = Mathf.Lerp(this.rad[num9, 0], this.rad[num9, 2], 0.01f);
            }
            if (UnityEngine.Random.value < this.Speed / 120f)
            {
                this.rad[0, 2] = ((UnityEngine.Random.value > this.activity) ? 0f : ((float)UnityEngine.Random.Range(-1, 3) * 20f));
                this.rad[1, 2] = ((UnityEngine.Random.value < 1f / Mathf.Lerp(1f, 5f, this.activity)) ? 1f : Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value));
            }
        }


        public float Circumference(float rad)
        {
            return 2f * rad * 3.1415927f;
        }


        public float RadAtCircle(float circle, float timeStacker, float disruption)
        {
            return ((circle + 1f) * 20f + Mathf.Lerp(this.rad[0, 1], this.rad[0, 0], timeStacker) * (1f - Mathf.Lerp(lastTelekin, telekinesis, timeStacker))) 
                * Mathf.Lerp(Mathf.Lerp(this.rad[1, 1], this.rad[1, 0], timeStacker), 0.7f, Mathf.Lerp(lastTelekin, telekinesis, timeStacker)) 
                * Mathf.Lerp(1f, UnityEngine.Random.value * disruption, Mathf.Pow(disruption, 2f));
        }


        public float CircumferenceAtCircle(float circle, float timeStacker, float disruption)
        {
            return this.Circumference(this.RadAtCircle(circle, timeStacker, disruption));
        }


        public float Speed
        {
            get
            {
                float b = 1.8f;
                /*if (this.owner.guard.AI.focusCreature != null && this.owner.guard.AI.FocusCreatureMovingTowardsProtectExit 
                    && this.owner.guard.AI.focusCreature.VisualContact && this.owner.guard.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    b = Custom.LerpMap(Vector2.Distance(this.owner.guard.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.lastPos, 
                        this.owner.guard.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos), 1.5f, 5f, 1.2f, 3f);
                }*/
                return Mathf.Lerp(0.2f, b, this.activity);
            }
        }


        public void ReactToCreature(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
        {
            if (false /*Mathf.Abs(this.owner.guard.mainBodyChunk.pos.x - this.owner.guard.room.MiddleOfTile(creatureRep.BestGuessForPosition()).x) < 300f*/ 
                && !this.reactedToCritters.Contains(creatureRep.representedCreature.ID))
            {
                this.ringsActive = Math.Max(this.ringsActive, UnityEngine.Random.Range(3, 5));
                this.rad[0, 2] = ((UnityEngine.Random.value > this.activity) ? 0f : ((float)UnityEngine.Random.Range(-1, 3) * 20f));
                this.rad[1, 2] = ((UnityEngine.Random.value < 1f / Mathf.Lerp(1f, 5f, this.activity)) ? 1f : Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value));
                this.reactedToCritters.Add(creatureRep.representedCreature.ID);
                for (int i = 0; i < (int)Custom.LerpMap(creatureRep.representedCreature.realizedCreature.TotalMass, 0.2f, 2f, 4f, 100f); i++)
                {
                    int num = UnityEngine.Random.Range(0, this.glyphs.Length);
                    int num2 = UnityEngine.Random.Range(0, this.glyphs[num].Length);
                    this.glyphs[num][num2] = -1;
                    this.dirtyGlyphs[num][num2] = true;
                }
                this.activity = Mathf.Min(1f, this.activity + 0.2f);
                return;
            }
            for (int j = 0; j < (int)Custom.LerpMap(creatureRep.representedCreature.realizedCreature.TotalMass, 0.2f, 2f, 2f, (float)(11 * this.ringsActive)); j++)
            {
                int num3 = UnityEngine.Random.Range(0, this.ringsActive);
                int num4 = UnityEngine.Random.Range(0, this.glyphs[num3].Length);
                this.glyphs[num3][num4] = UnityEngine.Random.Range(0, 7);
                this.dirtyGlyphs[num3][num4] = true;
                if (UnityEngine.Random.value < 0.5f)
                    this.glyphPositions[num3][num4, 2] = 1f;
            }
        }


        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[this.totalSprites]; //added initializer, because templeguard sprite leaser does not exist
            for (int i = 0; i < this.circles; i++)
            {
                sLeaser.sprites[this.firstSprite + i] = new FSprite("Futile_White", true);
                sLeaser.sprites[this.firstSprite + i].color = color;
                sLeaser.sprites[this.firstSprite + i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
            }
            int num = this.circles;
            for (int j = 0; j < this.glyphs.Length; j++)
            {
                for (int k = 0; k < this.glyphs[j].Length; k++)
                {
                    sLeaser.sprites[this.firstSprite + num] = new FSprite("haloGlyph" + this.glyphs[j][k].ToString(), true);
                    sLeaser.sprites[this.firstSprite + num].color = color;
                    num++;
                }
            }
            for (int l = 0; l < this.swappers.Length; l++)
                this.swappers[l].InitiateSprites(this.firstSwapperSprite + l * 3, sLeaser, rCam);
            for (int m = 0; m < this.lines.GetLength(0); m++)
            {
                sLeaser.sprites[this.firstSprite + this.firstLineSprite + m] = new FSprite("pixel", true);
                sLeaser.sprites[this.firstSprite + this.firstLineSprite + m].color = color;
            }
            for (int n = 0; n < this.smallCircles.GetLength(0); n++)
            {
                sLeaser.sprites[this.firstSprite + this.firstSmallCircleSprite + n] = new FSprite("Futile_White", true);
                sLeaser.sprites[this.firstSprite + this.firstSmallCircleSprite + n].color = color;
                sLeaser.sprites[this.firstSprite + this.firstSmallCircleSprite + n].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
            }
            this.AddToContainer(sLeaser, rCam, null); //added
        }


        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            Vector2 headPos = owner.pos; //replaced parameter with fixed value
            Vector2 headDir = new Vector2(); //replaced parameter with fixed value
            if (Options.templeGuardHaloOffset.Value && (owner.owner?.owner as Creature)?.mainBodyChunk != null)
                headDir = new Vector2(0f, -1f);
                //headDir = Custom.DirVec(owner.pos, (owner.owner.owner as Creature).mainBodyChunk.pos);
            headPos -= new Vector2(0f, 200f) * flashAmount; //makes halo flash

            //set color of all sprites
            Color curColor = Color.Lerp(prevColor, color, timeStacker);
            for (int i = firstSprite; i < firstSprite + totalSprites; i++)
                sLeaser.sprites[i].color = curColor;

            Vector2 vector = Vector2.Lerp(this.lastPos, this.pos, timeStacker);
            float num = Mathf.InverseLerp(10f, 150f, Vector2.Distance(vector, headPos - headDir 
                * Mathf.Lerp(distAboveHead, this.RadAtCircle(2f + this.slowRingsActive * 2f, timeStacker, 0f), 0.5f)));
            int num2 = Custom.IntClamp((int)(Mathf.Lerp(this.lastSlowRingsActive, this.slowRingsActive, timeStacker) 
                + Mathf.Lerp(-0.4f, 0.4f, UnityEngine.Random.value) * Mathf.InverseLerp(0.01f, 0.1f, Mathf.Abs(this.lastSlowRingsActive - this.slowRingsActive))), 2, 4);
            if (UnityEngine.Random.value < num || this.deactivated)
            {
                for (int i = this.firstSprite; i < this.firstSprite + this.totalSprites; i++)
                    sLeaser.sprites[i].isVisible = false;
                return;
            }
            for (int j = this.firstSprite; j < this.firstSprite + this.totalSprites; j++)
                sLeaser.sprites[j].isVisible = true;
            for (int k = 0; k < this.circles; k++)
            {
                sLeaser.sprites[this.firstSprite + k].x = vector.x - camPos.x;
                sLeaser.sprites[this.firstSprite + k].y = vector.y - camPos.y;
                float num3 = this.RadAtCircle((float)k - 0.5f, timeStacker, num);
                sLeaser.sprites[this.firstSprite + k].scale = num3 / 8f;
                sLeaser.sprites[this.firstSprite + k].alpha = 1f / num3;
                sLeaser.sprites[this.firstSprite + k].isVisible = (k < num2 * 2);
            }
            int num4 = this.circles;
            for (int l = 0; l < this.glyphs.Length; l++)
            {
                for (int m = 0; m < this.glyphs[l].Length; m++)
                {
                    Vector2 vector2 = vector + this.GlyphPos(l, m, timeStacker);
                    sLeaser.sprites[this.firstSprite + num4].x = vector2.x - camPos.x;
                    sLeaser.sprites[this.firstSprite + num4].y = vector2.y - camPos.y;
                    if (this.dirtyGlyphs[l][m])
                    {
                        sLeaser.sprites[this.firstSprite + num4].element = Futile.atlasManager.GetElementWithName("haloGlyph" + this.glyphs[l][m].ToString());
                        this.dirtyGlyphs[l][m] = false;
                    }
                    sLeaser.sprites[this.firstSprite + num4].isVisible = (UnityEngine.Random.value > num && l < num2);
                    if (this.glyphs[l][m] == -1 || (l == 0 && this.glyphPositions[l][m, 0] == 1f))
                    {
                        sLeaser.sprites[this.firstSprite + num4].rotation = 0f;
                    }
                    else
                    {
                        sLeaser.sprites[this.firstSprite + num4].rotation = ((float)m / (float)this.glyphs[l].Length 
                            + Mathf.Lerp(this.rotation[l, 1], this.rotation[l, 0], timeStacker)) * 360f;
                    }
                    num4++;
                }
            }
            for (int n = 0; n < this.swappers.Length; n++)
                this.swappers[n].DrawSprites(this.firstSwapperSprite + n * 3, sLeaser, rCam, timeStacker, camPos, vector);
            for (int num5 = 0; num5 < this.lines.GetLength(0); num5++)
            {
                float num6 = Mathf.Lerp(this.lines[num5, 1], this.lines[num5, 0], timeStacker);
                Vector2 vector3 = Custom.DegToVec(num6 * 360f) * this.RadAtCircle(this.lines[num5, 2] * 2f + 1f, timeStacker, num) + vector;
                sLeaser.sprites[this.firstSprite + this.firstLineSprite + num5].isVisible = (this.lines[num5, 2] < (float)(num2 - 1));
                if (UnityEngine.Random.value > num || UnityEngine.Random.value > 0.25f)
                {
                    sLeaser.sprites[this.firstSprite + this.firstLineSprite + num5].rotation = num6 * 360f;
                    sLeaser.sprites[this.firstSprite + this.firstLineSprite + num5].scaleY = this.RadAtCircle(this.lines[num5, 2] - 0.5f, timeStacker, num) 
                        - this.RadAtCircle(this.lines[num5, 2] + 0.5f, timeStacker, num);
                }
                else
                {
                    vector3 = Vector2.Lerp(vector3, headPos, 0.4f);
                    sLeaser.sprites[this.firstSprite + this.firstLineSprite + num5].rotation = Custom.AimFromOneVectorToAnother(vector3, headPos);
                    sLeaser.sprites[this.firstSprite + this.firstLineSprite + num5].scaleY = Vector2.Distance(vector3, headPos) * 1.5f * UnityEngine.Random.value;
                }
                sLeaser.sprites[this.firstSprite + this.firstLineSprite + num5].x = vector3.x - camPos.x;
                sLeaser.sprites[this.firstSprite + this.firstLineSprite + num5].y = vector3.y - camPos.y;
            }
            for (int num7 = 0; num7 < this.smallCircles.GetLength(0); num7++)
            {
                float num8 = Mathf.Lerp(this.smallCircles[num7, 1], this.smallCircles[num7, 0], timeStacker);
                float num9 = this.RadAtCircle(this.smallCircles[num7, 2] - 0.5f, timeStacker, num);
                float num10 = this.RadAtCircle(this.smallCircles[num7, 3] - 0.5f, timeStacker, num);
                sLeaser.sprites[this.firstSprite + this.firstSmallCircleSprite + num7].isVisible = (this.smallCircles[num7, 3] < (float)(num2 * 2));
                Vector2 vector4 = Custom.DegToVec(num8 * 360f) * Mathf.Lerp(num9, num10, 0.5f) + vector;
                sLeaser.sprites[this.firstSprite + this.firstSmallCircleSprite + num7].x = vector4.x - camPos.x;
                sLeaser.sprites[this.firstSprite + this.firstSmallCircleSprite + num7].y = vector4.y - camPos.y;
                sLeaser.sprites[this.firstSprite + this.firstSmallCircleSprite + num7].scale = (num10 - num9) / 16f;
                sLeaser.sprites[this.firstSprite + this.firstSmallCircleSprite + num7].alpha = 2f / (num10 - num9);
            }
        }


        public Vector2 GlyphPos(int circle, int glyph, float timeStacker)
        {
            //added to hide a connection position for a cursor behind the grabbed object
            if (HaloManager.lightningType == Options.LightningTypes.TempleGuard && circle == 0 && glyph == 0 && connectionPos != null)
                return connectionPos.Value - this.pos;

            if ((float)circle * 2f - Mathf.Lerp(this.glyphPositions[circle][glyph, 1], this.glyphPositions[circle][glyph, 0], timeStacker) < 0f)
                return new Vector2(0f, 0f);
            float num = Mathf.Lerp(this.rotation[circle, 1], this.rotation[circle, 0], timeStacker);
            return Custom.DegToVec(((float)glyph / (float)this.glyphs[circle].Length + num) * 360f) 
                * this.RadAtCircle((float)circle * 2f - Mathf.Lerp(this.glyphPositions[circle][glyph, 1], 
                this.glyphPositions[circle][glyph, 0], timeStacker), timeStacker, this.savDisruption);
        }


        public class GlyphSwapper
        {
            public TempleGuardHalo halo;
            public Cursor[] cursors;
            public int counter;
            public int switchAt;


            public GlyphSwapper(TempleGuardHalo halo)
            {
                this.halo = halo;
                this.cursors = new Cursor[2];
                for (int i = 0; i < this.cursors.Length; i++)
                    this.cursors[i] = new Cursor(this, i);
            }


            public void Update()
            {
                if (this.counter > 0)
                    this.counter--;
                if (this.counter == this.switchAt)
                {
                    int num = this.halo.glyphs[this.cursors[0].pos.x][this.cursors[0].pos.y];
                    int num2 = this.halo.glyphs[this.cursors[1].pos.x][this.cursors[1].pos.y];
                    if (num == -1 && num2 == -1) {
                        num = UnityEngine.Random.Range(0, 7);
                        num2 = UnityEngine.Random.Range(0, 7);
                    } else if (num == num2) {
                        num = -1;
                        num2 = -1;
                    } else if (num == -1) {
                        num = num2;
                    } else if (num2 == -1) {
                        num2 = num;
                    }
                    this.halo.glyphs[this.cursors[0].pos.x][this.cursors[0].pos.y] = num2;
                    this.halo.glyphs[this.cursors[1].pos.x][this.cursors[1].pos.y] = num;
                    this.halo.dirtyGlyphs[this.cursors[0].pos.x][this.cursors[0].pos.y] = true;
                    this.halo.dirtyGlyphs[this.cursors[1].pos.x][this.cursors[1].pos.y] = true;
                }
                for (int i = 0; i < this.cursors.Length; i++)
                    this.cursors[i].Update();
            }


            public void InitiateSprites(int frst, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites[frst + 2] = new FSprite("pixel", true);
                sLeaser.sprites[frst + 2].color = halo.color;
                sLeaser.sprites[frst + 2].anchorY = 0f;
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[frst + i] = new FSprite("Futile_White", true);
                    sLeaser.sprites[frst + i].scale = 1.25f;
                    sLeaser.sprites[frst + i].color = halo.color;
                    sLeaser.sprites[frst + i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
                    sLeaser.sprites[frst + i].alpha = 0.1f;
                }
            }


            public void DrawSprites(int frst, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 haloPos)
            {
                Vector2 vector = this.cursors[0].CursorPos(timeStacker) + haloPos;
                Vector2 vector2 = this.cursors[1].CursorPos(timeStacker) + haloPos;
                sLeaser.sprites[frst].x = vector.x - camPos.x;
                sLeaser.sprites[frst].y = vector.y - camPos.y;
                sLeaser.sprites[frst + 1].x = vector2.x - camPos.x;
                sLeaser.sprites[frst + 1].y = vector2.y - camPos.y;
                sLeaser.sprites[frst + 2].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
                sLeaser.sprites[frst + 2].scaleY = Vector2.Distance(vector, vector2) - 20f;
                vector += Custom.DirVec(vector, vector2) * 10f;
                sLeaser.sprites[frst + 2].x = vector.x - camPos.x;
                sLeaser.sprites[frst + 2].y = vector.y - camPos.y;
            }


            public class Cursor
            {
                public GlyphSwapper owner;
                public IntVector2 pos;
                public IntVector2 nextPos;
                public float prog;
                public float lastProg;
                public int num;


                public Cursor(GlyphSwapper owner, int num)
                {
                    this.owner = owner;
                    this.num = num;
                    this.pos = this.RandomGlyphPos();
                    this.nextPos = this.pos;
                    this.prog = 1f;
                    this.lastProg = 1f;
                }


                public void Update()
                {
                    this.lastProg = this.prog;
                    if (this.nextPos == this.pos) {
                        if (UnityEngine.Random.value < this.owner.halo.Speed / 10f && this.owner.halo.glyphPositions[this.pos.x][this.pos.y, 0] == 1f && this.pos.x > 0)
                        {
                            this.owner.halo.glyphs[this.pos.x][this.pos.y] = UnityEngine.Random.Range(0, 7);
                            this.owner.halo.dirtyGlyphs[this.pos.x][this.pos.y] = true;
                        }
                        if ((this.owner.counter == 0 && UnityEngine.Random.value < this.owner.halo.Speed / 40f 
                            && this.owner.cursors[1 - this.num].prog == 1f) || this.pos.x >= this.owner.halo.ringsActive)
                        {
                            this.nextPos = this.RandomGlyphPos();
                            this.lastProg = 0f;
                            this.prog = 0f;
                            return;
                        }
                    } else {
                        this.prog += 5f * Mathf.Lerp(this.owner.halo.Speed, 1f, 0.7f) / Mathf.Max(1f, Vector2.Distance(this.owner.halo.GlyphPos(this.pos.x, this.pos.y, 1f), 
                            this.owner.halo.GlyphPos(this.nextPos.x, this.nextPos.y, 1f)));
                        if (this.prog >= 1f)
                        {
                            this.pos = this.nextPos;
                            this.prog = 1f;
                            this.owner.counter = (int)(Mathf.Lerp(10f, 70f, UnityEngine.Random.value) / this.owner.halo.Speed);
                            this.owner.switchAt = this.owner.counter / 2;
                            if (UnityEngine.Random.value < 0.5f && this.owner.halo.glyphs[this.pos.x][this.pos.y] > -1 && this.pos.y > 0)
                                this.owner.halo.glyphPositions[this.pos.x][this.pos.y, 0] = 1f - this.owner.halo.glyphPositions[this.pos.x][this.pos.y, 0];
                        }
                    }
                }


                public Vector2 CursorPos(float timeStacker)
                {
                    Vector2 a = Vector2.Lerp(this.owner.halo.GlyphPos(this.pos.x, this.pos.y, timeStacker), 
                        this.owner.halo.GlyphPos(this.nextPos.x, this.nextPos.y, timeStacker), Mathf.Lerp(this.lastProg, this.prog, timeStacker));
                    Vector2 b = Vector3.Slerp(this.owner.halo.GlyphPos(this.pos.x, this.pos.y, timeStacker), 
                        this.owner.halo.GlyphPos(this.nextPos.x, this.nextPos.y, timeStacker), Mathf.Lerp(this.lastProg, this.prog, timeStacker));
                    return Vector2.Lerp(a, b, 0.5f);
                }


                public IntVector2 RandomGlyphPos()
                {
                    IntVector2 intVector = new IntVector2(0, 0);

                    //added force first glyph
                    if (HaloManager.lightningType == Options.LightningTypes.TempleGuard && owner.halo.connectionPos != null && num == 0)
                        return intVector;

                    intVector.x = UnityEngine.Random.Range(0, this.owner.halo.ringsActive);
                    intVector.y = UnityEngine.Random.Range(0, this.owner.halo.glyphs[intVector.x].Length);
                    return intVector;
                }
            }
        }
    }
}
