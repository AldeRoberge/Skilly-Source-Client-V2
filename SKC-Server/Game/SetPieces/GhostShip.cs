namespace SKC
{
    internal class GhostShip : ISetPiece
    {
        public int Size => 40;

        public void RenderSetPiece(World world, IntPoint pos)
        {
            SetPieces.RenderFromMap(world, pos, Resources.SetPieces["Ghost Ship"]);
        }
    }
}