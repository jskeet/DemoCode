using System;
using System.Collections.Generic;

namespace Demos
{
    public class Item
    {
        public string Description { get; }

        public Item(string description) => Description = description;
    }

    public class Weapon : Item
    {
        public int Damage { get; }

        public Weapon(string description, int damage) : base(description)
            => Damage = damage;
    }

    public class Artwork : Item
    {
        public DateTime CreationDate { get; }

        public Artwork(string description, DateTime creationDate) : base(description)
            => CreationDate = creationDate;
    }

    class PropertyMatching
    {
        static void Main()
        {
            var items = new List<Item>
            {
                new Item("Just a boring item"),
                new Weapon("Rocket launcher", 1000),
                new Artwork("Ouvrage du Vent II", new DateTime(1962, 1, 1)),
                new Weapon("Croquet mallet", 20),
                new Weapon("Bad joke", 0),
                new Artwork("A Sunday Afternoon [...]", new DateTime(1884, 1, 1))
            };

            foreach (var item in items)
            {
                Console.WriteLine($"{item.Description}: {GetFact(item)}");
            }
        }
    
        static string GetFact(Item item) => item switch
        {
            Weapon { Damage: 0 } => "Harmless",
            Weapon { Damage: var d } w when d > 500 => $"Ooh, heavy! {w}",
            Weapon { Damage: var d } => $"Lightweight: only does {d} damage",
            Artwork { CreationDate: var (year, _, _) } => $"Created in {year}",
            _ => "Nothing interesting"
        };

        // No type name in the patterns, because we assume it's a Weapon
        static string GetWeaponFact(Weapon weapon) => weapon switch
        {
            { Damage: 0 } => "Harmless",
            { Damage: var d } w when d > 500 => $"Ooh, heavy! {w}",
            { Damage: var d } => $"Lightweight: only does {d} damage",
            null => throw new ArgumentNullException(nameof(weapon))
        };
    }
}
