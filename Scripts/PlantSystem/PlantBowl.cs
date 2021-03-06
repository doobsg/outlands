using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class PlantBowl : Item
    {
        public PlantSeed m_PlantSeed = null;
        [CommandProperty(AccessLevel.Administrator)]
        public PlantSeed PlantSeed
        {
            get { return m_PlantSeed; }
            set { m_PlantSeed = value; }
        }

        public bool m_ReadyForHarvest = false;
        [CommandProperty(AccessLevel.Administrator)]
        public bool ReadyForHarvest
        {
            get { return m_ReadyForHarvest; }
            set 
            { 
                m_ReadyForHarvest = value;

                if (m_ReadyForHarvest)
                    ItemID = 4552;

                else
                    ItemID = 4551;
            }
        }

        public double m_GrowthValue = 0;
        [CommandProperty(AccessLevel.Administrator)]
        public double GrowthValue
        {
            get { return m_GrowthValue; }
            set
            { 
                m_GrowthValue = value;

                if (m_PlantSeed != null)
                {
                    if (m_GrowthValue >= m_PlantSeed.TargetGrowth)
                    {
                        m_GrowthValue = m_PlantSeed.TargetGrowth;
                        ReadyForHarvest = true;
                    }

                    if (m_GrowthValue < 0)
                        m_GrowthValue = 0;
                }
            }
        }

        public double m_WaterValue = 0;
        [CommandProperty(AccessLevel.Administrator)]
        public double WaterValue
        {
            get { return m_WaterValue; }
            set { m_WaterValue = value; }
        }

        public double m_SoilQualityValue = 0;
        [CommandProperty(AccessLevel.Administrator)]
        public double SoilQualityValue
        {
            get { return m_SoilQualityValue; }
            set { m_SoilQualityValue = value; }
        }

        public double m_HeatValue = 0;
        [CommandProperty(AccessLevel.Administrator)]
        public double HeatValue
        {
            get { return m_HeatValue; }
            set { m_HeatValue = value; }
        }

        [Constructable]
        public PlantBowl(): base(4551)
        {
            Name = "Plant Bowl";
            Weight = 1.0;

            PlantPersistance.m_Instances.Add(this);
        }

        public PlantBowl(Serial serial): base(serial)
        {
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            if (m_PlantSeed != null)
            {
                if (m_ReadyForHarvest)
                {
                    LabelTo(from, "(" + m_PlantSeed.PlantName + ")");
                    LabelTo(from, "[Ready for Harvest]");
                }

                else
                {
                    string growthText = Utility.CreateDecimalString(GrowthValue, 1);
                    growthText = growthText + " / " + PlantSeed.TargetGrowth.ToString();

                    LabelTo(from, "(" + m_PlantSeed.SeedName + ")");
                    LabelTo(from, "[Growth: " + growthText + "]");
                }
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);
            
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.SendMessage("You are too far away to access that.");
                return;
            }

            from.SendSound(0x5E3);

            from.CloseGump(typeof(PlantGump));
            from.SendGump(new PlantGump(from, this));
        }

        public void AddIngredient(Mobile from)
        {
            if (from == null) return;
            if (m_ReadyForHarvest) return;            

            if (m_PlantSeed == null)
                from.SendMessage("What seed do you wish to place in this plant bowl?");

            else
                from.SendMessage("What ingredient do you wish to add to this plant bowl?");
            
            from.Target = new AddIngredientTarget(from, this);
        }

        public static bool IsWaterContainer(object target)
        {
            if (target is BaseBeverage)
            {
                BaseBeverage beverage = target as BaseBeverage;

                if (beverage.Content == BeverageType.Water)
                    return true;
            }

            else if (target is BaseWaterContainer)
            {
                BaseWaterContainer waterContainer = target as BaseWaterContainer;
                
                return true;
            }

            else if (target is IWaterSource)
            {
                IWaterSource waterSource = target as IWaterSource;

                return true;
            }

            else
            {
                Item item = (Item)target;
                IWaterSource waterSource;

                waterSource = (item as IWaterSource);

                if (waterSource == null && item is AddonComponent)
                {
                    waterSource = (((AddonComponent)item).Addon as IWaterSource);

                    if (waterSource != null)
                        return true;
                }
            }

            return false;
        }

        public bool GetWaterFromContainer(Mobile from, object target)
        {
            if (target is BaseBeverage)
            {
                BaseBeverage beverage = target as BaseBeverage;
                
                if (beverage.Quantity >= 1 && beverage.Content == BeverageType.Water)
                {
                    beverage.Quantity--;
                    return true;
                }
            }

            else if (target is BaseWaterContainer)
            {
                BaseWaterContainer waterContainer = target as BaseWaterContainer;

                if (waterContainer.Quantity >= 1)
                {
                    waterContainer.Quantity--;
                    return true;
                }
            }

            else if (target is IWaterSource)
            {
                IWaterSource waterSource = target as IWaterSource;

                if (waterSource.Quantity >= 1)
                {
                    waterSource.Quantity--;
                    return true;
                }
            }

            else
            {
                Item item = (Item)target;
                IWaterSource waterSource;

                waterSource = (item as IWaterSource);

                if (waterSource == null && item is AddonComponent)
                {
                    waterSource = (((AddonComponent)item).Addon as IWaterSource);

                    if (waterSource != null)
                        return true;
                }
            }

            return false;
        }

        public void DetermineHeatLevel()
        {
            int MaxHeatDistance = 2;

            double BaseLightHeatScalar = 10.0;
            double OpenFireHeatScalar = 20.0;
            double ForgeHeatScalar = 30.0;

            double totalHeat = 0;

            if (RootParent == null)
            {
                IPooledEnumerable nearbyHeatSources = Map.GetItemsInRange(Location, MaxHeatDistance);

                foreach (Item item in nearbyHeatSources)
                {
                    double itemHeat = 0;

                    if (item.RootParent != null)
                        continue;

                    int distance = Utility.GetDistance(Location, item.Location);
                    double heatScalar = ((double)MaxHeatDistance + 1) - (double)distance;

                    if (Math.Abs(Location.Z - item.Location.Z) > 15)
                        continue;

                    //Organized by Ascending Priority
                    if (item is BaseLight)
                    {
                        BaseLight baseLight = item as BaseLight;

                        if (baseLight.Burning)
                            itemHeat = BaseLightHeatScalar * heatScalar;
                    }

                    if (item is BaseEquipableLight)
                    {
                        BaseEquipableLight candle = item as BaseEquipableLight;

                        if (candle.Burning)
                            itemHeat = BaseLightHeatScalar * heatScalar;
                    }
                    
                    if (item is Campfire)
                    {
                        Campfire campfire = item as Campfire;

                        if (campfire.Status == CampfireStatus.Burning || campfire.Status == CampfireStatus.Extinguishing)
                            itemHeat = OpenFireHeatScalar * heatScalar;
                    }

                    if (item is Brazier || item is BrazierTall)
                    {
                        BaseLight brazier = item as BaseLight;

                        if (brazier.Burning)
                            itemHeat = OpenFireHeatScalar * heatScalar;
                    }

                    if (item is Forge || item is LargeForgeEast || item is LargeForgeWest || item is ForgeComponent)
                        itemHeat = ForgeHeatScalar * heatScalar;

                    totalHeat += itemHeat;
                }

                nearbyHeatSources.Free();
            }

            HeatValue = totalHeat;

            if (HeatValue > PlantPersistance.MaxHeat)
                HeatValue = PlantPersistance.MaxHeat;
        }        

        public double GetWaterPenalty()
        {
            if (m_PlantSeed == null)
                return 0;

            double targetWater = m_PlantSeed.TargetWater;
            double waterDifference = 0.0;

            double insufficientWaterScalar = 0.01;
            double excessWaterScalar = 0.01;

            waterDifference = Math.Abs(WaterValue - targetWater);

            if (WaterValue < targetWater)
                return (waterDifference * insufficientWaterScalar);

            else
                return (waterDifference * excessWaterScalar);
        }

        public double GetSoilPenalty()
        {
            if (m_PlantSeed == null)
                return 0;

            double targetSoilQuality = m_PlantSeed.TargetSoilQuality;
            double soilQualityDifference = 0.0;

            double insufficientSoilQualityScalar = 0.01;
            double excessSoilQualityScalar = 0.01;

            soilQualityDifference = Math.Abs(SoilQualityValue - targetSoilQuality);

            if (SoilQualityValue < targetSoilQuality)
                return (soilQualityDifference * insufficientSoilQualityScalar);

            else
               return (soilQualityDifference * excessSoilQualityScalar);
        }

        public double GetHeatPenalty()
        {
            if (m_PlantSeed == null)
                return 0;

            double targetHeat = m_PlantSeed.TargetHeat;
            double heatDifference = 0.0;

            double insufficientHeatScalar = 0.01;
            double excessHeatScalar = 0.01;

            heatDifference = Math.Abs(HeatValue - targetHeat);

            if (HeatValue < targetHeat)
                return (heatDifference * insufficientHeatScalar);

            else
               return (heatDifference * excessHeatScalar); 
        }

        public double GetDailyGrowthScalar()
        {
            double growthScalar = 1 - GetWaterPenalty() - GetSoilPenalty() - GetHeatPenalty();

            if (growthScalar > 1)
                growthScalar = 1;

            if (growthScalar < 0)
                growthScalar = 0;

            return growthScalar;
        }

        public class AddIngredientTarget : Target
        {
            private Mobile m_From;
            private PlantBowl m_PlantBowl;

            public AddIngredientTarget(Mobile from, PlantBowl plantBowl): base(18, false, TargetFlags.None)
            {
                m_From = from;
                m_PlantBowl = plantBowl;
            }

            protected override void OnTarget(Mobile from, object target)
            {
                if (!SpecialAbilities.Exists(from)) return;
                if (m_PlantBowl == null) return;
                if (m_PlantBowl.Deleted) return;

                IPoint3D location = target as IPoint3D;

                if (location == null)
                    return;

                Point3D targetLocation = new Point3D(location.X, location.Y, location.Z);

                if (target is Item)
                {
                    Item item = target as Item;

                    targetLocation = item.GetWorldLocation();
                }

                if (Utility.GetDistance(from.Location, targetLocation) > 2)
                {
                    from.SendMessage("That target is too far away.");
                    return;
                }

                if (!from.Map.InLOS(from.Location, targetLocation))
                {
                    from.SendMessage("That target is out of sight.");
                    return;
                }

                if (!from.InRange(m_PlantBowl.GetWorldLocation(), 2))
                {
                    from.SendMessage("You are too far away from your plant bowl to target that.");

                    from.CloseGump(typeof(PlantGump));
                    from.SendGump(new PlantGump(from, m_PlantBowl));

                    return;
                }

                //Need Seed
                if (m_PlantBowl.PlantSeed == null)
                {
                    if (target is PlantSeed)
                    {
                        PlantSeed plantSeed = target as PlantSeed;
                        
                        m_PlantBowl.PlantSeed = plantSeed;
                        m_PlantBowl.PlantSeed.Internalize();

                        from.PlaySound(0x059);
                        from.SendMessage("You plant the seed into the plant bowl.");
                    }

                    else                    
                        from.SendMessage("That is not a plant seed.");      

                    from.CloseGump(typeof(PlantGump));
                    from.SendGump(new PlantGump(from, m_PlantBowl));

                    return;
                }

                //Add Ingredients
                else
                {
                    //Water Container
                    if (IsWaterContainer(target))
                    {
                        if (m_PlantBowl.WaterValue == PlantPersistance.MaxWater)
                            from.SendMessage("That plant bowl is already at full water capacity.");

                        else if (m_PlantBowl.GetWaterFromContainer(from, target))
                        {
                            m_PlantBowl.WaterValue += PlantPersistance.WaterAddedPerUse;

                            if (m_PlantBowl.WaterValue > PlantPersistance.MaxWater)
                                m_PlantBowl.WaterValue = PlantPersistance.MaxWater;

                            from.PlaySound(0x4E);
                            from.SendMessage("You fill the plant bowl with some water.");
                        }

                        else                    
                            from.SendMessage("There is not enough water available to fill this plant bowl.");                    

                        from.CloseGump(typeof(PlantGump));
                        from.SendGump(new PlantGump(from, m_PlantBowl));

                        return;
                    }

                    //Soil Item
                    else if (target is SoilEnhancer)
                    {
                        SoilEnhancer soilEnhancer = target as SoilEnhancer;
                        
                        if (m_PlantBowl.SoilQualityValue == PlantPersistance.MaxSoilQuality)
                            from.SendMessage("That plant bowl is at maximum soil richness.");

                        else if (m_PlantBowl.SoilQualityValue >= soilEnhancer.MaxSoilQuality)
                            from.SendMessage("That plant requires a higher quality soil enhancer to increase its soil richness further.");
                        
                        else
                        {
                            m_PlantBowl.SoilQualityValue += soilEnhancer.SoilQualityIncrease;

                            if (m_PlantBowl.SoilQualityValue > soilEnhancer.MaxSoilQuality)
                                m_PlantBowl.SoilQualityValue = soilEnhancer.MaxSoilQuality;

                            from.PlaySound(Utility.RandomList(0x134, 0x133,  0x132, 0x131));
                            from.SendMessage("You increase the plant's soil richness.");

                            soilEnhancer.Charges--;

                            if (soilEnhancer.Charges == 0)
                                soilEnhancer.Delete();
                        }

                        from.CloseGump(typeof(PlantGump));
                        from.SendGump(new PlantGump(from, m_PlantBowl));

                        return;
                    }

                    else
                    {
                        from.SendMessage("You cannot add that as an ingredient to this plant bowl.");

                        from.CloseGump(typeof(PlantGump));
                        from.SendGump(new PlantGump(from, m_PlantBowl));

                        return;
                    }
                }
            }
        }
        
        public bool Harvest(Mobile from)
        {
            if (!SpecialAbilities.Exists(from))
                return false;

            if (from.Backpack.TotalItems >= from.Backpack.MaxItems)
            {
                from.SendMessage("Your backpack does not have enough space for that.");
                return false;
            }

            if (m_PlantSeed != null && from.Backpack != null)
            {
                Item item = (Item)Activator.CreateInstance(m_PlantSeed.PlantType);

                if (item == null)
                    return false;

                if (item.Stackable && m_PlantSeed.PlantCount > 1)
                {
                    item.Amount = m_PlantSeed.PlantCount;
                    from.Backpack.DropItem(item);                    
                }

                else
                {
                    if (from.Backpack.TotalItems + m_PlantSeed.PlantCount > from.Backpack.MaxItems)
                    {
                        from.SendMessage("Your backpack does not have enough space for that.");
                        return false;
                    }

                    item.Delete();

                    for (int a = 0; a < m_PlantSeed.PlantCount; a++)
                    {
                        item = (Item)Activator.CreateInstance(m_PlantSeed.PlantType);
                        from.Backpack.DropItem(item);
                    }
                }

                m_PlantSeed.Delete();
                m_PlantSeed = null;

                m_ReadyForHarvest = false;
                m_GrowthValue = 0;
                m_WaterValue = 0;
                m_SoilQualityValue = 0;
                m_HeatValue = 0;

                from.PlaySound(0x5AE);
                from.SendMessage("You harvest the plant.");

                TimedStatic dirt = new TimedStatic(Utility.RandomList(7681, 7682), 3);
                dirt.Name = "dirt";
                dirt.MoveToWorld(from.Location, from.Map);

                int dirtCount = Utility.RandomMinMax(2, 3);
                for (int a = 0; a < dirtCount; a++)
                {
                    Point3D newLocation = SpecialAbilities.GetRandomAdjustedLocation(from.Location, from.Map, true, 1, false);

                    dirt = new TimedStatic(Utility.RandomList(7681, 7682), 3);
                    dirt.Name = "dirt";
                    dirt.MoveToWorld(newLocation, from.Map);
                }

                return true;
            }

            return false;
        }

        public override void OnDelete()
        {
            if(PlantPersistance.m_Instances.Contains(this))
                PlantPersistance.m_Instances.Remove(this);

            base.OnDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version

            //Version 0
            writer.Write(m_PlantSeed);
            writer.Write(m_ReadyForHarvest);
            writer.Write(m_GrowthValue);
            writer.Write(m_WaterValue);
            writer.Write(m_SoilQualityValue);
            writer.Write(m_HeatValue);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            //Version 0
            if (version >= 0)
            {
                m_PlantSeed = (PlantSeed)reader.ReadItem();
                m_ReadyForHarvest = reader.ReadBool();
                m_GrowthValue = reader.ReadDouble();
                m_WaterValue = reader.ReadDouble();
                m_SoilQualityValue = reader.ReadDouble();
                m_HeatValue = reader.ReadDouble();
            }

            //-----

            PlantPersistance.m_Instances.Add(this);
        }
    }
}