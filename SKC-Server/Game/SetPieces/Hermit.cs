using RotMG.Common;

namespace RotMG.Game.SetPieces
{
    internal class Hermit : ISetPiece
    {
        public int Size => 32;

        public void RenderSetPiece(World world, IntPoint pos)
        {
            SetPieces.RenderFromMap(world, pos, Resources.SetPieces["Hermit God"]);
        }
    }
}