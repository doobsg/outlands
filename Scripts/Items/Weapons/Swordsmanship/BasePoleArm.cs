using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Engines.Harvest;
using Server.ContextMenus;

namespace Server.Items
{
	public abstract class BasePoleArm : BaseMeleeWeapon, IUsesRemaining
	{
		public override int BaseHitSound{ get{ return 0x237; } }
		public override int BaseMissSound{ get{ return 0x238; } }

		public override SkillName BaseSkill{ get{ return SkillName.Swords; } }
		public override WeaponType BaseType{ get{ return WeaponType.Polearm; } }
		public override WeaponAnimation BaseAnimation { get{ return WeaponAnimation.Slash2H; } }

		public virtual HarvestSystem HarvestSystem{ get{ return Lumberjacking.System; } }

		private int m_UsesRemaining;
		private bool m_ShowUsesRemaining;

		[CommandProperty( AccessLevel.GameMaster )]
		public int UsesRemaining
		{
			get { return m_UsesRemaining; }
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool ShowUsesRemaining
		{
			get { return m_ShowUsesRemaining; }
			set { m_ShowUsesRemaining = value; InvalidateProperties(); }
		}

		public BasePoleArm( int itemID ) : base( itemID )
		{
			m_UsesRemaining = 150;
		}

		public BasePoleArm( Serial serial ) : base( serial )
		{
		}

		public override WeaponAnimation GetAnimation()
		{
			WeaponAnimation animation = WeaponAnimation.Slash1H;

			Mobile attacker = this.Parent as Mobile;

			if (attacker != null)
			{
                if (attacker.FindItemOnLayer(Layer.TwoHanded) is BaseShield)
                {
                    switch (Utility.RandomMinMax(1, 5))
                    {
                        case 1: animation = WeaponAnimation.Pierce2H; break;
                        case 2: animation = WeaponAnimation.Pierce2H; break;
                        case 3: animation = WeaponAnimation.Pierce2H; break;
                        case 4: animation = WeaponAnimation.ShootXBow; break;
                        case 5: animation = WeaponAnimation.Slash2H; break;                        
                    }

                    return animation;
                }

                else if (attacker.FindItemOnLayer(Layer.TwoHanded) != null)
                {
                    switch (Utility.RandomMinMax(1, 5))
                    {
                        case 1: animation = WeaponAnimation.Slash2H; break;
                        case 2: animation = WeaponAnimation.Slash2H; break;
                        case 3: animation = WeaponAnimation.Bash2H; break;
                        case 4: animation = WeaponAnimation.Pierce2H; break;
                        case 5: animation = WeaponAnimation.Slash2H; break;                        
                    }

                    return animation;
                }
			}

			return animation;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( HarvestSystem == null )
				return;

			if ( IsChildOf( from.Backpack ) || Parent == from )
				HarvestSystem.BeginHarvesting( from, this );
			else
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
		{
			base.GetContextMenuEntries( from, list );

			if ( HarvestSystem != null )
				BaseHarvestTool.AddContextMenuEntries( from, this, list, HarvestSystem );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 2 ); // version

			writer.Write( (bool) m_ShowUsesRemaining );

			writer.Write( (int) m_UsesRemaining );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 2:
				{
					m_ShowUsesRemaining = reader.ReadBool();
					goto case 1;
				}
				case 1:
				{
					m_UsesRemaining = reader.ReadInt();
					goto case 0;
				}
				case 0:
				{
					if ( m_UsesRemaining < 1 )
						m_UsesRemaining = 150;

					break;
				}
			}
		}

		public override void OnHit( Mobile attacker, Mobile defender, double damageBonus )
		{
			base.OnHit( attacker, defender, damageBonus );
            /*
			if ( !Core.AOS && (attacker.Player || attacker.Body.IsHuman) && Layer == Layer.TwoHanded && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() )
			{
				StatMod mod = defender.GetStatMod( "Concussion" );

				if ( mod == null )
				{
					defender.SendMessage( "You receive a concussion blow!" );
					defender.AddStatMod( new StatMod( StatType.Int, "Concussion", -(defender.RawInt / 2), TimeSpan.FromSeconds( 30.0 ) ) );

					attacker.SendMessage( "You deliver a concussion blow!" );
					attacker.PlaySound( 0x11C );
				}
			}
*/
		}
	}
}