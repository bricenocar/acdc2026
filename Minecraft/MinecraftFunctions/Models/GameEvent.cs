namespace MinecraftFunctions.Models
{
    public class GameEvent
    {
        public string EventType { get; set; }
        public string PlayerName { get; set; }
        public string Target { get; set; }
        public string BlockType { get; set; }
        public int Quantity { get; set; }
        public string World { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public long Timestamp { get; set; }
    }
}
