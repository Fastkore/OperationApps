using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace VoiceChatApp;

public class VoiceChatService : IDisposable
{
    private readonly UdpClient _udp;
    private readonly IPEndPoint _remote;
    private readonly WaveInEvent _waveIn;
    private readonly WaveOutEvent _waveOut;
    private readonly BufferedWaveProvider _buffer;
    private CancellationTokenSource? _cts;

    public VoiceChatService(string remoteIp, int remotePort, int localPort)
    {
        _remote = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
        _udp = new UdpClient(localPort);

        _waveIn = new WaveInEvent();
        _waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
        _waveIn.DataAvailable += OnDataAvailable;

        _buffer = new BufferedWaveProvider(_waveIn.WaveFormat);
        _waveOut = new WaveOutEvent();
        _waveOut.Init(_buffer);
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        _udp.Send(e.Buffer, e.BytesRecorded, _remote);
    }

    public void Start()
    {
        if (_cts != null) return;
        _cts = new CancellationTokenSource();
        _waveIn.StartRecording();
        Task.Run(ReceiveLoop);
    }

    public void Stop()
    {
        if (_cts == null) return;
        _cts.Cancel();
        _waveIn.StopRecording();
        _waveOut.Stop();
        _cts = null;
    }

    private async Task ReceiveLoop()
    {
        var token = _cts!.Token;
        while (!token.IsCancellationRequested)
        {
            var result = await _udp.ReceiveAsync(token);
            _buffer.AddSamples(result.Buffer, 0, result.Buffer.Length);
            if (_waveOut.PlaybackState != PlaybackState.Playing)
                _waveOut.Play();
        }
    }

    public void Dispose()
    {
        Stop();
        _waveIn.Dispose();
        _waveOut.Dispose();
        _udp.Dispose();
    }
}
