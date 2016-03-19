using System;
using Server.Items;
using Server.Network;

namespace Server.Items
{
	[FlipableAttribute( 0xF47, 0xF48 )]
	public class BattleAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility{ get{ return WeaponAbility.BleedAttack; } }
		public override WeaponAbility SecondaryAbility{ get{ return WeaponAbility.ConcussionBlow; } }

		public override int AosStrengthReq{ get{ return 35; } }
		public override int AosMinDamage{ get{ return 15; } }
		public override int AosMaxDamage{ get{ return 17; } }
		public override int AosSpeed{ get{ return 31; } }
		public override float MlSpeed{ get{ return 3.50f; } }

        public override int OldStrengthReq { get { return 35; } }
        public override int OldMinDamage { get { return 21; } }
        public override int OldMaxDamage { get { return 34; } }
        public override int OldSpeed { get { return 33; } }

		public override int InitMinHits{ get{ return 31; } }
		public override int InitMaxHits{ get{ return 70; } }

        public override int IconItemId { get { return 3912; } }
        public override int IconHue { get { return Hue; } }
        public override int IconOffsetX { get { return 0; } }
        public override int IconOffsetY { get { return 0; } }

		[Constructable]
		public BattleAxe() : base( 0xF47 )
		{
            Name = "battleaxe";

			Weight = 4.0;
			Layer = Layer.TwoHanded;
		}

		public BattleAxe( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

            Name = "battleaxe";
		}
	}
}