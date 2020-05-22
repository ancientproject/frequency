using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ancient.runtime;
using UnitsNet;
using UnitsNet.Units;


[Guid("DC6F3B04-9ADC-4672-8E3B-A4ABDC21B673")]
public class HwndFrequencyDevice : Device
{
    private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
    public HwndFrequencyDevice() : base(0x999, "<frequency-hwnd>") { }

    public override void warmUp()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        this.state = (this as IBusGate).getBus()?.State;

        if (this.state is null)
            ThrowMemoryRead(0x0, startAddress, new Exception("Cannot read bus connecter."));

        Task.Run(Render, tokenSource.Token);
    }

    private dynamic state { get; set; }

    public override void shutdown() => tokenSource.Cancel();

    private async Task Render()
    {
        while (!tokenSource.IsCancellationRequested)
        {
            var freq = new Frequency(state.GetHertz(), FrequencyUnit.Hertz);
            var type = FrequencyUnit.Hertz;

            if (freq.Gigahertz > 0.1)
                type = FrequencyUnit.Gigahertz;
            else if (freq.Megahertz > 0.1)
                type = FrequencyUnit.Megahertz;
            else if (freq.Kilohertz > 0.1)
                type = FrequencyUnit.Kilohertz;

            Console.Title = $"[vm_host] {freq.ToUnit(type).ToString(CultureInfo.InvariantCulture)}";
            await Task.Delay(200);
        }
    }
}