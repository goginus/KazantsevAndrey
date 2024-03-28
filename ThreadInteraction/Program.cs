using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpaceBattleCommandExecutor
{
    public interface ICommand
    {
        void Execute();
    }

    public class LaserFireCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("[Оружие#1] [Этап#1] Идёт зарядка лазера");
            Thread.Sleep(500);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#1] [Этап#2] Идёт наведение лазерного луча");
            Thread.Sleep(1000);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#1] [Этап#3] Запуск луча лазера");
            Thread.Sleep(1500);
            if (CommandExecutor.GlobalStopRequested) return;
        }
        
    }
    public class TorpedFireCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("[Оружие#2] [Этап#1] Идёт зарядка торпед");
            Thread.Sleep(1000);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#2] [Этап#2] Идёт наведение торпед");
            Thread.Sleep(1500);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#3] [Этап#3] Запуск торпед");
            Thread.Sleep(2000);
            if (CommandExecutor.GlobalStopRequested) return;
        }
    }
    public class PlasmaFireCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("[Оружие#3] [Этап#1] Идёт зарядка инжекторов плазмы");
            Thread.Sleep(1000);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#3] [Этап#2] Идёт наведение плазменого потока");
            Thread.Sleep(1500);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#3] [Этап#3] Запуск плазмы");
            Thread.Sleep(1750);
            if (CommandExecutor.GlobalStopRequested) return;
        }
    }
    public class MagnetronFireCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("[Оружие#4] [Этап#1] Идёт охлаждение магнитного ствола");
            Thread.Sleep(6000);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#4] [Этап#2] Идёт наведение магнитного потока");
            Thread.Sleep(12000);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#4] [Этап#3] Запуск магнитной волны");
            Thread.Sleep(24000);
            if (CommandExecutor.GlobalStopRequested) return;
        }
    }
    public class BioBlasterFireCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("[Оружие#5] [Этап#1] Идёт наполнение ёмкостей био топлевом");
            Thread.Sleep(750);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#5] [Этап#2] Идёт лазерная новодка катушек бластера");
            Thread.Sleep(1500);
            if (CommandExecutor.GlobalStopRequested) return;
            Console.WriteLine("[Оружие#5] [Этап#3] Запуск биологического бластера");
            Thread.Sleep(1600);
            if (CommandExecutor.GlobalStopRequested) return;
        }
    }
    
    // Реализуйте остальные команды по аналогии

    public class CommandExecutor
    {
        private ConcurrentQueue<ICommand> commandQueue = new ConcurrentQueue<ICommand>();
        public List<Thread> threads = new List<Thread>();
        private ManualResetEventSlim stopSignal = new ManualResetEventSlim(false);
        private ManualResetEventSlim GlobalPause = new ManualResetEventSlim(true);
        private ManualResetEventSlim commandsCompleted = new ManualResetEventSlim(false);

        private readonly object _lock = new object();
        public static bool GlobalStopRequested = false;

        public void WaitForCommandsCompletion()
        {
            Console.WriteLine("Ожидание завершения всех команд...");
            commandsCompleted.Wait();
            Console.WriteLine("Все команды завершены");
        }

        private void OnAllCommandsCompleted()
        {
            commandsCompleted.Set();
        }

        public CommandExecutor(int numberOfThreads)
        {
            for (int i = 0; i < numberOfThreads; i++)
            {
                var thread = new Thread(() =>
                    {
                        while (!stopSignal.IsSet)
                        {
                            GlobalPause.Wait();
                            if (commandQueue.TryDequeue(out var command))
                            {
                                try
                                {
                                    command.Execute();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Ошибка выполнения команды: {ex.Message}");
                                }
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }
                    })
                    { IsBackground = true };
                threads.Add(thread);
            }
        }

        public void Start()
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        public void AddCommand(ICommand command)
        {
            lock (_lock)
            {
                if (!stopSignal.IsSet)
                {
                    Console.WriteLine("Добавление команды");
                    commandQueue.Enqueue(command);
                }
            }
        }

        public void Stop(bool completeCurrentCommandsBeforeStopping)
        {
            lock (_lock)
            {
                GlobalStopRequested = true;
                if (completeCurrentCommandsBeforeStopping)
                {
                    Console.WriteLine("Начинается мягкая остановка: ожидаем завершения текущих команд...");
                    while (!commandQueue.IsEmpty)
                    {
                        Thread.Sleep(100);
                    }

                    Console.WriteLine("Мягкая остановка выполнена: все команды завершены.");
                    OnAllCommandsCompleted(); // Сигнализируем о завершении всех команд
                }
                else
                {
                    Console.WriteLine("Начинается жёсткая остановка: немедленное прекращение выполнения команд...");
                    GlobalPause.Set(); // Позволяет прервать ожидание потоками.
                }

                stopSignal.Set();
            }

            foreach (var thread in threads)
            {
                thread.Join(); // Дожидаемся завершения каждого потока
            }

            Console.WriteLine(completeCurrentCommandsBeforeStopping
                ? "Мягкая остановка потоков выполнена."
                : "Жёсткая остановка потоков выполнена.");

            stopSignal.Reset();
            GlobalPause.Reset();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var executor = new CommandExecutor(2);

            executor.AddCommand(new LaserFireCommand());
            executor.AddCommand(new MagnetronFireCommand());
            executor.AddCommand(new TorpedFireCommand());
            executor.AddCommand(new PlasmaFireCommand());
            executor.AddCommand(new BioBlasterFireCommand());
            executor.Start();
            Thread.Sleep(2000);
            executor.Stop(true);
            Thread.Sleep(1000);
            
            

        }
    }
}