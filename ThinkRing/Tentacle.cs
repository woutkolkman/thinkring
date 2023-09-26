using UnityEngine;
using RWCustom;

namespace ThinkRing
{
    public class Tentacle
    {
        public Vector2[,] segments;
        public float conRad;
        public Vector2? posB;
        public Vector2? rootDirB;
        public int startSprite;
        public PlayerGraphics pGraphics;
        public Vector2 wind;
        public float lengthFactor;
        public float length;
        public int activeUpdateTime;


        public Tentacle(PlayerGraphics pGraphics, float length, Vector2? posB)
        {
            this.startSprite = 0;
            this.pGraphics = pGraphics;
            this.posB = posB;
            Random.State state = Random.state;
            Random.InitState((int)length);
            if (posB != null)
            {
                IntVector2 tilePosition = pGraphics.player.room.GetTilePosition(posB.Value);
                for (int i = 0; i < 4; i++)
                {
                    if (pGraphics.player.room.GetTile(tilePosition + Custom.fourDirections[i]).Solid)
                    {
                        this.rootDirB = new Vector2?(-Custom.fourDirections[i].ToVector2());
                        if (Custom.fourDirections[i].x == 0) {
                            posB = new Vector2?(new Vector2(posB.Value.x, pGraphics.player.room.MiddleOfTile(tilePosition).y - this.rootDirB.Value.y * 20f));
                        } else {
                            posB = new Vector2?(new Vector2(pGraphics.player.room.MiddleOfTile(tilePosition).x - this.rootDirB.Value.x * 20f, posB.Value.y));
                        }
                    }
                }
            }
            this.segments = new Vector2[(int)Mathf.Clamp(length / 20f, 1f, 200f), 3];
            for (int j = 0; j < this.segments.GetLength(0); j++)
            {
                float num = (float)j / (float)(this.segments.GetLength(0) - 1);
                if (posB != null)
                    this.segments[j, 0] = posB.Value;
                this.segments[j, 1] = this.segments[j, 0];
                this.segments[j, 2] = Custom.RNV() * Random.value;
            }
            this.length = length;
            this.conRad = length / (float)this.segments.GetLength(0) * 1.5f;
            Random.state = state;
        }


        public void ActiveUpdate()
        {
            this.lengthFactor = Mathf.Lerp(this.lengthFactor, 1f, 0.01f);
            this.activeUpdateTime++;
            if (this.activeUpdateTime == 1)
            {
                for (int i = 1; i < this.segments.GetLength(0); i++)
                {
                    float num = (float)i / (float)(this.segments.GetLength(0) - 1);
                    this.segments[i, 0] = this.segments[0, 0];
                    this.segments[i, 1] = this.segments[0, 1];
                }
            }
        }


        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
                newContatiner = rCam.ReturnFContainer("Midground");
            sLeaser.sprites[this.startSprite].RemoveFromContainer();
            newContatiner.AddChild(sLeaser.sprites[this.startSprite]);
            sLeaser.sprites[this.startSprite + 1].RemoveFromContainer();
            newContatiner.AddChild(sLeaser.sprites[this.startSprite + 1]);
        }


        public void Connect(int A, int B)
        {
            Vector2 normalized = (this.segments[A, 0] - this.segments[B, 0]).normalized;
            float num = Vector2.Distance(this.segments[A, 0], this.segments[B, 0]);
            float d = Mathf.InverseLerp(0f, this.conRad, num);
            this.segments[A, 0] += normalized * (this.conRad - num) * 0.5f * d;
            this.segments[A, 2] += normalized * (this.conRad - num) * 0.5f * d;
            this.segments[B, 0] -= normalized * (this.conRad - num) * 0.5f * d;
            this.segments[B, 2] -= normalized * (this.conRad - num) * 0.5f * d;
        }


        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            (sLeaser.sprites[this.startSprite] as TriangleMesh).isVisible = true;
            (sLeaser.sprites[this.startSprite + 1] as TriangleMesh).isVisible = true;
            Vector2 a = Vector2.Lerp(this.segments[0, 1], this.segments[0, 0], timeStacker);
            for (int i = 0; i < this.segments.GetLength(0); i++)
            {
                Vector2 vector = Vector2.Lerp(this.segments[i, 1], this.segments[i, 0], timeStacker);
                Vector2 normalized = (a - vector).normalized;
                Vector2 vector2 = Custom.PerpendicularVector(normalized);
                float d = Vector2.Distance(a, vector) / 5f;
                float d2 = this.Rad((float)i / (float)(this.segments.GetLength(0) - 1));
                if (i != 0) {
                    (sLeaser.sprites[this.startSprite] as TriangleMesh).MoveVertice(i * 4, a - normalized * d - vector2 * 1.5f * d2 - camPos);
                    (sLeaser.sprites[this.startSprite] as TriangleMesh).MoveVertice(i * 4 + 1, a - normalized * d + vector2 * 1.5f * d2 - camPos);
                } else {
                    (sLeaser.sprites[this.startSprite] as TriangleMesh).MoveVertice(i * 4, a - vector2 - camPos);
                    (sLeaser.sprites[this.startSprite] as TriangleMesh).MoveVertice(i * 4 + 1, a + vector2 - camPos);
                }
                if (i != this.segments.GetLength(0) - 1) {
                    (sLeaser.sprites[this.startSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector + normalized * d - vector2 * 3.5f * d2 - camPos);
                    (sLeaser.sprites[this.startSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector + normalized * d + vector2 * 3.5f * d2 - camPos);
                } else {
                    (sLeaser.sprites[this.startSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector - vector2 - camPos);
                    (sLeaser.sprites[this.startSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector + vector2 - camPos);
                }
                for (int j = 0; j < 4; j++)
                    (sLeaser.sprites[this.startSprite + 1] as TriangleMesh).MoveVertice(i * 4 + j, (sLeaser.sprites[this.startSprite] as TriangleMesh).vertices[i * 4 + j]);
                a = vector;
            }
        }


        public void InactiveDrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            (sLeaser.sprites[this.startSprite] as TriangleMesh).isVisible = false;
            (sLeaser.sprites[this.startSprite + 1] as TriangleMesh).isVisible = false;
        }


        public void InactiveUpdate()
        {
            if (this.lengthFactor <= 0.01f)
            {
                this.lengthFactor = 0f;
                return;
            }
            this.lengthFactor = Mathf.Lerp(this.lengthFactor, 0f, 0.01f);
        }


        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites[this.startSprite] = TriangleMesh.MakeLongMesh(this.segments.GetLength(0), false, true);
            sLeaser.sprites[this.startSprite + 1] = TriangleMesh.MakeLongMesh(this.segments.GetLength(0), false, false);
            sLeaser.sprites[this.startSprite + 1].shader = rCam.room.game.rainWorld.Shaders["GhostSkin"];
            sLeaser.sprites[this.startSprite + 1].alpha = 1f / (float)this.segments.GetLength(0);
            for (int i = 0; i < (sLeaser.sprites[this.startSprite] as TriangleMesh).verticeColors.Length; i++)
            {
                float f = (float)i / (float)((sLeaser.sprites[this.startSprite] as TriangleMesh).verticeColors.Length - 1);
                (sLeaser.sprites[this.startSprite] as TriangleMesh).verticeColors[i] = this.MeshColor(f);
            }
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
        }


        public Color MeshColor(float f)
        {
            f = Mathf.Abs(f - 0.5f) * 2f;
            return Custom.HSL2RGB(
                Custom.Decimal(Mathf.Lerp(0.4f, 0.1f, 0.5f + 0.5f * Mathf.Pow(f, 3f))), 
                Custom.Decimal(Mathf.Lerp(0.4f, 0.1f, 0.5f + 0.5f * Mathf.Pow(f, 3f))), 
                Custom.LerpMap(f, 0.7f, 1f, 0.1f, 0.02f)
            );
        }


        public float Rad(float f)
        {
            return Mathf.Lerp(0.2f, 1f, Mathf.Pow(Mathf.Clamp(Mathf.Sin(f * 3.1415927f), 0f, 1f), 0.5f));
        }


        public void SetPosition(Vector2 pos)
        {
            this.posB = new Vector2?(pos);
            this.segments[0, 0] = pos;
            this.segments[0, 1] = pos;
        }


        public void Update()
        {
            if (this.pGraphics.player.room == null)
                return;
            this.conRad = this.length * this.lengthFactor / (float)this.segments.GetLength(0) * 1.5f;
            this.wind += Custom.RNV() * 0.2f * Random.value;
            this.wind = Vector2.ClampMagnitude(this.wind, 1f);
            for (int i = 2; i < this.segments.GetLength(0); i++)
            {
                Vector2 a = Custom.DirVec(this.segments[i - 2, 0], this.segments[i, 0]);
                this.segments[i - 2, 2] -= a * 0.15f;
                this.segments[i, 2] += a * 0.15f;
            }
            for (int j = 0; j < this.segments.GetLength(0); j++)
            {
                float num = (float)j / (float)(this.segments.GetLength(0) - 1);
                this.segments[j, 1] = this.segments[j, 0];
                this.segments[j, 0] += this.segments[j, 2];
                this.segments[j, 2] *= 0.999f;
                if (this.pGraphics.player.room.aimap != null && this.pGraphics.player.room.aimap.getAItile(this.segments[j, 0]).terrainProximity < 4)
                {
                    IntVector2 tilePosition = this.pGraphics.player.room.GetTilePosition(this.segments[j, 0]);
                    Vector2 a2 = new Vector2(0f, 0f);
                    for (int k = 0; k < 4; k++)
                    {
                        if (!this.pGraphics.player.room.GetTile(tilePosition + Custom.fourDirections[k]).Solid && !this.pGraphics.player.room.aimap.getAItile(tilePosition + Custom.fourDirections[k]).narrowSpace)
                        {
                            float num2 = 0f;
                            for (int l = 0; l < 4; l++)
                                num2 += (float)this.pGraphics.player.room.aimap.getAItile(tilePosition + Custom.fourDirections[k] + Custom.fourDirections[l]).terrainProximity;
                            a2 += Custom.fourDirections[k].ToVector2() * num2;
                        }
                    }
                    this.segments[j, 2] += a2.normalized * Custom.LerpMap((float)this.pGraphics.player.room.aimap.getAItile(this.segments[j, 0]).terrainProximity, 0f, 3f, 2f, 0.2f);
                }
                this.segments[j, 2] += this.wind * 0.005f;
                if (num > 0.5f && this.posB != null)
                    this.segments[j, 2] += Vector2.ClampMagnitude(this.posB.Value - this.segments[j, 0], 40f) / 420f * Mathf.InverseLerp(0.75f, 1f, num);
            }
            for (int m = this.segments.GetLength(0) - 1; m > 0; m--)
                this.Connect(m, m - 1);
            for (int n = 1; n < this.segments.GetLength(0); n++)
                this.Connect(n, n - 1);
        }
    }
}
