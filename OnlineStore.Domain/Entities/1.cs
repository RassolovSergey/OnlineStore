namespace RPG
{
    // Тип персонажа
    public enum CharacterType : byte
    {
        Warrior,
        Mage
    }

    // Интерфейс для оружия
    public interface IWeapon
    {
        ushort CalculateDamage(Character attacker);
        void OnEquip(Character attacker);
    }

    // Персонаж
    public class Character
    {
        public string Name { get; set; }
        public ushort Strength { get; set; }
        public ushort Magic { get; set; }
        private ushort _health;
        public ushort Health
        {
            get => _health;
            set => _health = value;
        }

        public CharacterType Type { get; set; }

        public Character(string name, CharacterType type, ushort strength, ushort magic, ushort health)
        {
            Name = name;
            Type = type;
            Strength = strength;
            Magic = magic;
            _health = health;
        }

        public void TakeDamage(ushort damage)
        {
            _health = damage >= _health ? (ushort)0 : (ushort)(_health - damage);
        }
    }

    // Меч
    public class Sword : IWeapon
    {
        public ushort CalculateDamage(Character attacker)
        {
            return (ushort)(attacker.Strength * 3);
        }

        public void OnEquip(Character attacker) { }
    }

    // Волшебный посох
    public class Staff : IWeapon
    {
        public ushort CalculateDamage(Character attacker)
        {
            return (ushort)(attacker.Magic * 2 + 2);
        }

        public void OnEquip(Character attacker) { }
    }

    // Жезл дурака
    public class FoolWand : IWeapon
    {
        private static readonly Random _rng = new Random();

        public ushort CalculateDamage(Character attacker)
        {
            return (ushort)_rng.Next(0, 11);
        }

        public void OnEquip(Character attacker)
        {
            if (attacker.Type == CharacterType.Mage)
            {
                if (_rng.Next(0, 2) == 0)
                {
                    ushort temp = attacker.Strength;
                    attacker.Strength = attacker.Magic;
                    attacker.Magic = temp;
                }
            }
        }
    }

    public class GameEngine
    {
        public void InflictDamage(Character attacker, Character target, IWeapon weapon)
        {
            weapon.OnEquip(attacker);
            ushort damage = weapon.CalculateDamage(attacker);
            target.TakeDamage(damage);

            Console.WriteLine($"{attacker.Name} наносит {damage} урона {target.Name}. {target.Name} осталось {target.Health} здоровья.");
        }
    }

    // Пример использования
    class Program
    {
        static void Main()
        {
            Character warrior = new Character("Воин", CharacterType.Warrior, 5, 0, 100);
            Character mage = new Character("Маг", CharacterType.Mage, 1, 2, 50);

            IWeapon sword = new Sword();
            IWeapon staff = new Staff();
            IWeapon foolWand = new FoolWand();

            GameEngine engine = new GameEngine();

            engine.InflictDamage(warrior, mage, sword);
            engine.InflictDamage(mage, warrior, staff);
            engine.InflictDamage(mage, warrior, foolWand);
        }
    }
}
