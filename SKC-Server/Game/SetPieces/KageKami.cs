using RotMG.Common;

namespace RotMG.Game.SetPieces
{
    internal class KageKami : ISetPiece
    {
        public int Size => 65;

        public void RenderSetPiece(World world, IntPoint pos)
        {
            SetPieces.RenderFromMap(world, pos, Resources.SetPieces["Kage Kami"]);
        }
    }
}