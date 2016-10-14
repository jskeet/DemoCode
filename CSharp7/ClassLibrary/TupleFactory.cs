namespace ClassLibrary
{
    public static class TupleFactory
    {
        public static (int x, int y) GetPosition() => (5, 3);

        public static (int dx, int dy) GetVelocity() => (1, 2);
    }
}
