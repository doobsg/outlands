using System;

namespace Server.Items
{
    public class RawMeatShank : Item
    {
        [Constructable]
        public RawMeatShank(): this(1)
        {
        }

        [Constructable]
        public RawMeatShank(int amount): base(5641)
        {
            Name = "raw meat shank";

            Stackable = true;
            Weight = 0.1;
            Amount = amount;
        }

        public RawMeatShank(Serial serial): base(serial)
        {
        }
        
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadEncodedInt();
        }
    }
}