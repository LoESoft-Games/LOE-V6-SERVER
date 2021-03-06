﻿#region

using System;
using System.Linq;
using LoESoft.Core;
using LoESoft.GameServer.logic.loot;
using LoESoft.GameServer.realm.entity;
using LoESoft.GameServer.realm.terrain;

#endregion

namespace LoESoft.GameServer.realm.mapsetpiece
{
    internal class Pyre : MapSetPiece
    {
        private static readonly string Floor = "Scorch Blend";

        private static readonly Loot chest = new Loot(
            new TierLoot(5, ItemType.Weapon, 0.3),
            new TierLoot(6, ItemType.Weapon, 0.2),
            new TierLoot(7, ItemType.Weapon, 0.1),
            new TierLoot(4, ItemType.Armor, 0.3),
            new TierLoot(5, ItemType.Armor, 0.2),
            new TierLoot(6, ItemType.Armor, 0.1),
            new TierLoot(2, ItemType.Ability, 0.3),
            new TierLoot(3, ItemType.Ability, 0.2),
            new TierLoot(1, ItemType.Ring, 0.25),
            new TierLoot(2, ItemType.Ring, 0.15),
            new TierLoot(1, ItemType.Potion, 0.5)
            );

        private readonly Random rand = new Random();

        public override int Size => 30;

        public override void RenderSetPiece(World world, IntPoint pos)
        {
            EmbeddedData dat = GameServer.Manager.GameData;
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                {
                    double dx = x - (Size / 2.0);
                    double dy = y - (Size / 2.0);
                    double r = Math.Sqrt(dx * dx + dy * dy) + rand.NextDouble() * 4 - 2;
                    if (r <= 10)
                    {
                        WmapTile tile = world.Map[x + pos.X, y + pos.Y].Clone();
                        tile.TileId = dat.IdToTileType[Floor];
                        tile.ObjType = 0;
                        world.Map[x + pos.X, y + pos.Y] = tile;
                    }
                }

            Entity lord = Entity.Resolve("Phoenix Lord");
            lord.Move(pos.X + 15.5f, pos.Y + 15.5f);
            world.EnterWorld(lord);

            Container container = new Container(0x0501, null, false);
            Item[] items = chest.GetLoots(5, 8).ToArray();
            for (int i = 0; i < items.Length; i++)
                container.Inventory[i] = items[i];
            container.Move(pos.X + 15.5f, pos.Y + 15.5f);
            world.EnterWorld(container);
        }
    }
}