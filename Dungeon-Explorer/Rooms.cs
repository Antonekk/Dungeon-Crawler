using System;
using characters;
using items;
using cto;


namespace rooms
{
    abstract class Room
    {
        protected Random rnd = new Random();
        protected CTO term = new CTO();
        protected int room_level;

        public abstract void start(Player p);
    }


    class Fight_Room : Room
    {
        Enemy enemy;

        public Fight_Room(){
            room_level = 1;
            enemy = generate_enemy();
        }
        public Fight_Room(int lvl){
            room_level = lvl;
            enemy = generate_enemy();
        }

        Enemy generate_enemy(){
            int e = rnd.Next(1,4);
            switch(e){
                case 1:
                    return new Orc(room_level);
                case 2:
                    return new Skeleton(room_level);
                case 3:
                    return new Goblin(room_level);
            }
            throw new Exception("Wrong character");
        }

        public override void start(Player p)
        {
            Start_Fight(p);
            ConsoleKeyInfo key = Console.ReadKey();


        }

        public void Start_Fight(Player p){
            //todo
            // Player can either focus to boost doge chance or prepare to boost next atack (effects can stack)
            int doge_chance = 10;
            double damage_mult = 1.0;
            while(!enemy.is_dead()){
                Console.Clear();
                term.WritePlayerData(p);
                term.Write_Center($"You've encountered  [{enemy.get_class_name()} {enemy.get_current_hp()}/{enemy.max_hp}]\n");
                Console.WriteLine("[1] Atack:\n");
                Console.WriteLine($"[2] Prepare (Bonus damage) [Current bonus: {damage_mult}]:\n");
                Console.WriteLine($"[3] Focus (Bonus doge chance) [Current bonus: {doge_chance}]\n");
                if(p.is_in_danger()){
                    Console.WriteLine("[4] Run Away (Chance to lose gold )\n");
                }
                ConsoleKeyInfo key = Console.ReadKey();
                switch(key.Key){
                    case ConsoleKey.D1:
                        p.Atack(enemy, damage_mult);
                        break;

                    case ConsoleKey.D2:
                        damage_mult += (rnd.NextDouble() * (1.25 - 1.0) + 1.0);
                        break;

                    case ConsoleKey.D3:
                        int bc = rnd.Next(10,15);
                        if (doge_chance + bc <= 100){
                            doge_chance += bc;
                            break;
                        }
                        doge_chance = 100;
                        break;

                    case ConsoleKey.D4:
                        p.lose_gold();
                        Console.WriteLine("You ran away:\n");
                        return;
                    default:
                        break;
                }
                if(enemy.is_dead()){
                    break;
                }

                enemy.Atack(p,1);
                if(p.is_dead()){
                    Console.WriteLine("You died\n");
                    System.Environment.Exit(0);
                }


            }


        }


    }

    class Shop : Room
    {
        List<Tuple<int, Item>> available_items;
        Item_Generator ig;
        public Shop(){
            ig = new Item_Generator();
            available_items = new List<Tuple<int, Item>>();
            add_items();

        }
        public Shop(int level){
            ig = new Item_Generator(level);
            available_items = new List<Tuple<int, Item>>();
            add_items();

        }

        void add_items(){
            //todo
            //The way of adding items if weird
            Tuple <int, Item> t;
            for (int i=0;i<3;i++){
                t = Tuple.Create(10, ig.generate_item(i));
                available_items.Add(t);
            }

        }

        void buy(int item, Player p){
            int price = available_items[item].Item1;
            if (p.pay(price)){
                Console.WriteLine("You bought" + available_items[item].Item2.ToString() + "\n");
                p.equip_item(item,available_items[item].Item2);
                return;
            }
            Console.WriteLine("Not enough gold coins\n");
        }

        public override void start(Player p)
        {
            Console.Clear();
            term.WritePlayerData(p);
            term.Write_Center("You found Shop\n");
            for(int i = 0; i<3; i++){
                Console.WriteLine($"[{i+1}] Pay {available_items[i].Item1} for {available_items[i].Item2} \n");
            }
            Console.WriteLine("[Other] Leave:\n");
            ConsoleKeyInfo key = Console.ReadKey();
            term.ClearCurrentConsoleLine();
            switch(key.Key){
                case ConsoleKey.D1:
                    buy(0, p);
                    break;

                case ConsoleKey.D2:
                    buy(1,p);
                    break;

                case ConsoleKey.D3:
                    buy(2,p);
                    break;
                default:
                    return;
            }
            Console.WriteLine("[Any] Go to the next room");
            ConsoleKeyInfo key2 = Console.ReadKey();
        }

    }

    class Healing_fountain : Room
    {
        int price;
        int chance;
        int heal;
        public Healing_fountain(){
            room_level = 1;
            price = Scale_price(room_level);
            heal = Scale_hp(room_level);
            chance = rnd.Next(40,90);

        }
        public Healing_fountain(int level){
            room_level = level;
            price = Scale_price(room_level);
            heal = Scale_hp(room_level);
            chance = rnd.Next(40,90);
        }

        public override void start(Player p)
        {
            Console.Clear();
            term.WritePlayerData(p);
            term.Write_Center("You found Healing Fountain\n");
            Console.WriteLine($"[1] Pay {price} gold, heal {heal}hp with {chance}% success rate\n");
            Console.WriteLine("[Other] Leave:\n");
            ConsoleKeyInfo key = Console.ReadKey();
            if(key.Key == ConsoleKey.D1){
                buy_state(p);
                Console.WriteLine("[Any] Go to the next room");
                ConsoleKeyInfo key2 = Console.ReadKey();
            }

        }

        private void buy_state(Player p){
            term.ClearCurrentConsoleLine();
            int r = rnd.Next(100);
            if(p.pay(price))
            {
                if((r >= chance && r<=90)){
                    Console.WriteLine("Success\n");
                    p.heal(this.heal);
                    return;
                }
                Console.WriteLine("Healing was not successful\n");
                return;

            }
            Console.WriteLine("Not enough gold coins\n");

        }

        int Scale_price(int lvl){
            int xlvl = 2*lvl;
            return (xlvl* (int)(Math.Ceiling(Math.Log(xlvl))));
        }
        int Scale_hp(int lvl){
            int xlvl = 5*lvl;
            return (xlvl* (int)(Math.Ceiling(Math.Log(xlvl)))) ;
        }
    }
}
