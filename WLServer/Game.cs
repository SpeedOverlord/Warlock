using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;

namespace sis
{
    public class Game
    {
        public static Form Main_Form;
        public static TextBox LogBox;
        public static Ground Ground = new Ground(GameDef.WIDTH, GameDef.HEIGHT);
        //  player list
        public static RegisterList<Player> Player_List = new RegisterList<Player>(GameDef.MAX_PLAYER);
        //  spell list
        public static Spell[] Spell_List = new Spell[GameDef.SPELL_COUNT];
        //  unit list
        public static List<Unit> Unit_List = new List<Unit>();
        //  missile list
        public static List<Missile> Missile_List = new List<Missile>();
        //  effect list
        public static List<Effect> Effect_List = new List<Effect>();
        //  packer
        public static GamePacker packer = new GamePacker();
        //  initialize
        public static void Initialize() {
            //  spell list set
            Spell_List[0] = new FireBall();
            Spell_List[1] = new Redemption();
            Spell_List[2] = new Flash();
            Spell_List[3] = new Gravity();
            Spell_List[4] = new TraceBall();
            Spell_List[5] = new HealBall();
        }
        //  update
        public volatile static Mutex UpdateMutex = new Mutex();
        public static void Update()
        {
            try
            {
                if (UpdateMutex.WaitOne(5) == false)
                {
                    ServerPT.Form.PushLog("update jump\n");
                    return;
                }

                //  unit move
                foreach (Unit unit in Unit_List) unit.Update();
                //  missile move
                foreach (Missile missile in Missile_List) missile.Update();
                //  unit action acount
                foreach (Unit unit in Unit_List) unit.UpdateAction();
                //  collision
                int missile_count = Missile_List.Count;
                for (int i = 0; i < missile_count; ++i)
                {
                    for (int j = i + 1; j < missile_count; ++j)
                    {
                        Missile_List[i].CheckCollision(Missile_List[j]);
                    }
                }
                foreach (Missile missile in Missile_List)
                {
                    if (missile.Collision == true) continue;
                    foreach (Unit unit in Unit_List)
                    {
                        missile.CheckCollision(unit);
                    }
                }
                //  hurt
                List<Missile> explode_list = new List<Missile>();
                foreach (Missile missile in Missile_List)
                {
                    if (missile.Explode) explode_list.Add(missile);
                    if (missile.Collision == false) continue;
                    foreach (Unit unit in Unit_List)
                    {
                        missile.CheckHurt(unit);
                    }
                    missile.Collision = false;  //  復原 collision
                }
                List<Unit> unit_dead_list = new List<Unit>();
                foreach (Unit unit in Unit_List)
                {
                    if (Ground.InLava(unit.Pos))
                    {
                        unit.Hp -= GameDef.LAVA_DAMAGE;
                    }
                    if (unit.Dead()) unit_dead_list.Add(unit);
                }
                foreach (Unit unit in unit_dead_list)
                {
                    unit.Owner.Dead();
                }
                //  lava hurt
                //  explode missile
                foreach (Missile missile in explode_list) missile.Unregister();
                //  effect update
                List<Effect> effect_dead_list = new List<Effect>();
                foreach (Effect effect in Effect_List)
                {
                    effect.Update();
                    if (effect.Dead) effect_dead_list.Add(effect);
                }
                foreach (Effect effect in effect_dead_list) effect.Unregister();

                packer.Update();
                for (int i = 0; i < GameDef.MAX_PLAYER; ++i)
                {
                    if (Player_List.Used[i] == true)
                    {
                        Player_List[i].PushSend(packer.Buffer);
                    }
                }
                UpdateMutex.ReleaseMutex();
            }
            catch
            {
                StreamWriter stream = new StreamWriter("update exception.log"); stream.Close(); stream.Dispose();
                ServerPT.Form.PushLog("update exception\n");
                return;
            }
        }
        //  respawn point
        public static IPair GetRespawnPoint()
        {
            return new IPair((GameDef.WIDTH >> 1) * GameDef.PIXEL_SCALE, (GameDef.HEIGHT >> 1) * GameDef.PIXEL_SCALE);
        }
    }
}
