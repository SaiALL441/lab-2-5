using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lab5;

public class Form
{
    public Form(string name)
    {
        Name = name;
    }
    public string Name;
    private Semaphore _carSemaphore = new Semaphore(3, 3);
    private BlockingCollection<Car> _carsQueue = new BlockingCollection<Car>();
    private Mutex _finishMutex;

    public void StartRoad(Mutex mutex)
    {
        _finishMutex = mutex;

        var passingCarsThread = new Thread(PassCars);
        passingCarsThread.Start();

        var arrivingCarsThread = new Thread(ArrivingCars);
        arrivingCarsThread.Start();
    }
    private void PassCars()
    {
        _finishMutex.WaitOne();

        foreach (var car in _carsQueue.GetConsumingEnumerable())
        {
            _carSemaphore.WaitOne();
            var carDriving = new Thread(car.Drive(() =>
            {
                _carSemaphore.Release();
            }));
            carDriving.Start();
        }

        _finishMutex.ReleaseMutex();
    }

    private void ArrivingCars()
    {
        for (int i = 0; i < Random.Shared.Next(2, 8); i++)
        {
            _carsQueue.Add(new Car($"TF-{Name}-{i.ToString()}"));
            Thread.Sleep(100 * Random.Shared.Next(1, 4));
        }
        _carsQueue.CompleteAdding();
    }
}
public class Car
{
    public string Name;
    public Car(string name)
    {
        Name = name;
    }
    public ThreadStart Drive(Action onDone)
    {
        return () =>
        {
            Console.WriteLine($"{Name} Go");
            Thread.Sleep(1000 * Random.Shared.Next(1, 2));
            Console.WriteLine($"{Name} Stay");

            onDone();
        };
    }
}
public class CrossRoad
{
    public void Road()
    {
        var mutex = new Mutex();
        var trafficLights = new[] {new Form("1"), new Form("2"), new Form("3")};
        foreach (var trafficLight in trafficLights)
        {
            trafficLight.StartRoad(mutex);
        }
    }
}
class Program
{
    public static void Main()
    {
        var crossRoad = new CrossRoad();
        crossRoad.Road();
        Console.ReadKey();
    }
}