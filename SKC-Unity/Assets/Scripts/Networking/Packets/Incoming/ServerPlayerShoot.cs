using Game;
using Game.Entities;
using UnityEngine;

namespace Networking.Packets.Incoming
{
    public class ServerPlayerShoot : IncomingPacket
    {
        public override PacketId Id => PacketId.ServerPlayerShoot;
        public override IncomingPacket CreateInstance() => new ServerPlayerShoot();

        private int     _bulletId;
        private int     _ownerId;
        private int     _ownerType;
        private Vector2 _startPos;
        private float   _angle;
        private float   _angleInc;
        private byte    _projCount;
        private short[] _damages;

        public override void Read(PacketReader rdr)
        {
            _bulletId = rdr.ReadInt32();
            _ownerId = rdr.ReadInt32();
            _ownerType = rdr.ReadInt16();

            _startPos = new Vector2()
            {
                x = rdr.ReadSingle(),
                y = rdr.ReadSingle()
            };

            _angle = rdr.ReadSingle();
            _angleInc = rdr.ReadSingle();
            _projCount = rdr.ReadByte();

            _damages = new short[_projCount];
            for (int i = 0; i < _projCount; i++)
                _damages[i] = rdr.ReadInt16();
        }

        public override void Handle(PacketHandler handler, Map map)
        {
            var owner = map.GetEntity(_ownerId);
            if (owner == null)
                return;

            var weaponXml = AssetLibrary.GetItemDesc(_ownerType);
            var projData = weaponXml.Projectile;

            var angle = _angle;
            for (int i = 0; i < _projCount; i++)
            {
                var projectile = Projectile.Create(
                    owner,
                    projData,
                    _bulletId + i,
                    GameTime.Time,
                    angle,
                    _startPos,
                    _damages[i], // Schaden direkt hier übergeben
                    map
                );

                map.AddObject(projectile, _startPos);
                angle += _angleInc;
            }

            owner.SetAttack(weaponXml, _angle);
        }
    }
}