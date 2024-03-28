using Xunit;
using System.Linq;
using System.Threading;
using SpaceBattleCommandExecutor;


public class UnitTest1
{
    [Fact]
    public void StartCommandExecutor_NoThreadsStarted()
    {
        // Arrange
        var executor = new CommandExecutor(2);

        // Act
        // Не вызываем executor.Start()

        // Assert
        foreach (var thread in executor.threads)
        {
            Assert.False(thread.IsAlive);
        }
    }
    [Fact]
    public void StartCommandExecutor_StartThreads_ThreadsAreRunning()
    {
        // Arrange
        var executor = new CommandExecutor(2);

        // Act
        executor.Start();
        Thread.Sleep(100);

        // Assert
        foreach (var thread in executor.threads)
        {
            Assert.True(thread.IsAlive);
        }

        // Clean up
        executor.Stop(false);
    }

    [Fact]
    public void StopCommandExecutor_HardStop_AllThreadsAreStoppedImmediately()
    {
        // Arrange
        var executor = new CommandExecutor(2);
        executor.Start();

        // Act
        executor.Stop(false);

        // Assert
        foreach (var thread in executor.threads)
        {
            Assert.False(thread.IsAlive);
        }
    }

    [Fact]
    public void StopCommandExecutor_SoftStop_AllThreadsStopAfterCommandsCompleted()
    {
        // Arrange
        var executor = new CommandExecutor(4);
        executor.AddCommand(new LaserFireCommand());
        executor.AddCommand(new MagnetronFireCommand());
        executor.AddCommand(new TorpedFireCommand());
        executor.AddCommand(new PlasmaFireCommand());
        executor.Start();

        // Act
        executor.Stop(true);

        // Assert
        foreach (var thread in executor.threads)
        {
            Assert.False(thread.IsAlive);
        }
    }
    
}