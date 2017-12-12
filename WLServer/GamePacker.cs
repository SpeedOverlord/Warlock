using System;
using System.IO;
using System.Threading;

namespace sis
{
    public class GamePacker
    {
        public const short PackMod = (short)GameDef.PackMod;
        public Mutex Mut = new Mutex();
        public short PackNumber { get; set;}
        public Byte[] Buffer { get; set; }

        public GamePacker()
        {
            PackNumber = -1;
            Buffer = null;
        }
        public void Update()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            short temp_short = (short)(2 + 1 + 1 + Game.Unit_List.Count * 6 + 1 + Game.Effect_List.Count * 4 + GameDef.SPELL_COUNT);
            byte temp_byte;

            writer.Write(temp_short); writer.Write((Byte)((PackNumber + 1) % PackMod));   //  length / id

            temp_byte = (byte)(Game.Unit_List.Count & 0xFF);    //  unit
            writer.Write(temp_byte);

            foreach (Unit unit in Game.Unit_List)
            {
                PushUnit(writer, unit);
            }

            temp_byte = (byte)(Game.Effect_List.Count & 0xFF);  //  effect
            writer.Write(temp_byte);

            foreach (Effect effect in Game.Effect_List)
            {
                PushEffect(writer, effect);
            }

            temp_byte = 0;
            for (int i = 0; i < GameDef.SPELL_COUNT; ++i) writer.Write(temp_byte);    //  reserve for cd data

            writer.Flush(); stream.Flush();
            writer.Dispose();

            Byte[] buffer = stream.ToArray();

            Mut.WaitOne();
            Buffer = buffer;
            PackNumber = (short)((PackNumber + 1) % PackMod);
            Mut.ReleaseMutex();
        }
        void PushEffect(BinaryWriter writer, Effect effect)
        {
            int temp_int = (int)((effect.Pos.X / GameDef.PIXEL_SCALE) & 0x7FF);
            temp_int <<= 11;
            temp_int += (int)((effect.Pos.Y / GameDef.PIXEL_SCALE) & 0x7FF);
            temp_int <<= 4;
            temp_int += effect.DrawId & 0xF;
            temp_int <<= 6;
            temp_int += effect.DrawArg & 0x3F;
            writer.Write(temp_int);
        }
        void PushUnit(BinaryWriter write, Unit unit)
        {
            short temp_short = (short)(unit.Hp & 0xFFFF);
            write.Write(temp_short);

            int temp_int = (int)((unit.Pos.X / GameDef.PIXEL_SCALE) & 0x7FF);
            temp_int <<= 11;
            temp_int += (int)((unit.Pos.Y / GameDef.PIXEL_SCALE) & 0x7FF);
            temp_int <<= 5;
            temp_int += unit.Owner.ID & 0x1F;
            temp_int <<= 5;
            temp_int += unit.DrawArg & 0x1F;
            write.Write(temp_int);
        }
    }
}
