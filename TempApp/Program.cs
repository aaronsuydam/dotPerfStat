using System;
using System.Collections.Generic;
using System.Threading;

// 1) Your data type
public class StockPrice
{
    public string Symbol { get; }
    public decimal Price { get; }
    public DateTime Timestamp { get; }

    public StockPrice(string symbol, decimal price)
    {
        Symbol    = symbol;
        Price     = price;
        Timestamp = DateTime.Now;
    }
}

// 2) Observable implementation
public class StockTicker : IObservable<StockPrice>, IDisposable
{
    private readonly List<IObserver<StockPrice>> _observers = new();
    private readonly Timer _timer;
    private readonly Random _rng = new();

    public StockTicker()
    {
        // fire every 1 second
        _timer = new Timer(_ => PublishPrice(), null, 0, 1000);
    }

    private void PublishPrice()
    {
        // generate a fake price
        var price = 100m + (decimal)_rng.NextDouble() * 20m;
        var data  = new StockPrice("ACME", Math.Round(price, 2));

        foreach (var obs in _observers)
            obs.OnNext(data);
    }

    public IDisposable Subscribe(IObserver<StockPrice> observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);

        // return a handle that lets them unsubscribe
        return new Unsubscriber(_observers, observer);
    }

    public void Dispose()
    {
        _timer.Dispose();
        foreach (var obs in _observers)
            obs.OnCompleted();
        _observers.Clear();
    }

    // helper IDisposable to remove observer
    private class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<StockPrice>> _obsList;
        private readonly IObserver<StockPrice>      _observer;

        public Unsubscriber(List<IObserver<StockPrice>> list, IObserver<StockPrice> observer)
        {
            _obsList  = list;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _obsList.Contains(_observer))
                _obsList.Remove(_observer);
        }
    }
}

// 3) Observer implementation
public class StockObserver : IObserver<StockPrice>
{
    public void OnNext(StockPrice value)
    {
        Console.WriteLine($"[{value.Timestamp:HH:mm:ss}] {value.Symbol} = ${value.Price}");
    }

    public void OnError(Exception error)
    {
        Console.Error.WriteLine($"Ticker error: {error.Message}");
    }

    public void OnCompleted()
    {
        Console.WriteLine("Ticker completed.");
    }
}

// 4) Putting it all together
public class Program
{
    public static void Main()
    {
        using var ticker   = new StockTicker();
        var       observer = new StockObserver();

        // Subscribe: returns IDisposable
        IDisposable subscription = ticker.Subscribe(observer);

        Console.WriteLine("Receiving stock prices for 5 seconds...");
        Thread.Sleep(5000);

        // Unsubscribe when done
        subscription.Dispose();
        Console.WriteLine("Unsubscribed. Press any key to exit.");
        Console.ReadKey();
    }
}