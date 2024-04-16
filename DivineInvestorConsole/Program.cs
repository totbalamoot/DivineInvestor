using DivineInvestorLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DivineInvestorConsole
{
    [Obsolete]
    class Program
    {
        private static string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

        private static string token = "";
        private static TelegramBotClient client;

        private static IReplyMarkup menuButtons = new ReplyKeyboardMarkup
        {
            Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>{new KeyboardButton { Text = "Проверить баланс"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "Мой портфель"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "Зайти на биржу"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "Инфо об игре"} }
            }
        };

        private static IReplyMarkup actionButtons = new ReplyKeyboardMarkup
        {
            Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>{new KeyboardButton { Text = "Купить"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "Продать"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "Информация о компании"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "Зайти на биржу"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "Мой портфель"} }
            }
        };

        private static IReplyMarkup valueButtons = new ReplyKeyboardMarkup
        {
            Keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>{new KeyboardButton { Text = "1"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "5"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "10"} },
                new List<KeyboardButton>{new KeyboardButton { Text = "К действиям"} }
            }
        };

        private static Chain chain = new Chain();
        private static List<string> companiesList;


        static void Main(string[] args)
        {
            client = new TelegramBotClient(token);

            GenerateWorld();
            FillCompaniesList();

            Task task = Task.Run(() =>
            {
                Timer(5);
            });

            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            Console.ReadLine();
            client.StopReceiving();

            Console.ReadLine();
        }

        private static void OnMessageHandler(object sender, MessageEventArgs e) //async?
        {
            var msg = e.Message;

            //???
            using (ApplicationContext db = new ApplicationContext())
            {
                Player findPlayer = db.Players.FirstOrDefault(p => p.Id == msg.Chat.Id);
                if (findPlayer == null)
                {
                    Player newPlayer = new Player(msg.Chat.Id);
                    db.Players.Add(newPlayer);
                    db.SaveChanges();
                }
            }

            WriteLog(msg);


            if (msg.Text != null)
            {
                if (msg.Text == "/start")
                {
                    StartCommand(msg);
                }
                else if (msg.Text == "Проверить баланс")
                {
                    CheckBalanceCommand(msg);
                }
                else if (msg.Text == "Мой портфель")
                {
                    MyPortfolioCommand(msg);
                }
                else if (msg.Text == "Зайти на биржу")
                {
                    StockCommand(msg);
                }
                else if (msg.Text == "Обновить портфель")
                {
                    MyPortfolioCommand(msg);
                }
                else if (msg.Text == "Обновить биржу")
                {
                    StockCommand(msg);
                }
                else if (msg.Text == "В главное меню")
                {
                    MainMenuCommand(msg);
                }
                else if (companiesList.Contains(msg.Text))
                {
                    SelectCompanyCommand(msg);
                }
                else if (msg.Text == "Купить" || msg.Text == "Продать")
                {
                    SelectActionCommand(msg);
                }
                else if (msg.Text == "1" || msg.Text == "5" || msg.Text == "10")
                {
                    SelectValueCommand(msg);
                }
                else if (msg.Text == "К действиям")
                {
                    ReturnToActionCommand(msg);
                }
                else if (msg.Text == "Информация о компании")
                {
                    AboutCompanyCommand(msg);
                }
                else if (msg.Text == "Инфо об игре")
                {
                    AboutGameCommand(msg);
                }
            }
            Thread.Sleep(100);//?
        }

        private static async void WriteLog(Message message)
        {
            string logText = $"@{message.Chat.Username}: { message.Text }\n";
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                await writer.WriteLineAsync(logText);
            }
            Console.Write(logText);
        }

        // redo
        private static void Timer(int seconds)
        {
            int milliseconds = seconds * 1000;
            while (true)
            {
                EndTurn();
                Console.WriteLine($"----------NewStep {DateTime.Now.Minute}:{DateTime.Now.Second}");
                Thread.Sleep(60000);
            }
        }

        private static void EndTurn()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var companies = db.Companies
                    .Include(c => c.Shares)
                    .ThenInclude(s => s.Blocks)
                    .ToList();
                foreach (var comp in companies)
                {
                    comp.Shares.ChangePrice();
                    comp.Shares.CalcDiff();
                }
                db.SaveChanges();
            }
        }

        private static void GenerateWorld()
        {
            Company company1 = new Company("GazzzProm", 100000, 5);
            company1.Info = "Добывающая корпорация, владеющая значительной частью шахт по всей Галактике.";

            Company company2 = new Company("MicroSold", 30000, 50);
            company2.Info = "Технологический гигант, занимающий лидирующие позиции в чипостроении.";

            Company company3 = new Company("SpremBank", 55000, 25);
            company3.Info = "Крупнейший финансовый конгломерат Галактики с многовековой историей. Открыт 40 лет назад.";

            Company company4 = new Company("BallMart", 74000, 13);
            company4.Info = "Крупнейшая сеть розничной и оптовой торговли.";

            Company company5 = new Company("Azon", 5000, 140);
            company5.Info = "Корпорация-лидер в сфере электронной коммерции.";

            using (ApplicationContext db = new ApplicationContext())
            {
                if (db.Companies.FirstOrDefault(c => c.Name == company1.Name) == null &&
                    db.Companies.FirstOrDefault(c => c.Name == company2.Name) == null &&
                    db.Companies.FirstOrDefault(c => c.Name == company3.Name) == null &&
                    db.Companies.FirstOrDefault(c => c.Name == company4.Name) == null &&
                    db.Companies.FirstOrDefault(c => c.Name == company5.Name) == null)
                {
                    db.Companies.AddRange(
                        company1,
                        company2,
                        company3,
                        company4,
                        company5);
                    db.SaveChanges();
                }
            }
        }

        private static void FillCompaniesList()
        {
            companiesList = new List<string>();
            using (ApplicationContext db = new ApplicationContext())
            {
                foreach (var comp in db.Companies)
                {
                    companiesList.Add(comp.Name);
                }
            }
        }

        private static string BlockForm(BlockOfShares block)
        {
            string result = $"{block.Company.Name} . . . . . . . . . . {block.CurrentAmount:f2}$ ({block.AmountDiffPercent:f2}%) \n" +
                            $"{block.Quantity} шт.     ({block.Company.Shares.PriceOne:f2}$/шт.)\n";// +
                            //$"-----\n";
            return result;
        }

        private static string CompanyInfoForm(Company company, BlockOfShares block)
        {
            string result;// = string.Empty;
            int myQuantity = 0;
            if (block != null)
            {
                myQuantity = block.Quantity;
            }

            result = $"Цена за акцию: {company.Shares.PriceOne:f2}$\n" +
                  $"Доступное кол-во акций: {company.Shares.Quantity} шт.\n" +
                  $"В Моём портфеле: {myQuantity} шт.";
            return result;
        }


        // пересоздать игрока?
        private static void StartCommand(Message msg)
        {
            AboutGameCommand(msg);

            using (ApplicationContext db = new ApplicationContext())
            {
                Player findPlayer = db.Players.FirstOrDefault(p => p.Id == msg.Chat.Id);
                if (findPlayer == null)
                {
                    Player newPlayer = new Player(msg.Chat.Id);
                    db.Players.Add(newPlayer);
                    db.SaveChanges();
                }
            }
        }

        private static async void CheckBalanceCommand(Message msg)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Player findPlayer = db.Players
                    .Include(p => p.Account)
                    .FirstOrDefault(p => p.Id == msg.Chat.Id);
                if (findPlayer != null)
                {
                    await client.SendTextMessageAsync(
                        msg.Chat.Id,
                        $"Мой баланс: {findPlayer.Account.Capital:f2}$",
                        replyMarkup: menuButtons);
                }
            }
        }

        private static async void AboutGameCommand(Message msg)
        {
            string infoStr = string.Empty;

            infoStr = "Это игра про торговлю акциями.\n\n" +
                "Управление происходит через inline-клавиатуру(чтобы использовать её, " +
                "нажмите на значок справа от поля ввода).\n\n" +
                "На бирже размещены акции компаний, которые можно покупать/продавать.\n\n" +
                "Приобретённые акции находятся в Моём портфеле. " +
                "Также здесь можно увидеть динамику портфеля(рост/убыток).\n\n" +
                "Каждые 2 минуты реального времени заканчивается ход, " +
                "после чего происходит рандомное изменение цены акций. " +
                "Это влияет на стоимость Вашего портфеля.\n\n" +
                "Увеличивайте свой баланс: " +
                "докупайте, продавайте, фиксируйте прибыль и не кладите все яйца в одну корзину!";

            await client.SendTextMessageAsync(
                msg.Chat.Id,
                infoStr,
                replyMarkup: menuButtons);
        }

        private static async void MainMenuCommand(Message msg)
        {
            chain.Clear();

            await client.SendTextMessageAsync(
                msg.Chat.Id,
                $"Ваш кабинет",
                replyMarkup: menuButtons);
        }

        private static async void SelectCompanyCommand(Message msg)
        {
            chain.CompanyName = msg.Text;

            string str;
            using (ApplicationContext db = new ApplicationContext())
            {
                Company comp = db.Companies
                    .Include(c => c.Shares)
                    .ThenInclude(s => s.Blocks)
                    .FirstOrDefault(c => c.Name == chain.CompanyName);
                if (comp != null)
                {
                    BlockOfShares block = db.BlockOfShares.FirstOrDefault(b => b.Owner.Id == msg.Chat.Id && b.Company.Name == chain.CompanyName);
                    str = CompanyInfoForm(comp, block);
                    await client.SendTextMessageAsync(
                        msg.Chat.Id,
                        str,
                        replyMarkup: actionButtons);
                }
                else
                {
                    await client.SendTextMessageAsync(
                        msg.Chat.Id,
                        $"Вы не выбрали компанию!");

                    MainMenuCommand(msg);
                }
            }
        }

        private static void DeleteEmptyBlocks()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var blocksOfShares = db.BlockOfShares
                    .Where(b => b.Quantity == 0)
                    .ToList();
                foreach (var block in blocksOfShares)
                {
                    db.BlockOfShares.Remove(block);
                }
                db.SaveChanges();
            }
        }

        private static async void SelectActionCommand(Message msg)
        {
            if (chain.CompanyName != string.Empty)
            {
                chain.Action = msg.Text;

                using (ApplicationContext db = new ApplicationContext())
                {
                    string str;
                    Company comp = db.Companies
                        .Include(c => c.Shares)
                        .ThenInclude(s => s.Blocks)
                        .FirstOrDefault(c => c.Name == chain.CompanyName);
                    BlockOfShares block = db.BlockOfShares.FirstOrDefault(b => b.Owner.Id == msg.Chat.Id && b.Company.Name == chain.CompanyName);

                    str = "Сколько хотите?\n\n" +
                        CompanyInfoForm(comp, block);
                    await client.SendTextMessageAsync(
                        msg.Chat.Id,
                        str,
                        //$"Сколько хотите?",
                        replyMarkup: valueButtons);
                }

            }
        }

        private static async void ReturnToActionCommand(Message msg)
        {
            chain.Value = string.Empty;

            await client.SendTextMessageAsync(
                msg.Chat.Id,
                $"Чего хотите?",
                replyMarkup: actionButtons);
        }

        private static async void AboutCompanyCommand(Message msg)
        {
            if (chain.CompanyName != string.Empty)
            {
                string companyInfo;
                using (ApplicationContext db = new ApplicationContext())
                {
                    companyInfo = db.Companies
                        .FirstOrDefault(c => c.Name == chain.CompanyName).Info;
                }

                await client.SendTextMessageAsync(
                    msg.Chat.Id,
                    $"{companyInfo}",
                    replyMarkup: actionButtons);
            }
        }

        private static async void StockCommand(Message msg)
        {
            IReplyMarkup stockButtons = new ReplyKeyboardMarkup();

            ReplyKeyboardMarkup rkm = new ReplyKeyboardMarkup();
            var cols = new List<KeyboardButton>();
            var rows = new List<KeyboardButton[]>();

            cols.Add(new KeyboardButton("Обновить биржу"));
            rows.Add(cols.ToArray());
            cols = new List<KeyboardButton>();
            foreach (var name in companiesList)
            {
                cols.Add(new KeyboardButton(name));
                rows.Add(cols.ToArray());
                cols = new List<KeyboardButton>();
            }
            cols.Add(new KeyboardButton("В главное меню"));
            rows.Add(cols.ToArray());
            cols = new List<KeyboardButton>();

            rkm.Keyboard = rows.ToArray();
            stockButtons = rkm;

            string str = string.Empty;
            using (ApplicationContext db = new ApplicationContext())
            {
                double stockIndex = 0;
                var companies = db.Companies
                    .Include(c => c.Shares)
                    .ThenInclude(s => s.Blocks)
                    .ToList();
                foreach (var comp in companies)
                {
                    stockIndex += comp.Shares.PriceDiffPercent;
                }
                str += $"Индекс Биржи: {stockIndex:f2}%\n\n";
                foreach (var comp in companies)
                {
                    str += $"{comp.Name}:   {comp.Shares.PriceOne:f2}$ ({comp.Shares.PriceDiffPercent:f2}%)\n";
                }
            }

            await client.SendTextMessageAsync(
                msg.Chat.Id,
                str,
                replyMarkup: stockButtons);
        }

        private static async void MyPortfolioCommand(Message msg)
        {
            List<string> portfolioList = new List<string>();
            List<string> portfolioInfoList = new List<string>();
            using (ApplicationContext db = new ApplicationContext())
            {
                var blocksOfShares = db.BlockOfShares
                    .Where(b => b.Owner.Id == msg.Chat.Id)
                    .Include(b => b.Company)
                    .ThenInclude(c => c.Shares)
                    .ToList();
                foreach (var block in blocksOfShares)
                {
                    if (block.Company != null)
                    {
                        portfolioList.Add(block.Company.Name);
                        portfolioInfoList.Add(BlockForm(block));
                    }
                }
            }

            IReplyMarkup portfolioButtons = new ReplyKeyboardMarkup();

            ReplyKeyboardMarkup rkm = new ReplyKeyboardMarkup();
            var cols = new List<KeyboardButton>();
            var rows = new List<KeyboardButton[]>();

            cols.Add(new KeyboardButton("Обновить портфель"));
            rows.Add(cols.ToArray());
            cols = new List<KeyboardButton>();
            foreach (var name in portfolioList)
            {
                cols.Add(new KeyboardButton(name));
                rows.Add(cols.ToArray());
                cols = new List<KeyboardButton>();
            }
            cols.Add(new KeyboardButton("В главное меню"));
            rows.Add(cols.ToArray());
            cols = new List<KeyboardButton>();

            rkm.Keyboard = rows.ToArray();
            portfolioButtons = rkm;

            chain.Clear();

            if (portfolioInfoList.Any())
            {
                string str = string.Empty;

                str += FullPortfolioInfo(msg.Chat.Id); 
                foreach (var _str in portfolioInfoList)
                {
                    str += _str;
                }
                await client.SendTextMessageAsync(
                    msg.Chat.Id,
                    str,
                    replyMarkup: portfolioButtons);
            }
            else
            {
                await client.SendTextMessageAsync(
                    msg.Chat.Id,
                    "Портфель пуст :(",
                    replyMarkup: portfolioButtons);
            }
        }

        private static async void SelectValueCommand(Message msg)
        {
            chain.Value = msg.Text;

            if (chain.Action == "Купить")
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    Player player = db.Players
                        .Include(p => p.Account)
                        .Include(p => p.Blocks)
                        .FirstOrDefault(p => p.Id == msg.Chat.Id);
                    Company comp = db.Companies
                        .Include(c => c.Shares)
                        .ThenInclude(s => s.Blocks)
                        .FirstOrDefault(c => c.Name == chain.CompanyName);

                    if (comp != null)
                    {
                        bool isSuccess;
                        isSuccess = player.BuyShares(comp, Convert.ToInt32(chain.Value));
                        db.SaveChanges();

                        string str;
                        BlockOfShares block = db.BlockOfShares.FirstOrDefault(b => b.Owner.Id == msg.Chat.Id && b.Company.Name == chain.CompanyName);

                        if (isSuccess)
                        {
                            str = $"Вы купили {chain.Value} акций {chain.CompanyName}\n\n";

                            chain.Value = string.Empty;
                        }
                        else
                        {
                            str = "Что-то пошло не так...\n\n";
                        }

                        str += CompanyInfoForm(comp, block);
                        await client.SendTextMessageAsync(
                            msg.Chat.Id,
                            str,
                            replyMarkup: valueButtons);
                    }
                    else
                    {
                        await client.SendTextMessageAsync(
                            msg.Chat.Id,
                            $"Вы не выбрали компанию!");

                        MainMenuCommand(msg);
                    }
                }
            }
            else if (chain.Action == "Продать")
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    Player player = db.Players
                        .Include(p => p.Account)
                        .Include(p => p.Blocks)
                        .FirstOrDefault(p => p.Id == msg.Chat.Id);
                    Company comp = db.Companies
                        .Include(c => c.Shares)
                        .ThenInclude(s => s.Blocks)
                        .FirstOrDefault(c => c.Name == chain.CompanyName);

                    if (comp != null)
                    {
                        // return bool?
                        bool isSuccess;
                        isSuccess = player.SellShares(comp, Convert.ToInt32(chain.Value));
                        db.SaveChanges();

                        string str;
                        BlockOfShares block = db.BlockOfShares.FirstOrDefault(b => b.Owner.Id == msg.Chat.Id && b.Company.Name == chain.CompanyName);

                        if (isSuccess)
                        {
                            str = $"Вы продали {chain.Value} акций {chain.CompanyName}\n\n";

                            chain.Value = string.Empty;

                            DeleteEmptyBlocks();
                        }
                        else
                        {
                            str = "Что-то пошло не так...\n\n";
                        }

                        str += CompanyInfoForm(comp, block);
                        await client.SendTextMessageAsync(
                            msg.Chat.Id,
                            str,
                            replyMarkup: valueButtons);
                    }
                    else
                    {
                        await client.SendTextMessageAsync(
                            msg.Chat.Id,
                            $"Вы не выбрали компанию!");

                        MainMenuCommand(msg);
                    }
                }
            }
            else
            {
                await client.SendTextMessageAsync(
                    msg.Chat.Id,
                    $"Вы не выбрали действие!");

                SelectCompanyCommand(msg);
            }
        }

        private static string FullPortfolioInfo(long playerId)
        {
            string result = string.Empty;
            double portfolioAmount = 0;
            double portfolioDiffPercent = 0;

            using (ApplicationContext db = new ApplicationContext())
            {
                Player player = db.Players
                    .Include(p => p.Account)
                    .Include(p => p.Blocks)
                    .FirstOrDefault(p => p.Id == playerId);
                foreach (var block in player.Blocks)
                {
                    portfolioAmount += block.CurrentAmount;
                    portfolioDiffPercent += block.AmountDiffPercent;
                }
            }
            result = $"Индекс портфеля: {portfolioAmount:f2}$ ({portfolioDiffPercent:f2}%)\n\n";

            return result;
        }
    }
}
